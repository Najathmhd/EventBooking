using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventBooking.Data;
using EventBooking.Models;
using System.Linq;

namespace EventBooking.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Event
        public IActionResult Index()
        {
            var events = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Category)
                .ToList();

            return View(events);
        }

        // GET: /Event/Create
        [Authorize(Roles = "Admin,Organizer")]
        public IActionResult Create()
        {
            ViewBag.Venues = new SelectList(_context.Venues, "Id", "Name");
            ViewBag.Categories = new SelectList(_context.EventCategories, "Id", "Name");
            return View();
        }

        // POST: /Event/Create
        [HttpPost]
        [Authorize(Roles = "Admin,Organizer")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Event evt)
        {
            if (ModelState.IsValid)
            {
                evt.CreatedBy = User.Identity?.Name;

                _context.Events.Add(evt);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Venues = new SelectList(_context.Venues, "Id", "Name", evt.VenueId);
            ViewBag.Categories = new SelectList(_context.EventCategories, "Id", "Name", evt.CategoryId);
            return View(evt);
        }

        // GET: /Event/Details/5
        public IActionResult Details(int id)
        {
            var evt = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Category)
                .FirstOrDefault(e => e.Id == id);

            if (evt == null)
            {
                return NotFound();
            }

            return View(evt);
        }

        // GET: /Event/Edit/5
        [Authorize(Roles = "Admin,Organizer")]
        public IActionResult Edit(int id)
        {
            var evt = _context.Events.Find(id);
            if (evt == null)
            {
                return NotFound();
            }

            ViewBag.Venues = new SelectList(_context.Venues, "Id", "Name", evt.VenueId);
            ViewBag.Categories = new SelectList(_context.EventCategories, "Id", "Name", evt.CategoryId);

            return View(evt);
        }

        // POST: /Event/Edit
        [HttpPost]
        [Authorize(Roles = "Admin,Organizer")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Event evt)
        {
            if (ModelState.IsValid)
            {
                _context.Events.Update(evt);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Venues = new SelectList(_context.Venues, "Id", "Name", evt.VenueId);
            ViewBag.Categories = new SelectList(_context.EventCategories, "Id", "Name", evt.CategoryId);
            return View(evt);
        }

        // GET: /Event/Delete/5
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var evt = _context.Events.Find(id);
            if (evt == null)
            {
                return NotFound();
            }

            return View(evt);
        }

        // POST: /Event/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var evt = _context.Events.Find(id);
            if (evt != null)
            {
                _context.Events.Remove(evt);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
