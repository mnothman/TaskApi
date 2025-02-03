using System.ComponentModel.DataAnnotations;

namespace TaskApi.Models
{
    public class TaskModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Title { get; set; } // Add "required" modifier

        public string? Description { get; set; } // Nullable description

        [Required]
        public string Status { get; set; } = "Pending"; // Default value

        public DateTime? DueDate { get; set; } // Nullable DateTime
    }
}
