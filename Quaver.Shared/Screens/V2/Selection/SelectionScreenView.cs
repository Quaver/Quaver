using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.V2.Selection.UI;
using Quaver.Shared.Screens.V2.SkinEditor;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Navigation;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.V2.Selection
{
    /// <summary>
    ///     Builds and sets the Song Select interface and its skin editor preview
    /// </summary>
    internal sealed class SelectionScreenView : ScreenView, ISkinV2EditorHost
    {
        private SelectionScreen SelectionScreen => (SelectionScreen)Screen;
        private SkinStoreV2Lease Skin { get; }
        private SkinV2Config RootConfig { get; set; }
        private SkinV2SelectionConfig Config => RootConfig.Screens.Selection;
        private Color BackgroundClearColor { get; set; }
        private Container ContentRoot { get; set; }
        private NavigationBar Background { get; set; }
        private FlexContainer Columns { get; set; }
        private HorizontalClippingContainer DetailsViewport { get; set; }
        private Sprite DetailsBackground { get; set; }
        private SpriteTextPlus Title { get; set; }
        private SpriteTextPlus Artist { get; set; }
        private SpriteTextPlus Difficulty { get; set; }
        private Container RightPanel { get; set; }
        private SelectionMapsetList List { get; set; }
        private SpriteTextPlus EmptyState { get; set; }
        private float LastWidth { get; set; } = -1;
        private float LastHeight { get; set; } = -1;
        private bool Destroyed { get; set; }
        private bool EditorLayoutActive { get; set; }
        private float EditorLeftWidth { get; set; }
        private float EditorRightWidth { get; set; }
        private float EditorBottomHeight { get; set; }
        private List<SkinEditorTarget> Targets { get; set; } = new();

        public Container PreviewRoot { get; }
        public Container EditorRoot { get; }
        public string EditorGroupLabel => LocalizationManager.Get("Screen_V2_SelectionScreen");
        public IReadOnlyList<SkinEditorTarget> EditorTargets => Targets;

        internal SelectionScreenView(SelectionScreen screen) : base(screen)
        {
            Skin = SkinManager.AcquireV2();
            RootConfig = Skin.Config;
            PreviewRoot = new Container { Parent = Container, Pivot = Vector2.Zero };
            EditorRoot = new Container
            {
                Parent = Container,
                Visible = false,
                UpdateWhenInvisible = false
            };

            BuildContent();
            screen.LibraryChanged += OnLibraryChanged;
            MapManager.Selected.ValueChanged += OnSelectedMapChanged;
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

        public void EnsureNavigation()
        {
            var navigation = ScreenNavigation.EnsureAttached(PreviewRoot);
            ConfigureNavigation(navigation);
            SetEditorTargets(navigation);
        }

        public void ApplySkinEditorPreview(SkinV2Config config)
        {
            RootConfig = config;
            BuildContent();
            var navigation = ScreenNavigation.ReplaceAttached(PreviewRoot, RootConfig);
            ConfigureNavigation(navigation);
            SetEditorTargets(navigation);
        }

        public void SetSkinEditorLayout(bool active, float leftPanelWidth = 0, float rightPanelWidth = 0, float assetPanelHeight = 0)
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
            ContentRoot = new Container { Parent = PreviewRoot };
            BackgroundClearColor = SkinV2Color.Parse(Config.Background.SolidColor);
            
            Background = new NavigationBar(WindowManager.Width, WindowManager.Height)
            {
                Parent = ContentRoot,
                Background = SkinV2Background.Create(Skin, Config.Background)
            };
            
            Columns = new FlexContainer
            {
                Parent = ContentRoot,
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Stretch,
                JustifyContent = FlexJustifyContent.SpaceBetween
            };
            DetailsViewport = new HorizontalClippingContainer { Parent = Columns };
            Columns.SetItemOptions(DetailsViewport, new FlexItemOptions { Basis = Config.Layout.LeftPanelWidth, Shrink = 1 });
            
            DetailsBackground = new Sprite
            {
                Parent = DetailsViewport,
                Tint = SkinV2Color.Parse(Config.Details.BackgroundColor),
                UsePreviousSpriteBatchOptions = true
            };
            
            Title = CreateDetailsText(Config.Details.TitleFontSize, Config.Details.TextColor);
            Artist = CreateDetailsText(Config.Details.FontSize, Config.Details.SecondaryTextColor);
            Difficulty = CreateDetailsText(Config.Details.FontSize, Config.Details.TextColor);

            RightPanel = new Container { Parent = Columns };
            Columns.SetItemOptions(RightPanel, new FlexItemOptions { Basis = Config.Layout.RightPanelWidth, Shrink = 1 });
            
            List = new SelectionMapsetList(Config.List, Config.Mapset, Config.Difficulty, () => SelectionScreen.CanHandleInput, SelectionScreen.SelectMapset, SelectionScreen.SelectMap)
            {
                Parent = RightPanel
            };

            EmptyState = new SpriteTextPlus(FontManager.GetWobbleFont(Config.Details.Font), SelectionLocalization.Get("No maps found!"), Config.Details.FontSize)
            {
                Parent = RightPanel,
                Alignment = Alignment.MidCenter,
                Tint = SkinV2Color.Parse(Config.Details.SecondaryTextColor)
            };
            LastWidth = LastHeight = -1;
            UpdateResponsiveLayout();
            List.Initialize(RightPanel.Size, SelectionScreen.AvailableMapsets, MapManager.Selected.Value);
            EmptyState.Visible = SelectionScreen.AvailableMapsets.Count == 0;
        }

        private SpriteTextPlus CreateDetailsText(int fontSize, string color) =>
            new SpriteTextPlus(FontManager.GetWobbleFont(Config.Details.Font), "", fontSize)
            {
                Parent = DetailsViewport,
                Tint = SkinV2Color.Parse(color),
                UsePreviousSpriteBatchOptions = true
            };

        private void ConfigureNavigation(ScreenNavigation navigation)
        {
            navigation.ShowApplicationTopBar(QuaverScreenType.Select);
            navigation.ShowSelectionFooter(SelectionScreen.HandleBackAction,
                SelectionScreen.ExitToGameplay);
        }

        private void SetEditorTargets(ScreenNavigation navigation)
        {
            Targets = new List<SkinEditorTarget>
            {
                new("selection-background", LocalizationManager.Get("SkinEditor_Component_Background"), "Screens.Selection.Background", Background),
                new("selection-details", LocalizationManager.Get("Screen_V2_SelectionScreen"), "Screens.Selection.Details", DetailsViewport),
                new("selection-list", LocalizationManager.Get("Screen_Download_Mapsets"), "Screens.Selection.List", List),
                new("selection-mapset", LocalizationManager.Get("Screen_Download_Mapsets"), "Screens.Selection.Mapset", List),
                new("selection-difficulty", LocalizationManager.Get("Screen_Selection_Difficulty"), "Screens.Selection.Difficulty", List)
            };
            Targets.AddRange(navigation.GetSkinEditorTargets());
        }

        internal bool TryMoveDifficultySelection(int direction) => List.TryMoveDifficultySelection(direction);

        private void UpdateResponsiveLayout()
        {
            var width = WindowManager.Width;
            var height = WindowManager.Height;
            if (LastWidth.Equals(width) && LastHeight.Equals(height))
                return;

            LastWidth = width;
            LastHeight = height;
            Container.Size = new ScalableVector2(width, height);
            PreviewRoot.Size = ContentRoot.Size = Background.Size = Container.Size;

            var navigation = RootConfig.Shared.Navigation;
            var barHeight = navigation.Button.Size + navigation.EdgePadding * 2;
            var horizontalPadding = Math.Min(Config.Layout.HorizontalPadding, width / 4f);
            var verticalPadding = Config.Layout.VerticalPadding;

            Columns.Position = new ScalableVector2(horizontalPadding, barHeight + verticalPadding);
            Columns.Size = new ScalableVector2(
                Math.Max(1, width - horizontalPadding * 2),
                Math.Max(1, height - barHeight * 2 - verticalPadding * 2));
            Columns.ColumnGap = Math.Min(Config.Layout.ContentGap, Columns.Width / 4f);
            Columns.RefreshLayout();

            DetailsBackground.Size = DetailsViewport.Size;
            if (List.IsInitialized)
                List.Size = RightPanel.Size;

            EmptyState.Text = SelectionLocalization.Get("No maps found!");
            EmptyState.TruncateWithEllipsis((int)Math.Max(1, RightPanel.Width));

            UpdateDetails();
            UpdateEditorLayout();
        }

        private void UpdateDetails()
        {
            var map = MapManager.Selected.Value;
            var padding = Math.Min(Config.Details.Padding, DetailsViewport.Width / 4f);
            var textWidth = Math.Max(1, DetailsViewport.Width - padding * 2);
            var y = padding;

            SetDetailsText(Title, map?.Title ?? "", padding, ref y, textWidth);
            SetDetailsText(Artist, map?.Artist ?? "", padding, ref y, textWidth);
            SetDetailsText(Difficulty, map?.DifficultyName ?? "", padding, ref y, textWidth);
        }

        private void SetDetailsText(SpriteTextPlus text, string value, float x, ref float y, float width)
        {
            text.Text = value;
            text.TruncateWithEllipsis((int)width);
            text.Position = new ScalableVector2(x, y);
            y += text.Height + Config.Details.Gap;
        }

        private void OnLibraryChanged()
        {
            List.SetMapsets(SelectionScreen.AvailableMapsets, MapManager.Selected.Value);
            EmptyState.Visible = SelectionScreen.AvailableMapsets.Count == 0;
            UpdateDetails();
        }

        private void OnSelectedMapChanged(object sender, BindableValueChangedEventArgs<Map> args)
        {
            Container.AddScheduledUpdate(() =>
            {
                if (Destroyed)
                    return;
                List.SetSelectedMap(MapManager.Selected.Value, true);
                UpdateDetails();
            });
        }

        private void UpdateEditorLayout()
        {
            EditorRoot.Size = Container.Size;
            if (!EditorLayoutActive)
            {
                PreviewRoot.Position = new ScalableVector2(0, 0);
                PreviewRoot.Scale = Vector2.One;
                return;
            }
            const float margin = 16;
            var width = WindowManager.Width;
            var height = WindowManager.Height;
            var availableWidth = Math.Max(1, width - EditorLeftWidth - EditorRightWidth - margin * 2);
            var availableHeight = Math.Max(1, height - EditorBottomHeight - margin * 2);
            var scale = Math.Min(availableWidth / width, availableHeight / height);

            PreviewRoot.Scale = new Vector2(scale);
            PreviewRoot.Position = new ScalableVector2(
                EditorLeftWidth + margin + (availableWidth - width * scale) / 2f,
                margin + (availableHeight - height * scale) / 2f);
        }

        public override void Destroy()
        {
            Destroyed = true;
            SelectionScreen.LibraryChanged -= OnLibraryChanged;
            MapManager.Selected.ValueChanged -= OnSelectedMapChanged;

            Container.Destroy();
            Skin.Dispose();
        }
    }
}
