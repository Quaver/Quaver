using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Screens.Edit.Dialogs;
using Quaver.Shared.Screens.Edit.Plugins;
using Quaver.Shared.Screens.Edit.UI;
using Quaver.Shared.Screens.Edit.UI.Playfield.Waveform;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Edit.Input
{
    public class EditorInputManager
    {
        public EditorInputConfig InputConfig { get; }
        public EditScreen Screen { get; }
        private EditScreenView View { get; }

        private Dictionary<Keybind, HashSet<KeybindActions>> KeybindDictionary;
        private GenericKeyState PreviousKeyState;

        private const int HoldRepeatActionDelay = 250;
        private const int HoldRepeatActionInterval = 25;
        private Dictionary<KeybindActions, long> LastActionPress = new Dictionary<KeybindActions, long>();
        private Dictionary<KeybindActions, long> LastActionTime = new Dictionary<KeybindActions, long>();

        private static HashSet<KeybindActions> HoldRepeatActions = new HashSet<KeybindActions>()
        {
        };

        private static HashSet<KeybindActions> HoldAndReleaseActions = new HashSet<KeybindActions>()
        {
        };

        private static HashSet<KeybindActions> EnabledActionsDuringGameplayPreview = new HashSet<KeybindActions>()
        {
        };

        public EditorInputManager(EditScreen screen)
        {
            InputConfig = EditorInputConfig.LoadFromConfig();
            KeybindDictionary = InputConfig.ReverseDictionary();
            PreviousKeyState = new GenericKeyState(GenericKeyManager.GetPressedKeys());
            Screen = screen;
        }

        public void HandleInput()
        {
            if (DialogManager.Dialogs.Count != 0) return;
            var view = (EditScreenView)Screen.View;
            if (view.IsImGuiHovered) return;

            HandleKeypresses();
            HandleKeyReleasesAfterHoldAction();
            HandlePluginKeypresses();

            HandleMouseInputs();
        }

        private void HandleKeypresses()
        {
            var keyState = new GenericKeyState(GenericKeyManager.GetPressedKeys());
            var uniqueKeypresses = keyState.UniqueKeypresses(PreviousKeyState);
            foreach (var pressedKeybind in keyState.PressedKeybinds())
            {
                HashSet<KeybindActions> actions;

                if (!KeybindDictionary.TryGetValue(pressedKeybind, out actions)) continue;

                foreach (var action in actions)
                {
                    if (uniqueKeypresses.Contains(pressedKeybind))
                    {
                        HandleAction(action);
                        LastActionPress[action] = GameBase.Game.TimeRunning;
                    }
                    else if (CanRepeat(action))
                        HandleAction(action);
                    else if (CanHold(action))
                        HandleAction(action, false);
                }
            }

            PreviousKeyState = keyState;
        }

        private void HandleKeyReleasesAfterHoldAction()
        {
            foreach (var action in HoldAndReleaseActions)
            {
                var binds = InputConfig.GetOrDefault(action);
                if (binds.IsNotBound()) continue;

                if (binds.IsUniqueRelease())
                    HandleAction(action, false, true);
            }
        }

        private long TimeSinceLastPress(KeybindActions action) => GameBase.Game.TimeRunning - LastActionPress.GetValueOrDefault(action, GameBase.Game.TimeRunning);
        private long TimeSinceLastAction(KeybindActions action) => GameBase.Game.TimeRunning - LastActionTime.GetValueOrDefault(action, GameBase.Game.TimeRunning);

        private bool CanRepeat(KeybindActions action)
        {
            if (!HoldRepeatActions.Contains(action))
                return false;

            return TimeSinceLastAction(action) > HoldRepeatActionInterval && TimeSinceLastPress(action) > HoldRepeatActionDelay;
        }

        private bool CanHold(KeybindActions action)
        {
            if (!HoldAndReleaseActions.Contains(action))
                return false;

            return TimeSinceLastAction(action) > HoldRepeatActionInterval;
        }

        private void HandleAction(KeybindActions action, bool isKeypress = true, bool isRelease = false)
        {
            switch (action)
            {
                // Action cases

                default:
                    return;
            }

            LastActionTime[action] = GameBase.Game.TimeRunning;
        }

        private void HandleMouseInputs()
        {
        }

        private void HandlePluginKeypresses()
        {
            foreach (var (pluginName, keybinds) in InputConfig.PluginKeybinds)
            {
                if (keybinds.IsUniquePress())
                {
                    // Toggle plugin
                }
            }
        }
    }
}