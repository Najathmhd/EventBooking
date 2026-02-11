using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EventBooking.Data;
using EventBooking.Models;
using System.Security.Claims;
using System.Linq;

namespace EventBooking.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Booking
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);

            if (member == null && !User.IsInRole("Admin"))
            {
                // Auto-create member profile if it doesn't exist (e.g. new registration)
                member = new Member
                {
                    FullName = User.Identity?.Name?.Split('@')[0] ?? "New Citizen",
                    Email = User.Identity?.Name ?? "unknown@eventbooking.com",
                    PhoneNumber = "0000000000",
                    UserId = userId ?? string.Empty
                };
                
                _context.Members.Add(member);
                await _context.SaveChangesAsync();
                
                // Re-fetch
                member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
            }

            var bookingsQuery = _context.Bookings
                .Include(b => b.Event)
                .ThenInclude(e => e!.Venue)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                if (member == null) return NotFound("Member profile not found.");
                bookingsQuery = bookingsQuery.Where(b => b.MemberId == member.MemberId);
            }

            return View(await bookingsQuery.ToListAsync());
        }

        // POST: /Booking/Book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int eventId, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);

            if (member == null)
            {
                // Auto-create member profile for authenticated users
                member = new Member
                {
                    FullName = User.Identity?.Name?.Split('@')[0] ?? "New Citizen",
                    Email = User.Identity?.Name ?? "unknown@eventbooking.com",
                    PhoneNumber = "0000000000",
                    UserId = userId ?? string.Empty
                };
                
                _context.Members.Add(member);
                await _context.SaveChangesAsync();
                
                // Re-fetch to ensure we have the ID
                member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
            }

            if (member == null)
            {
                TempData["Error"] = "Metropolitan synchronization failed. Please try again.";
                return RedirectToAction("Index", "Home");
            }

            var @event = await _context.Events
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(e => e.Id == eventId);
            
            if (@event == null) return NotFound();

            if (@event.EventDate < DateTime.Now)
            {
                TempData["Error"] = "Cannot book past events.";
                return RedirectToAction("Details", "Events", new { id = eventId });
            }

            // Check capacity (Strict Enforcement)
            int bookedSeats = @event.Bookings.Sum(b => b.TicketQuantity);
            int remainingSeats = @event.Capacity - bookedSeats;

            if (quantity > remainingSeats)
            {
                TempData["Error"] = $"Only {remainingSeats} seats available. Please reduce your ticket quantity.";
                return RedirectToAction("Details", "Events", new { id = eventId });
            }

            if (remainingSeats <= 0)
            {
                TempData["Error"] = "This event is fully booked.";
                return RedirectToAction("Details", "Events", new { id = eventId });
            }

            var booking = new Booking
            {
                EventId = eventId,
                MemberId = member.MemberId,
                BookingDate = DateTime.Now,
                TicketQuantity = quantity,
                TotalPrice = @event.Price * quantity
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking successful!";
            return RedirectToAction(nameof(Index));
        }
        // POST: /Booking/VerifyBooking
        [HttpPost]
        [Authorize(Roles = "Admin,Organizer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            booking.IsVerified = true;
            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking verification confirmed.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Booking/Ticket/5
        public async Task<IActionResult> Ticket(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);

            if (member == null && !User.IsInRole("Admin"))
            {
                return NotFound("Member profile not found.");
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                    .ThenInclude(e => e!.Venue)
                .Include(b => b.Event)
                    .ThenInclude(e => e!.Category)
                .Include(b => b.Member)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();

            // Security check: only owner or admin can see the ticket
            if (!User.IsInRole("Admin") && booking.MemberId != member?.MemberId)
            {
                return Forbid();
            }

            return View(booking);
        }

        // GET: /Booking/Verify?token=GUID or /Booking/Verify/5 (Legacy)
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> Verify(string? token, int? id)
        {
            Booking? booking = null;

            if (!string.IsNullOrEmpty(token) && Guid.TryParse(token, out var ticketCode))
            {
                // Secure Lookup by GUID
                booking = await _context.Bookings
                    .Include(b => b.Event)
                        .ThenInclude(e => e!.Venue)
                    .Include(b => b.Event)
                        .ThenInclude(e => e!.Category)
                    .Include(b => b.Member)
                    .FirstOrDefaultAsync(b => b.TicketCode == ticketCode);
            }
            else if (id.HasValue)
            {
                // Legacy Lookup by ID
                booking = await _context.Bookings
                    .Include(b => b.Event)
                        .ThenInclude(e => e!.Venue)
                    .Include(b => b.Event)
                        .ThenInclude(e => e!.Category)
                    .Include(b => b.Member)
                    .FirstOrDefaultAsync(b => b.BookingId == id);
            }

            if (booking == null)
            {
                ViewBag.Status = "Invalid";
                ViewBag.Message = "Metropolitan registry record not found. This pass may be counterfeit or revoked.";
                return View("Verify", null);
            }

            // Simple verification logic: If event is in the past, it's expired
            if (booking.Event?.EventDate < DateTime.Now.Date)
            {
                ViewBag.Status = "Expired";
                ViewBag.Message = "This access pass has expired. The cultural event has already concluded.";
            }
            else if (!booking.IsVerified)
            {
                ViewBag.Status = "Pending";
                ViewBag.Message = "Access Denied. This reservation has not yet been verified by the Metropolitan Administration.";
            }
            else
            {
                ViewBag.Status = "Valid";
                ViewBag.Message = "Identity and reservation verified. Access granted to the metropolitan venue.";
            }

            return View(booking);
        }
    }
}
