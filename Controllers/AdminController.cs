using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityLostAndFound.Data;
using UniversityLostAndFound.Models;
using UniversityLostAndFound.ViewModels;

namespace UniversityLostAndFound.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalItemsCount = await _context.Items.CountAsync(),
                ActiveLostCount = await _context.Items.CountAsync(i => i.ItemType == ItemType.Lost && i.Status == ItemStatus.Reported),
                ActiveFoundCount = await _context.Items.CountAsync(i => i.ItemType == ItemType.Found && i.Status == ItemStatus.Reported),
                ResolvedCount = await _context.Items.CountAsync(i => i.Status == ItemStatus.Resolved || i.Status == ItemStatus.Returned),
                PendingClaimsCount = await _context.Claims.CountAsync(c => c.Status == ClaimStatus.Pending),
                PendingAdminClaims = await _context.Claims
                    .Include(c => c.Item)
                    .Include(c => c.ClaimerUser)
                    .Where(c => c.Status == ClaimStatus.Pending)
                    .OrderByDescending(c => c.ClaimDate)
                    .ToListAsync(),
                ReviewedAdminClaims = await _context.Claims
                    .Include(c => c.Item)
                        .ThenInclude(i => i!.User)
                    .Include(c => c.ClaimerUser)
                    .Where(c => c.Status != ClaimStatus.Pending)
                    .OrderByDescending(c => c.ResolvedDate ?? c.ClaimDate)
                    .Take(50)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // GET: /Admin/Categories
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .Include(c => c.Items)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        // POST: /Admin/AddCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Category '{category.Name}' added successfully!";
                return RedirectToAction(nameof(Categories));
            }

            var categories = await _context.Categories.Include(c => c.Items).ToListAsync();
            return View("Categories", categories);
        }

        // GET: /Admin/Locations
        public async Task<IActionResult> Locations()
        {
            var locations = await _context.Locations
                .Include(l => l.Items)
                .OrderBy(l => l.Name)
                .ToListAsync();

            return View(locations);
        }

        // POST: /Admin/AddLocation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLocation(Location location)
        {
            if (ModelState.IsValid)
            {
                _context.Locations.Add(location);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Location '{location.Name}' added successfully!";
                return RedirectToAction(nameof(Locations));
            }

            var locations = await _context.Locations.Include(l => l.Items).ToListAsync();
            return View("Locations", locations);
        }

        // GET: /Admin/UserItems
        public async Task<IActionResult> UserItems()
        {
            var items = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Location)
                .Include(i => i.User)
                .Include(i => i.Claims)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return View(items);
        }
    }
}
