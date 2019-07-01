/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Server.Client;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Menu;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Menu;
using Quaver.Shared.Screens.Menu.UI.Navigation;
using Quaver.Shared.Screens.Menu.UI.Navigation.User;
using Quaver.Shared.Screens.Menu.UI.Visualizer;
using Quaver.Shared.Screens.Select.UI.Banner;
using Quaver.Shared.Screens.Select.UI.Leaderboard;
using Quaver.Shared.Screens.Select.UI.Leaderboard.Selector;
using Quaver.Shared.Screens.Select.UI.Maps;
using Quaver.Shared.Screens.Select.UI.Mapsets;
using Quaver.Shared.Screens.Select.UI.Modifiers;
using Quaver.Shared.Screens.Select.UI.Search;
using Quaver.Shared.Screens.Settings;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.Select
{
    public class SelectScreenView : ScreenView
    {
        /// <summary>
        ///     The background image for the screen
        /// </summary>
        public BackgroundImage Background { get; private set; }

        /// <summary>
        ///     The navigation bar used to go back/open up dialogs.
        /// </summary>
        public Navbar Navbar { get; private set; }

        /// <summary>
        ///     The user's profile when the click on their name in the navbar.
        /// </summary>
        public UserProfileContainer UserProfile { get; private set; }

        /// <summary>
        ///     Audio visualizer at the bottom of the screen.
        /// </summary>
        public MenuAudioVisualizer Visualizer { get; private set; }

        /// <summary>
        ///     Allows scrolling for different mapsets.
        /// </summary>
        public MapsetScrollContainer MapsetScrollContainer { get; private set; }

        /// <summary>
        ///     Allows scrolling to different difficulties (maps))
        /// </summary>
        public DifficultyScrollContainer DifficultyScrollContainer { get; private set; }

        /// <summary>
        ///     The banner that displays some map information.
        /// </summary>
        public SelectMapBanner Banner { get; private set; }

        /// <summary>
        ///     Allows for searching mapsets.
        /// </summary>
        public MapsetSearchContainer SearchContainer { get; private set; }

        /// <summary>
        ///     Displays the leaderboard to show user scores.
        /// </summary>
        public LeaderboardContainer Leaderboard { get; private set; }

        /// <summary>
        ///     Allows the user to select between different leaderboard sections.
        /// </summary>
        public LeaderboardSelector LeaderboardSelector { get; private set; }

        /// <summary>
        ///     The navigation bar at the bottom
        /// </summary>
        public Navbar BottomNavbar { get; private set; }

        /// <summary>
        /// </summary>
        private MenuHeader Header { get; set; }

        /// <summary>
        /// </summary>
        private MenuFooter Footer { get; set; }

        /// <summary>
        ///     Dictates which container (mapsets, or difficulties) are currently active.
        /// </summary>
        public SelectContainerStatus ActiveContainer { get; private set; } = SelectContainerStatus.Mapsets;

        /// <summary>
        ///     The time the user last exported a map
        /// </summary>
        private long LastExportTime { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public SelectScreenView(Screen screen) : base(screen)
        {
            // CreateBackground();
            BackgroundHelper.Background.Dim = 65;

            CreateHeader();
            CreateAudioVisualizer();
            CreateMapBanner();
            CreateMapsetScrollContainer();
            CreateDifficultyScrollContainer();
            CreateMapsetSearchContainer();
            CreateLeaderboardSelector();
            CreateLeaderboard();
            CreateMenuFooter();

            var selectScreen = Screen as SelectScreen;
            selectScreen.ScreenExiting += OnScreenExiting;

            // Needs to be called last so it's above the entire UI
            //CreateUserProfile();
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
            GameBase.Game.GraphicsDevice.Clear(Color.Black);
            BackgroundHelper.Draw(gameTime);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            var screen = Screen as SelectScreen;
            screen.ScreenExiting -= OnScreenExiting;

            Container?.Destroy();
        }

        /// <summary>
        ///     Creates the audio visaulizer container for the screen
        /// </summary>12
        private void CreateAudioVisualizer() => Visualizer = new MenuAudioVisualizer((int)WindowManager.Width, 400, 150, 5)
        {
            Parent = Container,
            Alignment = Alignment.BotLeft
        };

        /// <summary>
        ///     Creates the container to scroll for different mapsets.
        /// </summary>
        private void CreateMapsetScrollContainer()
        {
            MapsetScrollContainer = new MapsetScrollContainer(this)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Y = Header.Height + 2
            };

            MapsetScrollContainer.X = MapsetScrollContainer.Width;
            MapsetScrollContainer.MoveToX(-18, Easing.OutBounce, 1200);
        }

        /// <summary>
        ///     Creates the container to scroll to different maps.
        /// </summary>
        private void CreateDifficultyScrollContainer()
        {
            DifficultyScrollContainer = new DifficultyScrollContainer(this)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Y = MapsetScrollContainer.Y
            };

            // Hide it originally.
            DifficultyScrollContainer.X = DifficultyScrollContainer.Width;
        }

        /// <summary>
        ///     Creates the sprite that displays the map banner.
        /// </summary>
        private void CreateMapBanner()
        {
            Banner = new SelectMapBanner(this)
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(0, Header.Height + 20),
            };

            Banner.X = -Banner.Width;
            Banner.MoveToX(25, Easing.OutQuint, 900);
        }

        /// <summary>
        ///     Creates the container that has mapset search capabilities.
        /// </summary>
        private void CreateMapsetSearchContainer() => SearchContainer = new MapsetSearchContainer(this)
        {
            Parent = Container,
            Alignment = Alignment.TopRight,
            Position = new ScalableVector2(580, Header.Height),
            Animations =
            {
                new Animation(AnimationProperty.X, Easing.OutBounce, 580, -18, 1200)
            }
        };

        /// <summary>
        ///     Creates the container that houses the leaderboard.
        /// </summary>
        private void CreateLeaderboard() => Leaderboard = new LeaderboardContainer(this)
        {
            Parent = Container,
            Position = new ScalableVector2(28 - Banner.Border.Thickness, LeaderboardSelector.Y + LeaderboardSelector.Height + 10)
        };

        /// <summary>
        ///     Creates the interface to select between different leaderboard sections.
        /// </summary>
        private void CreateLeaderboardSelector() => LeaderboardSelector = new LeaderboardSelector(this, new List<LeaderboardSelectorItem>()
        {
            new LeaderboardSelectorItemRankings(LeaderboardType.Local, "Local"),
            new LeaderboardSelectorItemRankings(LeaderboardType.Global, "Global"),
            new LeaderboardSelectorItemRankings(LeaderboardType.Mods, "Mods")
        })
        {
            Parent = Container
        };

        /// <summary>
        ///     Switches the UI to the specified ScrollContainer.
        /// </summary>
        /// <param name="container"></param>
        public void SwitchToContainer(SelectContainerStatus container)
        {
            if (container == ActiveContainer)
                return;

            const int time = 400;
            const int targetX = -18;

            MapsetScrollContainer.ClearAnimations();
            DifficultyScrollContainer.ClearAnimations();

            switch (container)
            {
                case SelectContainerStatus.Mapsets:
                    MapsetScrollContainer.Parent = Container;

                    MapsetScrollContainer.MoveToX(targetX, Easing.OutQuint, time);
                    DifficultyScrollContainer.MoveToX(DifficultyScrollContainer.Width, Easing.OutQuint, time);
                    break;
                case SelectContainerStatus.Difficulty:
                    DifficultyScrollContainer.Parent = Container;

                    DifficultyScrollContainer.Visible = true;
                    DifficultyScrollContainer.ContentContainer.Visible = true;

                    MapsetScrollContainer.MoveToX(MapsetScrollContainer.Width, Easing.OutQuint, time);
                    DifficultyScrollContainer.MoveToX(targetX, Easing.OutQuint, time);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(container), container, null);
            }

            ActiveContainer = container;
            SearchContainer.Parent = Container;
            Banner.Parent = Container;
            Footer.Parent = Container;
            Logger.Debug($"Switched to Select Container: {ActiveContainer}", LogType.Runtime, false);
        }

        /// <summary>
        ///     Called when the screen is exiting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScreenExiting(object sender, ScreenExitingEventArgs e)
        {
            if (Screen is SelectScreen screen && !screen.IsExitingToGameplay)
                return;

            MapsetScrollContainer.MoveToX(MapsetScrollContainer.Width, Easing.OutQuint, 400);
            DifficultyScrollContainer.MoveToX(DifficultyScrollContainer.Width, Easing.OutQuint, 400);
            SearchContainer.MoveToX(SearchContainer.Width, Easing.OutQuint, 400);
            Banner.MoveToX(-Banner.Width, Easing.OutQuint, 400);
            LeaderboardSelector.MoveToX(-LeaderboardSelector.Width, Easing.OutQuint, 400);
            Leaderboard.MoveToX(-Leaderboard.Width, Easing.OutQuint, 400);
        }

        /// <summary>
        /// </summary>
        private void CreateHeader()
        {
            Header = new MenuHeader(FontAwesome.Get(FontAwesomeIcon.fa_gamepad_console), "Song", "Select",
                "Select a map to play", Colors.MainAccent)
            {
                Parent = Container,
                Alignment = Alignment.TopLeft
            };
        }

        /// <summary>
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void CreateMenuFooter()
        {
            var screen = (SelectScreen) Screen;

            var leftButtons = new List<ButtonText>()
            {
                new ButtonText(FontsBitmap.GothamRegular, "Back", 14, (sender, args) => screen.ExitToMenu()),
                new ButtonText(FontsBitmap.GothamRegular, "Options", 14,(sender, args) => DialogManager.Show(new SettingsDialog())),
                new ButtonText(FontsBitmap.GothamRegular, "Chat", 14, (sender, args) =>
                {
                    if (OnlineManager.Status.Value != ConnectionStatus.Connected)
                    {
                        NotificationManager.Show(NotificationLevel.Error, "You must be logged in to use the chat!");
                        return;
                    }

                    ChatManager.ToggleChatOverlay(true);
                }),
                new ButtonText(FontsBitmap.GothamRegular, "Download Maps", 14, (sender, args) =>
                {
                    if (OnlineManager.Status.Value != ConnectionStatus.Connected)
                    {
                        NotificationManager.Show(NotificationLevel.Error, "You must be online to download maps!");
                        return;
                    }

                    if (OnlineManager.CurrentGame != null)
                    {
                        NotificationManager.Show(NotificationLevel.Error, "You cannot download maps while in multiplayer!");
                        return;
                    }

                    screen.Exit(() => new DownloadScreen());
                }),
                new ButtonText(FontsBitmap.GothamRegular, "Profile", 14, (sender, args) => BrowserHelper.OpenURL($"https://quavergame.com/profile/{ConfigManager.Username.Value}?mode={(int) ConfigManager.SelectedGameMode.Value}")),
            };

            var rightButtons = new List<ButtonText>()
            {
                new ButtonText(FontsBitmap.GothamRegular, "Modifiers", 14, (sender, args) => DialogManager.Show(new ModifiersDialog())),
                new ButtonText(FontsBitmap.GothamRegular, "Export", 14, (sender, args) =>
                {
                    if (Math.Abs(GameBase.Game.TimeRunning - LastExportTime) < 2000)
                    {
                        NotificationManager.Show(NotificationLevel.Error,
                            "Slow down! You can only export a set every 2 seconds.");
                        return;
                    }

                    LastExportTime = GameBase.Game.TimeRunning;

                    ThreadScheduler.Run(() =>
                    {
                        NotificationManager.Show(NotificationLevel.Info, "Exporting mapset to file...");
                        MapManager.Selected.Value.Mapset.ExportToZip();
                        NotificationManager.Show(NotificationLevel.Success, "Successfully exported mapset!");
                    });
                }),
                new ButtonText(FontsBitmap.GothamRegular, "Delete", 14, ((sender, args) =>
                {
                    if (MapManager.Selected.Value == null)
                        return;

                    screen.DeleteSelected();
                }))
            };

            if (OnlineManager.CurrentGame == null)
                rightButtons.Add(new ButtonText(FontsBitmap.GothamRegular, "Edit", 14, (sender, args) => screen.ExitToEditor()));

            rightButtons.Add(new ButtonText(FontsBitmap.GothamRegular, "Random", 14, (sender, args) => screen.SelectRandomMap()));

            Footer = new MenuFooter(leftButtons, rightButtons, Colors.MainAccent)
            {
                Parent = Container,
                Alignment = Alignment.BotLeft
            };
        }
    }
}
