using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventBooking.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        // 🔹 Venue relationship
        [Required]
        public int VenueId { get; set; }

        [ForeignKey("VenueId")]
        public Venue? Venue { get; set; }

        // 🔹 Category relationship
        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public EventCategory? Category { get; set; }

        // 🔹 Auth user who created event
        public string? CreatedBy { get; set; }
    }
}
