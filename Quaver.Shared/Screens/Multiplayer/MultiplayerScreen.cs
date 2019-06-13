using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Loading;
using Quaver.Shared.Screens.Lobby;
using Quaver.Shared.Screens.Select.UI.Modifiers;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Multiplayer
{
    public sealed class MultiplayerScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Multiplayer;

        /// <summary>
        ///     The multiplayer game this represents
        /// </summary>
        public MultiplayerGame Game { get; }

        /// <summary>
        ///     If true, it will play the track on <see cref="OnFirstUpdate"/>
        /// </summary>
        public bool PlayTrackOnFirstUpdate { get; }

        ///  <summary>
        ///
        ///  </summary>
        ///  <param name="game"></param>
        /// <param name="playTrackOnFirstUpdate"></param>
        public MultiplayerScreen(MultiplayerGame game, bool playTrackOnFirstUpdate = false)
        {
            Game = game;
            PlayTrackOnFirstUpdate = playTrackOnFirstUpdate;

            SetRichPresence();

            OnlineManager.Client.OnGameStarted += OnGameStarted;
            OnlineManager.Client.OnGameCountdownStart += OnCountdownStart;
            OnlineManager.Client.OnGameCountdownStop += OnCountdownStop;
            View = new MultiplayerScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void OnFirstUpdate()
        {
            MapLoadingScreen.AddModsFromIdentifiers(OnlineManager.GetSelfActivatedMods());

            if (PlayTrackOnFirstUpdate)
            {
                var view = (MultiplayerScreenView) View;
                view.Map.UpdateContent();
            }

            base.OnFirstUpdate();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (!Exiting && DialogManager.Dialogs.Count == 0)
            {
                if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                    LeaveGame();

                if (KeyboardManager.IsUniqueKeyPress(Keys.F1) && OnlineManager.CurrentGame?.FreeModType != MultiplayerFreeModType.None)
                    DialogManager.Show(new ModifiersDialog());
            }

            KeepPlayingAudioTrack();
            base.Update(gameTime);
        }

        /// <summary>
        ///     Removes a player from the player list
        /// </summary>
        /// <param name="user"></param>
        public void RemovePlayer(OnlineUser user)
        {
            var view = (MultiplayerScreenView) View;
            view.PlayerList.RemovePlayer(user);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            OnlineManager.Client.OnGameStarted -= OnGameStarted;
            OnlineManager.Client.OnGameCountdownStart -= OnCountdownStart;
            OnlineManager.Client.OnGameCountdownStop -= OnCountdownStop;
            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.Multiplayer, -1, "", 1, "", 0);

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameStarted(object sender, GameStartedEventArgs e)
        {
            OnlineManager.CurrentGame.PlayersReady.Clear();
            OnlineManager.CurrentGame.CountdownStartTime = -1;

            var view = (MultiplayerScreenView) View;
            // view.PlayerList.Pool.ForEach(x => x.UpdateContent(x.Item, x.Index));

            if (OnlineManager.CurrentGame.PlayersWithoutMap.Contains(OnlineManager.Self.OnlineUser.Id))
            {
                NotificationManager.Show(NotificationLevel.Warning, "Match started, but we don't have the map!");
                return;
            }

            if (OnlineManager.CurrentGame.RefereeUserId == OnlineManager.Self.OnlineUser.Id)
            {
                NotificationManager.Show(NotificationLevel.Info, "Match started. Click to watch the match live on the web as a referee. ",
                    (o, args) => BrowserHelper.OpenURL($"https://quavergame.com/multiplayer/game/{OnlineManager.CurrentGame.GameId}"));

                return;
            }

            MapManager.Selected.Value.Scores.Value = GetScoresFromMultiplayerUsers();
            Exit(() =>
            {
                // Make sure map is absolutely correct before going to map loading screen.
                view.Map.UpdateContent();

                return new MapLoadingScreen(MapManager.Selected.Value.Scores.Value);
            });
        }

        /// <summary>
        ///     Returns a list of scores from multiplayer users
        /// </summary>
        /// <returns></returns>
        private static List<Score> GetScoresFromMultiplayerUsers()
        {
            var playingUsers = OnlineManager.OnlineUsers.ToList().FindAll(x =>
                OnlineManager.CurrentGame.PlayerIds.Contains(x.Key) &&
                !OnlineManager.CurrentGame.PlayersWithoutMap.Contains(x.Key) &&
                 OnlineManager.CurrentGame.RefereeUserId != x.Key &&
                x.Value != OnlineManager.Self);

            var scores = new List<Score>();

            playingUsers.ForEach(x =>
            {
                scores.Add(new Score
                {
                    PlayerId = x.Key,
                    SteamId = x.Value.OnlineUser.SteamId,
                    Name = x.Value.OnlineUser.Username,
                    Mods = (long) OnlineManager.GetUserActivatedMods(x.Value.OnlineUser.Id),
                    IsMultiplayer = true,
                    IsOnline = true
                });
            });

            return scores;
        }

        /// <summary>
        /// </summary>
        public void SetRichPresence()
        {
            DiscordHelper.Presence.Details = "Waiting to Start";
            DiscordHelper.Presence.State = $"{Game.Name} ({Game.PlayerIds.Count} of {Game.MaxPlayers})";
            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);
        }

        /// <summary>
        /// </summary>
        public void LeaveGame() => Exit(() =>
        {
            OnlineManager.LeaveGame();
            return new LobbyScreen();
        });

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountdownStart(object sender, StartCountdownEventArgs e)
            => OnlineManager.CurrentGame.CountdownStartTime = e.TimeStarted;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountdownStop(object sender, StopCountdownEventArgs e)
            => OnlineManager.CurrentGame.CountdownStartTime = -1;

        /// <summary>
        ///     Plays the audio track at the preview time if it has stopped
        /// </summary>
        private void KeepPlayingAudioTrack()
        {
            if (Exiting)
                return;

            if (AudioEngine.Track == null || AudioEngine.Track.HasPlayed && AudioEngine.Track.IsStopped)
            {
                try
                {
                    AudioEngine.LoadCurrentTrack();
                    AudioEngine.Track?.Play();
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }
    }
}