using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IniFileParser;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Tournament.Overlay.Components;
using Wobble;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;
using Wobble.Window;

namespace Quaver.Shared.Screens.Tournament.Overlay
{
    public class TournamentOverlay : Sprite
    {
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
        ///     Determines if the names of the players will be displayed
        /// </summary>
        public Bindable<bool> DisplayPlayerNames { get; } = new Bindable<bool>(true);

        /// <summary>
        ///     The font size of each player
        /// </summary>
        public BindableInt PlayerNameFontSize { get; } = new BindableInt(22, 1, int.MaxValue);

        /// <summary>
        ///     The position of player 1's username
        /// </summary>
        public Bindable<Vector2> Player1NamePosition { get; } = new Bindable<Vector2>(new Vector2(0, 0));

        /// <summary>
        ///     The position of player 2's username
        /// </summary>
        public Bindable<Vector2> Player2NamePosition { get; } = new Bindable<Vector2>(new Vector2(0, 100));

        /// <summary>
        ///     If true, the username and flag will have their positions inverted
        /// </summary>
        public Bindable<bool> Player1InvertFlagAndUsername { get; } = new Bindable<bool>(false);

        /// <summary>
        ///     If true, the username and flag will have their positions inverted
        /// </summary>
        public Bindable<bool> Player2InvertFlagAndUsername { get; } = new Bindable<bool>(true);

        /// <summary>
        ///     If true, the number of wins each player has will be displayed
        /// </summary>
        public Bindable<bool> DisplayWinCounts { get; } = new Bindable<bool>(true);

        /// <summary>
        /// </summary>
        public BindableInt WinCountFontSize { get; } = new BindableInt(22, 1, int.MaxValue);

        /// <summary>
        ///     The position of player 1's win count
        /// </summary>
        public Bindable<Vector2> Player1WinCountPosition { get; } = new Bindable<Vector2>(new Vector2(0, 0));

        /// <summary>
        ///     The position of player 2's win count
        /// </summary>
        public Bindable<Vector2> Player2WinCountPosition { get; } = new Bindable<Vector2>(new Vector2(0, 0));

        /// <summary>
        ///     Displays the usernames of the users
        /// </summary>
        private List<TournamentPlayerUsername> DrawableUsernames { get; set; }

        /// <summary>
        ///     Displays the win counts of each player
        /// </summary>
        private List<TournamentPlayerWinCount> WinCounts { get; set; }

        /// <summary>
        /// </summary>
        private FileSystemWatcher Watcher { get; }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        /// <param name="players"></param>
        public TournamentOverlay(MultiplayerGame game, List<TournamentPlayer> players)
        {
            Game = game;
            Players = players;

            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            InitializeOverlaySprite();
            ReadSettingsFile();

            CreateUsernames();
            CreateWinCounts();

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
            DisplayPlayerNames.Dispose();
            PlayerNameFontSize.Dispose();
            Player1NamePosition.Dispose();
            Player2NamePosition.Dispose();
            Player1InvertFlagAndUsername.Dispose();
            Player2InvertFlagAndUsername.Dispose();
            DisplayWinCounts.Dispose();
            WinCountFontSize.Dispose();
            Player1WinCountPosition.Dispose();
            Player2WinCountPosition.Dispose();
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
            var data = new IniFileParser.IniFileParser(new ConcatenateDuplicatedKeysIniDataParser()).ReadFile(SettingsPath);

            var players = data["Players"];
            DisplayPlayerNames.Value = ConfigHelper.ReadBool(DisplayPlayerNames.Default, players["DisplayPlayerNames"]);
            PlayerNameFontSize.Value = ConfigHelper.ReadInt32(PlayerNameFontSize.Default, players["PlayerNameFontSize"]);
            Player1NamePosition.Value = ConfigHelper.ReadVector2(Player1NamePosition.Default, players["Player1NamePosition"]);
            Player2NamePosition.Value = ConfigHelper.ReadVector2(Player2NamePosition.Default, players["Player2NamePosition"]);
            Player1InvertFlagAndUsername.Value = ConfigHelper.ReadBool(Player1InvertFlagAndUsername.Default,players["Player1InvertFlagAndUsername"]);
            Player2InvertFlagAndUsername.Value = ConfigHelper.ReadBool(Player2InvertFlagAndUsername.Default,players["Player2InvertFlagAndUsername"]);

            var wins = data["Wins"];
            DisplayWinCounts.Value = ConfigHelper.ReadBool(DisplayWinCounts.Default, wins["DisplayWinCounts"]);
            WinCountFontSize.Value = ConfigHelper.ReadInt32(WinCountFontSize.Default, wins["WinCountFontSize"]);
            Player1WinCountPosition.Value = ConfigHelper.ReadVector2(Player1WinCountPosition.Default, wins["Player1WinCountPosition"]);
            Player2WinCountPosition.Value = ConfigHelper.ReadVector2(Player2WinCountPosition.Default, wins["Player2WinCountPosition"]);
        }

        /// <summary>
        ///     Creates each player's drawable username + flag
        /// </summary>
        private void CreateUsernames()
        {
            DrawableUsernames = new List<TournamentPlayerUsername>();

            foreach (var player in Players)
            {
                var position = player == Players.First() ? Player1NamePosition : Player2NamePosition;
                var invertFlagAndUsername = player == Players.First() ? Player1InvertFlagAndUsername : Player2InvertFlagAndUsername;

                var username = new TournamentPlayerUsername(player, DisplayPlayerNames, PlayerNameFontSize, position,
                    invertFlagAndUsername)
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
                var position = player == Players.First() ? Player1WinCountPosition : Player2WinCountPosition;

                var winCount = new TournamentPlayerWinCount(Game, player, DisplayWinCounts, WinCountFontSize, position) {Parent = this};
                WinCounts.Add(winCount);
            }
        }
    }
}