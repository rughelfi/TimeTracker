using Microsoft.EntityFrameworkCore;
using System.IO;
using TimeTracker.Models;

namespace TimeTracker.Data;

public class TimeTrackerDbContext : DbContext
{
    public DbSet<ActivityEntry> Activities { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dbFolder = Path.Combine(appData, "TimeTracker");
        Directory.CreateDirectory(dbFolder);
        var dbPath = Path.Combine(dbFolder, "timetracker.db");
        options.UseSqlite($"Data Source={dbPath}");
    }
}
