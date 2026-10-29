using System.ComponentModel.DataAnnotations;

namespace TripMind.Models;

public class BookingFormViewModel
{
    public int TripId { get; set; }

    // Shown on the page, not filled in by the user
    public string? From { get; set; }
    public string? Destination { get; set; }

    [Required(ErrorMessage = "Please enter the number of passengers.")]
    [Range(1, 20, ErrorMessage = "Passengers must be between 1 and 20.")]
    public int? Travelers { get; set; }

    [Required(ErrorMessage = "Please choose a departure date.")]
    public DateOnly? StartDate { get; set; }

    [Required(ErrorMessage = "Please choose a return date.")]
    public DateOnly? EndDate { get; set; }

    [Required(ErrorMessage = "Please enter your budget.")]
    [Range(1, 1000000, ErrorMessage = "Budget must be greater than 0.")]
    public decimal? Budget { get; set; }
}