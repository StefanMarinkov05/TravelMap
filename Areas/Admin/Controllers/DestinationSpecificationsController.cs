using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelMap;
using TravelMap.Models;

namespace AutoDesigner.Areas.Administrator.Controllers
{
    [Authorize(Roles = TravelMapContext.ADMIN)]
    [Area("Admin")]
    public class DestinationSpecificationsController : Controller
    {

        private readonly TravelMapContext _context;
        public DestinationSpecificationsController(TravelMapContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DestinationSpecifications
            {
                Countries = await _context.Countries
                    .OrderBy(b => b.Name)
                    .ToListAsync(),

                Categories = await _context.Categories
                    .OrderBy(ct => ct.Name)
                    .ToListAsync(),

                Tags = await _context.Tags
                    .OrderBy(m => m.Name)
                    .ToListAsync()
            };

            return View(model);
        }


    }
}
