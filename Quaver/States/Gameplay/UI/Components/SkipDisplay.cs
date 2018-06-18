using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics;
using Quaver.Graphics.Sprites;
using Quaver.Main;

namespace Quaver.States.Gameplay.UI.Components
{
    internal class SkipDisplay : AnimatableSprite
    {
        /// <summary>
        ///     Reference to the gameplay screen.
        /// </summary>
        private GameplayScreen Screen { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="frames"></param>
        internal SkipDisplay(GameplayScreen screen, List<Texture2D> frames) : base(frames)
        {
            Screen = screen;
            Size = new UDim2D(230, 56);
            PosY = 30;
            Alignment = Alignment.TopCenter;            
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            if (Screen.OnBreak)
            {
                FadeIn(dt, 360 / GameBase.AudioEngine.PlaybackRate);
                StartLoop(LoopDirection.Forward, (int)(30 * GameBase.AudioEngine.PlaybackRate));
            }
            else
            {
                FadeOut(dt, 120 / GameBase.AudioEngine.PlaybackRate);
                StopLoop();
            }

            base.Update(dt);
        }
    }
}