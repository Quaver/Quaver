using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Quaver.Audio;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.States.Edit
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
            {
                // Get the current timing point
                var point = Screen.Map.GetTimingPointAt(GameBase.AudioEngine.Time);
                
                var msPerBeat = 60000 / point.Bpm / 4;
                var amountOfBeats = (int)(Screen.Map.GetTimingPointLength(point) / msPerBeat);

                var beats = new List<double>();
                for (var i = 0; i < amountOfBeats; i++)
                    beats.Add(point.StartTime + i * msPerBeat);

                try
                {
                    var index = beats.FindLastIndex(x => x < (int) GameBase.AudioEngine.Time);
                    GameBase.AudioEngine.Seek(beats[index]);
                }
                catch (Exception e)
                {
                    GameBase.AudioEngine.Seek(GameBase.AudioEngine.Time - msPerBeat);
                }
            }
            
            if (InputHelper.IsUniqueKeyPress(Keys.Right) || scrollDiff < 0)
            {
                // Get the current timing point
                var point = Screen.Map.GetTimingPointAt(GameBase.AudioEngine.Time);

                var msPerBeat = 60000 / point.Bpm / 4;
                
                GameBase.AudioEngine.Seek(GameBase.AudioEngine.Time + msPerBeat);
            }
        }

        private void Seek()
        {
            
        }
    }
}