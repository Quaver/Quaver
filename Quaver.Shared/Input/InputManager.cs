using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Wobble;
using Wobble.Input;

namespace Quaver.Shared.Input
{
    public class InputManager<TKeybindActions> where TKeybindActions : notnull
    {
        protected IInputConfig<TKeybindActions> InputConfig { get; }
        private IInputHandler<TKeybindActions> InputHandler { get; }

        private GenericKeyState previousKeyState;

        private const int HoldRepeatActionDelay = 250;
        private const int HoldRepeatActionInterval = 25;

        private readonly Dictionary<TKeybindActions, long> lastActionPress = [];

        private readonly Dictionary<TKeybindActions, long> lastActionTime = [];

        /// <summary>
        ///     When we regain focus, we won't take inputs until all keybinds are released
        ///     This is to prevent e.g. exiting from options menu triggering ExitEditor
        /// </summary>
        private bool WaitForKeybindClear { get; set; }

        public InputManager(IInputConfig<TKeybindActions> inputConfig,
            IInputHandler<TKeybindActions> inputHandler)
        {
            InputConfig = inputConfig;
            InputHandler = inputHandler;
            previousKeyState = new GenericKeyState(GenericKeyManager.GetPressedKeys());
        }

        public void HandleInput()
        {
            if (InputHandler.InFocus)
            {
                WaitForKeybindClear = true;
                return;
            }

            if (WaitForKeybindClear)
            {
                var keyState = new GenericKeyState(GenericKeyManager.GetPressedKeys());
                var keybinds = keyState.PressedKeybinds();

                WaitForKeybindClear = keybinds.Count != 0;
                return;
            }

            HandleKeyPresses();
            HandleKeyReleasesAfterHoldAction();

            HandleMouseInputs();
        }

        private void HandleKeyPresses()
        {
            var pressedKeys = GenericKeyManager.GetPressedKeys()
                .Where(k => !InputHandler.IsKeybindBlocked(k)).ToHashSet();

            var keyState = new GenericKeyState(pressedKeys);
            var uniqueKeyPresses = keyState.UniqueKeyPresses(previousKeyState);

            var allMatchedActions = new Dictionary<Keybind, HashSet<TKeybindActions>>();
            foreach (var pressedKeybind in keyState.PressedKeybinds())
            {
                HashSet<TKeybindActions>? actions;
                if (pressedKeybind.CanBeScrollingInverted)
                {
                    actions = [];
                    if (InputConfig.TryGetActionsFor(pressedKeybind, out var noninvertedActions))
                        actions.UnionWith(noninvertedActions!.Where(action =>
                            !(InputHandler.InvertedScrolling(action) ?? false)));
                    if (InputConfig.TryGetActionsFor(pressedKeybind.WithScrollingInverted(),
                            out var invertedActions))
                        actions.UnionWith(
                            invertedActions!.Where(action =>
                                InputHandler.InvertedScrolling(action) ?? false));
                }
                else if (!InputConfig.TryGetActionsFor(pressedKeybind, out actions)) continue;

                Debug.Assert(actions != null);

                allMatchedActions.Add(pressedKeybind, actions);

                foreach (var action in actions)
                {
                    if (uniqueKeyPresses.Contains(pressedKeybind))
                    {
                        HandleAction(action);
                        lastActionPress[action] = GameBase.Game.TimeRunning;
                    }
                    else if (CanRepeat(action))
                        HandleAction(action);
                    else if (CanHold(action) || pressedKeybind.Key.ScrollDirection != null)
                        HandleAction(action, false);
                }
            }

            InputHandler.HandleActionCombination(allMatchedActions, uniqueKeyPresses);
            InputHandler.HandleCustomActions(keyState, previousKeyState, uniqueKeyPresses);

            previousKeyState = keyState;
        }

        private void HandleKeyReleasesAfterHoldAction()
        {
            // If play testing, don't make any conflict with it.
            foreach (var action in InputHandler.HoldAndReleaseActions)
            {
                var binds = InputConfig.GetOrDefault(action);
                if (binds.IsNotBound() || binds.Any(x => InputHandler.IsKeybindBlocked(x.Key)))
                    continue;

                if (binds.IsUniqueRelease())
                    HandleAction(action, false, true);
            }
        }

        private long TimeSinceLastPress(TKeybindActions action) => GameBase.Game.TimeRunning -
            lastActionPress.GetValueOrDefault(action,
                GameBase.Game.TimeRunning);

        private long TimeSinceLastAction(TKeybindActions action) => GameBase.Game.TimeRunning -
            lastActionTime.GetValueOrDefault(action,
                GameBase.Game.TimeRunning);

        private bool CanRepeat(TKeybindActions action)
        {
            if (!InputHandler.IsHoldRepeat(action))
                return false;

            return TimeSinceLastAction(action) > HoldRepeatActionInterval &&
                   TimeSinceLastPress(action) > HoldRepeatActionDelay;
        }

        private bool CanHold(TKeybindActions action)
        {
            if (!InputHandler.IsHoldAndRelease(action))
                return false;

            return TimeSinceLastAction(action) > HoldRepeatActionInterval;
        }

        private void HandleAction(TKeybindActions action, bool isKeyPress = true,
            bool isRelease = false)
        {
            InputHandler.HandleAction(action, isKeyPress, isRelease);
            lastActionTime[action] = GameBase.Game.TimeRunning;
        }

        public void Destroy()
        {
        }

        private void HandleMouseInputs()
        {
        }
    }
}