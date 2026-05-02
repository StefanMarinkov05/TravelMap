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
    public class CatalogsController : Controller
    {
        private readonly TravelMapContext _context;

        public CatalogsController(TravelMapContext context)
        {
            _context = context;
        }

        // GET: Admin/Catalogs
        public async Task<IActionResult> Index()
        {
            var travelMapContext = _context.Catalogs.Include(c => c.ApplicationUser);
            return View(await travelMapContext.ToListAsync());
        }

        // GET: Admin/Catalogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var catalog = await _context.Catalogs
                .Include(c => c.ApplicationUser)
                .Include(c => c.CatalogDestinations.OrderBy(o => o.DisplayOrder))
                    .ThenInclude(d => d.Destination.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (catalog == null) return NotFound();

            return View(catalog);
        }

        [HttpPost]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveDestination(int catalogId, int destinationId)
        {
            var catalog = await _context.Catalogs
                .Include(c => c.CatalogDestinations)
                    .ThenInclude(d => d.Destination)
                .FirstOrDefaultAsync(c => c.Id == catalogId);

            if (catalog != null)
            {
                var destination = catalog.CatalogDestinations.FirstOrDefault(d => d.DestinationId == destinationId);
                if (destination != null)
                {
                    catalog.CatalogDestinations.Remove(destination);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Details), new { id = catalogId });
        }

        // GET: Admin/Catalogs/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id");
            return View();
        }

        // POST: Admin/Catalogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,Name,IsPublic,Notes")] Catalog catalog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(catalog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", catalog.UserId);
            return View(catalog);
        }

        // GET: Admin/Catalogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var catalog = await _context.Catalogs.FindAsync(id);
            if (catalog == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", catalog.UserId);
            return View(catalog);
        }

        // POST: Admin/Catalogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Name,IsPublic,Notes")] Catalog catalog)
        {
            if (id != catalog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(catalog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CatalogExists(catalog.Id))
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
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", catalog.UserId);
            return View(catalog);
        }

        // GET: Admin/Catalogs/Delete/5
        // GET: Admin/Catalogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var catalog = await _context.Catalogs
                .Include(c => c.ApplicationUser)
                .Include(c => c.CatalogDestinations)
                    .ThenInclude(d => d.Destination)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (catalog == null) return NotFound();

            return View(catalog);
        }

        // POST: Admin/Catalogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var catalog = await _context.Catalogs.FindAsync(id);
            if (catalog != null)
            {
                _context.Catalogs.Remove(catalog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CatalogExists(int id)
        {
            return _context.Catalogs.Any(e => e.Id == id);
        }
    }
}
