using Microsoft.EntityFrameworkCore;
using TripMind.Models;

namespace TripMind.Data;

public class TripMindDbContext : DbContext
{
    public TripMindDbContext(DbContextOptions<TripMindDbContext> options) : base(options) { }

    public DbSet<Trip> Trips => Set<Trip>();
}