using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Visualizer;
using Quaver.Shared.Screens.Selection.Components;
using Quaver.Shared.Screens.Selection.UI;
using Quaver.Shared.Screens.Selection.UI.FilterPanel;
using Quaver.Shared.Screens.Selection.UI.Leaderboard;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Quaver.Shared.Screens.Selection.UI.Mapsets.Maps;
using Quaver.Shared.Screens.Selection.UI.Modifiers;
using Quaver.Shared.Screens.Tests.UI.Borders;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.Selection
{
    public class SelectionScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        private SelectionScreen SelectScreen => (SelectionScreen) Screen;

        /// <summary>
        ///     Plays the audio for the song select screen
        /// </summary>
        private SelectJukebox Jukebox { get; set; }

        /// <summary>
        /// </summary>
        private BackgroundImage Background { get; set; }

        /// <summary>
        /// </summary>
        private MenuBorder Header { get; set; }

        /// <summary>
        /// </summary>
        private MenuBorder Footer { get; set; }

        /// <summary>
        /// </summary>
        private MenuAudioVisualizer Visualizer { get; set; }

        /// <summary>
        /// </summary>
        private SelectFilterPanel FilterPanel { get; set; }

        /// <summary>
        /// </summary>
        private LeaderboardContainer LeaderboardContainer { get; set; }

        /// <summary>
        /// </summary>
        private ModifierSelectorContainer ModifierSelector { get; set; }

        /// <summary>
        /// </summary>
        private MapsetScrollContainer MapsetContainer { get; set; }

        /// <summary>
        /// </summary>
        private MapScrollContainer MapContainer { get; set; }

        /// <summary>
        ///     The position of the active panel on the left
        /// </summary>
        private const int ScreenPaddingX = 50;

        /// <summary>
        ///     The amount of y-axis space between <see cref="FilterPanel"/> and the left panel
        /// </summary>
        private const int LeftPanelSpacingY = 24;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public SelectionScreenView(SelectionScreen screen) : base(screen)
        {
            CreateJukebox();
            CreateBackground();
            CreateHeader();
            CreateFooter();
            CreateAudioVisualizer();
            CreateFilterPanel();
            CreateMapsetContainer();
            CreateMapContainer();
            ReorderContainerLayerDepth();
            CreateLeaderboardContainer();
            CreateModifierSelectorContainer();

            SelectScreen.ActiveLeftPanel.ValueChanged += OnActiveLeftPanelChanged;
            SelectScreen.AvailableMapsets.ValueChanged += OnAvailableMapsetsChanged;
            SelectScreen.ActiveScrollContainer.ValueChanged += OnActiveScrollContainerChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#2F2F2F"));
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Container?.Destroy();

            // ReSharper disable once DelegateSubtraction
            SelectScreen.ActiveLeftPanel.ValueChanged -= OnActiveLeftPanelChanged;
        }

        /// <summary>
        ///     Creates <see cref="Jukebox"/>
        /// </summary>
        private void CreateJukebox() => Jukebox = new SelectJukebox { Parent = Container };

        /// <summary>
        ///     Creates <see cref="Background"/>
        /// </summary>
        private void CreateBackground()
            => Background = new BackgroundImage(UserInterface.Triangles, 0, false) { Parent = Container };

        /// <summary>
        ///     Creates <see cref="Header"/>
        /// </summary>
        private void CreateHeader() => Header = new TestMenuBorderHeader { Parent = Container };

        /// <summary>
        ///     Creates <see cref="Footer"/>
        /// </summary>
        private void CreateFooter() => Footer = new TestMenuBorderFooter
        {
            Parent = Container,
            Alignment = Alignment.BotLeft
        };

        /// <summary>
        ///     Creates <see cref="Visualizer"/>
        /// </summary>
        private void CreateAudioVisualizer()
        {
            Visualizer = new MenuAudioVisualizer((int) WindowManager.Width, 600, 65, 6)
            {
                Parent = Container,
                Alignment = Alignment.BotLeft,
                Y = -Footer.Height
            };

            Visualizer.Bars.ForEach(x => x.Alpha = 0.25f);
        }

        /// <summary>
        ///     Creates <see cref="FilterPanel"/>
        /// </summary>
        private void CreateFilterPanel() => FilterPanel = new SelectFilterPanel(SelectScreen.AvailableMapsets, SelectScreen.CurrentSearchQuery)
        {
            Parent = Container,
            Y = Header.Height + Header.ForegroundLine.Height
        };

        /// <summary>
        ///     Creates <see cref="LeaderboardContainer"/>
        /// </summary>
        private void CreateLeaderboardContainer()
        {
            LeaderboardContainer = new LeaderboardContainer
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                X = ScreenPaddingX,
                Y = FilterPanel.Y + FilterPanel.Height + LeftPanelSpacingY
            };

            LeaderboardContainer.X = -LeaderboardContainer.Width - ScreenPaddingX;
            LeaderboardContainer.MoveToX(ScreenPaddingX, Easing.OutQuint, 1000);
        }

        /// <summary>
        ///     Creates <see cref="ModifierSelector"/>
        /// </summary>
        private void CreateModifierSelectorContainer()
        {
            ModifierSelector = new ModifierSelectorContainer
            {
                Parent = Container,
                Y = LeaderboardContainer.Y
            };

            ModifierSelector.X = -ModifierSelector.Width - ScreenPaddingX;
        }

        /// <summary>
        ///     Creates <see cref="MapsetContainer"/>
        /// </summary>
        private void CreateMapsetContainer()
        {
            MapsetContainer  = new MapsetScrollContainer(SelectScreen.AvailableMapsets, SelectScreen.ActiveScrollContainer)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Y = FilterPanel.Y + FilterPanel.Height,
            };

            MapsetContainer.X = MapsetContainer.Width + ScreenPaddingX;
            MapsetContainer.MoveToX(-ScreenPaddingX, Easing.OutQuint, 1200);
        }

        /// <summary>
        ///     Creates <see cref="MapContainer"/>
        /// </summary>
        private void CreateMapContainer()
        {
            MapContainer = new MapScrollContainer(MapManager.Selected.Value?.Mapset?.Maps, SelectScreen.ActiveScrollContainer)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Y = MapsetContainer.Y,
            };

            MapContainer.X = MapContainer.Width + ScreenPaddingX;
        }

        /// <summary>
        ///     Handles animations when the active left panel has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnActiveLeftPanelChanged(object sender, BindableValueChangedEventArgs<SelectContainerPanel> e)
        {
            LeaderboardContainer.ClearAnimations();
            ModifierSelector.ClearAnimations();

            const int animTime = 400;
            const Easing easing = Easing.OutQuint;
            var inactivePos = -LeaderboardContainer.Width - ScreenPaddingX;

            switch (e.Value)
            {
                case SelectContainerPanel.Leaderboard:
                    LeaderboardContainer.MoveToX(ScreenPaddingX, easing, animTime);
                    ModifierSelector.MoveToX(inactivePos, easing, animTime);
                    break;
                case SelectContainerPanel.Modifiers:
                    LeaderboardContainer.MoveToX(inactivePos, easing, animTime);
                    ModifierSelector.MoveToX(ScreenPaddingX, easing, animTime);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Called when new available mapsets are changed.
        ///     This will effectively perform an animation to bring forward the mapset container
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAvailableMapsetsChanged(object sender, BindableValueChangedEventArgs<List<Mapset>> e)
        {
            SelectScreen.ActiveScrollContainer.Value = SelectScrollContainerType.Mapsets;

            MapsetContainer.ClearAnimations();

            const int animTime = 500;
            const int waitTime = 50;

            MapsetContainer.MoveToX(MapsetContainer.Width + ScreenPaddingX, Easing.OutQuint, animTime);
            MapsetContainer.Wait(waitTime);
            MapsetContainer.MoveToX(-ScreenPaddingX, Easing.OutQuint, animTime);
        }

        /// <summary>
        ///     Handles bringing the correct scroll container forward
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void OnActiveScrollContainerChanged(object sender, BindableValueChangedEventArgs<SelectScrollContainerType> e)
        {
            var inactivePosition = MapsetContainer.Width + ScreenPaddingX;
            const int activePosition = -ScreenPaddingX;
            const int animTime = 450;
            const Easing easing = Easing.OutQuint;

            MapsetContainer.ClearAnimations();
            MapContainer.ClearAnimations();

            switch (e.Value)
            {
                case SelectScrollContainerType.Mapsets:
                    MapsetContainer.MoveToX(activePosition, easing, animTime);
                    MapContainer.MoveToX(inactivePosition, easing, animTime);
                    break;
                case SelectScrollContainerType.Maps:
                    MapsetContainer.MoveToX(inactivePosition, easing, animTime);
                    MapContainer.MoveToX(activePosition, easing, animTime);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Makes sure the Menu headers/footers display on top of the scroll container
        ///     and that the correct buttons are clickable based on depth
        /// </summary>
        private void ReorderContainerLayerDepth()
        {
            ListHelper.Swap(Container.Children, Container.Children.IndexOf(MapContainer), Container.Children.IndexOf(FilterPanel));

            Header.Parent = Container;
            Footer.Parent = Container;
        }
    }
}