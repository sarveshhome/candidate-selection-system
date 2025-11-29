using Microsoft.Extensions.Diagnostics.HealthChecks;
using CandidateSelectionSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace CandidateSelectionSystem.Services;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _context;

    public DatabaseHealthCheck(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.CanConnectAsync(cancellationToken);
            var studentCount = await _context.Students.CountAsync(cancellationToken);
            
            return HealthCheckResult.Healthy($"Database is healthy. Students: {studentCount}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}

public class StreamingWorkerHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Check if streaming worker is processing
        var isHealthy = true; // Add actual logic to check worker status
        
        return Task.FromResult(isHealthy 
            ? HealthCheckResult.Healthy("Streaming worker is operational")
            : HealthCheckResult.Degraded("Streaming worker may have issues"));
    }
}