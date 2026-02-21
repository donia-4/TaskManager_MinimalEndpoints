using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Endpoints;
using TaskManager.Interfaces;
using TaskManager.Services;

var builder = WebApplication.CreateBuilder(args);

// ================== Database ==================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// ================== Services ==================
builder.Services.AddScoped<IPriorityService, PriorityService>();
builder.Services.AddScoped<ITaskService, TaskService>();

// ================== Swagger ==================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ================== Middleware ==================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ================== Endpoints ==================
app.MapTaskEndpoints();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
    await DbSeeder.SeedAsync(context);
}

app.Run();