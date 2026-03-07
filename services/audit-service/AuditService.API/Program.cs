using AuditService.Application.Mappings;
using AuditService.Infrastructure.Data;
using AuditService.Infrastructure.Extensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shared.ErrorHandling;
using Shared.HMAC;

var builder = WebApplication.CreateBuilder(args);

// HMAC
var hmacSecretKey = builder.Configuration["HMAC_SECRET_KEY"]
    ?? throw new InvalidOperationException("HMAC_SECRET_KEY not configured.");
builder.Services.AddHmacAuthentication(hmacSecretKey);
builder.Services.AddTransient<HmacDelegatingHandler>();

// --- Correlation ID ---
builder.Services.AddCorrelationId();

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

var app = builder.Build();

// Migrate DB on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCorrelationId();
app.UseStandardizedExceptionHandler();
app.UseMiddleware<HmacMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
