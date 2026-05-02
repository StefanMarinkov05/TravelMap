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
    public class DestinationsController : Controller
    {
        private readonly TravelMapContext _context;

        public DestinationsController(TravelMapContext context)
        {
            _context = context;
        }

        // GET: User/Destinations
        public async Task<IActionResult> Index(string sortOrder)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CountrySortParm"] = sortOrder == "Country" ? "country_desc" : "Country";
            ViewData["PopSortParm"] = sortOrder == "Popularity" ? "pop_desc" : "Popularity";

            var anonQuery = _context.Destinations
                .Select(d => new
                {
                    Destination = d,
                    PopCount = d.CatalogDestinations.Count(),
                    ImgCount = d.Images.Count()
                });

            anonQuery = sortOrder switch
            {
                "name_desc" => anonQuery.OrderByDescending(a => a.Destination.Name),
                "Date" => anonQuery.OrderBy(a => a.Destination.CreationDate),
                "date_desc" => anonQuery.OrderByDescending(a => a.Destination.CreationDate),
                "Country" => anonQuery.OrderBy(a => a.Destination.Country.Name),
                "country_desc" => anonQuery.OrderByDescending(a => a.Destination.Country.Name),
                "Popularity" => anonQuery.OrderBy(a => a.PopCount),
                "pop_desc" => anonQuery.OrderByDescending(a => a.PopCount),
                _ => anonQuery.OrderByDescending(a => a.PopCount)
            };

            var results = await anonQuery
                .AsNoTracking()
                .Select(a => a.Destination)
                .Include(d => d.Category)
                .Include(d => d.Country)
                .Include(d => d.Images)
                .Include(d => d.Tags)
                .Include(d => d.Author)
                .Include(d => d.CatalogDestinations)
                .AsSplitQuery()
                .ToListAsync();

            return View(results);
        }

        // GET: User/Destinations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var destination = await _context.Destinations

                .Include(d => d.Author)
                .Include(d => d.Category)
                .Include(d => d.Country)
                .Include(d => d.Tags)
                .Include(d => d.Images)
                .Include(d => d.Articles)
                .Include(d => d.CatalogDestinations)
                .Include(d => d.Events.OrderBy(e => e.Date)) // order from first to latest
                .FirstOrDefaultAsync(m => m.Id == id);

            if (destination == null) return NotFound();

            return View(destination);
        }
    }
}