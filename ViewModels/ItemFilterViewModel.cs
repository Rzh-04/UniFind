using UniversityLostAndFound.Models;

namespace UniversityLostAndFound.ViewModels
{
    public class ItemFilterViewModel
    {
        public string? SearchTerm { get; set; }
        public ItemType? ItemType { get; set; }
        public int? CategoryId { get; set; }
        public int? LocationId { get; set; }
        public ItemStatus? Status { get; set; }
        public string SortOrder { get; set; } = "newest";

        public IEnumerable<Item> Items { get; set; } = new List<Item>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public IEnumerable<Location> Locations { get; set; } = new List<Location>();

        public int TotalCount { get; set; }
        public string? CurrentUserId { get; set; }
        public bool ShowingMyReports { get; set; }
    }
}
