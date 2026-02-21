using TaskManager.Dtos.Responses;
using TaskManager.Models;

namespace TaskManager.Interfaces
{
    public interface IPriorityService
    {
        /// <summary>
        /// Calculates the urgency score for a task based on importance and time remaining
        /// Formula: Score = (Importance * Weight) / HoursRemaining
        /// </summary>
        double CalculateUrgencyScore(TodoTask task);

        /// <summary>
        /// Determines the status label based on urgency score
        /// Critical (>80), High (50-80), Normal (20-50), Low (<20)
        /// </summary>
        string DetermineStatus(double urgencyScore);

        /// <summary>
        /// Calculates hours remaining until deadline
        /// </summary>
        double CalculateHoursRemaining(DateTime deadline);

        /// <summary>
        /// Converts a TodoTask to SmartTaskResponse with calculated fields
        /// </summary>
        SmartTaskResponse ConvertToSmartResponse(TodoTask task);
    }
}