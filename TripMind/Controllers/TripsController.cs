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

    public async Task<IActionResult> Index(string? destination, decimal? maxBudget, string? travelStyle)
    {
        ViewData["destination"] = destination;
        ViewData["maxBudget"] = maxBudget;
        ViewData["travelStyle"] = travelStyle;

        var trips = await _repo.GetFilteredAsync(destination, maxBudget, travelStyle);
        return View(trips);
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

