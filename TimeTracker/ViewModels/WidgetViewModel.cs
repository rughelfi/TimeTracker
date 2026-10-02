using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TimeTracker.Models;
using TimeTracker.Services;

namespace TimeTracker.ViewModels;

public partial class WidgetViewModel : ObservableObject
{
    private readonly TimerService _timer;
    private readonly ActivityRepository _repo;
    private readonly ActivityGroupingService _grouping;
    private readonly SettingsService _settingsService;

    [ObservableProperty] private string _countdownText = "00:00";
    [ObservableProperty] private string _timerLabelText = "Prossimo reminder tra";
    [ObservableProperty] private bool _isUrgent = false;
    [ObservableProperty] private string _todaySummary = "Nessuna attività oggi";
    [ObservableProperty] private ObservableCollection<ProjectTotal> _projectTotals = [];
    [ObservableProperty] private AppSettings _settings;
    [ObservableProperty] private bool _currentIsRemote;

    public event Action? ReminderFired;
    public event Action? OpenLogRequested;
    public event Action? OpenSettingsRequested;

    public DateTime CurrentSessionStart { get; private set; }

    public WidgetViewModel(TimerService timer, ActivityRepository repo,
        ActivityGroupingService grouping, SettingsService settingsService)
    {
        _timer = timer;
        _repo = repo;
        _grouping = grouping;
        _settingsService = settingsService;
        Settings = settingsService.Load();
        CurrentIsRemote = Settings.TipoOraDefault.Equals("Remoto", StringComparison.OrdinalIgnoreCase);

        _timer.SetInterval(Settings.ReminderIntervalMinutes);
        _timer.Tick += OnTick;
        _timer.TimerElapsed += OnTimerElapsed;

        RecalculateTimer();
        _ = RefreshTodaySummaryAsync();
    }

    private void OnTick(TimeSpan remaining)
    {
        if (_timer.IsOutsideWorkHours)
        {
            TimerLabelText = $"Fuori orario (Inizio {Settings.WorkDayStart})";
            IsUrgent = false;
            if (remaining.TotalHours >= 1)
                CountdownText = $"{(int)remaining.TotalHours}h {remaining.Minutes:D2}m";
            else
                CountdownText = $"{(int)remaining.TotalMinutes:D2}m {remaining.Seconds:D2}s";
        }
        else
        {
            TimerLabelText = "Prossimo reminder tra";
            CountdownText = $"{(int)remaining.TotalMinutes:D2}:{remaining.Seconds:D2}";
            IsUrgent = remaining.TotalMinutes <= 2;
        }
    }

    private void OnTimerElapsed()
    {
        CurrentSessionStart = _timer.SessionStart;
        ReminderFired?.Invoke();
    }

    [RelayCommand]
    private void OpenLog() => OpenLogRequested?.Invoke();

    [RelayCommand]
    private void OpenSettings() => OpenSettingsRequested?.Invoke();

    [RelayCommand]
    private void TriggerNow()
    {
        CurrentSessionStart = _timer.SessionStart;
        _timer.Restart();
        ReminderFired?.Invoke();
    }

    [RelayCommand]
    public async Task RecalculateTimerAsync()
    {
        RecalculateTimer();
        await RefreshTodaySummaryAsync();
    }

    public void RecalculateTimer()
    {
        TimeSpan? workEnd = TimeSpan.TryParse(Settings.WorkDayEnd, out var end) ? end : null;

        if (TimeSpan.TryParse(Settings.WorkDayStart, out var workStart))
            _timer.StartAligned(workStart, workEnd);
        else
            _timer.Start();

        CurrentSessionStart = _timer.SessionStart;
    }

    public async Task RefreshTodaySummaryAsync()
    {
        var entries = await _repo.GetByDateAsync(DateTime.Today);
        var grouped = _grouping.Group(entries);

        var totals = grouped
            .GroupBy(g => g.Project)
            .Select(g => new ProjectTotal
            {
                Project = g.Key,
                TotalDuration = TimeSpan.FromTicks(g.Sum(x => x.TotalDuration.Ticks))
            })
            .OrderByDescending(x => x.TotalDuration)
            .ToList();

        ProjectTotals = new ObservableCollection<ProjectTotal>(totals);

        if (entries.Count == 0)
        {
            TodaySummary = "Nessuna attività oggi";
        }
        else
        {
            var total = TimeSpan.FromTicks(entries.Sum(e => e.Duration.Ticks));
            TodaySummary = $"{entries.Count} attività · {FormatDuration(total)} totali";
            CurrentIsRemote = entries.Last().IsRemote;
        }
    }

    public void ApplySettings(AppSettings settings)
    {
        Settings = settings;
        _settingsService.Save(settings);
        _timer.SetInterval(settings.ReminderIntervalMinutes);
        RecalculateTimer();
        CurrentIsRemote = settings.TipoOraDefault.Equals("Remoto", StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatDuration(TimeSpan t) =>
        t.TotalHours >= 1 ? $"{(int)t.TotalHours}h {t.Minutes:D2}min" : $"{t.Minutes}min";
}

public class ProjectTotal
{
    public string Project { get; set; } = string.Empty;
    public TimeSpan TotalDuration { get; set; }
    public string Display => $"{Project}: " +
        (TotalDuration.TotalHours >= 1
            ? $"{(int)TotalDuration.TotalHours}h {TotalDuration.Minutes:D2}min"
            : $"{TotalDuration.Minutes}min");
}
