using Microsoft.EntityFrameworkCore;
using WijkAgent.Core.Data;
using WijkAgent.Core.Models;

namespace WijkAgent.Core.Services
{
    /// <summary>
    /// Serviceklasse die CRUD-operaties uitvoert voor delicten (Crime).
    /// 
    /// Verantwoordelijkheden:
    /// - Ophalen van alle delicten of specifieke delicten.
    /// - Toevoegen en updaten van delicten.
    /// - Filteren van delicten op basis van datum, type en locatie.
    /// 
    /// Werkt samen met:
    /// - <see cref="WijkAgentDbContext"/> voor database-operaties.
    /// - UI-componenten (Blazor) die via ICrimeService communiceren.
    /// </summary>
    public class CrimeService : ICrimeService
    {
        private readonly WijkAgentDbContext _db;

        /// <summary>
        /// Constructor voor dependency injection.
        /// </summary>
        /// <param name="db">Databasecontext voor toegang tot de Crimes-tabel.</param>
        public CrimeService(WijkAgentDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Haalt alle delicten op, gesorteerd op aanmaaktijd (nieuwste eerst).
        /// </summary>
        /// <returns>Lijst van <see cref="Crime"/> objecten.</returns>
        public async Task<List<Crime>> GetAllAsync()
        {
            return await _db.Crimes
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Haalt één delict op basis van zijn unieke ID.
        /// </summary>
        /// <param name="id">Het ID van het gewenste delict.</param>
        /// <returns>Het gevonden <see cref="Crime"/> object, of null wanneer niet gevonden.</returns>
        public async Task<Crime?> GetByIdAsync(int id)
        {
            return await _db.Crimes.FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Voegt een nieuw delict toe aan de database.
        /// Zet <see cref="Crime.CreatedAt"/> automatisch wanneer deze nog niet is ingevuld.
        /// </summary>
        /// <param name="crime">Het nieuwe delict dat moet worden opgeslagen.</param>
        /// <returns>Het opgeslagen <see cref="Crime"/> object, inclusief gegenereerd ID.</returns>
        public async Task<Crime> AddAsync(Crime crime)
        {
            if (crime.CreatedAt == default)
            {
                crime.CreatedAt = DateTime.Now;
            }

            _db.Crimes.Add(crime);
            await _db.SaveChangesAsync();

            return crime;
        }

        /// <summary>
        /// Werkt een bestaand delict bij op basis van het ID van het meegegeven model.
        /// Alleen bestaande records worden aangepast.
        /// </summary>
        /// <param name="crime">Het delict met bijgewerkte gegevens.</param>
        /// <returns>
        /// Het bijgewerkte <see cref="Crime"/> object, of null wanneer het delict niet bestaat.
        /// </returns>
        public async Task<Crime?> UpdateAsync(Crime crime)
        {
            var existing = await _db.Crimes.FirstOrDefaultAsync(c => c.Id == crime.Id);
            if (existing is null)
                return null;

            existing.Uid = crime.Uid;
            existing.Type = crime.Type;
            existing.Description = crime.Description;
            existing.Street = crime.Street;
            existing.HouseNumber = crime.HouseNumber;
            existing.Postcode = crime.Postcode;
            existing.City = crime.City;
            existing.Province = crime.Province;
            existing.IncidentDateTime = crime.IncidentDateTime;
            existing.Lat = crime.Lat;
            existing.Lng = crime.Lng;

            await _db.SaveChangesAsync();
            return existing;
        }

        /// <summary>
        /// Nieuw: verwijdert een bestaand delict op basis van ID.
        /// </summary>
        /// <param name="id">Het ID van het delict dat verwijderd moet worden.</param>
        /// <returns>True als verwijderen gelukt is, anders false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _db.Crimes.FirstOrDefaultAsync(c => c.Id == id);
            if (existing is null)
                return false;

            _db.Crimes.Remove(existing);
            await _db.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Haalt delicten op die voldoen aan de opgegeven filtercriteria.
        /// Mogelijke filters:
        /// - Datum vanaf (from)
        /// - Datum tot en met (to)
        /// - Type delict (type)
        /// - Stad (city)
        /// 
        /// Alle filters zijn optioneel.
        /// </summary>
        /// <param name="from">Optioneel: filter vanaf deze datum/tijd.</param>
        /// <param name="to">Optioneel: filter t/m deze datum/tijd.</param>
        /// <param name="type">Optioneel: type delict dat gefilterd moet worden.</param>
        /// <param name="city">Optioneel: stad waarop wordt gefilterd.</param>
        /// <returns>Lijst van gefilterde delicten in aflopende volgorde van aanmaak.</returns>
        public async Task<List<Crime>> GetFilteredAsync(
            DateTime? from,
            DateTime? to,
            string? type,
            string? city)
        {
            var query = _db.Crimes.AsQueryable();

            if (from.HasValue)
                query = query.Where(c => c.IncidentDateTime >= from.Value);

            if (to.HasValue)
                query = query.Where(c => c.IncidentDateTime <= to.Value);

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(c => c.Type == type);

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(c => c.City == city);

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // --- New aggregate methods ---

        /// <summary>
        /// Haalt het totale aantal delicten op.
        /// </summary>
        /// <returns>Totaal aantal <see cref="Crime"/> records.</returns>
        public async Task<int> GetTotalCountAsync()
        {
            return await _db.Crimes.CountAsync();
        }

        /// <summary>
        /// Haalt het aantal delicten op binnen een bepaalde datum- en tijdsperiode.
        /// </summary>
        /// <param name="from">Begin van de periode.</param>
        /// <param name="to">Einde van de periode.</param>
        /// <returns>Aantal delicten binnen de opgegeven periode.</returns>
        public async Task<int> GetCountInRangeAsync(DateTime from, DateTime to)
        {
            return await _db.Crimes
                .Where(c => c.IncidentDateTime >= from && c.IncidentDateTime <= to)
                .CountAsync();
        }

        /// <summary>
        /// Haalt het aantal delicten op voor de huidige maand en de vorige maand.
        /// </summary>
        /// <returns>Een tuple met het aantal delicten voor deze maand en de vorige maand.</returns>
        public async Task<(int CurrentMonth, int PreviousMonth)> GetThisAndPreviousMonthCountsAsync()
        {
            var now = DateTime.Now;
            var firstOfMonth = new DateTime(now.Year, now.Month, 1);
            var prevStart = firstOfMonth.AddMonths(-1);
            var prevEnd = firstOfMonth.AddTicks(-1);

            var current = await _db.Crimes.CountAsync(c => c.IncidentDateTime >= firstOfMonth && c.IncidentDateTime <= now);
            var previous = await _db.Crimes.CountAsync(c => c.IncidentDateTime >= prevStart && c.IncidentDateTime <= prevEnd);

            return (current, previous);
        }

        /// <summary>
        /// Haalt de top N meeste voorkomende type delicten op.
        /// </summary>
        /// <param name="top">Het aantal top type delicten dat opgehaald moet worden.</param>
        /// <returns>Een lijst van tuples met type delict en bijbehorende aantal.</returns>
        public async Task<List<(string Type, int Count)>> GetCountsByTypeAsync(int top = 6)
        {
            var q = await _db.Crimes
                .GroupBy(c => string.IsNullOrWhiteSpace(c.Type) ? "Onbekend" : c.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(top)
                .ToListAsync();

            return q.Select(x => (x.Type, x.Count)).ToList();
        }

        /// <summary>
        /// Haalt de top N steden op met het meeste aantal delicten.
        /// </summary>
        /// <param name="top">Het aantal top steden dat opgehaald moet worden.</param>
        /// <returns>Een lijst van tuples met stad en bijbehorende aantal delicten.</returns>
        public async Task<List<(string City, int Count)>> GetTopCitiesAsync(int top = 5)
        {
            var q = await _db.Crimes
                .GroupBy(c => string.IsNullOrWhiteSpace(c.City) ? "Onbekend" : c.City)
                .Select(g => new { City = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(top)
                .ToListAsync();

            return q.Select(x => (x.City, x.Count)).ToList();
        }

        /// <summary>
        /// Haalt het aantal delicten op verdeeld over vooraf gedefinieerde tijdslots.
        /// Tijdslots:
        /// - 00:00-06:00
        /// - 06:00-12:00
        /// - 12:00-18:00
        /// - 18:00-00:00
        /// </summary>
        /// <returns>Een lijst van tuples met tijdslot label en bijbehorende aantal delicten.</returns>
        public async Task<List<(string Label, int Count)>> GetCountsByTimeSlotAsync()
        {
            // 4 slots: 00:00-06:00, 06:00-12:00, 12:00-18:00, 18:00-00:00
            var slot1 = await _db.Crimes.CountAsync(c => c.IncidentDateTime.Hour >= 0 && c.IncidentDateTime.Hour <= 5);
            var slot2 = await _db.Crimes.CountAsync(c => c.IncidentDateTime.Hour >= 6 && c.IncidentDateTime.Hour <= 11);
            var slot3 = await _db.Crimes.CountAsync(c => c.IncidentDateTime.Hour >= 12 && c.IncidentDateTime.Hour <= 17);
            var slot4 = await _db.Crimes.CountAsync(c => c.IncidentDateTime.Hour >= 18 && c.IncidentDateTime.Hour <= 23);

            return new List<(string, int)>
            {
                ("00:00-06:00", slot1),
                ("06:00-12:00", slot2),
                ("12:00-18:00", slot3),
                ("18:00-00:00", slot4)
            };
        }

        /// <summary>
        /// Haalt het aantal delicten op per dag voor de afgelopen X dagen.
        /// </summary>
        /// <param name="days">Aantal dagen terug vanaf vandaag.</param>
        /// <returns>Een lijst van tuples met datum en bijbehorende aantal delicten.</returns>
        public async Task<List<(DateTime Date, int Count)>> GetCountsPerDayAsync(int days = 7)
        {
            var start = DateTime.Today.AddDays(-(days - 1));
            var grouped = await _db.Crimes
                .Where(c => c.IncidentDateTime >= start)
                .GroupBy(c => new { c.IncidentDateTime.Year, c.IncidentDateTime.Month, c.IncidentDateTime.Day })
                .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Day, Count = g.Count() })
                .ToListAsync();

            var lookup = grouped.ToDictionary(x => new DateTime(x.Year, x.Month, x.Day), x => x.Count);

            var result = new List<(DateTime Date, int Count)>();
            for (int i = 0; i < days; i++)
            {
                var d = start.AddDays(i);
                lookup.TryGetValue(d, out var cnt);
                result.Add((d, cnt));
            }

            return result;
        }

        /// <summary>
        /// Haalt het meest recente delict op (op basis van aanmaaktijd).
        /// </summary>
        /// <returns>Het nieuwste <see cref="Crime"/> object, of null wanneer er geen delicten zijn.</returns>
        public async Task<Crime?> GetNewestAsync()
        {
            return await _db.Crimes.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync();
        }
    }
}
