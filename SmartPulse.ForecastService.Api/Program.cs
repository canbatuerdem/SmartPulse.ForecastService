using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartPulse.ForecastService.Repository;
using SmartPulse.ForecastService.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddForecastServiceModule(builder.Configuration);
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var db = services.GetRequiredService<ForecastDbContext>();
        db.Database.Migrate();
    }
    catch (SqlException ex) when (ex.Number == 1801)
    {
        // 1801 = "Database ... already exists." 
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Database already exists. Skipping database creation step.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or initializing the database.");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
