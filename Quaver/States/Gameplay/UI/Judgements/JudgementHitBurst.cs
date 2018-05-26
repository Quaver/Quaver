using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
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
        internal JudgementHitBurst(Texture2D spritesheet, int rows, int columns) : base(spritesheet, rows, columns)
        {
        }

        internal JudgementHitBurst(List<Texture2D> frames) : base(frames)
        {
            Visible = false;
            
            // Whenever the judgement is finished looping, then we'll make it invisible.
            FinishedLooping += (o, e) => Visible = false;
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
            
            if (Frames.Count > 0)
                StartLoop(LoopDirection.Forward, (int)(30 * GameBase.AudioEngine.PlaybackRate), 1);         
        }
    }
}