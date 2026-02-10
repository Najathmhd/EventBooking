using System.ComponentModel.DataAnnotations;

namespace EventBooking.Models
{
    public class EventCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        // Navigation
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
