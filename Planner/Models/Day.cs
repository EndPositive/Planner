using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Planner.Models
{
    public class Day
    {
        public DateTime Date { get; set; }

        public List<Shift> Shifts { get; set; }
    }
}
