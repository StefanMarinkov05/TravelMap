using System.ComponentModel.DataAnnotations;

namespace TravelMap.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Полето не може да бъде празно")]
        [MaxLength(100, ErrorMessage = "Името не може да бъде по-дълго от 100 символа.")]
        [Display(Name = "Име")]
        public string Name { get; set; }

        public List<Destination> Destinations { get; set; } = new List<Destination>();
    }
}
