using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Input;

namespace Quaver.Shared.Screens.Settings.Elements
{
    public class SettingsItem : Sprite
    {
        /// <summary>
        /// </summary>
        public SpriteText Name { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        public SettingsItem(SettingsDialog dialog, string name)
        {
            Size = new ScalableVector2(dialog.ContentContainer.Width - dialog.DividerLine.X - 10, 40);
            Tint = Color.Black;
            Alpha = 0.65f;

            Name = new SpriteText(BitmapFonts.Exo2Medium, name, 13)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 20
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            FadeToColor(GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                    ? Color.White : Color.Black, gameTime.ElapsedGameTime.TotalMilliseconds, 70);

            base.Update(gameTime);
        }
    }
}