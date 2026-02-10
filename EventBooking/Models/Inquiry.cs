using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventBooking.Models
{
    public class Inquiry
    {
        [Key]
        public int InquiryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(300)]
        public string Message { get; set; }

        public DateTime InquiryDate { get; set; }

        // Optional FK (guest or member)
        public int? MemberId { get; set; }

        [ForeignKey("MemberId")]
        public Member Member { get; set; }
    }
}
