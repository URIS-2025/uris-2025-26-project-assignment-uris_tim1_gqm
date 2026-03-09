using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using UserService.API.Middleware;
using UserService.Application.Interfaces.Clients;
using UserService.Application.Mappings;
using UserService.Application.Validators;
using UserService.Infrastructure;
using UserService.Infrastructure.Clients;
using UserService.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shared.HMAC;

var builder = WebApplication.CreateBuilder(args);

// --- Controllers ---
builder.Services.AddControllers();

// --- Swagger / OpenAPI ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- JWT Authentication ---
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// --- HMAC Authentication ---
var hmacSecretKey = builder.Configuration["HMAC_SECRET_KEY"]
    ?? throw new InvalidOperationException("HMAC_SECRET_KEY not configured");
builder.Services.AddHmacAuthentication(hmacSecretKey);
builder.Services.AddTransient<HmacDelegatingHandler>();

// --- Audit Client ---
builder.Services.AddHttpClient<IAuditClient, AuditClient>(client =>
{
    var baseUrl = builder.Configuration["Services:AuditService"] ?? "http://audit-service:8080";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<HmacDelegatingHandler>();

// --- Infrastructure (DbContext, services) ---
builder.Services.AddInfrastructure(builder.Configuration);

// --- AutoMapper ---
builder.Services.AddAutoMapper(typeof(UserProfile).Assembly);

// --- FluentValidation ---
builder.Services.AddValidatorsFromAssemblyContaining<UserRequestValidator>();


// --- OpenTelemetry ---
builder.Services.AddOpenTelemetry()
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

// --- Apply migrations and seed data ---
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserServiceDbContext>();
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
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service API v1");
    options.RoutePrefix = "swagger";
});

// --- Authentication & Authorization ---
app.UseAuthentication();
app.UseAuthorization();

// --- HMAC Middleware ---
app.UseMiddleware<HmacMiddleware>();

// --- Map Controllers ---
app.MapControllers();

// --- Health Check ---
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "user-service" }))
    .WithName("HealthCheck");

app.MapPrometheusScrapingEndpoint();

app.Run();

// Required for integration testing
public partial class Program { }






