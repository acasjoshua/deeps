using Deeps.Models;

namespace Deeps.Services;

/// <summary>
/// In-memory store, registered as a singleton. TODO: persist to browser localStorage / IndexedDB
/// (or a backend API) so data survives a page refresh.
/// </summary>
public class PracticeStore
{
    public List<Game> Games { get; } = new();
    public event Action? Changed;

    public PracticeStore() => Seed();

    public Game AddGame(string name)
    {
        var g = new Game { Name = name.Trim() };
        Games.Add(g);
        Changed?.Invoke();
        return g;
    }

    public Category AddCategory(Game game, string name)
    {
        var c = new Category { GameId = game.Id, Name = name.Trim() };
        game.Categories.Add(c);
        Changed?.Invoke();
        return c;
    }

    public void AddSegment(Category category, string name)
    {
        category.Segments.Add(new Segment { Name = name.Trim() });
        Changed?.Invoke();
    }

    public void SaveRun(Category category, Run run)
    {
        category.Runs.Add(run);
        Changed?.Invoke();
    }

    public Category? FindCategory(Guid id) =>
        Games.SelectMany(g => g.Categories).FirstOrDefault(c => c.Id == id);

    public Game? FindGame(Guid id) => Games.FirstOrDefault(g => g.Id == id);

    private void Seed()
    {
        var game = new Game { Name = "Super Example Bros." };
        var cat = new Category { GameId = game.Id, Name = "Any%" };
        foreach (var n in new[] { "World 1", "World 2", "World 3", "Final Boss" })
            cat.Segments.Add(new Segment { Name = n });
        game.Categories.Add(cat);
        Games.Add(game);
    }
}
