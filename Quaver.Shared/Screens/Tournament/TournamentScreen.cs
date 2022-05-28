using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Replays;
using Quaver.Server.Common.Objects;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Discord;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Gameplay.Rulesets.Input;
using Quaver.Shared.Screens.Gameplay.UI.Scoreboard;
using Quaver.Shared.Screens.Loading;
using Quaver.Shared.Screens.Results;
using Quaver.Shared.Screens.Tournament.Gameplay;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Input;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Tournament
{
    public sealed class TournamentScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Gameplay;

        /// <summary>
        ///     The type of tournament screen this is
        /// </summary>
        public TournamentScreenType TournamentType { get; }

        /// <summary>
        ///     The "main" first gameplay screen instance
        /// </summary>
        public TournamentGameplayScreen MainGameplayScreen { get; }

        /// <summary>
        ///     List of all gameplay screen instances for each player
        /// </summary>
        public List<TournamentGameplayScreen> GameplayScreens { get; }

        /// <summary>
        /// </summary>
        private double TimeSinceScreenStarted { get; set; }

        /// <summary>
        ///     Reference to the user's selected skin (player 1)
        /// </summary>
        private SkinStore UserSkin { get; }

        /// <summary>
        ///     The skin for player 2.
        /// </summary>
        private SkinStore Player2Skin { get; set; }

        /// <summary>
        ///     Creating a tournament screen with pre-made replays to view
        /// </summary>
        /// <param name="replays"></param>
        public TournamentScreen(List<Replay> replays)
        {
            TournamentType = TournamentScreenType.Replay;
            UserSkin = SkinManager.Skin;
            LoadPlayer2Skin(replays.Count);

            GameplayScreens = new List<TournamentGameplayScreen>();

            // Go through each replay and create a gameplay screen for it.
            for (var i = 0; i < replays.Count; i++)
            {
                var replay = replays[i];

                // Load the .qua file and and make sure any mods that were used are applied
                var qua = MapManager.Selected.Value.LoadQua();
                qua.ApplyMods(replay.Mods);

                // Change the selected Qua prior to creating the screen because GameplayScreen relies on it.
                MapManager.Selected.Value.Qua = qua;
                MapLoadingScreen.AddModsFromIdentifiers(replay.Mods);

                SetSkin(replays.Count, i);

                var screen = new TournamentGameplayScreen(qua, MapManager.Selected.Value.GetAlternativeMd5(), replay);

                if (GameplayScreens.Count == 0)
                    MainGameplayScreen = screen;

                GameplayScreens.Add(screen);
            }

            // Change the selected Qua back to a fresh version
            MapManager.Selected.Value.Qua = MapManager.Selected.Value.LoadQua();

            ModManager.RemoveAllMods();
            ModManager.AddSpeedMods(ModHelper.GetRateFromMods(replays.First().Mods));

            SetRichPresenceForTournamentViewer();
            View = new TournamentScreenView(this);
        }

        /// <summary>
        ///     Tournament screen for multiplayer (spectator)
        /// </summary>
        /// <param name="game"></param>
        /// <param name="spectatees"></param>
        public TournamentScreen(MultiplayerGame game, IReadOnlyList<SpectatorClient> spectatees)
        {
            TournamentType = TournamentScreenType.Spectator;
            UserSkin = SkinManager.Skin;
            LoadPlayer2Skin(spectatees.Count);

            GameplayScreens = new List<TournamentGameplayScreen>();

            for (var i = 0; i < spectatees.Count; i++)
            {
                var qua = MapManager.Selected.Value.LoadQua();

                spectatees[i].PlayNewMap(new List<ReplayFrame>());
                qua.ApplyMods(spectatees[i].Replay.Mods);

                MapManager.Selected.Value.Qua = qua;
                MapLoadingScreen.AddModsFromIdentifiers(spectatees[i].Replay.Mods);

                SetSkin(spectatees.Count, i);

                var screen = new TournamentGameplayScreen(qua, MapManager.Selected.Value.GetAlternativeMd5(), spectatees[i]);

                if (GameplayScreens.Count == 0)
                    MainGameplayScreen = screen;

                GameplayScreens.Add(screen);
            }

            ModManager.RemoveAllMods();
            ModManager.AddSpeedMods(ModHelper.GetRateFromMods(spectatees.First().Replay.Mods));

            SetRichPresenceForTournamentViewer();
            View = new TournamentScreenView(this);
        }

        /// <summary>
        ///     Create a tournament screen (co-op mode)
        /// </summary>
        public TournamentScreen(int players)
        {
            TournamentType = TournamentScreenType.Coop;
            UserSkin = SkinManager.Skin;
            LoadPlayer2Skin(players);

            if (players < 2 || players > 4)
                throw new InvalidOperationException($"You can only create a tournament screen with 2-4 players. Got: {players}");

            GameplayScreens = new List<TournamentGameplayScreen>();

            for (var i = 0; i < players; i++)
            {
                var qua = MapManager.Selected.Value.LoadQua();
                qua.ApplyMods(ModManager.Mods);

                MapManager.Selected.Value.Qua = qua;

                SetSkin(players, i);

                var screen = new TournamentGameplayScreen(qua, MapManager.Selected.Value.GetAlternativeMd5(), new TournamentPlayerOptions(i));

                if (GameplayScreens.Count == 0)
                    MainGameplayScreen = screen;

                GameplayScreens.Add(screen);
            }

            View = new TournamentScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void OnFirstUpdate()
        {
            GameBase.Game.GlobalUserInterface.Cursor.Alpha = 0;
            base.OnFirstUpdate();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            TimeSinceScreenStarted += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (!Exiting)
            {
                UpdateScreens(gameTime);

                if (GenericKeyManager.IsDown(ConfigManager.KeyPause.Value))
                {
                    if (TournamentType == TournamentScreenType.Spectator)
                    {
                        OnlineManager.LeaveGame();
                        OnlineManager.Client?.StopSpectating();
                    }
                    else
                        MainGameplayScreen.Pause(gameTime);
                }

                // Add skipping
                if (MainGameplayScreen.EligibleToSkip && GenericKeyManager.IsUniquePress(ConfigManager.KeySkipIntro.Value))
                {
                    GameplayScreens.ForEach(x =>
                    {
                        x.SkipToNextObject();
                        x.IsPaused = true;

                        var view = (TournamentScreenView) View;

                        var player = view.TournamentPlayers.Find(y => y.User == x.SpectatorClient?.Player);

                        if (player != null)
                            player.Scoring = x.Ruleset.ScoreProcessor;
                    });

                    if (AudioEngine.Track.IsPlaying)
                        AudioEngine.Track.Pause();
                }

                switch (MainGameplayScreen.Type)
                {
                    case TournamentScreenType.Spectator:
                        break;
                    case TournamentScreenType.Coop:
                    case TournamentScreenType.Replay:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            SkinManager.Skin = UserSkin;

            GameBase.Game.GlobalUserInterface.Cursor.Alpha = 1;

            GameplayScreens.ForEach(x =>
            {
                x.Destroy();
                x.Ruleset.Playfield.Destroy();
            });

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdateScreens(GameTime gameTime)
        {
            for (var i = 0; i < GameplayScreens.Count; i++)
            {
                var screen = GameplayScreens[i];

                SetSkin(GameplayScreens.Count, i);
                screen.Update(gameTime);
            }

            try
            {
                HandleSpectator();
            }
            catch (NullReferenceException ex)
            {
                Logger.Error($"NullReferenceException at TournamentScreen.HandleSpectator()\n{ex}", LogType.Runtime);
                Logger.Error(ex.StackTrace, LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.Error($"Unhandled exception thrown at TournamentScreen.HandleSpectator()\n{e}", LogType.Runtime);
                Logger.Error(e.StackTrace, LogType.Runtime);
            }

            HandlePlayCompletion();
        }

        private void HandlePlayCompletion()
        {
            if (TournamentType == TournamentScreenType.Spectator)
                return;

            if (!Exiting)
            {
                if (GameplayScreens.All(x => x.IsPlayComplete))
                {
                    var processors = new List<ScoreProcessor>();

                    for (var i = 0; i < GameplayScreens.Count; i++)
                    {
                        var screen = GameplayScreens[i];

                        switch (TournamentType)
                        {
                            case TournamentScreenType.Replay:
                                screen.Ruleset.ScoreProcessor.PlayerName = screen.LoadedReplay.PlayerName;
                                screen.Ruleset.ScoreProcessor.SteamId = 0;
                                break;
                            case TournamentScreenType.Spectator:
                                break;
                            case TournamentScreenType.Coop:
                                screen.Ruleset.ScoreProcessor.PlayerName = $"Player {i + 1}";
                                screen.Ruleset.ScoreProcessor.SteamId = 0;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        processors.Add(screen.Ruleset.ScoreProcessor);
                    }

                    Exit(() => new ResultsScreen(MainGameplayScreen, new MultiplayerGame(), processors, new List<ScoreProcessor>()));
                }
            }
        }

        /// <summary>
        /// </summary>
        private void HandleSpectator()
        {
            if (TournamentType != TournamentScreenType.Spectator)
                return;

            var hasNoFrames = false;
            var gameStarted = GameplayScreens.Last().HasStarted;
            var track = AudioEngine.Track;

            foreach (var screen in GameplayScreens)
            {
                var inputManager = (KeysInputManager) screen.Ruleset.InputManager;

                var replay = inputManager.ReplayInputManager.Replay;

                if (replay.Frames.Count > 0)
                {
                    if (replay.Frames[replay.Frames.Count - 1].Time < track.Time
                        && GameplayScreens.All(x => !x.IsPlayComplete)
                        && OnlineManager.SpectatorClients.All(x => !x.Value.FinishedPlayingMap))
                    {
                        if (track.Length - track.Time > 2000)
                        {
                            hasNoFrames = true;

                            if (gameStarted && !track.IsDisposed && track.IsPlaying)
                                track.Pause();
                        }

                        break;
                    }
                }

                if (replay.Frames.Count == 0)
                {
                    hasNoFrames = true;
                    break;
                }
            }

            if (gameStarted && !hasNoFrames && !track.IsStopped && !track.IsPlaying)
                track.Play();

            foreach (var screen in GameplayScreens)
            {
                screen.IsPaused = hasNoFrames;

                var inputManager = (KeysInputManager) screen.Ruleset.InputManager;
                var replayInput = inputManager.ReplayInputManager;
                replayInput.VirtualPlayer.PlayAllFrames();
            }

            try
            {
                if (!Exiting && (GameplayScreens.All(x => x.IsPlayComplete)
                                 && OnlineManager.SpectatorClients.All(x => x.Value.FinishedPlayingMap)))
                {
                    var processors = new List<ScoreProcessor>();

                    foreach (var screen in GameplayScreens)
                    {
                        screen.Ruleset.ScoreProcessor.PlayerName = screen.SpectatorClient.Player.OnlineUser.Username;
                        screen.Ruleset.ScoreProcessor.SteamId = (ulong) screen.SpectatorClient.Player.OnlineUser.SteamId;
                        processors.Add(screen.Ruleset.ScoreProcessor);
                    }

                    Exit(() => new ResultsScreen(MainGameplayScreen, OnlineManager.CurrentGame,
                        processors, new List<ScoreProcessor>()));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private List<ScoreboardUser> GetScoreboardUsers()
        {
            var users = new List<ScoreboardUser>();

            foreach (var screen in GameplayScreens)
            {
                var view = (GameplayScreenView) screen.View;
                users.AddRange(view.ScoreboardLeft.Users);

                if (view.ScoreboardRight != null)
                    users.AddRange(view.ScoreboardRight.Users);
            }

            return users;
        }

        /// <summary>
        ///     Loads the skin for player 2
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void LoadPlayer2Skin(int playerCount)
        {
            if (playerCount != 2)
                return;

            Player2Skin = SkinManager.TournamentPlayer2Skin;
        }

        /// <summary>
        ///     Sets the skin based on the index of the player
        /// </summary>
        /// <param name="players"></param>
        /// <param name="index"></param>
        private void SetSkin(int players, int index)
        {
            if (players != 2)
                return;

            SkinManager.Skin = index == 0 ? UserSkin : Player2Skin;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => null;

        /// <summary>
        /// </summary>
        private void SetRichPresenceForTournamentViewer()
        {
            DiscordHelper.Presence.PartySize = GameplayScreens.Count;
            DiscordHelper.Presence.PartyMax = 4;
            DiscordHelper.Presence.EndTimestamp = 0;
            DiscordHelper.Presence.LargeImageText = OnlineManager.GetRichPresenceLargeKeyText(ConfigManager.SelectedGameMode.Value);
            DiscordHelper.Presence.SmallImageKey = ModeHelper.ToShortHand(ConfigManager.SelectedGameMode.Value).ToLower();
            DiscordHelper.Presence.SmallImageText = ModeHelper.ToLongHand(ConfigManager.SelectedGameMode.Value);

            RichPresenceHelper.UpdateRichPresence("Tournament Viewer", MapManager.Selected.Value.ToString());
        }
    }
}