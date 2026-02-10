using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EventBooking.Data;
using EventBooking.Models;
using System.Security.Claims;

namespace EventBooking.Controllers
{
    [Authorize(Roles = "Member,Admin")]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: /Review/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int eventId, int rating, string comment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);

            if (member == null) return RedirectToAction("Index", "Home");

            var @event = await _context.Events.FindAsync(eventId);
            if (@event == null) return NotFound();

            // Allow reviews only after attending events (event date must be in the past)
            if (@event.EventDate > DateTime.Now)
            {
                TempData["Error"] = "You can only review an event after it has taken place.";
                return RedirectToAction("Details", "Events", new { id = eventId });
            }

            // Check if they booked the event
            var hasBooked = await _context.Bookings.AnyAsync(b => b.EventId == eventId && b.MemberId == member.MemberId);
            if (!hasBooked && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "You must have a booking to review this event.";
                return RedirectToAction("Details", "Events", new { id = eventId });
            }

            var review = new Review
            {
                EventId = eventId,
                MemberId = member.MemberId,
                Rating = rating,
                Comment = comment,
                ReviewDate = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Review submitted!";
            return RedirectToAction("Details", "Events", new { id = eventId });
        }
    }
}
