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
    public class ArticlesController : Controller
    {
        private readonly TravelMapContext _context;

        public ArticlesController(TravelMapContext context)
        {
            _context = context;
        }

        // GET: User/Articles
        public async Task<IActionResult> Index()
        {
            var articles = await _context.Articles
                .Select(a => new {
                    Article = a,
                    Destination = a.Destination,
                    FirstImage = a.Destination.Images.OrderBy(img => img.Id).FirstOrDefault(),
                    Country = a.Destination.Country
                })
                .ToListAsync();

            var result = articles.Select(x => {
                if (x.Destination != null && x.FirstImage != null)
                {
                    x.Destination.Images = new List<DestinationImage> { x.FirstImage };
                }
                    return x.Article;
            }).ToList();

            return View(result);
        }

        // GET: User/Articles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a => a.Destination)
                    .ThenInclude(d => d.Images.OrderBy(i => i.Id).Take(1))
                .Include(a => a.Destination.Country)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }
    }
}
