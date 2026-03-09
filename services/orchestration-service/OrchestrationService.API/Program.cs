using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using FluentValidation;
using OrchestrationService.API.Middleware;
using OrchestrationService.Application.Interfaces;
using OrchestrationService.Application.Interfaces.Clients;
using OrchestrationService.Application.Interfaces.Persistence;
using OrchestrationService.Application.Services;
using OrchestrationService.Infrastructure.Clients;
using OrchestrationService.Infrastructure.Consumers;
using OrchestrationService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.HMAC;

var builder = WebApplication.CreateBuilder(args);

// --- Database ---
var connectionString = builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string not configured.");

builder.Services.AddDbContext<OrchestrationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IOrchestrationDbContext>(provider =>
    provider.GetRequiredService<OrchestrationDbContext>());

// --- Application Services ---
builder.Services.AddScoped<IWorkflowService, WorkflowService>();

// --- FluentValidation ---
builder.Services.AddValidatorsFromAssemblyContaining<WorkflowService>();

// --- Swagger / OpenAPI ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- HMAC Authentication ---
var hmacSecretKey = builder.Configuration["HMAC_SECRET_KEY"]
    ?? throw new InvalidOperationException("HMAC_SECRET_KEY not configured");
builder.Services.AddHmacAuthentication(hmacSecretKey);
builder.Services.AddTransient<HmacDelegatingHandler>();

// --- Cross-Service HTTP Clients ---
// --- MassTransit ---
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<WorkflowTransitionRequestedConsumer>();

    x.AddEntityFrameworkOutbox<OrchestrationDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
        o.DisableInboxCleanupService(); // Optional: handle cleanup ourselves or keep default
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

        // Add Retry Policy to handle race conditions during workflow startup
        cfg.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(1)));

        cfg.ConfigureEndpoints(ctx);
    });
});

builder.Services.AddHttpClient<ICompensationHttpClient, CompensationHttpClient>(client =>
{
    // Base address intentionally empty — compensation endpoints are absolute URLs
}).AddHttpMessageHandler<HmacDelegatingHandler>();

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

// --- Apply Migrations & Swagger (development only) ---
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<OrchestrationDbContext>();
        await db.Database.MigrateAsync();
    }

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Orchestration Service API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// --- HMAC Middleware ---
app.UseMiddleware<HmacMiddleware>();

// --- Map Controllers ---
app.MapControllers();

// --- Health Check ---
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "orchestration-service" }))
    .WithName("HealthCheck");

app.MapPrometheusScrapingEndpoint();

app.Run();

