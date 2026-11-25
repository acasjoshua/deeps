using Deeps.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace Deeps.Services;

/// <summary>
/// Persists all data to localStorage via JS interop.
/// Falls back to in-memory if JS is unavailable.
/// </summary>
public class PracticeStore
{
    private const string GamesKey    = "deeps_games";
    private const string SettingsKey = "deeps_settings";

    private readonly IJSRuntime _js;
    private readonly JsonSerializerOptions _jsonOpts = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    public List<Game> Games { get; private set; } = new();
    public AppSettings Settings { get; private set; } = new();

    public event Action? Changed;

    public PracticeStore(IJSRuntime js) => _js = js;

    // ─── Init ─────────────────────────────────────────────────────────────────

    public async Task InitAsync()
    {
        try
        {
            var gamesJson    = await _js.InvokeAsync<string?>("localStorage.getItem", GamesKey);
            var settingsJson = await _js.InvokeAsync<string?>("localStorage.getItem", SettingsKey);

            if (!string.IsNullOrEmpty(gamesJson))
                Games = JsonSerializer.Deserialize<List<Game>>(gamesJson, _jsonOpts) ?? new();

            if (!string.IsNullOrEmpty(settingsJson))
                Settings = JsonSerializer.Deserialize<AppSettings>(settingsJson, _jsonOpts) ?? new();

            if (Games.Count == 0)
                Seed();
        }
        catch
        {
            if (Games.Count == 0) Seed();
        }
    }

    // ─── Persistence ─────────────────────────────────────────────────────────

    public async Task SaveAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", GamesKey,
                JsonSerializer.Serialize(Games, _jsonOpts));
            await _js.InvokeVoidAsync("localStorage.setItem", SettingsKey,
                JsonSerializer.Serialize(Settings, _jsonOpts));
        }
        catch { /* silently ignore if storage is unavailable */ }
        Changed?.Invoke();
    }

    public async Task SaveSettingsAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", SettingsKey,
                JsonSerializer.Serialize(Settings, _jsonOpts));
        }
        catch { }
        Changed?.Invoke();
    }

    // ─── Export / Import ──────────────────────────────────────────────────────

    public string ExportJson() => JsonSerializer.Serialize(Games, new JsonSerializerOptions { WriteIndented = true });

    public async Task ImportJsonAsync(string json)
    {
        var imported = JsonSerializer.Deserialize<List<Game>>(json, _jsonOpts);
        if (imported is not null)
        {
            Games = imported;
            await SaveAsync();
        }
    }

    // ─── Games ───────────────────────────────────────────────────────────────

    public async Task<Game> AddGameAsync(string name)
    {
        var g = new Game { Name = name.Trim() };
        Games.Add(g);
        await SaveAsync();
        return g;
    }

    public async Task UpdateGameAsync(Game game, string newName)
    {
        game.Name = newName.Trim();
        await SaveAsync();
    }

    public async Task DeleteGameAsync(Guid gameId)
    {
        Games.RemoveAll(g => g.Id == gameId);
        await SaveAsync();
    }

    // ─── Categories ──────────────────────────────────────────────────────────

    public async Task<Category> AddCategoryAsync(Game game, string name)
    {
        var c = new Category { GameId = game.Id, Name = name.Trim() };
        game.Categories.Add(c);
        await SaveAsync();
        return c;
    }

    public async Task UpdateCategoryAsync(Category cat, string newName, TimeSpan? goalTime = null)
    {
        cat.Name = newName.Trim();
        if (goalTime.HasValue) cat.GoalTime = goalTime;
        await SaveAsync();
    }

    public async Task DeleteCategoryAsync(Game game, Guid catId)
    {
        game.Categories.RemoveAll(c => c.Id == catId);
        await SaveAsync();
    }

    // ─── Segments ────────────────────────────────────────────────────────────

    public async Task AddSegmentAsync(Category category, string name)
    {
        int order = category.Segments.Count > 0 ? category.Segments.Max(s => s.Order) + 1 : 0;
        category.Segments.Add(new Segment { Name = name.Trim(), Order = order });
        await SaveAsync();
    }

    public async Task UpdateSegmentAsync(Segment seg, string name, string? icon = null,
        string? notes = null, TimeSpan? goal = null, Guid? parentId = null, bool updateParent = false)
    {
        seg.Name = name.Trim();
        if (icon  is not null) seg.IconEmoji = icon;
        if (notes is not null) seg.Notes     = notes;
        if (goal.HasValue)     seg.GoalTime  = goal;
        if (updateParent)      seg.ParentId  = parentId;
        await SaveAsync();
    }

    public async Task DeleteSegmentAsync(Category category, Guid segId)
    {
        category.Segments.RemoveAll(s => s.Id == segId);
        // Re-order
        for (int i = 0; i < category.Segments.Count; i++)
            category.Segments[i].Order = i;
        await SaveAsync();
    }

    public async Task ReorderSegmentsAsync(Category category, List<Segment> reordered)
    {
        for (int i = 0; i < reordered.Count; i++)
            reordered[i].Order = i;
        category.Segments = reordered;
        await SaveAsync();
    }

    // ─── Runs ────────────────────────────────────────────────────────────────

    public async Task SaveRunAsync(Category category, Run run)
    {
        // Mark gold splits
        foreach (var split in run.Splits)
            split.IsGold = StatsService.IsGold(category, split.SegmentId, split.Duration);

        category.Runs.Add(run);
        await SaveAsync();
    }

    public async Task UpdateRunAsync(Category category, Run run)
    {
        foreach (var split in run.Splits)
            split.IsGold = StatsService.IsGold(category, split.SegmentId, split.Duration);

        await SaveAsync();
    }

    public async Task DeleteRunAsync(Category category, Guid runId)
    {
        category.Runs.RemoveAll(r => r.Id == runId);
        await SaveAsync();
    }

    public async Task UpdateRunNotesAsync(Run run, string notes)
    {
        run.Notes = notes;
        await SaveAsync();
    }

    // ─── Strategies ──────────────────────────────────────────────────────────

    public async Task AddStrategyAsync(Category cat, Guid? segmentId, string title, string notes)
    {
        cat.Strategies.Add(new Strategy
        {
            SegmentId = segmentId,
            Title     = title.Trim(),
            Notes     = notes,
            IsActive  = !cat.Strategies.Any(s => s.SegmentId == segmentId) // first = active
        });
        await SaveAsync();
    }

    public async Task SetActiveStrategyAsync(Category cat, Guid strategyId, Guid? segmentId)
    {
        foreach (var s in cat.Strategies.Where(s => s.SegmentId == segmentId))
            s.IsActive = s.Id == strategyId;
        await SaveAsync();
    }

    public async Task DeleteStrategyAsync(Category cat, Guid strategyId)
    {
        cat.Strategies.RemoveAll(s => s.Id == strategyId);
        await SaveAsync();
    }

    // ─── Lookups ─────────────────────────────────────────────────────────────

    public Category? FindCategory(Guid id) =>
        Games.SelectMany(g => g.Categories).FirstOrDefault(c => c.Id == id);

    public Game? FindGame(Guid id) => Games.FirstOrDefault(g => g.Id == id);

    public Game? GameForCategory(Guid catId) =>
        Games.FirstOrDefault(g => g.Categories.Any(c => c.Id == catId));

    // ─── Seed ────────────────────────────────────────────────────────────────

    private void Seed()
    {
        var game = new Game { Name = "Super Example Bros." };
        var cat  = new Category { GameId = game.Id, Name = "Any%" };
        int i    = 0;
        foreach (var n in new[] { "World 1", "World 2", "World 3", "Final Boss" })
            cat.Segments.Add(new Segment { Name = n, Order = i++ });
        game.Categories.Add(cat);
        Games.Add(game);
    }
}
