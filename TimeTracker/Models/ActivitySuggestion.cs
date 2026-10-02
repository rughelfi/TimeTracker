namespace TimeTracker.Models;

public class ActivitySuggestion
{
    public string Description { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public bool IsRemote { get; set; }
    public string Source { get; set; } = string.Empty; // "Cronologia" or "Dizionario"
    public int Frequency { get; set; } = 1;
}
