using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Dtos.Requests;
using TaskManager.Dtos.Responses;
using TaskManager.Interfaces;
using TaskManager.Models;

namespace TaskManager.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;
        private readonly IPriorityService _priorityService;

        public TaskService(AppDbContext context, IPriorityService priorityService)
        {
            _context = context;
            _priorityService = priorityService;
        }

        // =================================================================
        // SMART ENGINE METHODS
        // =================================================================

        public async Task<PagedResponse<SmartTaskResponse>> GetSmartQueueAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var tasks = await _context.Tasks
                    .AsNoTracking()
                    .Where(t => !t.IsCompleted)
                    .ToListAsync();

                var smartQuery = tasks
                    .Select(task => _priorityService.ConvertToSmartResponse(task))
                    .OrderByDescending(st => st.UrgencyScore);

                var totalRecords = smartQuery.Count();

                var pagedData = smartQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new PagedResponse<SmartTaskResponse>
                {
                    Data = pagedData,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
                };
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the smart queue.", ex);
            }
        }

        public async Task<SmartTaskResponse?> GetNextTaskAsync()
        {
            try
            {
                var tasks = await _context.Tasks
                    .AsNoTracking()
                    .Where(t => !t.IsCompleted)
                    .ToListAsync();

                if (!tasks.Any())
                    return null;

                var nextTask = tasks
                    .Select(task => _priorityService.ConvertToSmartResponse(task))
                    .OrderByDescending(st => st.UrgencyScore)
                    .FirstOrDefault();

                return nextTask;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the next task.", ex);
            }
        }

        public async Task<TaskStatsResponse> GetStatsAsync()
        {
            try
            {
                var allTasks = await _context.Tasks.AsNoTracking().ToListAsync();
                var pendingTasks = allTasks.Where(t => !t.IsCompleted).ToList();

                var smartTasks = pendingTasks
                    .Select(task => _priorityService.ConvertToSmartResponse(task))
                    .ToList();

                var stats = new TaskStatsResponse
                {
                    TotalTasks = allTasks.Count,
                    CompletedTasks = allTasks.Count(t => t.IsCompleted),
                    PendingTasks = pendingTasks.Count,
                    CriticalTasks = smartTasks.Count(st => st.Status == "Critical"),
                    OverdueTasks = smartTasks.Count(st => st.HoursRemaining <= 0),
                    AverageUrgencyScore = smartTasks.Any()
                        ? Math.Round(smartTasks.Average(st => st.UrgencyScore), 2)
                        : 0
                };

                return stats;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving statistics.", ex);
            }
        }

        // =================================================================
        // CRUD METHODS
        // =================================================================

        public async Task<PagedResponse<TaskResponse>> GetAllTasksAsync(int pageNumber,int pageSize,bool includeCompleted)
        {
            try
            {
                // Validate pagination parameters
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100; // Max page size

                var query = _context.Tasks.AsNoTracking();

                // Apply filter for completed tasks
                if (!includeCompleted)
                {
                    query = query.Where(t => !t.IsCompleted);
                }

                // Get total count before pagination
                var totalRecords = await query.CountAsync();

                // Apply pagination
                var tasks = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Map to response DTOs
                var taskResponses = tasks.Select(MapToTaskResponse).ToList();

                return new PagedResponse<TaskResponse>
                {
                    Data = taskResponses,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
                };
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving tasks.", ex);
            }
        }

        public async Task<TaskResponse?> GetTaskByIdAsync(int id)
        {
            try
            {
                var task = await _context.Tasks
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id);

                return task != null ? MapToTaskResponse(task) : null;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving task with ID {id}.", ex);
            }
        }

        public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request)
        {
            try
            {
                // Validate deadline is in the future
                if (request.Deadline <= DateTime.UtcNow)
                {
                    throw new ArgumentException("Deadline must be in the future.");
                }

                var task = new TodoTask
                {
                    Title = request.Title.Trim(),
                    Description = request.Description?.Trim(),
                    Deadline = request.Deadline,
                    Importance = request.Importance,
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                return MapToTaskResponse(task);
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while creating the task.", ex);
            }
        }

        public async Task<TaskResponse?> UpdateTaskAsync(int id, UpdateTaskRequest request)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);

                // Not found is a normal outcome, not an exception
                if (task == null)
                {
                    return null;
                }

                // Update only provided fields
                if (!string.IsNullOrWhiteSpace(request.Title))
                {
                    task.Title = request.Title.Trim();
                }

                if (request.Description != null)
                {
                    task.Description = request.Description.Trim();
                }

                if (request.Deadline.HasValue)
                {
                    if (request.Deadline.Value <= DateTime.UtcNow)
                    {
                        throw new ArgumentException("Deadline must be in the future.");
                    }
                    task.Deadline = request.Deadline.Value;
                }

                if (request.Importance.HasValue)
                {
                    task.Importance = request.Importance.Value;
                }

                await _context.SaveChangesAsync();

                return MapToTaskResponse(task);
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating task with ID {id}.", ex);
            }
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            try
            {
                var rowsAffected = await _context.Tasks
                    .Where(t => t.Id == id)
                    .ExecuteDeleteAsync();

                // Return false if not found (0 rows affected), true if deleted
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while deleting task with ID {id}.", ex);
            }
        }

        // =================================================================
        // WORKFLOW METHODS
        // =================================================================

        public async Task<bool?> MarkCompleteAsync(int id)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);

                // Not found is a normal outcome
                if (task == null)
                {
                    return null;
                }

                // Already completed is a validation issue
                if (task.IsCompleted)
                {
                    throw new InvalidOperationException("Task is already completed.");
                }

                task.IsCompleted = true;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while marking task {id} as complete.", ex);
            }
        }

        public async Task<bool?> ReopenTaskAsync(int id)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(id);

                // Not found is a normal outcome
                if (task == null)
                {
                    return null;
                }

                // Not completed is a validation issue
                if (!task.IsCompleted)
                {
                    throw new InvalidOperationException("Task is not completed.");
                }

                task.IsCompleted = false;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while reopening task {id}.", ex);
            }
        }

        // =================================================================
        // HELPER METHODS
        // =================================================================

        private static TaskResponse MapToTaskResponse(TodoTask task)
        {
            return new TaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Deadline = task.Deadline,
                Importance = task.Importance,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt
            };
        }
    }
}