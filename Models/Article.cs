using System.ComponentModel.DataAnnotations;

namespace TravelMap.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Полето не може да бъде празно")]
        [MaxLength(200, ErrorMessage = "Заглавието не може да бъде по-дълго от 200 символа.")]
        [Display(Name = "Заглавие")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Полето не може да бъде празно")]
        [Display(Name = "Съдържание")]
        public string Content { get; set; }

        [Display(Name = "Описание")]
        public string? Description { get; set; } = null;

        [Required]
        public int DestinationId { get; set; }
        public Destination? Destination { get; set; }
    }
}
