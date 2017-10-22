using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Main;

namespace Quaver.Utility
{
    internal static class FpsCounter
    {
        /// <summary>
        ///     The current FPS
        /// </summary>
        private static double FpsCurrent { get; set; }

        /// <summary>
        ///     The FPS Count
        /// </summary>
        private static double FpsCount { get; set; }

        /// <summary>
        ///     The current interval
        /// </summary>
        private static int Interval { get; set; }

        /// <summary>
        ///     The SpriteFont for the FPS Counter.
        /// </summary>
        private static SpriteFont Font { get; } = GameBase.Content.Load<SpriteFont>("testFont");

        /// <summary>
        /// After this many frames, it will update the current FPS
        /// </summary>
        private const int FrameCount = 100;

        /// <summary>
        /// Use this to calculate FPS on every frame.
        /// </summary>
        /// <param name="dt"></param>
        public static void Count(double dt)
        {
            FpsCount += dt;
            Interval++;

            // Only after the total FrameCount, it will update the current FPS
            if (Interval < FrameCount)
                return;

            FpsCurrent = 1 / (FpsCount / FrameCount);

            // Reset both the FPS Count & Intrval
            FpsCount = 0;
            Interval = 0;
        }

        /// <summary>
        /// Get the current FPS
        /// </summary>
        /// <returns></returns>
        public static double Get()
        {
            return FpsCurrent;
        }

        /// <summary>
        /// Draw the current FPS as a text sprite
        /// </summary>
        public static void Draw()
        {
            GameBase.SpriteBatch.DrawString(Font, Math.Floor(FpsCurrent) + " FPS", new Vector2(0, GameBase.WindowSize.Y-20), Color.LightGreen);
        }

    }
}
