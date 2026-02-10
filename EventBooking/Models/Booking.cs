using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventBooking.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        public int TicketQuantity { get; set; }

        // 🔗 Foreign Keys
        [Required]
        public int MemberId { get; set; }

        [Required]
        public int EventId { get; set; }

        // 🔗 Navigation
        [ForeignKey("MemberId")]
        public Member? Member { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }
    }
}
