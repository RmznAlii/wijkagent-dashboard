using WijkAgent.Core.Models;

namespace WijkAgent.Core.Services
{
    /// <summary>
    /// Interface voor de CrimeService.
    /// 
    /// Beschrijft alle functionaliteiten rondom het beheren van delicten.
    /// Wordt gebruikt voor dependency injection zodat de UI-laag niet afhankelijk is
    /// van een concrete implementatie.
    /// 
    /// Verantwoordelijkheden:
    /// - Ophalen van delicten
    /// - Toevoegen van nieuwe delicten
    /// - Bewerken van bestaande delicten
    /// - Filteren op datum, type en locatie
    /// </summary>
    public interface ICrimeService
    {
        /// <summary>
        /// Haalt alle delicten op, gesorteerd op aanmaaktijd.
        /// </summary>
        /// <returns>Lijst van alle delicten.</returns>
        Task<List<Crime>> GetAllAsync();

        /// <summary>
        /// Haalt één specifiek delict op basis van zijn unieke ID.
        /// </summary>
        /// <param name="id">Het ID van het gezochte delict.</param>
        /// <returns>
        /// Het gevonden <see cref="Crime"/> object,
        /// of null wanneer geen delict met dit ID bestaat.
        /// </returns>
        Task<Crime?> GetByIdAsync(int id);

        /// <summary>
        /// Voegt een nieuw delict toe aan de database.
        /// </summary>
        /// <param name="crime">Het delict dat moet worden aangemaakt.</param>
        /// <returns>
        /// Het opgeslagen <see cref="Crime"/> object, inclusief gegenereerd ID.
        /// </returns>
        Task<Crime> AddAsync(Crime crime);

        /// <summary>
        /// Werkt een bestaand delict bij.
        /// </summary>
        /// <param name="crime">Het delict met bijgewerkte velden.</param>
        /// <returns>
        /// Het geüpdatete <see cref="Crime"/> object,
        /// of null wanneer het delict niet bestaat.
        /// </returns>
        Task<Crime?> UpdateAsync(Crime crime);

        /// <summary>
        /// Haalt een lijst delicten op die voldoen aan de opgegeven filtercriteria.
        /// Alle filterparameters zijn optioneel.
        /// </summary>
        /// <param name="from">Optioneel: filter vanaf deze datum/tijd.</param>
        /// <param name="to">Optioneel: filter tot en met deze datum/tijd.</param>
        /// <param name="type">Optioneel: type delict waarop gefilterd wordt.</param>
        /// <param name="city">Optioneel: stad waarop gefilterd wordt.</param>
        /// <returns>Lijst delicten die voldoen aan de filterinstellingen.</returns>
        Task<List<Crime>> GetFilteredAsync(
            DateTime? from,
            DateTime? to,
            string? type,
            string? city);
    }
}
