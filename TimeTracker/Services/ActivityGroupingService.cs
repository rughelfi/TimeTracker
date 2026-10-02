using TimeTracker.Models;

namespace TimeTracker.Services;

/// <summary>
/// Groups ActivityEntries by project and similar description using simple token overlap.
/// </summary>
public class ActivityGroupingService
{
    private readonly double _threshold;

    public ActivityGroupingService(double similarityThreshold = 0.5)
    {
        _threshold = similarityThreshold;
    }

    public List<GroupedActivity> Group(IEnumerable<ActivityEntry> entries)
    {
        var result = new List<GroupedActivity>();

        foreach (var entry in entries)
        {
            var match = result.FirstOrDefault(g =>
                g.Project.Equals(entry.Project, StringComparison.OrdinalIgnoreCase) &&
                g.IsRemote == entry.IsRemote &&
                Similarity(g.Description, entry.Description) >= _threshold);

            if (match != null)
            {
                match.Entries.Add(entry);
                match.TotalDuration += entry.Duration;
                // Keep the most common/longer description
                if (entry.Description.Length > match.Description.Length)
                    match.Description = entry.Description;
            }
            else
            {
                result.Add(new GroupedActivity
                {
                    Description = entry.Description,
                    Project = entry.Project,
                    TotalDuration = entry.Duration,
                    Entries = [entry]
                });
            }
        }

        return result;
    }

    /// <summary>
    /// Jaccard similarity on word tokens (case-insensitive).
    /// </summary>
    private static double Similarity(string a, string b)
    {
        var tokensA = Tokenize(a);
        var tokensB = Tokenize(b);
        if (tokensA.Count == 0 && tokensB.Count == 0) return 1.0;
        if (tokensA.Count == 0 || tokensB.Count == 0) return 0.0;

        var intersection = tokensA.Intersect(tokensB).Count();
        var union = tokensA.Union(tokensB).Count();
        return (double)intersection / union;
    }

    private static HashSet<string> Tokenize(string text) =>
        text.ToLowerInvariant()
            .Split([' ', '-', '_', '/', '.', ','], StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet();
}
