using Microsoft.AspNetCore.Mvc;
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
    }
}
