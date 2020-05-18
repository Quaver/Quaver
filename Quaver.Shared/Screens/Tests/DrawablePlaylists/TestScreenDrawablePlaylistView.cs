using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Selection.UI.Playlists;
using Wobble;
using Wobble.Graphics;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.DrawablePlaylists
{
    public class TestScreenDrawablePlaylistView : ScreenView
    {
        public TestScreenDrawablePlaylistView(Screen screen) : base(screen)
        {
            var mapset = new Mapset()
            {
                Maps = new List<Map>()
                {
                    new Map()
                    {
                        Artist = "Swan",
                        Title = "Left Right",
                        Creator = "Test",
                        DifficultyName = "Offset Calibrator",
                        Md5Checksum = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                        Difficulty10X = 12.34,
                        Mode = GameMode.Keys4,
                        RankedStatus = RankedStatus.Ranked
                    },
                    new Map()
                    {
                        Artist = "Swan",
                        Title = "Left Right",
                        Creator = "Test2",
                        DifficultyName = "Offset Calibrator",
                        Md5Checksum = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                        Difficulty10X = 42.69,
                        Mode = GameMode.Keys4,
                        RankedStatus = RankedStatus.Unranked
                    },
                }
            };

            var playlist = new Playlist()
            {
                Name = "Favorite Maps",
                Creator = "Test User",
                Description = "???",
                Maps = mapset.Maps,
                PlaylistGame = MapGame.Quaver,
            };

            var playlist2 = new Playlist()
            {
                Name = "Maps To Clear",
                Creator = "Player",
                Description = "???",
                Maps = playlist.Maps,
                PlaylistGame = MapGame.Quaver,
            };

            // ReSharper disable once ObjectCreationAsStatement
            new DrawablePlaylist(null, playlist, 0)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
            };

            // ReSharper disable once ObjectCreationAsStatement
            new DrawablePlaylist(null, new Playlist()
            {
                Name = "Maps To Clear",
                Creator = "Player",
                Maps = new List<Map>()
                {
                    new Map()
                    {
                        Mode = GameMode.Keys7,
                        Difficulty10X = 24.60,
                        RankedStatus = RankedStatus.Unranked
                    }
                }
            }, 1)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = 100
            };

            // ReSharper disable once ObjectCreationAsStatement
            new DrawablePlaylist(null, new Playlist()
            {
                Name = "Empty Playlist",
                Creator = "Player",
                Maps = new List<Map>()
            }, 1)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = 200
            };
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
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#2f2f2f"));
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}