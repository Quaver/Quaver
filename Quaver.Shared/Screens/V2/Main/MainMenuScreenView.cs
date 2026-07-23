using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.V2.Main.UI;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Navigation;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.V2.Main
{
    /// <summary>
    ///     View for the rewritten main menu.
    /// </summary>
    public sealed class MainMenuScreenView : ScreenView
    {
        private SkinStoreV2Lease Skin { get; }

        private SkinV2MainConfig Config { get; }

        private SkinV2NavigationConfig NavigationConfig { get; }

        private Color BackgroundClearColor { get; }

        private NavigationBar Background { get; }

        private Sprite BackgroundEffect { get; }

        private FlexContainer Content { get; }

        private Sprite Logo { get; }

        private FlexItemOptions LogoOptions { get; }

        private FlexContainer ActionRow { get; }

        private List<FlexItemOptions> ActionOptions { get; } = new List<FlexItemOptions>();

        private MainMenuNewsCard News { get; }

        private float LastWindowWidth { get; set; } = -1;

        private float LastWindowHeight { get; set; } = -1;

        private float NavigationBarHeight => NavigationConfig.Button.Size + NavigationConfig.EdgePadding * 2;

        public MainMenuScreenView(MainMenuScreen screen) : base(screen)
        {
            Skin = SkinManager.AcquireV2();
            Config = Skin.Config.Screens.Main;
            NavigationConfig = Skin.Config.Shared.Navigation;
            BackgroundClearColor = SkinV2Color.Parse(Config.Background.SolidColor);

            Background = new NavigationBar(WindowManager.Width, WindowManager.Height)
            {
                Parent = Container,
                Background = SkinV2Background.Create(Skin, Config.Background,
                    TextureManager.Load("Quaver.Resources/Textures/UI/Screens/Main/background.png"))
            };

            BackgroundEffect = CreateBackgroundEffect(Config.BackgroundEffects.Effect);
            if (BackgroundEffect != null)
                BackgroundEffect.Parent = Container;

            Content = new FlexContainer
            {
                Parent = Container,
                Position = new ScalableVector2(Config.Layout.HorizontalPadding, NavigationBarHeight),
                Size = new ScalableVector2(WindowManager.Width - Config.Layout.HorizontalPadding * 2,
                    WindowManager.Height - NavigationBarHeight * 2),
                Direction = FlexDirection.Column,
                JustifyContent = FlexJustifyContent.Center,
                AlignItems = FlexAlignItems.Center,
                RowGap = Config.Layout.RowGap
            };

            var logoTexture = Skin.LoadTexture(Config.Logo.Image, TextureManager.Load("Quaver.Resources/Textures/UI/Screens/Main/logo-colored.png"));

            Logo = new Sprite
            {
                Parent = Content,
                Image = logoTexture,
                Size = new ScalableVector2(logoTexture.Width, logoTexture.Height)
            };
            LogoOptions = new FlexItemOptions { Basis = logoTexture.Height, Shrink = 1 };
            Content.SetItemOptions(Logo, LogoOptions);

            ActionRow = new FlexContainer
            {
                Parent = Content,
                Size = new ScalableVector2(Content.Width, Config.Actions.SingleRowHeight),
                Direction = FlexDirection.Row,
                Wrap = FlexWrap.Wrap,
                JustifyContent = FlexJustifyContent.Center,
                AlignItems = FlexAlignItems.Center,
                RowGap = Config.Actions.RowGap,
                ColumnGap = Config.Actions.Gap
            };
            Content.SetItemOptions(ActionRow,
                new FlexItemOptions { Basis = Config.Actions.SingleRowHeight, Shrink = 0 });

            CreateAction(FontAwesome.Get(FontAwesomeIcon.fa_gamepad_console), Config.Actions.SinglePlayerIcon,
                "Screen_Main_SinglePlayer",
                screen.ExitToSinglePlayer);
            CreateAction(FontAwesome.Get(FontAwesomeIcon.fa_group_profile_users), Config.Actions.MultiplayerIcon,
                "Screen_Main_Multiplayer",
                screen.ExitToMultiplayer);
            CreateAction(FontAwesome.Get(FontAwesomeIcon.fa_pencil), Config.Actions.EditorIcon,
                "Screen_Main_Editor", screen.ExitToEditor);
            CreateAction(FontAwesome.Get(FontAwesomeIcon.fa_download_to_storage_drive), Config.Actions.DownloadIcon,
                "Screen_Main_DownloadSongs",
                screen.ExitToDownload);

            News = new MainMenuNewsCard(Config.News.MaximumWidth, Skin, Config.News)
            {
                Parent = Container,
                Alignment = Alignment.BotCenter,
                Y = Config.News.BottomOffset
            };

            UpdateResponsiveLayout(true);
        }

        public override void Update(GameTime gameTime)
        {
            UpdateResponsiveLayout();
            Container.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(BackgroundClearColor);
            Container.Draw(gameTime);
        }

        public override void Destroy()
        {
            Container.Destroy();
            Skin.Dispose();
        }

        private void CreateAction(Texture2D fallbackIcon, string iconPath, string localizationKey, Action action)
        {
            var button = new MainActionButton(action, Config.Actions)
            {
                Parent = ActionRow,
                Size = new ScalableVector2(Config.Actions.ButtonWidth, Config.Actions.ButtonHeight),
                Tint = SkinV2Color.Parse(Config.Actions.Color),
                CornerRadius = Config.Actions.CornerRadius,
                PerformHoverFade = false,
                NormalColor = SkinV2Color.Parse(Config.Actions.Color),
                HoverColor = SkinV2Color.Parse(Config.Actions.HoverColor),
                AccentColor = SkinV2Color.Parse(Config.Actions.AccentColor)
            };
            button.SetIcon(Skin.LoadTexture(iconPath, fallbackIcon),
                new Vector2(Config.Actions.IconSize, Config.Actions.IconSize));
            button.SetLabel(FontManager.GetWobbleFont(Config.Actions.Font), LocalizationManager.Get(localizationKey),
                Config.Actions.FontSize, SkinV2Color.Parse(Config.Actions.TextColor));

            var options = new FlexItemOptions { Basis = Config.Actions.ButtonWidth, Grow = 1, Shrink = 1 };
            ActionOptions.Add(options);
            ActionRow.SetItemOptions(button, options);
        }

        private void UpdateResponsiveLayout(bool force = false)
        {
            var width = WindowManager.Width;
            var height = WindowManager.Height;

            if (!force && Math.Abs(width - LastWindowWidth) < 0.001f &&
                Math.Abs(height - LastWindowHeight) < 0.001f)
                return;

            LastWindowWidth = width;
            LastWindowHeight = height;

            // The root container captures the window size when it is constructed, so keep it
            // synchronized after resizes. Bottom-aligned children (such as the news card) are
            // recalculated when this changes.
            Container.Size = new ScalableVector2(width, height);
            Background.Size = new ScalableVector2(width, height);
            if (BackgroundEffect != null)
                BackgroundEffect.Size = new ScalableVector2(width, height);

            var contentWidth = Math.Max(Config.Layout.MinimumContentWidth,
                width - Config.Layout.HorizontalPadding * 2);
            var contentHeight = Math.Max(Config.Layout.MinimumContentHeight,
                height - NavigationBarHeight * 2);
            Content.Position = new ScalableVector2((width - contentWidth) / 2f, NavigationBarHeight);
            Content.Size = new ScalableVector2(contentWidth, contentHeight);

            var fourColumns = contentWidth >= Config.Actions.FourColumnBreakpoint;
            var columns = fourColumns ? 4 : 2;
            var actionWidth = Math.Min(contentWidth, fourColumns ? contentWidth :
                Math.Max(Config.Layout.MinimumContentWidth, contentWidth - Config.Actions.TwoColumnMargin));
            var basis = (actionWidth - Config.Actions.Gap * (columns - 1)) / columns;

            ActionRow.Size = new ScalableVector2(actionWidth, fourColumns ? Config.Actions.SingleRowHeight :
                Config.Actions.WrappedRowHeight);
            foreach (var options in ActionOptions)
                options.Basis = basis;

            Content.SetItemOptions(ActionRow,
                new FlexItemOptions
                {
                    Basis = fourColumns ? Config.Actions.SingleRowHeight : Config.Actions.WrappedRowHeight,
                    Shrink = 0
                });

            var logoTexture = Logo.Image;
            var logoWidth = Math.Min(logoTexture.Width,
                Math.Max(Config.Logo.MinimumWidth,
                    Math.Min(contentWidth - Config.Logo.HorizontalMargin, width * Config.Logo.ViewportWidthRatio)));
            var logoHeight = logoWidth * logoTexture.Height / logoTexture.Width;
            Logo.Size = new ScalableVector2(logoWidth, logoHeight);
            LogoOptions.Basis = logoHeight;

            News.ApplyWidth(Math.Min(Config.News.MaximumWidth, contentWidth - Config.News.HorizontalMargin));
            Content.RefreshLayout();
            ActionRow.RefreshLayout();
        }

        private Sprite CreateBackgroundEffect(SkinV2MainBackgroundEffect effect)
        {
            var primaryColor = SkinV2Color.Parse(Config.BackgroundEffects.PrimaryColor);
            var secondaryColor = SkinV2Color.Parse(Config.BackgroundEffects.SecondaryColor);

            switch (effect)
            {
                case SkinV2MainBackgroundEffect.Particles:
                    return new BackgroundParticleSystem(primaryColor, secondaryColor);
                case SkinV2MainBackgroundEffect.SoftPop:
                    return new BackgroundSoftPopParticleSystem(primaryColor, secondaryColor);
                case SkinV2MainBackgroundEffect.Matrix:
                    return new BackgroundMatrixIconSystem(LoadBackgroundEffectIcons(), primaryColor, secondaryColor);
                case SkinV2MainBackgroundEffect.RibbonTrail:
                    return new BackgroundRibbonTrailSystem(primaryColor, secondaryColor);
                case SkinV2MainBackgroundEffect.Snowfall:
                    return new BackgroundSnowfallIconSystem(LoadBackgroundEffectIcons(), primaryColor, secondaryColor);
                case SkinV2MainBackgroundEffect.StreakRain:
                    return new BackgroundStreakRainSystem(primaryColor, secondaryColor);
                default:
                    return null;
            }
        }

        private Texture2D[] LoadBackgroundEffectIcons()
        {
            var icons = Config.BackgroundEffects.Icons;
            return new[]
            {
                Skin.LoadTexture(icons.Primary,
                    TextureManager.Load(
                        "Quaver.Resources/Textures/UI/Screens/Main/background-shapes/shape-primary.png")),
                Skin.LoadTexture(icons.Secondary,
                    TextureManager.Load(
                        "Quaver.Resources/Textures/UI/Screens/Main/background-shapes/shape-secondary.png")),
                Skin.LoadTexture(icons.Tertiary,
                    TextureManager.Load(
                        "Quaver.Resources/Textures/UI/Screens/Main/background-shapes/shape-tertiary.png"))
            };
        }

        private sealed class MainActionButton : RoundedButton
        {
            private RoundedButton Indicator { get; }

            private bool HoverVisualActive { get; set; }

            public Color NormalColor { get; set; }

            public Color HoverColor { get; set; }

            public Color AccentColor
            {
                set => Indicator.Tint = value;
            }

            private SkinV2MainActionsConfig Config { get; }

            public MainActionButton(Action action, SkinV2MainActionsConfig config)
                : base((sender, args) => action())
            {
                Config = config;
                Indicator = new RoundedButton
                {
                    Parent = this,
                    Alignment = Alignment.BotCenter,
                    Position = new ScalableVector2(0, Config.IndicatorHeight + Config.IndicatorSpacing),
                    Size = new ScalableVector2(Config.IndicatorWidth, Config.IndicatorHeight),
                    Tint = Color.White,
                    Alpha = Config.IndicatorIdleOpacity,
                    PerformHoverFade = false,
                    IsClickable = false,
                    IsInteractionEnabled = false
                };
            }

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);

                if (HoverVisualActive == IsHovered)
                    return;

                HoverVisualActive = IsHovered;
                Tint = HoverVisualActive ? HoverColor : NormalColor;
                Indicator.Alpha = HoverVisualActive ? 1 : Config.IndicatorIdleOpacity;
            }
        }
    }
}
