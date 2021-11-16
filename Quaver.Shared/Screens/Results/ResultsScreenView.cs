using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Results.UI;
using Quaver.Shared.Screens.Results.UI.Footer;
using Quaver.Shared.Screens.Results.UI.Header;
using Quaver.Shared.Screens.Results.UI.Header.Contents.Tabs;
using Quaver.Shared.Screens.Results.UI.Tabs;
using Quaver.Shared.Screens.Results.UI.Tabs.Multiplayer;
using Quaver.Shared.Screens.Results.UI.Tabs.Overview;
using Quaver.Shared.Screens.Tests.UI.Borders;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Results
{
    public class ResultsScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        private ResultsScreen ResultsScreen => (ResultsScreen) Screen;

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
        private ResultsScreenHeader ScreenHeader { get; set; }

        /// <summary>
        /// </summary>
        private ResultsOverviewTab OverviewTab { get; set; }

        /// <summary>
        /// </summary>
        private ResultsMultiplayerTab MultiplayerTab { get; set; }

        /// <summary>
        ///     The width of the screen content
        /// </summary>
        public static int CONTENT_WIDTH { get; } = 1692;

        /// <summary>
        /// </summary>
        private Dictionary<ResultsScreenTabType, ResultsTabContainer> TabContainers { get; } = new Dictionary<ResultsScreenTabType, ResultsTabContainer>();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public ResultsScreenView(Screen screen) : base(screen)
        {
            CreateBackground();
            CreateHeader();
            CreateFooter();
            CreateScreenHeader();
            CreateOverviewTab();

            if (ResultsScreen.MultiplayerGame != null)
            {
                CreateMultiplayerTab();
                ResultsScreen.ActiveTab.Value = ResultsScreenTabType.Multiplayer;
                SnapTabPositions();
            }

            Header.Parent = Container;
            Footer.Parent = Container;


            ResultsScreen.Processor.ValueChanged += OnProcessorValueChanged;
            ResultsScreen.ActiveTab.ValueChanged += OnActiveTabChanged;
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
            // ReSharper disable twice DelegateSubtraction
            ResultsScreen.Processor.ValueChanged -= OnProcessorValueChanged;
            ResultsScreen.ActiveTab.ValueChanged -= OnActiveTabChanged;

            Container?.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateBackground() => Background = new BackgroundImage(SkinManager.Skin.Results.ResultsBackgroundType == ResultsBackgroundType.Background ?
           BackgroundHelper.RawTexture : SkinManager.Skin?.Results?.ResultsBackground ?? UserInterface.Triangles, 0,false)
        {
            Parent = Container,
            Y = 0,
            X = 0
        };

        /// <summary>
        /// </summary>
        private void CreateHeader() => Header = new MenuHeaderMain {Parent = Container};

        /// <summary>
        /// </summary>
        private void CreateFooter() => Footer = new ResultsScreenFooter(ResultsScreen)
        {
            Parent = Container,
            Alignment = Alignment.BotLeft
        };

        /// <summary>
        /// </summary>
        private void CreateScreenHeader() => ScreenHeader = new ResultsScreenHeader(ResultsScreen.Map, ResultsScreen.Processor,
            ResultsScreen.ActiveTab)
        {
            Parent = Container,
            Alignment = Alignment.TopLeft,
            Y = Header.Height
        };

        /// <summary>
        /// </summary>
        private void CreateOverviewTab()
        {
            OverviewTab = new ResultsOverviewTab(ResultsScreen.Map, ResultsScreen.Processor,
                ResultsScreen.ActiveTab, ResultsScreen.IsSubmittingScore, ResultsScreen.ScoreSubmissionStats)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = ScreenHeader.Y + ScreenHeader.Height + ResultsTabContainer.PADDING_Y / 2f + 4,
            };

            TabContainers[ResultsScreenTabType.Overview] = OverviewTab;
        }

        /// <summary>
        /// </summary>
        private void CreateMultiplayerTab()
        {
            MultiplayerTab = new ResultsMultiplayerTab(ResultsScreen.Map, ResultsScreen.Processor,
                ResultsScreen.ActiveTab, ResultsScreen.MultiplayerGame, ResultsScreen.MultiplayerTeam1Users, ResultsScreen.MultiplayerTeam2Users)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = OverviewTab.Y
            };

            MultiplayerTab.X = -Container.Width - 50;
            TabContainers[ResultsScreenTabType.Multiplayer] = MultiplayerTab;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProcessorValueChanged(object sender, BindableValueChangedEventArgs<ScoreProcessor> e)
        {
            Container.ScheduleUpdate(() =>
            {
                OverviewTab.Destroy();
                CreateOverviewTab();
                SnapTabPositions();
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnActiveTabChanged(object sender, BindableValueChangedEventArgs<ResultsScreenTabType> e)
        {
            const int animTime = 500;

            foreach (var tab in TabContainers)
            {
                var container = tab.Value;

                container.ClearAnimations();

                if (tab.Key == e.Value)
                    container.MoveToX(0, Easing.OutQuint, animTime);
                else
                    container.MoveToX(-Container.Width - 50, Easing.OutQuint, animTime);
            }
        }

        /// <summary>
        /// </summary>
        private void SnapTabPositions()
        {
            foreach (var tab in TabContainers)
            {
                var container = tab.Value;

                container.ClearAnimations();
                container.X = tab.Key == ResultsScreen.ActiveTab.Value ? 0 : -Container.Width - 50;
            }
        }
    }
}