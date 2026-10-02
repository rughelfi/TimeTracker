using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TimeTracker.Models;
using TimeTracker.Services;

namespace TimeTracker.ViewModels;

public partial class QuickEntryViewModel : ObservableObject
{
    private readonly ActivityRepository _repo;
    private readonly ActivityGroupingService _grouping;
    private readonly AppSettings _settings;
    private readonly AutocompleteService _autocomplete;
    private bool _isSelectingSuggestion;

    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _selectedProject = string.Empty;
    [ObservableProperty] private bool _isRemote = false;
    [ObservableProperty] private string _durationDisplay = string.Empty;
    [ObservableProperty] private ObservableCollection<GroupedActivity> _groupedActivities = [];
    [ObservableProperty] private GroupedActivity? _selectedGroupedActivity;
    [ObservableProperty] private bool _hasGroupedActivities = false;

    [ObservableProperty] private ObservableCollection<ActivitySuggestion> _filteredSuggestions = [];
    [ObservableProperty] private ActivitySuggestion? _selectedSuggestion;
    [ObservableProperty] private bool _isSuggestionsOpen = false;

    public ObservableCollection<string> Projects { get; }
    public DateTime SessionStart { get; }
    public DateTime SessionEnd { get; }

    public event Action? Saved;
    public event Action? Cancelled;

    public QuickEntryViewModel(DateTime sessionStart, AppSettings settings,
        ActivityRepository repo, ActivityGroupingService grouping, AutocompleteService autocomplete,
        bool? currentIsRemote = null)
    {
        SessionStart = sessionStart;
        SessionEnd = DateTime.Now;
        _repo = repo;
        _settings = settings;
        _grouping = grouping;
        _autocomplete = autocomplete;

        IsRemote = currentIsRemote ?? settings.TipoOraDefault.Equals("Remoto", StringComparison.OrdinalIgnoreCase);
        Projects = new ObservableCollection<string>(settings.Projects);
        SelectedProject = settings.Projects.FirstOrDefault() ?? string.Empty;

        var duration = SessionEnd - SessionStart;
        DurationDisplay = duration.TotalHours >= 1
            ? $"{(int)duration.TotalHours}h {duration.Minutes:D2}min"
            : $"{duration.Minutes}min";

        _ = LoadRecentActivitiesAsync();
    }

    async partial void OnDescriptionChanged(string value)
    {
        if (_isSelectingSuggestion) return;

        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length < 1)
        {
            FilteredSuggestions.Clear();
            IsSuggestionsOpen = false;
            return;
        }

        try
        {
            var suggestions = await _autocomplete.GetSuggestionsAsync(value, _settings);
            FilteredSuggestions = new ObservableCollection<ActivitySuggestion>(suggestions);
            IsSuggestionsOpen = FilteredSuggestions.Count > 0;
        }
        catch
        {
            IsSuggestionsOpen = false;
        }
    }

    partial void OnSelectedSuggestionChanged(ActivitySuggestion? value)
    {
        if (value != null)
        {
            SelectSuggestion(value);
        }
    }

    [RelayCommand]
    public void SelectSuggestion(ActivitySuggestion? suggestion)
    {
        if (suggestion == null) return;

        _isSelectingSuggestion = true;
        Description = suggestion.Description;
        if (!string.IsNullOrWhiteSpace(suggestion.Project))
        {
            if (!Projects.Contains(suggestion.Project))
            {
                Projects.Add(suggestion.Project);
            }
            SelectedProject = suggestion.Project;
        }

        IsSuggestionsOpen = false;
        _isSelectingSuggestion = false;
    }

    private async Task LoadRecentActivitiesAsync()
    {
        try
        {
            var fromDate = DateTime.Today.AddDays(-7);
            var toDate = DateTime.Today;
            var raw = await _repo.GetByRangeAsync(fromDate, toDate);

            if (raw != null && raw.Count > 0)
            {
                var grouped = _grouping.Group(raw);
                var sorted = grouped
                    .OrderByDescending(g => g.Entries.Max(e => e.StartTime))
                    .ToList();

                GroupedActivities = new ObservableCollection<GroupedActivity>(sorted);
                HasGroupedActivities = GroupedActivities.Count > 0;
            }
            else
            {
                GroupedActivities.Clear();
                HasGroupedActivities = false;
            }
        }
        catch
        {
            HasGroupedActivities = false;
        }
    }

    partial void OnSelectedGroupedActivityChanged(GroupedActivity? value)
    {
        if (value != null)
        {
            SelectGroupedActivity(value);
        }
    }

    [RelayCommand]
    private void SelectGroupedActivity(GroupedActivity? activity)
    {
        if (activity == null) return;
        _isSelectingSuggestion = true;
        Description = activity.Description;
        if (!string.IsNullOrWhiteSpace(activity.Project))
        {
            if (!Projects.Contains(activity.Project))
            {
                Projects.Add(activity.Project);
            }
            SelectedProject = activity.Project;
        }
        IsSuggestionsOpen = false;
        _isSelectingSuggestion = false;
    }

    [RelayCommand]
    private async Task AddTimeToGroupedActivityAsync(GroupedActivity? activity)
    {
        if (activity == null) return;
        SelectGroupedActivity(activity);
        await SaveAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Description)) return;

        await _repo.AddAsync(new ActivityEntry
        {
            StartTime = SessionStart,
            EndTime = SessionEnd,
            Description = Description.Trim(),
            Project = SelectedProject,
            IsRemote = IsRemote
        });

        Saved?.Invoke();
    }

    [RelayCommand]
    private void Cancel() => Cancelled?.Invoke();
}
