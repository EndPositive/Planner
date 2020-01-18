using Microsoft.EntityFrameworkCore;
using Planner.Models;

namespace Planner.Data
{
    public class AvailabilityContext : DbContext
    {
        public AvailabilityContext(DbContextOptions<AvailabilityContext> options)
            : base(options)
        {
        }

        public DbSet<Availability> Availability { get; set; }
    }
}