using MoonSharp.Interpreter;
using Quaver.API.Enums;
using Quaver.Shared.Screens.Gameplay.Rulesets.Input;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;

// ReSharper disable ExpressionIsAlwaysNull

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

[MoonSharpUserData]
public class ModChartEvents
{
    private ElementAccessShortcut Shortcut { get; set; }

    public HitObjectManagerKeys HitObjectManagerKeys =>
        (HitObjectManagerKeys)Shortcut.GameplayScreen.Ruleset.HitObjectManager;

    internal KeysInputManager InputManager => (KeysInputManager)HitObjectManagerKeys.Ruleset.InputManager;


    public ModChartEvent NoteEntry = new();
    public ModChartEvent KeyPress = new();
    public ModChartEvent KeyRelease = new();
    private void CallNoteEntry(GameplayHitObjectKeysInfo info) => NoteEntry.Invoke(info);

    private void CallOnKeyPress(GameplayHitObjectKeysInfo info, int pressTime, Judgement judgement)
    {
        KeyPress.Invoke(info, pressTime, judgement);
    }

    private void CallOnKeyRelease(GameplayHitObjectKeysInfo info, int pressTime, Judgement judgement)
    {
        KeyRelease.Invoke(info, pressTime, judgement);
    }

    public ModChartEvents(GameplayScreenView gameplayScreenView)
    {
        Shortcut = new ElementAccessShortcut(gameplayScreenView);
        HitObjectManagerKeys.RenderedHitObjectInfoAdded += CallNoteEntry;
        InputManager.OnKeyPress += CallOnKeyPress;
        InputManager.OnKeyRelease += CallOnKeyRelease;
    }

    ~ModChartEvents()
    {
        HitObjectManagerKeys.RenderedHitObjectInfoAdded -= CallNoteEntry;
        InputManager.OnKeyPress -= CallOnKeyPress;
        InputManager.OnKeyRelease -= CallOnKeyRelease;
    }
}