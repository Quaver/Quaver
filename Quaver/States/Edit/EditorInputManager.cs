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
        ///     
        /// </summary>
        private void HandleMapSaving()
        {
            if (!GameBase.KeyboardState.IsKeyDown(Keys.LeftControl) && !GameBase.KeyboardState.IsKeyDown(Keys.RightControl))
                return;
                    
            if (InputHelper.IsUniqueKeyPress(Keys.S))
                Screen.SaveMap();;
        }

        /// <summary>
        ///     Seeks through the audio based on the current snap.
        /// </summary>
        private void HandleSeeking()
        {
            
        }
    }
}