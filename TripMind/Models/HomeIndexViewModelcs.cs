using System;
using System.Collections.Generic;

namespace TripMind.Models
{
    public class HomeIndexViewModel
    {
        public List<string> Origins { get; set; } = new();
        public List<string> Destinations { get; set; } = new();
        public List<Trip> Trips { get; set; } = new();

        public string? SelectedFrom { get; set; }
        public string? SelectedDestination { get; set; }
        public DateTime? SelectedStartDate { get; set; }
        public DateTime? SelectedEndDate { get; set; }
        public int? SelectedTravelers { get; set; }
        public decimal? SelectedMaxBudget { get; set; }
    }
}