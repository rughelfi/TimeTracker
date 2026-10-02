using System.Collections.ObjectModel;

namespace TimeTracker.Models;

/// <summary>
/// Represents one or more ActivityEntries merged together for review/export.
/// </summary>
public class GroupedActivity
{
    public string Description { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public TimeSpan TotalDuration { get; set; }
    public ObservableCollection<ActivityEntry> Entries { get; set; } = [];
    public bool IsGrouped => Entries.Count > 1;

    public string DurationDisplay =>
        TotalDuration.TotalHours >= 1
            ? $"{(int)TotalDuration.TotalHours}h {TotalDuration.Minutes:D2}min"
            : $"{TotalDuration.Minutes}min";

    public TimeSpan TodayDuration =>
        TimeSpan.FromTicks(Entries.Where(e => e.StartTime.Date == DateTime.Today).Sum(e => e.Duration.Ticks));

    public string TodayDurationDisplay
    {
        get
        {
            var t = TodayDuration;
            if (t.TotalMinutes == 0) return "Registrata nei giorni scorsi";
            return t.TotalHours >= 1
                ? $"{(int)t.TotalHours}h {t.Minutes:D2}min oggi"
                : $"{t.Minutes}min oggi";
        }
    }

    public bool IsRemote => Entries.Count > 0 && Entries.All(e => e.IsRemote);

    public string LocationDisplay
    {
        get
        {
            if (Entries.Count == 0) return string.Empty;
            if (Entries.All(e => e.IsRemote)) return "🏠 Remoto";
            if (Entries.All(e => !e.IsRemote)) return "🏢 Ufficio";
            return "🏠/🏢 Misto";
        }
    }
}

