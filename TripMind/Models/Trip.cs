using System.ComponentModel.DataAnnotations;

namespace TripMind.Models;

public class Trip
{
    public int Id { get; set; }

    [Required] public string Destination { get; set; } = "";

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public decimal Budget { get; set; }
    public int Travelers { get; set; } = 1;

    // Budget / Balanced / Luxury

    public string TravelStyle { get; set; } = "Balanced";
}