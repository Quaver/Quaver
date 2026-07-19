using System.Collections.Generic;

namespace Quaver.Shared.Input;

public interface IInputConfig<KeybindActions>
{
    IReadOnlyDictionary<KeybindActions, KeybindList> ReadOnlyKeybinds { get; }
    KeybindList GetOrDefault(KeybindActions action);
    void AddKeybindToAction(KeybindActions action, Keybind keybind);
    bool RemoveKeybindFromAction(KeybindActions action, Keybind keybind);
    KeybindList? SetKeybindsForAction(KeybindActions action, KeybindList keybindList);
    bool TryGetActionsFor(Keybind keybind, out HashSet<KeybindActions>? set);
    int FillMissingKeys(bool fillWithDefaultBinds);
    KeybindList DefaultKeybindsFor(KeybindActions action);
    void ResetConfigFile();
}