/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.API.Replays;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Modifiers.Mods;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Multi;
using Quaver.Shared.Screens.Multiplayer;
using Quaver.Shared.Screens.Select;
using Quaver.Shared.Screens.Selection;
using Wobble;
using Wobble.Audio;
using Wobble.Audio.Tracks;
using Wobble.Logging;
using Wobble.Screens;
using AudioEngine = Quaver.Shared.Audio.AudioEngine;

namespace Quaver.Shared.Screens.Loading
{
    public class MapLoadingScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Loading;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///     The local scores from the leaderboard that'll be used during gameplay.
        /// </summary>
        private List<Score> Scores { get; }

        /// <summary>
        ///     The replay to play back.
        /// </summary>
        private Replay Replay { get; }

        /// <summary>
        ///     The spectator client (if any)
        /// </summary>
        private SpectatorClient SpectatorClient { get; }

        /// <summary>
        /// </summary>
        public MapLoadingScreen(List<Score> scores, Replay replay = null, SpectatorClient spectatorClient = null)
        {
            Scores = scores;
            Replay = replay;
            SpectatorClient = spectatorClient;

            var game = GameBase.Game as QuaverGame;
            var cursor = game?.GlobalUserInterface.Cursor;
            cursor.Alpha = 0;

            View = new MapLoadingScreenView(this);
            AudioTrack.AllowPlayback = false;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void OnFirstUpdate()
        {
            ThreadScheduler.Run(() =>
            {
                try
                {
                    ParseAndLoadMap();
                    WriteStreamerFiles();
                    LoadGameplayScreen();
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, "Failed to load the map. Is your .qua file valid?");
                    GameBase.Game.GlobalUserInterface.Cursor.Alpha = 1;
                    Exit(() => new SelectionScreen());
                }
            });

            base.OnFirstUpdate();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => null;

        /// <summary>
        ///     Loads the currently selected map asynchronously.
        /// </summary>
        private void ParseAndLoadMap()
        {
            // Throw an exception if there is no selected map.
            if (MapManager.Selected.Value == null)
                throw new Exception("No selected map, we should not be on this screen!");

            // Make sure the absolutely correct map is selected in multiplayer
            if (OnlineManager.CurrentGame != null)
            {
                MultiplayerGameScreen.SelectMultiplayerMap();
                AddModsFromIdentifiers(OnlineManager.GetSelfActivatedMods());

                if (MapManager.Selected.Value == null)
                {
                    Exit(() => new MultiplayerGameScreen());
                    return;
                }
            }

            MapManager.Selected.Value.Qua = MapManager.Selected.Value.LoadQua();

            if (Replay != null)
                AddModsFromIdentifiers(Replay.Mods);

            MapManager.Selected.Value.Qua.ApplyMods(ModManager.Mods);

            // Generate seed and randomize lanes if Randomize modifier is active.
            if (ModManager.IsActivated(ModIdentifier.Randomize))
            {
                int seed;

                // If loading from a replay.
                if (Replay != null)
                    seed = Replay.RandomizeModifierSeed;
                // If loading gameplay.
                else
                {
                    var randomizeModifier = (ModRandomize) ModManager.CurrentModifiersList.Find(x => x.ModIdentifier.Equals(ModIdentifier.Randomize));
                    randomizeModifier.GenerateSeed();
                    seed = randomizeModifier.Seed;
                }

                MapManager.Selected.Value.Qua.RandomizeLanes(seed);
            }
        }

        /// <summary>
        ///    Asynchronously writes files for livestreamers
        /// </summary>
        private static void WriteStreamerFiles()
        {
            var streamerValues = new[]
            {
                ("difficulty", $"{MapManager.Selected.Value.DifficultyFromMods(ModManager.Mods):0.00}"),
                ("map", MapManager.Selected.Value.Qua + " "),
                ("mods", ModHelper.GetModsString(ModManager.Mods)),
                ("mapid", MapManager.Selected.Value.MapId.ToString())
            };

            foreach (var (fileName, value) in streamerValues)
            {
                try
                {
                    using (var writer = File.CreateText($"{ConfigManager.TempDirectory}/Now Playing/{fileName}.txt"))
                        writer.Write(value);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }
        }

        /// <summary>
        ///     Loads the gameplay screen (audio, md5 hashes, etc.)
        /// </summary>
        private void LoadGameplayScreen()
        {
            // Stop the current audio and load it again before moving onto the next state.
            try
            {
                AudioEngine.LoadCurrentTrack();
            }
            catch (AudioEngineException e)
            {
                Logger.Error(e, LogType.Runtime);
                Logger.Warning("Audio file could not be loaded, but proceeding anyway!", LogType.Runtime);
            }

            // Get the MD5 Hash of the played map and change the state.
            var quaPath = $"{ConfigManager.SongDirectory}/{MapManager.Selected.Value.Directory}/{MapManager.Selected.Value.Path}";

            // Get the Md5 of the played map
            string md5;
            switch (MapManager.Selected.Value.Game)
            {
                case MapGame.Quaver:
                    md5 = MapsetHelper.GetMd5Checksum(quaPath);
                    break;
                case MapGame.Osu:
                    md5 = MapsetHelper.GetMd5Checksum($"{MapManager.OsuSongsFolder}/{MapManager.Selected.Value.Directory}/{MapManager.Selected.Value.Path}");
                    break;
                // Etterna uses a "Chart Key" rather than an md5 hash, so allow it
                case MapGame.Etterna:
                    md5 = MapManager.Selected.Value.Md5Checksum;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Exit(() => new GameplayScreen(MapManager.Selected.Value.Qua, md5, Scores ?? new List<Score>(), Replay, false, 0, false, SpectatorClient));
        }

        /// <summary>
        ///     Adds all modifiers from a mod enum combo
        /// </summary>
        public static void AddModsFromIdentifiers(ModIdentifier mods)
        {
            if (ModManager.Mods == mods)
                return;

            // Only remove the modifiers that need to be removed and aren't already activated
            for (var i = ModManager.CurrentModifiersList.Count - 1; i >= 0; i--)
            {
                var mod = ModManager.CurrentModifiersList[i];

                if (mods.HasFlag(mod.ModIdentifier))
                    continue;

                ModManager.RemoveMod(mod.ModIdentifier);
            }

            for (var i = 0; i <= Math.Log((long)mods, 2); i++)
            {
                var mod = (ModIdentifier)((long)Math.Pow(2, i));

                if (!mods.HasFlag(mod))
                    continue;

                try
                {
                    // Mod is already activated
                    if (ModManager.Mods.HasFlag(mod))
                        continue;

                    ModManager.AddMod(mod);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }

            ModManager.FireModsChangedEvent();
            ModManager.CheckModInconsistencies();
        }
    }
}
