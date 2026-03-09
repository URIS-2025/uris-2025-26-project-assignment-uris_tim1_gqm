using FluentValidation;
using GoalService.Application.Interfaces;
using GoalService.Application.Interfaces.Clients;
using GoalService.Infrastructure.Clients;
using GoalService.Infrastructure.Persistence;
using GoalService.Application.Interfaces.Persistence;
using GoalService.Application.Services;
using GoalService.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Shared.Auth;
using Shared.ErrorHandling;
using Shared.HMAC;

var builder = WebApplication.CreateBuilder(args);

// --- Database ---
var connectionString = builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string not configured.");

builder.Services.AddDbContext<GoalDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IGoalDbContext>(provider => provider.GetRequiredService<GoalDbContext>());

// --- Application Services ---
builder.Services.AddScoped<IGoalService, GoalServiceImpl>();
builder.Services.AddScoped<IStrategyService, StrategyServiceImpl>();
builder.Services.AddScoped<IGoalInfluenceService, GoalInfluenceServiceImpl>();

// --- FluentValidation ---
builder.Services.AddValidatorsFromAssemblyContaining<GoalServiceImpl>();

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

// --- Correlation ID ---
builder.Services.AddCorrelationId();

// --- Cross-Service HTTP Clients ---
builder.Services.AddHttpClient<IPremiseClient, PremiseClient>(client =>
{
    var baseUrl = builder.Configuration["Services:PremiseService"] ?? "http://premise-service:8080";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<HmacDelegatingHandler>()
  .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

builder.Services.AddHttpClient<IAssessmentClient, AssessmentClient>(client =>
{
    var baseUrl = builder.Configuration["Services:AssessmentService"] ?? "http://assessment-service:8080";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<HmacDelegatingHandler>()
  .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

builder.Services.AddHttpClient<IQgmGoalClient, QgmGoalClient>(client =>
{
    var baseUrl = builder.Configuration["Services:QgmGoalService"] ?? "http://gqm-goal-service:8080";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<HmacDelegatingHandler>()
  .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

builder.Services.AddHttpClient<IOrchestrationClient, OrchestrationClient>(client =>
{
    var baseUrl = builder.Configuration["Services:OrchestrationService"] ?? "http://orchestration-service:8080";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<HmacDelegatingHandler>()
  .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

builder.Services.AddHttpClient<IAuditClient, AuditClient>(client =>
{
    var baseUrl = builder.Configuration["Services:AuditService"] ?? "http://audit-service:8080";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<HmacDelegatingHandler>()
  .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

builder.Services.AddHttpClient<IDepartmentClient, DepartmentClient>(client =>
{
    var baseUrl = builder.Configuration["Services:DepartmentService"] ?? "http://department-service:8080";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<HmacDelegatingHandler>()
  .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

builder.Services.AddHttpContextAccessor();

// --- Controllers ---
builder.Services.AddControllers();

var app = builder.Build();

// --- Correlation ID & Global Exception Handler (first in pipeline) ---
app.UseCorrelationId();
app.UseStandardizedExceptionHandler();

// --- Seed Data & Swagger (development only) ---
if (app.Environment.IsDevelopment())
{
    await GoalDbSeeder.SeedAsync(app.Services);
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Goal Service API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

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
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "goal-service" }))
    .WithName("HealthCheck");

app.Run();
