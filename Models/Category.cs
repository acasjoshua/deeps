namespace Deeps.Models;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GameId { get; set; }
    public string Name { get; set; } = "";
    public List<Segment> Segments { get; set; } = new();
    public List<Run> Runs { get; set; } = new();
    public List<Strategy> Strategies { get; set; } = new();
    public TimeSpan? GoalTime { get; set; }           // whole-run target
    public int AttemptCount { get; set; }             // counts every start, including resets
}
