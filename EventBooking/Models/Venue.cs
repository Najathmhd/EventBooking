using System.ComponentModel.DataAnnotations;

namespace EventBooking.Models
{
    public class Venue
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Address { get; set; }

        // Navigation
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
