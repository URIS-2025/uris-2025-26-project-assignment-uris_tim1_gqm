using FluentValidation;
using GoalService.API.Middleware;
using GoalService.Application.Interfaces;
using GoalService.Application.Services;
using GoalService.Infrastructure.Persistence;
using GoalService.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Shared.HMAC;

var builder = WebApplication.CreateBuilder(args);

// --- Database ---
var connectionString = builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string not configured.");

builder.Services.AddDbContext<GoalDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- Application Services ---
builder.Services.AddScoped<IGoalService, GoalServiceImpl>();
builder.Services.AddScoped<IStrategyService, StrategyServiceImpl>();
builder.Services.AddScoped<IGoalInfluenceService, GoalInfluenceServiceImpl>();

// --- FluentValidation ---
builder.Services.AddValidatorsFromAssemblyContaining<GoalServiceImpl>();

// --- OpenAPI / Swagger ---
builder.Services.AddOpenApi();

// --- HMAC Authentication ---
var hmacSecretKey = builder.Configuration["HMAC_SECRET_KEY"]
    ?? throw new InvalidOperationException("HMAC_SECRET_KEY not configured");
builder.Services.AddHmacAuthentication(hmacSecretKey);
builder.Services.AddTransient<HmacDelegatingHandler>();

// --- Controllers ---
builder.Services.AddControllers();

var app = builder.Build();

// --- Global Exception Handler (first in pipeline) ---
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// --- Seed Data & Swagger (development only) ---
if (app.Environment.IsDevelopment())
{
    await GoalDbSeeder.SeedAsync(app.Services);
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// --- HMAC Middleware ---
app.UseMiddleware<HmacMiddleware>();

// --- Map Controllers ---
app.MapControllers();

// --- Health Check ---
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "goal-service" }))
    .WithName("HealthCheck");

app.Run();
