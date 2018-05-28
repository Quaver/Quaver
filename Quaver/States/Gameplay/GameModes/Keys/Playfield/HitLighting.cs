using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics.Sprites;
using Quaver.Main;

namespace Quaver.States.Gameplay.GameModes.Keys.Playfield
{
    internal class HitLighting : AnimatableSprite
    {
        /// <summary>
        ///     If we're curerntly holding a long note.
        ///     It'll loop through the animation until we aren't anymore.
        /// </summary>
        internal bool IsHoldingLongNote { get; set; }

        internal HitLighting(Texture2D spritesheet, int rows, int columns) : base(spritesheet, rows, columns)
        {
            FinishedLooping += OnLoopCompletion;
        }

        internal HitLighting(List<Texture2D> frames) : base(frames)
        {
            FinishedLooping += OnLoopCompletion;
        }

        /// <summary>
        ///     When hitting an object, it'll loop through once.
        /// </summary>
        internal void PerformHitAnimation()
        {
            ChangeTo(0);
            Visible = true;
            StartLoop(LoopDirection.Forward, (int)(120 * GameBase.AudioEngine.PlaybackRate), 1);
        }

        /// <summary>
        ///     Stops holding (looping forever). Used when the user isn't holding the LN anymore.
        /// </summary>
        internal void StopHolding()
        {
            StopLoop();
            Visible = false;
            IsHoldingLongNote = false;
        }
        
        /// <summary>
        ///     When the animation loop is completed, we'll dictate 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoopCompletion(object sender, EventArgs e)
        {
            // If the object isn't a long note, then just make it invisible.
            if (!IsHoldingLongNote)
                Visible = false;
            // If it is however, then we'll want to hold it forever once it's done.
            else
            {
                StopLoop();  
                ChangeTo(0);
                StartLoop(LoopDirection.Forward, (int)(120 * GameBase.AudioEngine.PlaybackRate));
            }
        }
    }
}