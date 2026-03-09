using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using FluentValidation;
using GoalService.API.Middleware;
using GoalService.Application.Interfaces;
using GoalService.Application.Interfaces.Clients;
using GoalService.Infrastructure.Clients;
using GoalService.Infrastructure.Persistence;
using GoalService.Application.Interfaces.Persistence;
using GoalService.Application.Services;
using GoalService.Infrastructure.Seed;
using MassTransit;
using Microsoft.EntityFrameworkCore;
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

// --- HMAC Authentication ---
var hmacSecretKey = builder.Configuration["HMAC_SECRET_KEY"]
    ?? throw new InvalidOperationException("HMAC_SECRET_KEY not configured");
builder.Services.AddHmacAuthentication(hmacSecretKey);
builder.Services.AddTransient<HmacDelegatingHandler>();

// --- Cross-Service HTTP Clients ---
builder.Services.AddHttpClient<IPremiseClient, PremiseClient>(client =>
{
    var baseUrl = builder.Configuration["Services:PremiseService"] ?? "http://premise-service:8080";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<HmacDelegatingHandler>();

builder.Services.AddHttpClient<IAssessmentClient, AssessmentClient>(client =>
{
    var baseUrl = builder.Configuration["Services:AssessmentService"] ?? "http://assessment-service:8080";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<HmacDelegatingHandler>();

builder.Services.AddHttpClient<IQgmGoalClient, QgmGoalClient>(client =>
{
    var baseUrl = builder.Configuration["Services:QgmGoalService"] ?? "http://gqm-goal-service:8080";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<HmacDelegatingHandler>();

// --- MassTransit ---
builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<GoalDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var rabbitMqHost = builder.Configuration["RabbitMQ:Host"] ?? throw new InvalidOperationException("RabbitMQ:Host is not configured.");
        var rabbitMqUsername = builder.Configuration["RabbitMQ:Username"] ?? throw new InvalidOperationException("RabbitMQ:Username is not configured.");
        var rabbitMqPassword = builder.Configuration["RabbitMQ:Password"] ?? throw new InvalidOperationException("RabbitMQ:Password is not configured.");

        cfg.Host(rabbitMqHost, h =>
        {
            h.Username(rabbitMqUsername);
            h.Password(rabbitMqPassword);
        });

        cfg.ConfigureEndpoints(ctx);
    });
});

// --- Controllers ---
builder.Services.AddControllers();


// --- OpenTelemetry ---
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddPrometheusExporter();
    })
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddOtlpExporter(opt =>
               {
                   opt.Endpoint = new Uri(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://jaeger:4317");
               });
    });
var app = builder.Build();

// --- Global Exception Handler (first in pipeline) ---
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

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

// --- HMAC Middleware ---
app.UseMiddleware<HmacMiddleware>();

// --- Map Controllers ---
app.MapControllers();

// --- Health Check ---
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "goal-service" }))
    .WithName("HealthCheck");

app.MapPrometheusScrapingEndpoint();

app.Run();

