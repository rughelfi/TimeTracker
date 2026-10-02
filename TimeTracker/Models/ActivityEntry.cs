using System;
using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Models;

public class ActivityEntry
{
    [Key]
    public int Id { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    public string Project { get; set; } = string.Empty;

    public bool IsRemote { get; set; } = false;

    public TimeSpan Duration => EndTime - StartTime;

    public string LocationDisplay => IsRemote ? "🏠 Remoto" : "🏢 Ufficio";
}
