using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection.Emit;
using TravelMap.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TravelMap;

public class TravelMapContext : IdentityDbContext<IdentityUser>
{
    public TravelMapContext(DbContextOptions<TravelMapContext> options)
        : base(options)
    {
    }

    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Catalog> Catalogs { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Destination> Destinations { get; set; }
    public DbSet<DestinationImage> DestinationImages { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<CatalogDestination> CatalogDestinations { get; set; }

    public const string ADMIN = "Admin";
    public const string USER = "User";
    public const string ADMIN_ACCOUNT_ID = "admin-user-id";
    public const string USER_ACCOUNT_ID = "regular-user-id";
    public const string ADMIN_ROLE_ID = "admin-role-id";
    public const string USER_ROLE_ID = "user-role-id";
    public const string ADMIN_EMAIL = "admin@travel.com";
    public const string ADMIN_PASSWORD = "Admin123!";
    public const string USER_EMAIL = "user@travel.com";
    public const string USER_PASSWORD = "User123!";
    public const string LOREM_IPSUM = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.\r\n\r\nDuis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat, vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi. Lorem ipsum dolor sit amet, consectetuer adipiscing elit, sed diam nonummy nibh euismod tincidunt ut laoreet dolore magna aliquam erat volutpat.\r\n\r\nUt wisi enim ad minim veniam, quis nostrud exerci tation ullamcorper suscipit lobortis nisl ut aliquip ex ea commodo consequat. Duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat, vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi.\r\n\r\nNam liber tempor cum soluta nobis eleifend option congue nihil imperdiet doming id quod mazim placerat facer possim assum. Lorem ipsum dolor sit amet, consectetuer adipiscing elit, sed diam nonummy nibh euismod tincidunt ut laoreet dolore magna aliquam erat volutpat. Ut wisi enim ad minim veniam, quis nostrud exerci tation ullamcorper suscipit lobortis nisl ut aliquip ex ea commodo consequat.";

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Many-to-Many relationship between Destination and Tag
        builder.Entity<Destination>()
            .HasMany(d => d.Tags)
            .WithMany(t => t.Destinations)
            .UsingEntity(j => j.ToTable("DestinationTags"));

        // Define Shadow Properties and Keys for the shadow table
        builder.Entity("DestinationTags", b =>
        {
            b.Property<int>("DestinationsId");
            b.Property<int>("TagsId");
            b.HasKey("DestinationsId", "TagsId");
        });

        // blocks deletion of country if there exist Destinations from it
        builder.Entity<Destination>()
            .HasOne(d => d.Country)
            .WithMany(c => c.Destinations)
            .HasForeignKey(d => d.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        // blocks deletion of category if there exist Destinations with it
        builder.Entity<Destination>()
            .HasOne(d => d.Category)
            .WithMany(c => c.Destinations)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // set destionation's author to null if user is deleted
        builder.Entity<Destination>()
            .HasOne(d => d.Author)
            .WithMany(a => a.Destinations)
            .HasForeignKey(d => d.AuthorId)
            .OnDelete(DeleteBehavior.SetNull);

        // if destination is deleted - image records are deleted too
        builder.Entity<DestinationImage>()
            .HasOne(di => di.Destination)
            .WithMany(d => d.Images)
            .HasForeignKey(di => di.DestinationId)
            .OnDelete(DeleteBehavior.Cascade);

        // if user is deleted - his catalogs are deleted too
        builder.Entity<Catalog>()
            .HasOne(c => c.ApplicationUser)
            .WithMany(au => au.Catalogs)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // unique pair (catalog id, destination id)
        builder.Entity<CatalogDestination>()
            .HasKey(cd => new { cd.CatalogId, cd.DestinationId });

        builder.Entity<CatalogDestination>()
            .HasOne(cd => cd.Catalog)
            .WithMany(c => c.CatalogDestinations)
            .HasForeignKey(cd => cd.CatalogId);

        builder.Entity<CatalogDestination>()
            .HasOne(cd => cd.Destination)
            .WithMany(d => d.CatalogDestinations)
            .HasForeignKey(cd => cd.DestinationId);

        // if catalog is deleted - delete all links to destinations
        builder.Entity<CatalogDestination>()
            .HasOne(cd => cd.Catalog)
            .WithMany(c => c.CatalogDestinations)
            .HasForeignKey(cd => cd.CatalogId)
            .OnDelete(DeleteBehavior.Cascade);

        // if destination is deleted - delete all links to catalogs
        builder.Entity<CatalogDestination>()
            .HasOne(cd => cd.Destination)
            .WithMany(d => d.CatalogDestinations)
            .HasForeignKey(cd => cd.DestinationId)
            .OnDelete(DeleteBehavior.Cascade);

        // if destination is deleted - delete all its articles and events
        builder.Entity<Article>()
            .HasOne(a => a.Destination)
            .WithMany(d => d.Articles) 
            .HasForeignKey(a => a.DestinationId)
            .OnDelete(DeleteBehavior.Cascade);

        // if destination is deleted - delete all its articles and events
        builder.Entity<Event>()
            .HasOne(e => e.Destination)
            .WithMany(d => d.Events) 
            .HasForeignKey(e => e.DestinationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.PhoneNumber).IsRequired();
        });

        builder.Entity<Country>().HasData(
            new Country { Id = 1, Name = "България" },
            new Country { Id = 2, Name = "Гърция" },
            new Country { Id = 3, Name = "Италия" },
            new Country { Id = 4, Name = "Испания" },
            new Country { Id = 5, Name = "Франция" },
            new Country { Id = 6, Name = "Германия" },
            new Country { Id = 7, Name = "Хърватия" },
            new Country { Id = 8, Name = "Турция" },
            new Country { Id = 9, Name = "Португалия" },
            new Country { Id = 10, Name = "Черна гора" }
        );

        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Плаж" },
            new Category { Id = 2, Name = "Планина" },
            new Category { Id = 3, Name = "Градска дестинация" },
            new Category { Id = 4, Name = "Културна дестинация" },
            new Category { Id = 5, Name = "Природна забележителност" },
            new Category { Id = 6, Name = "Селски туризъм" },
            new Category { Id = 7, Name = "Приключенски туризъм" },
            new Category { Id = 8, Name = "Гастрономически туризъм" },
            new Category { Id = 9, Name = "СПА и уелнес" }
        );

        builder.Entity<Tag>().HasData(
            new Tag { Id = 1, Name = "Историческо място" },
            new Tag { Id = 2, Name = "Семейно" },
            new Tag { Id = 3, Name = "Романтично" },
            new Tag { Id = 4, Name = "Приключенско" },
            new Tag { Id = 5, Name = "Културно" },
            new Tag { Id = 6, Name = "Планинско изкачване" },
            new Tag { Id = 7, Name = "Водни спортове" },
            new Tag { Id = 8, Name = "Гурме" },
            new Tag { Id = 9, Name = "Нощен живот" },
            new Tag { Id = 10, Name = "Природни чудеса" },
            new Tag { Id = 11, Name = "Разходки" },
            new Tag { Id = 12, Name = "Почивка и релакс" }
            );

        builder.Entity<DestinationImage>().HasData(
            new DestinationImage { Id = 1, ImageUrl = "/images/destinations/dest1_1.jpg", DestinationId = 1 },
            new DestinationImage { Id = 2, ImageUrl = "/images/destinations/dest1_2.jpg", DestinationId = 1 },
            new DestinationImage { Id = 3, ImageUrl = "/images/destinations/dest1_3.jpg", DestinationId = 1 },

            new DestinationImage { Id = 4, ImageUrl = "/images/destinations/dest2_1.jpg", DestinationId = 2 },
            new DestinationImage { Id = 5, ImageUrl = "/images/destinations/dest2_2.jpg", DestinationId = 2 },
            new DestinationImage { Id = 6, ImageUrl = "/images/destinations/dest2_3.jpg", DestinationId = 2 },

            new DestinationImage { Id = 7, ImageUrl = "/images/destinations/dest3_1.jpg", DestinationId = 3 },
            new DestinationImage { Id = 8, ImageUrl = "/images/destinations/dest3_2.jpg", DestinationId = 3 },
       
            // Destination 4 - 0 images

            new DestinationImage { Id = 9, ImageUrl = "/images/destinations/dest5_1.jpg", DestinationId = 5 },
            new DestinationImage { Id = 10, ImageUrl = "/images/destinations/dest5_2.jpg", DestinationId = 5 },
            new DestinationImage { Id = 11, ImageUrl = "/images/destinations/dest5_3.jpg", DestinationId = 5 },
            new DestinationImage { Id = 12, ImageUrl = "/images/destinations/dest5_4.jpg", DestinationId = 5 },
            new DestinationImage { Id = 13, ImageUrl = "/images/destinations/dest5_5.jpg", DestinationId = 5 },

            new DestinationImage { Id = 14, ImageUrl = "/images/destinations/dest6_1.jpg", DestinationId = 6 },

            new DestinationImage { Id = 15, ImageUrl = "/images/destinations/dest7_1.jpg", DestinationId = 7 },
            new DestinationImage { Id = 16, ImageUrl = "/images/destinations/dest7_2.jpg", DestinationId = 7 },
            new DestinationImage { Id = 17, ImageUrl = "/images/destinations/dest7_3.jpg", DestinationId = 7 },
            new DestinationImage { Id = 18, ImageUrl = "/images/destinations/dest7_4.jpg", DestinationId = 7 },

            new DestinationImage { Id = 19, ImageUrl = "/images/destinations/dest8_1.jpg", DestinationId = 8 },
            new DestinationImage { Id = 20, ImageUrl = "/images/destinations/dest8_2.jpg", DestinationId = 8 },

            new DestinationImage { Id = 21, ImageUrl = "/images/destinations/dest9_1.jpg", DestinationId = 9 },
            new DestinationImage { Id = 22, ImageUrl = "/images/destinations/dest9_2.jpg", DestinationId = 9 },
            new DestinationImage { Id = 23, ImageUrl = "/images/destinations/dest9_3.jpg", DestinationId = 9 },

            // Destination 10 to 20 - 0 images (Skipped)

            new DestinationImage { Id = 24, ImageUrl = "/images/destinations/dest11_1.jpg", DestinationId = 11 },

            new DestinationImage { Id = 25, ImageUrl = "/images/destinations/dest12_1.jpg", DestinationId = 12 },
            new DestinationImage { Id = 26, ImageUrl = "/images/destinations/dest12_2.jpg", DestinationId = 12 },
            new DestinationImage { Id = 27, ImageUrl = "/images/destinations/dest12_3.jpg", DestinationId = 12 },
            new DestinationImage { Id = 28, ImageUrl = "/images/destinations/dest12_4.jpg", DestinationId = 12 },
            new DestinationImage { Id = 29, ImageUrl = "/images/destinations/dest12_5.jpg", DestinationId = 12 },

            new DestinationImage { Id = 30, ImageUrl = "/images/destinations/dest13_1.jpg", DestinationId = 13 },
            new DestinationImage { Id = 31, ImageUrl = "/images/destinations/dest13_2.jpg", DestinationId = 13 },

            new DestinationImage { Id = 32, ImageUrl = "/images/destinations/dest14_1.jpg", DestinationId = 14 },
            new DestinationImage { Id = 33, ImageUrl = "/images/destinations/dest14_2.jpg", DestinationId = 14 },
            new DestinationImage { Id = 34, ImageUrl = "/images/destinations/dest14_3.jpg", DestinationId = 14 },

            new DestinationImage { Id = 35, ImageUrl = "/images/destinations/dest15_1.jpg", DestinationId = 15 },
            new DestinationImage { Id = 36, ImageUrl = "/images/destinations/dest15_2.jpg", DestinationId = 15 },
            new DestinationImage { Id = 37, ImageUrl = "/images/destinations/dest15_3.jpg", DestinationId = 15 },
            new DestinationImage { Id = 38, ImageUrl = "/images/destinations/dest15_4.jpg", DestinationId = 15 },

            new DestinationImage { Id = 39, ImageUrl = "/images/destinations/dest16_1.jpg", DestinationId = 16 },

            new DestinationImage { Id = 40, ImageUrl = "/images/destinations/dest17_1.jpg", DestinationId = 17 },
            new DestinationImage { Id = 41, ImageUrl = "/images/destinations/dest17_2.jpg", DestinationId = 17 },

            new DestinationImage { Id = 42, ImageUrl = "/images/destinations/dest18_1.jpg", DestinationId = 18 },
            new DestinationImage { Id = 43, ImageUrl = "/images/destinations/dest18_2.jpg", DestinationId = 18 },
            new DestinationImage { Id = 44, ImageUrl = "/images/destinations/dest18_3.jpg", DestinationId = 18 },
            new DestinationImage { Id = 45, ImageUrl = "/images/destinations/dest18_4.jpg", DestinationId = 18 },
            new DestinationImage { Id = 46, ImageUrl = "/images/destinations/dest18_5.jpg", DestinationId = 18 },

            new DestinationImage { Id = 47, ImageUrl = "/images/destinations/dest19_1.jpg", DestinationId = 19 },
            new DestinationImage { Id = 48, ImageUrl = "/images/destinations/dest19_2.jpg", DestinationId = 19 },
            new DestinationImage { Id = 49, ImageUrl = "/images/destinations/dest19_3.jpg", DestinationId = 19 },

            new DestinationImage { Id = 50, ImageUrl = "/images/destinations/dest20_1.jpg", DestinationId = 20 },
            new DestinationImage { Id = 51, ImageUrl = "/images/destinations/dest20_2.jpg", DestinationId = 20 }
        );

        builder.Entity<Destination>().HasData(
            new Destination
            {
                Id = 1,
                Name = "Рилски манастир",
                Region = "Кюстендил",
                CountryId = 1,
                CategoryId = 4,
                AuthorId = null, // System entry
                CreationDate = new DateOnly(2024, 5, 12),
                Description = "Най-големият и известен манастир в България."
            },
            new Destination
            {
                Id = 2,
                Name = "Плаж Навагио",
                Region = "Закинтос",
                CountryId = 2,
                CategoryId = 1,
                AuthorId = ADMIN_ACCOUNT_ID,
                CreationDate = new DateOnly(2024, 6, 20),
                Description = "Един от най-красивите плажове в света."
            },
            new Destination
            {
                Id = 3,
                Name = "Колизеум",
                Region = "Рим",
                CountryId = 3,
                CategoryId = 4,
                AuthorId = ADMIN_ACCOUNT_ID,
                CreationDate = new DateOnly(2023, 11, 05),
                Description = "Емблематичен амфитеатър в сърцето на Италия."
            },
            new Destination
            {
                Id = 4,
                Name = "Саграда Фамилия",
                Region = "Барселона",
                CountryId = 4,
                CategoryId = 4,
                AuthorId = ADMIN_ACCOUNT_ID,
                CreationDate = new DateOnly(2024, 1, 15),
                Description = "Шедьовърът на Антони Гауди."
            },
            new Destination
            {
                Id = 5,
                Name = "Айфелова кула",
                Region = "Париж",
                CountryId = 5,
                CategoryId = 3,
                AuthorId = ADMIN_ACCOUNT_ID,
                CreationDate = new DateOnly(2023, 12, 25),
                Description = "Символът на Франция и любовта."
            },
            new Destination
            {
                Id = 6,
                Name = "Нойшванщайн",
                Region = "Бавария",
                CountryId = 6,
                CategoryId = 4,
                AuthorId = null, // System entry
                CreationDate = new DateOnly(2024, 2, 10),
                Description = "Приказният замък на Лудвиг II."
            },
            new Destination
            {
                Id = 7,
                Name = "Дубровник - Стар град",
                Region = "Далмация",
                CountryId = 7,
                CategoryId = 4,
                AuthorId = ADMIN_ACCOUNT_ID,
                CreationDate = new DateOnly(2024, 7, 02),
                Description = "Перлата на Адриатическо море."
            },
            new Destination
            {
                Id = 8,
                Name = "Кападокия",
                Region = "Анатолия",
                CountryId = 8,
                CategoryId = 5,
                AuthorId = ADMIN_ACCOUNT_ID,
                CreationDate = new DateOnly(2024, 4, 18),
                Description = "Магическо място с летящи балони и пещери."
            },
            new Destination
            {
                Id = 9,
                Name = "Лисабон - Белем",
                Region = "Лисабон",
                CountryId = 9,
                CategoryId = 3,
                AuthorId = ADMIN_ACCOUNT_ID,
                CreationDate = new DateOnly(2024, 3, 30),
                Description = "Исторически квартал с изглед към океана."
            },
            new Destination
            {
                Id = 10,
                Name = "Которски залив",
                Region = "Котор",
                CountryId = 10,
                CategoryId = 5,
                AuthorId = null, // System entry
                CreationDate = new DateOnly(2024, 8, 12),
                Description = "Най-южният фиорд в Европа."
            },
            new Destination
            {
                Id = 11,
                Name = "Седемте рилски езера",
                Region = "Рила",
                CountryId = 1,
                CategoryId = 2,
                AuthorId = ADMIN_ACCOUNT_ID,
                CreationDate = new DateOnly(2024, 8, 25),
                Description = "Група езера с ледников произход в Рила."
            },
            new Destination
            {
                Id = 12,
                Name = "Метеора",
                Region = "Тесалия",
                CountryId = 2,
                CategoryId = 5,
                AuthorId = ADMIN_ACCOUNT_ID,
                CreationDate = new DateOnly(2023, 10, 14),
                Description = "Манастири, кацнали върху величествени скали."
            },
            new Destination
            {
                Id = 13,
                Name = "Позитано",
                Region = "Амалфийско крайбрежие",
                CountryId = 3,
                CategoryId = 1,
                AuthorId = ADMIN_ACCOUNT_ID,
                CreationDate = new DateOnly(2024, 5, 05),
                Description = "Цветно градче по стръмните скали на Италия."
            },
            new Destination
            {
                Id = 14,
                Name = "Парк Гюел",
                Region = "Барселона",
                CountryId = 4,
                CategoryId = 5,
                AuthorId = null, // System entry
                CreationDate = new DateOnly(2024, 2, 20),
                Description = "Обществен парк с мозайки и уникална архитектура."
            },
            new Destination
            {
                Id = 15,
                Name = "Мон Сен Мишел",
                Region = "Нормандия",
                CountryId = 5,
                CategoryId = 4,
                AuthorId = null, // System entry
                CreationDate = new DateOnly(2023, 09, 12),
                Description = "Остров-крепост, обграден от приливи."
            },
            new Destination
            {
                Id = 16,
                Name = "Бранденбургската врата",
                Region = "Берлин",
                CountryId = 6,
                CategoryId = 3,
                AuthorId = ADMIN_ACCOUNT_ID,
                CreationDate = new DateOnly(2024, 01, 01),
                Description = "Символ на единството и историята на Германия."
            },
            new Destination
            {
                Id = 17,
                Name = "Плитвишки езера",
                Region = "Лика",
                CountryId = 7,
                CategoryId = 5,
                AuthorId = null, // System entry
                CreationDate = new DateOnly(2024, 6, 15),
                Description = "Тюркоазени езера и каскадни водопади."
            },
            new Destination
            {
                Id = 18,
                Name = "Памуккале",
                Region = "Денизли",
                CountryId = 8,
                CategoryId = 5,
                AuthorId = ADMIN_ACCOUNT_ID,
                CreationDate = new DateOnly(2024, 4, 02),
                Description = "Бели тераси от травертин с термални води."
            },
            new Destination
            {
                Id = 19,
                Name = "Алгарве - Прая да Роша",
                Region = "Алгарве",
                CountryId = 9,
                CategoryId = 1,
                AuthorId = ADMIN_ACCOUNT_ID,
                CreationDate = new DateOnly(2024, 7, 20),
                Description = "Златни пясъци и зашеметяващи скални образувания."
            },
            new Destination
            {
                Id = 20,
                Name = "Будва - Стар град",
                Region = "Будва",
                CountryId = 10,
                CategoryId = 3,
                AuthorId = ADMIN_ACCOUNT_ID,
                CreationDate = new DateOnly(2024, 8, 05),
                Description = "Средновековен град на брега на морето."
            }
        );

        builder.Entity<Article>().HasData(
            new Article { Id = 1, Title = "История на Рилския манастир", Content = "Подробен пътеводител през вековете...", DestinationId = 1, Description = LOREM_IPSUM },
            new Article { Id = 2, Title = "Тайните на Закинтос", Content = "Как да стигнете до най-фотографирания плаж...", DestinationId = 2, Description = LOREM_IPSUM },
            new Article { Id = 3, Title = "Рим: Вечният град", Content = "Интересни факти за гладиаторските борби в Колизеума.", DestinationId = 3, Description = LOREM_IPSUM },
            new Article { Id = 4, Title = "Архитектурата на Гауди", Content = "Защо Саграда Фамилия още не е завършена?", DestinationId = 4, Description = LOREM_IPSUM },
            new Article { Id = 5, Title = "Нощен Париж", Content = "Най-добрите места за наблюдение на Айфеловата кула.", DestinationId = 5, Description = LOREM_IPSUM },
            new Article { Id = 6, Title = "Замъкът от приказките", Content = "Вдъхновението зад логото на Дисни - Нойшванщайн.", DestinationId = 6, Description = LOREM_IPSUM },
            new Article { Id = 7, Title = "Дубровник - перлата на Адриатика", Content = "Къде са снимани сцените от Game of Thrones.", DestinationId = 7, Description = LOREM_IPSUM },
            new Article { Id = 8, Title = "Балони над Кападокия", Content = "Всичко, което трябва да знаете преди полета.", DestinationId = 8, Description = LOREM_IPSUM },
            new Article { Id = 9, Title = "Лисабон: Кулата Белем", Content = "Символът на епохата на Великите географски открития.", DestinationId = 9, Description = LOREM_IPSUM },
            new Article { Id = 10, Title = "Легендите на Метеора", Content = "Как са строени манастирите върху тези скали.", DestinationId = 12, Description = LOREM_IPSUM }
        );

        builder.Entity<Event>().HasData(
            new Event { Id = 1, Name = "Празник на Рилския манастир", Description = "Голям църковен събор.", Date = new DateTime(2026, 8, 28), DestinationId = 1 },
            new Event { Id = 2, Name = "Летен плажен фестивал", Description = "Музика на живо на брега.", Date = new DateTime(2026, 7, 15), DestinationId = 2 },
            new Event { Id = 3, Name = "Римски празници", Description = "Реконструкция на антични битки.", Date = new DateTime(2026, 4, 21), DestinationId = 3 },
            new Event { Id = 4, Name = "Ден на Барселона", Description = "Парад и фойерверки.", Date = new DateTime(2026, 9, 24), DestinationId = 4 },
            new Event { Id = 5, Name = "Ден на Бастилията", Description = "Тържества около Айфеловата кула.", Date = new DateTime(2026, 7, 14), DestinationId = 5 },
            new Event { Id = 6, Name = "Октоберфест екскурзия", Description = "Традиционни занаяти и бира.", Date = new DateTime(2026, 9, 20), DestinationId = 6 },
            new Event { Id = 7, Name = "Летен фестивал Дубровник", Description = "Театрални постановки на открито.", Date = new DateTime(2026, 8, 10), DestinationId = 7 },
            new Event { Id = 8, Name = "Фестивал на балоните", Description = "Най-голямото излитане на годината.", Date = new DateTime(2026, 10, 05), DestinationId = 8 },
            new Event { Id = 9, Name = "Fado вечер в Лисабон", Description = "Концерт с традиционна музика.", Date = new DateTime(2026, 11, 12), DestinationId = 9 },
            new Event { Id = 10, Name = "Зимен празник Будва", Description = "Новогодишен концерт на площада.", Date = new DateTime(2026, 12, 31), DestinationId = 20 }
        );

        // Many-to-Many: Destination <-> Tag
        builder.Entity<Destination>()
            .HasMany(d => d.Tags)
            .WithMany(t => t.Destinations)
            .UsingEntity<Dictionary<string, object>>(
                "DestinationTags", // The table name
                j => j.HasOne<Tag>().WithMany().HasForeignKey("TagsId"),
                j => j.HasOne<Destination>().WithMany().HasForeignKey("DestinationsId"),
                j =>
                {
                    j.HasKey("DestinationsId", "TagsId"); // Explicitly define the composite key

                    j.HasData(
                        new { DestinationsId = 1, TagsId = 1 },
                        new { DestinationsId = 1, TagsId = 5 },
                        new { DestinationsId = 1, TagsId = 12 },
                        new { DestinationsId = 2, TagsId = 3 },
                        new { DestinationsId = 2, TagsId = 4 },
                        new { DestinationsId = 2, TagsId = 10 },
                        new { DestinationsId = 2, TagsId = 12 },
                        new { DestinationsId = 3, TagsId = 1 },
                        new { DestinationsId = 3, TagsId = 5 },
                        new { DestinationsId = 3, TagsId = 8 },
                        new { DestinationsId = 3, TagsId = 11 },
                        new { DestinationsId = 4, TagsId = 1 },
                        new { DestinationsId = 4, TagsId = 5 },
                        new { DestinationsId = 4, TagsId = 8 },
                        new { DestinationsId = 4, TagsId = 9 },
                        new { DestinationsId = 4, TagsId = 11 },
                        new { DestinationsId = 5, TagsId = 3 },
                        new { DestinationsId = 5, TagsId = 5 },
                        new { DestinationsId = 5, TagsId = 8 },
                        new { DestinationsId = 5, TagsId = 11 },
                        new { DestinationsId = 6, TagsId = 1 },
                        new { DestinationsId = 6, TagsId = 3 },
                        new { DestinationsId = 6, TagsId = 10 },
                        new { DestinationsId = 7, TagsId = 1 },
                        new { DestinationsId = 7, TagsId = 3 },
                        new { DestinationsId = 7, TagsId = 5 },
                        new { DestinationsId = 7, TagsId = 11 },
                        new { DestinationsId = 8, TagsId = 4 },
                        new { DestinationsId = 8, TagsId = 10 },
                        new { DestinationsId = 8, TagsId = 3 },
                        new { DestinationsId = 8, TagsId = 5 },
                        new { DestinationsId = 9, TagsId = 1 },
                        new { DestinationsId = 9, TagsId = 8 },
                        new { DestinationsId = 9, TagsId = 9 },
                        new { DestinationsId = 10, TagsId = 10 },
                        new { DestinationsId = 10, TagsId = 12 },
                        new { DestinationsId = 11, TagsId = 4 },
                        new { DestinationsId = 11, TagsId = 6 },
                        new { DestinationsId = 11, TagsId = 10 },
                        new { DestinationsId = 11, TagsId = 11 },
                        new { DestinationsId = 12, TagsId = 1 },
                        new { DestinationsId = 12, TagsId = 5 },
                        new { DestinationsId = 12, TagsId = 10 },
                        new { DestinationsId = 13, TagsId = 3 },
                        new { DestinationsId = 13, TagsId = 8 },
                        new { DestinationsId = 13, TagsId = 12 },
                        new { DestinationsId = 14, TagsId = 5 },
                        new { DestinationsId = 14, TagsId = 11 },
                        new { DestinationsId = 14, TagsId = 2 },
                        new { DestinationsId = 15, TagsId = 1 },
                        new { DestinationsId = 15, TagsId = 10 },
                        new { DestinationsId = 15, TagsId = 5 },
                        new { DestinationsId = 16, TagsId = 1 },
                        new { DestinationsId = 16, TagsId = 5 },
                        new { DestinationsId = 17, TagsId = 10 },
                        new { DestinationsId = 17, TagsId = 11 },
                        new { DestinationsId = 17, TagsId = 2 },
                        new { DestinationsId = 17, TagsId = 12 },
                        new { DestinationsId = 18, TagsId = 10 },
                        new { DestinationsId = 18, TagsId = 12 },
                        new { DestinationsId = 18, TagsId = 2 },
                        new { DestinationsId = 19, TagsId = 7 },
                        new { DestinationsId = 19, TagsId = 9 },
                        new { DestinationsId = 19, TagsId = 12 },
                        new { DestinationsId = 20, TagsId = 1 },
                        new { DestinationsId = 20, TagsId = 9 }
                    );
                });

        builder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = ADMIN_ROLE_ID, Name = ADMIN, NormalizedName = "ADMIN" },
            new IdentityRole { Id = USER_ROLE_ID, Name = USER, NormalizedName = "USER" }
        );

        var hasher = new PasswordHasher<ApplicationUser>();

        builder.Entity<ApplicationUser>().HasData(
            new ApplicationUser
            {
                Id = ADMIN_ACCOUNT_ID,
                UserName = ADMIN_EMAIL,
                NormalizedUserName = ADMIN_EMAIL.ToUpper(),
                Email = ADMIN_EMAIL,
                NormalizedEmail = ADMIN_EMAIL.ToUpper(),
                EmailConfirmed = true,
                FirstName = "Админ",
                LastName = "Системен",
                PhoneNumber = "+359888111222",
                PhoneNumberConfirmed = true,
                PasswordHash = hasher.HashPassword(null!, ADMIN_PASSWORD),
                SecurityStamp = Guid.NewGuid().ToString()
            },
            new ApplicationUser
            {
                Id = USER_ACCOUNT_ID,
                UserName = USER_EMAIL,
                NormalizedUserName = USER_EMAIL.ToUpper(),
                Email = USER_EMAIL,
                NormalizedEmail = USER_EMAIL.ToUpper(),
                EmailConfirmed = true,
                FirstName = "Иван",
                LastName = "Иванов",
                PhoneNumber = "+359888333444",
                PhoneNumberConfirmed = true,
                PasswordHash = hasher.HashPassword(null!, USER_PASSWORD),
                SecurityStamp = Guid.NewGuid().ToString()
            }
        );

        builder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>
            {
                RoleId = ADMIN_ROLE_ID,
                UserId = ADMIN_ACCOUNT_ID
            },
            new IdentityUserRole<string>
            {
                RoleId = USER_ROLE_ID,
                UserId = USER_ACCOUNT_ID
            }
        );

        builder.Entity<Catalog>().HasData(
            new Catalog
            {
                Id = 1,
                Name = "Балкани",
                UserId = ADMIN_ACCOUNT_ID,
                IsPublic = true,
                Notes = "Да се проверят граничните такси.\nПланиране на маршрут през уикенда.\nВключи местни ресторанти."
            },
            new Catalog
            {
                Id = 2,
                Name = "Историческо наследство",
                UserId = ADMIN_ACCOUNT_ID,
                IsPublic = false,
                Notes = "Списъкът включва обекти под защитата на ЮНЕСКО.\nПровери работното време на музеите.\nНоси удобни обувки."
            },
            new Catalog
            {
                Id = 3,
                Name = "Европейски столици",
                UserId = ADMIN_ACCOUNT_ID,
                IsPublic = true,
                Notes = "Полети от София - нискотарифни компании.\nРезервация на хотели поне месец по-рано.\nКарти за градския транспорт."
            },
            new Catalog
            {
                Id = 4,
                Name = "Приключенски дестинации",
                UserId = ADMIN_ACCOUNT_ID,
                IsPublic = false,
                Notes = "Изисква се висока физическа подготовка.\nПровери екипировката преди тръгване.\nЗастраховката е задължителна."
            },
            new Catalog
            {
                Id = 5,
                Name = "Моят списък за лятото",
                UserId = USER_ACCOUNT_ID,
                IsPublic = true,
                Notes = "Резервирай почивка до края на април.\nКупи нов слънцезащитен крем.\nПровери кога изтича личната карта."
            },
            new Catalog
            {
                Id = 6,
                Name = "Места за посещение в Гърция",
                UserId = USER_ACCOUNT_ID,
                IsPublic = false,
                Notes = "Търси по-спокойни острови.\nПровери фериботните разписания.\nДа се опита местния октопод."
            },
            new Catalog
            {
                Id = 7,
                Name = "Зимни курорти",
                UserId = USER_ACCOUNT_ID,
                IsPublic = true,
                Notes = "Провери цените на лифт картите.\nРезервация за ски училище.\nСервиз на ските и борда."
            }
        );

        builder.Entity<CatalogDestination>().HasData(
            new CatalogDestination { CatalogId = 1, DestinationId = 1, DisplayOrder = 0 },
            new CatalogDestination { CatalogId = 1, DestinationId = 7, DisplayOrder = 1 },
            new CatalogDestination { CatalogId = 1, DestinationId = 10, DisplayOrder = 2 },

            new CatalogDestination { CatalogId = 2, DestinationId = 1, DisplayOrder = 0 },
            new CatalogDestination { CatalogId = 2, DestinationId = 3, DisplayOrder = 1 },
            new CatalogDestination { CatalogId = 2, DestinationId = 4, DisplayOrder = 2 },

            new CatalogDestination { CatalogId = 3, DestinationId = 5, DisplayOrder = 0 },
            new CatalogDestination { CatalogId = 3, DestinationId = 16, DisplayOrder = 1 },

            new CatalogDestination { CatalogId = 4, DestinationId = 8, DisplayOrder = 0 },
            new CatalogDestination { CatalogId = 4, DestinationId = 11, DisplayOrder = 1 },

            new CatalogDestination { CatalogId = 5, DestinationId = 1, DisplayOrder = 0 },
            new CatalogDestination { CatalogId = 5, DestinationId = 2, DisplayOrder = 1 },

            new CatalogDestination { CatalogId = 6, DestinationId = 2, DisplayOrder = 0 },
            new CatalogDestination { CatalogId = 6, DestinationId = 12, DisplayOrder = 1 },

            new CatalogDestination { CatalogId = 7, DestinationId = 6, DisplayOrder = 0 }
        );
    }
}
