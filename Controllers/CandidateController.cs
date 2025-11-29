using Microsoft.AspNetCore.Mvc;
using CandidateSelectionSystem.Models;
using CandidateSelectionSystem.Services;

namespace CandidateSelectionSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CandidateController : ControllerBase
{
    private readonly StreamingProcessor _processor;

    public CandidateController(StreamingProcessor processor)
    {
        _processor = processor;
    }

    [HttpPost]
    public async Task<IActionResult> AddCandidate([FromBody] Candidate candidate)
    {
        candidate.Timestamp = DateTime.UtcNow;
        await _processor.AddCandidateAsync(candidate);
        return Ok(new { message = "Candidate added to processing queue" });
    }

    [HttpPost("batch")]
    public async Task<IActionResult> AddCandidates([FromBody] List<Candidate> candidates)
    {
        foreach (var candidate in candidates)
        {
            candidate.Timestamp = DateTime.UtcNow;
            await _processor.AddCandidateAsync(candidate);
        }
        return Ok(new { message = $"{candidates.Count} candidates added to processing queue" });
    }
}