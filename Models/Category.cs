namespace Deeps.Models;

/// <summary>A runnable category of a game (e.g. "Any%"), split into ordered segments.</summary>
public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GameId { get; set; }
    public string Name { get; set; } = "";
    public List<Segment> Segments { get; set; } = new();
    public List<Run> Runs { get; set; } = new();
    public List<Strategy> Strategies { get; set; } = new();
}
