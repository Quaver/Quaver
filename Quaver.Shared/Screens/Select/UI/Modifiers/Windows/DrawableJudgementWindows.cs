using Microsoft.Xna.Framework;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Select.UI.Modifiers.Windows
{
    public class DrawableJudgementWindows : PoolableSprite<JudgementWindows>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override int HEIGHT { get; } = 50;

        /// <summary>
        /// </summary>
        private ImageButton Button { get; }

        /// <summary>
        /// </summary>
        public SpriteTextBitmap Name { get; }

        /// <summary>
        ///     Returns the background color of the table
        /// </summary>
        private Color BackgroundColor => Index % 2 == 0 ? ColorHelper.HexToColor("#363636") : Colors.DarkGray;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public DrawableJudgementWindows(PoolableScrollContainer<JudgementWindows> container, JudgementWindows item, int index) : base(container, item, index)
        {
            Size = new ScalableVector2(Container.Width, HEIGHT);
            Alpha = 0;

            Button = new ImageButton(UserInterface.BlankBox, (sender, args) => JudgementWindowsDatabaseCache.Selected.Value = item)
            {
                Parent = this,
                Size = Size,
                Tint = BackgroundColor
            };

            Name = new SpriteTextBitmap(FontsBitmap.GothamRegular, item.Name)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 18,
                FontSize = 18
            };
        }

        public override void Update(GameTime gameTime)
        {
            PerformHHoverAnimation(gameTime);
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(JudgementWindows item, int index)
        {
        }

        private void PerformHHoverAnimation(GameTime gameTime)
        {
            Color color;

            Button.Alpha = 1;

            if (JudgementWindowsDatabaseCache.Selected.Value == Item)
                color = ColorHelper.HexToColor("#636363");
            else if (Button.IsHovered)
                color = ColorHelper.HexToColor("#575757");
            else
            {
                color = Color.Transparent;
                Button.Alpha = 0;
            }


            Button.FadeToColor(color, gameTime.ElapsedGameTime.TotalMilliseconds, 30);
        }
    }
}