using Microsoft.Extensions.Options;
using CandidateSelectionSystem.Models;
using CandidateSelectionSystem.Config;

namespace CandidateSelectionSystem.Services;

public interface IReservationEngine
{
    Task<Dictionary<int, string>> ProcessAsync(IEnumerable<Student> students);
}

public class ReservationEngine : IReservationEngine
{
    private readonly ReservationSettings _settings;

    public ReservationEngine(IOptions<ReservationSettings> options)
    {
        _settings = options.Value;
    }

    public Task<Dictionary<int, string>> ProcessAsync(IEnumerable<Student> students)
    {
        var studentList = students.ToList();
        var result = new Dictionary<int, string>();
        var selectedIds = new HashSet<int>();

        int total = _settings.TotalVacancies;

        int obcSeats   = (int)Math.Floor(total * _settings.ObcPercent);
        int scstSeats  = (int)Math.Floor(total * _settings.ScstPercent);
        int womenSeats = (int)Math.Floor(total * _settings.WomenPercent);

        int usedReserved = obcSeats + scstSeats + womenSeats;
        int generalSeats = total - usedReserved;

        // Category groups
        var obc  = studentList.Where(s => s.Category.Equals("OBC", StringComparison.OrdinalIgnoreCase))
                              .OrderByDescending(s => s.Marks)
                              .ToList();

        var scst = studentList.Where(s => s.Category.Equals("SCST", StringComparison.OrdinalIgnoreCase))
                              .OrderByDescending(s => s.Marks)
                              .ToList();

        var gen  = studentList.Where(s => s.Category.Equals("GEN", StringComparison.OrdinalIgnoreCase))
                              .OrderByDescending(s => s.Marks)
                              .ToList();

        // Helper local function
        void SelectFromList(List<Student> source, int seats)
        {
            foreach (var s in source.Where(s => !selectedIds.Contains(s.Id))
                                    .Take(seats))
            {
                selectedIds.Add(s.Id);
                result[s.Id] = "SELECTED";
            }
        }

        // 1. Fill OBC quota (women + men by pure merit in OBC)
        SelectFromList(obc, obcSeats);

        // 2. Fill SC/ST quota
        SelectFromList(scst, scstSeats);

        // 3. Women quota (dual reservation)
        // Only women who are not already selected in OBC/SCST quota
        var womenPool = studentList
            .Where(s => s.Gender.Equals("F", StringComparison.OrdinalIgnoreCase)
                        && !selectedIds.Contains(s.Id))
            .OrderByDescending(s => s.Marks)
            .ToList();

        SelectFromList(womenPool, womenSeats);

        // 4. General quota (remaining students – any category, any gender, not yet selected)
        var remaining = studentList
            .Where(s => !selectedIds.Contains(s.Id))
            .OrderByDescending(s => s.Marks)
            .ToList();

        SelectFromList(remaining, generalSeats);

        // 5. Mark remaining as NOT_SELECTED
        foreach (var s in studentList.Where(s => !selectedIds.Contains(s.Id)))
        {
            result[s.Id] = "NOT_SELECTED";
        }

        return Task.FromResult(result);
    }
}