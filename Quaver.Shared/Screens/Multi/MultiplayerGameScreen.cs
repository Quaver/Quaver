using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Selection.UI;
using Wobble.Bindables;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Multi
{
    public sealed class MultiplayerGameScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Multiplayer;

        /// <summary>
        ///     The current multiplayer game
        /// </summary>
        public Bindable<MultiplayerGame> Game { get; private set; }

        /// <summary>
        ///     The currently active panel on the left side of the screen
        /// </summary>
        public Bindable<SelectContainerPanel> ActiveLeftPanel { get; private set; }

        /// <summary>
        ///     If true, when the screen exits, it will not exit the user out of the game
        /// </summary>
        public bool DontLeaveGameUponScreenSwitch { get; set; }

        /// <summary>
        /// </summary>
        public MultiplayerGameScreen()
        {
            CreateGameBindable();
            InitializeActiveLeftPanelBindable();

            ScreenExiting += (sender, args) =>
            {
                if (DontLeaveGameUponScreenSwitch)
                    return;

                OnlineManager.LeaveGame();
            };

            SelectMultiplayerMap();

            if (OnlineManager.Client != null)
                OnlineManager.Client.OnGameMapChanged += OnMapChanged;

            View = new MultiplayerGameScreenView(this);
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleInput();
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Game?.Dispose();

            if (OnlineManager.Client != null)
                OnlineManager.Client.OnGameMapChanged -= OnMapChanged;

            base.Destroy();
        }

        /// <summary>
        ///     Creates <see cref="Game"/>
        /// </summary>
        private void CreateGameBindable()
        {
            var game = OnlineManager.CurrentGame ?? GetTestGame();
            Game = new Bindable<MultiplayerGame>(game) {Value = game};
        }

        /// <summary>
        ///     Initializes the bindable which keeps track of which panel on the left side of the screen is active
        /// </summary>
        private void InitializeActiveLeftPanelBindable()
        {
            ActiveLeftPanel = new Bindable<SelectContainerPanel>(SelectContainerPanel.MatchSettings)
            {
                Value = SelectContainerPanel.MatchSettings
            };
        }

        /// <summary>
        ///     Finds and selects the multiplayer map
        /// </summary>
        private void SelectMultiplayerMap()
        {
            var map = MapManager.FindMapFromMd5(Game.Value.MapMd5);

            if (map == null)
                map = MapManager.FindMapFromMd5(Game.Value.AlternativeMd5);

            if (map == null)
            {
                if (AudioEngine.Track.IsPlaying)
                    AudioEngine.Track.Pause();

                MapManager.Selected.Value = null;
                OnlineManager.Client?.DontHaveMultiplayerGameMap();
                return;
            }

            MapManager.Selected.Value = map;
            OnlineManager.Client?.HasMultiplayerGameMap();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, GameMapChangedEventArgs e) => SelectMultiplayerMap();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.Multiplayer, -1, "-1", 1, "", 0);

        /// <summary>
        /// </summary>
        private void HandleInput()
        {
            if (DialogManager.Dialogs.Count != 0)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.F2))
                HandleKeyPressF2();
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressF2()
        {
            if (ActiveLeftPanel.Value != SelectContainerPanel.Leaderboard)
                ActiveLeftPanel.Value = SelectContainerPanel.Leaderboard;
            else
                ActiveLeftPanel.Value = SelectContainerPanel.MatchSettings;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private MultiplayerGame GetTestGame()
        {
            return new MultiplayerGame
            {
                Id = "abc123",
                GameId = 999,
                Name = "Example Game",
                Type = MultiplayerGameType.Friendly,
                Ruleset = MultiplayerGameRuleset.Battle_Royale,
                GameMode = (byte) GameMode.Keys4,
                PlayerIds = new List<int>(),
                MaxPlayers = 16,
                MapMd5 = "None",
                AlternativeMd5 = "None",
                MapId = -1,
                MapsetId = -1,
                Map = "Artist - Title [Example]",
                JudgementCount = 1000,
                HostId = 1,
                PlayersReady = new List<int>() { 3, 6, 13, 7 },
                RedTeamPlayers = new List<int> { 0, 1, 2, 3, 4, 5, 6 },
                BlueTeamPlayers = new List<int> { 7, 8, 9, 10, 11, 12, 13, 14},
                PlayersWithoutMap = new List<int>()
                {
                    8, 9, 7, 2
                },
                PlayerMods = new List<MultiplayerPlayerMods>()
                {
                    new MultiplayerPlayerMods()
                    {
                        UserId = 2,
                        Modifiers = ((long) (ModIdentifier.NoFail)).ToString()
                    },
                    new MultiplayerPlayerMods()
                    {
                        UserId = 3,
                        Modifiers = ((long) (ModIdentifier.Mirror | ModIdentifier.Speed07X)).ToString()
                    }
                },
                PlayerWins = new List<MultiplayerPlayerWins>()
                {
                    new MultiplayerPlayerWins()
                    {
                        UserId = 0,
                        Wins = 6
                    },
                    new MultiplayerPlayerWins()
                    {
                        UserId = 5,
                        Wins = 47
                    }
                },
                RefereeUserId = 11,
                HostRotation = true,
                FreeModType = MultiplayerFreeModType.Regular,
            };
        }
    }
}