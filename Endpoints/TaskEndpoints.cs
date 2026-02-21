using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Dtos.Requests;
using TaskManager.Dtos.Responses;
using TaskManager.Interfaces;

namespace TaskManager.Endpoints;

public static class TaskEndpoints
{
    public static RouteGroupBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var taskApi = app.MapGroup("/api/tasks");

        // =================================================================
        // 1. SMART ENGINE ROUTES
        // =================================================================
        taskApi.MapGet("smart-queue", GetSmartQueue)
            .WithName("GetSmartQueue")
            .WithSummary("Get dynamically prioritized task queue")
            .WithDescription(@"
                Returns all pending tasks sorted by dynamic urgency score. The score is calculated using:
                        **Formula**: Score = (Importance × 1.5) / (Hours Remaining + 0.1)

                        Tasks with approaching deadlines automatically rise to the top, regardless of their static importance level.
                        This ensures you're always working on the most time-sensitive task.")
            .Produces<List<SmartTaskResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        taskApi.MapGet("suggest-next", GetNextTask)
            .WithName("GetNextTask")
            .WithSummary("Get the single highest-priority task")
            .WithDescription(@"
                Returns only the top-priority task based on urgency score. 
                Perfect for a 'What should I do next?' feature to eliminate decision paralysis.
                Returns 404 if no pending tasks exist.")
            .Produces<SmartTaskResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        taskApi.MapGet("stats", GetStats)
            .WithName("GetStats")
            .WithSummary("Get task statistics and analytics")
            .WithDescription(@"
                Provides an overview of task metrics:
                - Total tasks (completed + pending)
                - Critical tasks (urgency score > 80)
                - Overdue tasks (past deadline)
                - Average urgency score

                Useful for dashboard widgets and productivity insights.")
            .Produces<TaskStatsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        // =================================================================
        // 2. STANDARD CRUD ROUTES
        // =================================================================
        taskApi.MapGet("", GetAllTasks)
            .WithName("GetAllTasks")
            .WithSummary("Get paginated list of tasks")
            .WithDescription(@"
                Retrieves tasks with pagination support. Defaults to showing only pending tasks.

                **Query Parameters:**
                - `pageNumber`: Page to retrieve (default: 1)
                - `pageSize`: Items per page (default: 10, max: 100)
                - `includeCompleted`: Include completed tasks (default: false)")
            .Produces<PagedResponse<TaskResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        taskApi.MapGet("{id:int}", GetTaskById)
            .WithName("GetTaskById")
            .WithSummary("Get a specific task by ID")
            .WithDescription("Retrieves detailed information for a single task. Returns 404 if task doesn't exist.")
            .Produces<TaskResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        taskApi.MapPost("", CreateTask)
            .WithName("CreateTask")
            .WithSummary("Create a new task")
            .WithDescription(@"
                Creates a new task with validation:
                - **Title**: Required, 1-200 characters
                - **Description**: Optional, max 1000 characters
                - **Deadline**: Must be in the future
                - **Importance**: Required, range 1-10 (1=low, 10=critical)

                Returns 201 Created with Location header pointing to the new task.")
            .Produces<TaskResponse>(StatusCodes.Status201Created)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        taskApi.MapPut("{id:int}", UpdateTask)
            .WithName("UpdateTask")
            .WithSummary("Update an existing task")
            .WithDescription(@"
                Updates task properties (partial update supported - only provided fields are updated):
                - Title, Description, Deadline, or Importance
                - Validates that deadlines remain in the future
                - Returns 404 if task doesn't exist")
            .Produces<TaskResponse>(StatusCodes.Status200OK)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        taskApi.MapDelete("{id:int}", DeleteTask)
            .WithName("DeleteTask")
            .WithSummary("Delete a task permanently")
            .WithDescription("Permanently removes a task from the system. Returns 204 No Content on success, 404 if task doesn't exist.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        // =================================================================
        // 3. WORKFLOW ROUTES
        // =================================================================
        taskApi.MapPatch("{id:int}/complete", MarkComplete)
            .WithName("MarkComplete")
            .WithSummary("Mark a task as completed")
            .WithDescription(@"
                Changes task status to completed.
                - Returns 404 if task doesn't exist
                - Returns 400 if task is already completed")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        taskApi.MapPatch("{id:int}/reopen", ReopenTask)
            .WithName("ReopenTask")
            .WithSummary("Reopen a completed task")
            .WithDescription(@"
                Changes task status back to pending (uncompletes it).
                - Returns 404 if task doesn't exist
                - Returns 400 if task is not completed")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        return taskApi;
    }

    // =================================================================
    // SMART ENGINE HANDLERS
    // =================================================================

    private static async Task<IResult> GetSmartQueue(ITaskService taskService,[FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 10)
    {
        try
        {
            var response = await taskService.GetSmartQueueAsync(pageNumber, pageSize);
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving smart queue",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    private static async Task<IResult> GetNextTask(ITaskService taskService)
    {
        try
        {
            var nextTask = await taskService.GetNextTaskAsync();

            if (nextTask == null)
            {
                return Results.NotFound(new { message = "No pending tasks found" });
            }

            return Results.Ok(nextTask);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving next task",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    private static async Task<IResult> GetStats(ITaskService taskService)
    {
        try
        {
            var stats = await taskService.GetStatsAsync();
            return Results.Ok(stats);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving statistics",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    // =================================================================
    // STANDARD CRUD HANDLERS
    // =================================================================

    private static async Task<IResult> GetAllTasks(
        ITaskService taskService,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeCompleted = false)
    {
        try
        {
            var pagedTasks = await taskService.GetAllTasksAsync(pageNumber, pageSize, includeCompleted);
            return Results.Ok(pagedTasks);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error retrieving tasks",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    private static async Task<Results<Ok<TaskResponse>, NotFound, ProblemHttpResult>> GetTaskById(int id,ITaskService taskService)
    {
        try
        {
            var task = await taskService.GetTaskByIdAsync(id);

            return task is not null
                ? TypedResults.Ok(task)
                : TypedResults.NotFound();
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                title: "Error retrieving task",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    private static async Task<IResult> CreateTask(
        CreateTaskRequest request,
        ITaskService taskService)
    {
        try
        {
            var createdTask = await taskService.CreateTaskAsync(request);

            return Results.CreatedAtRoute(
                "GetTaskById",
                new { id = createdTask.Id },
                createdTask
            );
        }
        catch (ArgumentException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "Deadline", new[] { ex.Message } }
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error creating task",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    private static async Task<IResult> UpdateTask(
        int id,
        UpdateTaskRequest request,
        ITaskService taskService)
    {
        try
        {
            var updatedTask = await taskService.UpdateTaskAsync(id, request);

            // Service returns null if task not found - normal outcome
            if (updatedTask == null)
            {
                return Results.NotFound(new { message = $"Task with ID {id} was not found." });
            }

            return Results.Ok(updatedTask);
        }
        catch (ArgumentException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "Deadline", new[] { ex.Message } }
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error updating task",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    private static async Task<IResult> DeleteTask(
        int id,
        ITaskService taskService)
    {
        try
        {
            var deleted = await taskService.DeleteTaskAsync(id);

            // Service returns false if task not found - normal outcome
            if (!deleted)
            {
                return Results.NotFound(new { message = $"Task with ID {id} was not found." });
            }

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error deleting task",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    // =================================================================
    // WORKFLOW HANDLERS
    // =================================================================

    private static async Task<IResult> MarkComplete(
        int id,
        ITaskService taskService)
    {
        try
        {
            var result = await taskService.MarkCompleteAsync(id);

            // Service returns null if task not found - normal outcome
            if (result == null)
            {
                return Results.NotFound(new { message = $"Task with ID {id} was not found." });
            }

            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "IsCompleted", new[] { ex.Message } }
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error marking task as complete",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    private static async Task<IResult> ReopenTask(
        int id,
        ITaskService taskService)
    {
        try
        {
            var result = await taskService.ReopenTaskAsync(id);

            // Service returns null if task not found - normal outcome
            if (result == null)
            {
                return Results.NotFound(new { message = $"Task with ID {id} was not found." });
            }

            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "IsCompleted", new[] { ex.Message } }
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Error reopening task",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}