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
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,StartTime,EndTime")] Availability availability)
        {
            if (ModelState.IsValid)
            {
                availability.Username = User.Identity.Name;
                _context.Add(availability);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(availability);
        }

        // GET: Availabilities/Edit/5
        public async Task<IActionResult> Edit(long? id)
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Date,StartTime,EndTime")] Availability availability)
        {
            if (id != availability.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    availability.Username = User.Identity.Name;
                    _context.Update(availability);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AvailabilityExists(availability.Id))
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
            return View(availability);
        }

        // GET: Availabilities/Delete/5
        public async Task<IActionResult> Delete(long? id)
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
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var availability = await _context.Availability.FindAsync(id);
            _context.Availability.Remove(availability);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AvailabilityExists(long id)
        {
            return _context.Availability.Any(e => e.Id == id);
        }
    }
}
