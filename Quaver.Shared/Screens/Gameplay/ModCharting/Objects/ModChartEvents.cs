using System;
using System.Collections.Generic;
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


    private readonly Dictionary<ModChartEventCategory, ModChartCategorizedEvent> _events = new();
    [MoonSharpHidden] internal ModChartDeferredEventQueue DeferredEventQueue { get; }

    public ModChartCategorizedEvent this[ModChartEventCategory category]
    {
        get
        {
            _events.TryAdd(category, new ModChartCategorizedEvent(category));
            return _events[category];
        }
    }

    public void Invoke(ModChartEventType eventType, params object[] args)
    {
        this[eventType.Category].Invoke(eventType.SpecificType, args);
    }

    public void Invoke(ModChartEventCategory category, ulong specificType, params object[] args)
    {
        this[category].Invoke(specificType, args);
    }

    public void Invoke(Enum specificType, params object[] args)
    {
        var category = GetCategory(specificType);
        Invoke(category, Convert.ToUInt64(specificType), args);
    }

    public void Invoke(ulong specificType, params object[] args)
    {
        Invoke(ModChartEventCategory.Custom, specificType, args);
    }

    public void Enqueue(ModChartEventInstance eventInstance) => DeferredEventQueue.Enqueue(eventInstance);

    public void Enqueue(ModChartEventType type, params object[] args) =>
        DeferredEventQueue.Enqueue(new ModChartEventInstance(type, args));

    public void Enqueue(Enum specificType, params object[] args) => Enqueue(GetType(specificType), args);

    public void Enqueue(ulong specificType, params object[] args) =>
        Enqueue(new ModChartEventType(ModChartEventCategory.Custom, specificType), args);

    public static ModChartEventType GetType(Enum specificType) => ModChartEventType.From(specificType);

    public static ModChartEventCategory GetCategory(Enum specificType) => specificType switch
    {
        ModChartNoteEventType => ModChartEventCategory.Note,
        ModChartInputEventType => ModChartEventCategory.Input,
        _ => ModChartEventCategory.None
    };

    private void CallNoteEntry(GameplayHitObjectKeysInfo info) =>
        Invoke(ModChartNoteEventType.NoteEntry, info);

    private void CallOnKeyPress(GameplayHitObjectKeysInfo info, int pressTime, Judgement judgement)
    {
        Invoke(ModChartInputEventType.KeyPress, info, pressTime, judgement);
    }

    private void CallOnKeyRelease(GameplayHitObjectKeysInfo info, int pressTime, Judgement judgement)
    {
        Invoke(ModChartInputEventType.KeyRelease, info, pressTime, judgement);
    }

    public ModChartEvents(GameplayScreenView gameplayScreenView)
    {
        Shortcut = new ElementAccessShortcut(gameplayScreenView);
        DeferredEventQueue = new ModChartDeferredEventQueue(this);
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