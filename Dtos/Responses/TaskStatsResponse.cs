namespace TaskManager.Dtos.Responses
{
    public class TaskStatsResponse
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int CriticalTasks { get; set; }
        public int OverdueTasks { get; set; }
        public double AverageUrgencyScore { get; set; }
    }
}
