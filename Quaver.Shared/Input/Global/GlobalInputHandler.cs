using System;
using System.Collections.Generic;
using System.Linq;
using Wobble.Bindables;
using Wobble.Input;

namespace Quaver.Shared.Input.Global;

public class GlobalInputHandler : IInputHandler<GlobalKeybindActions>
{
    private readonly Dictionary<GlobalKeybindActions, Bindable<bool>> _invertScrollingActions = [];

    private static readonly HashSet<GlobalKeybindActions> HoldRepeatActions = [];

    private static readonly HashSet<GlobalKeybindActions> HoldAndReleaseActionsSet = [];

    /// <inheritdoc />
    public bool? InvertedScrolling(GlobalKeybindActions action)
    {
        if (!_invertScrollingActions.TryGetValue(action, out var bindable))
            return null;
        return bindable.Value;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void HandleAction(GlobalKeybindActions action, bool isKeyPress = true,
        bool isRelease = false)
    {
        var scopes = GlobalInputManager.ScopeTokens.AsEnumerable().Reverse().ToList();
        foreach (var scope in scopes)
        {
            var shouldBreak = false;
            switch (scope.Handle(action, isKeyPress, isRelease))
            {
                case GlobalInputHandleResult.Consumed:
                    shouldBreak = true;
                    break;
                case GlobalInputHandleResult.Pass:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (shouldBreak)
                break;
        }
    }

    /// <inheritdoc />
    public void HandleCustomActions(GenericKeyState keyState, GenericKeyState previousKeyState,
        HashSet<Keybind> uniqueKeyPresses)
    {
    }

    /// <inheritdoc />
    public void HandleActionCombination(Dictionary<Keybind, HashSet<GlobalKeybindActions>> actions,
        HashSet<Keybind> uniqueKeyPresses)
    {
    }

    /// <inheritdoc />
    public bool IsHoldRepeat(GlobalKeybindActions action) => HoldRepeatActions.Contains(action);

    /// <inheritdoc />
    public bool IsHoldAndRelease(GlobalKeybindActions action) =>
        HoldAndReleaseActionsSet.Contains(action);

    /// <inheritdoc />
    public IEnumerable<GlobalKeybindActions> HoldAndReleaseActions => HoldAndReleaseActionsSet;

    /// <inheritdoc />
    public bool IsKeybindBlocked(GenericKey key) => false;

    /// <inheritdoc />
    public bool InFocus => false;
}
