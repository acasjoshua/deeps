namespace Deeps.Models;

public class Segment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string? IconEmoji { get; set; }          // per-segment icon (emoji or null)
    public string Notes { get; set; } = "";          // per-segment free text
    public int Order { get; set; }                   // position (for drag-reorder)
    public Guid? ParentId { get; set; }              // null = top-level, set = sub-split
    public TimeSpan? GoalTime { get; set; }          // per-segment target
}
