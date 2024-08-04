using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Edit.Actions;

// ReSharper disable ExpressionIsAlwaysNull

namespace Quaver.Shared.Screens.Edit.LuaEvents;

[MoonSharpUserData]
public class EditorEvents
{
    private readonly Dictionary<EditorActionType, EditorEvent> _events = new();

    /// <summary>
    ///     Gets the corresponding event handler for the action type
    /// </summary>
    /// <param name="type">The type of the event handler</param>
    public EditorEvent this[EditorActionType type]
    {
        get
        {
            if (_events.TryGetValue(type, out var editorEvent)) return editorEvent;

            editorEvent = new EditorEvent();
            // None is treated as global action: every action will trigger this
            editorEvent.Add(a => _events[EditorActionType.None].Invoke(a));
            _events.Add(type, editorEvent);
            return editorEvent;
        }
    }

    /// <summary>
    ///     Subscribes to a particular event, or the whole category of events.<br/>
    ///     The function has the signature <c>function (type: event_types, args: object[])</c>
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="closure"></param>
    /// <seealso cref="EditorActionType"/>
    /// <seealso cref="EditorEvent"/>
    public void Subscribe(EditorActionType eventType, Closure closure)
    {
        this[eventType].Add(closure);
    }

    /// <summary>
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="action"></param>
    public void Subscribe(EditorActionType eventType, Action<EditorEventInstance> action)
    {
        this[eventType].Add(action);
    }

    /// <summary>
    ///     Unsubscribes to a particular event, or the whole category of events
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="closure"></param>
    /// <seealso cref="Subscribe(Quaver.Shared.Screens.Edit.Actions.EditorActionType,MoonSharp.Interpreter.Closure)"/>
    /// <seealso cref="EditorActionType"/>
    /// <seealso cref="EditorEvent"/>
    public void Unsubscribe(EditorActionType eventType, Closure closure)
    {
        this[eventType].Remove(closure);
    }

    /// <summary>
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="action"></param>
    public void Unsubscribe(EditorActionType eventType, Action<EditorEventInstance> action)
    {
        this[eventType].Remove(action);
    }

    public EditorEvents()
    {
        // The None event action marks the global event: any actions triggered will call this event
        _events.Add(EditorActionType.None, new EditorEvent());
    }
}