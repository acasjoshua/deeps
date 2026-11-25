namespace Deeps.Models;

public enum TimingMethod { RealTime, GameTime }

public class Run
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public bool Completed { get; set; }
    public int ResetAtSegmentIndex { get; set; } = -1; // -1 = completed or never started
    public string Notes { get; set; } = "";             // free text for the run
    public TimingMethod TimingMethod { get; set; } = TimingMethod.RealTime;
    public List<SplitTime> Splits { get; set; } = new();
    public List<Mistake> Mistakes { get; set; } = new();

    public TimeSpan Total => TimeSpan.FromTicks(Splits.Sum(s => s.Duration.Ticks));
}

/// <summary>Duration of a single segment within a run (not cumulative).</summary>
public class SplitTime
{
    public Guid SegmentId { get; set; }
    public TimeSpan Duration { get; set; }
    public bool IsGold { get; set; }                 // was this segment a gold at the time?
}

public class Mistake
{
    public Guid SegmentId { get; set; }
    public string Note { get; set; } = "";
    public string Category { get; set; } = "";       // e.g. "missed trick", "RNG", "menu"
    public string[] Tags { get; set; } = Array.Empty<string>();
    public double SecondsLost { get; set; }
}
