using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TripMind.Models;
using TripMind.Repositories;

namespace TripMind.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITripRepository _repo;

        public HomeController(ILogger<HomeController> logger, ITripRepository repo)
        {
            _logger = logger;
            _repo = repo;
        }

        public async Task<IActionResult> Index(
            string? from,
            string? destination,
            DateTime? startDate,
            DateTime? endDate,
            int? travelers,
            decimal? maxBudget)
        {
            // An empty or 0 budget means "no budget filter"
            if (maxBudget is <= 0) maxBudget = null;

            // All trips, used to fill the From / Destination dropdowns
            var allTrips = await _repo.GetFilteredAsync(null, null, null, null, isBooked: false);

            var trips = await _repo.GetFilteredAsync(
                destination: destination,
                from: from,
                maxBudget: maxBudget,
                travelStyle: null,
                isBooked: false);

            var vm = new HomeIndexViewModel
            {
                Origins = allTrips
                    .Select(t => t.From)
                    .Where(f => !string.IsNullOrWhiteSpace(f))
                    .Distinct()
                    .OrderBy(f => f)
                    .ToList(),
                Destinations = allTrips
                    .Select(t => t.Destination)
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList(),
                Trips = trips,
                SelectedFrom = from,
                SelectedDestination = destination,
                SelectedStartDate = startDate,
                SelectedEndDate = endDate,
                SelectedTravelers = travelers,
                SelectedMaxBudget = maxBudget
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}