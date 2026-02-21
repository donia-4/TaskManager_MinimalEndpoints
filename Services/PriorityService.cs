using TaskManager.Dtos.Responses;
using TaskManager.Interfaces;
using TaskManager.Models;

namespace TaskManager.Services
{
    public class PriorityService : IPriorityService
    {
        private const double URGENCY_WEIGHT = 1.5;
        private const double BUFFER_HOURS = 0.1; // Prevents division by zero
        private const double CRITICAL_THRESHOLD = 80.0;
        private const double HIGH_THRESHOLD = 50.0;
        private const double NORMAL_THRESHOLD = 20.0;

        public double CalculateUrgencyScore(TodoTask task)
        {
            var hoursRemaining = CalculateHoursRemaining(task.Deadline);

            // If task is overdue, return maximum urgency
            if (hoursRemaining <= 0)
            {
                return 999.0;
            }

            // Formula: Score = (Importance * Weight) / (HoursRemaining + Buffer)
            // The buffer prevents division by zero and smooths the curve
            var score = (task.Importance * URGENCY_WEIGHT) / (hoursRemaining + BUFFER_HOURS);

            return Math.Round(score, 2);
        }

        public string DetermineStatus(double urgencyScore)
        {
            return urgencyScore switch
            {
                >= CRITICAL_THRESHOLD => "Critical",
                >= HIGH_THRESHOLD => "High",
                >= NORMAL_THRESHOLD => "Normal",
                _ => "Low"
            };
        }

        public double CalculateHoursRemaining(DateTime deadline)
        {
            var timeSpan = deadline - DateTime.UtcNow;
            var hours = timeSpan.TotalHours;

            return Math.Round(hours, 2);
        }

        public SmartTaskResponse ConvertToSmartResponse(TodoTask task)
        {
            var hoursRemaining = CalculateHoursRemaining(task.Deadline);
            var urgencyScore = CalculateUrgencyScore(task);
            var status = DetermineStatus(urgencyScore);

            return new SmartTaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Deadline = task.Deadline,
                Importance = task.Importance,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                UrgencyScore = urgencyScore,
                Status = status,
                HoursRemaining = hoursRemaining
            };
        }
    }
}