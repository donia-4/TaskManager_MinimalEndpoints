namespace TaskManager.Dtos.Responses
{
    public class SmartTaskResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Deadline { get; set; }
        public int Importance { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }

        // Smart fields
        public double UrgencyScore { get; set; }
        public string Status { get; set; } = string.Empty; // Critical, High, Normal, Low
        public double HoursRemaining { get; set; }
    }
}
