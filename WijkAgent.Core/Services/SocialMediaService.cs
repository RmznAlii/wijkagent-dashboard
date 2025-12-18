using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text.Json;
using WijkAgent.Core.Models;
using Microsoft.Extensions.Configuration;


namespace WijkAgent.Core.Services
{
    public class SocialMediaService : ISocialMediaService
    {   
        private readonly HttpClient _httpClient;

        public SocialMediaService(HttpClient httpClient, string bearerToken)
        {
            _httpClient = httpClient;

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", bearerToken);
        }
        public async Task<IReadOnlyList<SocialMediaPost>> GetPostsForIncidentAsync(
            DateTime incidentTime,
            IEnumerable<string> keywords,
            CancellationToken cancellationToken = default)
        {
            var keywordQuery = string.Join(" OR ", keywords);

            if (string.IsNullOrWhiteSpace(keywordQuery))
                return Array.Empty<SocialMediaPost>();

            var query = $"({keywordQuery}) -is:retweet lang:nl";

            var startTime = incidentTime.AddMinutes(-30).ToUniversalTime();

            // X API vereist dat end_time duidelijk in het verleden ligt
            var maxAllowedEndTime = DateTime.UtcNow.AddSeconds(-30);

            // Kies de vroegste geldige eindtijd
            var calculatedEndTime = incidentTime.AddMinutes(30).ToUniversalTime();

            var endTime = calculatedEndTime <= maxAllowedEndTime
                ? calculatedEndTime
                : maxAllowedEndTime;

            // Extra veiligheidscheck
            if (startTime >= endTime)
            {
                return Array.Empty<SocialMediaPost>();
            }

            var startTimeIso = startTime.ToString("o");
            var endTimeIso = endTime.ToString("o");

            var url =
                $"https://api.x.com/2/tweets/search/recent" +
                $"?query={Uri.EscapeDataString(query)}" +
                $"&start_time={startTimeIso}" +
                $"&end_time={endTimeIso}" +
                $"&tweet.fields=created_at,author_id" +
                $"&max_results=10";

            using var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new Exception($"X API error: {response.StatusCode} - {error}");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            return MapResponse(json);

        }
        private static IReadOnlyList<SocialMediaPost> MapResponse(string json)
        {
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("data", out var data))
                return Array.Empty<SocialMediaPost>();

            return data.EnumerateArray().Select(tweet => new SocialMediaPost
            {
                Id = tweet.GetProperty("id").GetString()!,
                Platform = "X",
                Username = "unknown",
                Content = tweet.GetProperty("text").GetString()!,
                PostedAt = tweet.GetProperty("created_at").GetDateTime(),
                PostUrl = $"https://x.com/i/web/status/{tweet.GetProperty("id").GetString()}",
            }).ToList();
        }
    }
}

