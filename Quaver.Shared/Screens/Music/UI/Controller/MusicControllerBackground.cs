using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Music.UI.Controller
{
    public class MusicControllerBackground : ScrollContainer
    {
        /// <summary>
        /// </summary>
        private Sprite Background { get; }

        /// <summary>
        /// </summary>
        private Sprite Darkness { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public MusicControllerBackground(ScalableVector2 size) : base(size, size)
        {
            Alpha = 0;
            InputEnabled = false;

            Background = new Sprite
            {
                Alignment = Alignment.MidCenter,
                Y = -200,
                Size = new ScalableVector2(1920, 1080),
                Image = UserInterface.MenuBackgroundClear
            };

            AddContainedDrawable(Background);

            Darkness = new Sprite
            {
                Parent = this,
                Size = Size,
                Tint = Color.Black,
                Alpha = 0.70f
            };
        }
    }
}