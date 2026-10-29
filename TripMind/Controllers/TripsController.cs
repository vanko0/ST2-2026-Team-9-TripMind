using Microsoft.AspNetCore.Mvc;
using TripMind.Models;
using TripMind.Repositories;
using TripMind.Services;

namespace TripMind.Controllers;

public class TripsController : Controller
{
    private readonly ITripRepository _repo;

    public TripsController(ITripRepository repo)
    {
        _repo = repo;
    }

    // "My Flights": only the trips the user has booked
    public async Task<IActionResult> Index(string? destination, string? from, decimal? maxBudget, string? travelStyle)
    {
        ViewData["destination"] = destination;
        ViewData["from"] = from;
        ViewData["maxBudget"] = maxBudget;
        ViewData["travelStyle"] = travelStyle;

        var trips = await _repo.GetFilteredAsync(destination, from, maxBudget, travelStyle, isBooked: true);
        return View(trips);
    }

    // GET: Trips/Book/5  (the booking form)
    [HttpGet]
    public async Task<IActionResult> Book(int id)
    {
        var offer = await _repo.GetByIdAsync(id);
        if (offer == null || offer.IsBooked)
            return NotFound();

        return View(new BookingFormViewModel
        {
            TripId = offer.Id,
            From = offer.From,
            Destination = offer.Destination
        });
    }

    // POST: Trips/Book/5  (submit the form)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(int id, BookingFormViewModel model)
    {
        var offer = await _repo.GetByIdAsync(id);
        if (offer == null || offer.IsBooked)
            return NotFound();

        // These are display-only, so refill them in case the form is shown again
        model.TripId = offer.Id;
        model.From = offer.From;
        model.Destination = offer.Destination;

        if (model.StartDate.HasValue && model.StartDate.Value < DateOnly.FromDateTime(DateTime.Today))
            ModelState.AddModelError(nameof(model.StartDate), "Departure date cannot be in the past.");

        if (model.StartDate.HasValue && model.EndDate.HasValue && model.EndDate < model.StartDate)
            ModelState.AddModelError(nameof(model.EndDate), "Return date cannot be before the departure date.");

        if (!ModelState.IsValid)
            return View(model);

        await _repo.BookAsync(offer.Id, model.Travelers!.Value, model.StartDate!.Value, model.EndDate!.Value, model.Budget!.Value);
        return RedirectToAction(nameof(Index));
    }

    // POST: Trips/Cancel/5  (remove a booking from My Flights)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var cancelled = await _repo.CancelBookingAsync(id);
        if (!cancelled)
            return NotFound();

        return RedirectToAction(nameof(Index));
    }

    // GET: Trips/Update/5
    public async Task<IActionResult> Update(int id)
    {
        var trip = await _repo.GetByIdAsync(id);
        if (trip == null)
            return NotFound();

        return View(trip);
    }

    // POST: Trips/Update/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, Trip trip)
    {
        if (id != trip.Id)
            return BadRequest();

        if (trip.EndDate < trip.StartDate)
            ModelState.AddModelError(nameof(trip.EndDate), "End date cannot be before start date.");

        if (!ModelState.IsValid)
            return View(trip);

        var updated = await _repo.UpdateAsync(trip);
        if (!updated)
            return NotFound();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Log()
    {
        return View(AppLogger.Instance.GetEntries());
    }
}