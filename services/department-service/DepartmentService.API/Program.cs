using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using DepartmentService.API.Middleware;
using DepartmentService.Application.Mappings;
using DepartmentService.Application.Validators;
using DepartmentService.Infrastructure;
using DepartmentService.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Shared.HMAC;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Department Service API",
        Version = "v1",
        Description = "API for managing organizations and departments."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT token from POST /auth/dev-token. Enter just the token value (without 'Bearer' prefix).",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // OpenApiSecuritySchemeReference doesn't serialize correctly as a dict key in
    // Microsoft.OpenApi v2, producing "security":[{}] instead of "security":[{"Bearer":[]}].
    // The response middleware below patches the swagger.json output.
    options.AddSecurityRequirement(_ =>
    {
        var req = new OpenApiSecurityRequirement();
        req.Add(new OpenApiSecuritySchemeReference("Bearer"), new List<string>());
        return req;
    });
});

// Add JWT Authentication
var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"]
    ?? builder.Configuration["JWT_SECRET_KEY"]
    ?? throw new InvalidOperationException("JWT secret key not configured.");

var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "GqmPlus";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "GqmPlus";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // For local/docker development
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Authentication failed: {ErrorMessage}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Token validated for user: {User}", context.Principal?.Identity?.Name);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("OnChallenge: {Error}, {Description}", context.Error, context.ErrorDescription);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Add HMAC authentication for service-to-service communication
var hmacSecretKey = builder.Configuration["HMAC_SECRET_KEY"]
    ?? throw new InvalidOperationException("HMAC_SECRET_KEY not configured");
builder.Services.AddHmacAuthentication(hmacSecretKey);
builder.Services.AddTransient<HmacDelegatingHandler>();

// Add Infrastructure (DbContext, services)
builder.Services.AddInfrastructure(builder.Configuration);

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(OrganizationProfile).Assembly);

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<OrganizationRequestValidator>();

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DepartmentServiceDbContext>();
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

// Configure the HTTP request pipeline

// Middleware to fix broken security requirement in swagger.json
// (OpenApiSecuritySchemeReference serializes as {} instead of "Bearer":[] in Microsoft.OpenApi v2)
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

        // Fix the empty security requirement: [{ }] -> [{"Bearer":[]}]
        json = System.Text.RegularExpressions.Regex.Replace(
            json,
            @"""security""\s*:\s*\[\s*\{\s*\}\s*\]",
            @"""security"":[{""Bearer"":[]}]");

        var buffer = Encoding.UTF8.GetBytes(json);
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
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Department Service API v1");
    options.RoutePrefix = "swagger";
    options.ConfigObject.AdditionalItems["persistAuthorization"] = "true";
});

// Global exception handler (must be early in pipeline)
app.UseMiddleware<GlobalExceptionHandler>();

app.UseAuthentication();
app.UseAuthorization();

// HMAC middleware for service-to-service validation
app.UseMiddleware<HmacMiddleware>();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "department-service" }))
    .AllowAnonymous();

// Redirect root to swagger
app.MapGet("/", () => Results.Redirect("/swagger"))
    .AllowAnonymous();

app.Run();

// Required for integration testing
public partial class Program { }
