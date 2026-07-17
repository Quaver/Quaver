using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Playlists
{
    public class PlaylistContainer : SongSelectContainer<Playlist>
    {
        /// <summary>
        /// </summary>
        public override SelectScrollContainerType Type { get; } = SelectScrollContainerType.Playlists;

        /// <summary>
        /// </summary>
        public Bindable<SelectScrollContainerType> ActiveScrollContainer { get; }

        /// <summary>
        ///     The amount of time that has elapsed since the user requested to initialize the mapsets
        /// </summary>
        private double TimeElapsedUntilInitializationRequest { get; set; } = ReinitializeTime;

        /// <summary>
        ///     The time it takes until the mapsets will reinitialize
        /// </summary>
        private const int ReinitializeTime = 250;

        /// <summary>
        ///     If the mapsets have reinitialized
        /// </summary>
        private bool HasReinitialized { get; set; }

        /// <summary>
        ///     Shows if there are no playlists
        /// </summary>
        private SpriteTextPlus NoPlaylistText { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="activeScrollContainer"></param>
        public PlaylistContainer(Bindable<SelectScrollContainerType> activeScrollContainer) : base(PlaylistManager.Playlists, 12)
        {
            ActiveScrollContainer = activeScrollContainer;
            NoPlaylistText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterHeavy), "No playlists created!", 28)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Tint = Color.White,
                Visible = PlaylistManager.Playlists.Count == 0
            };

        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => base.Destroy();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            TimeElapsedUntilInitializationRequest += gameTime.ElapsedGameTime.TotalMilliseconds;
            InitializePlaylists(false);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected override float GetSelectedPosition() => (-SelectedIndex.Value + 4) * DrawableMapset.MapsetHeight;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<Playlist> CreateObject(Playlist item, int index) => new DrawablePlaylist(this, item, index);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void HandleInput(GameTime gameTime)
        {
            if (ActiveScrollContainer.Value != SelectScrollContainerType.Playlists)
                return;

            // Move to the next mapset
            if (KeyboardManager.IsUniqueKeyPress(Keys.Right) || KeyboardManager.IsUniqueKeyPress(Keys.Down))
            {
                if (SelectedIndex.Value + 1 >= AvailableItems.Count)
                    return;

                PlaylistManager.Selected.Value = AvailableItems[SelectedIndex.Value + 1];
                SelectedIndex.Value++;

                ScrollToSelected();
            }
            // Move to the previous mapset
            else if (KeyboardManager.IsUniqueKeyPress(Keys.Left) || KeyboardManager.IsUniqueKeyPress(Keys.Up))
            {
                if (SelectedIndex.Value - 1 < 0)
                    return;

                PlaylistManager.Selected.Value = AvailableItems[SelectedIndex.Value - 1];

                SelectedIndex.Value--;
                ScrollToSelected();
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void SetSelectedIndex()
        {
            SelectedIndex.Value = AvailableItems.FindIndex(x => x == PlaylistManager.Selected.Value);

            var oldValue = SelectedIndex.Value;

            if (SelectedIndex.Value == -1)
                SelectedIndex.Value = 0;

            // Manually trigger the change event in the case that the selected index is still the same value.
            // Other components that rely on the bindable will want to update their state in case
            // the mapset has changed in any way.
            if (oldValue == SelectedIndex.Value)
                SelectedIndex.TriggerChangeEvent();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public void InitializePlaylists(bool restart)
        {
            if (restart)
            {
                TimeElapsedUntilInitializationRequest = 0;
                HasReinitialized = false;
                return;
            }

            if (TimeElapsedUntilInitializationRequest < ReinitializeTime || HasReinitialized)
                return;

            lock (Pool)
            {
                DestroyAndClearPool();

                AvailableItems = OrderPlaylists(PlaylistManager.Playlists);

                SetSelectedIndex();

                // Reset the starting index so we can be aware of the mapsets that are needed
                PoolStartingIndex = DesiredPoolStartingIndex(SelectedIndex.Value);

                // Recreate the object pool
                CreatePool();
                PositionAndContainPoolObjects();

                // Snap to it immediately
                SnapToSelected();

                NoPlaylistText.Visible = AvailableItems.Count == 0;
            }

            HasReinitialized = true;
            FireInitializedEvent();
        }

        /// <summary>
        ///     Restarts and immediately initializes the playlists.
        /// </summary>
        public void InitializePlaylistsImmediately()
        {
            TimeElapsedUntilInitializationRequest = ReinitializeTime;
            HasReinitialized = false;
            InitializePlaylists(false);
        }

        /// <summary>
        ///     Orders playlists by the song select sort setting when playlists are the visible list.
        /// </summary>
        /// <param name="playlists"></param>
        /// <returns></returns>
        private static List<Playlist> OrderPlaylists(IEnumerable<Playlist> playlists)
        {
            switch (ConfigManager.SelectOrderPlaylistsBy?.Value)
            {
                case OrderPlaylistsBy.Creator:
                    return playlists.OrderBy(x => x.Creator).ThenBy(x => x.Name).ToList();
                case OrderPlaylistsBy.DateAdded:
                    return playlists.OrderByDescending(x => x.Id).ThenBy(x => x.Name).ToList();
                case OrderPlaylistsBy.Game:
                    return playlists.OrderBy(x => x.PlaylistGame).ThenBy(x => x.Name).ToList();
                case OrderPlaylistsBy.Difficulty:
                    return playlists.OrderBy(GetPlaylistMinDifficulty).ThenBy(x => x.Name).ToList();
                case OrderPlaylistsBy.Length:
                    return playlists.OrderBy(x => x.Maps.Count).ThenBy(x => x.Name).ToList();
                case OrderPlaylistsBy.Title:
                default:
                    return playlists.OrderBy(x => x.Name).ToList();
            }
        }

        /// <summary>
        ///     Gets the minimum difficulty shown for a playlist.
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        private static double GetPlaylistMinDifficulty(Playlist playlist)
        {
            if (playlist.Maps.Count == 0)
                return 0;

            return playlist.Maps.Min(map => playlist.IsTournament()
                ? playlist.GetMapDifficulty(map)
                : map.DifficultyFromMods(ModManager.Mods));
        }

    }
}
