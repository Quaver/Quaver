using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics.UI.Form;

namespace Quaver.Shared.Graphics.Form
{
    public class QuaverCheckbox : Checkbox
    {
        /// <summary>
        /// </summary>
        private static Texture2D ActiveTexture => UserInterface.On;

        /// <summary>
        /// </summary>
        private static Texture2D InactiveTexutre => UserInterface.Off;

        /// <summary>
        /// </summary>
        /// <param name="bindedValue"></param>
        public QuaverCheckbox(Bindable<bool> bindedValue) : base(bindedValue, new Vector2(78, 23), ActiveTexture,
            InactiveTexutre, false)
        {
        }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
            Alpha = MathHelper.Lerp(Alpha, IsHovered ? 0.75f : 1, (float) Math.Min(dt / 60, 1));

            base.Update(gameTime);
        }
    }
}