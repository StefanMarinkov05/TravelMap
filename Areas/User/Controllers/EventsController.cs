using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TravelMap;
using TravelMap.Models;

namespace TravelMap.Areas.User.Controllers
{
    [Area("User")]
    public class EventsController : Controller
    {
        private readonly TravelMapContext _context;

        public EventsController(TravelMapContext context)
        {
            _context = context;
        }

        // GET: User/Events
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Include(e => e.Destination)
                    .ThenInclude(d => d.Images)
                .Include(e => e.Destination.Country)
                .OrderBy(e => e.Date)
                .ToListAsync();

            return View(events);
        }

        // GET: User/Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eevent = await _context.Events
                .Include(e => e.Destination)
                    .ThenInclude(d => d.Images)
                .Include(e => e.Destination.Country)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (eevent == null)
            {
                return NotFound();
            }

            return View(eevent);
        }
    }
}