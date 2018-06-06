using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics.Sprites;
using Quaver.Helpers;
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

        /// <summary>
        ///     If we're currently performing a one frame animation.
        /// </summary>
        private bool PerformingOneFrameAnimation { get; set; }

        /// <summary>
        ///     Dictates if we're currently decreasing the alpha in the one frame LN
        ///     hold animation.
        /// </summary>
        private bool DecreasingAlphaInAnimation { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        internal HitLighting() : base(GameBase.LoadedSkin.HitLighting)
        {
            FinishedLooping += OnLoopCompletion;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            // Performs a one animation frame if possible.
            if (PerformingOneFrameAnimation)
                PerformOneFrameAnimation(dt);
            
            base.Update(dt);
        }

        /// <summary>
        ///     When hitting an object, it'll loop through once.
        /// </summary>
        internal void PerformHitAnimation()
        {
            // First begin by replacing the frames
            ReplaceFrames(IsHoldingLongNote ? GameBase.LoadedSkin.HoldLighting : GameBase.LoadedSkin.HitLighting);
            
            // Go to the first frame and reset each of the properties 
            ChangeTo(0);
            Visible = true;
            Alpha = 1;
            
            // If we are performing a hone frame animation however, we don't want to handle it
            // through standard looping, but rather through our own rolled out animation.
            PerformingOneFrameAnimation = Frames.Count == 1;
            if (PerformingOneFrameAnimation)
                return;

            // Standard looping animations.
            if (!IsHoldingLongNote)
                StartLoop(LoopDirection.Forward, (int)(180 * GameBase.AudioEngine.PlaybackRate), 1);
            else
                StartLoop(LoopDirection.Forward, (int)(180 * GameBase.AudioEngine.PlaybackRate));
        }

        /// <summary>
        ///     Stops holding (looping forever). Used when the user isn't holding the LN anymore.
        /// </summary>
        internal void StopHolding()
        {
            StopLoop();
            Visible = false;
            IsHoldingLongNote = false;
            PerformingOneFrameAnimation = false;
        }
        
        /// <summary>
        ///     When the animation loop is completed, we'll dictate 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoopCompletion(object sender, EventArgs e)
        {
            // If the loop is done and this isn't a long note, then we'll want to make it invisible.
            if (IsHoldingLongNote) 
                return;
            
            Visible = false;
            PerformingOneFrameAnimation = false;
        }

        /// <summary>
        ///     Performs all one frame animations for both normal notes and LN holding.
        /// </summary>
        /// <param name="dt"></param>
        private void PerformOneFrameAnimation(double dt)
        {
            // Animation for normal HitObjects
            if (!IsHoldingLongNote)
            {
                Alpha -= AlphaChangePerFrame(dt);

                if (Alpha <= 0)
                    FinishedLooping?.Invoke(this, null);             
            }
            // Animation for LN HitObjects.
            // Pulsate the alpha of it.
            else
            {
                if (Alpha >= 1)
                    DecreasingAlphaInAnimation = true;

                if (DecreasingAlphaInAnimation)
                {
                    Alpha -= AlphaChangePerFrame(dt);

                    if (Alpha <= 0)
                        DecreasingAlphaInAnimation = false;
                }
                else
                {
                    Alpha += AlphaChangePerFrame(dt);
                }
            }
        }

        /// <summary>
        ///     The amount of alpha change per frame when doing one frame animations.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private static float AlphaChangePerFrame(double dt) => (float)(dt / (60 * GameBase.AudioEngine.PlaybackRate));
    }
}