using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.States.Edit.Input
{
    internal class EditorInputManager
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
        }

        /// <summary>
        ///     Pauses and resumes the audio.
        /// </summary>
        private static void HandlePauseAndResume()
        {
            // Handle play
            if (!InputHelper.IsUniqueKeyPress(Keys.Space)) 
                return;
            
            if (GameBase.AudioEngine.IsPlaying)
                GameBase.AudioEngine.Pause();
            else if (GameBase.AudioEngine.IsPaused)
                GameBase.AudioEngine.Play();
            else if (GameBase.AudioEngine.IsStopped)
            {
                GameBase.AudioEngine.ReloadStream();
                GameBase.AudioEngine.Play();
            }
        }

        /// <summary>
        ///     Shortcut to save the map.
        /// </summary>
        private void HandleMapSaving()
        {
            if (!GameBase.KeyboardState.IsKeyDown(Keys.LeftControl) && !GameBase.KeyboardState.IsKeyDown(Keys.RightControl))
                return;
                    
            if (InputHelper.IsUniqueKeyPress(Keys.S))
                Screen.SaveMap();
        }

        /// <summary>
        ///     Seeks through the audio based on the current snap.
        /// </summary>
        private void HandleSeeking()
        {
            var scrollDiff = GameBase.MouseState.ScrollWheelValue - GameBase.PreviousMouseState.ScrollWheelValue;
            
            if (InputHelper.IsUniqueKeyPress(Keys.Left) || scrollDiff > 0)
                GameBase.AudioEngine.SeekToBeat(Screen.Map, SeekDirection.Backward, 4);
            
            if (InputHelper.IsUniqueKeyPress(Keys.Right) || scrollDiff < 0)
                GameBase.AudioEngine.SeekToBeat(Screen.Map, SeekDirection.Forward, 4);
        }
    }
}