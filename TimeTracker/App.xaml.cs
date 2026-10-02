using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;
using System.Windows;
using TimeTracker.Data;
using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.ViewModels;
using TimeTracker.Views;

namespace TimeTracker;

public partial class App : System.Windows.Application
{
    private static IServiceProvider _services = null!;

    public static T GetService<T>() where T : notnull => _services.GetRequiredService<T>();

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _services = services.BuildServiceProvider();

        // Ensure DB is created/migrated
        var db = _services.GetRequiredService<TimeTrackerDbContext>();
        await db.Database.EnsureCreatedAsync();
        try
        {
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE Activities ADD COLUMN IsRemote INTEGER NOT NULL DEFAULT 0;");
        }
        catch
        {
            // Column already exists or table was just created
        }

        var trayService = _services.GetRequiredService<TrayIconService>();
        var widgetVm = _services.GetRequiredService<WidgetViewModel>();
        var widget = new WidgetWindow(widgetVm, trayService);
        widget.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        var trayService = _services.GetService<TrayIconService>();
        trayService?.Dispose();
        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Infrastructure
        services.AddDbContext<TimeTrackerDbContext>(ServiceLifetime.Transient);
        services.AddSingleton<SettingsService>();
        services.AddSingleton<AppSettings>(sp => sp.GetRequiredService<SettingsService>().Load());
        services.AddSingleton<TimerService>(sp =>
            new TimerService(sp.GetRequiredService<AppSettings>().ReminderIntervalMinutes));
        services.AddTransient<ActivityRepository>();
        services.AddTransient<AutocompleteService>();
        services.AddSingleton<ActivityGroupingService>(sp =>
            new ActivityGroupingService(sp.GetRequiredService<AppSettings>().SimilarityThreshold));
        services.AddTransient<ExportService>();
        services.AddSingleton<TrayIconService>();

        // ViewModels
        services.AddSingleton<WidgetViewModel>();
        services.AddTransient<LogViewModel>(sp => new LogViewModel(
            sp.GetRequiredService<ActivityRepository>(),
            sp.GetRequiredService<ActivityGroupingService>(),
            sp.GetRequiredService<ExportService>(),
            sp.GetRequiredService<AppSettings>()));
        services.AddTransient<SettingsViewModel>(sp => new SettingsViewModel(
            sp.GetRequiredService<AppSettings>(),
            sp.GetRequiredService<SettingsService>()));
    }

    public static void PlayAlarmSound()
    {
        try
        {
            var settings = _services.GetRequiredService<AppSettings>();
            if (!settings.PlaySound) return;

            // Custom file takes priority
            var soundPath = Path.Combine(AppContext.BaseDirectory, "Resources", "alarm.wav");
            if (File.Exists(soundPath))
            {
                var reader = new AudioFileReader(soundPath);
                var output = new WaveOutEvent { Volume = 1.0f };
                output.Init(reader);
                output.PlaybackStopped += (_, _) => { output.Dispose(); reader.Dispose(); };
                output.Play();
                return;
            }

            // Generate a loud double-beep with NAudio (880Hz + 660Hz, full volume)
            Task.Run(() =>
            {
                try
                {
                    PlayBeep(880, 0.25);
                    Thread.Sleep(80);
                    PlayBeep(660, 0.35);
                }
                catch { }
            });
        }
        catch { }
    }

    private static void PlayBeep(double frequency, double durationSeconds)
    {
        var sine = new SignalGenerator(44100, 1)
        {
            Type = SignalGeneratorType.Sin,
            Frequency = frequency,
            Gain = 0.9f
        }.Take(TimeSpan.FromSeconds(durationSeconds));

        using var output = new WaveOutEvent { Volume = 1.0f };
        output.Init(sine.ToWaveProvider());
        output.Play();
        while (output.PlaybackState == PlaybackState.Playing)
            Thread.Sleep(20);
    }
}
