using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Planner.Data;
using Planner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using System.Diagnostics;


namespace Planner.Controllers
{
    [Authorize]
    public class AvailabilitiesController : Controller
    {
        private readonly AvailabilityContext _context;

        public AvailabilitiesController(AvailabilityContext context)
        {
            _context = context;
        }

        // GET: Availabilities
        public async Task<IActionResult> Index()
        {
            var availability = from m in _context.Availability select m;
            availability = availability.Where(m => m.Username == User.Identity.Name);
            return View(await availability.ToListAsync());
        }

        // GET: Availabilities/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Availabilities/Create
        [HttpPost]
        public async Task<IActionResult> Create(DateTime Date, TimeSpan StartTime, TimeSpan EndTime)
        {
            if (StartTime >= EndTime) return BadRequest("bad_times");

            var availability = new Availability();
            availability.Date = Date;
            availability.StartTime = StartTime;
            availability.EndTime = EndTime;
            availability.Username = User.Identity.Name;
            availability.Series = 0;

            if (AvailabilityOverlaps(availability)) return BadRequest("overlap");

            _context.Add(availability);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // POST: Availabilities/CreateDailyEvery
        [HttpPost]
        public async Task<IActionResult> CreateDaily(DateTime Date, TimeSpan StartTime, TimeSpan EndTime, int Pattern, DateTime Range)
        {
            try
            {
                if (StartTime >= EndTime) return BadRequest("bad_times");

                var availabilities = new List<Availability>();

                var totalDays = (Range - Date).TotalDays;
                for (var i = 1; i <= totalDays; i++)
                {
                    Date = Date.AddDays(1);
                    if (i % Pattern == 0)
                    {
                        var availability = new Availability();
                        availability.Date = Date;
                        availability.StartTime = StartTime;
                        availability.EndTime = EndTime;
                        availability.Username = User.Identity.Name;

                        if (AvailabilityOverlaps(availability)) return BadRequest("overlap");

                        availabilities.Append(availability);
                    }
                }

                var primary = new Availability();
                primary.Date = Date;
                primary.StartTime = StartTime;
                primary.EndTime = EndTime;
                primary.Username = User.Identity.Name;
                primary.Series = 0;

                if (AvailabilityOverlaps(primary)) return BadRequest("overlap");

                _context.Add(primary);
                await _context.SaveChangesAsync();
                primary.Series = primary.Id;
                _context.Update(primary);

                var series = primary.Id;

                foreach (var availability in availabilities)
                {
                    availability.Series = series;
                    _context.Add(availability);
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        // POST: Availabilities/CreateDailyWeekdays
        [HttpPost]
        public async Task<IActionResult> CreateDailyWeekdays(DateTime Date, TimeSpan StartTime, TimeSpan EndTime, DateTime Range)
        {
            try
            {
                if (StartTime >= EndTime) return BadRequest("bad_times");

                var availabilities = new List<Availability>();

                var totalDays = (Range - Date).TotalDays;
                for (var i = 1; i <= totalDays; i++)
                {
                    Date = Date.AddDays(1);
                    if (!(Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday))
                    {
                        var availability = new Availability();
                        availability.Date = Date;
                        availability.StartTime = StartTime;
                        availability.EndTime = EndTime;
                        availability.Username = User.Identity.Name;

                        if (AvailabilityOverlaps(availability)) return BadRequest("overlap");

                        availabilities.Add(availability);
                    }
                }

                var primary = new Availability();
                primary.Date = Date;
                primary.StartTime = StartTime;
                primary.EndTime = EndTime;
                primary.Username = User.Identity.Name;
                primary.Series = 0;

                if (AvailabilityOverlaps(primary)) return BadRequest("overlap");

                _context.Add(primary);
                await _context.SaveChangesAsync();
                primary.Series = primary.Id;
                _context.Update(primary);

                var series = primary.Id;

                foreach (var availability in availabilities)
                {
                    availability.Series = series;
                    _context.Add(availability);
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }

        }

        // POST: Availabilities/CreateWeeklyEvery
        [HttpPost]
        public async Task<IActionResult> CreateWeekly(DateTime Date, TimeSpan StartTime, TimeSpan EndTime, int Pattern, String[] days, DateTime Range)
        {
            try
            {
                if (StartTime >= EndTime) return BadRequest("bad_times");

                var availabilities = new List<Availability>();

                var totalDays = (Range - Date).TotalDays;
                var weeknumber = 0;
                for (var i = 1; i <= totalDays + 1; i++)
                {
                    Date = Date.AddDays(1);
                    if (days.Contains(Date.DayOfWeek.ToString().ToLower()) && (weeknumber % Pattern == 0 || weeknumber == 0))
                    {
                        var availability = new Availability();
                        availability.Date = Date;
                        availability.StartTime = StartTime;
                        availability.EndTime = EndTime;
                        availability.Username = User.Identity.Name;

                        if (AvailabilityOverlaps(availability)) return BadRequest("overlap");

                        availabilities.Add(availability);
                    }

                    if ((i - 1) % 7 == 0) weeknumber++;
                }

                var primary = new Availability();
                primary.Date = Date;
                primary.StartTime = StartTime;
                primary.EndTime = EndTime;
                primary.Username = User.Identity.Name;
                primary.Series = 0;

                if (AvailabilityOverlaps(primary)) return BadRequest("overlap");

                _context.Add(primary);
                await _context.SaveChangesAsync();
                primary.Series = primary.Id;
                _context.Update(primary);

                var series = primary.Id;

                foreach (var availability in availabilities)
                {
                    availability.Series = series;
                    _context.Add(availability);
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }

        }

        // GET: Availabilities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var availability = await _context.Availability.FindAsync(id);
            if (availability == null)
            {
                return NotFound();
            }
            return View(availability);
        }

        // POST: Availabilities/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(int id, TimeSpan StartTime, TimeSpan EndTime, DateTime Date)
        {
            if (StartTime >= EndTime) return BadRequest("bad_times");

            var availability = await _context.Availability.FindAsync(id);
            availability.Date = Date;
            availability.StartTime = StartTime;
            availability.EndTime = EndTime;
            availability.Series = 0;

            if (AvailabilityOverlaps(availability)) return BadRequest("overlap");

            _context.Availability.Update(availability);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: Availabilities/EditSeries/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> EditSeries(int id, TimeSpan StartTime, TimeSpan EndTime)
        {
            if (StartTime >= EndTime) return BadRequest("bad_times");

            var availabilities = from m in _context.Availability select m;
            availabilities = availabilities.Where(m => m.Series == id);
            
            if (availabilities.Count() == 0) return BadRequest(); 

            foreach (var availability in availabilities)
            {
                availability.StartTime = StartTime;
                availability.EndTime = EndTime;

                if (AvailabilityOverlaps(availability)) return BadRequest("overlap");
            }

            foreach (var availability in availabilities)
            {
                _context.Update(availability);
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET: Availabilities/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var availability = await _context.Availability
                .FirstOrDefaultAsync(m => m.Id == id);
            if (availability == null)
            {
                return NotFound();
            }

            return View(availability);
        }

        // POST: Availabilities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var availability = await _context.Availability.FindAsync(id);
            _context.Availability.Remove(availability);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AvailabilityExists(int id)
        {
            return _context.Availability.Any(e => e.Id == id);
        }

        private bool AvailabilityOverlaps(Availability availability)
        {
            var availabilities = from m in _context.Availability select m;
            availabilities = availabilities.Where(m => m.Username == User.Identity.Name);
            availabilities = availabilities.Where(m => m.Date == availability.Date);

            foreach (var av in availabilities)
            {
                if (av.StartTime < availability.EndTime && av.StartTime > availability.StartTime ||
                    av.StartTime < availability.StartTime && av.EndTime > availability.StartTime)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
