using TripMind.Models;

namespace TripMind.Repositories;

public interface ITripRepository
{
    Task<List<Trip>> GetFilteredAsync(string? destination, decimal? maxBudget, string? travelStyle);
}
