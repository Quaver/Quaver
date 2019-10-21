using System;
using System.Collections.Generic;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Editor;
using Quaver.Shared.Screens.Music.UI.Sidebar.Playlists;
using Quaver.Shared.Screens.Selection;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Music.UI.Sidebar
{
    public class MusicPlayerSidebar : Sprite
    {
        /// <summary>
        /// </summary>
        private MusicPlayerSidebarHeader RecentlyPlayedHeader { get; set; }

        /// <summary>
        /// </summary>
        private Sprite RecentlyPlayedContainer { get; set; }

        /// <summary>
        /// </summary>
        private MusicPlayerSidebarHeader ExploreHeader { get; set; }

        /// <summary>
        /// </summary>
        private Sprite ExploreContainer { get; set; }

        /// <summary>
        /// </summary>
        private MusicPlayerSidebarHeader PlaylistHeader { get; set; }

        /// <summary>
        /// </summary>
        private SidebarPlaylistContainer PlaylistContainer { get; set; }

        /// <summary>
        /// </summary>
        private IconTextButton DownloadSongs { get; set; }

        /// <summary>
        /// </summary>
        private IconTextButton OnlineMapPools { get; set; }

        /// <summary>
        /// </summary>
        private IconTextButton SongSelect { get; set; }

        /// <summary>
        /// </summary>
        private IconTextButton MapEditor { get; set; }

        /// <summary>
        /// </summary>
        private List<RecentlyPlayedSong> RecentlyPlayedSongs { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public MusicPlayerSidebar(ScalableVector2 size)
        {
            Size = size;
            Tint = ColorHelper.HexToColor("#202020");

            // Recently Played
            CreateRecentlyPlayedHeader();
            CreateRecentlyPlayedSongs();

            // Explore
            CreateExploreHeader();
            CreateDownloadSongsButton();
            CreateOnlineMapPoolsButton();
            CreateSongSelectButton();
            CreateMapEditorButton();

            // Playlists
            CreatePlaylistHeader();

            MapManager.Selected.ValueChanged += OnMapChanged;
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateRecentlyPlayedHeader()
        {
            RecentlyPlayedHeader = new MusicPlayerSidebarHeader(Width, FontAwesome.Get(FontAwesomeIcon.fa_time), "RECENTLY PLAYED")
            {
                Parent = this,
            };

            RecentlyPlayedContainer = new Sprite()
            {
                Parent = this,
                Y = RecentlyPlayedHeader.Height,
                Size = new ScalableVector2(Width, 228),
                Alpha = 0
            };
        }

        /// <summary>
        /// </summary>
        private void CreateRecentlyPlayedSongs()
        {
            RecentlyPlayedSongs = new List<RecentlyPlayedSong>();

            for (var i = 0; i < 5; i++)
            {
                var song = new RecentlyPlayedSong(null)
                {
                    Parent = RecentlyPlayedContainer,
                    X = 16,
                    Y = 20 + i * 42
                };

                if (i < MapManager.RecentlyPlayed.Count)
                    song.SetMap(MapManager.RecentlyPlayed[MapManager.RecentlyPlayed.Count - i - 1]);

                RecentlyPlayedSongs.Add(song);
            }
        }

        /// <summary>
        /// </summary>
        private void CreateExploreHeader()
        {
            ExploreHeader = new MusicPlayerSidebarHeader(Width, FontAwesome.Get(FontAwesomeIcon.fa_magnifying_glass), "EXPLORE")
            {
                Parent = this,
                Y = RecentlyPlayedContainer.Y + RecentlyPlayedContainer.Height,
            };

            ExploreContainer = new Sprite
            {
                Parent = this,
                Y = ExploreHeader.Y + ExploreHeader.Height,
                Size = new ScalableVector2(Width, 224),
                Alpha = 0
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDownloadSongsButton()
        {
            DownloadSongs = new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_download_to_storage_drive),
                FontManager.GetWobbleFont(Fonts.LatoBlack), "Download Songs", (sender, args) =>
                {
                    if (!OnlineManager.Connected)
                    {
                        NotificationManager.Show(NotificationLevel.Error, "You must be logged in to download songs!");
                        return;
                    }

                    var game = (QuaverGame) GameBase.Game;
                    game.CurrentScreen.Exit(() => new DownloadScreen());
                })
            {
                Parent = ExploreContainer,
                X = 14,
                Y = 32,
                Text =
                {
                    FontSize = 22,
                    Text = "Download Songs"
                }
            };
        }

        /// <summary>
        /// </summary>
        private void CreateOnlineMapPoolsButton()
        {
            OnlineMapPools = new IconTextButton(UserInterface.JukeboxHamburgerIcon,
                FontManager.GetWobbleFont(Fonts.LatoBlack), "Online Map Pools", (sender, args) =>
                {
                    BrowserHelper.OpenURL($"https://quavergame.com/mappools/");
                })
            {
                Parent = ExploreContainer,
                X = DownloadSongs.X,
                Y = DownloadSongs.Y + DownloadSongs.Height + DownloadSongs.Y,
                Text =
                {
                    FontSize = 22,
                    Text = "Online Map Pools"
                }
            };
        }

        /// <summary>
        /// </summary>
        private void CreateSongSelectButton()
        {
            SongSelect = new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_play_button),
                FontManager.GetWobbleFont(Fonts.LatoBlack), "Song Select", (sender, args) =>
                {
                    var game = (QuaverGame) GameBase.Game;
                    game.CurrentScreen.Exit(() => new SelectionScreen());
                })
            {
                Parent = ExploreContainer,
                X = DownloadSongs.X,
                Y = OnlineMapPools.Y + OnlineMapPools.Height + DownloadSongs.Y,
                Text =
                {
                    FontSize = 22,
                    Text = "Song Select"
                }
            };
        }

        /// <summary>
        /// </summary>
        private void CreateMapEditorButton()
        {
            MapEditor = new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_pencil),
                FontManager.GetWobbleFont(Fonts.LatoBlack), "Map Editor", (sender, args) =>
                {
                    if (MapManager.Selected.Value == null)
                    {
                        NotificationManager.Show(NotificationLevel.Error, "You cannot use the editor, as no map is selected!");
                        return;
                    }

                    if (AudioEngine.Track != null && AudioEngine.Track.IsPlaying)
                        AudioEngine.Track.Stop();

                    if (OnlineManager.CurrentGame != null)
                    {
                        NotificationManager.Show(NotificationLevel.Error, "You cannot use the editor while playing multiplayer.");
                        return;
                    }

                    var game = (QuaverGame) GameBase.Game;
                    game.CurrentScreen.Exit(() => new EditorScreen(MapManager.Selected.Value.LoadQua()));
                })
            {
                Parent = ExploreContainer,
                X = DownloadSongs.X,
                Y = SongSelect.Y + SongSelect.Height + DownloadSongs.Y,
                Text =
                {
                    FontSize = 22,
                    Text = "Map Editor"
                }
            };
        }

        /// <summary>
        /// </summary>
        private void CreatePlaylistHeader()
        {
            PlaylistHeader = new MusicPlayerSidebarHeader(Width, UserInterface.JukeboxHamburgerIcon, "PLAYLISTS")
            {
                Parent = this,
                Y = ExploreContainer.Y + ExploreContainer.Height
            };

            PlaylistContainer = new SidebarPlaylistContainer(new ScalableVector2(Width,
                Height - PlaylistHeader.Y - PlaylistHeader.Height))
            {
                Parent = this,
                Y = PlaylistHeader.Y + PlaylistHeader.Height
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            for (var i = 0; i < RecentlyPlayedSongs.Count; i++)
            {
                var song = RecentlyPlayedSongs[i];

                if (i < MapManager.RecentlyPlayed.Count)
                    song.SetMap(MapManager.RecentlyPlayed[MapManager.RecentlyPlayed.Count - i - 1]);
            }
        }
    }
}