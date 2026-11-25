namespace Deeps.Models;

public class Run
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public bool Completed { get; set; }
    public List<SplitTime> Splits { get; set; } = new();
    public List<Mistake> Mistakes { get; set; } = new();

    public TimeSpan Total => TimeSpan.FromTicks(Splits.Sum(s => s.Duration.Ticks));
}

/// <summary>Duration of a single segment within a run (not cumulative).</summary>
public class SplitTime
{
    public Guid SegmentId { get; set; }
    public TimeSpan Duration { get; set; }
}

public class Mistake
{
    public Guid SegmentId { get; set; }
    public string Note { get; set; } = "";
    public double SecondsLost { get; set; }
}
