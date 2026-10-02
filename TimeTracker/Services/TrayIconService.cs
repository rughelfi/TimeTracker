using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace TimeTracker.Services;

public class TrayIconService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    public bool IsExiting { get; set; } = false;

    public event Action? ShowWidgetRequested;
    public event Action? OpenLogRequested;
    public event Action? OpenSettingsRequested;
    public event Action? ExitRequested;

    public TrayIconService()
    {
        _notifyIcon = new NotifyIcon
        {
            Text = "TimeTracker",
            Visible = true
        };

        Icon? icon = null;
        try
        {
            var uri = new Uri("pack://application:,,,/Resources/app_icon.ico", UriKind.Absolute);
            var streamInfo = Application.GetResourceStream(uri);
            if (streamInfo != null)
            {
                icon = new Icon(streamInfo.Stream);
            }
        }
        catch { }

        if (icon == null)
        {
            var iconPath = Path.Combine(AppContext.BaseDirectory, "Resources", "app_icon.ico");
            if (File.Exists(iconPath))
            {
                icon = new Icon(iconPath);
            }
        }

        _notifyIcon.Icon = icon ?? SystemIcons.Application;

        var contextMenu = new ContextMenuStrip();

        var openItem = new ToolStripMenuItem("Apri TimeTracker", null, (_, _) => ShowWidgetRequested?.Invoke());
        openItem.Font = new Font(openItem.Font, FontStyle.Bold);
        contextMenu.Items.Add(openItem);

        contextMenu.Items.Add(new ToolStripMenuItem("📋 Log Attività", null, (_, _) => OpenLogRequested?.Invoke()));
        contextMenu.Items.Add(new ToolStripMenuItem("⚙ Impostazioni", null, (_, _) => OpenSettingsRequested?.Invoke()));
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(new ToolStripMenuItem("Esci", null, (_, _) =>
        {
            IsExiting = true;
            ExitRequested?.Invoke();
        }));

        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += (_, _) => ShowWidgetRequested?.Invoke();
    }

    public void ShowNotification(string title, string message)
    {
        _notifyIcon.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        GC.SuppressFinalize(this);
    }
}
