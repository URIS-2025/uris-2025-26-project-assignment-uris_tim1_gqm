using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using PremiseService.API.Middleware;
using PremiseService.Application.Interfaces;
using PremiseService.Application.Interfaces.Clients;
using PremiseService.Application.Services;
using PremiseService.Infrastructure.Clients;
using PremiseService.Infrastructure.Persistence;
using PremiseService.Infrastructure.Seed;
using Shared.HMAC;

var builder = WebApplication.CreateBuilder(args);

// --- Database ---
var connectionString = builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string not configured.");

builder.Services.AddDbContext<PremiseDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IPremiseDbContext>(provider => provider.GetRequiredService<PremiseDbContext>());

// --- Application Services ---
builder.Services.AddScoped<IPremiseService, PremiseAppService>();
builder.Services.AddAutoMapper(typeof(PremiseAppService).Assembly);

// --- FluentValidation ---
builder.Services.AddValidatorsFromAssemblyContaining<PremiseAppService>();

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Premise Service API",
        Version = "v1"
    });
    options.AddServer(new OpenApiServer { Url = "/api/v1/premise" });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// --- HMAC Authentication ---
var hmacSecretKey = builder.Configuration["HMAC_SECRET_KEY"]
    ?? throw new InvalidOperationException("HMAC_SECRET_KEY not configured");
builder.Services.AddHmacAuthentication(hmacSecretKey);
builder.Services.AddTransient<HmacDelegatingHandler>();

// --- HTTP Clients for inter-service communication ---
var goalServiceUrl = builder.Configuration["ServiceUrls:GoalService"]
    ?? "http://goal-service:8080";

builder.Services.AddHttpClient<IGoalClient, GoalClient>(client =>
{
    client.BaseAddress = new Uri(goalServiceUrl);
}).AddHttpMessageHandler<HmacDelegatingHandler>();

builder.Services.AddHttpClient<IStrategyClient, StrategyClient>(client =>
{
    client.BaseAddress = new Uri(goalServiceUrl);
}).AddHttpMessageHandler<HmacDelegatingHandler>();

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

// --- Controllers ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper));
    });


// --- OpenTelemetry ---
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation()
               .AddPrometheusExporter()
               .AddRuntimeInstrumentation()
               .AddProcessInstrumentation();
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
app.UseMiddleware<ExceptionHandlingMiddleware>();

// --- Seed Data & Migrations (development only) ---
if (app.Environment.IsDevelopment())
{
    // Middleware to patch OpenAPI version (3.0.4 -> 3.0.1) for SwaggerUI compatibility
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.Value?.Contains("swagger") == true
            && context.Request.Path.Value.EndsWith(".json"))
        {
            var originalBody = context.Response.Body;
            using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            await next();

            memStream.Position = 0;
            var json = await new StreamReader(memStream).ReadToEndAsync();

            json = System.Text.RegularExpressions.Regex.Replace(
                json,
                @"""openapi""\s*:\s*""3\.0\.4""",
                @"""openapi"": ""3.0.1""");

            var buffer = System.Text.Encoding.UTF8.GetBytes(json);
            context.Response.Body = originalBody;
            context.Response.ContentLength = buffer.Length;
            await context.Response.Body.WriteAsync(buffer);
        }
        else
        {
            await next();
        }
    });

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = "swagger";
        options.SwaggerEndpoint("./v1/swagger.json", "Premise Service API v1");
    });
    var retries = 10;
    while (retries > 0)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PremiseDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migration applied successfully.");
            await PremiseSeeder.SeedAsync(dbContext, logger);
            break;
        }
        catch (Exception ex) when (retries > 1)
        {
            retries--;
            Console.WriteLine($"Database not ready, retrying in 3 seconds... ({retries} retries left)");
            await Task.Delay(3000);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to database after all retries: {ex.Message}");
            throw;
        }
    }
}


app.UseHttpsRedirection();

app.UseWhen(ctx =>
{
    var p = ctx.Request.Path.Value?.ToLower() ?? "";
    return !(p.StartsWith("/swagger") || p.StartsWith("/health"));
},
branch =>
{
    branch.UseMiddleware<HmacMiddleware>();
});

// --- Map Controllers ---
app.MapControllers();

// --- Health Check ---
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "premise-service" }))
    .WithName("HealthCheck");

app.MapPrometheusScrapingEndpoint();

app.Run();


