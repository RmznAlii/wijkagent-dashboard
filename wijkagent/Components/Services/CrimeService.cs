using WijkAgent.Models;

namespace WijkAgent.Services;

public class CrimeService : ICrimeService
{
    private readonly List<CrimeModel> _crimes = new();
    private bool _isSeeded;

    public Task<List<CrimeModel>> GetAllAsync()
    {
        EnsureSeeded();
        return Task.FromResult(_crimes.ToList());
    }

    public Task<CrimeModel?> GetByIdAsync(int id)
    {
        EnsureSeeded();
        var crime = _crimes.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(crime);
    }

    public Task AddAsync(CrimeModel crime)
    {
        EnsureSeeded();

        crime.Id = _crimes.Count == 0 ? 1 : _crimes.Max(c => c.Id) + 1;
        _crimes.Add(crime);

        return Task.CompletedTask;
    }

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

    public Task DeleteAsync(int id)
    {
        EnsureSeeded();

        var existing = _crimes.FirstOrDefault(c => c.Id == id);
        if (existing != null)
            _crimes.Remove(existing);

        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        _crimes.Clear();
        _isSeeded = true; // voorkom opnieuw seeden
        return Task.CompletedTask;
    }

    private void EnsureSeeded()
    {
        // Geen default delicten meer seeden
        if (_isSeeded) return;

        // Hier niks toevoegen
        _isSeeded = true;
    }
}