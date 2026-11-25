namespace Deeps.Models;

public class Strategy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? SegmentId { get; set; }   // null = whole-run strategy
    public string Title { get; set; } = "";
    public string Notes { get; set; } = "";
    public bool IsActive { get; set; }     // the currently chosen strategy for this segment
}
