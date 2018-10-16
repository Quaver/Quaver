using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Screens.Menu.UI.User
{
    public class BorderedTextButton : BitmapTextButton
    {
        /// <summary>
        ///     The original color of the button
        /// </summary>
        public Color OriginalColor { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <param name="clickAction"></param>
        public BorderedTextButton(string text, Color color, EventHandler clickAction = null)
            : base(UserInterface.BlankBox, BitmapFonts.Exo2Medium, text, 0.55f, 24, color, clickAction)
        {
            OriginalColor = color;
            Size = new ScalableVector2(175, 35);
            Tint = Color.Transparent;
            AddBorder(color, 2);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            Border.FadeToColor(IsHovered ? Color.White : OriginalColor, dt, 60);
            Text.FadeToColor(IsHovered ? Color.White : OriginalColor, dt, 60);

            base.Update(gameTime);
        }
    }
}