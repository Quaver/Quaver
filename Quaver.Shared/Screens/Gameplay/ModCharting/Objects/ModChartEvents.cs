using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using Quaver.API.Enums;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;
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


    private readonly Dictionary<ModChartEventType, ModChartCategorizedEvent> _events = new();
    [MoonSharpHidden] internal ModChartDeferredEventQueue DeferredEventQueue { get; }

    public ModChartCategorizedEvent this[ModChartEventType type]
    {
        get
        {
            var category = type.GetCategory();
            _events.TryAdd(category, new ModChartCategorizedEvent(category));
            return _events[category];
        }
    }

    public void Invoke(ModChartEventType eventType, params object[] args)
    {
        this[eventType].Invoke(eventType, args);
    }

    public void Enqueue(ModChartEventInstance eventInstance) => DeferredEventQueue.Enqueue(eventInstance);

    public void Enqueue(ModChartEventType type, params object[] args) =>
        DeferredEventQueue.Enqueue(new ModChartEventInstance(type, args));

    public void Enqueue(Closure closure, params object[] args) => Enqueue(new ModChartEventInstance(
        ModChartEventType.FunctionCall,
        new object[] { closure }.Concat(args).ToArray()));

    public static int GetSpecificType(ModChartEventType eventType) => eventType.GetSpecificType();

    public static ModChartEventType GetCategory(ModChartEventType eventType) => eventType.GetCategory();

    public static ModChartEventType CustomEventType(int id) => ModChartEventType.Custom.WithSpecificType(id);

    private void CallNoteEntry(GameplayHitObjectKeysInfo info)
    {
        Invoke(ModChartEventType.NoteEntry, info);
    }

    private void CallOnKeyPress(GameplayHitObjectKeysInfo info, int pressTime, Judgement judgement)
    {
        Invoke(ModChartEventType.InputKeyPress, info, pressTime, judgement);
    }

    private void CallOnKeyRelease(GameplayHitObjectKeysInfo info, int pressTime, Judgement judgement)
    {
        Invoke(ModChartEventType.InputKeyRelease, info, pressTime, judgement);
    }

    public void Subscribe(ModChartEventType eventType, Closure closure)
    {
        var (category, specificType) = eventType;
        if (specificType == 0)
            this[category].Add(closure);
        else
            this[category][specificType].Add(closure);
    }

    public void Unsubscribe(ModChartEventType eventType, Closure closure)
    {
        var (category, specificType) = eventType;
        if (specificType == 0)
            this[category].Remove(closure);
        else
            this[category][specificType].Remove(closure);
    }

    public ModChartEvents(GameplayScreenView gameplayScreenView)
    {
        Shortcut = new ElementAccessShortcut(gameplayScreenView);
        DeferredEventQueue = new ModChartDeferredEventQueue(this);
        this[ModChartEventType.Function][ModChartEventType.FunctionCall].OnInvoke += (type, args) =>
        {
            if (args.Length < 1) return;
            var closure = (Closure)args[0];
            closure.Call(args[1..]);
        };
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