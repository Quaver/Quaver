using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Playercards;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Users
{
    public class LoggedInUserDropdown : ScrollContainer
    {
        /// <summary>
        /// </summary>
        public static ScalableVector2 ContainerSize { get; } = new ScalableVector2(480, 300);

        /// <summary>
        /// </summary>
        public ImageButton Button { get; }

        /// <summary>
        /// </summary>
        private UserPlayercardLoggedOut LoggedOutPlayercard { get; }

        /// <summary>
        /// </summary>
        private bool IsOpen { get; set; }

        /// <summary>
        /// </summary>
        private Sprite ActiveSprite => LoggedOutPlayercard;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public LoggedInUserDropdown() : base(new ScalableVector2(ContainerSize.X.Value, 0), ContainerSize)
        {
            Tint = Color.Black;
            Alpha = 0;

            Scrollbar.Visible = false;

            Button = new ImageButton(UserInterface.BlankBox)
            {
                Parent = this,
                Alpha = 0,
                UsePreviousSpriteBatchOptions = true,
            };

            LoggedOutPlayercard = new UserPlayercardLoggedOut
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Y = 14,
            };

            AddContainedDrawable(LoggedOutPlayercard);
            Open();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (!IsOpen && Animations.Count == 0)
                LoggedOutPlayercard.Y = -1000;
            else
                LoggedOutPlayercard.Y = 14;

            if (DialogManager.Dialogs.Count == 0)
            {
                if (LoggedOutPlayercard.Visible && LoggedOutPlayercard.LoginButton.IsHovered())
                    Button.Depth = 1;
                else
                    Button.Depth = 0;
            }
            else
            {
                Button.Depth = 0;
            }

            Button.Alignment = ActiveSprite.Alignment;
            Button.Size = new ScalableVector2(ActiveSprite.Width, Height);
            Button.Position = ActiveSprite.Position;
            base.Update(gameTime);
        }

        public void Open()
        {
            IsOpen = true;
            var height = ActiveSprite.Visible ? ActiveSprite.Height + 28 : 0;

            ClearAnimations();
            ChangeHeightTo((int) height, Easing.OutQuint, 450);
        }

        public void Close()
        {
            ClearAnimations();
            ChangeHeightTo(0, Easing.OutQuint, 550);
            IsOpen = false;
        }
    }
}