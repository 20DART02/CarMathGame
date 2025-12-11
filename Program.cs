using CarMathGame.Data;
using CarMathGame.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Information);
});

// Add DbContext with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Using connection string: {connectionString?.Substring(0, Math.Min(50, connectionString?.Length ?? 0))}...");

builder.Services.AddDbContext<GameDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.UseSnakeCaseNamingConvention();
    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
});

// Add Game Service
builder.Services.AddScoped<IMathGameService, MathGameService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Create database if it doesn't exist
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Attempting to connect to database...");

        // Test connection
        var canConnect = await dbContext.Database.CanConnectAsync();
        logger.LogInformation($"Database connection: {(canConnect ? "SUCCESS" : "FAILED")}");

        if (canConnect)
        {
            // Apply any pending migrations
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully!");

            // Check tables
            var playersCount = await dbContext.Players.CountAsync();
            var sessionsCount = await dbContext.GameSessions.CountAsync();

            logger.LogInformation($"Database contains {playersCount} players and {sessionsCount} game sessions.");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database initialization failed: {ex.Message}");
}

app.Run();