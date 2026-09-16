using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityLostAndFound.Data;
using UniversityLostAndFound.Models;
using UniversityLostAndFound.ViewModels;

namespace UniversityLostAndFound.Controllers
{
    [Authorize]
    public class ClaimsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClaimsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Claims/Create?itemId=5
        [HttpGet]
        public async Task<IActionResult> Create(int itemId)
        {
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";

            var item = await _context.Items
                .Include(i => i.Location)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);

            // Prevent user from claiming their own post
            if (item.UserId == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot submit a claim for an item you posted yourself.";
                return RedirectToAction("Details", "Items", new { id = itemId });
            }

            // Check if user already submitted a claim
            var existingClaim = await _context.Claims
                .FirstOrDefaultAsync(c => c.ItemId == itemId && c.ClaimerUserId == currentUserId);

            if (existingClaim != null)
            {
                TempData["InfoMessage"] = "You have already submitted a response for this item.";
                return RedirectToAction("MyClaims");
            }

            var user = await _userManager.GetUserAsync(User);

            var model = new ClaimCreateViewModel
            {
                ItemId = item.Id,
                ItemType = item.ItemType,
                ItemTitle = item.Title,
                ItemDescription = item.Description,
                ItemImagePath = item.ImagePath,
                ItemLocationName = item.Location?.Name,
                ItemRewardDetails = item.RewardDetails,
                ContactPhoneNumber = user?.PhoneNumber ?? ""
            };

            return View(model);
        }

        // POST: /Claims/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClaimCreateViewModel model)
        {
            var item = await _context.Items.FindAsync(model.ItemId);
            if (item == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null)
            {
                return Challenge();
            }

            if (item.UserId == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot respond to your own item post.";
                return RedirectToAction("Details", "Items", new { id = item.Id });
            }

            model.ItemType = item.ItemType;

            if (item.ItemType == ItemType.Lost && string.IsNullOrWhiteSpace(model.CollectionLocation))
            {
                ModelState.AddModelError(nameof(model.CollectionLocation), "Please tell the owner where they can collect the item.");
            }

            if (ModelState.IsValid)
            {
                var claim = new Claim
                {
                    ItemId = model.ItemId,
                    ClaimerUserId = currentUserId,
                    ProofOfOwnership = model.ProofOfOwnership,
                    ContactPhoneNumber = model.ContactPhoneNumber,
                    Status = ClaimStatus.Pending,
                    ClaimDate = DateTime.UtcNow,
                    AdminOrFinderNotes = item.ItemType == ItemType.Lost
                        ? $"Suggested collection location: {model.CollectionLocation!.Trim()}"
                        : null
                };

                _context.Claims.Add(claim);

                // Optionally update item status to Under Review / Claimed
                item.Status = ItemStatus.Claimed;
                _context.Update(item);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = item.ItemType == ItemType.Lost
                    ? "Your found-item response has been submitted. The person who reported the loss will review your details."
                    : "Your ownership claim has been submitted successfully. The finder / Lost & Found office will review your details.";
                return RedirectToAction(nameof(MyClaims));
            }

            return View(model);
        }

        // GET: /Claims/MyClaims
        [HttpGet]
        public async Task<IActionResult> MyClaims()
        {
            var currentUserId = _userManager.GetUserId(User);
            var claims = await _context.Claims
                .Include(c => c.Item)
                    .ThenInclude(i => i!.Location)
                .Include(c => c.Item)
                    .ThenInclude(i => i!.Category)
                .Include(c => c.Item)
                    .ThenInclude(i => i!.User)
                .Where(c => c.ClaimerUserId == currentUserId)
                .OrderByDescending(c => c.ClaimDate)
                .ToListAsync();

            return View(claims);
        }

        // POST: /Claims/Review
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(int claimId, ClaimStatus status, string? reviewNotes)
        {
            var claim = await _context.Claims
                .Include(c => c.Item)
                .FirstOrDefaultAsync(c => c.Id == claimId);

            if (claim == null || claim.Item == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            var isPoster = claim.Item.UserId == currentUserId;
            var isAdmin = User.IsInRole("Admin");

            if (!isPoster && !isAdmin)
            {
                return Forbid();
            }

            claim.Status = status;
            if (!string.IsNullOrWhiteSpace(reviewNotes))
            {
                claim.AdminOrFinderNotes = string.IsNullOrWhiteSpace(claim.AdminOrFinderNotes)
                    ? reviewNotes.Trim()
                    : $"{claim.AdminOrFinderNotes}\n\nReviewer instructions: {reviewNotes.Trim()}";
            }
            claim.ResolvedDate = DateTime.UtcNow;

            if (status == ClaimStatus.Approved)
            {
                claim.Item.Status = ItemStatus.Resolved;
            }
            else if (status == ClaimStatus.Rejected)
            {
                // Revert item back to reported if no other pending claims exist
                var pendingOtherClaims = await _context.Claims
                    .AnyAsync(c => c.ItemId == claim.ItemId && c.Id != claim.Id && c.Status == ClaimStatus.Pending);

                if (!pendingOtherClaims)
                {
                    claim.Item.Status = ItemStatus.Reported;
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Claim decision updated to: {status}.";

            if (isAdmin)
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Details", "Items", new { id = claim.ItemId });
        }
    }
}
