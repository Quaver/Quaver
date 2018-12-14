/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Audio;
using Quaver.Shared.Screens.Menu;
using Wobble.Graphics;
using Wobble.Input;

namespace Quaver.Shared.Screens.Edit.Input
{
    public class EditorInputManager
    {
        /// <summary>
        ///     Reference to the gameplay screen.
        /// </summary>
        private EditorScreen Screen { get; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        internal EditorInputManager(EditorScreen screen) => Screen = screen;

        /// <summary>
        ///     Captures all input for the editor.
        /// </summary>
        /// <param name="dt"></param>
        internal void HandleInput(double dt)
        {
            HandlePauseAndResume();
            HandleSeeking();
            HandleMapSaving();
            HandleExit();
        }

        /// <summary>
        ///     Pauses and resumes the audio.
        /// </summary>
        private static void HandlePauseAndResume()
        {
            // Handle play
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Space))
                return;

            if (AudioEngine.Track.IsPlaying)
                AudioEngine.Track.Pause();
            else if (AudioEngine.Track.IsPaused)
                AudioEngine.Track.Play();
            else if (AudioEngine.Track.IsStopped)
            {
                AudioEngine.LoadCurrentTrack();
                AudioEngine.Track.Play();
            }
        }

        /// <summary>
        ///     Shortcut to save the map.
        /// </summary>
        private void HandleMapSaving()
        {
            if (!KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl))
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.S))
                Screen.SaveMap();
        }

        /// <summary>
        ///     Seeks through the audio based on the current snap.
        /// </summary>
        private void HandleSeeking()
        {
            var scrollDiff = MouseManager.CurrentState.ScrollWheelValue - MouseManager.PreviousState.ScrollWheelValue;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Left) || scrollDiff > 0)
                AudioEngine.SeekTrackToNearestSnap(Screen.Map, Direction.Backward, Screen.BeatSnap.Value);

            if (KeyboardManager.IsUniqueKeyPress(Keys.Right) || scrollDiff < 0)
                AudioEngine.SeekTrackToNearestSnap(Screen.Map, Direction.Forward, Screen.BeatSnap.Value);
        }

        /// <summary>
        ///     Gets out of the screen.
        /// </summary>
        private void HandleExit()
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                QuaverScreenManager.ChangeScreen(new MenuScreen());
        }
    }
}
