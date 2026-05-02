using System.ComponentModel.DataAnnotations;

namespace TravelMap.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Полето не може да бъде празно")]
        [MaxLength(200, ErrorMessage = "Името не може да бъде по-дълго от 200 символа.")]
        [Display(Name = "Име")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string? Description { get; set; } = null;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int DestinationId { get; set; }
        public Destination? Destination { get; set; }
    }
}