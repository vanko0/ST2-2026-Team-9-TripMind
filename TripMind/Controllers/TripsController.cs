using Microsoft.AspNetCore.Mvc;
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

    public IActionResult Log()
    {
        return View(AppLogger.Instance.GetEntries());
    }
}
