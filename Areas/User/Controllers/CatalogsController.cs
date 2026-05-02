using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TravelMap;
using TravelMap.Models;

namespace TravelMap.Areas.User.Controllers
{
    [Area("User")]
    [Authorize] // requires the user to be logged in
    public class CatalogsController : Controller
    {
        private readonly TravelMapContext _context;

        public CatalogsController(TravelMapContext context)
        {
            _context = context;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // GET: User/Catalogs
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var catalogs = _context.Catalogs
                .Where(c => c.UserId == userId)
                .Include(c => c.ApplicationUser)
                .Include(c => c.CatalogDestinations.OrderBy(cd => cd.DisplayOrder))
                    .ThenInclude(cd => cd.Destination)
                .OrderBy(c => c.Id);

            return View(await catalogs.ToListAsync());
        }

        // GET: User/Catalogs/Public
        [AllowAnonymous] // bypasses authorize
        public async Task<IActionResult> Public()
        {
            var publicCatalogs = _context.Catalogs
                .Where(c => c.IsPublic)
                .Include(c => c.ApplicationUser)
                .Include(c => c.CatalogDestinations.OrderBy(cd => cd.DisplayOrder))
                    .ThenInclude(cd => cd.Destination)
                .OrderBy(c => c.Id);

            return View(await publicCatalogs.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleDestination(int catalogId, int destinationId)
        {
            var userId = GetUserId();

            var catalog = await _context.Catalogs
                .FirstOrDefaultAsync(c => c.Id == catalogId && c.UserId == userId);

            if (catalog == null) return Forbid(); 

            var entry = await _context.CatalogDestinations
                .FirstOrDefaultAsync(cd => cd.CatalogId == catalogId && cd.DestinationId == destinationId);

            if (entry != null)
            {
                _context.CatalogDestinations.Remove(entry);
            }
            else
            {
                _context.CatalogDestinations.Add(new CatalogDestination
                {
                    CatalogId = catalogId,
                    DestinationId = destinationId
                });
            }

            await _context.SaveChangesAsync();
            return Redirect(Request.Headers["Referer"].ToString() ?? "/User/Destinations");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ReorderDestination(int catalogId, int destinationId, string direction)
        {
            var catalog = await _context.Catalogs
                .Include(c => c.CatalogDestinations)
                .FirstOrDefaultAsync(c => c.Id == catalogId);

            if (catalog == null) return NotFound();

            var items = catalog.CatalogDestinations.OrderBy(cd => cd.DisplayOrder).ToList();

            for (int i = 0; i < items.Count; i++)
            {
                items[i].DisplayOrder = i;
            }

            var currentItem = items.FirstOrDefault(cd => cd.DestinationId == destinationId);
            if (currentItem != null)
            {
                int index = items.IndexOf(currentItem);
                CatalogDestination targetItem = null;

                if (direction == "up" && index > 0) targetItem = items[index - 1];
                else if (direction == "down" && index < items.Count - 1) targetItem = items[index + 1];

                if (targetItem != null)
                {
                    int tempOrder = currentItem.DisplayOrder;
                    currentItem.DisplayOrder = targetItem.DisplayOrder;
                    targetItem.DisplayOrder = tempOrder;

                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Details), new { id = catalogId });
        }

        // GET: User/Catalogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = GetUserId();
            var catalog = await _context.Catalogs
                .Include(c => c.ApplicationUser)
                .Include(c => c.CatalogDestinations.OrderBy(o => o.DisplayOrder))
                    .ThenInclude(cd => cd.Destination.Images)
                .FirstOrDefaultAsync(m => m.Id == id && (m.UserId == userId || m.IsPublic));

            if (catalog == null) return NotFound();

            return View(catalog);
        }

        // GET: User/Catalogs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Catalogs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,IsPublic,Notes")] Catalog catalog)
        {
            catalog.UserId = GetUserId();

            ModelState.Remove("UserId");
            ModelState.Remove("ApplicationUser");

            if (ModelState.IsValid)
            {
                _context.Add(catalog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(catalog);
        }

        // GET: User/Catalogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = GetUserId();
            var catalog = await _context.Catalogs
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId); 

            if (catalog == null) return NotFound();

            return View(catalog);
        }

        // POST: User/Catalogs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IsPublic,Notes")] Catalog catalog)
        {
            if (id != catalog.Id) return NotFound();

            var userId = GetUserId();
            var existingCatalog = await _context.Catalogs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (existingCatalog == null) return Forbid();

            ModelState.Remove("UserId");
            ModelState.Remove("ApplicationUser");

            if (ModelState.IsValid)
            {
                catalog.UserId = userId;
                _context.Update(catalog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(catalog);
        }

        // GET: User/Catalogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = GetUserId();
            var catalog = await _context.Catalogs
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (catalog == null) return NotFound();

            return View(catalog);
        }

        // POST: User/Catalogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetUserId();
            var catalog = await _context.Catalogs
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (catalog != null)
            {
                _context.Catalogs.Remove(catalog);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}