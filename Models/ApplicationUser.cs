using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TravelMap.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Полето не може да бъде празно")]
        [MaxLength(100, ErrorMessage = "Името не може да бъде по-дълго от 100 символа.")]
        [Display(Name = "Първо име")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Полето не може да бъде празно")]
        [MaxLength(100, ErrorMessage = "Името не може да бъде по-дълго от 100 символа.")]
        [Display(Name = "Фамилия")]
        public required string LastName { get; set; }
        public List<Catalog> Catalogs { get; set; } = new List<Catalog>();
        public List<Destination> Destinations { get; set; } = new List<Destination>();
    }
}
