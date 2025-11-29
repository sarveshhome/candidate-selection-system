using Microsoft.EntityFrameworkCore;
using CandidateSelectionSystem.Models;

namespace CandidateSelectionSystem.Data;

public class AppDbContext : DbContext
{
    public DbSet<Student> Students => Set<Student>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}