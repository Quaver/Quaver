using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Screens.V2.SkinEditor;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Navigation;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.V2
{
    /// <summary>
    ///     Shared view for empty V2 replacement screens while their real layouts are being developed.
    /// </summary>
    internal class PlaceholderScreenView : ScreenView, ISkinV2EditorHost
    {
        private SkinStoreV2Lease Skin { get; }

        private SkinV2Config RootConfig { get; set; }

        private Func<SkinV2Config, ISkinV2PlaceholderConfig> ConfigSelector { get; }

        private ISkinV2PlaceholderConfig Config => ConfigSelector(RootConfig);

        private string ConfigPath { get; }

        private string TitleLocalizationKey { get; }

        private Color BackgroundClearColor { get; set; }

        private NavigationBar Background { get; set; }

        private SpriteTextPlus Title { get; set; }

        private Container ContentRoot { get; set; }

        protected ScreenNavigation Navigation { get; private set; }

        protected virtual bool UsesNavigation => true;

        public string EditorGroupLabel => LocalizationManager.Get(TitleLocalizationKey);

        public Container PreviewRoot { get; }

        public Container EditorRoot { get; }

        public IReadOnlyList<SkinEditorTarget> EditorTargets => editorTargets;

        private List<SkinEditorTarget> editorTargets = new List<SkinEditorTarget>();

        private bool EditorLayoutActive { get; set; }

        private float EditorLeftWidth { get; set; }

        private float EditorRightWidth { get; set; }

        private float EditorBottomHeight { get; set; }

        private float LastWindowWidth { get; set; } = -1;

        private float LastWindowHeight { get; set; } = -1;

        protected PlaceholderScreenView(Screen screen,
            Func<SkinV2Config, ISkinV2PlaceholderConfig> configSelector,
            string configPath, string titleLocalizationKey) : base(screen)
        {
            ConfigSelector = configSelector;
            ConfigPath = configPath;
            TitleLocalizationKey = titleLocalizationKey;
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

            BackgroundClearColor = SkinV2Color.Parse(Config.Background.SolidColor);
            Background = new NavigationBar(WindowManager.Width, WindowManager.Height)
            {
                Parent = ContentRoot,
                Background = SkinV2Background.Create(Skin, Config.Background)
            };

            Title = new SpriteTextPlus(FontManager.GetWobbleFont(Config.Title.Font),
                LocalizationManager.Get(TitleLocalizationKey), Config.Title.FontSize)
            {
                Parent = ContentRoot,
                Alignment = Alignment.MidCenter,
                Tint = SkinV2Color.Parse(Config.Title.Color)
            };

            editorTargets = new List<SkinEditorTarget>
            {
                new SkinEditorTarget(ConfigPath + "-background",
                    LocalizationManager.Get("SkinEditor_Component_Background"),
                    ConfigPath + ".Background", Background),
                new SkinEditorTarget(ConfigPath + "-title",
                    LocalizationManager.Get("SkinEditor_Component_ScreenTitle"),
                    ConfigPath + ".Title", Title)
            };

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
            if (!UsesNavigation)
            {
                ScreenManager.RemoveElement(ScreenNavigation.ElementKey);
                Navigation = null;
                return;
            }

            Navigation = ScreenNavigation.EnsureAttached(PreviewRoot);
            ConfigureNavigation(Navigation);
            AddNavigationTargets(Navigation);
        }

        public void ApplySkinEditorPreview(SkinV2Config config)
        {
            RootConfig = config;
            BuildContent();
            if (UsesNavigation)
            {
                Navigation = ScreenNavigation.ReplaceAttached(PreviewRoot, RootConfig);
                ConfigureNavigation(Navigation);
                AddNavigationTargets(Navigation);
            }
            else
            {
                ScreenManager.RemoveElement(ScreenNavigation.ElementKey);
                Navigation = null;
            }

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

        private void AddNavigationTargets(ScreenNavigation navigation) =>
            editorTargets.AddRange(navigation.GetSkinEditorTargets());

        protected virtual void ConfigureNavigation(ScreenNavigation navigation)
        {
            navigation.ShowApplicationTopBar(((QuaverScreen) Screen).Type);
            navigation.ShowDefaultFooter();
        }

        /// <summary>
        ///     Reapplies this screen's navigation configuration after screen state changes.
        /// </summary>
        protected void RefreshNavigation()
        {
            if (!UsesNavigation)
                return;

            Navigation = ScreenNavigation.EnsureAttached(PreviewRoot);
            ConfigureNavigation(Navigation);
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
            Container.Size = new ScalableVector2(width, height);
            PreviewRoot.Size = new ScalableVector2(width, height);
            ContentRoot.Size = new ScalableVector2(width, height);
            Background.Size = new ScalableVector2(width, height);
            UpdateEditorLayout();
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
    }
}
