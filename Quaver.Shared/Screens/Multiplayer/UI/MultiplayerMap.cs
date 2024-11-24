using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Server.Client.Events.Download;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Helpers;
using Quaver.Server.Client.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Importing;
using Quaver.Shared.Screens.Selection;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace Quaver.Shared.Screens.Multiplayer.UI
{
    public class MultiplayerMap : ScrollContainer
    {
        /// <summary>
        /// </summary>
        private MultiplayerScreen Screen { get; }

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
        private ImageButton DownloadButton { get; }

        /// <summary>
        ///     The current mapset download instance
        /// </summary>
        private MapsetDownload CurrentDownload { get; set; }

        /// <summary>
        /// </summary>
        public bool HasMap { get; private set; }

        /// <summary>
        /// </summary>
        public MultiplayerMap(MultiplayerScreen screen, MultiplayerGame game) : base(new ScalableVector2(682, 86), new ScalableVector2(682, 86))
        {
            Screen = screen;
            Game = game;
            Size = new ScalableVector2(650, 86);
            Image = UserInterface.MapPanel;

            DownloadButton = new ImageButton(UserInterface.BlankBox, OnDownloadButtonClicked)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(Width - 4, Height - 4),
                Alpha = 0
            };

            Background = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(Height * 1.70f, Height - 4),
                Alignment = Alignment.MidLeft,
                X = 2,
                Image = MapManager.Selected.Value == BackgroundHelper.Map && MapManager.Selected.Value.Md5Checksum == Game.MapMd5 ? BackgroundHelper.RawTexture : UserInterface.MenuBackground,
                Alpha = MapManager.Selected.Value == BackgroundHelper.Map && MapManager.Selected.Value.Md5Checksum == Game.MapMd5 ? 1 : 0
            };

            AddContainedDrawable(Background);

            var diffName = GetDifficultyName();

            ArtistTitle = new SpriteTextBitmap(FontsBitmap.GothamRegular, game.Map.Replace($"[{diffName}]", ""))
            {
                Parent = this,
                X = Background.X + Background.Width + 16,
                Y = 12,
                FontSize = 16
            };

            AddContainedDrawable(ArtistTitle);

            Mode = new SpriteTextBitmap(FontsBitmap.GothamRegular, "[" + ModeHelper.ToShortHand((GameMode)game.GameMode) + "]")
            {
                Parent = this,
                X = ArtistTitle.X,
                Y = ArtistTitle.Y + ArtistTitle.Height + 8,
                FontSize = 14
            };

            AddContainedDrawable(Mode);

            DifficultyRating = new SpriteTextBitmap(FontsBitmap.GothamRegular, $"{game.DifficultyRating:0.00}")
            {
                Parent = this,
                X = Mode.X + Mode.Width + 8,
                Y = Mode.Y,
                FontSize = 14,
                Tint = ColorHelper.DifficultyToColor((float)game.DifficultyRating)
            };

            AddContainedDrawable(DifficultyRating);

            DifficultyName = new SpriteTextBitmap(FontsBitmap.GothamRegular, " - \"" + diffName + "\"")
            {
                Parent = this,
                X = DifficultyRating.X + DifficultyRating.Width + 2,
                Y = Mode.Y,
                FontSize = 14,
            };

            AddContainedDrawable(DifficultyName);

            Creator = new SpriteTextBitmap(FontsBitmap.GothamRegular, "Mods: None")
            {
                Parent = this,
                X = Mode.X,
                Y = DifficultyRating.Y + DifficultyRating.Height + 8,
                FontSize = DifficultyRating.FontSize
            };

            AddContainedDrawable(Creator);

            BackgroundHelper.Loaded += OnBackgroundLoaded;
            OnlineManager.Client.OnGameMapChanged += OnGameMapChanged;
            OnlineManager.Client.OnChangedModifiers += OnChangedModifiers;
            OnlineManager.Client.OnGameHostSelectingMap += OnGameHostSelectingMap;
            ModManager.ModsChanged += OnModsChanged;

            BackgroundHelper.Load(MapManager.Selected.Value);
            UpdateContent();
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (!HasMap || OnlineManager.CurrentGame?.HostId == OnlineManager.Self.OnlineUser.Id)
            {
                DownloadButton.Alpha = MathHelper.Lerp(DownloadButton.Alpha, DownloadButton.IsHovered ? 0.3f : 0f,
                    (float)Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 60, 1));
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            BackgroundHelper.Loaded -= OnBackgroundLoaded;
            OnlineManager.Client.OnGameMapChanged -= OnGameMapChanged;
            OnlineManager.Client.OnChangedModifiers -= OnChangedModifiers;
            OnlineManager.Client.OnGameHostSelectingMap -= OnGameHostSelectingMap;
            ModManager.ModsChanged -= OnModsChanged;

            if (CurrentDownload != null)
            {
                // ReSharper disable twice DelegateSubtraction
                CurrentDownload.Progress.ValueChanged -= OnDownloadProgressChanged;
                CurrentDownload.Status.ValueChanged -= OnDownloadStatusChanged;
            }

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
            /*Map map;

            if (MapManager.Selected.Value?.Md5Checksum == Game.MapMd5)
                map = MapManager.Selected.Value;
            else
            {
                map = MapManager.FindMapFromMd5(Game.MapMd5);

                // In the event that we don't have the correct version, try to find the
                // alternative one. This is commonly used for situations where one has osu!
                // beatmaps auto-loaded and someone downloads and converts the file to .qua format
                if (map == null && Game.MapMd5 != Game.AlternativeMd5)
                    map = MapManager.FindMapFromMd5(Game.AlternativeMd5);

                MapManager.Selected.Value = map;
            }

            HasMap = map != null;

            if (OnlineManager.CurrentGame != null && OnlineManager.CurrentGame.HostSelectingMap)
            {
                ArtistTitle.Text = "Host is currently selecting a map!";
                Mode.Text = "Please wait...";
                Creator.Text = "";
                DifficultyName.Text = "";
                DifficultyRating.Text = "";
            }
            else
            {
                var diffName = GetDifficultyName();

                ArtistTitle.Text = Game.Map.Replace($"[{diffName}]", "");
                Mode.Text = $"[{ModeHelper.ToShortHand((GameMode) Game.GameMode)}]";
                Creator.Tint = Color.White;

                DifficultyRating.Text = map != null ? $"{map.DifficultyFromMods(ModManager.Mods):0.00}" : $"{Game.DifficultyRating:0.00}";
                DifficultyRating.Tint = ColorHelper.DifficultyToColor((float) (map?.DifficultyFromMods(ModManager.Mods) ?? Game.DifficultyRating));
                DifficultyRating.X = Mode.X + Mode.Width + 8;
                DifficultyName.X = DifficultyRating.X + DifficultyRating.Width + 2;
                DifficultyName.Text = " - \"" + diffName + "\"";
            }

            var game = (QuaverGame) GameBase.Game;

            if (map != null)
            {
                ArtistTitle.Tint = Color.White;

                var length = TimeSpan.FromMilliseconds(map.SongLength / ModHelper.GetRateFromMods(ModManager.Mods));
                var time = length.Hours > 0 ? length.ToString(@"hh\:mm\:ss") : length.ToString(@"mm\:ss");

                if (OnlineManager.CurrentGame != null && OnlineManager.CurrentGame.HostSelectingMap)
                    Creator.Text = "";
                else
                    Creator.Text = $"By: {map.Creator} | Length: {time} | BPM: {(int) (map.Bpm * ModHelper.GetRateFromMods(ModManager.Mods))} " +
                                   $"| LNs: {(int) map.LNPercentage}%";

                // Inform the server that we now have the map if we didn't before.
                if (OnlineManager.CurrentGame != null && OnlineManager.CurrentGame.PlayersWithoutMap.Contains(OnlineManager.Self.OnlineUser.Id))
                    OnlineManager.Client.HasMultiplayerGameMap();

                if (game.CurrentScreen.Type == QuaverScreenType.Lobby || game.CurrentScreen.Type == QuaverScreenType.Multiplayer
                                                                      || QuaverScreenManager.QueuedScreen.Type == QuaverScreenType.Multiplayer
                                                                      || AudioEngine.Map != map)
                {
                    if (BackgroundHelper.Map != MapManager.Selected.Value)
                    {
                        Background.Alpha = 0;

                        var view = Screen.View as MultiplayerScreenView;
                        view?.FadeBackgroundOut();
                        BackgroundHelper.Load(map);
                    }

                    ThreadScheduler.Run(() =>
                    {
                        try
                        {
                            if (AudioEngine.Map != map)
                            {
                                if (!HasMap)
                                    return;

                                AudioEngine.LoadCurrentTrack();
                                AudioEngine.Track.Play();
                            }
                        }
                        catch (Exception e)
                        {
                            // ignored
                        }
                    });
                }
            }
            // Let the server know that we don't have the selected map
            else
            {
                ArtistTitle.Tint = Color.Red;

                Creator.Text = Game.MapId != -1 ? "You don't have this map. Click to download!" : "You don't have this map. Download not available!";
                Creator.Tint = Colors.SecondaryAccent;

                if (OnlineManager.CurrentGame != null && !OnlineManager.CurrentGame.PlayersWithoutMap.Contains(OnlineManager.Self.OnlineUser.Id))
                    OnlineManager.Client.DontHaveMultiplayerGameMap();

                if (!AudioEngine.Track.IsStopped)
                    AudioEngine.Track.Stop();

                MapManager.Selected.Value = MapManager.Mapsets.First().Maps.First();
            }*/
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
            Game.AlternativeMd5 = e.AlternativeMd5;
            Game.MapId = e.MapId;
            Game.MapsetId = e.MapsetId;
            Game.Map = e.Map;
            Game.DifficultyRating = e.DifficultyRating;
            Game.AllDifficultyRatings = e.AllDifficultyRatings;
            Game.GameMode = e.GameMode;

            var game = (QuaverGame)GameBase.Game;

            if (game.CurrentScreen.Type == QuaverScreenType.Gameplay || game.CurrentScreen.Type == QuaverScreenType.Results)
                return;

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

            var game = GameBase.Game as QuaverGame;

            if (game.CurrentScreen.Type == QuaverScreenType.Select)
                return;

            UpdateContent();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameHostSelectingMap(object sender, GameHostSelectingMapEventArgs e)
            => UpdateContent();

        private void OnModsChanged(object sender, ModsChangedEventArgs e)
        {
            var game = GameBase.Game as QuaverGame;

            if (game.CurrentScreen.Type == QuaverScreenType.Select)
                return;

            UpdateContent();
        }

        /// <summary>
        ///     Called when the download button has been clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownloadButtonClicked(object sender, EventArgs e)
        {
            // Go to song select if host
            if (OnlineManager.CurrentGame?.HostId == OnlineManager.Self.OnlineUser.Id)
            {
                var game = (QuaverGame)GameBase.Game;
                var screen = game.CurrentScreen as MultiplayerScreen;
                screen?.Exit(() => new SelectionScreen(), 0, QuaverScreenChangeType.AddToStack);
                return;
            }

            if (Game.MapsetId == -1 || !OnlineManager.CurrentGame.PlayersWithoutMap.Contains(OnlineManager.Self.OnlineUser.Id))
                return;

            if (CurrentDownload != null)
            {
                if (CurrentDownload.MapsetId == Game.MapsetId)
                {
                    NotificationManager.Show(NotificationLevel.Error, "The mapset is already downloading. Slow down!");
                    return;
                }

                // ReSharper disable twice DelegateSubtraction
                CurrentDownload.Progress.ValueChanged -= OnDownloadProgressChanged;
                CurrentDownload.Status.ValueChanged -= OnDownloadStatusChanged;
            }

            CurrentDownload = MapsetDownloadManager.Download(Game.MapsetId, Game.Map, "");
            CurrentDownload.Progress.ValueChanged += OnDownloadProgressChanged;
            CurrentDownload.Status.ValueChanged += OnDownloadStatusChanged;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownloadProgressChanged(object sender, BindableValueChangedEventArgs<DownloadProgressEventArgs> e)
        {
            if (CurrentDownload.MapsetId == Game.MapsetId)
                Creator.Text = $"Downloading Map: {e.Value.ProgressPercentage}%";
        }

        private void OnDownloadStatusChanged(object sender, BindableValueChangedEventArgs<DownloadStatusChangedEventArgs> e)
        {
            if (e.Value.Status != FileDownloaderStatus.Complete)
                return;

            CurrentDownload.Dispose();

            if (e.Value.Error != null)
                NotificationManager.Show(NotificationLevel.Error, "Download Failed!");

            if (CurrentDownload.MapsetId == Game.MapsetId)
            {
                NotificationManager.Show(NotificationLevel.Success, "Download Complete!");

                var game = GameBase.Game as QuaverGame;

                if (game?.CurrentScreen.Type == QuaverScreenType.Multiplayer || game?.CurrentScreen.Type == QuaverScreenType.Select)
                    game?.CurrentScreen.Exit(() => new ImportingScreen(Screen), 0, QuaverScreenChangeType.AddToStack);
            }

            CurrentDownload = null;
        }
    }
}