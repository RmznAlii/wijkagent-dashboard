using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WijkAgent.Core.Data;
using WijkAgent.Core.Models;
using System.Text.RegularExpressions;

namespace WijkAgent.Core.Services
{
    /// <summary>
    /// Polls an external JSON API at a regular interval, maps received items to <see cref="Crime"/>
    /// and inserts new items into the database while preventing duplicates using the external uid.
    /// 
    /// This service is lightweight and starts polling when instantiated by DI.
    /// Configure the endpoint and interval in MauiProgram when registering the service.
    /// </summary>
    public sealed class ApiPollService : IDisposable
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly WijkAgentDbContext _db;
        private readonly ICrimeService _crimeService;
        private readonly CrimeNotifier _notifier;
        private readonly ILogger<ApiPollService> _logger;
        private readonly TimeSpan _interval;
        private readonly string _apiUrl;
        private readonly Timer _timer;
        private int _isRunning;

        public ApiPollService(
            IHttpClientFactory httpFactory,
            WijkAgentDbContext db,
            ICrimeService crimeService,
            CrimeNotifier notifier,
            ILogger<ApiPollService> logger,
            string apiUrl,
            TimeSpan pollInterval)
        {
            _httpFactory = httpFactory;
            _db = db;
            _crimeService = crimeService;
            _notifier = notifier;
            _logger = logger;
            _apiUrl = apiUrl ?? throw new ArgumentNullException(nameof(apiUrl));
            _interval = pollInterval == default ? TimeSpan.FromSeconds(10) : pollInterval;

            // Start timer immediately, interval thereafter.
            _timer = new Timer(OnTimerTick, null, TimeSpan.Zero, _interval);
        }

        private void OnTimerTick(object? state)
        {
            // prevent overlapping runs
            if (Interlocked.Exchange(ref _isRunning, 1) == 1) return;

            _ = Task.Run(async () =>
            {
                try
                {
                    await PollOnceAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ApiPollService polling failed.");
                }
                finally
                {
                    Interlocked.Exchange(ref _isRunning, 0);
                }
            });
        }

        private async Task PollOnceAsync()
        {
            using var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            HttpResponseMessage resp;
            try
            {
                resp = await client.GetAsync(_apiUrl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to call API: {Url}", _apiUrl);
                return;
            }

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("API returned non-success {Status} for {Url}", resp.StatusCode, _apiUrl);
                return;
            }

            var json = await resp.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json)) return;

            using var doc = JsonDocument.Parse(json);

            // handle either array of items or a single object
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    await ProcessApiItemAsync(item);
                }
            }
            else if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                await ProcessApiItemAsync(doc.RootElement);
            }
            else
            {
                _logger.LogDebug("API returned unexpected JSON root kind: {Kind}", doc.RootElement.ValueKind);
            }
        }

        private async Task ProcessApiItemAsync(JsonElement item)
        {
            // read uid
            if (!item.TryGetProperty("uid", out var uidProp)) return;
            var uid = uidProp.GetString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(uid)) return;

            // read dienst and only proceed when dienst == "Politie"
            var dienst = item.TryGetProperty("dienst", out var d) ? d.GetString() ?? string.Empty : string.Empty;
            if (!string.Equals(dienst, "Politie", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Skipping API item uid={Uid} because dienst != Politie (dienst={Dienst})", uid, dienst);
                return;
            }

            // map fields
            var description = item.TryGetProperty("melding", out var m) ? m.GetString() ?? "" : "";
            var city = item.TryGetProperty("plaats", out var p) ? p.GetString() ?? "" : "";

            // Derive type from first word of description (remove digits first)
            string type;
            var descTrim = (description ?? string.Empty).Trim();

            if (!string.IsNullOrEmpty(descTrim))
            {
                // remove all digits from the description before selecting first word
                var descNoDigits = Regex.Replace(descTrim, @"\d+", "").Trim();

                if (!string.IsNullOrEmpty(descNoDigits))
                {
                    // take first token and strip leading/trailing non-alphanumeric characters
                    var firstToken = descNoDigits.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    firstToken = Regex.Replace(firstToken, @"^[^\p{L}\p{N}]+|[^\p{L}\p{N}]+$", ""); // remove leading/trailing non-alnum
                    type = string.IsNullOrEmpty(firstToken) ? dienst : firstToken;
                }
                else
                {
                    // all characters were digits/punctuation after removal -> fallback to dienst
                    type = dienst;
                }
            }
            else
            {
                type = dienst;
            }

            // NORMALIZE description for duplicate checks
            var descNorm = descTrim;
            var descLower = string.IsNullOrEmpty(descNorm) ? string.Empty : descNorm.ToLowerInvariant();

            // FIRST: dedupe by UID (fast, exact)
            var existsByUid = await _db.Crimes.AnyAsync(c => c.Uid == uid);
            if (existsByUid)
            {
                _logger.LogDebug("Skipping existing item uid={Uid}", uid);
                return;
            }

            // SECOND: dedupe by description (case-insensitive, trimmed)
            if (!string.IsNullOrEmpty(descLower))
            {
                // use CLR Trim/ToLower on the column — EF Core translates these to SQL TRIM/LOWER
                var existsWithSameDescription = await _db.Crimes
                    .AnyAsync(c => c.Description != null && c.Description.Trim().ToLower() == descLower);
                if (existsWithSameDescription)
                {
                    _logger.LogDebug("Skipping API item uid={Uid} because a crime with same description already exists", uid);
                    return;
                }
            }

            var lat = 0.0;
            var lng = 0.0;

            if (item.TryGetProperty("latlong", out var ll) && !string.IsNullOrWhiteSpace(ll.GetString()))
            {
                var s = ll.GetString()!;
                var parts = s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length >= 2 &&
                    double.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var la) &&
                    double.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lo))
                {
                    lat = la;
                    lng = lo;
                }
            }
            else if (item.TryGetProperty("plaats_latlon", out var pl) && !string.IsNullOrWhiteSpace(pl.GetString()))
            {
                var s = pl.GetString()!;
                var parts = s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length >= 2 &&
                    double.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var la) &&
                    double.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lo))
                {
                    lat = la;
                    lng = lo;
                }
            }

            // --- Skip items that have both lat and lng equal to 0.0 ---
            if (lat == 0.0 && lng == 0.0)
            {
                _logger.LogDebug("Skipping API item uid={Uid} because coordinates are 0,0", uid);
                return;
            }

            // parse date/time - API sample had "datum" "tijd" (datum dd-MM-yyyy, tijd HH:mm:ss)
            DateTime incident = DateTime.Now;
            if (item.TryGetProperty("datum", out var datumProp) && item.TryGetProperty("tijd", out var tijdProp))
            {
                var dat = datumProp.GetString() ?? "";
                var tim = tijdProp.GetString() ?? "";
                if (DateTime.TryParseExact($"{dat} {tim}", new[] { "dd-MM-yyyy HH:mm:ss", "dd-MM-yyyy H:mm:ss", "dd-MM-yyyy HH:mm", "dd-MM-yyyy H:mm" },
                    System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out var dt))
                {
                    incident = dt;
                }
            }
            else if (item.TryGetProperty("timestamp", out var tsProp) && tsProp.ValueKind == JsonValueKind.String)
            {
                var tsStr = tsProp.GetString() ?? "";
                if (long.TryParse(tsStr, out var unix))
                {
                    // sample timestamp looks like seconds since epoch
                    incident = DateTimeOffset.FromUnixTimeSeconds(unix).LocalDateTime;
                }
            }

            var crime = new Crime
            {
                Uid = uid,
                Type = type,
                Description = description,
                City = city,
                Lat = lat,
                Lng = lng,
                IncidentDateTime = incident,
                CreatedAt = DateTime.Now
            };

            // Optional: extract address parts if API provides
            if (item.TryGetProperty("locatie", out var loc)) crime.Street = loc.GetString() ?? "";
            if (item.TryGetProperty("postcode", out var pc)) crime.Postcode = pc.GetString() ?? "";
            if (item.TryGetProperty("regio", out var prov)) crime.Province = prov.GetString() ?? "";

            try
            {
                // Save via service so createdAt logic stays consistent
                var saved = await _crimeService.AddAsync(crime);

                // publish notification so UI components can update live
                try
                {
                    _notifier.Publish(saved);
                }
                catch (Exception exPub)
                {
                    _logger.LogWarning(exPub, "Notifier publish failed for uid={Uid}", uid);
                }

                _logger.LogInformation("Inserted crime from API uid={Uid} at {Lat},{Lng}", uid, saved.Lat, saved.Lng);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert crime uid={Uid}", uid);
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}