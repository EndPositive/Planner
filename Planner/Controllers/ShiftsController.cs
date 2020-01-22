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

        // GET: Shifts
        public IActionResult Index()
        {
            var shifts = from m in _context.Shift select m;
            shifts = shifts.OrderBy(m => m.Date).ThenBy(m => m.StartTime);
            return View(shifts);
        }

        // GET: Shifts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Shifts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Details,Date,StartTime,EndTime")] Shift shift)
        {
            if (ModelState.IsValid)
            {
                _context.Add(shift);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(shift);
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

        // GET: Shifts/availableOnDate
        public IActionResult availableOnDate(DateTime date)
        {
            var availabilities = from m in _availabilityContext.Availability select m;
            availabilities = availabilities.Where(m => m.Date == date);

            if (!availabilities.Any())
            {
                return NotFound();
            }

            var shifts = from m in _context.Shift select m;
            shifts = shifts.Where(m => m.Date == date);


            var actualAvailabilities = new List<Availability>();
            foreach (var availability in availabilities)
            {
                // Get shifts that have the user assigned
                var userShifts = shifts.Where(m => m.Users == availability.Username);

                // If there are no assigned shifts, add the availability.
                if (!userShifts.Any())
                {
                    actualAvailabilities.Add(availability);
                } else
                {
                    foreach (var userShift in userShifts)
                    {
                        // User isn't available at all.
                        if (userShift.StartTime == availability.StartTime && userShift.EndTime == availability.EndTime)
                        {
                        }
                        // User is available before the shift.
                        else if (userShift.StartTime >= availability.StartTime && userShift.EndTime > availability.EndTime)
                        {
                            // Before
                            availability.EndTime = userShift.StartTime;
                            actualAvailabilities.Add(availability);
                        }
                        // User is available before and after the shift.
                        else if (userShift.StartTime >= availability.StartTime && userShift.EndTime < availability.EndTime)
                        {
                            // After
                            var extraAvailability = new Availability()
                            {
                                Date = availability.Date,
                                StartTime = userShift.EndTime,
                                EndTime = availability.EndTime,
                                Username = availability.Username
                            };
                            actualAvailabilities.Add(extraAvailability);

                            // Before
                            availability.EndTime = userShift.StartTime;
                            actualAvailabilities.Add(availability);
                        }
                        // User is not available
                        else if (userShift.StartTime < availability.StartTime && userShift.EndTime > availability.EndTime)
                        {
                        }
                        // User is available after the shift.
                        else if (userShift.StartTime < availability.StartTime && userShift.EndTime <= availability.EndTime)
                        {
                            availability.StartTime = userShift.EndTime;
                            actualAvailabilities.Add(availability);
                        }
                    }
                }
            }

            actualAvailabilities = actualAvailabilities.OrderBy(m => m.StartTime - m.EndTime).ToList();

            var json = JsonConvert.SerializeObject(actualAvailabilities);

            return Content(json);
        }

        // POST: Shifts/assignShift
        [HttpPost]
        public async Task<IActionResult> assignAvailability(int shiftId, int availabilityId)
        {
            try
            {
                var availability = await _availabilityContext.Availability.FindAsync(availabilityId);
                if (availability == null)
                {
                    return NotFound();
                }

                var shift = await _context.Shift.FindAsync(shiftId);
                if (shift == null)
                {
                    return NotFound();
                }

                if (shift.Date != availability.Date)
                {
                    return NotFound();
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
                } else
                {
                    return BadRequest();
                }

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

        public async Task<IActionResult> assignUser(int shiftId, string username)
        {
            try
            {
                var shift = await _context.Shift.FindAsync(shiftId);
                if (shift == null)
                {
                    return NotFound();
                }

                shift.Users = username;
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


        // POST: Shifts/unAssignShift
        [HttpPost]
        public async Task<IActionResult> unAssignShift(int id)
        {
            try
            {
                var shift = await _context.Shift.FindAsync(id);
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
                if (!ShiftExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
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
    }
}
