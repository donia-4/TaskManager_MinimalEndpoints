# Smart Task Manager API

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Minimal APIs](https://img.shields.io/badge/Minimal%20APIs-ASP.NET%20Core-512BD4)](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4)](https://docs.microsoft.com/en-us/ef/core/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A **simple REST API** built with **.NET Minimal APIs** that implements dynamic task prioritization using real-time urgency scoring. Tasks automatically re-prioritize as deadlines approach, ensuring you always work on what matters most.

---

## 🎯 The Core Idea

### The Problem
Traditional to-do apps use static priorities (High, Medium, Low). A "High Priority" task due in 30 days will always rank higher than a "Medium Priority" task due in 2 hours, even though the latter is more urgent.

### The Solution
Dynamic urgency scoring that considers both **importance** and **time remaining**:

```
UrgencyScore = (Importance × 1.5) / (HoursRemaining + 0.1)
```

**Result:** Tasks with approaching deadlines automatically rise to the top, regardless of static importance!

---

## ✨ Features

### 🧠 Smart Prioritization
- **Dynamic Queue**: Tasks re-sort automatically as time passes
- **Urgency Algorithm**: Balances importance with deadline proximity
- **Status Classification**: Auto-categorizes as Critical, High, Normal, or Low
- **Next Task Suggestion**: "What should I do next?" endpoint

### 📊 Pagination
- Smart queue pagination (handle thousands of tasks)
- Standard task list pagination
- Customizable page sizes (max 100 items)

### 🔧 REST API
- Full CRUD operations (Create, Read, Update, Delete)
- Task workflow (Mark complete, Reopen)
- Statistics endpoint (analytics & insights)
- Proper HTTP status codes

### 📖 Developer Experience
- **Swagger UI**: Interactive API documentation
- **Auto-migrations**: Database setup on first run
- **Seed data**: Sample tasks for testing

---

## 📁 Project Structure

```
TaskManager/
├── Program.cs                          # App entry point & configuration
├── appsettings.json                    # Configuration file
│
├── Data/
│   ├── AppDbContext.cs                 # EF Core context
│   └── DbSeeder.cs                     # Sample data seeder
│
├── Models/
│   └── TodoTask.cs                     # Task entity
│
├── Dtos/
│   ├── Requests/
│   │   ├── CreateTaskRequest.cs       # Create DTO
│   │   └── UpdateTaskRequest.cs       # Update DTO
│   └── Responses/
│       ├── TaskResponse.cs            # Basic task DTO
│       ├── SmartTaskResponse.cs       # Task with urgency score
│       ├── TaskStatsResponse.cs       # Statistics DTO
│       └── PagedResponse<T>.cs        # Pagination wrapper
│
├── Interfaces/
│   ├── ITaskService.cs                # Task service contract
│   └── IPriorityService.cs            # Priority service contract
│
├── Services/
│   ├── TaskService.cs                 # Business logic
│   └── PriorityService.cs             # Urgency calculations
│
├── Endpoints/
│   └── TaskEndpoints.cs               # API route definitions
│
└── Migrations/                         # EF Core migrations
```

**Simple & Organized:**
- No complex layers or abstractions
- Straightforward folder structure
- Easy to navigate and understand

---

## 🚀 Quick Start

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) (Express or higher)

### Installation

1. **Clone & Navigate**
   ```bash
   git clone https://github.com/yourusername/smart-task-manager.git
   cd smart-task-manager
   ```

2. **Update Connection String**
   
   Edit `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=.;Database=TaskManagerDb;Trusted_Connection=True;TrustServerCertificate=True"
     }
   }
   ```

3. **Run**
   ```bash
   dotnet run
   ```
   
   The app will:
   - ✅ Create the database
   - ✅ Run migrations
   - ✅ Seed sample data
   - ✅ Start the API

4. **Open Swagger**
   ```
   https://localhost:5001/swagger
   ```

That's it! 🎉

---

## 📖 API Endpoints

### Base URL
```
https://localhost:5001/api/tasks
```

### Endpoint Reference

| Method | Endpoint | Description | Paginated |
|--------|----------|-------------|-----------|
| **Smart Engine** ||||
| GET | `/smart-queue` | Get tasks sorted by urgency | ✅ |
| GET | `/suggest-next` | Get highest priority task | - |
| GET | `/stats` | Get task statistics | - |
| **CRUD Operations** ||||
| GET | `/` | Get all tasks | ✅ |
| GET | `/{id}` | Get task by ID | - |
| POST | `/` | Create new task | - |
| PUT | `/{id}` | Update task | - |
| DELETE | `/{id}` | Delete task | - |
| **Workflow** ||||
| PATCH | `/{id}/complete` | Mark task complete | - |
| PATCH | `/{id}/reopen` | Reopen completed task | - |

---

## 💡 Usage Examples

### Get Smart Queue (Paginated)

```http
GET /api/tasks/smart-queue?pageNumber=1&pageSize=10
```

**Response:**
```json
{
  "data": [
    {
      "id": 1,
      "title": "Fix critical bug",
      "urgencyScore": 125.5,
      "status": "Critical",
      "hoursRemaining": 2.5,
      "importance": 10,
      "deadline": "2025-02-15T18:00:00Z"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalRecords": 15,
  "totalPages": 2,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### Create a Task

```http
POST /api/tasks
Content-Type: application/json

{
  "title": "Deploy to production",
  "description": "Deploy v2.0 with new features",
  "deadline": "2025-02-20T10:00:00Z",
  "importance": 9
}
```

**Response (201 Created):**
```json
{
  "id": 16,
  "title": "Deploy to production",
  "description": "Deploy v2.0 with new features",
  "deadline": "2025-02-20T10:00:00Z",
  "importance": 9,
  "isCompleted": false,
  "createdAt": "2025-02-11T14:30:00Z"
}
```

### Get Next Task

```http
GET /api/tasks/suggest-next
```

**Response:**
```json
{
  "id": 1,
  "title": "Fix critical bug",
  "urgencyScore": 125.5,
  "status": "Critical",
  "hoursRemaining": 2.5
}
```

---

## 🧮 How the Algorithm Works

### Urgency Score Formula

```
UrgencyScore = (Importance × Weight) / (HoursRemaining + Buffer)

Constants:
- Weight = 1.5 (amplifies urgency effect)
- Buffer = 0.1 (prevents division by zero)
```

### Status Thresholds

| Score | Status | Color | Meaning |
|-------|--------|-------|---------|
| ≥ 80 | Critical | 🔴 | Drop everything! |
| 50-79 | High | 🟠 | Do soon |
| 20-49 | Normal | 🟡 | Schedule it |
| < 20 | Low | 🟢 | No rush |

### Special Cases

- **Overdue tasks**: Assigned score of **999** (maximum priority)
- **Tasks due very soon**: Buffer ensures smooth calculation even at < 1 hour

### Real Example

```
Task A: Importance 10, Due in 30 days (720 hours)
Score = (10 × 1.5) / (720 + 0.1) = 0.02

Task B: Importance 5, Due in 2 hours
Score = (5 × 1.5) / (2 + 0.1) = 3.57

Result: Task B ranks 178x higher! ✅
```

---

## 🛠️ Development

### Run with Auto-Reload
```bash
dotnet watch run
```

### Create Migration
```bash
dotnet ef migrations add YourMigrationName
```

### Apply Migration
```bash
dotnet ef database update
```

### Build for Production
```bash
dotnet publish -c Release -o ./publish
```

---

## ⚙️ Configuration

### Program.cs Structure

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Services (Dependency Injection)
builder.Services.AddScoped<IPriorityService, PriorityService>();
builder.Services.AddScoped<ITaskService, TaskService>();

// 3. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 5. Endpoints
app.MapTaskEndpoints();

// 6. Auto-migrate & Seed
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
    await DbSeeder.SeedAsync(context);
}

app.Run();
```

**Simple & Clear:**
- No complex startup classes
- No excessive middleware
- Everything in one file

---

## 📊 Database Schema

### TodoTask Table

```sql
CREATE TABLE [dbo].[Tasks] (
    [Id]           INT            IDENTITY(1,1) PRIMARY KEY,
    [Title]        NVARCHAR(200)  NOT NULL,
    [Description]  NVARCHAR(1000) NULL,
    [Deadline]     DATETIME2      NOT NULL,
    [Importance]   INT            NOT NULL CHECK (Importance >= 1 AND Importance <= 10),
    [IsCompleted]  BIT            NOT NULL DEFAULT 0,
    [CreatedAt]    DATETIME2      NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX IX_Tasks_IsCompleted ON [dbo].[Tasks] (IsCompleted);
CREATE INDEX IX_Tasks_Deadline ON [dbo].[Tasks] (Deadline);
```

---

## 🧪 Testing with cURL

### Create Task
```bash
curl -X POST https://localhost:5001/api/tasks \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test task",
    "deadline": "2025-02-20T10:00:00Z",
    "importance": 5
  }'
```

### Get Smart Queue
```bash
curl "https://localhost:5001/api/tasks/smart-queue?pageNumber=1&pageSize=5"
```

### Mark Complete
```bash
curl -X PATCH https://localhost:5001/api/tasks/1/complete
```

### Get Statistics
```bash
curl https://localhost:5001/api/tasks/stats
```

---

## 🎓 What I Learnt

Building this project teaches:

1. **Minimal APIs**: Modern, lightweight API development
2. **Entity Framework Core**: Database operations with LINQ
3. **Dependency Injection**: Service registration and lifetime management
4. **DTOs**: Separating API models from database entities
5. **Pagination**: Efficient data retrieval for large datasets
6. **Swagger/OpenAPI**: API documentation and testing
7. **Exception Handling**: Proper error responses without custom exceptions for "not found"
8. **Algorithm Design**: Mathematical models for real-world problems
9. **REST Principles**: Proper HTTP verbs and status codes

---

## 🔒 Exception Handling Approach

### Not Found is NOT an Exception

```csharp
// ✅ CORRECT
public async Task<TaskResponse?> GetTaskByIdAsync(int id)
{
    var task = await _context.Tasks.FindAsync(id);
    return task != null ? MapToTaskResponse(task) : null;
}
```

### When to Throw Exceptions

| Scenario | Approach | Status Code |
|----------|----------|-------------|
| Task not found | Return `null` or `false` | 404 |
| Invalid deadline | Throw `ArgumentException` | 400 |
| Already completed | Throw `InvalidOperationException` | 400 |
| Database error | Throw `Exception` | 500 |

**Principle:** "404 Not Found is a successful HTTP response to a valid request for a non-existent resource."

---

## 🚧 Possible Enhancements

- [ ] Add JWT authentication
- [ ] Add task categories/tags
- [ ] Support recurring tasks
- [ ] Email notifications for critical tasks
- [ ] Task dependencies
- [ ] Time tracking
- [ ] Subtasks
- [ ] Full-text search
- [ ] Export to CSV/JSON

---

## 📝 Notes

### Why Minimal APIs?

**Pros:**
- ✅ Less boilerplate code
- ✅ Faster development
- ✅ Easy to understand
- ✅ Performance-optimized
- ✅ Great for small to medium APIs

**When to use:**
- Simple REST APIs
- Microservices
- Learning projects
- Proof of concepts

**When NOT to use:**
- Large enterprise applications with complex business logic
- Need for extensive middleware customization
- Team prefers MVC/Controller pattern

### Why Not Clean Architecture?

This project intentionally avoids:
- ❌ Multiple project layers (Domain, Application, Infrastructure)
- ❌ Complex abstractions (Unit of Work, Repository patterns)
- ❌ Excessive interfaces and mappers

**Reason:** For an API of this size, Clean Architecture adds complexity without proportional benefit. The current structure is:
- ✅ Easy to understand
- ✅ Easy to modify
- ✅ Testable
- ✅ Maintainable

---

## 📄 License

feel free to use this for learning or production!

---

## 🤝 Contributing

Contributions welcome! This is a learning-focused project, so:
- Keep it simple
- Comment your code
- Update documentation
- Follow existing patterns

---

## 📧 Contact

Your Name - [Donia Shaban](https://www.linkedin.com/in/donia-shaban/) - doniashaban723@gmail.com

Project Link: [https://github.com/donia4/TaskManager_MinimalEndpoints/](https://github.com/donia-4/TaskManager_MinimalEndpoints/)

---

## 🎯 Perfect For

- Learning **.NET Minimal APIs**
- Understanding **dynamic algorithms**
- Practicing **REST API design**

---

<div align="center">

**A simple, practical API that solves a real problem** 🚀

Built for developers who value **simplicity over complexity**

⭐ Star this repo if you found it helpful!

</div>
