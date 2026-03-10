using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using AuditService.API.Middleware;
using AuditService.Application.Mappings;
using AuditService.Infrastructure.Data;
using AuditService.Infrastructure.Extensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shared.HMAC;

var builder = WebApplication.CreateBuilder(args);

// HMAC
var hmacSecretKey = builder.Configuration["HMAC_SECRET_KEY"]
    ?? throw new InvalidOperationException("HMAC_SECRET_KEY not configured.");
builder.Services.AddHmacAuthentication(hmacSecretKey);
builder.Services.AddTransient<HmacDelegatingHandler>();

// Infrastructure (DB + services)
builder.Services.AddInfrastructure(builder.Configuration);

// AutoMapper
builder.Services.AddAutoMapper(typeof(AuditLogProfile));

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<AuditService.Application.Validators.CreateAuditLogRequestValidator>();

// Controllers
builder.Services.AddControllers();

// Health checks
builder.Services.AddHealthChecks();

// Swagger (dev only)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}


// --- OpenTelemetry ---
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("audit-service"))
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddPrometheusExporter()
               .AddRuntimeInstrumentation()
               .AddMeter("Npgsql")
               .AddMeter("MassTransit");
    })
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddEntityFrameworkCoreInstrumentation(opt => opt.SetDbStatementForText = true)
               .AddSource("Npgsql")
               .AddSource("MassTransit")
               .AddOtlpExporter(opt =>
               {
                   opt.Endpoint = new Uri(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://jaeger:4317");
               });
    });
var app = builder.Build();

// Migrate DB on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
    await db.Database.MigrateAsync();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<HmacMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.MapPrometheusScrapingEndpoint();

app.Run();

namespace AuditService.API
{
    public partial class Program { }
}
