using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TravelMap;
using TravelMap.Models;

namespace TravelMap.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EventsController : Controller
    {
        private readonly TravelMapContext _context;

        public EventsController(TravelMapContext context)
        {
            _context = context;
        }

        // GET: Admin/Events
        public async Task<IActionResult> Index()
        {
            var travelMapContext = _context.Events.Include(e => e.Destination);
            return View(await travelMapContext.ToListAsync());
        }

        // GET: Admin/Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eevent = await _context.Events
                .Include(e => e.Destination)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eevent == null)
            {
                return NotFound();
            }

            return View(eevent);
        }

        // GET: Admin/Events/Create
        public IActionResult Create()
        {
            ViewData["DestinationId"] = new SelectList(_context.Destinations, "Id", "Name");
            return View();
        }

        // POST: Admin/Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Date,DestinationId")] Event eevent)
        {
            if (ModelState.IsValid)
            {
                _context.Add(eevent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DestinationId"] = new SelectList(_context.Destinations, "Id", "Name", eevent.DestinationId);
            return View(eevent);
        }

        // GET: Admin/Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eevent = await _context.Events.FindAsync(id);
            if (eevent == null)
            {
                return NotFound();
            }
            ViewData["DestinationId"] = new SelectList(_context.Destinations, "Id", "Name", eevent.DestinationId);
            return View(eevent);
        }

        // POST: Admin/Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Date,DestinationId")] Event eevent)
        {
            if (id != eevent.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eevent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eevent.Id))
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
            ViewData["DestinationId"] = new SelectList(_context.Destinations, "Id", "Name", eevent.DestinationId);
            return View(eevent);
        }

        // GET: Admin/Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eevent = await _context.Events
                .Include(e => e.Destination)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eevent == null)
            {
                return NotFound();
            }

            return View(eevent);
        }

        // POST: Admin/Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eevent = await _context.Events.FindAsync(id);
            if (eevent != null)
            {
                _context.Events.Remove(eevent);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
