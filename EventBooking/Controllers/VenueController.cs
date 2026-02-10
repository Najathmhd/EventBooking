using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EventBooking.Data;
using EventBooking.Models;
using System.Linq;

namespace EventBooking.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VenueController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VenueController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Venue
        public IActionResult Index()
        {
            var venues = _context.Venues.ToList();
            return View(venues);
        }

        // GET: /Venue/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Venue/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Venue venue)
        {
            if (ModelState.IsValid)
            {
                _context.Venues.Add(venue);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(venue);
        }

        // GET: /Venue/Edit/5
        public IActionResult Edit(int id)
        {
            var venue = _context.Venues.Find(id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // POST: /Venue/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Venue venue)
        {
            if (ModelState.IsValid)
            {
                _context.Venues.Update(venue);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(venue);
        }

        // GET: /Venue/Delete/5
        public IActionResult Delete(int id)
        {
            var venue = _context.Venues.Find(id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // POST: /Venue/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var venue = _context.Venues.Find(id);
            if (venue == null) return NotFound();

            bool isUsed = _context.Events.Any(e => e.VenueId == id);
            if (isUsed)
            {
                ModelState.AddModelError("", "Cannot delete venue because it is used by events.");
                return View(venue);
            }

            _context.Venues.Remove(venue);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
