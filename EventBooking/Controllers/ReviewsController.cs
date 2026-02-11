using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventBooking.Data;
using EventBooking.Models;
using System.Security.Claims;

namespace EventBooking.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: Reviews/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int eventId, string comment, int rating)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);

            if (member == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Verify member attended event (or at least booked it)
            // Ideally check if event is in the past, but for now just check booking
            var hasBooked = await _context.Bookings.AnyAsync(b => b.EventId == eventId && b.MemberId == member.MemberId);
            if (!hasBooked)
            {
                TempData["Error"] = "You can only review events you have reserved.";
                return RedirectToAction("Details", "Events", new { id = eventId });
            }

            // Prevent duplicate reviews
            if (await _context.Reviews.AnyAsync(r => r.EventId == eventId && r.MemberId == member.MemberId))
            {
                TempData["Error"] = "You have already reviewed this event.";
                return RedirectToAction("Details", "Events", new { id = eventId });
            }

            var review = new Review
            {
                EventId = eventId,
                MemberId = member.MemberId,
                Comment = comment,
                Rating = rating,
                ReviewDate = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thank you for your feedback!";
            return RedirectToAction("Details", "Events", new { id = eventId });
        }
    }
}
