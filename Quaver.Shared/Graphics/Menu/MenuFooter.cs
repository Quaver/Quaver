using System.Collections.Generic;
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
    public class MenuFooter : Sprite
    {
        /// <summary>
        /// </summary>
        private Sprite BackgroundLine { get; }

        /// <summary>
        /// </summary>
        private Sprite ForegroundLine { get; }

        /// <summary>
        /// </summary>
        protected List<ButtonText> LeftAligned { get; }

        /// <summary>
        /// </summary>
        protected List<ButtonText> RightAligned { get; }

        /// <summary>
        /// </summary>
        public MenuFooter(List<ButtonText> leftAligned, List<ButtonText> rightAlighed, Color colorTheme)
        {
            LeftAligned = leftAligned;
            RightAligned = rightAlighed;
            Size = new ScalableVector2(WindowManager.Width, 44);
            Tint = ColorHelper.HexToColor("#181818");
            Alpha = 1;

            BackgroundLine = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(Width, 2),
                Tint = colorTheme
            };

            ForegroundLine = new Sprite
            {
                Parent = BackgroundLine,
                Size = new ScalableVector2(100, 2),
                Tint = Color.White,
            };

            ForegroundLine.X = Width - ForegroundLine.Width;

            AlignLeftItems(LeftAligned);
            AlignRightItems(RightAligned);
        }

        /// <inheritdoc />
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

        /// <summary>
        /// </summary>
        protected void AlignLeftItems(IReadOnlyList<ButtonText> buttons)
        {
            for (var i = 0; i < buttons.Count; i++)
            {
                var btn = buttons[i];

                btn.Parent = this;
                btn.Alignment = Alignment.MidLeft;
                btn.Y = 4;

                if (i == 0)
                    btn.X = 25;
                else
                    btn.X = buttons[i - 1].X + buttons[i - 1].Width + 40;
            }
        }

        /// <summary>
        /// </summary>
        protected void AlignRightItems(IReadOnlyList<ButtonText> buttons)
        {
            for (var i = 0; i < buttons.Count; i++)
            {
                var btn = buttons[i];

                btn.Parent = this;
                btn.Alignment = Alignment.MidRight;
                btn.Y = 4;

                if (i == 0)
                    btn.X = -25;
                else
                    btn.X = buttons[i - 1].X - buttons[i - 1].Width - 40;
            }
        }

    }
}