using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Logging;
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

            FpsCurrent = 1000 / (FpsCount / FrameCount);

            // Reset both the FPS Count & Intrval
            FpsCount = 0;
            Interval = 0;

            //If it ever stutters, it will log it. (If dt is higher than 1/144th of a second)
            if (dt > 6) LogManager.QuickLog("LAG/STUTTER. DT DELAY: "+Math.Round(dt,2)+"ms",Color.Red,5);
        }

        /// <summary>
        /// Draw the current FPS as a text sprite
        /// </summary>
        public static void Draw()
        {
            GameBase.SpriteBatch.DrawString(Font, Math.Floor(FpsCurrent) + " FPS", new Vector2(0, GameBase.Window.Height-20), Color.Black);
        }

    }
}
