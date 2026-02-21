using TaskManager.Models;

namespace TaskManager.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (context.Tasks.Any())
                return; // Database already seeded

            var random = new Random();
            var tasks = new List<TodoTask>();

            for (int i = 1; i <= 50; i++)
            {
                var importance = random.Next(1, 11); // 1 - 10
                var daysToAdd = random.Next(-3, 15);  // some overdue, some future
                var deadline = DateTime.UtcNow.AddDays(daysToAdd);

                tasks.Add(new TodoTask
                {
                    Title = $"Task #{i}",
                    Description = $"Auto-generated task number {i}",
                    Deadline = deadline,
                    Importance = importance,
                    IsCompleted = random.Next(0, 2) == 1,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 30))
                });
            }

            await context.Tasks.AddRangeAsync(tasks);
            await context.SaveChangesAsync();
        }
    }
}