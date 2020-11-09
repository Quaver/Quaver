using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Database.Profiles;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Menu.UI.Visualizer;
using Quaver.Shared.Screens.Selection.Components;
using Quaver.Shared.Screens.Selection.UI;
using Quaver.Shared.Screens.Selection.UI.Background;
using Quaver.Shared.Screens.Selection.UI.Borders.Footer;
using Quaver.Shared.Screens.Selection.UI.FilterPanel;
using Quaver.Shared.Screens.Selection.UI.Leaderboard;
using Quaver.Shared.Screens.Selection.UI.Maps;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Quaver.Shared.Screens.Selection.UI.Modifiers;
using Quaver.Shared.Screens.Selection.UI.Playlists;
using Quaver.Shared.Screens.Selection.UI.Playlists.Dialogs.Create;
using Quaver.Shared.Screens.Selection.UI.Preview;
using Quaver.Shared.Screens.Selection.UI.Profile;
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
        public MapsetScrollContainer MapsetContainer { get; private set; }

        /// <summary>
        /// </summary>
        public MapScrollContainer MapContainer { get; private set; }

        /// <summary>
        /// </summary>
        private PlaylistContainer PlaylistContainer { get; set; }

        /// <summary>
        /// </summary>
        private SelectMapPreviewContainer MapPreviewContainer { get; set; }

        /// <summary>
        /// </summary>
        private LocalProfileContainer ProfileContainer { get; set; }

        /// <summary>
        ///     The position of the active panel on the left
        /// </summary>
        private const int ScreenPaddingX = 50;

        /// <summary>
        ///     The amount of y-axis space between <see cref="FilterPanel"/> and the left panel
        /// </summary>
        private const int LeftPanelSpacingY = 20;

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
            CreateMapPreviewContainer();
            CreatePlaylistContainer();
            ReorderContainerLayerDepth();
            CreateLeaderboardContainer();
            CreateModifierSelectorContainer();
            CreateUserProfileContainer();

            SelectScreen.ActiveLeftPanel.ValueChanged += OnActiveLeftPanelChanged;
            SelectScreen.AvailableMapsets.ValueChanged += OnAvailableMapsetsChanged;
            SelectScreen.ActiveScrollContainer.ValueChanged += OnActiveScrollContainerChanged;
            MapsetContainer.ContainerInitialized += OnMapsetContainerInitialized;
            SelectScreen.ScreenExiting += OnExiting;
            ConfigManager.SelectGroupMapsetsBy.ValueChanged += OnGroupingChanged;
            PlaylistManager.PlaylistCreated += OnPlaylistCreated;
            PlaylistManager.PlaylistDeleted += OnPlaylistDeleted;
            PlaylistManager.PlaylistSynced += OnPlaylistSynced;
            PlaylistContainer.ContainerInitialized += OnPlaylistContainerInitialized;
            FilterPanel.SearchBox.OnStoppedTyping += OnSearchingStopped;

            // Trigger a scroll container change, to bring in the correct container
            SelectScreen.ActiveScrollContainer.TriggerChange();
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

            // ReSharper disable twice DelegateSubtraction
            SelectScreen.ActiveLeftPanel.ValueChanged -= OnActiveLeftPanelChanged;
            MapsetContainer.ContainerInitialized -= OnMapsetContainerInitialized;
            ConfigManager.SelectGroupMapsetsBy.ValueChanged -= OnGroupingChanged;
            PlaylistManager.PlaylistCreated -= OnPlaylistCreated;
            PlaylistManager.PlaylistDeleted -= OnPlaylistDeleted;
            PlaylistManager.PlaylistSynced -= OnPlaylistSynced;
            PlaylistContainer.ContainerInitialized -= OnPlaylistContainerInitialized;
            SelectScreen.ScreenExiting -= OnExiting;
            FilterPanel.SearchBox.OnStoppedTyping -= OnSearchingStopped;;
        }

        /// <summary>
        ///     Creates <see cref="Jukebox"/>
        /// </summary>
        private void CreateJukebox() => Jukebox = new SelectJukebox(SelectScreen) { Parent = Container };

        /// <summary>
        ///     Creates <see cref="Background"/>
        /// </summary>
        private void CreateBackground()
            => Background = new SongSelectBackground() { Parent = Container };

        /// <summary>
        ///     Creates <see cref="Header"/>
        /// </summary>
        private void CreateHeader() => Header = new MenuHeaderMain() { Parent = Container };

        /// <summary>
        ///     Creates <see cref="Footer"/>
        /// </summary>
        private void CreateFooter() => Footer = new SelectMenuFooter(SelectScreen)
        {
            Parent = Container,
            Alignment = Alignment.BotLeft
        };

        /// <summary>
        ///     Creates <see cref="Visualizer"/>
        /// </summary>
        private void CreateAudioVisualizer()
        {
            Visualizer = new MenuAudioVisualizer((int) WindowManager.Width, 600, 65, 3, 8)
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
        private void CreateFilterPanel() => FilterPanel = new SelectFilterPanel(SelectScreen.AvailableMapsets,
            SelectScreen.CurrentSearchQuery, SelectScreen.IsPlayTestingInPreview)
        {
            Parent = Container,
            Y = Header.Height + Header.ForegroundLine.Height - 2
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
            LeaderboardContainer.MoveToX(ScreenPaddingX, Easing.OutQuint, 500);
        }

        /// <summary>
        ///     Creates <see cref="ModifierSelector"/>
        /// </summary>
        private void CreateModifierSelectorContainer()
        {
            ModifierSelector = new ModifierSelectorContainer(SelectScreen.ActiveLeftPanel)
            {
                Parent = Container,
                Y = LeaderboardContainer.Y
            };

            ModifierSelector.X = -ModifierSelector.Width - ScreenPaddingX;
        }

        /// <summary>
        ///     Creates <see cref="MapPreviewContainer"/>
        /// </summary>
        private void CreateMapPreviewContainer()
        {
            MapPreviewContainer = new SelectMapPreviewContainer(SelectScreen.IsPlayTestingInPreview, SelectScreen.ActiveLeftPanel,
                (int) (WindowManager.Height - MenuBorder.HEIGHT * 2 - FilterPanel.Height))
            {
                Parent = Container,
                Y = FilterPanel.Y + FilterPanel.Height
            };

            MapPreviewContainer.X = -MapPreviewContainer.Width - ScreenPaddingX;
        }

        /// <summary>
        ///     Creates <see cref="ProfileContainer"/>
        /// </summary>
        private void CreateUserProfileContainer()
        {
            ProfileContainer = new LocalProfileContainer(UserProfileDatabaseCache.Selected ?? new Bindable<UserProfile>(null)
            {
                Value = new UserProfile()
            })
            {
                Parent = Container,
                Y = LeaderboardContainer.Y
            };

            ProfileContainer.X = -ProfileContainer.Width - ScreenPaddingX;
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
                Y = FilterPanel.Y + FilterPanel.Height - 4,
            };

            MapsetContainer.X = MapsetContainer.Width + ScreenPaddingX;
        }

        /// <summary>
        ///     Creates <see cref="MapContainer"/>
        /// </summary>
        private void CreateMapContainer()
        {
            MapContainer = new MapScrollContainer(SelectScreen.AvailableMapsets, MapsetContainer,
                MapManager.Selected.Value?.Mapset?.Maps, SelectScreen.ActiveScrollContainer)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Y = MapsetContainer.Y,
            };

            MapContainer.X = MapContainer.Width + ScreenPaddingX;
        }

        /// <summary>
        /// </summary>
        private void CreatePlaylistContainer()
        {
            PlaylistContainer = new PlaylistContainer(SelectScreen.ActiveScrollContainer)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Y = MapsetContainer.Y
            };

            PlaylistContainer.X = PlaylistContainer.Width + ScreenPaddingX;
            PlaylistContainer.InitializePlaylists(false);
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
                    MapPreviewContainer.MoveToX(inactivePos, easing, animTime);
                    ModifierSelector.MoveToX(inactivePos, easing, animTime);
                    ProfileContainer.MoveToX(inactivePos, easing, animTime);
                    break;
                case SelectContainerPanel.Modifiers:
                    LeaderboardContainer.MoveToX(inactivePos, easing, animTime);
                    MapPreviewContainer.MoveToX(inactivePos, easing, animTime);
                    ModifierSelector.MoveToX(ScreenPaddingX, easing, animTime);
                    ProfileContainer.MoveToX(inactivePos, easing, animTime);
                    break;
                case SelectContainerPanel.MapPreview:
                    LeaderboardContainer.MoveToX(inactivePos, easing, animTime);
                    ModifierSelector.MoveToX(inactivePos, easing, animTime);
                    MapPreviewContainer.MoveToX(ScreenPaddingX, easing, animTime);
                    ProfileContainer.MoveToX(inactivePos, easing, animTime);
                    break;
                case SelectContainerPanel.UserProfile:
                    LeaderboardContainer.MoveToX(inactivePos, easing, animTime);
                    ModifierSelector.MoveToX(inactivePos, easing, animTime);
                    MapPreviewContainer.MoveToX(inactivePos, easing, animTime);
                    ProfileContainer.MoveToX(ScreenPaddingX, easing, animTime);
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
            if (SelectScreen.ActiveScrollContainer.Value == SelectScrollContainerType.Playlists)
                return;

            SelectScreen.ActiveScrollContainer.Value = SelectScrollContainerType.Mapsets;

            MapsetContainer.ClearAnimations();

            const int animTime = 500;
            MapsetContainer.MoveToX(MapsetContainer.Width + ScreenPaddingX, Easing.OutQuint, animTime);
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

            MapContainer.ClearAnimations();
            PlaylistContainer.ClearAnimations();
            MapsetContainer.ClearAnimations();
            switch (e.Value)
            {
                case SelectScrollContainerType.Mapsets:
                    MapsetContainer.MoveToX(activePosition, easing, animTime);
                    MapContainer.MoveToX(inactivePosition, easing, animTime);
                    PlaylistContainer.MoveToX(inactivePosition, easing, animTime);
                    break;
                case SelectScrollContainerType.Maps:
                    MapContainer.MoveToX(activePosition, easing, animTime);
                    MapsetContainer.MoveToX(inactivePosition, easing, animTime);
                    PlaylistContainer.MoveToX(inactivePosition, easing, animTime);
                    break;
                case SelectScrollContainerType.Playlists:
                    if (PlaylistManager.Playlists.Count != 0)
                        PlaylistContainer.MoveToX(activePosition, easing, animTime);

                    MapsetContainer.MoveToX(inactivePosition, easing, animTime);
                    MapContainer.MoveToX(inactivePosition, easing, animTime);
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
            ListHelper.Swap(Container.Children, Container.Children.IndexOf(PlaylistContainer), Container.Children.IndexOf(FilterPanel));

            Header.Parent = Container;
            Footer.Parent = Container;
        }

        /// <summary>
        ///     Animations perform when the screen exits
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExiting(object sender, ScreenExitingEventArgs e)
        {
            LeaderboardContainer.ClearAnimations();
            ModifierSelector.ClearAnimations();
            MapContainer.ClearAnimations();
            MapsetContainer.ClearAnimations();
            PlaylistContainer.ClearAnimations();
            MapPreviewContainer.ClearAnimations();
            ProfileContainer.ClearAnimations();

            const Easing easing = Easing.OutQuint;
            const int time = 400;

            LeaderboardContainer.MoveToX(-LeaderboardContainer.Width - ScreenPaddingX, easing, time);
            ModifierSelector.MoveToX(-ModifierSelector.Width - ScreenPaddingX, easing, time);
            MapPreviewContainer.MoveToX(-MapPreviewContainer.Width - ScreenPaddingX, easing, time);
            ProfileContainer.MoveToX(-ProfileContainer.Width - ScreenPaddingX, easing, time);

            MapContainer.MoveToX(MapContainer.Width + ScreenPaddingX, easing, time);
            MapsetContainer.MoveToX(MapsetContainer.Width + ScreenPaddingX, easing, time);
            PlaylistContainer.MoveToX(PlaylistContainer.Width + ScreenPaddingX, easing, time);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapsetContainerInitialized(object sender, SelectContainerInitializedEventArgs e)
        {
            if (SelectScreen.ActiveScrollContainer.Value != SelectScrollContainerType.Mapsets)
                return;

            MapsetContainer.ClearAnimations();
            MapsetContainer.MoveToX(-ScreenPaddingX, Easing.OutQuint, 600);
        }

        /// <summary>
        ///     Called when the user changes their grouping setting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void OnGroupingChanged(object sender, BindableValueChangedEventArgs<GroupMapsetsBy> e)
        {
            switch (e.Value)
            {
                case GroupMapsetsBy.None:
                    // We want to completely destroy the pool to prevent the mapsets from
                    // coming in and displaying prematurely
                    if (SelectScreen.ActiveScrollContainer.Value == SelectScrollContainerType.Playlists)
                    {
                        SelectScreen.AvailableMapsets.Value = new List<Mapset>();
                        MapsetContainer.DestroyPool();
                    }

                    SelectScreen.ActiveScrollContainer.Value = SelectScrollContainerType.Mapsets;
                    break;
                case GroupMapsetsBy.Playlists:
                    SelectScreen.ActiveScrollContainer.Value = SelectScrollContainerType.Playlists;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Called when a new playlist has been created
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlaylistCreated(object sender, PlaylistCreatedEventArgs e) => ReInitializePlaylists();

        /// <summary>
        ///     Called when a playlist has been deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlaylistDeleted(object sender, PlaylistDeletedEventArgs e) => ReInitializePlaylists();

        /// <summary>
        ///     Called when a playlist has been synced to an online map pool
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlaylistSynced(object sender, PlaylistSyncedEventArgs e)
        {
            PlaylistManager.Selected.Value = e.Playlist;
            ReInitializePlaylists();
        }

        /// <summary>
        ///     Handles reinitializing the playlist container with animations
        /// </summary>
        private void ReInitializePlaylists()
        {
            switch (SelectScreen.ActiveScrollContainer.Value)
            {
                case SelectScrollContainerType.Mapsets:
                    PlaylistContainer.DestroyPool();
                    MapsetContainer.ClearAnimations();
                    MapsetContainer.MoveToX(MapsetContainer.Width + ScreenPaddingX, Easing.OutQuint, 450);

                    // Add a delay before updating these values to account for animations
                    ThreadScheduler.RunAfter(() =>
                    {
                        ConfigManager.SelectGroupMapsetsBy.Value = GroupMapsetsBy.Playlists;
                        SelectScreen.ActiveScrollContainer.Value = SelectScrollContainerType.Playlists;
                    },250);
                    break;
                case SelectScrollContainerType.Playlists:
                    PlaylistContainer.ClearAnimations();
                    PlaylistContainer.MoveToX(PlaylistContainer.Width + ScreenPaddingX, Easing.OutQuint, 450);
                    break;
            }

            PlaylistContainer.InitializePlaylists(true);
        }

        /// <summary>
        ///     Called when the playlist container has been initialized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlaylistContainerInitialized(object sender, SelectContainerInitializedEventArgs e)
        {
            if (SelectScreen.ActiveScrollContainer.Value != SelectScrollContainerType.Playlists)
                return;

            PlaylistContainer.MoveToX(-ScreenPaddingX, Easing.OutQuint, 600);
        }

        /// <summary>
        ///     Called when the user stops typing to search. Used for immediately animating
        /// </summary>
        /// <param name="obj"></param>
        private void OnSearchingStopped(string obj)
        {
            MapsetContainer.ClearAnimations();
            MapContainer.ClearAnimations();

            const Easing easing = Easing.OutQuint;
            const int time = 400;

            MapContainer.MoveToX(MapContainer.Width + ScreenPaddingX, easing, time);
            MapsetContainer.MoveToX(MapsetContainer.Width + ScreenPaddingX, easing, time);
        }
    }
}