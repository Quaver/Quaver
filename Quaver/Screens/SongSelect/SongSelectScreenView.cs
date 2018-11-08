using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Backgrounds;
using Quaver.Online.Chat;
using Quaver.Screens.Menu;
using Quaver.Screens.Menu.UI.Navigation;
using Quaver.Screens.Menu.UI.Navigation.User;
using Quaver.Screens.Menu.UI.Visualizer;
using Quaver.Screens.SongSelect.UI;
using Quaver.Screens.SongSelect.UI.Banner;
using Quaver.Screens.SongSelect.UI.Leaderboard;
using Quaver.Screens.SongSelect.UI.Leaderboard.Selector;
using Quaver.Screens.SongSelect.UI.Maps;
using Quaver.Screens.SongSelect.UI.Mapsets;
using Quaver.Screens.SongSelect.UI.Mapsets.Search;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Screens.SongSelect
{
    public class SongSelectScreenView : ScreenView
    {
        /// <summary>
        ///     The navigation bar used to go back/open up dialogs.
        /// </summary>
        public Navbar Navbar { get; private set; }

        /// <summary>
        ///     The line on the bottom.
        /// </summary>
        public Line BottomLine { get; private set; }

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
        ///     Dictates which container (mapsets, or difficulties) are currently active.
        /// </summary>
        public SelectContainerStatus ActiveContainer { get; private set; } = SelectContainerStatus.Mapsets;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public SongSelectScreenView(Screen screen) : base(screen)
        {
            CreateNavbar();
            CreateBottomLine();
            CreateAudioVisualizer();
            CreateMapsetScrollContainer();
            CreateDifficultyScrollContainer();
            CreateMapBanner();
            CreateMapsetSearchContainer();
            CreateLeaderboardSelector();
            CreateLeaderboard();

            // Needs to be called last so it's above the entire UI
            CreateUserProfile();
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
        public override void Destroy() => Container?.Destroy();

        /// <summary>
        ///     Creates the navbar for this screen.
        /// </summary>
        private void CreateNavbar() => Navbar = new Navbar(new List<NavbarItem>
        {
            new NavbarItem("Home", false, OnHomeButtonClicked),
            new NavbarItem("Select Song", true),
            new NavbarItem("Download Maps"),
            new NavbarItem("Open Chat", false, (o, e) => ChatManager.ToggleChatOverlay(true))
        }, new List<NavbarItem>
        {
            new NavbarItemUser(this)
        }) { Parent = Container };

        /// <summary>
        ///     Called when the home button is clicked in the navbar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnHomeButtonClicked(object sender, EventArgs e) => QuaverScreenManager.ChangeScreen(new MenuScreen());

        /// <summary>
        ///     Creates the line at the bottom of the screen.
        /// </summary>
        private void CreateBottomLine()
        {
            BottomLine = new Line(Vector2.Zero, Color.LightGray, 2)
            {
                Parent = Container,
                Position = new ScalableVector2(20, WindowManager.Height - 54),
                Alpha = 0.90f
            };

            BottomLine.EndPosition = new Vector2(WindowManager.Width - BottomLine.X, BottomLine.AbsolutePosition.Y);
        }

        /// <summary>
        ///     Creates the user profile container.
        /// </summary>
        private void CreateUserProfile() => UserProfile = new UserProfileContainer(this)
        {
            Parent = Container,
            Alignment = Alignment.TopRight,
            Y = Navbar.Line.Y + Navbar.Line.Thickness,
            X = -28
        };

        /// <summary>
        ///     Creates the audio visaulizer container for the screen
        /// </summary>12
        private void CreateAudioVisualizer() => Visualizer = new MenuAudioVisualizer((int) WindowManager.Width, 400, 150, 5)
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
                Y = Navbar.Line.Y + 2,
            };

            MapsetScrollContainer.X = MapsetScrollContainer.Width;
            MapsetScrollContainer.MoveToX(-28, Easing.OutBounce, 1200);
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
                Position = new ScalableVector2(0, Navbar.Line.Y + 20),
            };

            Banner.X = -Banner.Width;
            Banner.MoveToX(28, Easing.OutQuint, 900);
        }

        /// <summary>
        ///     Creates the container that has mapset search capabilities.
        /// </summary>
        private void CreateMapsetSearchContainer() => SearchContainer = new MapsetSearchContainer(this)
        {
            Parent = Container,
            Alignment = Alignment.TopRight,
            Position = new ScalableVector2(-28, Navbar.Line.Y + 3)
        };

        /// <summary>
        ///     Creates the container that houses the leaderboard.
        /// </summary>
        private void CreateLeaderboard() => Leaderboard = new LeaderboardContainer(this)
        {
            Parent = Container,
            Position = new ScalableVector2(28 - Banner.Border.Thickness, LeaderboardSelector.Y + LeaderboardSelector.Height)
        };

        /// <summary>
        ///     Creates the interface to select between different leaderboard sections.
        /// </summary>
        private void CreateLeaderboardSelector() => LeaderboardSelector = new LeaderboardSelector(this, new List<LeaderboardSelectorItem>()
        {
            new LeaderboardSelectorItemRankings(LeaderboardType.Local, "Local Scores"),
            new LeaderboardSelectorItemRankings(LeaderboardType.Global, "Global Scores")
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
            const int targetX = -28;

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
            UserProfile.Parent = Container;
            Logger.Debug($"Switched to Select Container: {ActiveContainer}", LogType.Runtime, false);
        }
    }
}