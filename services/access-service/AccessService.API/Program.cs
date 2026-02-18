using Shared.HMAC;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add HMAC authentication
var hmacSecretKey = builder.Configuration["HMAC_SECRET_KEY"] ?? throw new InvalidOperationException("HMAC_SECRET_KEY not configured");
builder.Services.AddHmacAuthentication(hmacSecretKey);
builder.Services.AddTransient<HmacDelegatingHandler>();

// Add HttpClient for calling DepartmentService
builder.Services.AddHttpClient("DepartmentService", client =>
{
    client.BaseAddress = new Uri("http://department-service:8080");
}).AddHmacHandler();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Add HMAC middleware
app.UseMiddleware<HmacMiddleware>();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Test endpoint: Service-to-Service HMAC call
app.MapGet("/test-hmac", async (IHttpClientFactory httpClientFactory) =>
{
    try
    {
        var client = httpClientFactory.CreateClient("DepartmentService");
        
        // Test 1: Call whitelisted endpoint
        var weatherResponse = await client.GetAsync("/weatherforecast");
        
        // Test 2: Call protected GET endpoint
        var deptResponse = await client.GetAsync("/departments");
        
        // Test 3: Call protected POST endpoint
        var postContent = new StringContent("{\"name\":\"Marketing Department\"}", System.Text.Encoding.UTF8, "application/json");
        var postResponse = await client.PostAsync("/departments", postContent);
        
        var weatherData = await weatherResponse.Content.ReadAsStringAsync();
        var deptData = await deptResponse.Content.ReadAsStringAsync();
        var postData = await postResponse.Content.ReadAsStringAsync();
        
        return Results.Ok(new { 
            success = true,
            message = "HMAC service-to-service calls successful!",
            weatherTest = new { statusCode = (int)weatherResponse.StatusCode, data = weatherData },
            departmentGetTest = new { statusCode = (int)deptResponse.StatusCode, data = deptData },
            departmentPostTest = new { statusCode = (int)postResponse.StatusCode, data = postData }
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error: {ex.Message}");
    }
})
.WithName("TestHMAC");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
