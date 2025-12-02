using WijkAgent.Models;

namespace WijkAgent.Services;

public interface ICrimeService
{
    Task<List<CrimeModel>> GetAllAsync();
    Task<CrimeModel?> GetByIdAsync(int id);
    Task AddAsync(CrimeModel crime);
    Task UpdateAsync(CrimeModel crime);
    Task DeleteAsync(int id);

    // Voor "Alles verwijderen"
    Task ClearAsync();
}