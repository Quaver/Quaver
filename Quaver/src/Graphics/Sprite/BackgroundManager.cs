using System;
using System.Collections.Generic;
using System.IO;
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

        public static bool TintReady { get; set; } = true;

        /// <summary>
        ///     Initializes the background.
        /// </summary>
        public static void Initialize()
        {
            Background = new Sprite()
            {
                Size = new UDim2(GameBase.WindowRectangle.Width, GameBase.WindowRectangle.Height),
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
            float tween = (float)Math.Min(dt / 400, 1);

            if (TintReady)
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
            //Checks if image is not null
            if (newBG == null) return;

            //Update Image
            GameBase.CurrentBackground = newBG;

            //Update Background Image Resolution
            var bgYRatio = ((float)newBG.Height / newBG.Width) / (GameBase.WindowRectangle.Height / GameBase.WindowRectangle.Width);
            if (bgYRatio > 1)
            {
                Background.SizeX = newBG.Width * (GameBase.WindowRectangle.Width / newBG.Width);
                Background.SizeY = newBG.Height * (GameBase.WindowRectangle.Width / newBG.Width);
            }
            else
            {
                Background.SizeX = newBG.Width * (GameBase.WindowRectangle.Height / newBG.Height);
                Background.SizeY = newBG.Height * (GameBase.WindowRectangle.Height / newBG.Height);
            }

            Background.Image = GameBase.CurrentBackground;
            TintReady = true;
        }

        /// <summary>
        ///     Make the Background Black and turn TintReady off
        /// </summary>
        public static void Blacken()
        {
            // Update TintReady
            TintReady = false;

            // Update Background Color
            Brightness = Configuration.BackgroundBrightness / 100f;
            TargetColor = Vector3.One * Brightness;
            CurrentColor = Vector3.Zero;
        }

        /// <summary>
        ///     Draws the background sprite.
        /// </summary>
        public static void Draw()
        {
            Background.Draw();
        }

        /// <summary>
        ///     Loads a beatmap's background
        /// </summary>
        public static void LoadBackground()
        {
            if (GameBase.CurrentBackground != null)
                GameBase.CurrentBackground.Dispose();

            var bgPath = Configuration.SongDirectory + "/" + GameBase.SelectedBeatmap.Directory + "/" + GameBase.SelectedBeatmap.BackgroundPath;

            if (!File.Exists(bgPath))
                return;

            GameBase.CurrentBackground = ImageLoader.Load(bgPath);
        }
    }
}
