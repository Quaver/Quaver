using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Gameplay;
using Wobble.Bindables;
using Wobble.Input;
using Wobble.Logging;

namespace Quaver.Shared.Input.Global;

public class GlobalInputHandler : IInputHandler<GlobalKeybindActions>
{

    /// <summary>
    /// </summary>
    private const string VISUAL_OFFSET_NOTIFICATION_KEY = "gameplay-visual-offset";

    /// <summary>
    /// </summary>
    private const string LOCAL_MAP_OFFSET_NOTIFICATION_KEY = "gameplay-local-map-offset";

    private readonly Dictionary<GlobalKeybindActions, Bindable<bool>> _invertScrollingActions = [];

    private static readonly HashSet<GlobalKeybindActions> HoldRepeatActions = [];

    private static readonly HashSet<GlobalKeybindActions> HoldAndReleaseActionsSet =
    [
        GlobalKeybindActions.GameplayPause,
        GlobalKeybindActions.GameplayRetry
    ];

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
        List<GlobalInputScopeToken> scopes;
        lock (GlobalInputManager.ScopeTokensLock)
            scopes = new List<GlobalInputScopeToken>(GlobalInputManager.ScopeTokens);
        scopes.Reverse();

        var consumed = false;
        foreach (var scope in scopes)
        {
            switch (scope.Handle(action, isKeyPress, isRelease))
            {
                case GlobalInputHandleResult.Consumed:
                    Logger.Debug($"Action {action} is consumed by {scope.GetType().FullName}", LogType.Runtime);
                    consumed = true;
                    break;
                case GlobalInputHandleResult.Pass:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (consumed)
                break;
        }

        if (!consumed)
        {
            Logger.Debug($"Action {action} is not consumed by any scope", LogType.Runtime);
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

    public static void HandleOffsetAction(GlobalKeybindActions action)
    {
        var change = action.HasFlag(GlobalKeybindActions.Small) ? 1 : 5;
        if (action.HasFlag(GlobalKeybindActions.Reverse))
            change *= -1;
                
        if ((action.BaseWithLayer()) == GlobalKeybindActions.ResetOffset)
        {
            if (action.HasFlag(GlobalKeybindActions.Visual))
            {
                ConfigManager.VisualOffset.Value = 0;
                NotificationManager.ShowOrUpdate(VISUAL_OFFSET_NOTIFICATION_KEY, NotificationLevel.Success,
                    $"Visual offset has been reset to: {ConfigManager.VisualOffset.Value} ms", null, true);
            }
            else
            {
                MapManager.Selected.Value.LocalOffset = 0;
                NotificationManager.ShowOrUpdate(LOCAL_MAP_OFFSET_NOTIFICATION_KEY, NotificationLevel.Success,
                    $"Local map audio offset has been reset to: {MapManager.Selected.Value.LocalOffset} ms", null, true);

                ThreadScheduler.Run(() => MapDatabaseCache.UpdateMap(MapManager.Selected.Value));
            }
        }

        if ((action.BaseWithLayer()) == GlobalKeybindActions.IncreaseOffset)
        {
            if (action.HasFlag(GlobalKeybindActions.Visual))
            {
                ConfigManager.VisualOffset.Value += change;
                NotificationManager.ShowOrUpdate(VISUAL_OFFSET_NOTIFICATION_KEY, NotificationLevel.Success,
                    $"Visual offset has been changed to: {ConfigManager.VisualOffset.Value} ms", null, true);
            }
            else
            {
                MapManager.Selected.Value.LocalOffset += change;
                NotificationManager.ShowOrUpdate(LOCAL_MAP_OFFSET_NOTIFICATION_KEY, NotificationLevel.Success,
                    $"Local map audio offset is now: {MapManager.Selected.Value.LocalOffset} ms", null, true);

                ThreadScheduler.Run(() => MapDatabaseCache.UpdateMap(MapManager.Selected.Value));
            }
        }
    }

    /// <summary>
    ///     Handles scroll speed changes.
    /// </summary>
    internal static void ChangeScrollSpeed(GlobalKeybindActions action)
    {
        var speedIncrease = action.HasFlag(GlobalKeybindActions.Small) ? 1 : 10;
        if (action.HasFlag(GlobalKeybindActions.Reverse))
            speedIncrease *= -1;

        var scrollSpeed = ConfigManager.ScrollSpeeds[MapManager.Selected.Value.Mode];

        if (action.HasFlag(GlobalKeybindActions.Local))
        {
            // Handle local scroll speed changes with <shift> key held.
            var targetScrollSpeed = MapManager.CustomScrollSpeed ?? scrollSpeed.Value;
            targetScrollSpeed += speedIncrease;

            if (targetScrollSpeed == scrollSpeed.Value)
            {
                // Reset to global if the target speed is the same as global
                MapManager.CustomScrollSpeed = null;

                NotificationManager.ShowOrUpdate("gameplay-scroll-speed", NotificationLevel.Info,
                    $"Scroll speed (local) has been reset to global: {scrollSpeed.Value / 10f:0.0}",
                    null, true);
            }
            else
            {
                // Set custom local scroll speed
                MapManager.CustomScrollSpeed = targetScrollSpeed;

                NotificationManager.ShowOrUpdate("gameplay-scroll-speed", NotificationLevel.Info,
                    $"Scroll speed (local) has been changed to: {targetScrollSpeed / 10f:0.0}",
                    null, true);
            }
        }
        else
        {
            // Update the global scroll speed.
            // If there is a custom local scroll speed set, this would not have any
            // visual effect right away.
            scrollSpeed.Value += speedIncrease;

            NotificationManager.ShowOrUpdate("gameplay-scroll-speed", NotificationLevel.Info,
                $"Scroll speed (global) has been changed to: {scrollSpeed.Value / 10f:0.0}",
                null, true);
        }
    }
}
