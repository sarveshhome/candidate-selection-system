namespace CandidateSelectionSystem.Models;

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Category { get; set; } = default!; // "GEN", "OBC", "SCST"
    public string Gender { get; set; } = default!;   // "M", "F"
    public int Marks { get; set; }
    public string? Result { get; set; }              // null / "SELECTED" / "NOT_SELECTED"
}