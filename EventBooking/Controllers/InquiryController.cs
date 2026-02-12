using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using EventBooking.Data;
using EventBooking.Models;

namespace EventBooking.Controllers
{
    public class InquiryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InquiryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: /Inquiry/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Inquiry inquiry)
        {
            if (ModelState.IsValid)
            {
                inquiry.InquiryDate = DateTime.Now;
                _context.Inquiries.Add(inquiry);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Your inquiry has been sent. We will contact you soon.";
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Please fill in all required fields.";
            return RedirectToAction("Index", "Home");
        }
        // GET: /Inquiry (Admin Only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var inquiries = await _context.Inquiries
                .OrderByDescending(i => i.InquiryDate)
                .ToListAsync();
            return View(inquiries);
        }

        // POST: /Inquiry/MarkResolved/5
        // POST: /Inquiry/MarkResolved/5
        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> MarkResolved(int id)
        //{
        //    var inquiry = await _context.Inquiries.FindAsync(id);
        //    if (inquiry == null) return NotFound();

        //    inquiry.IsResolved = !inquiry.IsResolved; // Toggle status
        //    _context.Update(inquiry);
        //    await _context.SaveChangesAsync();

        //    TempData["Success"] = inquiry.IsResolved ? "Inquiry marked as resolved." : "Inquiry reopened.";
        //    return RedirectToAction(nameof(Index));
        //}
    }
}
