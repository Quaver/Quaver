using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Menu
{
    public class MenuHeader : Sprite
    {
        /// <summary>
        /// </summary>
        public Sprite Icon { get; }

        /// <summary>
        /// </summary>
        public SpriteTextBitmap Title { get; }

        /// <summary>
        /// </summary>
        public SpriteTextBitmap Title2 { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap Subtitle { get; }

        /// <summary>
        /// </summary>
        private Sprite BackgroundLine { get; }

        /// <summary>
        /// </summary>
        private Sprite ForegroundLine { get; }

        /// <summary>
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="title"></param>
        /// <param name="title2"></param>
        /// <param name="subtitle"></param>
        public MenuHeader(Texture2D icon, string title, string title2, string subtitle, Color colorTheme)
        {
            Size = new ScalableVector2(WindowManager.Width, 44);
            Tint = ColorHelper.HexToColor("#181818");
            Alpha = 1f;

            Icon = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Image = icon,
                Size = new ScalableVector2(20, 20),
                X = 20
            };

            Title = new SpriteTextBitmap(FontsBitmap.GothamRegular, title.ToUpper())
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Icon.X + Icon.Width + 12,
                FontSize = 16
            };

            Title2 = new SpriteTextBitmap(FontsBitmap.GothamRegular, title2.ToUpper())
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Title.X + Title.Width + 10,
                Y = Title.Y,
                FontSize = Title.FontSize,
                Tint = colorTheme
            };

            Subtitle = new SpriteTextBitmap(FontsBitmap.GothamRegular, subtitle.ToUpper())
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                FontSize = 14,
                X = -25,
                Tint = ColorHelper.HexToColor("#eeeeee")
            };

            BackgroundLine = new Sprite
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(Width, 2),
                Tint = colorTheme
            };

            ForegroundLine = new Sprite
            {
                Parent = BackgroundLine,
                Size = new ScalableVector2(100, 2),
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            AnimateForegoundLine();
            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void AnimateForegoundLine()
        {
            if (ForegroundLine.Animations.Count != 0)
                return;

            if (ForegroundLine.X > WindowManager.Width / 2f)
                ForegroundLine.MoveToX(0, Easing.Linear, 15000);
            else
                ForegroundLine.MoveToX(WindowManager.Width - ForegroundLine.Width, Easing.Linear, 15000);
        }
    }
}