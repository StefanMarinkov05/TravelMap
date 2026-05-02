using System.ComponentModel.DataAnnotations;

namespace TravelMap.Models
{
    public class DestinationImage
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Полето не може да бъде празно")]
        [Display(Name = "URL на изображението")]
        public string ImageUrl { get; set; }

        [Required]
        public int DestinationId { get; set; }
        public Destination? Destination { get; set; }

          
    }
}