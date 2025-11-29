using System.Threading.Channels;
using CandidateSelectionSystem.Models;
using Microsoft.AspNetCore.SignalR;
using CandidateSelectionSystem.Hubs;

namespace CandidateSelectionSystem.Services;

public class StreamingProcessor : BackgroundService
{
    private readonly Channel<Candidate> _candidateChannel;
    private readonly IHubContext<SelectionHub> _hubContext;
    private readonly ReservationConfig _config;
    private readonly List<SelectionResult> _results = new();

    public StreamingProcessor(IHubContext<SelectionHub> hubContext)
    {
        _candidateChannel = Channel.CreateUnbounded<Candidate>();
        _hubContext = hubContext;
        _config = new ReservationConfig();
    }

    public async Task AddCandidateAsync(Candidate candidate)
    {
        await _candidateChannel.Writer.WriteAsync(candidate);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var candidate in _candidateChannel.Reader.ReadAllAsync(stoppingToken))
        {
            var result = ProcessCandidate(candidate);
            _results.Add(result);
            
            await _hubContext.Clients.All.SendAsync("SelectionUpdate", result, stoppingToken);
            
            if (_results.Count % 10 == 0) // Batch processing every 10 candidates
            {
                var cutoffs = CalculateCutoffs();
                await _hubContext.Clients.All.SendAsync("CutoffUpdate", cutoffs, stoppingToken);
            }
            
            await Task.Delay(100, stoppingToken); // Simulate processing time
        }
    }

    private SelectionResult ProcessCandidate(Candidate candidate)
    {
        var cutoff = GetCutoffForCategory(candidate.Category);
        return new SelectionResult
        {
            CandidateId = candidate.CandidateId,
            CandidateName = candidate.CandidateName,
            Category = candidate.Category,
            Marks = candidate.Marks,
            IsSelected = candidate.Marks >= cutoff,
            CutoffMark = cutoff,
            ProcessedAt = DateTime.UtcNow
        };
    }

    private decimal GetCutoffForCategory(Category category)
    {
        var categoryResults = _results.Where(r => r.Category == category || IsEligibleForDualReservation(r.Category, category))
                                    .OrderByDescending(r => r.Marks)
                                    .ToList();

        var seatCount = GetSeatCountForCategory(category);
        return categoryResults.Count >= seatCount ? categoryResults[seatCount - 1].Marks : 0;
    }

    private bool IsEligibleForDualReservation(Category candidateCategory, Category targetCategory)
    {
        return (candidateCategory == Category.WOMAN_OBC && (targetCategory == Category.WOMAN || targetCategory == Category.OBC)) ||
               (candidateCategory == Category.WOMAN_SC_ST && (targetCategory == Category.WOMAN || targetCategory == Category.SC_ST));
    }

    private int GetSeatCountForCategory(Category category) => category switch
    {
        Category.OBC => _config.OBC,
        Category.SC_ST => _config.SC_ST,
        Category.WOMAN => _config.Women,
        Category.WOMAN_OBC => _config.Women_OBC,
        Category.WOMAN_SC_ST => _config.Women_SC_ST,
        Category.GENERAL => _config.General,
        _ => 0
    };

    private Dictionary<Category, decimal> CalculateCutoffs()
    {
        return Enum.GetValues<Category>().ToDictionary(cat => cat, GetCutoffForCategory);
    }
}