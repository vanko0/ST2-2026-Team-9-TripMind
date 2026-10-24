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
        var query = _db.Trips.AsQueryable();
        var filters = new List<string>();

        if (!string.IsNullOrWhiteSpace(destination))
        {
            query = query.Where(t => t.Destination.Contains(destination));
            filters.Add($"destination contains \"{destination}\"");
        }

        if (maxBudget.HasValue)
        {
            query = query.Where(t => t.Budget <= maxBudget.Value);
            filters.Add($"budget up to {maxBudget}");
        }

        if (!string.IsNullOrWhiteSpace(travelStyle))
        {
            query = query.Where(t => t.TravelStyle == travelStyle);
            filters.Add($"style is {travelStyle}");
        }

        var result = await query.OrderBy(t => t.StartDate).ToListAsync();

        var description = filters.Count == 0
            ? "No filters, all trips requested"
            : "Filters: " + string.Join(", ", filters);

        // Singleton Pattern in use: the same AppLogger instance is shared by the whole app.
        AppLogger.Instance.Log("Trips filtered", $"{description}. Found {result.Count} trip(s).");

        return result;
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