using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using TimeTracker.Models;
using TimeTracker.Services;
public partial class LogViewModel : ObservableObject
{
    private readonly ActivityRepository _repo;
    private readonly ActivityGroupingService _grouping;
    private readonly ExportService _export;
    private readonly AppSettings _settings;

    [ObservableProperty] private DateTime _selectedDate = DateTime.Today;
    [ObservableProperty] private ObservableCollection<ActivityEntry> _entries = [];
    [ObservableProperty] private ObservableCollection<GroupedActivity> _groupedActivities = [];
    [ObservableProperty] private bool _showGrouped = true;
    [ObservableProperty] private bool _isBusy = false;
    [ObservableProperty] private bool _isNotBusy = true;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public event Action? CloseRequested;

    public LogViewModel(ActivityRepository repo, ActivityGroupingService grouping,
        ExportService export, AppSettings settings)
    {
        _repo = repo;
        _grouping = grouping;
        _export = export;
        _settings = settings;
        _ = LoadAsync();
    }

    partial void OnSelectedDateChanged(DateTime value) => _ = LoadAsync();

    public async Task LoadAsync()
    {
        IsBusy = true;
        var raw = await _repo.GetByDateAsync(SelectedDate);
        Entries = new ObservableCollection<ActivityEntry>(raw);
        RefreshGrouped();
        IsBusy = false;
        StatusMessage = string.Empty;
    }

    private void RefreshGrouped()
    {
        var grouped = _grouping.Group(Entries);
        GroupedActivities = new ObservableCollection<GroupedActivity>(grouped);
    }

    [RelayCommand]
    private async Task DeleteEntryAsync(ActivityEntry entry)
    {
        await _repo.DeleteAsync(entry.Id);
        await LoadAsync();
    }

    [RelayCommand]
    private void SplitGroup(GroupedActivity group)
    {
        // Ungroup: remove from grouped list, keep entries as separate items
        var ungrouped = group.Entries.Select(e => new GroupedActivity
        {
            Description = e.Description,
            Project = e.Project,
            TotalDuration = e.Duration,
            Entries = [e]
        }).ToList();

        var idx = GroupedActivities.IndexOf(group);
        GroupedActivities.RemoveAt(idx);
        foreach (var g in ungrouped)
            GroupedActivities.Insert(idx++, g);
    }

    [RelayCommand]
    private void ExportCsv()
    {
        try
        {
            var path = Path.Combine(_settings.ExportPath,
                $"ore_{SelectedDate:yyyy-MM-dd}.csv");
            _export.ExportToCsv(GroupedActivities, path);
            StatusMessage = $"✓ Esportato in {path}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Errore export: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ExportExcel()
    {
        try
        {
            var path = Path.Combine(_settings.ExportPath,
                $"ore_{SelectedDate:yyyy-MM-dd}.xlsx");
            _export.ExportToExcel(GroupedActivities, path);
            StatusMessage = $"✓ Esportato in {path}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Errore export: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ExportAiText()
    {
        try
        {
            var text = _export.GenerateAiExportText(GroupedActivities);
            if (string.IsNullOrWhiteSpace(text))
            {
                StatusMessage = "Nessuna attività da esportare";
                return;
            }

            System.Windows.Clipboard.SetText(text);

            var path = Path.Combine(_settings.ExportPath,
                $"ore_ai_{SelectedDate:yyyy-MM-dd}.txt");
            _export.ExportToAiText(GroupedActivities, path);
            StatusMessage = $"✓ Copiato negli appunti e salvato in {path}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Errore export AI: {ex.Message}";
        }
    }


    [RelayCommand]
    private void Close() => CloseRequested?.Invoke();
}
