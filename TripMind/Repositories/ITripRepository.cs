using TripMind.Models;

namespace TripMind.Repositories;

public interface ITripRepository
{
    Task<List<Trip>> GetFilteredAsync(
        string? destination,
        string? from,
        decimal? maxBudget,
        string? travelStyle,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? travelers = null,
        bool? isBooked = null);

    Task<Trip?> GetByIdAsync(int id);

    Task<bool> UpdateAsync(Trip trip);

    Task<Trip?> BookAsync(int offerId, int travelers, DateOnly startDate, DateOnly endDate, decimal budget);

    Task<bool> CancelBookingAsync(int id);
}