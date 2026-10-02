using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using TimeTracker.Models;
using TimeTracker.Services;

namespace TimeTracker.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private readonly AppSettings _settings;

    [ObservableProperty] private int _intervalMinutes;
    [ObservableProperty] private string _workDayStart = "09:00";
    [ObservableProperty] private string _workDayEnd = "18:00";
    [ObservableProperty] private string _exportPath = string.Empty;
    [ObservableProperty] private bool _playSound;
    [ObservableProperty] private bool _autoStartWithWindows;
    [ObservableProperty] private bool _defaultIsRemote;
    [ObservableProperty] private double _similarityThreshold;
    [ObservableProperty] private ObservableCollection<string> _projects = [];
    [ObservableProperty] private string _newProject = string.Empty;

    public event Action<AppSettings>? Saved;
    public event Action? Cancelled;

    public SettingsViewModel(AppSettings settings, SettingsService settingsService)
    {
        _settingsService = settingsService;
        _settings = settings;

        IntervalMinutes = settings.ReminderIntervalMinutes;
        WorkDayStart = settings.WorkDayStart;
        WorkDayEnd = settings.WorkDayEnd;
        ExportPath = settings.ExportPath;
        PlaySound = settings.PlaySound;
        AutoStartWithWindows = settings.AutoStartWithWindows;
        DefaultIsRemote = settings.TipoOraDefault.Equals("Remoto", StringComparison.OrdinalIgnoreCase);
        SimilarityThreshold = settings.SimilarityThreshold;
        Projects = new ObservableCollection<string>(settings.Projects);
    }

    [RelayCommand]
    private void TestSound() => App.PlayAlarmSound();

    [RelayCommand]
    private void AddProject()
    {
        if (!string.IsNullOrWhiteSpace(NewProject) && !Projects.Contains(NewProject))
        {
            Projects.Add(NewProject.Trim());
            NewProject = string.Empty;
        }
    }

    [RelayCommand]
    private void RemoveProject(string project) => Projects.Remove(project);

    [RelayCommand]
    private void Save()
    {
        _settings.ReminderIntervalMinutes = IntervalMinutes;
        _settings.WorkDayStart = WorkDayStart;
        _settings.WorkDayEnd = WorkDayEnd;
        _settings.ExportPath = ExportPath;
        _settings.PlaySound = PlaySound;
        _settings.AutoStartWithWindows = AutoStartWithWindows;
        _settings.TipoOraDefault = DefaultIsRemote ? "Remoto" : "Normale";
        _settings.SimilarityThreshold = SimilarityThreshold;
        _settings.Projects = [.. Projects];
        _settingsService.Save(_settings);
        Saved?.Invoke(_settings);
    }

    [RelayCommand]
    private void Cancel() => Cancelled?.Invoke();

    [RelayCommand]
    private void BrowseExportPath()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Scegli cartella di export",
            FileName = "seleziona cartella",
            Filter = "Cartella|*.none"
        };
        // We just use the folder, not the file
        if (dialog.ShowDialog() == true)
            ExportPath = Path.GetDirectoryName(dialog.FileName) ?? ExportPath;
    }
}
