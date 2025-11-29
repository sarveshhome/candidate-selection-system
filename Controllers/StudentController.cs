using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CandidateSelectionSystem.Models;
using CandidateSelectionSystem.Data;

namespace CandidateSelectionSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly AppDbContext _context;

    public StudentController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> AddStudent([FromBody] Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Student added successfully", id = student.Id });
    }

    [HttpPost("batch")]
    public async Task<IActionResult> AddStudents([FromBody] List<Student> students)
    {
        _context.Students.AddRange(students);
        await _context.SaveChangesAsync();
        return Ok(new { message = $"{students.Count} students added successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> GetStudents()
    {
        var students = await _context.Students.OrderByDescending(s => s.Marks).ToListAsync();
        return Ok(students);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var students = await _context.Students.ToListAsync();
        var stats = new {
            total = students.Count,
            selected = students.Count(s => s.Result == "SELECTED"),
            notSelected = students.Count(s => s.Result == "NOT_SELECTED"),
            pending = students.Count(s => s.Result == null),
            byCategory = students.GroupBy(s => s.Category)
                .Select(g => new { category = g.Key, count = g.Count() }),
            byGender = students.GroupBy(s => s.Gender)
                .Select(g => new { gender = g.Key, count = g.Count() })
        };
        return Ok(stats);
    }
}