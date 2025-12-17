using WijkAgent.Core.Services;
using WijkAgent.Core.Models;

namespace WijkAgent.Tests;

/// <summary>
/// Fake implementatie van ICrimeService voor unit testing zonder database.
/// Alle CRUD- en statistiekmethoden zijn in geheugen geïmplementeerd.
/// </summary>
public class FakeCrimeService : ICrimeService
{
    // Lijst die dient als "in-memory database"
    private readonly List<Crime> _crimes = new();

    /// <summary>
    /// Voegt een nieuw delict toe aan de in-memory lijst.
    /// </summary>
    public Task<Crime> AddAsync(Crime crime)
    {
        crime.Id = _crimes.Count + 1; // Automatisch ID toewijzen
        crime.CreatedAt = DateTime.Now;
        _crimes.Add(crime);
        return Task.FromResult(crime);
    }

    /// <summary>
    /// Verwijdert een delict op basis van ID. Retourneert true als succesvol.
    /// </summary>
    public Task<bool> DeleteAsync(int id)
    {
        var crime = _crimes.FirstOrDefault(c => c.Id == id);
        if (crime == null) return Task.FromResult(false);

        _crimes.Remove(crime);
        return Task.FromResult(true);
    }

    /// <summary>
    /// Haalt alle delicten op.
    /// </summary>
    public Task<List<Crime>> GetAllAsync() => Task.FromResult(_crimes.ToList());

    /// <summary>
    /// Haalt één delict op basis van ID.
    /// </summary>
    public Task<Crime?> GetByIdAsync(int id) => Task.FromResult(_crimes.FirstOrDefault(c => c.Id == id));

    /// <summary>
    /// Haalt het meest recent aangemaakte delict op.
    /// </summary>
    public Task<Crime?> GetNewestAsync() => Task.FromResult(_crimes.OrderByDescending(c => c.CreatedAt).FirstOrDefault());

    /// <summary>
    /// Filtert delicten op datum, type en stad.
    /// </summary>
    public Task<List<Crime>> GetFilteredAsync(DateTime? from, DateTime? to, string? type, string? city)
    {
        var query = _crimes.AsQueryable();

        if (from.HasValue) query = query.Where(c => c.IncidentDateTime >= from.Value);
        if (to.HasValue) query = query.Where(c => c.IncidentDateTime <= to.Value);
        if (!string.IsNullOrWhiteSpace(type)) query = query.Where(c => c.Type == type);
        if (!string.IsNullOrWhiteSpace(city)) query = query.Where(c => c.City == city);

        return Task.FromResult(query.ToList());
    }

    /// <summary>
    /// Update een bestaand delict. Retourneert null als het delict niet bestaat.
    /// </summary>
    public Task<Crime?> UpdateAsync(Crime crime)
    {
        var existing = _crimes.FirstOrDefault(c => c.Id == crime.Id);
        if (existing == null) return Task.FromResult<Crime?>(null);

        existing.Type = crime.Type;
        existing.Description = crime.Description;
        existing.Street = crime.Street;
        existing.City = crime.City;
        existing.HouseNumber = crime.HouseNumber;
        existing.Postcode = crime.Postcode;
        existing.Province = crime.Province;
        existing.Lat = crime.Lat;
        existing.Lng = crime.Lng;
        existing.IncidentDateTime = crime.IncidentDateTime;

        return Task.FromResult(existing);
    }

    // =========================
    // Statistiek-methoden
    // =========================

    public Task<int> GetTotalCountAsync() => Task.FromResult(_crimes.Count);

    public Task<int> GetCountInRangeAsync(DateTime from, DateTime to)
        => Task.FromResult(_crimes.Count(c => c.IncidentDateTime >= from && c.IncidentDateTime <= to));

    public Task<(int CurrentMonth, int PreviousMonth)> GetThisAndPreviousMonthCountsAsync()
    {
        var now = DateTime.Now;
        int current = _crimes.Count(c => c.IncidentDateTime.Month == now.Month && c.IncidentDateTime.Year == now.Year);
        int previous = _crimes.Count(c => c.IncidentDateTime.Month == now.AddMonths(-1).Month && c.IncidentDateTime.Year == now.Year);
        return Task.FromResult((current, previous));
    }

    public Task<List<(string Type, int Count)>> GetCountsByTypeAsync(int top = 6)
    {
        var query = _crimes.GroupBy(c => c.Type)
            .Select(g => (Type: g.Key ?? "Onbekend", Count: g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        if (top > 0) query = query.Take(top).ToList();
        return Task.FromResult(query);
    }

    public Task<List<(string City, int Count)>> GetTopCitiesAsync(int top = 5)
    {
        var query = _crimes.GroupBy(c => c.City)
            .Select(g => (City: g.Key ?? "Onbekend", Count: g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        if (top > 0) query = query.Take(top).ToList();
        return Task.FromResult(query);
    }

    public Task<List<(string Label, int Count)>> GetCountsByTimeSlotAsync()
        => Task.FromResult(new List<(string Label, int Count)>()); // Dummy-implementatie

    public Task<List<(DateTime Date, int Count)>> GetCountsPerDayAsync(int days = 7)
    {
        var result = Enumerable.Range(0, days)
            .Select(i =>
            {
                var date = DateTime.Today.AddDays(-i);
                int count = _crimes.Count(c => c.IncidentDateTime.Date == date);
                return (Date: date, Count: count);
            })
            .ToList();

        return Task.FromResult(result);
    }
}
