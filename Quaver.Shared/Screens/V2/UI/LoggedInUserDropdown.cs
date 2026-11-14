using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Server.Client;
using Quaver.Server.Client.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Menu.Border.Components.Users;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     Login dropdown used by replacement-screen navigation while disconnected.
    /// </summary>
    internal sealed class LoggedInUserDropdown : LoggedInUserDropdownBase
    {
        public static ScalableVector2 ContainerSize
        {
            get
            {
                var config = SkinManager.SkinV2?.Config.Shared.Navigation.AccountDropdown ?? new SkinV2AccountDropdownConfig();
                return new ScalableVector2(config.Width, config.OfflineHeight);
            }
        }

        private SkinStoreV2Lease Skin { get; }

        private SkinV2AccountDropdownConfig Config { get; }

        public override ImageButton Button { get; }

        private Sprite ScreenDarkness { get; }

        private Container LoginPanel { get; }

        private FlexContainer LoginLayout { get; }

        private FlexContainer LoginInfoLayout { get; }

        private FlexContainer LoginActionLayout { get; }

        private FlexItemOptions LoginButtonOptions { get; }

        private FlexItemOptions LoginWheelOptions { get; }

        private SpriteTextPlus LoginStatus { get; }

        private RoundedButton LoginButton { get; }

        private LoadingWheel LoginWheel { get; }

        private bool IsOpen { get; set; }

        private bool StateDirty { get; set; } = true;

        private float CloseAnimationRemaining { get; set; }

        private Action Closed { get; }

        public LoggedInUserDropdown(Container overlayHost = null, Action closed = null) : base(new ScalableVector2(ContainerSize.X.Value, 0), ContainerSize)
        {
            Closed = closed;
            Skin = SkinManager.AcquireV2();
            Config = Skin.Config.Shared.Navigation.AccountDropdown;
            Tint = Color.Black;
            Alpha = 0;
            Scrollbar.Visible = false;

            var game = GameBase.Game as QuaverGame;
            ScreenDarkness = new Sprite
            {
                Parent = overlayHost ?? game?.CurrentScreen?.View.Container,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height),
                Tint = Color.Black,
                Alpha = 0
            };

            Button = new ImageButton(UserInterface.BlankBox)
            {
                Parent = this,
                Alpha = 0,
                IsInteractionEnabled = false,
                UsePreviousSpriteBatchOptions = true
            };

            LoginPanel = CreateLoginPanel();
            LoginLayout = CreateFlex(LoginPanel, new ScalableVector2(Config.Width, Config.OfflineHeight), FlexDirection.Row, FlexJustifyContent.FlexStart, FlexAlignItems.Center);
            CreateSpacer(LoginLayout, Config.ContentPadding, Config.OfflineHeight);
            CreateAvatar(LoginLayout, Config.OfflineAvatarSize, UserInterface.OfflineAvatar);
            CreateSpacer(LoginLayout, Config.OfflineAvatarSpacing, Config.OfflineHeight);

            LoginInfoLayout = CreateFlex(LoginLayout, new ScalableVector2(100, Config.OfflineInfoHeight), FlexDirection.Column, FlexJustifyContent.Center, FlexAlignItems.FlexStart, Config.InfoGap);
            LoginLayout.SetItemOptions(LoginInfoLayout, new FlexItemOptions { Basis = 100, Grow = 1, Shrink = 1 });
            _ = new SpriteTextPlus(FontManager.GetWobbleFont(Config.PrimaryFont), "Login", Config.OfflineTitleFontSize)
            {
                Parent = LoginInfoLayout,
                Tint = SkinV2Color.Parse(Config.TextColor),
                UsePreviousSpriteBatchOptions = true
            };
            LoginStatus = new SpriteTextPlus(FontManager.GetWobbleFont(Config.SecondaryFont), string.Empty, Config.OfflineStatusFontSize)
            {
                Parent = LoginInfoLayout,
                Tint = SkinV2Color.Parse(Config.TextColor),
                UsePreviousSpriteBatchOptions = true
            };

            CreateSpacer(LoginLayout, Config.ContentPadding, Config.OfflineHeight);
            LoginActionLayout = CreateFlex(LoginLayout, new ScalableVector2(Config.LoginButtonSize, Config.LoginButtonSize), FlexDirection.Row, FlexJustifyContent.Center, FlexAlignItems.Center);
            LoginLayout.SetItemOptions(LoginActionLayout, FixedBasis(Config.LoginButtonSize));
            LoginButton = new RoundedButton((sender, args) => Login())
            {
                Parent = LoginActionLayout,
                Size = new ScalableVector2(Config.LoginButtonSize, Config.LoginButtonSize),
                CornerRadius = Config.CornerRadius,
                Tint = SkinV2Color.Parse(Config.ActionButtonColor),
                UsePreviousSpriteBatchOptions = true
            };
            LoginButton.SetIcon(GlobalIcons.Get(GlobalIcon.LogIn), new Vector2(Config.LoginIconSize, Config.LoginIconSize));
            LoginButtonOptions = FixedBasis(Config.LoginButtonSize);
            LoginActionLayout.SetItemOptions(LoginButton, LoginButtonOptions);

            LoginWheel = new LoadingWheel
            {
                Parent = LoginActionLayout,
                Size = new ScalableVector2(Config.LoginIconSize, Config.LoginIconSize),
                Visible = false,
                UsePreviousSpriteBatchOptions = true
            };
            LoginWheelOptions = FixedBasis(0);
            LoginActionLayout.SetItemOptions(LoginWheel, LoginWheelOptions);
            CreateSpacer(LoginLayout, Config.ContentPadding, Config.OfflineHeight);

            AddContainedDrawable(LoginPanel);
            RefreshLayouts();

            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
            RefreshState();
            Open();
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsOpen && CloseAnimationRemaining > 0)
            {
                CloseAnimationRemaining -= (float) gameTime.ElapsedGameTime.TotalMilliseconds;
                if (CloseAnimationRemaining <= 0)
                    Visible = false;
            }

            if (Math.Abs(ScreenDarkness.Width - WindowManager.Width) > 0.001f ||
                Math.Abs(ScreenDarkness.Height - WindowManager.Height) > 0.001f)
                ScreenDarkness.Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);

            if (IsOpen && StateDirty)
                RefreshState();

            if (Math.Abs(Button.Width - Config.Width) > 0.001f ||
                Math.Abs(Button.Height - Config.OfflineHeight) > 0.001f)
                Button.Size = new ScalableVector2(Config.Width, Config.OfflineHeight);

            base.Update(gameTime);
        }

        public override void Destroy()
        {
            OnlineManager.Status.ValueChanged -= OnConnectionStatusChanged;
            ScreenDarkness.Destroy();
            base.Destroy();
            Skin.Dispose();
        }

        public override void Open()
        {
            IsOpen = true;
            CloseAnimationRemaining = 0;
            Visible = true;
            RefreshState();
            ClearAnimations();
            ChangeHeightTo((int) Config.OfflineHeight, Easing.OutQuint, 450);

            ScreenDarkness.ClearAnimations();
            ScreenDarkness.FadeTo(Config.DarknessOpacity, Easing.Linear, 200);
        }

        public override void Close()
        {
            if (!IsOpen)
                return;

            IsOpen = false;
            Closed?.Invoke();
            CloseAnimationRemaining = 550;
            ClearAnimations();
            ChangeHeightTo(0, Easing.OutQuint, 550);

            ScreenDarkness.ClearAnimations();
            ScreenDarkness.FadeTo(0, Easing.Linear, 200);
        }

        private Container CreateLoginPanel()
        {
            var panel = new Container
            {
                Size = new ScalableVector2(Config.Width, Config.OfflineHeight)
            };

            var backgroundLayout = CreateFlex(panel,
                new ScalableVector2(Config.Width, Config.OfflineHeight), FlexDirection.Row,
                FlexJustifyContent.FlexStart, FlexAlignItems.Stretch);
            var background = CreateRoundedSprite(Config.Width, Config.OfflineHeight, Config.CornerRadius,
                SkinV2Color.Parse(Config.UpperPanelColor));
            background.Parent = backgroundLayout;
            backgroundLayout.SetItemOptions(background, FixedBasis(Config.Width));
            return panel;
        }

        private static RoundedAvatar CreateAvatar(Drawable parent, float size, Texture2D image) => new RoundedAvatar(size, SkinV2BorderRadiusConfig.Normal, image)
            {
                Parent = parent
            };

        private static FlexContainer CreateFlex(Drawable parent, ScalableVector2 size, FlexDirection direction,
            FlexJustifyContent justifyContent, FlexAlignItems alignItems, float gap = 0) => new FlexContainer
        {
            Parent = parent,
            Size = size,
            Direction = direction,
            JustifyContent = justifyContent,
            AlignItems = alignItems,
            Gap = gap,
            UsePreviousSpriteBatchOptions = true
        };

        private static Container CreateSpacer(Drawable parent, float width, float height) => new Container
        {
            Parent = parent,
            Size = new ScalableVector2(width, height),
            UsePreviousSpriteBatchOptions = true
        };

        private static FlexItemOptions FixedBasis(float basis) => new FlexItemOptions
        {
            Basis = basis,
            Grow = 0,
            Shrink = 0
        };

        private static Sprite CreateRoundedSprite(float width, float height, float radius, Color tint) => new Sprite
        {
            Size = new ScalableVector2(width, height),
            Image = RoundedRectTextureCache.Get(width, height, radius),
            Tint = tint,
            UsePreviousSpriteBatchOptions = true
        };

        private void RefreshLayouts()
        {
            LoginLayout.RefreshLayout();
            LoginInfoLayout.RefreshLayout();
            LoginActionLayout.RefreshLayout();
        }

        private void RefreshState()
        {
            StateDirty = false;
            LoginStatus.Text = GetConnectionStatusText();
            LoginStatus.TruncateWithEllipsis(385);

            var canLogin = OnlineManager.Status.Value == ConnectionStatus.Disconnected;
            LoginButton.Visible = canLogin;
            LoginButton.IsClickable = canLogin;
            LoginWheel.Visible = !canLogin;
            LoginButtonOptions.Basis = canLogin ? Config.LoginButtonSize : 0;
            LoginWheelOptions.Basis = canLogin ? 0 : Config.LoginIconSize;

            RefreshLayouts();
            LoginStatus.TruncateWithEllipsis((int) LoginInfoLayout.Width);
        }

        private void Login()
        {
            Close();
            OnlineManager.Login();
        }

        private static string GetConnectionStatusText()
        {
            switch (OnlineManager.Status.Value)
            {
                case ConnectionStatus.Connecting:
                    return "Connecting to the server...";
                case ConnectionStatus.Reconnecting:
                    return "Reconnecting to the server...";
                case ConnectionStatus.Connected:
                    return "Connected!";
                default:
                    return "Disconnected from the server.";
            }
        }

        private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> args)
        {
            StateDirty = true;
            AddScheduledUpdate(() =>
            {
                if (!IsDisposed)
                    Close();
            });
        }

        private abstract class ShaderRoundedSprite : Sprite
        {
            private SpriteBatchOptions PlainScissorOptions { get; } =
                RoundedRectShader.CreateScissorSafeOptions();

            protected ShaderRoundedSprite(float width, float height, float cornerRadius)
            {
                Size = new ScalableVector2(width, height);
                var shader = RoundedRectShader.Create(cornerRadius);
                RoundedRectShader.UpdateSize(shader, new Vector2(width, height));
                SpriteBatchOptions = new SpriteBatchOptions
                {
                    RasterizerState = RoundedRectShader.ScissorSafeRasterizerState,
                    Shader = shader
                };
            }

            public override void Draw(GameTime gameTime)
            {
                base.Draw(gameTime);
                PlainScissorOptions.Begin();
            }
        }

        private sealed class RoundedAvatar : ShaderRoundedSprite
        {
            public RoundedAvatar(float size, float cornerRadius, Texture2D image) : base(size, size, cornerRadius)
            {
                if (image != null && !image.IsDisposed)
                    Image = image;
            }
        }
    }
}
