using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Multiplayer.UI
{
    public class MultiplayerMap : Sprite
    {
        /// <summary>
        /// </summary>
        private MultiplayerGame Game { get; }

        /// <summary>
        /// </summary>
        private Sprite Background { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap ArtistTitle { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap DifficultyRating { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap DifficultyName { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap Mode { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap Creator { get; }

        /// <summary>
        /// </summary>
        public MultiplayerMap(MultiplayerGame game)
        {
            Game = game;
            Size = new ScalableVector2(620, 80);
            Tint = Color.Black;
            Alpha = 0.75f;

            Background = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(Height * 1.75f, Height),
                Alpha = 0
            };

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(2, Height),
                X = Background.Width,
            };

            var diffName = GetDifficultyName();

            ArtistTitle = new SpriteTextBitmap(FontsBitmap.GothamRegular, game.Map.Replace($"[{diffName}]", ""))
            {
                Parent = this,
                X = Background.X + Background.Width + 16,
                Y = 12,
                FontSize = 16
            };

            Mode = new SpriteTextBitmap(FontsBitmap.GothamRegular, "["  + ModeHelper.ToShortHand((GameMode) game.GameMode) + "]")
            {
                Parent = this,
                X = ArtistTitle.X,
                Y = ArtistTitle.Y + ArtistTitle.Height + 5,
                FontSize = 14
            };

            DifficultyRating = new SpriteTextBitmap(FontsBitmap.GothamRegular, $"{game.DifficultyRating:0.00}")
            {
                Parent = this,
                X = Mode.X + Mode.Width + 10,
                Y = Mode.Y,
                FontSize = 14,
                Tint = ColorHelper.DifficultyToColor((float) game.DifficultyRating)
            };

            DifficultyName = new SpriteTextBitmap(FontsBitmap.GothamRegular, " - \"" + diffName + "\"")
            {
                Parent = this,
                X = DifficultyRating.X + DifficultyRating.Width + 2,
                Y = Mode.Y,
                FontSize = 14,
            };

            Creator = new SpriteTextBitmap(FontsBitmap.GothamRegular, "Mods: None")
            {
                Parent = this,
                X = Mode.X,
                Y = DifficultyRating.Y + DifficultyRating.Height + 5,
                FontSize = DifficultyRating.FontSize
            };

            AddBorder(Color.White, 2);

            BackgroundHelper.Loaded += OnBackgroundLoaded;
            OnlineManager.Client.OnGameMapChanged += OnGameMapChanged;
            OnlineManager.Client.OnChangedModifiers += OnChangedModifiers;

            UpdateContent();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            BackgroundHelper.Loaded -= OnBackgroundLoaded;
            OnlineManager.Client.OnGameMapChanged -= OnGameMapChanged;
            OnlineManager.Client.OnChangedModifiers -= OnChangedModifiers;
            base.Destroy();
        }

        /// <summary>
        ///     Gets the name of the difficulty from the map
        /// </summary>
        /// <returns></returns>
        private string GetDifficultyName()
        {
            var diffName = "";
            var pattern = @"\[(.*?)\]";
            var matches = Regex.Matches(Game.Map, pattern);

            foreach (Match match in matches)
            {
                if (match != matches.Last())
                    continue;

                diffName = match.Groups[1].ToString();
            }

            return diffName;
        }

        /// <summary>
        /// </summary>
        public void UpdateContent()
        {
            var diffName = GetDifficultyName();

            ArtistTitle.Text = Game.Map.Replace($"[{diffName}]", "");
            Mode.Text = $"[{ModeHelper.ToShortHand((GameMode) Game.GameMode)}]";

            DifficultyRating.Text = $"{Game.DifficultyRating:0.00}";
            DifficultyRating.Tint = ColorHelper.DifficultyToColor((float) Game.DifficultyRating);

            DifficultyName.Text = " - \"" + diffName + "\"";

            // Find the map
            var map = MapManager.FindMapFromMd5(Game.MapMd5);
            MapManager.Selected.Value = map;

            if (map != null)
            {
                ArtistTitle.Tint = Color.White;
                Creator.Text = $"By: {map.Creator}";

                // Inform the server that we now have the map if we didn't before.
                if (OnlineManager.CurrentGame.PlayersWithoutMap.Contains(OnlineManager.Self.OnlineUser.Id))
                    OnlineManager.Client.HasMultiplayerGameMap();

                if (map != BackgroundHelper.Map)
                    Background.Alpha = 0;

                var game = (QuaverGame) GameBase.Game;

                if (game.CurrentScreen.Type == QuaverScreenType.Lobby || game.CurrentScreen.Type == QuaverScreenType.Multiplayer
                                                                      || QuaverScreenManager.QueuedScreen.Type == QuaverScreenType.Multiplayer
                                                                      || AudioEngine.Map != map)
                {
                    BackgroundHelper.Load(MapManager.Selected.Value);

                    ThreadScheduler.Run(() =>
                    {
                        try
                        {
                            if (AudioEngine.Map != map)
                            {
                                AudioEngine.LoadCurrentTrack();
                                AudioEngine.Track.Play();
                            }
                        }
                        catch (Exception e)
                        {

                        }
                    });
                }
            }
            // Let the server know that we don't have the selected map
            else
            {
                ArtistTitle.Tint = Color.Red;
                Creator.Text = "You don't have this map!";

                OnlineManager.Client.DontHaveMultiplayerGameMap();

                if (!AudioEngine.Track.IsStopped)
                    AudioEngine.Track.Stop();

                MapManager.Selected.Value = MapManager.Mapsets.First().Maps.First();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackgroundLoaded(object sender, BackgroundLoadedEventArgs e)
        {
            Background.Image = e.Texture;
            Background.FadeTo(1, Easing.Linear, 400);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameMapChanged(object sender, GameMapChangedEventArgs e)
        {
            // Make sure to clear all the players that don't have the map, as this information is
            // now outdated.
            Game.PlayersWithoutMap.Clear();

            Game.MapMd5 = e.MapMd5;
            Game.MapId = e.MapId;
            Game.MapsetId = e.MapsetId;
            Game.Map = e.Map;
            Game.DifficultyRating = e.DifficultyRating;
            Game.AllDifficultyRatings = e.AllDifficultyRatings;
            Game.GameMode = e.GameMode;

            UpdateContent();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChangedModifiers(object sender, ChangeModifiersEventArgs e)
        {
            OnlineManager.CurrentGame.DifficultyRating = e.DifficultyRating;
            OnlineManager.CurrentGame.Modifiers = e.Modifiers.ToString();
            UpdateContent();
        }
    }
}