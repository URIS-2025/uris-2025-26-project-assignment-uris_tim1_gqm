using DepartmentService.API.Middleware;
using DepartmentService.Application.Mappings;
using DepartmentService.Application.Validators;
using DepartmentService.Infrastructure;
using DepartmentService.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shared.Auth;
using Shared.HMAC;

var builder = WebApplication.CreateBuilder(args);

// --- Controllers ---
builder.Services.AddControllers();

// --- Swagger / OpenAPI ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- JWT Authentication & Authorization ---
builder.Services.AddJwtAuthentication(builder.Configuration);

// --- HMAC Authentication ---
var hmacSecretKey = builder.Configuration["HMAC_SECRET_KEY"]
    ?? throw new InvalidOperationException("HMAC_SECRET_KEY not configured");
builder.Services.AddHmacAuthentication(hmacSecretKey);
builder.Services.AddTransient<HmacDelegatingHandler>();

// --- Infrastructure (DbContext, services) ---
builder.Services.AddInfrastructure(builder.Configuration);

// --- AutoMapper ---
builder.Services.AddAutoMapper(typeof(OrganizationProfile).Assembly);

// --- FluentValidation ---
builder.Services.AddValidatorsFromAssemblyContaining<OrganizationRequestValidator>();

var app = builder.Build();

// --- Apply migrations and seed data ---
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DepartmentServiceDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying database migrations...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");

        await DataSeeder.SeedAsync(dbContext, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

// --- Global exception handler (first in pipeline) ---
app.UseMiddleware<GlobalExceptionHandler>();

// --- Swagger ---
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Department Service API v1");
    options.RoutePrefix = "swagger";
});

// --- Authentication & Authorization ---
app.UseAuthentication();
app.UseAuthorization();

// --- Organization Context ---
app.UseMiddleware<OrganizationContextMiddleware>();

// --- HMAC Middleware ---
app.UseMiddleware<HmacMiddleware>();

// --- Map Controllers ---
app.MapControllers();

// --- Health Check ---
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "department-service" }))
    .WithName("HealthCheck");

app.Run();

// Required for integration testing
public partial class Program { }
