using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Multi.UI.Chat;
using Quaver.Shared.Screens.Multi.UI.Footer;
using Quaver.Shared.Screens.Multi.UI.Players;
using Quaver.Shared.Screens.Multi.UI.Settings;
using Quaver.Shared.Screens.Multi.UI.Status;
using Quaver.Shared.Screens.Selection.Components;
using Quaver.Shared.Screens.Selection.UI;
using Quaver.Shared.Screens.Selection.UI.Leaderboard;
using Quaver.Shared.Screens.Selection.UI.Modifiers;
using Quaver.Shared.Screens.Tests.UI.Borders;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Multi
{
    public class MultiplayerGameScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        private MultiplayerGameScreen GameScreen => (MultiplayerGameScreen) Screen;

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
        private MultiplayerGameStatusPanel StatusPanel { get; set; }

        /// <summary>
        /// </summary>
        private MultiplayerMatchSettings MatchSettings { get; set; }

        /// <summary>
        /// </summary>
        private LeaderboardContainer Leaderboard { get; set; }

        /// <summary>
        /// </summary>
        private ModifierSelectorContainer Modifiers { get; set; }

        /// <summary>
        /// </summary>
        private MultiplayerPlayerList PlayerList { get; set; }

        /// <summary>
        /// </summary>
        private MultiplayerChatBox Chat { get; set; }

        /// <summary>
        /// </summary>
        public float ScreenPaddingX { get; } = 50;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public MultiplayerGameScreenView(Screen screen) : base(screen)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new SelectJukebox(GameScreen, true) {Parent = Container};

            CreateBackground();
            CreateHeader();
            CreateFooter();
            CreateStatusPanel();
            CreateMatchSettings();
            CreateLeaderboard();
            CreatePlayerList();
            CreateChat();
            CreateModifiers();

            Header.Parent = Container;
            Footer.Parent = Container;

            GameScreen.ActiveLeftPanel.ValueChanged += OnActiveLeftPanelChanged;
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
        public override void Destroy() => Container?.Destroy();

        /// <summary>
        /// </summary>
        private void CreateBackground() => Background = new BackgroundImage(UserInterface.Triangles, 0, false)
        {
            Parent = Container
        };

        /// <summary>
        /// </summary>
        private void CreateHeader() => Header = new MenuHeaderMain {Parent = Container};

        /// <summary>
        /// </summary>
        private void CreateFooter() => Footer = new MultiplayerGameFooter(GameScreen, GameScreen.Game)
        {
            Parent = Container,
            Alignment = Alignment.BotLeft
        };

        /// <summary>
        /// </summary>
        private void CreateStatusPanel() => StatusPanel = new MultiplayerGameStatusPanel(GameScreen.Game)
        {
            Parent = Container,
            Y = Header.Y + Header.Height
        };

        /// <summary>
        /// </summary>
        private void CreateMatchSettings() => MatchSettings = new MultiplayerMatchSettings(GameScreen.Game)
        {
            Parent = Container,
            Y = StatusPanel.Y + StatusPanel.Height + 20,
            X = 50
        };

        /// <summary>
        /// </summary>
        private void CreateLeaderboard()
        {
            Leaderboard = new LeaderboardContainer
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                Y = MatchSettings.Y
            };

            Leaderboard.X = -Leaderboard.Width - ScreenPaddingX;
        }

        /// <summary>
        /// </summary>
        private void CreateModifiers()
        {
            Modifiers = new ModifierSelectorContainer(GameScreen.ActiveLeftPanel)
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                Y = MatchSettings.Y
            };

            Modifiers.X = -Modifiers.Width - ScreenPaddingX;
        }

        /// <summary>
        /// </summary>
        private void CreatePlayerList()
        {
            PlayerList = new MultiplayerPlayerList(GameScreen.Game)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Y = MatchSettings.Y,
                X = -MatchSettings.X
            };
        }

        /// <summary>
        /// </summary>
        private void CreateChat()
        {
            Chat = new MultiplayerChatBox(GameScreen.Game)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Y = PlayerList.Y + PlayerList.Height + 20,
                X = -MatchSettings.X
            };
        }

        /// <summary>
        ///     Called when the active left panel is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnActiveLeftPanelChanged(object sender, BindableValueChangedEventArgs<SelectContainerPanel> e)
        {
            Leaderboard.ClearAnimations();
            MatchSettings.ClearAnimations();
            Modifiers.ClearAnimations();

            const int animTime = 400;
            const Easing easing = Easing.OutQuint;
            var inactivePos = -Leaderboard.Width - ScreenPaddingX;

            switch (e.Value)
            {
                case SelectContainerPanel.Leaderboard:
                    Leaderboard.MoveToX(ScreenPaddingX, easing, animTime);
                    MatchSettings.MoveToX(inactivePos, easing, animTime);
                    Modifiers.MoveToX(inactivePos, easing, animTime);
                    break;
                case SelectContainerPanel.MatchSettings:
                    MatchSettings.MoveToX(ScreenPaddingX, easing, animTime);
                    Leaderboard.MoveToX(inactivePos, easing, animTime);
                    Modifiers.MoveToX(inactivePos, easing, animTime);
                    break;
                case SelectContainerPanel.Modifiers:
                    Modifiers.MoveToX(ScreenPaddingX, easing, animTime);
                    MatchSettings.MoveToX(inactivePos, easing, animTime);
                    Leaderboard.MoveToX(inactivePos, easing, animTime);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}