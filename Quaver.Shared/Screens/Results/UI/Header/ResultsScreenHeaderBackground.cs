using System;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Music.UI.Controller;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Results.UI.Header
{
    public class ResultsScreenHeaderBackground : MusicControllerBackground
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public ResultsScreenHeaderBackground(ScalableVector2 size) : base(size, false)
        {
            Background.Y = 100;
            Background.Alignment = Alignment.MidCenter;
            Darkness.Alpha = 0f;

            var game = GameBase.Game;
            var ratio = (float) game.Window.ClientBounds.Width / game.Window.ClientBounds.Height;

            // Stretch the background if using widescreen resolution. Default is 1920x1080 res
            if (ratio > 16 / 9f)
            {
                Background.Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
                Darkness.Size = Background.Size;
            }
        }
    }
}