using Microsoft.EntityFrameworkCore;
using Planner.Models;

namespace Planner.Data
{
    public class ShiftContext : DbContext
    {
        public ShiftContext(DbContextOptions<ShiftContext> options)
            : base(options)
        {
        }

        public DbSet<Shift> Shift { get; set; }
    }
}