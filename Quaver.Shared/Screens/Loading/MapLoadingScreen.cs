/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Replays;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Gameplay;
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
        /// </summary>
        public MapLoadingScreen(List<Score> scores, Replay replay = null)
        {
            Scores = scores;
            Replay = replay;
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
                ParseAndLoadMap();
                LoadGameplayScreen();
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
            try
            {
                // Throw an exception if there is no selected map.
                if (MapManager.Selected.Value == null)
                    throw new Exception("No selected map, we should not be on this screen!");

                MapManager.Selected.Value.Qua = MapManager.Selected.Value.LoadQua();

                if (Replay != null)
                {
                    AddModsFromReplay(Replay);
                }

                MapManager.Selected.Value.Qua.ApplyMods(ModManager.Mods);

                // Asynchronously write to a file for livestreamers the difficulty rating
                using (var writer = File.CreateText(ConfigManager.DataDirectory + "/temp/Now Playing/difficulty.txt"))
                    writer.Write($"{MapManager.Selected.Value.DifficultyFromMods(ModManager.Mods):0.00}");

                using (var writer = File.CreateText(ConfigManager.DataDirectory + "/temp/Now Playing/map.txt"))
                    writer.Write($"{MapManager.Selected.Value.Qua.Artist} - {MapManager.Selected.Value.Qua.Title} [{MapManager.Selected.Value.Qua.DifficultyName}] ");
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Loads the gameplay screen (audio, md5 hashes, etc.)
        /// </summary>
        private void LoadGameplayScreen()
        {
            try
            {
                // Stop the current audio and load it again before moving onto the next state.
                try
                {
                    AudioEngine.LoadCurrentTrack();
                }
                catch (AudioEngineException e)
                {
                    Console.WriteLine(e);
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
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Exit(() => new GameplayScreen(MapManager.Selected.Value.Qua, md5, new List<Score>(), Replay));
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Adds all modifiers that were present in the replay.
        /// </summary>
        private static void AddModsFromReplay(Replay replay)
        {
            // Remove all the current mods that we have on.
            ModManager.RemoveAllMods();

            // Put on the mods from the replay.);
            for (var i = 0; i <= Math.Log((int)replay.Mods, 2); i++)
            {
                var mod = (ModIdentifier)Math.Pow(2, i);

                if (!replay.Mods.HasFlag(mod))
                    continue;

                ModManager.AddMod(mod);
            }
        }
    }
}
