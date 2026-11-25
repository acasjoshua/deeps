namespace Deeps.Models;

public sealed class OverlaySnapshot
{
    public string GameName { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string State { get; set; } = "Ready";
    public string Elapsed { get; set; } = "";
    public int Countdown { get; set; }
    public string? SegmentName { get; set; }
    public string? LastSplit { get; set; }
    public string? Delta { get; set; }
    public string? PersonalBest { get; set; }
}
