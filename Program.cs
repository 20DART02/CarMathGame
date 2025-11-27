using CarMathGame.Data;
using CarMathGame.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure DbContext with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    // Fallback for Railway environment variables
    var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrEmpty(dbUrl))
    {
        connectionString = ConvertDatabaseUrlToConnectionString(dbUrl);
    }
    else
    {
        connectionString = "Host=maglev.proxy.rlwy.net;Port=54723;Database=railway;Username=postgres;Password=XyscxFeKBjexDiEvFsajYozNSGGYTuir;SSL Mode=Require;Trust Server Certificate=true;";
    }
}

Console.WriteLine($"Using connection string: {connectionString.Replace("Password=", "Password=****")}");

builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());

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

// Initialize database
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();

        // Wait for database to be available
        await dbContext.Database.CanConnectAsync();
        Console.WriteLine("Database connection successful!");

        // Apply migrations
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("Database migrations applied successfully!");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database initialization failed: {ex.Message}");
    Console.WriteLine($"Full error: {ex}");
}

// Get port from Railway environment variable
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");

// Helper method to convert Railway DATABASE_URL to connection string
static string ConvertDatabaseUrlToConnectionString(string databaseUrl)
{
    if (string.IsNullOrEmpty(databaseUrl))
        return string.Empty;

    var uri = new Uri(databaseUrl);
    var db = uri.AbsolutePath.Trim('/');
    var user = uri.UserInfo.Split(':')[0];
    var passwd = uri.UserInfo.Split(':')[1];
    var port = uri.Port > 0 ? uri.Port : 5432;

    return $"Host={uri.Host};Port={port};Database={db};Username={user};Password={passwd};SSL Mode=Require;Trust Server Certificate=true;";
}