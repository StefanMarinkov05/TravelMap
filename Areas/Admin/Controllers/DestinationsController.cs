using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TravelMap;
using TravelMap.Models;

namespace TravelMap.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DestinationsController : Controller
    {
        private readonly TravelMapContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public DestinationsController(TravelMapContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }
        // GET: Admin/Destinations
        public async Task<IActionResult> Index()
        {
            var travelMapContext = _context.Destinations
                .AsNoTracking()
                .Include(d => d.Author)
                .Include(d => d.Category)
                .Include(d => d.Country)
                .Include(d => d.Tags)
                .Include(d => d.Images) 
                .Include(d => d.CatalogDestinations)
                .AsSplitQuery();

            return View(await travelMapContext.OrderByDescending(d => d.CreationDate).ToListAsync());
        }

        // GET: Admin/Destinations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var destination = await _context.Destinations
                .AsNoTracking()
                .Include(d => d.Author)
                .Include(d => d.Category)
                .Include(d => d.Country)
                .Include(d => d.Tags)
                .Include(d => d.Images)
                .Include(d => d.CatalogDestinations)
                .AsSplitQuery()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (destination == null)
            {
                return NotFound();
            }

            return View(destination);
        }

        // GET: Admin/Destinations/Create
        public IActionResult Create()
        {
            ViewData["Tags"] = new SelectList(_context.Tags, "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Name");
            return View();
        }

        // POST: Admin/Destinations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Destination destination, List<int> selectedTags, IFormFileCollection images)
        {
            destination.AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            destination.CreationDate = DateOnly.FromDateTime(DateTime.UtcNow);

            if (ModelState.IsValid)
            {
                if (selectedTags != null)
                {
                    destination.Tags = await _context.Tags.Where(t => selectedTags.Contains(t.Id)).ToListAsync();
                }

                if (images != null && images.Count > 0)
                {
                    destination.Images = new List<DestinationImage>();

                    string uploadDir = Path.Combine(_hostEnvironment.WebRootPath, "images/destinations");
                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    destination.Images = new List<DestinationImage>();

                    foreach (var file in images)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string filePath = Path.Combine(uploadDir, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        destination.Images.Add(new DestinationImage { ImageUrl = "/images/destinations/" + fileName });
                    }
                }

                _context.Add(destination);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", destination.CategoryId);
            ViewData["CountryId"] = new SelectList(_context.Countries, "Id", "Name", destination.CountryId);
            ViewData["Tags"] = new SelectList(_context.Tags, "Id", "Name");
            return View(destination);
        }
        // GET: Admin/Destinations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var destination = await _context.Destinations
                .Include(d => d.Images)
                .Include(d => d.Tags)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (destination == null) return NotFound();

            ViewBag.Tags = new SelectList(_context.Tags, "Id", "Name");
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", destination.CategoryId);
            ViewBag.CountryId = new SelectList(_context.Countries, "Id", "Name", destination.CountryId);

            ViewData["Tags"] = new SelectList(_context.Tags, "Id", "Name");

            ViewBag.SelectedTagIds = destination.Tags.Select(t => t.Id).ToList();

            return View(destination);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Destination destination, List<int> selectedTags, IFormFileCollection newImages, List<int> deletedImageIds)
        {
            if (id != destination.Id) return NotFound();

            ModelState.Remove("Images");
            ModelState.Remove("Tags");
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("Catalogs");

            if (ModelState.IsValid)
            {
                try
                {
                    var destinationToUpdate = await _context.Destinations
                        .Include(d => d.Tags)
                        .Include(d => d.Images)
                        .FirstOrDefaultAsync(d => d.Id == id);

                    if (destinationToUpdate == null) return NotFound();

                    if (!deletedImageIds.IsNullOrEmpty())
                    {
                        var imagesToRemove = destinationToUpdate.Images
                            .Where(img => deletedImageIds.Contains(img.Id)).ToList();

                        foreach (var img in imagesToRemove)
                        {
                            var filePath = Path.Combine(_hostEnvironment.WebRootPath, img.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                            _context.DestinationImages.Remove(img);
                        }
                    }

                    if (newImages != null && newImages.Count > 0)
                    {
                        string uploadDir = Path.Combine(_hostEnvironment.WebRootPath, "images/destinations");
                        if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                        foreach (var file in newImages)
                        {
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            string filePath = Path.Combine(uploadDir, fileName);
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }
                            destinationToUpdate.Images.Add(new DestinationImage { ImageUrl = "/images/destinations/" + fileName });
                        }
                    }

                    destinationToUpdate.Tags.Clear();
                    if (selectedTags != null)
                    {
                        var tags = await _context.Tags.Where(t => selectedTags.Contains(t.Id)).ToListAsync();
                        foreach (var tag in tags) destinationToUpdate.Tags.Add(tag);
                    }

                    destinationToUpdate.Name = destination.Name;
                    destinationToUpdate.Region = destination.Region;
                    destinationToUpdate.CountryId = destination.CountryId;
                    destinationToUpdate.CategoryId = destination.CategoryId;
                    destinationToUpdate.Description = destination.Description;

                    _context.Entry(destinationToUpdate).CurrentValues.SetValues(destination);

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DestinationExists(destination.Id)) return NotFound();
                    else throw;
                }
            }

            ViewBag.Tags = new SelectList(_context.Tags, "Id", "Name");
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", destination.CategoryId);
            ViewBag.CountryId = new SelectList(_context.Countries, "Id", "Name", destination.CountryId);

            destination.Images = await _context.DestinationImages.Where(i => i.DestinationId == id).ToListAsync();

            return View(destination);
        }

        // GET: Admin/Destinations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var destination = await _context.Destinations
                .AsNoTracking()
                .Include(d => d.Author)
                .Include(d => d.Category)
                .Include(d => d.Country)
                .Include(d => d.Tags)
                .Include(d => d.Images)
                .Include(d => d.CatalogDestinations)
                .AsSplitQuery()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (destination == null)
            {
                return NotFound();
            }

            return View(destination);
        }

        // POST: Admin/Destinations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var destination = await _context.Destinations
                .Include(d => d.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (destination != null)
            {
                if (destination.Images != null)
                {
                    foreach (var img in destination.Images)
                    {
                        if (!string.IsNullOrEmpty(img.ImageUrl))
                        {
                            var relativePath = img.ImageUrl.TrimStart('/');
                            var fullPath = Path.Combine(_hostEnvironment.WebRootPath, relativePath);

                            if (System.IO.File.Exists(fullPath))
                            {
                                try
                                {
                                    System.IO.File.Delete(fullPath);
                                }
                                catch (IOException ex)
                                {
                                }
                            }
                        }
                    }
                }

                _context.Destinations.Remove(destination);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DestinationExists(int id)
        {
            return _context.Destinations.Any(e => e.Id == id);
        }
    }
}
