using Microsoft.EntityFrameworkCore;
using CandidateSelectionSystem.Data;
using CandidateSelectionSystem.Config;
using CandidateSelectionSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<ReservationSettings>(
    builder.Configuration.GetSection("Reservation"));

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(cs);
});

// Services
builder.Services.AddControllers();
builder.Services.AddScoped<IReservationEngine, ReservationEngine>();
builder.Services.AddHostedService<StreamingWorker>();

// Health Checks
builder.Services.AddScoped<DatabaseHealthCheck>();
builder.Services.AddScoped<StreamingWorkerHealthCheck>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database")
    .AddCheck<DatabaseHealthCheck>("database_custom")
    .AddCheck<StreamingWorkerHealthCheck>("streaming_worker")
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Application is running"));

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

// Health Check Endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/index.html"));

app.Run();