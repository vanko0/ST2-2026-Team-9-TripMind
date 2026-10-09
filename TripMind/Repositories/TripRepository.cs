using Microsoft.EntityFrameworkCore;
using TripMind.Data;
using TripMind.Models;

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

        if (!string.IsNullOrWhiteSpace(destination))
            query = query.Where(t => t.Destination.Contains(destination));

        if (maxBudget.HasValue)
            query = query.Where(t => t.Budget <= maxBudget.Value);

        if (!string.IsNullOrWhiteSpace(travelStyle))
            query = query.Where(t => t.TravelStyle == travelStyle);

        return await query.OrderBy(t => t.StartDate).ToListAsync();
    }
}
