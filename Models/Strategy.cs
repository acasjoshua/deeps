namespace Deeps.Models;

public class Strategy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? SegmentId { get; set; }
    public string Title { get; set; } = "";
    public string Notes { get; set; } = "";
}
