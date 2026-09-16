using System.ComponentModel.DataAnnotations;

namespace UniversityLostAndFound.Models
{
    // Campus locations where items were lost or found
    public class Location
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Location Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Building / Area Code")]
        public string? BuildingCode { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }

        public virtual ICollection<Item> Items { get; set; } = new List<Item>();
    }
}
