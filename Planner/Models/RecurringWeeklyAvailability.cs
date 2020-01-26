using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Planner.Models
{
    public class RecurringWeeklyAvailability
    {
        public int availableWeeks { get; set; }

        public DateTime availableTilDate { get; set; }

        public Availability availability { get; set; }
    }
}
