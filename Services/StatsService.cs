using Deeps.Models;

namespace Deeps.Services;

/// <summary>Pure calculations over a category's runs: PBs, best segments, consistency.</summary>
public static class StatsService
{
    public static Run? PersonalBest(Category c) =>
        c.Runs.Where(r => r.Completed).OrderBy(r => r.Total).FirstOrDefault();

    public static TimeSpan? BestSegment(Category c, Guid segmentId)
    {
        var times = SegmentTimes(c, segmentId).ToList();
        return times.Count == 0 ? null : times.Min();
    }

    /// <summary>Sum of best segments: the theoretical ideal run.</summary>
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

    public static double? AverageSeconds(Category c, Guid segmentId)
    {
        var t = SegmentTimes(c, segmentId).Select(x => x.TotalSeconds).ToList();
        return t.Count == 0 ? null : t.Average();
    }

    /// <summary>Standard deviation (seconds) of a segment's times. Lower = more consistent.</summary>
    public static double? Consistency(Category c, Guid segmentId)
    {
        var t = SegmentTimes(c, segmentId).Select(x => x.TotalSeconds).ToList();
        if (t.Count < 2) return null;
        var avg = t.Average();
        return Math.Sqrt(t.Sum(x => (x - avg) * (x - avg)) / (t.Count - 1));
    }

    public static IEnumerable<TimeSpan> SegmentTimes(Category c, Guid segmentId) =>
        c.Runs.SelectMany(r => r.Splits).Where(s => s.SegmentId == segmentId).Select(s => s.Duration);

    public static int MistakeCount(Category c, Guid segmentId) =>
        c.Runs.SelectMany(r => r.Mistakes).Count(m => m.SegmentId == segmentId);

    public static string Format(TimeSpan? t)
    {
        if (t is null) return "--";
        var v = t.Value;
        return v.TotalHours >= 1
            ? $"{(int)v.TotalHours}:{v.Minutes:00}:{v.Seconds:00}.{v.Milliseconds / 10:00}"
            : $"{v.Minutes}:{v.Seconds:00}.{v.Milliseconds / 10:00}";
    }

    public static string FormatSeconds(double? s) => s is null ? "--" : $"{s.Value:0.00}s";
}
