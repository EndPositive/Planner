using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Planner.Data;
using Planner.Models;
using System.Diagnostics;

namespace Planner.Controllers
{
    public class ShiftsController : Controller
    {
        private readonly ShiftContext _context;
        private readonly AvailabilityContext _availabilityContext;

        public ShiftsController(ShiftContext context, AvailabilityContext availabilityContext)
        {
            _context = context;
            _availabilityContext = availabilityContext;
        }

        // GET: Shifts/Week
        [Route("Shifts/")]
        [Route("Shifts/Index")]
        [Route("Shifts/Week")]
        public IActionResult Week(DateTime? date, string calendar = "All")
        {
            var shifts = from m in _context.Shift select m;

            // Get available types
            var types = new List<string>();
            foreach (var shift in shifts)
            {
                if (!types.Contains(shift.Title)) { types.Add(shift.Title); }
            }
            ViewBag.Types = types;
            ViewBag.SelectedType = calendar;

            // Only show current week
            DateTime trueDate = date ?? DateTime.Now;
            ViewBag.date = trueDate;

            System.Globalization.CultureInfo ci = System.Threading.Thread.CurrentThread.CurrentCulture;
            DayOfWeek fdow = ci.DateTimeFormat.FirstDayOfWeek;
            DayOfWeek today = trueDate.DayOfWeek;
            DateTime monday = new DateTime();
            if (today == DayOfWeek.Sunday)
            {
                monday = trueDate.AddDays(-(today - fdow) -7);
            } else
            {
                monday = trueDate.AddDays(-(today - fdow));
            }

            shifts = shifts.Where(m => m.Date.Date >= monday.Date && m.Date.Date <= monday.AddDays(6).Date);

            // Only show selected calendar
            if (calendar != "All")
            {
                shifts = shifts.Where(m => m.Title == calendar);
            }

            // Group by day mon-sun
            var days = new List<Day>();

            for (var i = 0; i < 7; i++)
            {
                var day = new Day {
                    Date = monday.AddDays(i),
                    Shifts = shifts.Where(m => m.Date.Day == monday.AddDays(i).Day)
                    .OrderBy(m => m.Title)
                    .ThenBy(m => m.StartTime)
                    .ToList<Shift>()
                };
                days.Add(day);
            }


            return View("Week", days);
        }

        // GET: Shifts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Availabilities/Create
        [HttpPost]
        public async Task<IActionResult> Create(string Title, string Details, DateTime Date, TimeSpan StartTime, TimeSpan EndTime)
        {
            if (StartTime >= EndTime) return BadRequest("bad_times");

            var shift = new Shift() {
                Title = Title,
                Details = Details,
                Date = Date,
                StartTime = StartTime,
                EndTime = EndTime,
                Series = 0
            };

            _context.Add(shift);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // POST: Availabilities/CreateDailyEvery
        [HttpPost]
        public async Task<IActionResult> CreateDaily(string Title, string Details, DateTime Date, TimeSpan StartTime, TimeSpan EndTime, int Pattern, DateTime Range)
        {
            try
            {
                if (StartTime >= EndTime) return BadRequest("bad_times");

                var primary = new Shift() {
                    Title = Title,
                    Details = Details,
                    Date = Date,
                    StartTime = StartTime,
                    EndTime = EndTime,
                    Series = 0
                };

                _context.Add(primary);
                await _context.SaveChangesAsync();
                primary.Series = primary.Id;
                _context.Update(primary);

                var series = primary.Id;

                var shifts = new List<Shift>();

                var totalDays = (Range - Date).TotalDays;
                for (var i = 1; i <= totalDays; i++)
                {
                    Date = Date.AddDays(1);
                    if (i % Pattern == 0)
                    {
                        var shift = new Shift() {
                            Title = Title,
                            Details = Details,
                            Date = Date,
                            StartTime = StartTime,
                            EndTime = EndTime,
                            Series = series
                        };

                        _context.Add(shift);
                    }
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
        public async Task<IActionResult> CreateDailyWeekdays(string Title, string Details, DateTime Date, TimeSpan StartTime, TimeSpan EndTime, DateTime Range)
        {
            try
            {
                if (StartTime >= EndTime) return BadRequest("bad_times");

                var primary = new Shift()
                {
                    Title = Title,
                    Details = Details,
                    Date = Date,
                    StartTime = StartTime,
                    EndTime = EndTime,
                    Series = 0
                };

                _context.Add(primary);
                await _context.SaveChangesAsync();
                primary.Series = primary.Id;
                _context.Update(primary);

                var series = primary.Id;

                var shifts = new List<Shift>();

                var totalDays = (Range - Date).TotalDays;
                for (var i = 1; i <= totalDays; i++)
                {
                    Date = Date.AddDays(1);
                    if (!(Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday))
                    {
                        var shift = new Shift()
                        {
                            Title = Title,
                            Details = Details,
                            Date = Date,
                            StartTime = StartTime,
                            EndTime = EndTime,
                            Series = series
                        };

                        _context.Add(shift);
                    }
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
        public async Task<IActionResult> CreateWeekly(string Title, string Details, DateTime Date, TimeSpan StartTime, TimeSpan EndTime, int Pattern, String[] days, DateTime Range)
        {
            try
            {
                if (StartTime >= EndTime) return BadRequest("bad_times");

                var primary = new Shift()
                {
                    Title = Title,
                    Details = Details,
                    Date = Date,
                    StartTime = StartTime,
                    EndTime = EndTime,
                    Series = 0
                };

                _context.Add(primary);
                await _context.SaveChangesAsync();
                primary.Series = primary.Id;
                _context.Update(primary);

                var series = primary.Id;

                var shifts = new List<Shift>();

                var totalDays = (Range - Date).TotalDays;
                var weeknumber = 0;
                for (var i = 1; i <= totalDays + 1; i++)
                {
                    Date = Date.AddDays(1);
                    if (days.Contains(Date.DayOfWeek.ToString().ToLower()) && (weeknumber % Pattern == 0 || weeknumber == 0))
                    {
                        var shift = new Shift()
                        {
                            Title = Title,
                            Details = Details,
                            Date = Date,
                            StartTime = StartTime,
                            EndTime = EndTime,
                            Series = series
                        };

                        _context.Add(shift);
                    }

                    if ((i - 1) % 7 == 0) weeknumber++;
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }

        }

        // GET: Shifts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shift = await _context.Shift.FindAsync(id);
            if (shift == null)
            {
                return NotFound();
            }
            return View(shift);
        }

        // POST: Shifts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Details,Date,StartTime,EndTime,Users")] Shift shift)
        {
            if (id != shift.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shift);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShiftExists(shift.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(shift);
        }

        // GET: Shifts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shift = await _context.Shift
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shift == null)
            {
                return NotFound();
            }

            return View(shift);
        }

        // POST: Shifts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shift = await _context.Shift.FindAsync(id);
            _context.Shift.Remove(shift);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ShiftExists(int id)
        {
            return _context.Shift.Any(e => e.Id == id);
        }

        // POST: Shifts/Assign
        [HttpPost]
        public IActionResult Assign(int shiftId, string assign)
        {
            try
            {
                int availabilityId = 0;
                if (int.TryParse(assign, out availabilityId))
                {
                    if (AssignAvailability(shiftId, availabilityId))
                    {
                        return Ok();
                    } else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    if (AssignAvailability(shiftId, assign))
                    {
                        return Ok();
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShiftExists(shiftId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // POST: Shifts/AssignRecurringly
        [HttpPost]
        public async Task<IActionResult> AssignRecurringly(int shiftId, int availabilityId, int availableWeeks)
        {
            try
            {
                var shift = await _context.Shift.FindAsync(shiftId);
                if (shift == null)
                {
                    return NotFound();
                }

                var availability = await _availabilityContext.Availability.FindAsync(availabilityId);
                if (availability == null)
                {
                    return NotFound();
                }

                var shifts = from m in _context.Shift select m;
                var availabilities = from m in _availabilityContext.Availability select m;

                for (var i = 0; i < availableWeeks; i++)
                {
                    var shiftOnDate = shifts.Where(m =>
                        m.Date == shift.Date.AddDays(i * 7) &&
                        m.Series == shift.Series &&
                        String.IsNullOrEmpty(m.Users));

                    if (!shiftOnDate.Any())
                    {
                        return BadRequest();
                    }

                    var availabilityOnDate = availabilities.Where(m =>
                        m.Date == availability.Date.AddDays(i * 7) &&
                        m.Series == availability.Series);

                    if (!availabilityOnDate.Any())
                    {
                        return BadRequest();
                    }

                    if (!AssignAvailability(shiftOnDate.ToList()[0].Id, availabilityOnDate.ToList()[0].Id))
                    {
                        return BadRequest();
                    }
                }

                return Ok();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShiftExists(shiftId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        public bool AssignAvailability(int shiftId, int availabilityId)
        {
            var availability = _availabilityContext.Availability.Find(availabilityId);
            if (availability == null)
            {
                return false;
            }

            var shift = _context.Shift.Find(shiftId);
            if (shift == null)
            {
                return false;
            }

            if (shift.Date != availability.Date)
            {
                return false;
            }

            // 0
            if (shift.StartTime == availability.StartTime && shift.EndTime == availability.EndTime)
            {
                shift.Users = availability.Username;
                _context.Update(shift);
            }
            // 1
            else if (shift.StartTime >= availability.StartTime && shift.EndTime > availability.EndTime)
            {
                var extraShiftEnd = new Shift()
                {
                    Title = shift.Title,
                    Details = shift.Details,
                    Date = shift.Date,
                    StartTime = availability.EndTime,
                    EndTime = shift.EndTime,
                };
                shift.EndTime = availability.EndTime;
                shift.Users = availability.Username;

                _context.Update(shift);
                _context.Add(extraShiftEnd);
            }
            // 2
            else if (shift.StartTime >= availability.StartTime && shift.EndTime < availability.EndTime)
            {
                shift.Users = availability.Username;
                _context.Update(shift);
            }
            // 3
            else if (shift.StartTime < availability.StartTime && shift.EndTime > availability.EndTime)
            {
                var extraShiftStart = new Shift()
                {
                    Title = shift.Title,
                    Details = shift.Details,
                    Date = shift.Date,
                    StartTime = shift.StartTime,
                    EndTime = availability.StartTime,
                };

                var extraShiftEnd = new Shift()
                {
                    Title = shift.Title,
                    Details = shift.Details,
                    Date = shift.Date,
                    StartTime = availability.EndTime,
                    EndTime = shift.EndTime,
                };

                shift.StartTime = availability.StartTime;
                shift.EndTime = availability.EndTime;
                shift.Users = availability.Username;

                _context.Update(shift);
                _context.Add(extraShiftStart);
                _context.Add(extraShiftEnd);
            }
            // 4
            else if (shift.StartTime < availability.StartTime && shift.EndTime <= availability.EndTime)
            {
                var extraShiftStart = new Shift()
                {
                    Title = shift.Title,
                    Details = shift.Details,
                    Date = shift.Date,
                    StartTime = shift.StartTime,
                    EndTime = availability.StartTime,
                };
                shift.StartTime = availability.StartTime;
                shift.Users = availability.Username;

                _context.Update(shift);
                _context.Add(extraShiftStart);
            }
            else
            {
                return false;
            }

            _context.SaveChanges();
            return true;
        }

        public bool AssignAvailability(int shiftId, string username)
        {
            var shift = _context.Shift.Find(shiftId);
            if (shift == null)
            {
                return false;
            }

            shift.Users = username;
            _context.Update(shift);

            _context.SaveChanges();
            return true;
        }

        // POST: Shifts/UnAssign
        [HttpPost]
        public async Task<IActionResult> UnAssign(int shiftId)
        {
            try
            {
                var shift = await _context.Shift.FindAsync(shiftId);
                if (shift == null)
                {
                    return NotFound();
                }
                shift.Users = string.Empty;
                _context.Update(shift);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShiftExists(shiftId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // GET: Shifts/GetAvailabilitiesForShift
        public IActionResult GetAvailabilitiesForShift(int ShiftId)
        {
            var shift = _context.Shift.Find(ShiftId);
            var shifts = from m in _context.Shift select m;
            var availabilities = from m in _availabilityContext.Availability select m;

            var actualAvailabilities = GetActualAvailabilitiesForShift(shift);

            // Calculate the amount of weeks that the same shift takes place.
            var shiftWeeks = 0;
            while (shifts.Where(m =>
                m.Date == shift.Date.AddDays(shiftWeeks * 7) && 
                m.Series == shift.Series && 
                String.IsNullOrEmpty(m.Users)
                ).Any()) {
                shiftWeeks++;
            }

            var recurringWeeklyAvailabilities = new List<RecurringWeeklyAvailability>();

            foreach (var availability in actualAvailabilities)
            {
                var availableWeeks = 1;
                var availableTilDate = availability.Date;
                while (availabilities.Where(m =>
                    m.Date == availability.Date.AddDays(availableWeeks * 7) &&
                    m.Series == availability.Series
                    ).Any())
                {
                    var shiftOnDate = shifts.Where(m =>
                        m.Date == shift.Date.AddDays(availableWeeks * 7) &&
                        m.Series == shift.Series &&
                        String.IsNullOrEmpty(m.Users));
                    if (shiftOnDate.Any())
                    {
                        var actualAvailabilitiesOnDate = GetActualAvailabilitiesForShift(shiftOnDate.ToList()[0]).Where(m => m.Username == availability.Username);
                        if (actualAvailabilitiesOnDate.Any())
                        {
                            availableWeeks++;
                            availableTilDate = availability.Date.AddDays((availableWeeks - 1) * 7);
                        }
                        else
                        {
                            break;
                        }
                    } else
                    {
                        break;
                    }
                }

                var recurringWeeklyAvailability = new RecurringWeeklyAvailability() { 
                    availableWeeks = availableWeeks,
                    availableTilDate = availableTilDate,
                    availability = availability
                };

                recurringWeeklyAvailabilities.Add(recurringWeeklyAvailability);
            }


            var json = JsonConvert.SerializeObject(recurringWeeklyAvailabilities);

            return Content(json);
        }

        private List<Availability> GetActualAvailabilitiesForShift(Shift shift)
        {
            var shifts = from m in _context.Shift select m;
            var availabilities = from m in _availabilityContext.Availability select m;
            var availabilitiesForShift = availabilities.Where(m => m.Date == shift.Date);

            var actualAvailabilities = new List<Availability>();

            foreach (var availability in availabilitiesForShift)
            {
                var actualAvailability = new List<Availability>();

                var assignedShifts = shifts.Where(m => m.Date == availability.Date && m.Users == availability.Username);

                // User has multiple shifts assigned...
                if (assignedShifts.Count() > 1)
                {
                    foreach (var assignedShift in assignedShifts)
                    {
                        actualAvailability.AddRange(
                            GetAvailabilitiesAroundShift(assignedShift, availability)
                        );
                    }
                }
                // User has one shifts assigned
                else if (assignedShifts.Count() == 1)
                {
                    actualAvailability.AddRange(
                        GetAvailabilitiesAroundShift(assignedShifts.First(), availability)
                    );
                }
                // User has no shifts assigned
                else
                {
                    actualAvailability.Add(availability);
                }

                actualAvailabilities.AddRange(actualAvailability);
            }

            // Remove duplicates
            actualAvailabilities = actualAvailabilities.Distinct().ToList();

            // Remove availabilities that don't overlap with the shift
            for (var i = actualAvailabilities.Count() - 1; i >= 0; i--)
            {
                if (!AvailabilityOverlapsWithShift(shift, actualAvailabilities[i]))
                {
                    actualAvailabilities.Remove(actualAvailabilities[i]);
                }
            }

            // Order by available time
            actualAvailabilities = actualAvailabilities.OrderBy(m => m.StartTime - m.EndTime).ToList();

            return actualAvailabilities;
        }

        private List<Availability> GetAvailabilitiesAroundShift(Shift shift, Availability availability)
        {
            var availabilities = new List<Availability>();
            // User is available before the shift.
            if (shift.StartTime > availability.StartTime && shift.EndTime >= availability.EndTime)
            {
                availability.EndTime = shift.StartTime;
                availabilities.Add(availability);
            }
            // User is available before and after the shift.
            else if (shift.StartTime > availability.StartTime && shift.EndTime < availability.EndTime)
            {
                // After
                var extraAvailability = new Availability()
                {
                    Date = availability.Date,
                    StartTime = shift.EndTime,
                    EndTime = availability.EndTime,
                    Username = availability.Username
                };
                availabilities.Add(extraAvailability);

                // Before
                availability.EndTime = shift.StartTime;
                availabilities.Add(availability);
            }
            // User is available after the shift.
            else if (shift.StartTime <= availability.StartTime && shift.EndTime < availability.EndTime)
            {
                availability.StartTime = shift.EndTime;
                availabilities.Add(availability);
            }
            return availabilities;
        }

        private bool AvailabilityOverlapsWithShift(Shift shift, Availability availability)
        {
            if (availability.StartTime <= shift.EndTime && availability.StartTime >= shift.StartTime ||
                availability.StartTime <= shift.StartTime && availability.EndTime >= shift.StartTime)
            {
                return true;
            }

            return false;
        }
    }
}
