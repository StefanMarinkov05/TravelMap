namespace TravelMap.Models
{
    public class DestinationSpecifications
    {
        public IEnumerable<Country> Countries { get; set; } = new List<Country>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public IEnumerable<Tag> Tags { get; set; } = new List<Tag>();

    }
}
