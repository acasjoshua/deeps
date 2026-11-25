using Deeps.Models;
using Microsoft.JSInterop;
using System.Text;
using System.Xml.Linq;

namespace Deeps.Services;

/// <summary>Import/export: LiveSplit .lss XML, JSON, CSV.</summary>
public static class ImportExportService
{
    // ─── JSON ─────────────────────────────────────────────────────────────────

    public static string ExportJson(List<Game> games)
    {
        var opts = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
        return System.Text.Json.JsonSerializer.Serialize(games, opts);
    }

    // ─── CSV ──────────────────────────────────────────────────────────────────

    public static string ExportCsv(Category cat)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Segment,Run Date,Duration (s),Is Gold,Mistakes,Seconds Lost");
        foreach (var run in cat.Runs.Where(r => r.Completed).OrderBy(r => r.StartedAt))
        {
            foreach (var split in run.Splits)
            {
                var seg  = cat.Segments.FirstOrDefault(s => s.Id == split.SegmentId);
                var mistakes = run.Mistakes.Count(m => m.SegmentId == split.SegmentId);
                var lost     = run.Mistakes.Where(m => m.SegmentId == split.SegmentId).Sum(m => m.SecondsLost);
                sb.AppendLine(
                    $"\"{seg?.Name ?? split.SegmentId.ToString()}\"," +
                    $"\"{run.StartedAt:yyyy-MM-dd HH:mm}\"," +
                    $"{split.Duration.TotalSeconds:F3}," +
                    $"{(split.IsGold ? "yes" : "no")}," +
                    $"{mistakes},{lost:F2}");
            }
        }
        return sb.ToString();
    }

    // ─── LiveSplit .lss ───────────────────────────────────────────────────────

    public static string ExportLss(Game game, Category cat)
    {
        var pb  = StatsService.PersonalBest(cat);
        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement("Run", new XAttribute("version", "1.7.0"),
                new XElement("GameIcon"),
                new XElement("GameName",     game.Name),
                new XElement("CategoryName", cat.Name),
                new XElement("Metadata",
                    new XElement("Run",      new XAttribute("id", "")),
                    new XElement("Platform", new XAttribute("usesEmulator", "False"), ""),
                    new XElement("Region")),
                new XElement("Offset", "00:00:00"),
                new XElement("AttemptCount", cat.AttemptCount),
                new XElement("AttemptHistory",
                    cat.Runs.Select((r, idx) => new XElement("Attempt",
                        new XAttribute("id",      idx + 1),
                        new XAttribute("started", r.StartedAt.ToString("MM/dd/yyyy HH:mm:ss")),
                        new XAttribute("isStartedSynced", "True"),
                        new XAttribute("ended",   r.StartedAt.Add(r.Total).ToString("MM/dd/yyyy HH:mm:ss")),
                        new XAttribute("isEndedSynced", "True"),
                        r.Completed
                            ? new XElement("RealTime", FormatLssTime(r.Total))
                            : (object)new XElement("Ended", new XAttribute("time", "1"))))),
                new XElement("Segments",
                    cat.Segments.Select(seg =>
                    {
                        var pbSplit  = pb?.Splits.FirstOrDefault(s => s.SegmentId == seg.Id);
                        var bestSeg  = StatsService.BestSegment(cat, seg.Id);
                        return new XElement("Segment",
                            new XElement("Name", seg.Name),
                            new XElement("Icon"),
                            new XElement("SplitTimes",
                                new XElement("SplitTime", new XAttribute("name", "Personal Best"),
                                    pbSplit is not null
                                        ? new XElement("RealTime", FormatLssTime(SumUpTo(cat, pb!, seg.Id)))
                                        : null)),
                            new XElement("BestSegmentTime",
                                bestSeg.HasValue
                                    ? new XElement("RealTime", FormatLssTime(bestSeg.Value))
                                    : null),
                            new XElement("SegmentHistory",
                                cat.Runs.Select((r, idx) =>
                                {
                                    var split = r.Splits.FirstOrDefault(s => s.SegmentId == seg.Id);
                                    return split is null ? null
                                        : (object)new XElement("Time", new XAttribute("id", idx + 1),
                                            new XElement("RealTime", FormatLssTime(split.Duration)));
                                }).OfType<object>()));
                    })),
                new XElement("AutoSplitterSettings")));
        return doc.Declaration + "\r\n" + doc.ToString();
    }

    private static string FormatLssTime(TimeSpan t)
        => $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}.{t.Milliseconds:000}0000";

    private static TimeSpan SumUpTo(Category cat, Run run, Guid segId)
    {
        var total = TimeSpan.Zero;
        foreach (var seg in cat.Segments)
        {
            var s = run.Splits.FirstOrDefault(x => x.SegmentId == seg.Id);
            if (s is not null) total += s.Duration;
            if (seg.Id == segId) break;
        }
        return total;
    }

    // ─── LiveSplit .lss import ────────────────────────────────────────────────

    public static (Game game, Category cat)? ImportLss(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            var root = doc.Root!;

            var game = new Game { Name = root.Element("GameName")?.Value ?? "Imported Game" };
            var cat  = new Category
            {
                GameId       = game.Id,
                Name         = root.Element("CategoryName")?.Value ?? "Any%",
                AttemptCount = int.TryParse(root.Element("AttemptCount")?.Value, out var ac) ? ac : 0
            };
            game.Categories.Add(cat);

            var segEls = root.Element("Segments")?.Elements("Segment").ToList() ?? new();
            int order  = 0;
            foreach (var el in segEls)
            {
                cat.Segments.Add(new Segment
                {
                    Name  = el.Element("Name")?.Value ?? "",
                    Order = order++
                });
            }

            // Reconstruct attempt history from SegmentHistory
            var attemptIds = root.Element("AttemptHistory")?.Elements("Attempt")
                .Select(a => int.Parse(a.Attribute("id")!.Value)).ToHashSet() ?? new();

            foreach (var id in attemptIds.OrderBy(x => x))
            {
                var splits = new List<SplitTime>();
                bool complete = true;
                for (int i = 0; i < segEls.Count; i++)
                {
                    var segEl = segEls[i];
                    var seg   = cat.Segments[i];
                    var timeEl = segEl.Element("SegmentHistory")?
                        .Elements("Time").FirstOrDefault(t => t.Attribute("id")?.Value == id.ToString());

                    if (timeEl?.Element("RealTime")?.Value is string ts && ParseLssTime(ts) is TimeSpan dur)
                        splits.Add(new SplitTime { SegmentId = seg.Id, Duration = dur });
                    else
                        complete = false;
                }
                cat.Runs.Add(new Run { Completed = complete, Splits = splits });
            }

            return (game, cat);
        }
        catch { return null; }
    }

    private static TimeSpan? ParseLssTime(string s)
    {
        try { return TimeSpan.Parse(s[..Math.Min(s.Length, 12)]); }
        catch { return null; }
    }

    // ─── Browser download helper ──────────────────────────────────────────────

    public static async Task DownloadAsync(IJSRuntime js, string filename, string content, string mime = "text/plain")
    {
        var bytes  = Encoding.UTF8.GetBytes(content);
        var base64 = Convert.ToBase64String(bytes);
        await js.InvokeVoidAsync("deepsDownload", filename, base64, mime);
    }
}
