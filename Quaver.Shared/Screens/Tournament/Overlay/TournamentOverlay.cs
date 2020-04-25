using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IniFileParser;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Tournament.Overlay.Components;
using Wobble;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Screens.Tournament.Overlay
{
    public class TournamentOverlay : Sprite
    {
        /// <summary>
        ///     The map that is currently being played
        /// </summary>
        private Qua Qua { get; }

        /// <summary>
        ///     The current multiplayer game in progress
        /// </summary>
        private MultiplayerGame Game { get; }

        /// <summary>
        ///     The players of the game
        /// </summary>
        private List<TournamentPlayer> Players { get; }

        /// <summary>
        /// </summary>
        private string Directory => $"{WobbleGame.WorkingDirectory}/Tournament";

        /// <summary>
        /// </summary>
        private string OverlayPath => $"{Directory}/tournament-overlay.png";

        /// <summary>
        /// </summary>
        private string SettingsPath => $"{Directory}/settings.ini";

        /// <summary>
        /// </summary>
        public TournamentDrawableSettings SongTitleSettings { get; } = new TournamentDrawableSettings("SongTitle");

        /// <summary>
        /// </summary>
        public TournamentDrawableSettings Player1WinCountSettings { get; } = new TournamentDrawableSettings("Player1WinCount");

        /// <summary>
        /// </summary>
        public TournamentDrawableSettings Player2WinCountSettings { get; } = new TournamentDrawableSettings("Player2WinCount");

        /// <summary>
        /// </summary>
        public TournamentDrawableSettings Player1UsernameSettings { get; } = new TournamentDrawableSettings("Player1Username");

        /// <summary>
        /// </summary>
        public TournamentDrawableSettings Player2UsernameSettings { get; } = new TournamentDrawableSettings("Player2Username");

        /// <summary>
        /// </summary>
        public TournamentDrawableSettings DifficultyNameSettings { get; } = new TournamentDrawableSettings("DifficultyName");

        /// <summary>
        /// </summary>
        public TournamentDrawableSettings SongLengthSettings { get; } = new TournamentDrawableSettings("SongLength");

        /// <summary>
        /// </summary>
        public TournamentDrawableSettings SongBpmSettings { get; } = new TournamentDrawableSettings("SongBpm");

        /// <summary>
        /// </summary>
        public TournamentSettingsDifficultyRating DifficultyRatingSettings { get; } = new TournamentSettingsDifficultyRating("DifficultyRating");

        /// <summary>
        /// </summary>
        public TournamentDrawableSettings Player1AccuracySettings { get; } = new TournamentDrawableSettings("Player1Accuracy");

        /// <summary>
        /// </summary>
        public TournamentDrawableSettings Player2AccuracySettings { get; } = new TournamentDrawableSettings("Player2Accuracy");

        /// <summary>
        /// </summary>
        public TournamentDrawableSettings Player1RatingSettings { get; } = new TournamentDrawableSettings("Player1Rating");

        /// <summary>
        /// </summary>
        public TournamentDrawableSettings Player2RatingSettings { get; } = new TournamentDrawableSettings("Player2Rating");

        /// <summary>
        /// </summary>
        public TournamentSettingsBanner BannerSettings { get; } = new TournamentSettingsBanner("Banner");

        /// <summary>
        /// </summary>
        public TournamentDrawableSettings MapCreatorSettings { get; } = new TournamentDrawableSettings("MapCreator");

        /// <summary>
        ///     Displays the usernames of the users
        /// </summary>
        private List<TournamentPlayerUsername> DrawableUsernames { get; set; }

        /// <summary>
        ///     Displays the win counts of each player
        /// </summary>
        private List<TournamentPlayerWinCount> WinCounts { get; set; }

        /// <summary>
        ///     Displays the name of the song being played
        /// </summary>
        private TournamentSongArtistAndTitle SongTitle { get; set; }

        /// <summary>
        /// </summary>
        private FileSystemWatcher Watcher { get; }

        /// <summary>
        /// </summary>
        /// <param name="qua"></param>
        /// <param name="game"></param>
        /// <param name="players"></param>
        public TournamentOverlay(Qua qua, MultiplayerGame game, List<TournamentPlayer> players)
        {
            Qua = qua;
            Game = game;
            Players = players;

            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            InitializeOverlaySprite();
            ReadSettingsFile();

            CreateBanner();
            CreateUsernames();
            CreateWinCounts();
            CreateSongArtistAndTitle();
            CreateDifficultyNameSettings();
            CreateSongLength();
            CreateSongBpm();
            CreateDifficultyRating();
            CreateAccuracyDisplays();
            CreateRatingDisplays();
            CreateMapCreator();

            Watcher = new FileSystemWatcher(Directory)
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "settings.ini",
                EnableRaisingEvents = true
            };

            Watcher.Changed += (o, e) => ScheduleUpdate(ReadSettingsFile);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            SongTitleSettings.Dispose();
            Player1WinCountSettings.Dispose();
            Player2WinCountSettings.Dispose();
            Player1UsernameSettings.Dispose();
            Player2UsernameSettings.Dispose();
            DifficultyNameSettings.Dispose();
            SongLengthSettings.Dispose();
            SongBpmSettings.Dispose();
            DifficultyRatingSettings.Dispose();
            Player1AccuracySettings.Dispose();
            Player2AccuracySettings.Dispose();
            Player1RatingSettings.Dispose();
            Player2RatingSettings.Dispose();
            BannerSettings.Dispose();
            MapCreatorSettings.Dispose();
            Watcher.Dispose();

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void InitializeOverlaySprite()
        {
            if (!File.Exists(OverlayPath))
            {
                Logger.Important($"Skipping load on tournament overlay as the `tournament-overlay.png` file is missing", LogType.Runtime);
                Visible = false;
                return;
            }

            if (!File.Exists(SettingsPath))
            {
                Logger.Important($"Skipping load on tournament overlay as the `settings.ini` file is missing", LogType.Runtime);
                Visible = false;
                return;
            }

            try
            {
                Image = AssetLoader.LoadTexture2DFromFile(OverlayPath);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                Visible = false;
            }
        }

        /// <summary>
        ///     Handles reading the settings file
        /// </summary>
        private void ReadSettingsFile()
        {
            try
            {
                var data = new IniFileParser.IniFileParser(new ConcatenateDuplicatedKeysIniDataParser()).ReadFile(SettingsPath);

                var players = data["Players"];
                Player1UsernameSettings.Load(players);
                Player2UsernameSettings.Load(players);

                var wins = data["Wins"];
                Player1WinCountSettings.Load(wins);
                Player2WinCountSettings.Load(wins);

                SongTitleSettings.Load(data["Song"]);
                DifficultyNameSettings.Load(data["DifficultyName"]);
                SongLengthSettings.Load(data["SongLength"]);
                SongBpmSettings.Load(data["SongBpm"]);
                DifficultyRatingSettings.Load(data["DifficultyRating"]);
                MapCreatorSettings.Load(data["MapCreator"]);

                var accuracy = data["Accuracy"];
                Player1AccuracySettings.Load(accuracy);
                Player2AccuracySettings.Load(accuracy);

                var rating = data["Rating"];
                Player1RatingSettings.Load(rating);
                Player2RatingSettings.Load(rating);

                BannerSettings.Load(data["Banner"]);
                LoadFonts();
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Loads any .ttf fonts that aren't currently cached by the user in the directory
        /// </summary>
        private void LoadFonts()
        {
            foreach (var file in System.IO.Directory.GetFiles(Directory, "*.ttf"))
            {
                var name = Path.GetFileNameWithoutExtension(file);

                if (FontManager.WobbleFonts.ContainsKey(name))
                    continue;

                try
                {
                    var font = new WobbleFontStore(22, File.ReadAllBytes(file));
                    FontManager.CacheWobbleFont(name, font);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }
        }

        /// <summary>
        ///     Creates each player's drawable username + flag
        /// </summary>
        private void CreateUsernames()
        {
            DrawableUsernames = new List<TournamentPlayerUsername>();

            foreach (var player in Players)
            {
                var isFirstPlayer = player == Players.First();

                var settings = isFirstPlayer ? Player1UsernameSettings : Player2UsernameSettings;

                var username = new TournamentPlayerUsername(player, settings)
                {
                    Parent = this
                };

                DrawableUsernames.Add(username);
            }
        }

        /// <summary>
        ///     Creates <see cref="WinCounts"/>
        /// </summary>
        private void CreateWinCounts()
        {
            WinCounts = new List<TournamentPlayerWinCount>();

            foreach (var player in Players)
            {
                var settings = player == Players.First() ? Player1WinCountSettings : Player2WinCountSettings;
                WinCounts.Add(new TournamentPlayerWinCount(Game, player, settings) {Parent = this});
            }
        }

        private void CreateSongArtistAndTitle() => SongTitle = new TournamentSongArtistAndTitle(Qua, SongTitleSettings) { Parent = this };

        // ReSharper disable twice ObjectCreationAsStatement
        private void CreateDifficultyNameSettings() => new TournamentDifficultyName(Qua, DifficultyNameSettings) {Parent = this};

        private void CreateSongLength() => new TournamentSongLength(Qua, SongLengthSettings) {Parent = this};

        private void CreateSongBpm() => new TournamentBpm(Qua, SongBpmSettings) {Parent = this};

        private void CreateDifficultyRating() => new TournamentDifficultyRating(Qua, DifficultyRatingSettings) {Parent = this};

        private void CreateAccuracyDisplays()
        {
            foreach (var player in Players)
            {
                var settings = player == Players.First() ? Player1AccuracySettings : Player2AccuracySettings;
                new TournamentPlayerAccuracy(settings, player, Players) {Parent = this};
            }
        }

        private void CreateRatingDisplays()
        {
            foreach (var player in Players)
            {
                var settings = player == Players.First() ? Player1RatingSettings : Player2RatingSettings;
                new TournamentPlayerRating(settings, player, Players) {Parent = this};
            }
        }

        private void CreateMapCreator() => new TournamentMapCreator(Qua, MapCreatorSettings) {Parent = this};

        private void CreateBanner() => new TournamentMapBanner(BannerSettings, new ScalableVector2(300, 200)) { Parent = this };
    }
}