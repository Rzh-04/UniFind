using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using UniversityLostAndFound.Models;

namespace UniversityLostAndFound.ViewModels
{
    public class ItemCreateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(120, ErrorMessage = "Title cannot exceed 120 characters.")]
        [Display(Name = "Item Name / Short Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please provide a detailed description.")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
        [Display(Name = "Detailed Description & Distinguishing Features")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please specify whether item was lost or found.")]
        [Display(Name = "Post Type")]
        public ItemType ItemType { get; set; } = ItemType.Lost;

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Please select a campus location.")]
        [Display(Name = "Campus Location")]
        public int LocationId { get; set; }

        [Required(ErrorMessage = "Please enter the date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date Lost / Found")]
        public DateTime DateLostOrFound { get; set; } = DateTime.Today;

        [Display(Name = "Upload Photo")]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImagePath { get; set; }

        [StringLength(50)]
        [Display(Name = "Contact Phone Number (Optional)")]
        public string? ContactPhone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Contact Email Address (Optional)")]
        public string? ContactEmail { get; set; }

        [StringLength(200)]
        [Display(Name = "Reward Details / Additional Notes (Optional)")]
        public string? RewardDetails { get; set; }
    }
}
