using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelMap.Models
{
    public class Catalog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? ApplicationUser { get; set; }

        [Required(ErrorMessage = "Полето не може да бъде празно")]
        [StringLength(100, ErrorMessage = "Името не може да бъде по-дълго от 100 символа.")]
        [Display(Name = "Име")]
        public string? Name { get; set; }

        [Display(Name = "Публичен ли е каталога?")]
        public bool IsPublic { get; set; }

        [Display(Name = "Бележки")]
        public string? Notes { get; set; } = null;

        public List<CatalogDestination> CatalogDestinations { get; set; } = new List<CatalogDestination>();

        }
}
