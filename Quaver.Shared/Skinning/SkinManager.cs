/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.IO;
using IniFileParser;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Transitions;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Main;
using Quaver.Shared.Screens.Selection;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Writers.Zip;
using Wobble;
using Wobble.Logging;
using Wobble.Platform;

namespace Quaver.Shared.Skinning
{
    public static class SkinManager
    {
        /// <summary>
        ///     The time that the user has requested their skin be reloaded.
        /// </summary>
        public static long TimeSkinReloadRequested { get; set; }

        /// <summary>
        ///     If non-null, we require a skin reload.
        /// </summary>
        public static string NewQueuedSkin { get; set; }

        /// <summary>
        ///     If non-null, this is the workshop skin that'll be reloaded
        /// </summary>
        public static string NewWorkshopSkin { get; set; }

        /// <summary>
        ///     The currently selected skin
        /// </summary>
        public static SkinStore Skin { get; set; }

        /// <summary>
        ///     The skin for player 2 in the tournament screen
        /// </summary>
        public static SkinStore TournamentPlayer2Skin { get; set; }

        /// <summary>
        ///     Loads the currently selected skin
        /// </summary>
        public static void Load()
        {
            Skin = new SkinStore();

            if (ConfigManager.TournamentPlayer2Skin.Value == null ||
                ConfigManager.TournamentPlayer2Skin.Value == ConfigManager.Skin.Value)
            {
                TournamentPlayer2Skin = Skin;
                return;
            }

            TournamentPlayer2Skin = new SkinStore(ConfigManager.TournamentPlayer2Skin.Value);
        }

        /// <summary>
        /// </summary>
        private static bool SkinExportInProgress { get; set; }

        /// <summary>
        ///     Event raised when the user's skin has reloaded.
        /// </summary>
        public static event EventHandler<SkinReloadedEventArgs> SkinLoaded;

        /// <summary>
        ///     Called every frame. Waits for a skin reload to be queued up.
        /// </summary>
        public static void HandleSkinReloading()
        {
            // Reload skin when applicable
            if (TimeSkinReloadRequested != 0 && GameBase.Game.TimeRunning - TimeSkinReloadRequested >= 400)
            {
                Load();
                TimeSkinReloadRequested = 0;
                SkinLoaded?.Invoke(typeof(SkinManager), new SkinReloadedEventArgs());

                var game = (QuaverGame) GameBase.Game;

                switch (game.CurrentScreen.Type)
                {
                    case QuaverScreenType.Menu:
                        game.CurrentScreen.Exit(() => new MainMenuScreen());
                        break;
                    case QuaverScreenType.Select:
                        game.CurrentScreen.Exit(() => new SelectionScreen());
                        break;
                }

                ThreadScheduler.RunAfter(() =>
                {
                    Transitioner.FadeOut();
                    NotificationManager.Show(NotificationLevel.Success, "Skin has been successfully loaded!");
                }, 200);
            }
        }

        /// <summary>
        ///     Exports the current skin to a file
        /// </summary>
        public static void Export()
        {
            if (SkinExportInProgress)
            {
                NotificationManager.Show(NotificationLevel.Error, "Slow down! You're already exporting a skin.");
                return;
            }

            if (string.IsNullOrEmpty(ConfigManager.Skin.Value))
            {
                NotificationManager.Show(NotificationLevel.Error, "You don't have a custom skin selected!");
                return;
            }

            NotificationManager.Show(NotificationLevel.Info, "Please wait while we export your skin...");
            SkinExportInProgress = true;

            ThreadScheduler.Run(() =>
            {
                try
                {
                    using (var archive = ArchiveFactory.Create(ArchiveType.Zip))
                    {
                        var dir = $"{ConfigManager.DataDirectory.Value}/Exports";
                        Directory.CreateDirectory(dir);

                        var skinDir = ConfigManager.UseSteamWorkshopSkin.Value ?
                            $"{ConfigManager.SteamWorkshopDirectory.Value}/{ConfigManager.Skin.Value}"
                                : $"{ConfigManager.SkinDirectory.Value}/{ConfigManager.Skin.Value}";

                        if (!Directory.Exists(skinDir))
                        {
                            NotificationManager.Show(NotificationLevel.Warning, "You cannot export this skin!");
                            SkinExportInProgress = false;
                            return;
                        }

                        archive.AddAllFromDirectory(skinDir);

                        var name = ConfigManager.Skin.Value;

                        if (ConfigManager.UseSteamWorkshopSkin.Value)
                        {
                            if (File.Exists($"{skinDir}/skin.ini"))
                            {
                                var data = new IniFileParser.IniFileParser(new ConcatenateDuplicatedKeysIniDataParser())
                                    .ReadFile($"{skinDir}/skin.ini")["General"];

                                if (data["Name"] != null)
                                    name = $"{data["Name"]}";
                            }
                        }

                        var path = $"{dir}/{name}.qs";
                        archive.SaveTo(path, new ZipWriterOptions(CompressionType.None));

                        Utils.NativeUtils.HighlightInFileManager(path);
                    }
                }
                catch (Exception e)
                {
                    NotificationManager.Show(NotificationLevel.Error, "An error occurred while trying to export your skin!");
                    Logger.Error(e, LogType.Runtime);
                }

                SkinExportInProgress = false;
            });
        }

        /// <summary>
        ///     Imports a skin file.
        /// </summary>
        public static void Import(string path)
        {
            Transitioner.FadeIn();

            try
            {
                ThreadScheduler.Run(() =>
                {
                    var skinName = Path.GetFileNameWithoutExtension(path);
                    var dir = $"{ConfigManager.SkinDirectory.Value}/{skinName}";

                    Directory.CreateDirectory(dir);

                    // Extract the skin into a directory.
                    using (var archive = ArchiveFactory.Open(path))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            if (!entry.IsDirectory)
                                entry.WriteToDirectory(dir, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                        }
                    }

                    // Reload the skin.
                    ConfigManager.Skin.Value = skinName;
                    NewQueuedSkin = skinName;
                    TimeSkinReloadRequested = GameBase.Game.TimeRunning;
                });
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }
    }
}
