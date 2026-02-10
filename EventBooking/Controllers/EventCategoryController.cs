using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EventBooking.Data;
using EventBooking.Models;
using System.Linq;

namespace EventBooking.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EventCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /EventCategory
        public IActionResult Index()
        {
            var categories = _context.EventCategories.ToList();
            return View(categories);
        }

        // GET: /EventCategory/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /EventCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EventCategory category)
        {
            if (_context.EventCategories.Any(c => c.Name == category.Name))
            {
                ModelState.AddModelError("Name", "Category already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.EventCategories.Add(category);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: /EventCategory/Edit/5
        public IActionResult Edit(int id)
        {
            var category = _context.EventCategories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: /EventCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EventCategory category)
        {
            if (ModelState.IsValid)
            {
                _context.EventCategories.Update(category);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: /EventCategory/Delete/5
        public IActionResult Delete(int id)
        {
            var category = _context.EventCategories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: /EventCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var category = _context.EventCategories.Find(id);
            if (category == null) return NotFound();

            bool isUsed = _context.Events.Any(e => e.CategoryId == id);
            if (isUsed)
            {
                ModelState.AddModelError("", "Cannot delete category because it is used by events.");
                return View(category);
            }

            _context.EventCategories.Remove(category);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
