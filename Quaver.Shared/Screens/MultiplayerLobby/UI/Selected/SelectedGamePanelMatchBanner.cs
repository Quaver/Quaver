using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Online.API.Maps;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Importing;
using Quaver.Shared.Screens.Multi;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Games;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation.Metadata;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;
using Wobble.Managers;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected
{
    public class SelectedGamePanelMatchBanner : ScrollContainer, IMultiplayerGameComponent
    {
        /// <summary>
        /// </summary>
        public Bindable<MultiplayerGame> SelectedGame { get; set; }

        /// <summary>
        /// </summary>
        private bool IsMultiplayer { get; }

        /// <summary>
        /// </summary>
        private Sprite Background { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Name { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Map { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus DifficultyRating { get; set; }

        /// <summary>
        /// </summary>
        private FilterMetadataCreator Creator { get; set; }

        /// <summary>
        /// </summary>
        private FilterMetadataBpm Bpm { get; set; }

        /// <summary>
        /// </summary>
        private FilterMetadataLength Length { get; set; }

        /// <summary>
        /// </summary>
        private FilterMetadataLongNotePercentage LongNotePercentage { get; set; }

        /// <summary>
        /// </summary>
        private FilterMetadataNotesPerSecond NotesPerSecond { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Ruleset { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Mode { get; set; }

        /// <summary>
        /// </summary>
        private ImageButton Button { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus DownloadStatus { get; set; }

        /// <summary>
        ///     The color the text is displayed as when not having the map
        /// </summary>
        private Color DontHaveMapColor { get; } = ColorHelper.HexToColor("#fa4d4d");

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="selectedGame"></param>
        /// <param name="size"></param>
        /// <param name="isMultiplayer"></param>
        public SelectedGamePanelMatchBanner(Bindable<MultiplayerGame> selectedGame, ScalableVector2 size, bool isMultiplayer)
            : base(size, size)
        {
            SelectedGame = selectedGame;
            IsMultiplayer = isMultiplayer;
            Alpha = 0;

            CreateBackground();
            CreateNameText();
            CreateMapText();
            CreateDifficultyRatingText();
            CreateRuleset();
            CreateMode();
            CreateBpm();
            CreateLength();
            CreateLongNotePercentage();
            CreateNotesPerSecond();
            CreateCreator();
            CreateDownloadStatus();
            CreateButton();

            BackgroundHelper.Loaded += OnBackgroundLoaded;
            MapManager.Selected.ValueChanged += OnMapChanged;
            ModManager.ModsChanged += OnModsChanged;

            if (OnlineManager.Client != null)
            {
                OnlineManager.Client.OnGameMapChanged += OnMultplayerMapChanged;
                OnlineManager.Client.OnGameRulesetChanged += OnMultiplayerGameRulesetChanged;
                OnlineManager.Client.OnGameMapsetShared += OnMultiplayerMapsetShared;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (IsMultiplayer)
            {
                Button.Alpha = MathHelper.Lerp(Button.Alpha, Button.IsHovered ? 0.55f : 0,
                    (float) GameBase.Game.TimeSinceLastFrame / 40);
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            BackgroundHelper.Loaded -= OnBackgroundLoaded;

            if (OnlineManager.Client != null)
            {
                OnlineManager.Client.OnGameMapChanged -= OnMultplayerMapChanged;
                OnlineManager.Client.OnGameRulesetChanged -= OnMultiplayerGameRulesetChanged;
                OnlineManager.Client.OnGameMapsetShared -= OnMultiplayerMapsetShared;
            }

            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;
            ModManager.ModsChanged -= OnModsChanged;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateBackground()
        {
            Background = new Sprite
            {
                Size = new ScalableVector2(576, 324),
                Alignment = Alignment.TopCenter,
                Y = -75,
                Image = UserInterface.MenuBackgroundNormal,
                UsePreviousSpriteBatchOptions = true,
                Alpha = 0f,
            };

            AddContainedDrawable(Background);
        }

        /// <summary>
        /// </summary>
        private void CreateNameText()
        {
            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 24)
            {
                Parent = this,
                Position = new ScalableVector2(14, 14),
                Visible = !IsMultiplayer
            };
        }

        /// <summary>
        /// </summary>
        private void CreateMapText()
        {
            Map = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 21)
            {
                Parent = this,
                Position = new ScalableVector2(Name.X, Name.Y + Name.Height + 32)
            };

            if (IsMultiplayer)
                Map.Y = Name.Y + Name.Height + 6;
        }

        /// <summary>
        /// </summary>
        private void CreateDifficultyRatingText()
        {
            DifficultyRating = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 21)
            {
                Parent = this,
                Position = new ScalableVector2(Name.X, Map.Y + Map.Height + 10)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateRuleset()
        {
            Ruleset = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Size = new ScalableVector2(107, 23),
                Position = new ScalableVector2(-Name.X, Name.Y)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateMode()
        {
            Mode = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Size = new ScalableVector2(56, 23),
                Position = new ScalableVector2(Ruleset.X - Ruleset.Width - 12, Ruleset.Y)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateBpm() => Bpm = new FilterMetadataBpm()
        {
            Parent = this,
            X = Name.X,
            Y = DifficultyRating.Y + DifficultyRating.Height + 10,
            Visible = IsMultiplayer,
        };

        /// <summary>
        /// </summary>
        private void CreateLength() => Length = new FilterMetadataLength()
        {
            Parent = this,
            Y = Bpm.Y,
            X = Bpm.X + Bpm.Width + 22,
            Visible = IsMultiplayer,
        };

        /// <summary>
        /// </summary>
        private void CreateLongNotePercentage() => LongNotePercentage = new FilterMetadataLongNotePercentage
        {
            Parent = this,
            Y = Bpm.Y,
            X = Length.X + Length.Width + 22,
            Visible = IsMultiplayer,
        };

        /// <summary>
        /// </summary>
        private void CreateNotesPerSecond() => NotesPerSecond = new FilterMetadataNotesPerSecond()
        {
            Parent = this,
            Y = Bpm.Y,
            X = LongNotePercentage.X + LongNotePercentage.Width + 22,
            Visible = IsMultiplayer
        };

        /// <summary>
        /// </summary>
        private void CreateCreator() => Creator = new FilterMetadataCreator()
        {
            Parent = this,
            X = Name.X,
            Y = Map.Y + Map.Height + 10,
            Visible = IsMultiplayer,
        };

        /// <summary>
        /// </summary>
        private void CreateButton()
        {
            Button = new ImageButton(UserInterface.BlankBox)
            {
                Parent = this,
                Size = Size,
                Alpha = 0,
                Tint = Color.Black
            };

            Button.Clicked += (sender, args) =>
            {
                if (!IsMultiplayer)
                    return;
                
                // Download the map if they don't already have it.
                if (MapManager.Selected.Value == null || MapManager.Selected.Value.Md5Checksum != SelectedGame.Value.MapMd5
                    && MapManager.Selected.Value.Md5Checksum != SelectedGame.Value.AlternativeMd5)
                {
                    DownloadMapset();
                }
                // Have the host select the map
                else if (SelectedGame.Value.HostId == OnlineManager.Self?.OnlineUser?.Id)
                {
                    var game = (QuaverGame) GameBase.Game;

                    // Automatically start importing
                    var multi = (MultiplayerGameScreen) game.CurrentScreen;
                    multi.DontLeaveGameUponScreenSwitch = true;

                    multi.Exit(() => new SelectionScreen());
                }
                else if (SelectedGame.Value.MapId != -1)
                    BrowserHelper.OpenURL($"https://quavergame.com/mapsets/map/{SelectedGame.Value.MapId}");
                else if (SelectedGame.Value.MapId == -1)
                    NotificationManager.Show(NotificationLevel.Warning, "Cannot view online listing because this map is not submitted online!");
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDownloadStatus()
        {
            DownloadStatus = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 20)
            {
                Parent = this,
                Alignment = Bpm.Alignment,
                Position = new ScalableVector2(Bpm.X, Bpm.Y - 20),
                Visible = false,
                Tint = DontHaveMapColor
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void UpdateState()
        {
            if (SelectedGame.Value == null)
                return;

            // Load new background
            ThreadScheduler.Run(() =>
            {
                var map = MapManager.FindMapFromMd5(SelectedGame.Value.MapMd5);

                if (map == null)
                    map = MapManager.FindMapFromMd5(SelectedGame.Value.AlternativeMd5);

                if (BackgroundHelper.Map == map)
                {
                    Background.Image = BackgroundHelper.RawTexture;
                    Background.Alpha = IsMultiplayer ? 0.45f : 0;
                }
                else
                {
                    Background.ClearAnimations();
                    Background.FadeTo(0, Easing.Linear, 200);

                    if (!IsMultiplayer || map != null)
                        BackgroundHelper.Load(map);
                }
            });

            ScheduleUpdate(() =>
            {
                var maxWidth = (int) Width - 225;

                Name.Text = SelectedGame.Value.Name;
                Name.TruncateWithEllipsis(maxWidth);

                Map.Text = SelectedGame.Value.GetMapName();
                Map.TruncateWithEllipsis(maxWidth);

                var map = MapManager.FindMapFromMd5(SelectedGame.Value.MapMd5);

                if (map == null)
                    map = MapManager.FindMapFromMd5(SelectedGame.Value.AlternativeMd5);

                if (map != null)
                {
                    DifficultyRating.Text = $"{map.DifficultyFromMods(ModManager.Mods):0.00}";
                    DifficultyRating.Tint = ColorHelper.DifficultyToColor((float) map.DifficultyFromMods(ModManager.Mods));

                    if (IsMultiplayer)
                    {
                        DownloadStatus.Visible = false;
                        Bpm.Visible = true;
                        Length.Visible = true;
                        NotesPerSecond.Visible = true;
                        LongNotePercentage.Visible = true;
                        Creator.Visible = true;
                    }
                }
                else
                {
                    DifficultyRating.Text = $"{StringHelper.RatingToString(SelectedGame.Value.DifficultyRating)}";
                    DifficultyRating.Tint = ColorHelper.DifficultyToColor((float) SelectedGame.Value.DifficultyRating);

                    if (IsMultiplayer)
                    {
                        DownloadStatus.Visible = true;
                        Bpm.Visible = false;
                        Length.Visible = false;
                        NotesPerSecond.Visible = false;
                        LongNotePercentage.Visible = false;
                        Creator.Visible = false;

                        if (SelectedGame.Value.MapsetId == -1 && !SelectedGame.Value.IsMapsetShared)
                            DownloadStatus.Text = $"The download for this map is not available.";
                        else
                            DownloadStatus.Text = $"You do not have this map. Click here to download!";
                    }
                }

                if (IsMultiplayer)
                    DifficultyRating.Y = Name.Y;

                DifficultyRating.Text += $" - {SelectedGame.Value.GetDifficultyName()}";
                DifficultyRating.TruncateWithEllipsis(maxWidth);

                Ruleset.Image = DrawableMultiplayerGame.GetRulesetIcon(SelectedGame.Value);
                Mode.Image = DrawableMultiplayerGame.GetModeIcon(SelectedGame.Value);
            });
        }

        /// <summary>
        /// </summary>
        private void DownloadMapset() => ThreadScheduler.Run(() =>
        {
            // Map is already downloading
            if (MapsetDownloadManager.CurrentDownloads.Any(x => x.MapsetId == SelectedGame.Value.MapsetId))
                return;

            // Prevent multiple downloads of the map
            if (MapsetDownloadManager.CurrentDownloads.Any(x => x.MapsetId == -SelectedGame.Value.GameId))
                return;

            if (SelectedGame.Value.IsMapsetShared && SelectedGame.Value.MapId == -1)
                DownloadSharedMapset();
            else
                DownloadOnlineMapset();
        });

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackgroundLoaded(object sender, BackgroundLoadedEventArgs e)
        {
            lock (Background.Image)
            {
                Background.Image = e.Texture;

                Background.ClearAnimations();
                Background.FadeTo(0.45f, Easing.Linear, 200);

                if (IsMultiplayer && e.Texture == UserInterface.MenuBackgroundRaw)
                {
                    Background.ClearAnimations();
                    Background.FadeTo(0, Easing.Linear, 200);
                }
            }
        }

        /// <summary>
        /// </summary>
        private void DownloadOnlineMapset()
        {
            var game = (QuaverGame) GameBase.Game;

            try
            {
                var response = new APIRequestMapInformation(SelectedGame.Value.MapId).ExecuteRequest();

                // If we're already downloading it, don't restart
                if (MapsetDownloadManager.CurrentDownloads.Any(x => x.MapsetId == response.Map.MapsetId))
                    return;

                // The mapset is currently being imported
                if (MapsetImporter.Queue.Contains($"{ConfigManager.DataDirectory.Value}/Downloads/{response.Map.MapsetId}.qp"))
                    return;

                var download = MapsetDownloadManager.Download(response.Map.MapsetId, response.Map.Artist, response.Map.Title);

                // Automatically start importing
                var multi = (MultiplayerGameScreen) game.CurrentScreen;
                multi.DontLeaveGameUponScreenSwitch = true;

                download.Completed.ValueChanged += (sender2, args2) => game.CurrentScreen.Exit(() => new ImportingScreen());

                MapsetDownloadManager.OpenOnlineHub();
                game.OnlineHub.SelectSection(OnlineHubSectionType.ActiveDownloads);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Network);
            }
        }

        /// <summary>
        /// </summary>
        private void DownloadSharedMapset()
        {
            var game = (QuaverGame) GameBase.Game;

            try
            {
                // If we're already downloading it, don't restart
                if (MapsetDownloadManager.CurrentDownloads.Any(x => x.MapsetId == SelectedGame.Value.GameId))
                    return;

                // The mapset is currently being imported
                if (MapsetImporter.Queue.Contains($"{ConfigManager.DataDirectory.Value}/Downloads/{SelectedGame.Value.GameId}.qp"))
                    return;

                // Make a request to the server for the DL link
                var download = MapsetDownloadManager.DownloadSharedMultiplayerMapset(SelectedGame.Value.GetMapName(), "");

                // Automatically start importing
                var multi = (MultiplayerGameScreen) game.CurrentScreen;
                multi.DontLeaveGameUponScreenSwitch = true;

                download.Completed.ValueChanged += (sender2, args2) => game.CurrentScreen.Exit(() => new ImportingScreen());

                MapsetDownloadManager.OpenOnlineHub();
                game.OnlineHub.SelectSection(OnlineHubSectionType.ActiveDownloads);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Network);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMultiplayerGameRulesetChanged(object sender, RulesetChangedEventArgs e) => UpdateState();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMultplayerMapChanged(object sender, GameMapChangedEventArgs e) => UpdateState();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e) => UpdateState();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMultiplayerMapsetShared(object sender, GameMapsetSharedEventArgs e) => UpdateState();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModsChanged(object sender, ModsChangedEventArgs e) => UpdateState();
    }
}