using System.Threading.Tasks;
using TripMind.Models;

namespace TripMind.Repositories;

public interface ITripRepository
{
    Task<List<Trip>> GetFilteredAsync(string? destination, decimal? maxBudget, string? travelStyle);
    Task<Trip?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(Trip trip);

}
