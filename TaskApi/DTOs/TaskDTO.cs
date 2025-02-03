namespace TaskApi.DTOs
{
    public class TaskDTO
    {
        public required string Title { get; set; }  // Ensure it's required
        public string? Description { get; set; }   // Nullable field
        public required string Status { get; set; } = "Pending";  // Default value
        public DateTime? DueDate { get; set; }  // Nullable field
    }
}
