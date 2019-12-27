using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Server.Client;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Playercards;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Users
{
    public class LoggedInUserDropdown : ScrollContainer
    {
        /// <summary>
        /// </summary>
        public static ScalableVector2 ContainerSize { get; } = new ScalableVector2(436, 300);

        /// <summary>
        /// </summary>
        public ImageButton Button { get; }

        /// <summary>
        /// </summary>
        private UserPlayercardLoggedOut LoggedOutPlayercard { get; }

        /// <summary>
        /// </summary>
        private UserPlayercard UserPlayercard { get; }

        /// <summary>
        /// </summary>
        private bool IsOpen { get; set; }

        /// <summary>
        /// </summary>
        private Drawable ActiveSprite => OnlineManager.Connected ? (Drawable) UserPlayercard : LoggedOutPlayercard;

        /// <summary>
        /// </summary>
        private Sprite ScreenDarkness { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public LoggedInUserDropdown() : base(new ScalableVector2(ContainerSize.X.Value, 0), ContainerSize)
        {
            Tint = Color.Black;
            Alpha = 0f;

            var game = GameBase.Game as QuaverGame;

            ScreenDarkness = new Sprite()
            {
                Parent = game?.CurrentScreen.View.Container,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height - 56 * 2),
                Y = 56,
                Tint = Color.Black,
                Alpha = 0
            };

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
                Alignment = Alignment.TopRight
            };

            UserPlayercard = new UserPlayercard(OnlineManager.Self)
            {
                Parent = this,
                Alignment = Alignment.TopRight
            };

            AddContainedDrawable(LoggedOutPlayercard);
            AddContainedDrawable(UserPlayercard);

            Open();

            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            const int inactivePos = -1000;
            const int activePos = 18;

            if (!IsOpen && Animations.Count == 0)
            {
                UserPlayercard.Y = inactivePos;
                LoggedOutPlayercard.Y = inactivePos;
            }
            else
            {
                LoggedOutPlayercard.Y = LoggedOutPlayercard == ActiveSprite ? activePos : inactivePos;
                UserPlayercard.Y = UserPlayercard == ActiveSprite ? activePos : inactivePos;
            }

            if (DialogManager.Dialogs.Count == 0)
            {
                if (LoggedOutPlayercard.Visible && LoggedOutPlayercard.LoginButton.IsHovered()
                    || UserPlayercard.Visible
                    && (UserPlayercard.ModeButton.IsHovered() || UserPlayercard.LogoutButton.IsHovered()
                        || UserPlayercard.ViewProfileButton.IsHovered()))
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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            OnlineManager.Status.ValueChanged -= OnConnectionStatusChanged;

            base.Destroy();
        }

        public void Open()
        {
            IsOpen = true;
            var height = ActiveSprite.Visible ? ActiveSprite.Height + 40 : 0;

            ClearAnimations();
            ChangeHeightTo((int) height, Easing.OutQuint, 450);

            ScreenDarkness.ClearAnimations();
            ScreenDarkness.FadeTo(0.75f, Easing.Linear, 200);
        }

        public void Close()
        {
            ClearAnimations();
            ChangeHeightTo(0, Easing.OutQuint, 550);
            IsOpen = false;

            ScreenDarkness.ClearAnimations();
            ScreenDarkness.FadeTo(0f, Easing.Linear, 200);
        }

        private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
        {
            if (IsOpen)
                Open();
        }
    }
}