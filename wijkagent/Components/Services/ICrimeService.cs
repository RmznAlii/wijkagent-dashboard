using WijkAgent.Models;

namespace WijkAgent.Services;

/// <summary>
/// Interface voor het beheren van delicten binnen de applicatie.
/// Definieert alle CRUD-operaties en hulpmethoden voor het werken
/// met <see cref="CrimeModel"/> objecten.
/// </summary>
public interface ICrimeService
{
    /// <summary>
    /// Haalt alle delicten op.
    /// </summary>
    /// <returns>Een lijst met <see cref="CrimeModel"/> objecten.</returns>
    Task<List<CrimeModel>> GetAllAsync();

    /// <summary>
    /// Haalt één specifiek delict op op basis van het ID.
    /// </summary>
    /// <param name="id">Het ID van het delict.</param>
    /// <returns>Het gevonden delict, of null als het niet bestaat.</returns>
    Task<CrimeModel?> GetByIdAsync(int id);

    /// <summary>
    /// Voegt een nieuw delict toe aan het systeem.
    /// </summary>
    /// <param name="crime">Het delict dat toegevoegd moet worden.</param>
    Task AddAsync(CrimeModel crime);

    /// <summary>
    /// Werkt een bestaand delict bij.
    /// </summary>
    /// <param name="crime">Het aangepaste delict inclusief bestaand ID.</param>
    Task UpdateAsync(CrimeModel crime);

    /// <summary>
    /// Verwijdert een delict op basis van ID.
    /// </summary>
    /// <param name="id">Het ID van het te verwijderen delict.</param>
    Task DeleteAsync(int id);

    /// <summary>
    /// Verwijdert alle delicten uit het systeem.
    /// </summary>
    Task ClearAsync();
}