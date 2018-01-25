using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics.Text;
using Quaver.Logging;

namespace Quaver.Utility
{
    internal static class FpsCounter
    {
        /// <summary>
        ///     After this many frames, it will update the current FPS
        /// </summary>
        private const int FrameCount = 100;

        /// <summary>
        ///     Determines if fps graph should be displayed
        /// </summary>
        private static bool DisplayGraph = true;

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
        ///     The average fps for each interval
        /// </summary>
        private static double[] AverageFpsIntervals = new double[40];

        /// <summary>
        ///     Each bar sizes in the graph
        /// </summary>
        private static int[] CurrentBarSize = new int[40];

        /// <summary>
        ///     The current Max FPS
        /// </summary>
        private static double CurrentMaxFPS;

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

            // Calculate current fps
            FpsCurrent = 1000 / (FpsCount / FrameCount);
            if (!DisplayGraph) return;

            // Shift AverageFpsIntervals back by 1 and calculate max fps
            CurrentMaxFPS = 10;
            for (var i = 39; i > 0; i--)
            {
                AverageFpsIntervals[i] = AverageFpsIntervals[i - 1];
                if (AverageFpsIntervals[i] > CurrentMaxFPS) CurrentMaxFPS = AverageFpsIntervals[i];
            }

            // Calculate current position on list + max fps
            if (FpsCurrent > CurrentMaxFPS) CurrentMaxFPS = FpsCurrent;
                CurrentMaxFPS += 10;

            AverageFpsIntervals[0] = FpsCurrent;

            // Calculate Current bar sizes for graph
            for (var i = 0; i < 40; i++)
                CurrentBarSize[i] = (int)(40 * AverageFpsIntervals[i] / CurrentMaxFPS);

            // Reset both the FPS Count & Intrval
            FpsCount = 0;
            Interval = 0;
        }

        /// <summary>
        /// Draw the current FPS as a text sprite
        /// </summary>
        public static void Draw()
        {
            // Draw text
            GameBase.SpriteBatch.Draw(GameBase.UI.BlankBox, new Rectangle((int)GameBase.WindowRectangle.Width - 80, (int)GameBase.WindowRectangle.Height - 20, 75, 18), Color.Black);
            GameBase.SpriteBatch.DrawString(Fonts.Medium12, Math.Floor(FpsCurrent) + " FPS", new Vector2(GameBase.WindowRectangle.Width - 80, GameBase.WindowRectangle.Height - 20), Color.White);

            // Draw graph and color according to fps.
            if (!DisplayGraph) return;
            for (var i = 0; i < 40; i++)
            {
                if (AverageFpsIntervals[i] < 60)
                    GameBase.SpriteBatch.Draw(GameBase.UI.BlankBox, new Rectangle(i * 10, (int)GameBase.WindowRectangle.Height - CurrentBarSize[i], 8, CurrentBarSize[i]), Color.Red);
                else if (AverageFpsIntervals[i] < 144)
                    GameBase.SpriteBatch.Draw(GameBase.UI.BlankBox, new Rectangle(i * 10, (int)GameBase.WindowRectangle.Height - CurrentBarSize[i], 8, CurrentBarSize[i]), Color.DarkOrange);
                else if (AverageFpsIntervals[i] < 240)
                    GameBase.SpriteBatch.Draw(GameBase.UI.BlankBox, new Rectangle(i * 10, (int)GameBase.WindowRectangle.Height - CurrentBarSize[i], 8, CurrentBarSize[i]), Color.Gold);
                else if (AverageFpsIntervals[i] < 500)
                    GameBase.SpriteBatch.Draw(GameBase.UI.BlankBox, new Rectangle(i * 10, (int)GameBase.WindowRectangle.Height - CurrentBarSize[i], 8, CurrentBarSize[i]), Color.LightGreen * 0.5f);
                else if (AverageFpsIntervals[i] < 1000)
                    GameBase.SpriteBatch.Draw(GameBase.UI.BlankBox, new Rectangle(i * 10, (int)GameBase.WindowRectangle.Height - CurrentBarSize[i], 8, CurrentBarSize[i]), Color.DeepSkyBlue * 0.5f);
                else if (AverageFpsIntervals[i] < 1500)
                    GameBase.SpriteBatch.Draw(GameBase.UI.BlankBox, new Rectangle(i * 10, (int)GameBase.WindowRectangle.Height - CurrentBarSize[i], 8, CurrentBarSize[i]), Color.LightBlue * 0.5f);
                else
                    GameBase.SpriteBatch.Draw(GameBase.UI.BlankBox, new Rectangle(i * 10, (int)GameBase.WindowRectangle.Height - CurrentBarSize[i], 8, CurrentBarSize[i]), Color.Azure * 0.5f);
            }
        }
    }
}
