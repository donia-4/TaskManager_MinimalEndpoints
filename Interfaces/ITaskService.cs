using TaskManager.Dtos.Requests;
using TaskManager.Dtos.Responses;
namespace TaskManager.Interfaces
{
    public interface ITaskService
    {
        // Smart Engine Methods
        Task<PagedResponse<SmartTaskResponse>> GetSmartQueueAsync(int pageNumber, int pageSize);
        Task<SmartTaskResponse?> GetNextTaskAsync();
        Task<TaskStatsResponse> GetStatsAsync();

        // CRUD Methods
        Task<PagedResponse<TaskResponse>> GetAllTasksAsync(int pageNumber, int pageSize, bool includeCompleted);
        Task<TaskResponse?> GetTaskByIdAsync(int id);
        Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request);
        Task<TaskResponse?> UpdateTaskAsync(int id, UpdateTaskRequest request);
        Task<bool> DeleteTaskAsync(int id);

        // Workflow Methods
        Task<bool?> MarkCompleteAsync(int id);
        Task<bool?> ReopenTaskAsync(int id);
    }
}