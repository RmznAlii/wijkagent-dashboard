using WijkAgent.Core.Services;
using WijkAgent.Core.Models;

namespace WijkAgent.Tests;

public class FakeCrimeService : ICrimeService
{
    private readonly List<Crime> _crimes = new();

    public Task<Crime> AddAsync(Crime crime)
    {
        crime.Id = _crimes.Count + 1;
        crime.CreatedAt = DateTime.Now;
        _crimes.Add(crime);
        return Task.FromResult(crime);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var crime = _crimes.FirstOrDefault(c => c.Id == id);
        if (crime == null) return Task.FromResult(false);

        _crimes.Remove(crime);
        return Task.FromResult(true);
    }

    public Task<List<Crime>> GetAllAsync() => Task.FromResult(_crimes.ToList());

    public Task<Crime?> GetByIdAsync(int id) => Task.FromResult(_crimes.FirstOrDefault(c => c.Id == id));

    public Task<Crime?> GetNewestAsync() => Task.FromResult(_crimes.OrderByDescending(c => c.CreatedAt).FirstOrDefault());

    public Task<List<Crime>> GetFilteredAsync(DateTime? from, DateTime? to, string? type, string? city)
    {
        var query = _crimes.AsQueryable();

        if (from.HasValue) query = query.Where(c => c.IncidentDateTime >= from.Value);
        if (to.HasValue) query = query.Where(c => c.IncidentDateTime <= to.Value);
        if (!string.IsNullOrWhiteSpace(type)) query = query.Where(c => c.Type == type);
        if (!string.IsNullOrWhiteSpace(city)) query = query.Where(c => c.City == city);

        return Task.FromResult(query.ToList());
    }

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

    // Statistiek-methoden
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
        => Task.FromResult(new List<(string Label, int Count)>());

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
