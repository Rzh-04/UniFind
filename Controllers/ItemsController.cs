using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniversityLostAndFound.Data;
using UniversityLostAndFound.Models;
using UniversityLostAndFound.Services;
using UniversityLostAndFound.ViewModels;

namespace UniversityLostAndFound.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileUploadService _fileUploadService;

        public ItemsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IFileUploadService fileUploadService)
        {
            _context = context;
            _userManager = userManager;
            _fileUploadService = fileUploadService;
        }

        // GET: /Items
        public async Task<IActionResult> Index(
            string? searchTerm,
            ItemType? itemType,
            int? categoryId,
            int? locationId,
            ItemStatus? status,
            string sortOrder = "newest",
            bool myReports = false)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (myReports && currentUserId == null)
            {
                return Challenge();
            }

            var query = _context.Items
                .Include(i => i.Category)
                .Include(i => i.Location)
                .Include(i => i.User)
                .AsQueryable();

            if (myReports)
            {
                query = query.Where(i => i.UserId == currentUserId);
            }

            // Search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(i => i.Title.ToLower().Contains(term) || i.Description.ToLower().Contains(term));
            }

            // Item type filter (Lost vs Found)
            if (itemType.HasValue)
            {
                query = query.Where(i => i.ItemType == itemType.Value);
            }

            // Category filter
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(i => i.CategoryId == categoryId.Value);
            }

            // Location filter
            if (locationId.HasValue && locationId.Value > 0)
            {
                query = query.Where(i => i.LocationId == locationId.Value);
            }

            // Status filter (default to Reported if unspecified, or allow all)
            if (status.HasValue)
            {
                query = query.Where(i => i.Status == status.Value);
            }
            else
            {
                if (!myReports)
                {
                    query = query.Where(i => i.Status == ItemStatus.Reported);
                }
            }

            // Sorting
            query = sortOrder switch
            {
                "oldest" => query.OrderBy(i => i.CreatedAt),
                "date_asc" => query.OrderBy(i => i.DateLostOrFound),
                "date_desc" => query.OrderByDescending(i => i.DateLostOrFound),
                _ => query.OrderByDescending(i => i.CreatedAt)
            };

            var itemsList = await query.ToListAsync();

            var viewModel = new ItemFilterViewModel
            {
                SearchTerm = searchTerm,
                ItemType = itemType,
                CategoryId = categoryId,
                LocationId = locationId,
                Status = myReports ? status : status ?? ItemStatus.Reported,
                SortOrder = sortOrder,
                Items = itemsList,
                Categories = await _context.Categories.ToListAsync(),
                Locations = await _context.Locations.ToListAsync(),
                TotalCount = itemsList.Count,
                CurrentUserId = currentUserId,
                ShowingMyReports = myReports
            };

            return View(viewModel);
        }

        // GET: /Items/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Location)
                .Include(i => i.User)
                .Include(i => i.Claims)
                    .ThenInclude(c => c.ClaimerUser)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            ViewData["IsOwnerOrAdmin"] = currentUserId != null && (currentUserId == item.UserId || User.IsInRole("Admin"));
            ViewData["HasAlreadyClaimed"] = currentUserId != null && item.Claims.Any(c => c.ClaimerUserId == currentUserId);

            return View(item);
        }

        // GET: /Items/Create
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create(ItemType type = ItemType.Lost)
        {
            await PopulateDropdownsAsync();
            var currentUser = await _userManager.GetUserAsync(User);

            var model = new ItemCreateViewModel
            {
                ItemType = type,
                DateLostOrFound = DateTime.Today,
                ContactEmail = currentUser?.Email,
                ContactPhone = currentUser?.PhoneNumber
            };

            return View(model);
        }

        // POST: /Items/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemCreateViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            if (ModelState.IsValid)
            {
                string? imagePath = null;
                if (model.ImageFile != null)
                {
                    try
                    {
                        imagePath = await _fileUploadService.UploadImageAsync(model.ImageFile, "items");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("ImageFile", ex.Message);
                        await PopulateDropdownsAsync();
                        return View(model);
                    }
                }

                var item = new Item
                {
                    Title = model.Title,
                    Description = model.Description,
                    ItemType = model.ItemType,
                    CategoryId = model.CategoryId,
                    LocationId = model.LocationId,
                    DateLostOrFound = model.DateLostOrFound,
                    ImagePath = imagePath,
                    Status = ItemStatus.Reported,
                    ContactPhone = model.ContactPhone,
                    ContactEmail = model.ContactEmail ?? currentUser.Email,
                    RewardDetails = model.RewardDetails,
                    UserId = currentUser.Id,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Items.Add(item);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Your {(model.ItemType == ItemType.Lost ? "Lost" : "Found")} item report has been published successfully!";
                return RedirectToAction(nameof(Details), new { id = item.Id });
            }

            await PopulateDropdownsAsync();
            return View(model);
        }

        // GET: /Items/Edit/5
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (item.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            await PopulateDropdownsAsync();

            var model = new ItemCreateViewModel
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                ItemType = item.ItemType,
                CategoryId = item.CategoryId,
                LocationId = item.LocationId,
                DateLostOrFound = item.DateLostOrFound,
                ExistingImagePath = item.ImagePath,
                ContactPhone = item.ContactPhone,
                ContactEmail = item.ContactEmail,
                RewardDetails = item.RewardDetails
            };

            return View(model);
        }

        // POST: /Items/Edit/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemCreateViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (item.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                if (model.ImageFile != null)
                {
                    try
                    {
                        var newPath = await _fileUploadService.UploadImageAsync(model.ImageFile, "items");
                        if (!string.IsNullOrEmpty(item.ImagePath))
                        {
                            _fileUploadService.DeleteImage(item.ImagePath);
                        }
                        item.ImagePath = newPath;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("ImageFile", ex.Message);
                        await PopulateDropdownsAsync();
                        return View(model);
                    }
                }

                item.Title = model.Title;
                item.Description = model.Description;
                item.ItemType = model.ItemType;
                item.CategoryId = model.CategoryId;
                item.LocationId = model.LocationId;
                item.DateLostOrFound = model.DateLostOrFound;
                item.ContactPhone = model.ContactPhone;
                item.ContactEmail = model.ContactEmail;
                item.RewardDetails = model.RewardDetails;

                _context.Update(item);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Item updated successfully!";
                return RedirectToAction(nameof(Details), new { id = item.Id });
            }

            await PopulateDropdownsAsync();
            return View(model);
        }

        // POST: /Items/Delete/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (item.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (!string.IsNullOrEmpty(item.ImagePath))
            {
                _fileUploadService.DeleteImage(item.ImagePath);
            }

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Post deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Items/MarkResolved/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkResolved(int id, ItemStatus targetStatus = ItemStatus.Resolved)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (item.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            item.Status = targetStatus;
            _context.Update(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Item marked as {targetStatus}.";
            return RedirectToAction(nameof(Details), new { id = item.Id });
        }

        private async Task PopulateDropdownsAsync()
        {
            ViewBag.Categories = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewBag.Locations = new SelectList(await _context.Locations.OrderBy(l => l.Name).ToListAsync(), "Id", "Name");
        }
    }
}
