using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventBooking.Data;
using EventBooking.Models;
using System.Security.Claims;

namespace EventBooking.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var member = await _context.Members
                .Include(m => m.Bookings)
                .ThenInclude(b => b.Event)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (member == null) return RedirectToAction("Index", "Home");
            return View(member);
        }

        // GET: /Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);

            if (member == null) return RedirectToAction("Index", "Home");
            return View(member);
        }

        // POST: /Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("MemberId,FullName,Email,PhoneNumber,Preferences,UserId")] Member member)
        {
            // Verify ownership
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (member.UserId != userId) return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(member);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Profile updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Members.Any(e => e.MemberId == member.MemberId))
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
            return View(member);
        }
    }
}
