using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelMap.Models
{
    public class Destination
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Полето 'Име' е задължително.")]
        [StringLength(100,
            ErrorMessage = "Името не може да бъде по-дълго от 100 символа.")]
        [Display(Name = "Име на дестинацията")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Трябва да изберете държава.")]
        [Display(Name = "Държава")]
        public int CountryId { get; set; }

        [Required(ErrorMessage = "Полето 'Регион' е задължително.")]
        [StringLength(100,
            ErrorMessage = "Името не може да бъде по-дълго от 100 символа.")]
        [Display(Name = "Име на региона")]
        public required string Region { get; set; }


        [Display(Name = "Ключови думи")]
        public List<Tag>? Tags { get; set; } = new List<Tag>();


        [Required(ErrorMessage = "Трябва да изберете категория.")]
        [Display(Name = "Категория на дестинацията")]
        public int CategoryId { get; set; }

        [Display(Name = "Автор")]
        public string? AuthorId { get; set; } = null;


        [Display(Name = "Държава")]
        public Country? Country { get; set; }

        [Display(Name = "Категория")]
        public Category? Category { get; set; }

        [Display(Name = "Автор")]
        public ApplicationUser? Author { get; set; }


        [Display(Name = "Дата на добавяне")]
        public DateOnly CreationDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        [MaxLength(10000, ErrorMessage = "Описанито не може да бъде по-дълго от 10 000 символа.")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Популярност")]
        public int Popularity => CatalogDestinations?.Count ?? 0;
        public List<DestinationImage> Images { get; set; } = new List<DestinationImage>();

        [Display(Name = "Брой изображения")]
        public int ImageCount => Images?.Count ?? 0;


        [NotMapped]
        [Display(Name = "Качване на изображения")]
        [ValidateNever]
        public List<IFormFile>? UploadedImages { get; set; }
        public List<CatalogDestination> CatalogDestinations { get; set; } = new List<CatalogDestination>();
    
        public List<Event> Events { get; set; } = new List<Event>();
        public List<Article> Articles { get; set; } = new List<Article>();

    }
}
