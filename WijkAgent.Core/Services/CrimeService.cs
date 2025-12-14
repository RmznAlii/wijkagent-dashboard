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
    }
}
