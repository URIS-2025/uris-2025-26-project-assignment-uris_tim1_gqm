using Microsoft.EntityFrameworkCore;
using GQMGoalService.API.Extensions;
using GQMGoalService.API.Middleware;
using GQMGoalService.Infrastructure.Persistence;
using Shared.HMAC;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure infrastructure and application services
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();

// OpenAPI setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "GQM Goal Service API", Version = "v1" });
});

// Add HMAC authentication
var hmacSecretKey = builder.Configuration["HMAC_SECRET_KEY"] ?? "dev-secret-key-for-local";
builder.Services.AddHmacAuthentication(hmacSecretKey);
builder.Services.AddTransient<HmacDelegatingHandler>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/api/v1/gqm-goal/swagger/v1/swagger.json", "GQM Goal Service API v1");
        c.RoutePrefix = "swagger"; // Let's simplify this since it's going through nginx or direct, /swagger is standard
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!builder.Configuration.GetValue<bool>("UseInMemoryDatabase"))
    {
        // Add retry logic for database connection during startup
        var maxRetries = 5;
        var retryDelay = TimeSpan.FromSeconds(5);
        for (var i = 0; i < maxRetries; i++)
        {
            try
            {
                context.Database.Migrate();
                break;
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogWarning(ex, "Failed to connect to database. Retrying in {Delay}s...", retryDelay.TotalSeconds);
                if (i == maxRetries - 1) throw;
                await Task.Delay(retryDelay);
            }
        }
    }
    
    // Seed dev data
    await DataSeeder.SeedAsync(context);
}

app.UseHttpsRedirection();

// Add HMAC middleware
app.UseMiddleware<HmacMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
