using Deeps.Models;
using Deeps.Services;
using Xunit;

namespace Deeps.Tests;

public class StatsServiceTests
{
    private static Category CreateTestCategory()
    {
        var cat = new Category { Name = "Any%" };
        var seg1 = new Segment { Name = "Seg 1", Order = 0 };
        var seg2 = new Segment { Name = "Seg 2", Order = 1 };
        var seg3 = new Segment { Name = "Seg 3", Order = 2 };
        cat.Segments.AddRange(new[] { seg1, seg2, seg3 });
        return cat;
    }

    [Fact]
    public void PersonalBest_ReturnsFastestCompletedRun()
    {
        var cat = CreateTestCategory();
        var s1 = cat.Segments[0].Id;
        var s2 = cat.Segments[1].Id;

        // Run 1: 30s completed
        cat.Runs.Add(new Run
        {
            Completed = true,
            Splits = new()
            {
                new() { SegmentId = s1, Duration = TimeSpan.FromSeconds(10) },
                new() { SegmentId = s2, Duration = TimeSpan.FromSeconds(20) }
            }
        });

        // Run 2: 25s completed (Fastest!)
        cat.Runs.Add(new Run
        {
            Completed = true,
            Splits = new()
            {
                new() { SegmentId = s1, Duration = TimeSpan.FromSeconds(12) },
                new() { SegmentId = s2, Duration = TimeSpan.FromSeconds(13) }
            }
        });

        // Run 3: 15s NOT completed (reset) - should be ignored
        cat.Runs.Add(new Run
        {
            Completed = false,
            Splits = new()
            {
                new() { SegmentId = s1, Duration = TimeSpan.FromSeconds(5) },
                new() { SegmentId = s2, Duration = TimeSpan.FromSeconds(10) }
            }
        });

        var pb = StatsService.PersonalBest(cat);
        Assert.NotNull(pb);
        Assert.Equal(TimeSpan.FromSeconds(25), pb.Total);
    }

    [Fact]
    public void BestAndWorstSegment_ReturnsExpectedTimes()
    {
        var cat = CreateTestCategory();
        var s1 = cat.Segments[0].Id;

        cat.Runs.Add(new Run
        {
            Splits = new() { new() { SegmentId = s1, Duration = TimeSpan.FromSeconds(15) } }
        });
        cat.Runs.Add(new Run
        {
            Splits = new() { new() { SegmentId = s1, Duration = TimeSpan.FromSeconds(10) } }
        });
        cat.Runs.Add(new Run
        {
            Splits = new() { new() { SegmentId = s1, Duration = TimeSpan.FromSeconds(20) } }
        });

        var best = StatsService.BestSegment(cat, s1);
        var worst = StatsService.WorstSegment(cat, s1);

        Assert.Equal(TimeSpan.FromSeconds(10), best);
        Assert.Equal(TimeSpan.FromSeconds(20), worst);
    }

    [Fact]
    public void SumOfBest_ReturnsSumOfBestForEachSegment()
    {
        var cat = CreateTestCategory(); // 3 segments
        var s1 = cat.Segments[0].Id;
        var s2 = cat.Segments[1].Id;
        var s3 = cat.Segments[2].Id;

        // Run 1: s1=10, s2=20, s3=30
        cat.Runs.Add(new Run
        {
            Completed = true,
            Splits = new()
            {
                new() { SegmentId = s1, Duration = TimeSpan.FromSeconds(10) },
                new() { SegmentId = s2, Duration = TimeSpan.FromSeconds(20) },
                new() { SegmentId = s3, Duration = TimeSpan.FromSeconds(30) }
            }
        });

        // Run 2: s1=15, s2=15, s3=25 -> Best s1=10, best s2=15, best s3=25 -> Sum = 50
        cat.Runs.Add(new Run
        {
            Completed = true,
            Splits = new()
            {
                new() { SegmentId = s1, Duration = TimeSpan.FromSeconds(15) },
                new() { SegmentId = s2, Duration = TimeSpan.FromSeconds(15) },
                new() { SegmentId = s3, Duration = TimeSpan.FromSeconds(25) }
            }
        });

        var sob = StatsService.SumOfBest(cat);
        Assert.NotNull(sob);
        Assert.Equal(TimeSpan.FromSeconds(50), sob.Value);
    }

    [Fact]
    public void MedianSegment_OddAndEvenCounts()
    {
        var cat = CreateTestCategory();
        var s1 = cat.Segments[0].Id;

        // 3 items: 10, 20, 30 -> Median is 20
        cat.Runs.Add(new Run { Splits = new() { new() { SegmentId = s1, Duration = TimeSpan.FromSeconds(30) } } });
        cat.Runs.Add(new Run { Splits = new() { new() { SegmentId = s1, Duration = TimeSpan.FromSeconds(10) } } });
        cat.Runs.Add(new Run { Splits = new() { new() { SegmentId = s1, Duration = TimeSpan.FromSeconds(20) } } });

        var medOdd = StatsService.MedianSegment(cat, s1);
        Assert.Equal(TimeSpan.FromSeconds(20), medOdd);

        // Add 4th item: 40 -> 10, 20, 30, 40 -> Median is (20+30)/2 = 25
        cat.Runs.Add(new Run { Splits = new() { new() { SegmentId = s1, Duration = TimeSpan.FromSeconds(40) } } });
        var medEven = StatsService.MedianSegment(cat, s1);
        Assert.Equal(TimeSpan.FromSeconds(25), medEven);
    }

    [Fact]
    public void Consistency_CalculatesStandardDeviationCorrectly()
    {
        var cat = CreateTestCategory();
        var s1 = cat.Segments[0].Id;

        // Times: 10, 12, 14. Mean = 12. Variance = ((10-12)^2 + (12-12)^2 + (14-12)^2) / 2 = 8 / 2 = 4. StdDev = 2.
        cat.Runs.Add(new Run { Splits = new() { new() { SegmentId = s1, Duration = TimeSpan.FromSeconds(10) } } });
        cat.Runs.Add(new Run { Splits = new() { new() { SegmentId = s1, Duration = TimeSpan.FromSeconds(12) } } });
        cat.Runs.Add(new Run { Splits = new() { new() { SegmentId = s1, Duration = TimeSpan.FromSeconds(14) } } });

        var sigma = StatsService.Consistency(cat, s1);
        Assert.NotNull(sigma);
        Assert.Equal(2.0, sigma.Value, precision: 4);

        // Consistency rating: (1 - 2/12) * 100 = 83.33%
        var rating = StatsService.ConsistencyRating(cat, s1);
        Assert.NotNull(rating);
        Assert.Equal(83.33, rating.Value, precision: 1);
    }

    [Fact]
    public void DeltaAndFormatting_HandlesPositiveAndNegative()
    {
        var actualAhead = TimeSpan.FromSeconds(45);
        var comp = TimeSpan.FromSeconds(50);
        var deltaAhead = StatsService.Delta(actualAhead, comp);
        Assert.Equal(TimeSpan.FromSeconds(-5), deltaAhead);
        Assert.Equal("-0:05.00", StatsService.FormatDelta(deltaAhead, 2));

        var actualBehind = TimeSpan.FromSeconds(55);
        var deltaBehind = StatsService.Delta(actualBehind, comp);
        Assert.Equal(TimeSpan.FromSeconds(5), deltaBehind);
        Assert.Equal("+0:05.00", StatsService.FormatDelta(deltaBehind, 2));
    }

    [Fact]
    public void BestPossibleTime_CombinesCurrentElapsedWithFutureBestSegments()
    {
        var cat = CreateTestCategory();
        var s1 = cat.Segments[0].Id;
        var s2 = cat.Segments[1].Id;
        var s3 = cat.Segments[2].Id;

        cat.Runs.Add(new Run
        {
            Completed = true,
            Splits = new()
            {
                new() { SegmentId = s1, Duration = TimeSpan.FromSeconds(10) },
                new() { SegmentId = s2, Duration = TimeSpan.FromSeconds(20) },
                new() { SegmentId = s3, Duration = TimeSpan.FromSeconds(30) }
            }
        });

        // Currently in a run: finished s1 in 8s. Next segment is index 1 (s2).
        var currentSplits = new List<SplitTime>
        {
            new() { SegmentId = s1, Duration = TimeSpan.FromSeconds(8) }
        };

        // Best remaining: s2 (20s) + s3 (30s) = 50s.
        // Elapsed: 8s.
        // Best possible: 8 + 50 = 58s.
        var bpt = StatsService.BestPossibleTime(cat, currentSplits, 1);
        Assert.NotNull(bpt);
        Assert.Equal(TimeSpan.FromSeconds(58), bpt.Value);
    }

    [Fact]
    public void IsGold_DetectsNewBestSegment()
    {
        var cat = CreateTestCategory();
        var s1 = cat.Segments[0].Id;

        cat.Runs.Add(new Run
        {
            Splits = new() { new() { SegmentId = s1, Duration = TimeSpan.FromSeconds(15) } }
        });

        // 16s is not gold
        Assert.False(StatsService.IsGold(cat, s1, TimeSpan.FromSeconds(16)));
        // 14s is gold!
        Assert.True(StatsService.IsGold(cat, s1, TimeSpan.FromSeconds(14)));
    }

    [Fact]
    public void ResetHeatmap_CorrectlyCountsResetsPerIndex()
    {
        var cat = CreateTestCategory();

        // 2 resets at index 0, 1 reset at index 1, 1 completed
        cat.Runs.Add(new Run { Completed = false, ResetAtSegmentIndex = 0 });
        cat.Runs.Add(new Run { Completed = false, ResetAtSegmentIndex = 0 });
        cat.Runs.Add(new Run { Completed = false, ResetAtSegmentIndex = 1 });
        cat.Runs.Add(new Run { Completed = true });

        var heatmap = StatsService.ResetHeatmap(cat);
        Assert.Equal(3, heatmap.Length);
        Assert.Equal(2, heatmap[0]);
        Assert.Equal(1, heatmap[1]);
        Assert.Equal(0, heatmap[2]);
    }
}
