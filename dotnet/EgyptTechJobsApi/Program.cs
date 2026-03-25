using Microsoft.EntityFrameworkCore;
using EgyptTechJobsApi.Application.Abstractions;
using EgyptTechJobsApi.Application.Services;
using EgyptTechJobsApi.Data;
using EgyptTechJobsApi.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

// Database configuration
builder.Services.AddDbContext<JobsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services (clean architecture)
builder.Services.AddScoped<IJobRepository, PostgresJobRepository>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddHttpClient<IJobFetchService, EgyptTechJobsApi.Services.JobFetchService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Verify database connectivity on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<JobsDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Test database connection
        var canConnect = await dbContext.Database.CanConnectAsync();
        if (canConnect)
        {
            logger.LogInformation("Successfully connected to PostgreSQL database");
        }
        else
        {
            logger.LogWarning("Could not connect to database. Make sure PostgreSQL is running.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to connect to database");
    }
}

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Egypt Tech Jobs API V1");
    c.RoutePrefix = "swagger"; // Swagger UI at /swagger
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
