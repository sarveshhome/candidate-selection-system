using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CandidateSelectionSystem.Data;

namespace CandidateSelectionSystem.Services;

public class StreamingWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StreamingWorker> _logger;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(10);

    public StreamingWorker(IServiceProvider serviceProvider, ILogger<StreamingWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("StreamingWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var engine = scope.ServiceProvider.GetRequiredService<IReservationEngine>();

                // "Stream" = new/unprocessed records
                var newStudents = await db.Students
                    .Where(s => s.Result == null)
                    .OrderByDescending(s => s.Marks)
                    .ToListAsync(stoppingToken);

                if (newStudents.Any())
                {
                    _logger.LogInformation("Found {Count} new students, processing...", newStudents.Count);

                    var resultMap = await engine.ProcessAsync(newStudents);

                    foreach (var s in newStudents)
                    {
                        if (resultMap.TryGetValue(s.Id, out var res))
                        {
                            s.Result = res;
                        }
                        else
                        {
                            s.Result = "NOT_SELECTED";
                        }
                    }

                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Processed {Count} students", newStudents.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in StreamingWorker loop");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }

        _logger.LogInformation("StreamingWorker stopped");
    }
}