using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Helpers;
using Quaver.API.Replays;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Discord;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Loading;
using Quaver.Shared.Screens.Tournament.Gameplay;
using Wobble;
using Wobble.Input;

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
        ///     Creating a tournament screen with pre-made replays to view
        /// </summary>
        /// <param name="replays"></param>
        public TournamentScreen(IReadOnlyCollection<Replay> replays)
        {
            TournamentType = TournamentScreenType.Replay;

            GameplayScreens = new List<TournamentGameplayScreen>();

            // Go through each replay and create a gameplay screen for it.
            foreach (var replay in replays)
            {
                // Load the .qua file and and make sure any mods that were used are applied
                var qua = MapManager.Selected.Value.LoadQua();
                qua.ApplyMods(replay.Mods);

                // Change the selected Qua prior to creating the screen because GameplayScreen relies on it.
                MapManager.Selected.Value.Qua = qua;
                MapLoadingScreen.AddModsFromIdentifiers(replay.Mods);

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
        ///     Create a tournament screen (co-op mode)
        /// </summary>
        public TournamentScreen(int players)
        {
            TournamentType = TournamentScreenType.Coop;

            if (players < 2 || players > 4)
                throw new InvalidOperationException($"You can only create a tournament screen with 2-4 players. Got: {players}");

            GameplayScreens = new List<TournamentGameplayScreen>();

            for (var i = 0; i < players; i++)
            {
                var qua = MapManager.Selected.Value.LoadQua();
                qua.ApplyMods(ModManager.Mods);

                MapManager.Selected.Value.Qua = qua;

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
            if (!Exiting)
            {
                UpdateScreens(gameTime);

                if (KeyboardManager.CurrentState.IsKeyDown(ConfigManager.KeyPause.Value))
                    MainGameplayScreen.Pause(gameTime, false);

                switch (MainGameplayScreen.Type)
                {
                    case TournamentScreenType.Spectator:
                        break;
                    case TournamentScreenType.Coop:
                    case TournamentScreenType.Replay:
                        if (MainGameplayScreen.EligibleToSkip && KeyboardManager.IsUniqueKeyPress(ConfigManager.KeySkipIntro.Value))
                            MainGameplayScreen.SkipToNextObject();
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
            foreach (var screen in GameplayScreens)
                screen.Update(gameTime);
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
            DiscordHelper.Presence.Details = MapManager.Selected.Value.ToString();
            DiscordHelper.Presence.State = "Tournament Viewer";
            DiscordHelper.Presence.PartySize = GameplayScreens.Count;
            DiscordHelper.Presence.PartyMax = 4;
            DiscordHelper.Presence.EndTimestamp = 0;
            DiscordHelper.Presence.LargeImageText = OnlineManager.GetRichPresenceLargeKeyText(ConfigManager.SelectedGameMode.Value);
            DiscordHelper.Presence.SmallImageKey = ModeHelper.ToShortHand(ConfigManager.SelectedGameMode.Value).ToLower();
            DiscordHelper.Presence.SmallImageText = ModeHelper.ToLongHand(ConfigManager.SelectedGameMode.Value);
            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);
        }
    }
}