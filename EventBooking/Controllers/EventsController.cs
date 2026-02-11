using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventBooking.Data;
using EventBooking.Models;
using Microsoft.AspNetCore.Authorization;

namespace EventBooking.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index(string searchString, int? categoryId, int? venueId, DateTime? startDate, DateTime? endDate, decimal? minPrice, decimal? maxPrice)
        {
            var eventsQuery = _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                eventsQuery = eventsQuery.Where(e => e.Title.Contains(searchString) || e.Description.Contains(searchString));
            }

            if (categoryId.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.CategoryId == categoryId.Value);
            }

            if (venueId.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.VenueId == venueId.Value);
            }

            if (startDate.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.EventDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.EventDate <= endDate.Value);
            }

            if (minPrice.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.Price <= maxPrice.Value);
            }

            ViewData["CategoryId"] = new SelectList(_context.EventCategories, "Id", "Name", categoryId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "Id", "Name", venueId);
            ViewData["CurrentFilter"] = searchString;
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["MinPrice"] = minPrice;
            ViewData["MaxPrice"] = maxPrice;

            return View(await eventsQuery.ToListAsync());
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .Include(e => e.Bookings) // Include bookings to calculate capacity
                .Include(e => e.Reviews).ThenInclude(r => r.Member) // Include reviews
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (@event == null)
            {
                return NotFound();
            }

            // Calculate Availability
            int bookedCount = @event.Bookings.Sum(b => b.TicketQuantity);
            int remainingCapacity = @event.Capacity - bookedCount;
            
            ViewData["RemainingCapacity"] = remainingCapacity;
            ViewData["IsFull"] = remainingCapacity <= 0;

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.EventCategories, "Id", "Name");
            ViewData["VenueId"] = new SelectList(_context.Venues, "Id", "Name");
            return View();
        }

        [Authorize(Roles = "Admin,Organizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,EventDate,VenueId,CategoryId,Price,ImageUrl")] Event @event)
        {
            if (ModelState.IsValid)
            {
                @event.CreatedBy = User.Identity?.Name;
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.EventCategories, "Id", "Name", @event.CategoryId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "Id", "Name", @event.VenueId);
            return View(@event);
        }

        [Authorize(Roles = "Admin,Organizer")]
        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.EventCategories, "Id", "Name", @event.CategoryId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "Id", "Name", @event.VenueId);
            return View(@event);
        }

        [Authorize(Roles = "Admin,Organizer")]
        // POST: Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,EventDate,VenueId,CategoryId,Price,ImageUrl")] Event @event)
        {
            if (id != @event.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve existing event to keep its CreatedBy value
                    var existingEvent = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
                    if (existingEvent != null)
                    {
                        @event.CreatedBy = existingEvent.CreatedBy;
                    }

                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.EventCategories, "Id", "Name", @event.CategoryId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "Id", "Name", @event.VenueId);
            return View(@event);
        }

        [Authorize(Roles = "Admin")]
        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        [Authorize(Roles = "Admin")]
        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                _context.Events.Remove(@event);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
