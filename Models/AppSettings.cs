namespace Deeps.Models;

public class AppSettings
{
    // Hotkeys (key name, as reported by KeyboardEvent.key)
    public string HotkeyStart    { get; set; } = " ";        // Space
    public string HotkeySplit    { get; set; } = " ";        // Space (same as start)
    public string HotkeyPause    { get; set; } = "p";
    public string HotkeyReset    { get; set; } = "r";
    public string HotkeyUndo     { get; set; } = "Backspace";
    public string HotkeySkip     { get; set; } = "s";

    // Gamepad button bindings (-1 = disabled, 0=A/Cross, 1=B/Circle, 2=X/Square, 3=Y/Triangle, etc.)
    public bool GamepadEnabled          { get; set; } = true;
    public int  GamepadButtonStartSplit { get; set; } = 0; // A / Cross
    public int  GamepadButtonPause      { get; set; } = 9; // Start / Options
    public int  GamepadButtonReset      { get; set; } = 8; // Back / Select / Share
    public int  GamepadButtonUndo       { get; set; } = 1; // B / Circle
    public int  GamepadButtonSkip       { get; set; } = 2; // X / Square

    // Timer display precision
    public int TimerDecimalPlaces { get; set; } = 2;         // 0,1,2,3

    // Comparison mode
    public string Comparison { get; set; } = "PersonalBest"; // PersonalBest|BestSegments|Average|Median|Worst

    // Timing method
    public TimingMethod TimingMethod { get; set; } = TimingMethod.RealTime;

    // Countdown before run starts (seconds, 0 = instant)
    public int StartDelay { get; set; } = 0;

    // Theme (reserved for later)
    public string Theme { get; set; } = "dark";
}
