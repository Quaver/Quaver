using System.Collections.Generic;
using Wobble.Bindables;
using Wobble.Graphics.UI.Dialogs;
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
    public void HandleAction(GlobalKeybindActions action, bool isKeyPress = true,
        bool isRelease = false)
    {
        switch (action)
        {
            default:
                return;
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

    public GlobalInputHandler(GlobalInputConfig globalInputConfig)
    {
        GlobalInputConfig = globalInputConfig;
    }

    /// <inheritdoc />
    public bool IsKeybindBlocked(GenericKey key) => false;

    /// <inheritdoc />
    public bool InFocus => DialogManager.Dialogs.Count != 0;

    private GlobalInputConfig GlobalInputConfig { get; set; }
}