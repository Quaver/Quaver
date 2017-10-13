using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Quaver.Main;

namespace Quaver.Tests
{
    internal static class FPSCounter
    {
        private static double _fpsCurrent = 0;
        private static double _fpsCount = 0;
        private static int _interval = 0;
        private static SpriteFont _FpsFont = GameBase.Content.Load<SpriteFont>("testFont");

        /// <summary>
        /// Use this to calculate FPS on every frame.
        /// </summary>
        /// <param name="dt"></param>
        public static void Count(double dt)
        {
            _fpsCount += dt;
            _interval++;
            //After 20 frames, it will update the current FPS
            if (_interval >= 20)
            {
                _fpsCurrent = 1/(_fpsCount/20);
                _fpsCount = 0;
                _interval = 0;
            }
        }

        /// <summary>
        /// Get the current FPS
        /// </summary>
        /// <returns></returns>
        public static double Get()
        {
            return _fpsCurrent;
        }

        /// <summary>
        /// Draw the current FPS as a text sprite
        /// </summary>
        public static void Draw()
        {
            GameBase.SpriteBatch.DrawString(_FpsFont, Math.Floor(_fpsCurrent).ToString() + " FPS", new Vector2(0, 0), Color.LightGreen);
        }

    }
}
