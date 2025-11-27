using CarMathGame.Data;
using CarMathGame.Services;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure DbContext with PostgreSQL - SIMPLIFIED
var connectionString = "Host=maglev.proxy.rlwy.net;Port=54723;Database=railway;Username=postgres;Password=XyscxFeKBjexDiEvFsajYozNSGGYTuir;SSL Mode=Require;Trust Server Certificate=true;";

Console.WriteLine("Configuring database...");

builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseNpgsql(connectionString));

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

// SIMPLIFIED Database initialization - just try to connect
try
{
    Console.WriteLine("Testing database connection...");
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        var canConnect = await dbContext.Database.CanConnectAsync();
        Console.WriteLine($"Database connection test: {canConnect}");

        if (canConnect)
        {
            // Just ensure database exists, don't worry about tables yet
            await dbContext.Database.EnsureCreatedAsync();
            Console.WriteLine("Database ensured created!");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database init warning: {ex.Message}");
    // Don't crash the app if database fails
}

// Get port from Railway environment variable
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
Console.WriteLine($"Starting on port: {port}");

app.Run($"http://0.0.0.0:{port}");