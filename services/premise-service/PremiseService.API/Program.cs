using Microsoft.EntityFrameworkCore;
using PremiseService.API.Extensions;
using PremiseService.API.Middleware;
using PremiseService.Infrastructure.Persistence;
using PremiseService.Infrastructure.Seed;
using Shared.HMAC;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// OpenAPI / Swagger
builder.Services.AddOpenApi();

// Database
builder.Services.AddPersistence(builder.Configuration);

// Application services (AutoMapper, FluentValidation, services, repositories)
builder.Services.AddApplicationServices();

// HMAC authentication
var hmacSecretKey = builder.Configuration["HMAC_SECRET_KEY"]
    ?? throw new InvalidOperationException("HMAC_SECRET_KEY not configured");
builder.Services.AddHmacAuthentication(hmacSecretKey);
builder.Services.AddTransient<HmacDelegatingHandler>();

var app = builder.Build();

// Apply migrations and seed data on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PremiseDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migration applied successfully.");

        await PremiseSeeder.SeedAsync(dbContext, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Custom exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// HMAC middleware for service-to-service authentication
app.UseMiddleware<HmacMiddleware>();

app.MapControllers();

app.Run();
