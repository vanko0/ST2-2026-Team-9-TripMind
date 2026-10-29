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
    public async Task<List<Trip>> GetFilteredAsync(
        string? destination,
        string? from,
        decimal? maxBudget,
        string? travelStyle,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? travelers = null,
        bool? isBooked = null)
    {
        var query = _db.Trips.AsQueryable();
        var filters = new List<string>();

        if (isBooked.HasValue)
        {
            query = query.Where(t => t.IsBooked == isBooked.Value);
            filters.Add(isBooked.Value ? "booked only" : "available offers only");
        }

        if (!string.IsNullOrWhiteSpace(from))
        {
            query = query.Where(t => t.From == from);
            filters.Add($"from is \"{from}\"");
        }

        if (!string.IsNullOrWhiteSpace(destination))
        {
            query = query.Where(t => t.Destination.Contains(destination));
            filters.Add($"destination contains \"{destination}\"");
        }

        if (startDate.HasValue)
        {
            var start = DateOnly.FromDateTime(startDate.Value);
            query = query.Where(t => t.StartDate >= start);
            filters.Add($"departs on or after {start:yyyy-MM-dd}");
        }

        if (endDate.HasValue)
        {
            var end = DateOnly.FromDateTime(endDate.Value);
            query = query.Where(t => t.EndDate <= end);
            filters.Add($"returns on or before {end:yyyy-MM-dd}");
        }

        if (travelers.HasValue)
        {
            query = query.Where(t => t.Travelers == travelers.Value);
            filters.Add($"travelers = {travelers}");
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

        existing.From = trip.From;
        existing.Destination = trip.Destination;
        existing.StartDate = trip.StartDate;
        existing.EndDate = trip.EndDate;
        existing.Budget = trip.Budget;
        existing.Travelers = trip.Travelers;
        existing.TravelStyle = trip.TravelStyle;

        await _db.SaveChangesAsync();
        return true;
    }

    // BOOK: copy the offer into a new booked trip with the user's own values
    public async Task<Trip?> BookAsync(int offerId, int travelers, DateOnly startDate, DateOnly endDate, decimal budget)
    {
        var offer = await _db.Trips.FindAsync(offerId);
        if (offer == null || offer.IsBooked)
            return null;

        var booking = new Trip
        {
            From = offer.From,
            Destination = offer.Destination,
            StartDate = startDate,
            EndDate = endDate,
            Budget = budget,
            Travelers = travelers,
            TravelStyle = offer.TravelStyle,
            IsBooked = true
        };

        _db.Trips.Add(booking);
        await _db.SaveChangesAsync();

        AppLogger.Instance.Log("Flight booked",
            $"{booking.From} -> {booking.Destination}, {travelers} passenger(s), {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

        return booking;
    }

    // CANCEL: remove a booking (offers are never touched)
    public async Task<bool> CancelBookingAsync(int id)
    {
        var existing = await _db.Trips.FindAsync(id);
        if (existing == null || !existing.IsBooked)
            return false;

        _db.Trips.Remove(existing);
        await _db.SaveChangesAsync();

        AppLogger.Instance.Log("Booking cancelled", $"{existing.From} -> {existing.Destination} (trip #{existing.Id})");
        return true;
    }
}