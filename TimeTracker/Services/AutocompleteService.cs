using TimeTracker.Models;

namespace TimeTracker.Services;

public class AutocompleteService
{
    private readonly ActivityRepository _repository;
    private static readonly List<string> BuiltInDictionary =
    [
        "Meeting di allineamento",
        "Daily Standup",
        "Call con il cliente",
        "Fix bug",
        "Code Review",
        "Sviluppo nuova funzionalità",
        "Sviluppo frontend",
        "Sviluppo backend",
        "Analisi requisiti",
        "Supporto tecnico",
        "Deploy in produzione",
        "Manutenzione database",
        "Stesura documentazione",
        "Test e QA",
        "Ottimizzazione performance",
        "Pianificazione sprint",
        "Refactoring codice",
        "Formazione e aggiornamento",
        "Gestione ticket"
    ];

    public AutocompleteService(ActivityRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ActivitySuggestion>> GetSuggestionsAsync(string query, AppSettings settings, int maxResults = 7)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        query = query.Trim();

        // 1. Get history suggestions from DB
        var history = await _repository.GetHistorySuggestionsAsync();

        // 2. Build dictionary suggestions
        var dictSuggestions = BuiltInDictionary.Select(term => new ActivitySuggestion
        {
            Description = term,
            Project = string.Empty,
            IsRemote = false,
            Source = "Dizionario",
            Frequency = 0
        }).ToList();

        // Add projects as potential suggestions if they fit
        foreach (var project in settings.Projects)
        {
            if (!dictSuggestions.Any(d => d.Description.Equals(project, StringComparison.OrdinalIgnoreCase)))
            {
                dictSuggestions.Add(new ActivitySuggestion
                {
                    Description = $"Attività {project}",
                    Project = project,
                    IsRemote = false,
                    Source = "Dizionario",
                    Frequency = 0
                });
            }
        }

        // Combine history + dictionary, preferring history if duplicates exist
        var combined = new List<ActivitySuggestion>(history);
        var existingDescriptions = new HashSet<string>(history.Select(h => h.Description), StringComparer.OrdinalIgnoreCase);

        foreach (var dictItem in dictSuggestions)
        {
            if (!existingDescriptions.Contains(dictItem.Description))
            {
                combined.Add(dictItem);
            }
        }

        // Rank results:
        // Priority 1: Starts with query (History first, then Dictionary)
        // Priority 2: Contains query (History first, then Dictionary)
        var startsWith = combined
            .Where(s => s.Description.StartsWith(query, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(s => s.Source == "Cronologia")
            .ThenByDescending(s => s.Frequency)
            .ThenBy(s => s.Description.Length);

        var contains = combined
            .Where(s => !s.Description.StartsWith(query, StringComparison.OrdinalIgnoreCase) &&
                        s.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(s => s.Source == "Cronologia")
            .ThenByDescending(s => s.Frequency)
            .ThenBy(s => s.Description.Length);

        return startsWith.Concat(contains).Take(maxResults).ToList();
    }
}
