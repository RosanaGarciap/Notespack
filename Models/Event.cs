using System;
using System.ComponentModel.DataAnnotations;

namespace NOTESPACK.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required")]
        public DateTime Date { get; set; } = DateTime.Now;
        
        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "End date is required")]
        public string Duration {get; set;} = string.Empty;

        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Organizer is required")]
        public string Organizer { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public User User { get; set; } = null!;
    }
}