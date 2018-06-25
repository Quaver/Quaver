using Microsoft.Xna.Framework.Input;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.States.Edit
{
    internal class EditorInputManager
    {
        /// <summary>
        ///     Captures all input for the editor.
        /// </summary>
        /// <param name="dt"></param>
        internal void HandleInput(double dt)
        {
            PauseAndResume();
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
            else
                GameBase.AudioEngine.Play();
        }
    }
}