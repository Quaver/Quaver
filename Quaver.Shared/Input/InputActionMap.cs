using System.Collections.Generic;
using System.Linq;

namespace Quaver.Shared.Input;

public class InputActionMap<TKeybindActions> where TKeybindActions : notnull
{
    private readonly Dictionary<TKeybindActions, KeybindList> _keybinds;
    private Dictionary<Keybind, HashSet<TKeybindActions>> _keybindActions = [];

    public InputActionMap(Dictionary<TKeybindActions, KeybindList> keybinds)
    {
        _keybinds = keybinds;
        RebuildReverseDictionary();
    }

    public KeybindList GetOrDefault(TKeybindActions action) =>
        _keybinds.GetValueOrDefault(action, new KeybindList());

    public void AddKeybindToAction(TKeybindActions action, Keybind keybind)
    {
        if (!_keybinds.TryGetValue(action, out KeybindList? value))
            _keybinds.Add(action, new KeybindList(keybind));
        else
            value.Add(keybind);

        foreach (var matchingKeybind in keybind.MatchingKeybinds())
        {
            if (!_keybindActions.TryGetValue(matchingKeybind, out var actions))
            {
                actions = [];
                _keybindActions.Add(matchingKeybind, actions);
            }

            actions.Add(action);
        }
    }

    public bool RemoveKeybindFromAction(TKeybindActions action, Keybind keybind)
    {
        if (!_keybinds.TryGetValue(action, out KeybindList? value) || !value.Remove(keybind))
            return false;

        var remainingMatchingKeybinds =
            value.SelectMany(k => k.MatchingKeybinds()).ToHashSet();

        foreach (var matchingKeybind in keybind.MatchingKeybinds())
        {
            if (remainingMatchingKeybinds.Contains(matchingKeybind))
                continue;

            if (_keybindActions.TryGetValue(matchingKeybind, out var actions))
                actions.Remove(action);
        }

        return true;
    }

    public KeybindList? SetKeybindsForAction(TKeybindActions action,
        KeybindList keybindList)
    {
        // Remove from reverse dict
        var previousList = _keybinds.GetValueOrDefault(action) ?? [];
        foreach (var keybind in previousList.SelectMany(k => k.MatchingKeybinds()))
        {
            if (_keybindActions.TryGetValue(keybind, out var actions))
            {
                actions.Remove(action);
            }
        }

        _keybinds[action] = keybindList;

        // Add to reverse dict
        foreach (var keybind in keybindList.SelectMany(k => k.MatchingKeybinds()))
        {
            if (!_keybindActions.TryGetValue(keybind, out var actions))
            {
                actions = [];
                _keybindActions.Add(keybind, actions);
            }

            actions.Add(action);
        }

        return previousList;
    }

    public bool SetKeybindsForActionIfNotExits(TKeybindActions action,
        KeybindList keybindList)
    {
        if (_keybinds.ContainsKey(action))
            return false;

        SetKeybindsForAction(action, keybindList);
        return true;
    }

    private void RebuildReverseDictionary()
    {
        _keybindActions = new Dictionary<Keybind, HashSet<TKeybindActions>>();

        foreach (var (action, keybinds) in _keybinds)
        {
            foreach (var keybind in keybinds.MatchingKeybinds())
            {
                if (_keybindActions.ContainsKey(keybind))
                    _keybindActions[keybind].Add(action);
                else
                    _keybindActions[keybind] = [action];
            }
        }
    }

    public bool TryGetActionsFor(Keybind keybind, out HashSet<TKeybindActions>? set) =>
        _keybindActions.TryGetValue(keybind, out set);
}