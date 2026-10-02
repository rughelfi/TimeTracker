namespace TimeTracker.Models;

public class AppSettings
{
    public int ReminderIntervalMinutes { get; set; } = 30;
    public List<string> Projects { get; set; } = ["Generale", "ISBets", "ISBets-Frontend", "Monitor", "Supporto", "Meeting"];
    public string ExportPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    public double WidgetLeft { get; set; } = 100;
    public double WidgetTop { get; set; } = 100;
    public bool PlaySound { get; set; } = true;
    public string WorkDayStart { get; set; } = "09:00";
    public string WorkDayEnd { get; set; } = "18:00";
    public string TipoOraDefault { get; set; } = "Normale"; // "Normale" | "Remoto"
    public double SimilarityThreshold { get; set; } = 0.5;
    public bool AutoStartWithWindows { get; set; } = false;
}
