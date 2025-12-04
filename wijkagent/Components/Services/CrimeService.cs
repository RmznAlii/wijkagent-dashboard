using WijkAgent.Models;

namespace WijkAgent.Services;

/// <summary>
/// In-memory implementatie van <see cref="ICrimeService"/>.
/// Beheert delicten zonder database en biedt CRUD-functionaliteit.
/// </summary>
public class CrimeService : ICrimeService
{
    /// <summary>
    /// Interne lijst waarin alle delicten worden opgeslagen.
    /// </summary>
    private readonly List<CrimeModel> _crimes = new();

    /// <summary>
    /// Voorkomt dat seeding meerdere keren wordt uitgevoerd.
    /// </summary>
    private bool _isSeeded;

    /// <summary>
    /// Haalt alle delicten op uit de in-memory lijst.
    /// </summary>
    public Task<List<CrimeModel>> GetAllAsync()
    {
        EnsureSeeded();
        return Task.FromResult(_crimes.ToList());
    }

    /// <summary>
    /// Haalt één delict op basis van het meegegeven ID.
    /// </summary>
    /// <param name="id">Het ID van het gezochte delict.</param>
    /// <returns>Het gevonden <see cref="CrimeModel"/> of null.</returns>
    public Task<CrimeModel?> GetByIdAsync(int id)
    {
        EnsureSeeded();
        var crime = _crimes.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(crime);
    }

    /// <summary>
    /// Voegt een nieuw delict toe aan de lijst.
    /// Wijs automatisch een nieuwe ID toe
    /// </summary>
    /// <param name="crime">Het nieuwe delict dat toegevoegd moet worden.</param>
    public Task AddAsync(CrimeModel crime)
    {
        EnsureSeeded();

        crime.Id = _crimes.Count == 0 ? 1 : _crimes.Max(c => c.Id) + 1;
        _crimes.Add(crime);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Werk een bestaand delict bij. Alleen velden worden overschreven.
    /// </summary>
    /// <param name="crime">Het aangepaste delictmodel met bestaande ID.</param>
    public Task UpdateAsync(CrimeModel crime)
    {
        EnsureSeeded();

        var existing = _crimes.FirstOrDefault(c => c.Id == crime.Id);
        if (existing is null) return Task.CompletedTask;

        existing.Type = crime.Type;
        existing.Description = crime.Description;
        existing.Address = crime.Address;
        existing.DateTimeString = crime.DateTimeString;
        existing.Lat = crime.Lat;
        existing.Lng = crime.Lng;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Verwijdert een delict op basis van ID.
    /// </summary>
    /// <param name="id">Het ID van het delict dat verwijderd moet worden.</param>
    public Task DeleteAsync(int id)
    {
        EnsureSeeded();

        var existing = _crimes.FirstOrDefault(c => c.Id == id);
        if (existing != null)
            _crimes.Remove(existing);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Verwijdert alle delicten uit de lijst.
    /// </summary>
    public Task ClearAsync()
    {
        _crimes.Clear();
        _isSeeded = true; // voorkomt dat er opnieuw standaarddata wordt toegevoegd
        return Task.CompletedTask;
    }

    /// <summary>
    /// Wordt gebruikt om eenmalig standaarddata te laden.
    /// In deze implementatie is seeding uitgeschakeld.
    /// </summary>
    private void EnsureSeeded()
    {
        if (_isSeeded) return;

        // Geen standaard delicten toevoegen
        _isSeeded = true;
    }
}
