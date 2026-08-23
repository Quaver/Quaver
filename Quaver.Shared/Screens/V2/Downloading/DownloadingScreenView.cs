using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Screens.V2.Downloading.UI;
using Quaver.Shared.Screens.V2.SkinEditor;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Navigation;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.V2.Downloading
{
    internal sealed class DownloadingScreenView : ScreenView, ISkinV2EditorHost
    {
        private SkinStoreV2Lease Skin { get; }

        private SkinV2Config RootConfig { get; set; }

        private SkinV2DownloadingConfig Config => RootConfig.Screens.Downloading;

        private SkinV2NavigationConfig NavigationConfig => RootConfig.Shared.Navigation;

        private Color BackgroundClearColor { get; set; }

        private NavigationBar Background { get; set; }

        private DownloadingSearchPanel SearchPanel { get; set; }

        private Container ContentRoot { get; set; }

        private float LastWindowWidth { get; set; } = -1;

        private float LastWindowHeight { get; set; } = -1;

        private bool EditorLayoutActive { get; set; }

        private float EditorLeftWidth { get; set; }

        private float EditorRightWidth { get; set; }

        private float EditorBottomHeight { get; set; }

        private List<SkinEditorTarget> editorTargets = new List<SkinEditorTarget>();

        private float NavigationBarHeight =>
            NavigationConfig.Button.Size + NavigationConfig.EdgePadding * 2;

        public Container PreviewRoot { get; }

        public Container EditorRoot { get; }

        public string EditorGroupLabel =>
            LocalizationManager.Get("SkinEditor_Group_Download");

        public IReadOnlyList<SkinEditorTarget> EditorTargets => editorTargets;

        public DownloadingScreenView(DownloadingScreen screen) : base(screen)
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
            ConfigureNavigation(navigation);
            AddNavigationTargets(navigation);
        }

        public void ApplySkinEditorPreview(SkinV2Config config)
        {
            RootConfig = config;
            BuildContent();
            var navigation = ScreenNavigation.ReplaceAttached(PreviewRoot, RootConfig);
            ConfigureNavigation(navigation);
            AddNavigationTargets(navigation);
            UpdateEditorLayout();
        }

        public void SetSkinEditorLayout(bool active, float leftPanelWidth = 0,
            float rightPanelWidth = 0, float assetPanelHeight = 0)
        {
            EditorLayoutActive = active;
            EditorLeftWidth = leftPanelWidth;
            EditorRightWidth = rightPanelWidth;
            EditorBottomHeight = assetPanelHeight;
            EditorRoot.Visible = active;
            UpdateEditorLayout();
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
            SearchPanel = new DownloadingSearchPanel(
                Math.Max(1, WindowManager.Width - Config.Layout.HorizontalPadding * 2),
                ((DownloadingScreen) Screen).SearchState, Config)
            {
                Parent = ContentRoot,
                Position = new ScalableVector2(Config.Layout.HorizontalPadding,
                    NavigationBarHeight + Config.Layout.TopPadding)
            };

            editorTargets = new List<SkinEditorTarget>
            {
                new SkinEditorTarget("downloading-background",
                    LocalizationManager.Get("SkinEditor_Component_Background"),
                    "Screens.Downloading.Background", Background),
                new SkinEditorTarget("downloading-search",
                    LocalizationManager.Get("SkinEditor_Component_SearchArea"),
                    "Screens.Downloading.SearchArea", SearchPanel)
            };

            LastWindowWidth = -1;
            LastWindowHeight = -1;
            UpdateResponsiveLayout(true);
        }

        private static void ConfigureNavigation(ScreenNavigation navigation)
        {
            navigation.ShowApplicationTopBar(QuaverScreenType.Download);
            navigation.ShowDefaultFooter();
        }

        private void AddNavigationTargets(ScreenNavigation navigation) =>
            editorTargets.AddRange(navigation.GetSkinEditorTargets());

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
            SearchPanel.Position = new ScalableVector2(Config.Layout.HorizontalPadding,
                NavigationBarHeight + Config.Layout.TopPadding);
            SearchPanel.Width = Math.Max(1, width - Config.Layout.HorizontalPadding * 2);
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
