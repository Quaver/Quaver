using Microsoft.Xna.Framework.Input;
using Quaver.Audio;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.States.Edit
{
    internal class EditorInputManager
    {
        private EditorScreen Screen { get; }
        
        internal EditorInputManager(EditorScreen screen) => Screen = screen;
        
        /// <summary>
        ///     Captures all input for the editor.
        /// </summary>
        /// <param name="dt"></param>
        internal void HandleInput(double dt)
        {
            PauseAndResume();
            SaveMap();
        }

        /// <summary>
        ///     Pauses and resumes the audio.
        /// </summary>
        private static void PauseAndResume()
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
        private void SaveMap()
        {
            if (!GameBase.KeyboardState.IsKeyDown(Keys.LeftControl) && !GameBase.KeyboardState.IsKeyDown(Keys.RightControl))
                return;
                    
            if (InputHelper.IsUniqueKeyPress(Keys.S))
                Screen.SaveMap();;
        }
    }
}