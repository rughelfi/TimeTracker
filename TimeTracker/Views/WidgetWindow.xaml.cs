using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.ViewModels;
using TimeTracker.Views;

namespace TimeTracker.Views;

public partial class WidgetWindow : Window
{
    private readonly WidgetViewModel _vm;
    private readonly TrayIconService _trayService;

    public WidgetWindow(WidgetViewModel vm, TrayIconService trayService)
    {
        InitializeComponent();
        _vm = vm;
        _trayService = trayService;
        DataContext = vm;

        vm.ReminderFired += OnReminderFired;
        vm.OpenLogRequested += OnOpenLog;
        vm.OpenSettingsRequested += OnOpenSettings;

        _trayService.ShowWidgetRequested += RestoreWindow;
        _trayService.OpenLogRequested += OnOpenLog;
        _trayService.OpenSettingsRequested += OnOpenSettings;
        _trayService.ExitRequested += ExitApp;

        Left = vm.Settings.WidgetLeft;
        Top = vm.Settings.WidgetTop;
    }

    private void RestoreWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!e.Handled && e.Key == System.Windows.Input.Key.F5)
        {
            if (_vm.RecalculateTimerCommand.CanExecute(null))
            {
                _vm.RecalculateTimerCommand.Execute(null);
                e.Handled = true;
            }
        }
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        SaveState();
        Hide();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_trayService.IsExiting)
        {
            e.Cancel = true;
            SaveState();
            Hide();
        }
        else
        {
            base.OnClosing(e);
        }
    }

    private void ExitApp()
    {
        SaveState();
        System.Windows.Application.Current.Shutdown();
    }

    private void SaveState()
    {
        _vm.Settings.WidgetLeft = Left;
        _vm.Settings.WidgetTop = Top;
        _vm.ApplySettings(_vm.Settings);
    }

    private void OnReminderFired()
    {
        RestoreWindow();

        var vm = new QuickEntryViewModel(_vm.CurrentSessionStart, _vm.Settings,
            App.GetService<Services.ActivityRepository>(),
            App.GetService<Services.ActivityGroupingService>(),
            App.GetService<Services.AutocompleteService>(),
            _vm.CurrentIsRemote);
        var popup = new QuickEntryWindow(vm);
        vm.Saved += async () =>
        {
            _vm.CurrentIsRemote = vm.IsRemote;
            popup.Close();
            await _vm.RefreshTodaySummaryAsync();
        };
        vm.Cancelled += () => popup.Close();
        popup.Owner = this;
        popup.ShowDialog();
    }

    private void OnOpenLog()
    {
        RestoreWindow();

        var vm = App.GetService<LogViewModel>();
        var win = new LogWindow(vm);
        vm.CloseRequested += () => win.Close();
        win.Owner = this;
        win.ShowDialog();
        _ = _vm.RefreshTodaySummaryAsync();
    }

    private void OnOpenSettings()
    {
        RestoreWindow();

        var settingsVm = App.GetService<SettingsViewModel>();
        var win = new SettingsWindow(settingsVm);
        settingsVm.Saved += (newSettings) =>
        {
            _vm.ApplySettings(newSettings);
            win.Close();
        };
        settingsVm.Cancelled += () => win.Close();
        win.Owner = this;
        win.ShowDialog();
    }

    protected override void OnLocationChanged(EventArgs e)
    {
        base.OnLocationChanged(e);
        _vm.Settings.WidgetLeft = Left;
        _vm.Settings.WidgetTop = Top;
    }
}
