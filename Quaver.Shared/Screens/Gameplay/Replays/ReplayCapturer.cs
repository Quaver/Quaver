/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Replays;
using Quaver.Shared.Config;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Gameplay.Rulesets.Input;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;

namespace Quaver.Shared.Screens.Gameplay.Replays
{
    public class ReplayCapturer
    {
        /// <summary>
        ///     Reference to the gameplay screen itself.
        /// </summary>
        public GameplayScreen Screen { get; }

        /// <summary>
        ///     The replay to be captured.
        /// </summary>
        public Replay Replay { get; }

        /// <summary>
        ///     The amount of time that has elapsed since the last frame capture.
        /// </summary>
        private double TimeSinceLastCapture { get; set; }

        /// <summary>
        ///     The last recorded key press state.
        /// </summary>
        private ReplayKeyPressState LastKeyPressState { get; set; }

        /// <summary>
        ///     If the replay should be captured.
        /// </summary>
        public bool ShouldCapture { get; set; } = true;

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        public ReplayCapturer(GameplayScreen screen)
        {
            Screen = screen;

            var name = Screen.InReplayMode && Screen.LoadedReplay != null ? Screen.LoadedReplay.PlayerName : ConfigManager.Username.Value;
            var mods = Screen.InReplayMode && Screen.LoadedReplay != null ? Screen.LoadedReplay.Mods : ModManager.Mods;

            Replay = new Replay(Screen.Map.Mode, name, mods, Screen.MapHash)
            {
                TimePlayed = Screen.TimePlayed
            };

            // Add sample first frame.
            Replay.AddFrame(-10000, 0);
        }

        ///  <summary>
        ///
        ///      Important frames are also taken into account here.
        ///          - KeyPressState Changes.
        ///          - Combo is different than the previous frame.
        ///          - Frames at the capture interval.
        ///  </summary>
        /// <param name="gameTime"></param>
        public void Capture(GameTime gameTime)
        {
            if (Screen.IsPaused || !ShouldCapture)
                return;

            TimeSinceLastCapture += gameTime.ElapsedGameTime.TotalMilliseconds;

            var currentPressState = GetKeyPressState();

            // If the key press states don't match, add a frame.
            if (LastKeyPressState != currentPressState)
                AddFrame(currentPressState);

            if (Screen.LastRecordedCombo != Screen.Ruleset.ScoreProcessor.Combo)
                AddFrame(currentPressState);

            // Add frame for 60 fps.
            if (TimeSinceLastCapture >= 1000 / 60f)
            {
                AddFrame(currentPressState);
                TimeSinceLastCapture = 0;
            }

            LastKeyPressState = GetKeyPressState();
        }

        /// <summary>
        ///     Adds a replay frame with the correct key press state.
        /// </summary>
        private void AddFrame(ReplayKeyPressState state)
        {
            var manager = Screen.Ruleset.HitObjectManager as HitObjectManagerKeys;
            Replay.AddFrame((int)manager.CurrentAudioPosition, state);
        }

        /// <summary>
        ///     Gets the current key press state from the binding store.
        /// </summary>
        /// <returns></returns>
        private ReplayKeyPressState GetKeyPressState()
        {
            var inputManager = (KeysInputManager)Screen.Ruleset.InputManager;
            return BindingStoreToKeyPressState(inputManager.BindingStore);
        }

        /// <summary>
        ///     Converts an input manager's binding store to a key press state.
        /// </summary>
        /// <param name="bindingStore"></param>
        /// <returns></returns>
        private static ReplayKeyPressState BindingStoreToKeyPressState(IReadOnlyList<InputBindingKeys> bindingStore)
        {
            ReplayKeyPressState state = 0;

            for (var i = 0; i < bindingStore.Count; i++)
            {
                if (bindingStore[i].Pressed)
                    state |= Replay.KeyLaneToPressState(i + 1);
            }

            return state;
        }
    }
}
