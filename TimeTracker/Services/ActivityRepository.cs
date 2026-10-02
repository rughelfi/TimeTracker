using TimeTracker.Data;
using TimeTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace TimeTracker.Services;

public class ActivityRepository
{
    private readonly TimeTrackerDbContext _db;

    public ActivityRepository(TimeTrackerDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(ActivityEntry entry)
    {
        _db.Activities.Add(entry);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ActivityEntry entry)
    {
        _db.Activities.Update(entry);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entry = await _db.Activities.FindAsync(id);
        if (entry != null)
        {
            _db.Activities.Remove(entry);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<ActivityEntry>> GetByDateAsync(DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        return await _db.Activities
            .Where(a => a.StartTime >= start && a.StartTime < end)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<List<ActivityEntry>> GetByRangeAsync(DateTime from, DateTime to)
    {
        return await _db.Activities
            .Where(a => a.StartTime >= from.Date && a.StartTime < to.Date.AddDays(1))
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<List<ActivitySuggestion>> GetHistorySuggestionsAsync()
    {
        var entries = await _db.Activities
            .OrderByDescending(a => a.StartTime)
            .Take(500)
            .ToListAsync();

        return entries
            .Where(e => !string.IsNullOrWhiteSpace(e.Description))
            .GroupBy(e => e.Description.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var mostRecent = g.First();
                return new ActivitySuggestion
                {
                    Description = mostRecent.Description.Trim(),
                    Project = mostRecent.Project,
                    IsRemote = mostRecent.IsRemote,
                    Source = "Cronologia",
                    Frequency = g.Count()
                };
            })
            .OrderByDescending(s => s.Frequency)
            .ToList();
    }
}
