using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityLostAndFound.Data;
using UniversityLostAndFound.Models;
using UniversityLostAndFound.ViewModels;

namespace UniversityLostAndFound.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /
        public async Task<IActionResult> Index()
        {
            var totalItems = await _context.Items.CountAsync();
            var lostCount = await _context.Items.CountAsync(i => i.ItemType == ItemType.Lost && i.Status == ItemStatus.Reported);
            var foundCount = await _context.Items.CountAsync(i => i.ItemType == ItemType.Found && i.Status == ItemStatus.Reported);
            var resolvedCount = await _context.Items.CountAsync(i => i.Status == ItemStatus.Resolved || i.Status == ItemStatus.Returned);

            var recentItems = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Location)
                .Where(i => i.Status == ItemStatus.Reported)
                .OrderByDescending(i => i.CreatedAt)
                .Take(6)
                .ToListAsync();

            var categories = await _context.Categories
                .Select(c => new
                {
                    Category = c,
                    ItemCount = c.Items.Count(i => i.Status == ItemStatus.Reported)
                })
                .ToListAsync();
            var locations = await _context.Locations
                .OrderBy(l => l.Name)
                .ToListAsync();

            ViewData["TotalItems"] = totalItems;
            ViewData["LostCount"] = lostCount;
            ViewData["FoundCount"] = foundCount;
            ViewData["ResolvedCount"] = resolvedCount;
            ViewData["RecentItems"] = recentItems;
            ViewData["Categories"] = categories.Select(x => x.Category).ToList();
            ViewData["Locations"] = locations;

            return View();
        }

        // GET: /Home/About
        public IActionResult About()
        {
            return View();
        }

        // GET: /Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
