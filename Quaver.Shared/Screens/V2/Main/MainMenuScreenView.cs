using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.V2.Main.UI;
using Quaver.Shared.Screens.V2.SkinEditor;
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
    internal sealed class MainMenuScreenView : ScreenView, ISkinV2EditorHost
    {
        private SkinStoreV2Lease Skin { get; }

        private SkinV2Config RootConfig { get; set; }

        private SkinV2MainConfig Config => RootConfig.Screens.Main;

        private SkinV2NavigationConfig NavigationConfig => RootConfig.Shared.Navigation;

        private Color BackgroundClearColor { get; set; }

        private NavigationBar Background { get; set; }

        private Sprite BackgroundEffect { get; set; }

        private FlexContainer Content { get; set; }

        private Sprite Logo { get; set; }

        private bool UsesBundledLogo { get; set; }

        private FlexItemOptions LogoOptions { get; set; }

        private FlexContainer ActionRow { get; set; }

        private List<FlexItemOptions> ActionOptions { get; } = new List<FlexItemOptions>();

        private List<Drawable> ActionButtons { get; } = new List<Drawable>();

        private MainMenuNewsCard News { get; set; }

        private Container ContentRoot { get; set; }

        public Container PreviewRoot { get; }

        public Container EditorRoot { get; }

        public string EditorGroupLabel => LocalizationManager.Get("SkinEditor_Group_MainMenu");

        public IReadOnlyList<SkinEditorTarget> EditorTargets => editorTargets;

        private List<SkinEditorTarget> editorTargets = new List<SkinEditorTarget>();

        private bool EditorLayoutActive { get; set; }

        private float EditorLeftWidth { get; set; }

        private float EditorRightWidth { get; set; }

        private float EditorBottomHeight { get; set; }

        private float LastWindowWidth { get; set; } = -1;

        private float LastWindowHeight { get; set; } = -1;

        private float NavigationBarHeight => NavigationConfig.Button.Size + NavigationConfig.EdgePadding * 2;

        public MainMenuScreenView(MainMenuScreen screen) : base(screen)
        {
            Skin = SkinManager.AcquireV2();
            RootConfig = Skin.Config;

            Container.Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            PreviewRoot = new Container
            {
                Parent = Container,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height),
                Pivot = Vector2.Zero
            };
            EditorRoot = new Container
            {
                Parent = Container,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height),
                Visible = false
            };

            BuildContent();
        }

        private void BuildContent()
        {
            ContentRoot?.Destroy();
            ContentRoot = new Container
            {
                Parent = PreviewRoot,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height)
            };

            ActionOptions.Clear();
            ActionButtons.Clear();
            BackgroundClearColor = SkinV2Color.Parse(Config.Background.SolidColor);

            Background = new NavigationBar(WindowManager.Width, WindowManager.Height)
            {
                Parent = ContentRoot,
                Background = SkinV2Background.Create(Skin, Config.Background,
                    TextureManager.Load("Quaver.Resources/Textures/UI/Screens/Main/background.jpg"))
            };

            BackgroundEffect = CreateBackgroundEffect(Config.BackgroundEffects.Effect);
            if (BackgroundEffect != null)
                BackgroundEffect.Parent = ContentRoot;

            Content = new FlexContainer
            {
                Parent = ContentRoot,
                Position = new ScalableVector2(Config.Layout.HorizontalPadding, NavigationBarHeight),
                Size = new ScalableVector2(WindowManager.Width - Config.Layout.HorizontalPadding * 2,
                    WindowManager.Height - NavigationBarHeight * 2),
                Direction = FlexDirection.Column,
                JustifyContent = FlexJustifyContent.Center,
                AlignItems = FlexAlignItems.Center,
                RowGap = Config.Layout.RowGap
            };

            UsesBundledLogo = string.IsNullOrWhiteSpace(Config.Logo.Image);
            if (UsesBundledLogo)
            {
                var accentColor = SkinV2Color.Parse(RootConfig.Shared.Brand.AccentColor);
                Logo = accentColor == SkinV2Color.Parse(SkinV2BrandConfig.DefaultAccentColor)
                    ? new Sprite
                    {
                        Image = TextureManager.Load(
                            "Quaver.Resources/Textures/UI/Screens/Main/Logos/logo-colored.png")
                    }
                    : new TintableLogo(
                        TextureManager.Load("Quaver.Resources/Textures/UI/Screens/Main/Logos/logo.png"),
                        TextureManager.Load(
                            "Quaver.Resources/Textures/UI/Screens/Main/Logos/logo-tail.png"),
                        Color.Lerp(Color.White, accentColor, 0.5f),
                        TextureManager.Load(
                            "Quaver.Resources/Textures/UI/Screens/Main/Logos/logo-accent.png"),
                        accentColor);
                Logo.Size = new ScalableVector2(Logo.ImageWidth, Logo.ImageHeight);
            }
            else
            {
                var logoTexture = Skin.LoadTexture(Config.Logo.Image,
                    TextureManager.Load(
                        "Quaver.Resources/Textures/UI/Screens/Main/Logos/logo-colored.png"));
                Logo = new Sprite
                {
                    Image = logoTexture,
                    Size = new ScalableVector2(logoTexture.Width, logoTexture.Height)
                };
            }

            Logo.Parent = Content;
            LogoOptions = new FlexItemOptions { Basis = Logo.ImageHeight, Shrink = 1 };
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

            var screen = (MainMenuScreen) Screen;
            CreateAction(GlobalIcons.Get(GlobalIcon.SinglePlayer), "Screen_Main_SinglePlayer",
                screen.ExitToSinglePlayer);
            CreateAction(GlobalIcons.Get(GlobalIcon.Multiplayer), "Screen_Main_Multiplayer",
                screen.ExitToMultiplayer);
            CreateAction(GlobalIcons.Get(GlobalIcon.Edit), "Screen_Main_Editor", screen.ExitToEditor);
            CreateAction(GlobalIcons.Get(GlobalIcon.Download), "Screen_Main_DownloadSongs",
                screen.ExitToDownload);

            News = new MainMenuNewsCard(Config.News.MaximumWidth, Skin, Config.News)
            {
                Parent = ContentRoot,
                Alignment = Alignment.BotCenter,
                Y = Config.News.BottomOffset
            };

            editorTargets = new List<SkinEditorTarget>
            {
                new SkinEditorTarget("main-background",
                    LocalizationManager.Get("SkinEditor_Component_Background"),
                    "Screens.Main.Background", Background),
                new SkinEditorTarget("main-effects",
                    LocalizationManager.Get("SkinEditor_Component_BackgroundEffects"),
                    "Screens.Main.BackgroundEffects", BackgroundEffect ?? Background),
                new SkinEditorTarget("main-logo",
                    LocalizationManager.Get("SkinEditor_Component_Logo"),
                    "Screens.Main.Logo", Logo),
                new SkinEditorTarget("main-actions",
                    LocalizationManager.Get("SkinEditor_Component_Actions"),
                    "Screens.Main.Actions", ActionButtons.ToArray()),
                new SkinEditorTarget("main-news",
                    LocalizationManager.Get("SkinEditor_Component_News"),
                    "Screens.Main.News", News)
            };
            if (UsesBundledLogo)
            {
                editorTargets.Insert(3, new SkinEditorTarget("brand-logo-accent",
                    LocalizationManager.Get("SkinEditor_Component_LogoAccent"),
                    "Shared.Brand", Logo));
            }

            LastWindowWidth = -1;
            LastWindowHeight = -1;
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

        public void EnsureNavigation()
        {
            var navigation = ScreenNavigation.EnsureAttached(PreviewRoot);
            navigation.ShowMainTopBar();
            navigation.ShowDefaultFooter();
            AddNavigationTargets(navigation);
        }

        public void ApplySkinEditorPreview(SkinV2Config config)
        {
            RootConfig = config;
            BuildContent();
            var navigation = ScreenNavigation.ReplaceAttached(PreviewRoot, RootConfig);
            AddNavigationTargets(navigation);
            UpdateEditorLayout();
        }

        public void SetSkinEditorLayout(bool active, float leftPanelWidth = 0, float rightPanelWidth = 0,
            float assetPanelHeight = 0)
        {
            EditorLayoutActive = active;
            EditorLeftWidth = leftPanelWidth;
            EditorRightWidth = rightPanelWidth;
            EditorBottomHeight = assetPanelHeight;
            EditorRoot.Visible = active;
            UpdateEditorLayout();
        }

        private void AddNavigationTargets(ScreenNavigation navigation)
        {
            editorTargets.AddRange(navigation.GetSkinEditorTargets());
        }

        private void UpdateEditorLayout()
        {
            Container.Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            EditorRoot.Size = Container.Size;

            if (!EditorLayoutActive)
            {
                PreviewRoot.Position = new ScalableVector2(0, 0);
                PreviewRoot.Scale = Vector2.One;
                return;
            }

            const float margin = 16;
            var availableWidth = Math.Max(1,
                WindowManager.Width - EditorLeftWidth - EditorRightWidth - margin * 2);
            var availableHeight = Math.Max(1,
                WindowManager.Height - EditorBottomHeight - margin * 2);
            var scale = Math.Min(availableWidth / WindowManager.Width,
                availableHeight / WindowManager.Height);
            PreviewRoot.Scale = new Vector2(scale);
            PreviewRoot.Position = new ScalableVector2(
                EditorLeftWidth + margin + (availableWidth - WindowManager.Width * scale) / 2f,
                margin + (availableHeight - WindowManager.Height * scale) / 2f);
        }

        private void CreateAction(TextureRegion icon, string localizationKey, Action action)
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
            button.SetIcon(icon, new Vector2(Config.Actions.IconSize, Config.Actions.IconSize));
            button.SetLabel(FontManager.GetWobbleFont(Config.Actions.Font), LocalizationManager.Get(localizationKey),
                Config.Actions.FontSize, SkinV2Color.Parse(Config.Actions.TextColor));

            var options = new FlexItemOptions { Basis = Config.Actions.ButtonWidth, Grow = 1, Shrink = 1 };
            ActionOptions.Add(options);
            ActionButtons.Add(button);
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
            PreviewRoot.Size = new ScalableVector2(width, height);
            ContentRoot.Size = new ScalableVector2(width, height);
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
            var logoWidth = Config.Logo.Scale * Math.Min(logoTexture.Width,
                Math.Max(Config.Logo.MinimumWidth,
                    Math.Min(contentWidth - Config.Logo.HorizontalMargin, width * Config.Logo.ViewportWidthRatio)));
            var logoHeight = logoWidth * logoTexture.Height / logoTexture.Width;
            Logo.Size = new ScalableVector2(logoWidth, logoHeight);
            LogoOptions.Basis = logoHeight;

            News.ApplyWidth(Math.Min(Config.News.MaximumWidth, contentWidth - Config.News.HorizontalMargin));
            Content.RefreshLayout();
            ActionRow.RefreshLayout();
            UpdateEditorLayout();
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
                        "Quaver.Resources/Textures/UI/Screens/Main/BackgroundShapes/shape-primary.png")),
                Skin.LoadTexture(icons.Secondary,
                    TextureManager.Load(
                        "Quaver.Resources/Textures/UI/Screens/Main/BackgroundShapes/shape-secondary.png")),
                Skin.LoadTexture(icons.Tertiary,
                    TextureManager.Load(
                        "Quaver.Resources/Textures/UI/Screens/Main/BackgroundShapes/shape-tertiary.png"))
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
