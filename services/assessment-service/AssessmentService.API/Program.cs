using AssessmentService.API.Extensions;
using AssessmentService.API.Middleware;
using AssessmentService.Infrastructure.Persistence;
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

// Add application services (DI)
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddValidationServices();

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
