using Microsoft.EntityFrameworkCore;
using TripMind.Data;
using TripMind.Models;
using TripMind.Services;

namespace TripMind.Repositories;

// Repository Pattern: all database access is hidden behind an interface,
// so controllers never talk to EF Core directly.
public class TripRepository : ITripRepository
{
    private readonly TripMindDbContext _db;

    public TripRepository(TripMindDbContext db)
    {
        _db = db;
    }

    // SELECT with filter
    public async Task<List<Trip>> GetFilteredAsync(string? destination, decimal? maxBudget, string? travelStyle)
    {
        AppLogger.Instance.Log("Trips filtered");
        var query = _db.Trips.AsQueryable();

        if (!string.IsNullOrWhiteSpace(destination))
            query = query.Where(t => t.Destination.Contains(destination));

        if (maxBudget.HasValue)
            query = query.Where(t => t.Budget <= maxBudget.Value);

        if (!string.IsNullOrWhiteSpace(travelStyle))
            query = query.Where(t => t.TravelStyle == travelStyle);

        return await query.OrderBy(t => t.StartDate).ToListAsync();
    }

    // SELECT by id
    public async Task<Trip?> GetByIdAsync(int id)
    {
        return await _db.Trips.FindAsync(id);
    }

    // UPDATE
    public async Task<bool> UpdateAsync(Trip trip)
    {
        var existing = await _db.Trips.FindAsync(trip.Id);
        if (existing == null)
            return false;

        existing.Destination = trip.Destination;
        existing.StartDate = trip.StartDate;
        existing.EndDate = trip.EndDate;
        existing.Budget = trip.Budget;
        existing.Travelers = trip.Travelers;
        existing.TravelStyle = trip.TravelStyle;

        await _db.SaveChangesAsync();
        return true;
    }
}