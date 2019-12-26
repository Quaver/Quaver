using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Users
{
    public class LoggedInUserDropdown : ScrollContainer
    {
        /// <summary>
        /// </summary>
        public static ScalableVector2 ContainerSize { get; } = new ScalableVector2(450, 300);

        /// <summary>
        /// </summary>
        public ImageButton Button { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public LoggedInUserDropdown() : base(new ScalableVector2(ContainerSize.X.Value, 0), ContainerSize)
        {
            Tint = Color.Black;
            Alpha = 0.85f;

            Open();
            Scrollbar.Visible = false;

            Button = new ImageButton(UserInterface.BlankBox)
            {
                Parent = this,
                Alpha = 0,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Button.Size = Size;
            base.Update(gameTime);
        }

        public void Open()
        {
            ClearAnimations();
            ChangeHeightTo((int) ContainerSize.Y.Value, Easing.OutQuint, 450);
        }

        public void Close()
        {
            ClearAnimations();
            ChangeHeightTo(0, Easing.OutQuint, 450);
        }
    }
}