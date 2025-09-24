using System.Text.Json;

namespace Spoken.App;

internal static class SessionStore
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private static string GetPath() => Path.Combine(FileSystem.AppDataDirectory, "session.json");

    public static async Task<SessionState> LoadAsync()
    {
        try
        {
            var path = GetPath();
            if (!File.Exists(path))
                return CreateEmpty();
            using var fs = File.OpenRead(path);
            var state = await JsonSerializer.DeserializeAsync<SessionState>(fs, Options);
            if (state == null || state.Tabs.Count == 0)
                return CreateEmpty();
            // Ensure ActiveTabId valid
            if (!state.Tabs.Any(t => t.Id == state.ActiveTabId))
                state.ActiveTabId = state.Tabs[0].Id;
            return state;
        }
        catch
        {
            return CreateEmpty();
        }
    }

    public static async Task SaveAsync(SessionState state)
    {
        try
        {
            var path = GetPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var fs = File.Create(path);
            await JsonSerializer.SerializeAsync(fs, state, Options);
        }
        catch
        {
            // Swallow persistence errors silently for now; could log in future.
        }
    }

    private static SessionState CreateEmpty()
    {
        var tab = new TabSession();
        return new SessionState
        {
            Version = 1,
            Tabs = new List<TabSession> { tab },
            ActiveTabId = tab.Id
        };
    }
}

internal class SessionState
{
    public int Version { get; set; } = 1;
    public List<TabSession> Tabs { get; set; } = new();
    public Guid ActiveTabId { get; set; }
}

internal class TabSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Passage { get; set; } = "John 3:16-18"; // default sample
    public string Translation { get; set; } = "KJV";
    public string? Title { get; set; }
    public string Html { get; set; } = string.Empty;

    public string DisplayLabel => string.IsNullOrWhiteSpace(Title) ? (string.IsNullOrWhiteSpace(Passage) ? "New" : Passage!) : Title!;
}
