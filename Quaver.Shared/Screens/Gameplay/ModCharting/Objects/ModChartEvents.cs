using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter;
using Quaver.API.Enums;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events.Arguments;
using Quaver.Shared.Screens.Gameplay.Rulesets.Input;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Wobble.Logging;

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    public ModChartEvent this[ModChartEventType type]
    {
        get
        {
            var (category, specificType) = type;
            _events.TryAdd(category, new ModChartCategorizedEvent(category));
            return specificType == 0 ? _events[category] : _events[category][specificType];
        }
    }

    public void Invoke(ModChartEventInstance instance)
    {
        this[instance.EventType.GetCategory()].Invoke(instance);
    }

    /// <summary>
    ///     Queues an event to be processed at the end of <see cref="ModChartScript.Update"/>.
    /// </summary>
    /// <param name="eventInstance"></param>
    public void Enqueue(ModChartEventInstance eventInstance) => DeferredEventQueue.Enqueue(eventInstance);

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Enqueue(Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events.Arguments.ModChartEventInstance)"/>
    /// <param name="type"></param>
    /// <param name="args"></param>
    public void Enqueue(ModChartEventType type, params object[] args) =>
        DeferredEventQueue.Enqueue(GetArguments(type, args));

    /// <summary>
    ///     Queues a function call to be invoked at the end of <see cref="ModChartScript.Update"/>.
    /// </summary>
    /// <param name="closure">The lua function to call</param>
    /// <param name="args">The arguments of the function</param>
    public void Enqueue(Closure closure, params object[] args) =>
        Enqueue(new ModChartEventFunctionCallInstance(closure, args));

    public static int GetSpecificType(ModChartEventType eventType) => eventType.GetSpecificType();

    public static ModChartEventType GetCategory(ModChartEventType eventType) => eventType.GetCategory();

    public static ModChartEventType CustomEventType(int id) => ModChartEventType.Custom.WithSpecificType(id);

    private void CallNoteEntry(GameplayHitObjectKeysInfo info)
    {
        Invoke(new ModChartEventNoteEntryInstance(info));
    }

    private void CallOnKeyPress(GameplayHitObjectKeysInfo info, int pressTime, Judgement judgement)
    {
        Invoke(new ModChartEventInputKeyPressInstance(info, pressTime, judgement));
    }

    private void CallOnKeyRelease(GameplayHitObjectKeysInfo info, int pressTime, Judgement judgement)
    {
        Invoke(new ModChartEventInputKeyReleaseInstance(info, pressTime, judgement));
    }

    /// <summary>
    ///     Subscribes to a particular event, or the whole category of events.<br/>
    ///     The function has the signature <c>function (type: event_types, args: object[])</c>
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="closure"></param>
    /// <example>
    /// The following lua code subscribes a closure to a category:
    /// <code language="lua">
    /// <![CDATA[
    ///     function noteEvents(type, args)
    ///         print("Some note related events are triggered, namely " .. type)
    ///     end
    ///     events.subscribe(event_types.note, noteEvents)
    /// ]]>
    /// </code>
    /// The following lua code subscribes a closure to a specific type of event, under a specified category:
    /// <code language="lua">
    /// <![CDATA[
    ///     function onNoteEntry(type, args)
    ///         hitObject = args[1]
    ///         print("A note has become visible in the playfield at " .. hitObject.startTime)
    ///     end
    ///     events.subscribe(event_types.noteEntry, onNoteEntry)
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="ModChartEventType"/>
    /// <seealso cref="ModChartEvent"/>
    /// <seealso cref="ModChartCategorizedEvent"/>
    public void Subscribe(ModChartEventType eventType, Closure closure)
    {
        this[eventType].Add(closure);
    }

    /// <summary>
    ///     Unsubscribes to a particular event, or the whole category of events
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="closure"></param>
    /// <seealso cref="Subscribe"/>
    /// <seealso cref="ModChartEventType"/>
    /// <seealso cref="ModChartEvent"/>
    /// <seealso cref="ModChartCategorizedEvent"/>
    public void Unsubscribe(ModChartEventType eventType, Closure closure)
    {
        this[eventType].Remove(closure);
    }

    public ModChartEvents(ElementAccessShortcut shortcut)
    {
        Shortcut = shortcut;
        DeferredEventQueue = new ModChartDeferredEventQueue(this);
        this[ModChartEventType.FunctionCall].OnInvoke += args =>
        {
            if (args is not ModChartEventFunctionCallInstance functionCallArguments) return;
            functionCallArguments.Closure.SafeCall(functionCallArguments.Arguments);
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

    private static ModChartEventInstance GetArguments(Type type, params object[] args)
    {
        foreach (var constructorInfo in type.GetConstructors())
        {
            if (!constructorInfo.IsPublic || constructorInfo.IsAbstract) continue;
            try
            {
                return constructorInfo.Invoke(args) as ModChartEventInstance;
            }
            catch (MemberAccessException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (TargetParameterCountException)
            {
            }
        }

        throw new InvalidOperationException($"Constructing event instance {type} with params {args}");
    }

    private static ModChartEventInstance GetArguments(ModChartEventType eventType, params object[] args)
    {
        var type = eventType switch
        {
            ModChartEventType.FunctionCall => typeof(ModChartEventFunctionCallInstance),
            ModChartEventType.TimelineAddSegment => typeof(ModChartEventAddSegmentInstance),
            ModChartEventType.TimelineRemoveSegment => typeof(ModChartEventRemoveSegmentInstance),
            ModChartEventType.TimelineUpdateSegment => typeof(ModChartEventUpdateSegmentInstance),
            ModChartEventType.TimelineAddTrigger => typeof(ModChartEventAddTriggerInstance),
            ModChartEventType.TimelineRemoveTrigger => typeof(ModChartEventRemoveTriggerInstance),
            ModChartEventType.TimelineUpdateTrigger => typeof(ModChartEventUpdateTriggerInstance),
            ModChartEventType.NoteEntry => typeof(ModChartEventNoteEntryInstance),
            ModChartEventType.InputKeyPress => typeof(ModChartEventInputKeyPressInstance),
            ModChartEventType.InputKeyRelease => typeof(ModChartEventInputKeyReleaseInstance),
            _ => throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null)
        };
        return GetArguments(type, args);
    }
}