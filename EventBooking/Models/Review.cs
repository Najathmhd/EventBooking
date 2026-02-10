using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventBooking.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        [StringLength(300)]
        public string Comment { get; set; } = string.Empty;

        [Required]
        public int Rating { get; set; } // 1 to 5

        public DateTime ReviewDate { get; set; }

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
