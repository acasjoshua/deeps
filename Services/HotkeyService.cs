using Deeps.Models;

namespace Deeps.Services;

/// <summary>
/// Thin wrapper around AppSettings that lets components subscribe to setting changes.
/// The store owns the Settings object; this service provides hotkey matching helpers.
/// </summary>
public class HotkeyService
{
    private readonly PracticeStore _store;
    public HotkeyService(PracticeStore store) => _store = store;

    private AppSettings S => _store.Settings;

    public bool IsStartOrSplit(string key)   => MatchKey(key, S.HotkeyStart);
    public bool IsPause(string key)          => MatchKey(key, S.HotkeyPause);
    public bool IsReset(string key)          => MatchKey(key, S.HotkeyReset);
    public bool IsUndo(string key)           => MatchKey(key, S.HotkeyUndo);
    public bool IsSkip(string key)           => MatchKey(key, S.HotkeySkip);

    private static bool MatchKey(string key, string bound) =>
        string.Equals(key, bound, StringComparison.OrdinalIgnoreCase);

    public string DisplayKey(string key) => key switch
    {
        " "         => "Space",
        "Backspace" => "⌫",
        "ArrowUp"   => "↑",
        "ArrowDown" => "↓",
        "ArrowLeft" => "←",
        "ArrowRight"=> "→",
        "Enter"     => "↵",
        "Escape"    => "Esc",
        _           => key.ToUpperInvariant()
    };
}
