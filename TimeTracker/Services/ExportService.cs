using ClosedXML.Excel;
using CsvHelper;
using System.Globalization;
using System.IO;
using TimeTracker.Models;

namespace TimeTracker.Services;

public class ExportService
{
    public void ExportToCsv(IEnumerable<GroupedActivity> activities, string filePath)
    {
        using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteHeader<CsvRow>();
        csv.NextRecord();
        foreach (var a in activities)
        {
            csv.WriteRecord(ToCsvRow(a));
            csv.NextRecord();
        }
    }

    public void ExportToExcel(IEnumerable<GroupedActivity> activities, string filePath)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Ore");

        // Header
        var headers = new[] { "Data", "Progetto", "Descrizione", "Modalità", "Ore", "Minuti", "Durata" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(0x2D, 0x2D, 0x3A);
            cell.Style.Font.FontColor = XLColor.White;
        }

        int row = 2;
        foreach (var a in activities)
        {
            var firstEntry = a.Entries.First();
            ws.Cell(row, 1).Value = firstEntry.StartTime.ToString("dd/MM/yyyy");
            ws.Cell(row, 2).Value = a.Project;
            ws.Cell(row, 3).Value = a.Description;
            ws.Cell(row, 4).Value = a.LocationDisplay;
            ws.Cell(row, 5).Value = (int)a.TotalDuration.TotalHours;
            ws.Cell(row, 6).Value = a.TotalDuration.Minutes;
            ws.Cell(row, 7).Value = a.DurationDisplay;
            row++;
        }

        ws.Columns().AdjustToContents();
        workbook.SaveAs(filePath);
    }

    private static CsvRow ToCsvRow(GroupedActivity a) => new()
    {
        Data = a.Entries.First().StartTime.ToString("dd/MM/yyyy"),
        Progetto = a.Project,
        Descrizione = a.Description,
        Modalità = a.LocationDisplay,
        Ore = (int)a.TotalDuration.TotalHours,
        Minuti = a.TotalDuration.Minutes,
        Durata = a.DurationDisplay
    };

    private class CsvRow
    {
        public string Data { get; set; } = "";
        public string Progetto { get; set; } = "";
        public string Descrizione { get; set; } = "";
        public string Modalità { get; set; } = "";
        public int Ore { get; set; }
        public int Minuti { get; set; }
        public string Durata { get; set; } = "";
    }

    public string GenerateAiExportText(IEnumerable<GroupedActivity> activities)
    {
        var lines = new List<string>();
        var culture = new CultureInfo("it-IT");
        var ticketRegex = new System.Text.RegularExpressions.Regex(@"\b([A-Z]{2,10}-\d+|#\d+)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        foreach (var a in activities)
        {
            if (a.Entries.Count == 0) continue;

            var firstEntry = a.Entries.First();
            var dateStr = firstEntry.StartTime.ToString("dd/MM/yyyy");

            var isRemote = a.Entries.All(e => e.IsRemote);
            var isOffice = a.Entries.All(e => !e.IsRemote);
            var modeStr = isRemote ? "da remoto" : (isOffice ? "in ufficio" : "sia da remoto che in ufficio");

            var hours = Math.Round(a.TotalDuration.TotalHours, 2);
            var durationStr = hours == 1 ? "1 ora" : $"{hours.ToString("0.##", culture)} ore";

            var projectStr = string.IsNullOrWhiteSpace(a.Project) ? "" : $" sulla commessa {a.Project.Trim()}";

            var rawDesc = a.Description.Trim();
            var match = ticketRegex.Match(rawDesc);
            string? ticket = null;
            string cleanDesc = rawDesc;

            if (match.Success)
            {
                ticket = match.Value.ToUpperInvariant();
                cleanDesc = ticketRegex.Replace(rawDesc, "").Trim();
                cleanDesc = System.Text.RegularExpressions.Regex.Replace(cleanDesc, @"^[\s:\-\(\)\[\]]+|[\s:\-\(\)\[\]]+$", "").Trim();
                cleanDesc = System.Text.RegularExpressions.Regex.Replace(cleanDesc, @"\s+", " ");
            }

            if (string.IsNullOrWhiteSpace(cleanDesc))
            {
                cleanDesc = "attività di sviluppo e supporto";
            }

            string occStr;
            var lowerDesc = cleanDesc.ToLowerInvariant();
            if (lowerDesc.StartsWith("di ") || lowerDesc.StartsWith("del ") || lowerDesc.StartsWith("dello ") ||
                lowerDesc.StartsWith("della ") || lowerDesc.StartsWith("dell'") || lowerDesc.StartsWith("degli ") ||
                lowerDesc.StartsWith("delle ") || lowerDesc.StartsWith("a ") || lowerDesc.StartsWith("per "))
            {
                occStr = $"occupandoti {cleanDesc}";
            }
            else
            {
                occStr = $"occupandoti di {cleanDesc}";
            }

            string ticketStr = ticket != null
                ? $", nell'ambito dell'attività identificata dal ticket {ticket}."
                : ".";

            var sentence = $"Il {dateStr} hai lavorato {modeStr} per {durationStr}{projectStr}, {occStr}{ticketStr}";
            lines.Add(sentence);
        }

        return string.Join("\n", lines);
    }

    public void ExportToAiText(IEnumerable<GroupedActivity> activities, string filePath)
    {
        var text = GenerateAiExportText(activities);
        File.WriteAllText(filePath, text, System.Text.Encoding.UTF8);
    }
}
