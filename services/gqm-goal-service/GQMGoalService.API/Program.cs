using GQMGoalService.API.Middleware;
using GQMGoalService.Application;
using GQMGoalService.Application.Interfaces.Clients;
using GQMGoalService.Infrastructure;
using GQMGoalService.Infrastructure.Clients;
using Shared.HMAC;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

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

// Add HMAC authentication
var hmacSecretKey = builder.Configuration["HMAC_SECRET_KEY"] ?? "dev-secret-key-for-local";
builder.Services.AddHmacAuthentication(hmacSecretKey);
builder.Services.AddTransient<HmacDelegatingHandler>();

builder.Services.AddHttpClient<IOrchestrationClient, OrchestrationClient>(client =>
{
    var baseUrl = builder.Configuration["Services:OrchestrationService"] ?? "http://orchestration-service:8080";
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<HmacDelegatingHandler>();

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

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Apply migrations and seed data
await app.UseInfrastructureAsync();

app.UseHttpsRedirection();

// Add HMAC middleware
app.UseMiddleware<HmacMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
