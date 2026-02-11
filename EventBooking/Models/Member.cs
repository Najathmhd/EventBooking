using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventBooking.Models
{
    public class Member
    {
        [Key]
        public int MemberId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        // Interest Preferences (e.g. "Music, Theatre")
        public string? Preferences { get; set; }

        // Link to Identity User
        public string? UserId { get; set; }

        // Navigation
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
    }
}
