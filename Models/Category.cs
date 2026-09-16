using System.ComponentModel.DataAnnotations;

namespace UniversityLostAndFound.Models
{
    // Categories for items (Electronics, Books, Keys, IDs, etc.)
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Bootstrap Icon Class")]
        public string IconClass { get; set; } = "bi-folder";

        [StringLength(250)]
        public string? Description { get; set; }

        public virtual ICollection<Item> Items { get; set; } = new List<Item>();
    }
}
