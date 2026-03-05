using AssessmentService.API.Middleware;
using AssessmentService.Application.Interfaces;
using AssessmentService.Application.Interfaces.Clients;
using AssessmentService.Application.Services;
using AssessmentService.Application.Validators;
using AssessmentService.Infrastructure.Clients;
using AssessmentService.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shared.HMAC;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI with XML comments
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// Add HMAC authentication
var hmacSecretKey = builder.Configuration["HMAC_SECRET_KEY"]
    ?? throw new InvalidOperationException("HMAC_SECRET_KEY not configured");
builder.Services.AddHmacAuthentication(hmacSecretKey);
builder.Services.AddTransient<HmacDelegatingHandler>();

builder.Services.AddScoped<IAssessmentService, AssessmentServiceImpl>();

var connectionString = builder.Configuration["DATABASE_URL"]
    ?? throw new InvalidOperationException("DATABASE_URL is not configured.");

builder.Services.AddDbContext<AssessmentDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IAssessmentDbContext>(sp =>
    sp.GetRequiredService<AssessmentDbContext>());


builder.Services.AddValidatorsFromAssemblyContaining<CreateAssessmentValidator>();

// Goal client (HTTP)
builder.Services.AddHttpClient<IGoalClient, GoalClient>(client =>
{
    var baseUrl = builder.Configuration["Services:GoalService"];
    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<HmacDelegatingHandler>();

builder.Services.AddHttpClient<IOrchestrationClient, OrchestrationClient>(client =>
{
    var baseUrl = builder.Configuration["Services:OrchestrationService"] ?? "http://orchestration-service:8080";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<HmacDelegatingHandler>();

builder.Services.AddHttpClient<IAuditClient, AuditClient>(client =>
{
    var baseUrl = builder.Configuration["Services:AuditService"] ?? "http://audit-service:8080";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<HmacDelegatingHandler>();

var app = builder.Build();

// Apply database schema and seed data in development
if (app.Environment.IsDevelopment())
{
    var retryCount = 0;
    const int maxRetries = 10;

    while (retryCount < maxRetries)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AssessmentDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
            await AssessmentSeeder.SeedAsync(dbContext);
            break;
        }
        catch (Exception ex)
        {
            retryCount++;
            app.Logger.LogWarning(ex, "Database not ready (attempt {Attempt}/{Max}). Retrying in 3 seconds...", retryCount, maxRetries);
            await Task.Delay(3000);
        }
    }
}

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("v1/swagger.json", "Assessment Service API v1");
    options.RoutePrefix = "swagger";
});

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<HmacMiddleware>();

app.MapControllers();

app.Run();
