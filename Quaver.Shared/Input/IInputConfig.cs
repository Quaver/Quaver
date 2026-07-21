using System.Collections.Generic;

namespace Quaver.Shared.Input;

public interface IInputConfig<TKeybindActions>
{
    IReadOnlyDictionary<TKeybindActions, KeybindList> ReadOnlyKeybinds { get; }
    KeybindList GetOrDefault(TKeybindActions action);
    void AddKeybindToAction(TKeybindActions action, Keybind keybind);
    bool RemoveKeybindFromAction(TKeybindActions action, Keybind keybind);
    KeybindList? SetKeybindsForAction(TKeybindActions action, KeybindList keybindList);
    bool TryGetActionsFor(Keybind keybind, out HashSet<TKeybindActions>? set);
    int FillMissingKeys(bool fillWithDefaultBinds);
    KeybindList DefaultKeybindsFor(TKeybindActions action);
    void ResetConfigFile();
}