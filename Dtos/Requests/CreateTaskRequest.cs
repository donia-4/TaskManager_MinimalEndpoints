using System.ComponentModel.DataAnnotations;

namespace TaskManager.Dtos.Requests
{
    public class CreateTaskRequest
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Deadline is required")]
        [DataType(DataType.DateTime)]
        public DateTime Deadline { get; set; }

        [Required(ErrorMessage = "Importance is required")]
        [Range(1, 10, ErrorMessage = "Importance must be between 1 and 10")]
        public int Importance { get; set; }
    }
}