using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using Quaver.Graphics;

using Quaver.Utility;

namespace Quaver.Graphics.Sprite
{
    class BackgroundManager
    {
        /// <summary>
        ///     The Background Sprite.
        /// </summary>
        public static Sprite Background;

        /// <summary>
        ///     The tint of the background
        /// </summary>
        private static Color _tint = new Color(255, 255, 255);

        /// <summary>
        ///     The previous tint of the background. Used to save processing space.
        /// </summary>
        private static Color _prevTint = _tint;

        /// <summary>
        ///     Target Color of the background represented by percentage.
        /// </summary>
        public static Vector3 TargetColor { get; set; } = Vector3.One;

        /// <summary>
        ///     Current Color of the background represented by percentage.
        /// </summary>
        public static Vector3 CurrentColor { get; set; } = Vector3.One;

        /// <summary>
        ///     The dimness of the background.
        /// </summary>
        public static float Brightness { get; set; } = 1;

        /// <summary>
        ///     Initializes the background.
        /// </summary>
        public static void Initialize()
        {
            Background = new Sprite()
            {
                SizeX = GameBase.Window.Width,
                SizeY = GameBase.Window.Height,
                Alignment = Alignment.MidCenter,
                Image = GameBase.UI.DiffSelectMask,
                Tint = Color.Gray
            };
        }

        /// <summary>
        ///     Unloads the background sprite.
        /// </summary>
        public static void UnloadContent()
        {
            Background.Destroy();
        }

        /// <summary>
        ///     Updates the background.
        /// </summary>
        /// <param name="dt"></param>
        public static void Update(double dt)
        {
            //Tween Color
            float tween = (float)Math.Min(dt / 300, 1);
            CurrentColor = Vector3.Lerp(CurrentColor, TargetColor, tween);

            //Update Color
            _tint.R = (byte)(CurrentColor.X*255);
            _tint.G = (byte)(CurrentColor.Y*255);
            _tint.B = (byte)(CurrentColor.Z*255);

            //Update Background Tint
            if (_tint != _prevTint) Background.Tint = _tint;
            Background.Update(dt);
            _prevTint = _tint;
        }

        /// <summary>
        ///     Changes the background image. If no image is added to the parameter, it'll update the brightness to match the config.
        /// </summary>
        /// <param name="newBG"></param>
        public static void Change(Texture2D newBG = null)
        {
            //Update Background Color
            Brightness = Configuration.BackgroundBrightness / 100f;
            TargetColor = Vector3.One * Brightness;
            CurrentColor = Vector3.Zero;

            //Checks if image is not null
            if (newBG == null) return;

            //Update Image
            Background.Image = newBG;

            //Update Background Image Resolution
            var bgYRatio = ((float)newBG.Height / newBG.Width) / ((float)GameBase.Window.Height / GameBase.Window.Width);
            if (bgYRatio > 1)
            {
                Background.Size = new Vector2(newBG.Width, newBG.Height) * ((float)GameBase.Window.Width / newBG.Width);
            }
            else
            {
                Background.Size = new Vector2(newBG.Width, newBG.Height) * ((float)GameBase.Window.Height / newBG.Height);
            }
        }

        /// <summary>
        ///     Draws the background sprite.
        /// </summary>
        public static void Draw()
        {
            Background.Draw();
        }
    }
}
