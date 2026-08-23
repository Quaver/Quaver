using System.Collections.Generic;
using Wobble.Input;

namespace Quaver.Shared.Input;

public interface IInputHandler<TKeybindActions>
{
    void HandleAction(TKeybindActions action, bool isKeyPress = true, bool isRelease = false);

    void HandleActionCombination(Dictionary<Keybind, HashSet<TKeybindActions>> actions,
        HashSet<Keybind> uniqueKeyPresses);

    bool IsHoldRepeat(TKeybindActions action);
    bool IsHoldAndRelease(TKeybindActions action);

    IEnumerable<TKeybindActions> HoldAndReleaseActions { get; }
    bool? InvertedScrolling(TKeybindActions action);

    bool IsKeybindBlocked(GenericKey key);
    bool InFocus { get; }
    void HandleCustomActions(GenericKeyState keyState, GenericKeyState previousKeyState, HashSet<Keybind> uniqueKeyPresses);
}