namespace CandidateSelectionSystem.Models;

public enum Category
{
    GENERAL,
    OBC,
    SC_ST,
    WOMAN,
    WOMAN_OBC,
    WOMAN_SC_ST
}

public class Candidate
{
    public string CandidateId { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public Category Category { get; set; }
    public decimal Marks { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ReservationConfig
{
    public int OBC { get; set; } = 27;
    public int SC_ST { get; set; } = 22;
    public int Women { get; set; } = 33;
    public int Women_OBC { get; set; } = 9;
    public int Women_SC_ST { get; set; } = 7;
    public int General { get; set; } = 50;
    public int TotalSeats { get; set; } = 100;
}

public class SelectionResult
{
    public string CandidateId { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public Category Category { get; set; }
    public decimal Marks { get; set; }
    public bool IsSelected { get; set; }
    public decimal CutoffMark { get; set; }
    public DateTime ProcessedAt { get; set; }
}