namespace TravelMap.Models
{
    public class CatalogDestination
    {
        public int CatalogId { get; set; }
        public Catalog Catalog { get; set; } = null!;

        public int DestinationId { get; set; }
        public Destination Destination { get; set; } = null!;

        public int DisplayOrder { get; set; }
    }
}
