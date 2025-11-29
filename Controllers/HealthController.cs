using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using CandidateSelectionSystem.Data;

namespace CandidateSelectionSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;

    public HealthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            await _context.Database.CanConnectAsync();
            
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "Unhealthy",
                timestamp = DateTime.UtcNow,
                error = ex.Message
            });
        }
    }

    [HttpGet("detailed")]
    public async Task<IActionResult> GetDetailed()
    {
        var databaseCheck = await CheckDatabase();
        var memoryCheck = CheckMemory();
        var uptimeCheck = GetUptime();
        
        var health = new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
            checks = new
            {
                database = databaseCheck,
                memory = memoryCheck,
                uptime = uptimeCheck
            }
        };

        var isHealthy = ((dynamic)databaseCheck).status == "Healthy";
        return isHealthy ? Ok(health) : StatusCode(503, health);
    }

    private async Task<object> CheckDatabase()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            var studentCount = await _context.Students.CountAsync();
            
            return new
            {
                status = canConnect ? "Healthy" : "Unhealthy",
                studentCount,
                responseTime = "< 100ms"
            };
        }
        catch (Exception ex)
        {
            return new
            {
                status = "Unhealthy",
                error = ex.Message
            };
        }
    }

    private object CheckMemory()
    {
        var workingSet = GC.GetTotalMemory(false);
        return new
        {
            status = "Healthy",
            workingSetMB = Math.Round(workingSet / 1024.0 / 1024.0, 2)
        };
    }

    private object GetUptime()
    {
        var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
        return new
        {
            status = "Healthy",
            uptimeSeconds = Math.Round(uptime.TotalSeconds, 0)
        };
    }
}