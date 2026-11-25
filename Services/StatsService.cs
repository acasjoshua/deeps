using Deeps.Models;

namespace Deeps.Services;

/// <summary>
/// Pure calculations over a category's runs: PBs, best segments, consistency,
/// comparisons, deltas, gold splits, possible time saves.
/// </summary>
public static class StatsService
{
    // ─── Personal Best ───────────────────────────────────────────────────────

    public static Run? PersonalBest(Category c) =>
        c.Runs.Where(r => r.Completed).OrderBy(r => r.Total).FirstOrDefault();

    // ─── Segment times ───────────────────────────────────────────────────────

    public static IEnumerable<TimeSpan> SegmentTimes(Category c, Guid segmentId) =>
        c.Runs.SelectMany(r => r.Splits).Where(s => s.SegmentId == segmentId).Select(s => s.Duration);

    public static TimeSpan? BestSegment(Category c, Guid segmentId)
    {
        var times = SegmentTimes(c, segmentId).ToList();
        return times.Count == 0 ? null : times.Min();
    }

    public static TimeSpan? WorstSegment(Category c, Guid segmentId)
    {
        var times = SegmentTimes(c, segmentId).ToList();
        return times.Count == 0 ? null : times.Max();
    }

    public static double? AverageSeconds(Category c, Guid segmentId)
    {
        var t = SegmentTimes(c, segmentId).Select(x => x.TotalSeconds).ToList();
        return t.Count == 0 ? null : t.Average();
    }

    public static TimeSpan? AverageSegment(Category c, Guid segmentId)
    {
        var avg = AverageSeconds(c, segmentId);
        return avg is null ? null : TimeSpan.FromSeconds(avg.Value);
    }

    public static TimeSpan? MedianSegment(Category c, Guid segmentId)
    {
        var sorted = SegmentTimes(c, segmentId).OrderBy(x => x).ToList();
        if (sorted.Count == 0) return null;
        int mid = sorted.Count / 2;
        return sorted.Count % 2 == 1 ? sorted[mid]
            : TimeSpan.FromTicks((sorted[mid - 1].Ticks + sorted[mid].Ticks) / 2);
    }

    // ─── Sum of best ─────────────────────────────────────────────────────────

    public static TimeSpan? SumOfBest(Category c)
    {
        if (c.Segments.Count == 0) return null;
        var total = TimeSpan.Zero;
        foreach (var seg in c.Segments)
        {
            var best = BestSegment(c, seg.Id);
            if (best is null) return null;
            total += best.Value;
        }
        return total;
    }

    // ─── Comparison split time ────────────────────────────────────────────────

    /// <summary>Return the comparison time for a segment given the chosen method.</summary>
    public static TimeSpan? ComparisonTime(Category c, Guid segmentId, string comparison) =>
        comparison switch
        {
            "BestSegments" => BestSegment(c, segmentId),
            "Average"      => AverageSegment(c, segmentId),
            "Median"       => MedianSegment(c, segmentId),
            "Worst"        => WorstSegment(c, segmentId),
            _              => PbSegmentTime(c, segmentId),   // PersonalBest default
        };

    private static TimeSpan? PbSegmentTime(Category c, Guid segmentId)
    {
        var pb = PersonalBest(c);
        return pb?.Splits.FirstOrDefault(s => s.SegmentId == segmentId)?.Duration;
    }

    // ─── Delta ───────────────────────────────────────────────────────────────

    /// <summary>Positive = slower than comparison; negative = ahead.</summary>
    public static TimeSpan? Delta(TimeSpan? actual, TimeSpan? comparison)
    {
        if (actual is null || comparison is null) return null;
        return actual.Value - comparison.Value;
    }

    // ─── Possible time save ───────────────────────────────────────────────────

    /// <summary>How much faster the segment could be (avg vs best).</summary>
    public static TimeSpan? PossibleTimeSave(Category c, Guid segmentId)
    {
        var avg = AverageSegment(c, segmentId);
        var best = BestSegment(c, segmentId);
        if (avg is null || best is null) return null;
        var diff = avg.Value - best.Value;
        return diff <= TimeSpan.Zero ? TimeSpan.Zero : diff;
    }

    // ─── Best possible time ───────────────────────────────────────────────────

    /// <summary>Time so far + sum of best for remaining segments.</summary>
    public static TimeSpan? BestPossibleTime(Category c, List<SplitTime> soFar, int nextSegIndex)
    {
        var elapsed = TimeSpan.FromTicks(soFar.Sum(s => s.Duration.Ticks));
        var remaining = TimeSpan.Zero;
        for (int i = nextSegIndex; i < c.Segments.Count; i++)
        {
            var best = BestSegment(c, c.Segments[i].Id);
            if (best is null) return null;
            remaining += best.Value;
        }
        return elapsed + remaining;
    }

    // ─── Consistency (σ) ─────────────────────────────────────────────────────

    /// <summary>Standard deviation (seconds) of a segment's times.</summary>
    public static double? Consistency(Category c, Guid segmentId)
    {
        var t = SegmentTimes(c, segmentId).Select(x => x.TotalSeconds).ToList();
        if (t.Count < 2) return null;
        var avg = t.Average();
        return Math.Sqrt(t.Sum(x => (x - avg) * (x - avg)) / (t.Count - 1));
    }

    /// <summary>Median of segment times as double seconds (for display).</summary>
    public static double? MedianSeconds(Category c, Guid segmentId)
    {
        var m = MedianSegment(c, segmentId);
        return m?.TotalSeconds;
    }

    /// <summary>
    /// Consistency rating 0–100: 100 = perfectly consistent (σ=0).
    /// Scaled relative to average: 1 - σ/avg, clamped to [0,100].
    /// </summary>
    public static double? ConsistencyRating(Category c, Guid segmentId)
    {
        var sigma = Consistency(c, segmentId);
        var avg = AverageSeconds(c, segmentId);
        if (sigma is null || avg is null || avg.Value <= 0) return null;
        return Math.Max(0, Math.Min(100, (1 - sigma.Value / avg.Value) * 100));
    }

    // ─── Trend ───────────────────────────────────────────────────────────────

    /// <summary>Average of the last N segment times (seconds). Trend improvement if decreasing.</summary>
    public static double? TrendAvg(Category c, Guid segmentId, int lastN = 10)
    {
        var t = SegmentTimes(c, segmentId).TakeLast(lastN).Select(x => x.TotalSeconds).ToList();
        return t.Count == 0 ? null : t.Average();
    }

    // ─── Mistakes ────────────────────────────────────────────────────────────

    public static int MistakeCount(Category c, Guid segmentId) =>
        c.Runs.SelectMany(r => r.Mistakes).Count(m => m.SegmentId == segmentId);

    public static double TotalSecondsLost(Category c, Guid segmentId) =>
        c.Runs.SelectMany(r => r.Mistakes).Where(m => m.SegmentId == segmentId).Sum(m => m.SecondsLost);

    public static IEnumerable<(string Category, int Count, double SecondsLost)> MistakesByCategory(Category c) =>
        c.Runs.SelectMany(r => r.Mistakes)
              .GroupBy(m => string.IsNullOrWhiteSpace(m.Category) ? "Uncategorised" : m.Category)
              .Select(g => (g.Key, g.Count(), g.Sum(m => m.SecondsLost)))
              .OrderByDescending(x => x.Item2);

    // ─── Reset heatmap ────────────────────────────────────────────────────────

    /// <summary>How many runs were reset at or after each segment index.</summary>
    public static int[] ResetHeatmap(Category c)
    {
        var heatmap = new int[c.Segments.Count];
        foreach (var run in c.Runs.Where(r => !r.Completed && r.ResetAtSegmentIndex >= 0))
        {
            int idx = Math.Min(run.ResetAtSegmentIndex, c.Segments.Count - 1);
            heatmap[idx]++;
        }
        return heatmap;
    }

    // ─── Session summary ─────────────────────────────────────────────────────

    /// <summary>Runs that started in the given UTC date.</summary>
    public static List<Run> SessionRuns(Category c, DateTime sessionDate) =>
        c.Runs.Where(r => r.StartedAt.Date == sessionDate.Date).ToList();

    public static TimeSpan SessionPlaytime(Category c, DateTime sessionDate) =>
        TimeSpan.FromTicks(SessionRuns(c, sessionDate).Sum(r => r.Total.Ticks));

    // ─── Progress over time ───────────────────────────────────────────────────

    /// <summary>Running PB at each completed run (for the PB progress chart).</summary>
    public static List<(DateTime Date, TimeSpan Time)> PbProgression(Category c)
    {
        var result = new List<(DateTime, TimeSpan)>();
        TimeSpan? currentBest = null;
        foreach (var run in c.Runs.Where(r => r.Completed).OrderBy(r => r.StartedAt))
        {
            if (currentBest is null || run.Total < currentBest.Value)
            {
                currentBest = run.Total;
                result.Add((run.StartedAt, run.Total));
            }
        }
        return result;
    }

    // ─── Gold check ──────────────────────────────────────────────────────────

    /// <summary>True if the given duration is better than any previous segment time.</summary>
    public static bool IsGold(Category c, Guid segmentId, TimeSpan duration)
    {
        var best = BestSegment(c, segmentId);
        return best is null || duration < best.Value;
    }

    // ─── Formatting ──────────────────────────────────────────────────────────

    public static string Format(TimeSpan? t, int decimals = 2)
    {
        if (t is null) return "--";
        var v = t.Value;
        bool negative = v < TimeSpan.Zero;
        if (negative) v = v.Negate();

        string sign = negative ? "-" : "";
        string frac = decimals switch
        {
            0 => "",
            1 => $".{v.Milliseconds / 100:0}",
            3 => $".{v.Milliseconds:000}",
            _ => $".{v.Milliseconds / 10:00}",
        };

        string time = v.TotalHours >= 1
            ? $"{(int)v.TotalHours}:{v.Minutes:00}:{v.Seconds:00}{frac}"
            : $"{v.Minutes}:{v.Seconds:00}{frac}";

        return sign + time;
    }

    public static string FormatDelta(TimeSpan? delta, int decimals = 2)
    {
        if (delta is null) return "";
        string sign = delta.Value >= TimeSpan.Zero ? "+" : "";
        return sign + Format(delta, decimals);
    }

    public static string FormatSeconds(double? s) => s is null ? "--" : $"{s.Value:0.00}s";
}
