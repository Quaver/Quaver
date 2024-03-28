using System;
using MoonSharp.Interpreter;
using Quaver.API.Enums;
using Quaver.Shared.Screens.Gameplay.Rulesets.Input;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;

// ReSharper disable ExpressionIsAlwaysNull

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Scripting;

[MoonSharpUserData]
public class StoryboardEvents
{
    private ElementAccessShortcut Shortcut { get; set; }

    public HitObjectManagerKeys HitObjectManagerKeys =>
        (HitObjectManagerKeys)Shortcut.GameplayScreen.Ruleset.HitObjectManager;

    internal KeysInputManager InputManager => (KeysInputManager)HitObjectManagerKeys.Ruleset.InputManager;


    public event Action<GameplayHitObjectKeysInfo> NoteEntry;
    public event Action<GameplayHitObjectKeysInfo, int, Judgement> OnKeyPress;
    public event Action<GameplayHitObjectKeysInfo, int, Judgement> OnKeyRelease;
    private void CallNoteEntry(GameplayHitObjectKeysInfo info) => NoteEntry?.Invoke(info);

    private void CallOnKeyPress(GameplayHitObjectKeysInfo info, int pressTime, Judgement judgement) =>
        OnKeyPress?.Invoke(info, pressTime, judgement);

    private void CallOnKeyRelease(GameplayHitObjectKeysInfo info, int pressTime, Judgement judgement) =>
        OnKeyRelease?.Invoke(info, pressTime, judgement);

    public StoryboardEvents(GameplayScreenView gameplayScreenView)
    {
        Shortcut = new ElementAccessShortcut(gameplayScreenView);
        HitObjectManagerKeys.RenderedHitObjectInfoAdded += CallNoteEntry;
        InputManager.OnKeyPress += CallOnKeyPress;
        InputManager.OnKeyRelease += CallOnKeyRelease;
    }

    ~StoryboardEvents()
    {
        HitObjectManagerKeys.RenderedHitObjectInfoAdded -= CallNoteEntry;
        InputManager.OnKeyPress -= CallOnKeyPress;
        InputManager.OnKeyRelease -= CallOnKeyRelease;
    }
}