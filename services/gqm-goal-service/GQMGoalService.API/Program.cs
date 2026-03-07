using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using GQMGoalService.API.Middleware;
using GQMGoalService.Application;
using GQMGoalService.Application.Interfaces.Clients;
using GQMGoalService.Infrastructure;
using GQMGoalService.Infrastructure.Clients;
using Shared.Auth;
using Shared.ErrorHandling;
using Shared.HMAC;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Configure infrastructure and application services
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();

// OpenAPI setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "GQM Goal Service API", Version = "v1" });
    c.AddServer(new Microsoft.OpenApi.Models.OpenApiServer { Url = "/api/v1/GQM-goal" });
});

// --- JWT Authentication & Authorization ---
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add HMAC authentication
var hmacSecretKey = builder.Configuration["HMAC_SECRET_KEY"] ?? "dev-secret-key-for-local";
builder.Services.AddHmacAuthentication(hmacSecretKey);
builder.Services.AddTransient<HmacDelegatingHandler>();

// --- Correlation ID ---
builder.Services.AddCorrelationId();

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


// --- OpenTelemetry ---
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("gqm-goal-service"))
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", "GQM Goal Service API v1");
        c.RoutePrefix = "swagger";
        // http://localhost/api/v1/GQM-goal/swagger
    });
}

app.UseCorrelationId();
app.UseStandardizedExceptionHandler();

// Apply migrations and seed data
await app.UseInfrastructureAsync();

app.UseHttpsRedirection();

// --- Authentication & Authorization ---
app.UseAuthentication();
app.UseAuthorization();

// --- Organization Context ---
app.UseMiddleware<OrganizationContextMiddleware>();

// Add HMAC middleware
app.UseMiddleware<HmacMiddleware>();

app.MapControllers();

app.MapHealthChecks("/health");

app.MapPrometheusScrapingEndpoint();

app.Run();





