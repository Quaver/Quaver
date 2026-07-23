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
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Main;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.Tournament.Gameplay;
using Quaver.Shared.Skinning.V2;
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
        /// 
        public static long TimeSkinReloadRequested { get; set; }
        /// <summary>
        ///     The time that the user has requested their skin be reloaded.
        /// </summary>
        public static long TimeEditorSkinReloadRequested { get; set; }

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
        ///     The V2 configuration and assets for the primary selected skin.
        /// </summary>
        public static SkinStoreV2 SkinV2 { get; private set; }

        /// <summary>
        ///     The currently selected Editor skin
        /// </summary>
        public static SkinStore EditorSkin { get; set; }

        /// <summary>
        ///     The skin for player 2 in the tournament screen
        /// </summary>
        public static SkinStore TournamentPlayer2Skin { get; set; }

        /// <summary>
        ///     Watches for current skin changes
        /// </summary>
        public static FileSystemWatcher Watcher { get; private set; }

        /// <summary>
        ///     Loads the currently selected skin
        /// </summary>
        public static void Load(UniversalSkinElementsLoadFlags loadFlags = UniversalSkinElementsLoadFlags.All)
        {
            var previousV2 = SkinV2;
            Skin = new SkinStore(loadFlags: loadFlags);
            SkinV2 = new SkinStoreV2(Skin.Dir);
            previousV2?.Retire();

            if (ConfigManager.TournamentPlayer2Skin.Value == null ||
                ConfigManager.TournamentPlayer2Skin.Value == ConfigManager.Skin.Value)
            {
                TournamentPlayer2Skin = Skin;
            }
            else
            {
                TournamentPlayer2Skin = new SkinStore(ConfigManager.TournamentPlayer2Skin.Value);
            }
            
            LoadEditorSkin();
        }

        public static SkinStoreV2Lease AcquireV2()
        {
            if (SkinV2 == null)
                SkinV2 = new SkinStoreV2(Skin?.Dir ?? ConfigManager.SkinDirectory.Value);

            return SkinV2.Acquire();
        }

        public static void LoadEditorSkin()
        {
            var isSameDefaultSkin = ConfigManager.DefaultSkin.Value == ConfigManager.DefaultEditorSkin.Value;
            
            if (isSameDefaultSkin && (ConfigManager.EditorNoteSkin.Value == null ||
                ConfigManager.EditorNoteSkin.Value == ConfigManager.Skin.Value))
            {
                EditorSkin = Skin;
            }
            else if (isSameDefaultSkin && ConfigManager.EditorNoteSkin.Value == ConfigManager.TournamentPlayer2Skin.Value)
            {
                EditorSkin = TournamentPlayer2Skin;
            }
            else
            {
                EditorSkin = new SkinStore(ConfigManager.EditorNoteSkin.Value, true);
            }
        }

        /// <summary>
        ///     Start watching for changes in skin.ini and skin.yml.
        /// </summary>
        public static void StartWatching()
        {
            if (!ConfigManager.ReloadSkinOnChange.Value)
                return;

            if (Watcher != null)
            {
                Logger.Important("Skin manager resumed watching skin configuration files", LogType.Runtime);
                Watcher.EnableRaisingEvents = true;
                return;
            }

            if (!Directory.Exists(Skin.Dir))
                return;

            Logger.Important("Skin manager started watching skin.ini and skin.yml", LogType.Runtime);
            Watcher = new FileSystemWatcher(Skin.Dir)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };

            void QueueReload(string name)
            {
                if (!string.Equals(name, "skin.ini", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(name, "skin.yml", StringComparison.OrdinalIgnoreCase))
                    return;
                if (TimeSkinReloadRequested != 0) return;
                Logger.Important($"Skin change detected. Reloading", LogType.Runtime);
                TimeSkinReloadRequested = GameBase.Game.TimeRunning;
                if (Skin.Dir.IsSubDirectoryOf(ConfigManager.SteamWorkshopDirectory.Value))
                {
                    NotificationManager.Show(NotificationLevel.Warning,
                        $"You are making changes to a workshop skin! Changes to workshop skins might be lost when the skin updates!",
                        forceShow: true);
                }
            }

            Watcher.Changed += (sender, args) => QueueReload(args.Name);
            Watcher.Created += (sender, args) => QueueReload(args.Name);
            Watcher.Deleted += (sender, args) => QueueReload(args.Name);
            Watcher.Renamed += (sender, args) =>
            {
                QueueReload(args.Name);
                QueueReload(args.OldName);
            };
        }

        /// <summary>
        ///     Stops watching skin configuration files.
        /// </summary>
        public static void StopWatching()
        {
            if (Watcher == null) return;
            Logger.Important("Skin manager stopped watching skin configuration files", LogType.Runtime);
            // Dispose of the watcher
            Watcher.Dispose();
            Watcher = null;
        }

        /// <summary>
        /// </summary>
        private static bool SkinExportInProgress { get; set; }

        /// <summary>
        ///     Event raised when the user's skin has reloaded.
        /// </summary>
        public static event EventHandler<SkinReloadedEventArgs> SkinLoaded;

        /// <summary>
        ///     Event raised when the user's skin has reloaded.
        /// </summary>
        public static event EventHandler<SkinReloadedEventArgs> EditorSkinLoaded;

        /// <summary>
        ///     Called every frame. Waits for a skin reload to be queued up.
        /// </summary>
        public static void HandleSkinReloading()
        {
            if (TimeEditorSkinReloadRequested != 0 && GameBase.Game.TimeRunning - TimeEditorSkinReloadRequested >= 400)
            {
                LoadEditorSkin();
                TimeEditorSkinReloadRequested = 0;
                EditorSkinLoaded?.Invoke(typeof(SkinManager), new SkinReloadedEventArgs());
                
                ThreadScheduler.RunAfter(Transitioner.FadeOut, 300);
                return;
            }

            // Reload skin when applicable
            if (TimeSkinReloadRequested != 0 && GameBase.Game.TimeRunning - TimeSkinReloadRequested >= 400)
            {
                Load();
                TimeSkinReloadRequested = 0;
                SkinLoaded?.Invoke(typeof(SkinManager), new SkinReloadedEventArgs());
                EditorSkinLoaded?.Invoke(typeof(SkinManager), new SkinReloadedEventArgs());
                var showLoadedNotification = true;

                var game = (QuaverGame)GameBase.Game;

                switch (game.CurrentScreen.Type)
                {
                    case QuaverScreenType.Menu:
                        game.CurrentScreen.Exit(() => QuaverScreenFactory.CreateMainMenu());
                        break;
                    case QuaverScreenType.Select:
                        if (game.CurrentScreen is ISelectionScreenState selectionScreen)
                        {
                            var activeScrollContainer = selectionScreen.ActiveScrollContainer;
                            var activeLeftPanel = selectionScreen.ActiveLeftPanel;
                            game.CurrentScreen.Exit(() => QuaverScreenFactory.CreateSelection(activeScrollContainer, activeLeftPanel));
                        }
                        else
                        {
                            game.CurrentScreen.Exit(() => QuaverScreenFactory.CreateSelection());
                        }
                        break;
                    case QuaverScreenType.Gameplay when
                        game.CurrentScreen is GameplayScreen gameplayScreen and not TournamentGameplayScreen
                        && gameplayScreen.InReplayMode:
                        showLoadedNotification = ConfigManager.DisplayNotificationsInGameplay.Value;
                        game.CurrentScreen.Exit(() =>
                        {
                            var newScreen = new GameplayScreen(gameplayScreen.Map, gameplayScreen.MapHash,
                                gameplayScreen.LocalScores, gameplayScreen.LoadedReplay,
                                spectatorClient: gameplayScreen.SpectatorClient,
                                useExistingAudioTime: true);
                            newScreen.HandleReplaySeeking();
                            return newScreen;
                        });
                        break;
                }

                ThreadScheduler.RunAfter(() =>
                {
                    Transitioner.FadeOut();
                    if (showLoadedNotification)
                    {
                        if (SkinV2.Warnings.Count > 0)
                        {
                            NotificationManager.Show(NotificationLevel.Warning,
                                $"Skin loaded with {SkinV2.Warnings.Count} V2 warning(s). Defaults were used where needed; check Runtime.log.",
                                forceShow: true);
                        }
                        else
                        {
                            NotificationManager.Show(NotificationLevel.Success, "Skin has been successfully loaded!",
                                forceShow: true);
                        }
                    }
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
