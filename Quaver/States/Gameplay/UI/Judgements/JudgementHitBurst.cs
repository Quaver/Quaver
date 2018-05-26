using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.States.Gameplay.UI.Judgements
{
    /// <inheritdoc />
    /// <summary>
    ///     Animatable sprite used when a user hits an object.
    ///     It is capable of switching  judgements and performing the loop animation once.
    /// </summary>
    internal class JudgementHitBurst : AnimatableSprite
    {
        /// <summary>
        ///     If we are currently animating the hit burst with only one frame.
        /// </summary>
        internal bool IsAnimatingWithOneFrame { get; private set; }

        /// <summary>
        ///     The original size of the hit burst.
        /// </summary>
        private Vector2 OriginalSize { get; }

        /// <summary>
        ///     The original Y position of the hit burst.
        /// </summary>
        private float OriginalPosY { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="frames"></param>
        /// <param name="size"></param>
        /// <param name="posY"></param>
        internal JudgementHitBurst(List<Texture2D> frames, Vector2 size, float posY) : base(frames)
        {
            OriginalPosY = posY;
            OriginalSize = size;
            Size = new UDim2D(OriginalSize.X, OriginalSize.Y);
            PosY = OriginalPosY;
            Visible = false;
            
            // Whenever the judgement is finished looping, then we'll make it invisible.
            FinishedLooping += (o, e) => Visible = false;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            PerformOneFrameAnimation(dt);
            
            base.Update(dt);
        }

        /// <summary>
        ///     Replaces the animation frames with ones pertaining to the given judgement.
        /// </summary>
        /// <param name="j"></param>
        internal void ChangeJudgementFrames(Judgement j)
        {
            if (IsLooping)
                StopLoop();
            
            switch (j)
            {
                case Judgement.Marv:
                    ReplaceFrames(GameBase.LoadedSkin.JudgeMarv);
                    break;
                case Judgement.Perf:
                    ReplaceFrames(GameBase.LoadedSkin.JudgePerf);
                    break;
                case Judgement.Great:
                    ReplaceFrames(GameBase.LoadedSkin.JudgeGreat);
                    break;
                case Judgement.Good:
                    ReplaceFrames(GameBase.LoadedSkin.JudgeGood);
                    break;
                case Judgement.Okay:
                    ReplaceFrames(GameBase.LoadedSkin.JudgeOkay);
                    break;
                case Judgement.Miss:
                    ReplaceFrames(GameBase.LoadedSkin.JudgeMiss);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(j), j, null);
            }
        }

        /// <summary>
        ///     Switches to the correct judgement and performs a loop for it.
        /// </summary>
        /// <param name="j"></param>
        internal void PerformJudgementAnimation(Judgement j)
        {
            ChangeJudgementFrames(j);
            Visible = true;

            if (Frames.Count != 1)
                StartLoop(LoopDirection.Forward, (int) (30 * GameBase.AudioEngine.PlaybackRate), 1);
            else
            {;
                // Set the position to slightly above, so we can tween it back down in the animation.
                PosY = OriginalPosY - 5;
                Alpha = 1;
                IsAnimatingWithOneFrame = true;   
            }
        }

        /// <summary>
        ///     If there is only 1 frame in the list, then we'll roll out our own animations here.
        /// </summary>
        /// <param name="dt"></param>
        private void PerformOneFrameAnimation(double dt)
        {
            if (!IsAnimatingWithOneFrame)
                return;
            
            Console.WriteLine("hi");

            // Tween the position if need be
            if (Math.Abs(PosY - OriginalPosY) > 0.01)
                PosY = GraphicsHelper.Tween(OriginalPosY, PosY, Math.Min(dt / (30 / GameBase.AudioEngine.PlaybackRate), 1));
            // If we've already tweened it, then we can begin to fade it out.
            else
            {
                Alpha = GraphicsHelper.Tween(0, Alpha, Math.Min(dt / (240 / GameBase.AudioEngine.PlaybackRate), 1));

                if (Alpha <= 0)
                    IsAnimatingWithOneFrame = false;
            }
        }
    }
}