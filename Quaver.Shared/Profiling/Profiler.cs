using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using System;
using System.Collections.Generic;
using System.Text;
using Wobble;
using Wobble.Graphics.Sprites;
using Wobble.Graphics;

namespace Quaver.Shared.Profiling
{
    public class Profiler : Sprite
    {
        /// <summary>
        ///     The amount of time elapsed so we can begin counting each second.
        /// </summary>
        private TimeSpan ElapsedTime { get; set; } = TimeSpan.Zero;

        /// <summary>
        ///     The SpriteText that displays the FPS value.
        /// </summary>
        public SpriteText TextFps { get; }

        /// <summary>
        ///     The current frame rate.
        /// </summary>
        private int FrameRate { get; set; }

        /// <summary>
        ///     The amount of frames that we currently have.
        /// </summary>
        private int FrameCounter { get; set; }

        /// <summary>
        ///     The last FPS recorded, so we know if to update the counter.
        /// </summary>
        private int LastRecordedFps { get; set; }


        public Profiler(GlobalUserInterface ui)
        {
            Parent = ui;
            TextFps = new SpriteText(BitmapFonts.Exo2SemiBold, " ", 16)
            {
                Parent = this,
                Alignment = Alignment.MidCenter
            };

            ShowProfiler();
        }

        public void ShowProfiler()
        {

        }

        public void HideProfiler()
        {

        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            ElapsedTime += gameTime.ElapsedGameTime;

            if (ElapsedTime <= TimeSpan.FromSeconds(1))
            {
                base.Update(gameTime);
                return;
            }

            ElapsedTime -= TimeSpan.FromSeconds(1);
            FrameRate = FrameCounter;
            FrameCounter = 0;

            TextFps.Text = $"FPS: {FrameRate}";
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            FrameCounter++;
            base.Draw(gameTime);
        }
    }
}
