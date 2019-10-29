/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IniFileParser;
using IniFileParser.Model;
using ManagedBass;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.Shared.Graphics.Overlays.Chat.Components.Users;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Editor.UI.Graphing;
using Quaver.Shared.Screens.Select.UI.Leaderboard;
using Wobble;
using Wobble.Bindables;
using Wobble.Logging;

namespace Quaver.Shared.Config
{
    public static class ConfigManager
    {
        /// <summary>
        ///     These are all values that should never ben
        /// </summary>
        private static string _gameDirectory;
        internal static Bindable<string> GameDirectory { get; private set; }

        /// <summary>
        ///     The skin directory
        /// </summary>
        private static string _skinDirectory;
        internal static Bindable<string> SkinDirectory { get; private set; }

        /// <summary>
        ///     The screenshot directory
        /// </summary>
        private static string _screenshotDirectory;
        internal static Bindable<string> ScreenshotDirectory { get; private set; }

        /// <summary>
        ///     The replay directory
        /// </summary>
        private static string _replayDirectory;
        internal static Bindable<string> ReplayDirectory { get; private set; }

        /// <summary>
        ///     The Logs directory
        /// </summary>
        private static string _logsDirectory;
        internal static Bindable<string> LogsDirectory { get; private set; }

        /// <summary>
        ///     The data directory
        /// </summary>
        private static string _dataDirectory;
        internal static Bindable<string> DataDirectory { get; private set; }

        /// <summary>
        ///     The song directory
        /// </summary>
        private static string _songDirectory;
        internal static Bindable<string> SongDirectory { get; private set; }

        /// <summary>
        ///     The username of the user.
        /// </summary>
        internal static Bindable<string> Username { get; private set; }

        /// <summary>
        ///     The skin in the Skins directory that is loaded. Default is the only exception, as it'll be overrided.
        /// </summary>
        internal static Bindable<string> Skin { get; private set; }

        /// <summary>
        ///     The default skin that will be loaded if the skin property is blank
        /// </summary>
        internal static Bindable<DefaultSkins> DefaultSkin { get; private set; }

        /// <summary>
        ///     The master volume of the game.
        /// </summary>
        internal static BindableInt VolumeGlobal { get; private set; }

        /// <summary>
        ///     The SFX volume of the game.
        /// </summary>
        internal static BindableInt VolumeEffect { get; private set; }

        /// <summary>
        ///     The Music volume of the gamne.
        /// </summary>
        internal static BindableInt VolumeMusic { get; private set; }

        /// <summary>
        ///     The BASS device period.
        /// </summary>
        internal static BindableInt DevicePeriod { get; private set; }

        /// <summary>
        ///     The BASS device buffer length divided by DevicePeriod.
        /// </summary>
        internal static BindableInt DeviceBufferLengthMultiplier { get; private set; }

        /// <summary>
        ///     The dim for backgrounds during gameplay
        /// </summary>
        internal static BindableInt BackgroundBrightness { get; private set; }

        /// <summary>
        ///     The height of the window.
        /// </summary>
        internal static BindableInt WindowHeight { get; private set; }

        /// <summary>
        ///     The width of the window.
        /// </summary>
        internal static BindableInt WindowWidth { get; private set; }

        /// <summary>
        ///     Is the window fullscreen?
        /// </summary>
        internal static Bindable<bool> WindowFullScreen { get; private set; }

        /// <summary>
        ///     Should the game display the FPS Counter?
        /// </summary>
        internal static Bindable<bool> FpsCounter { get; private set; }

        /// <summary>
        ///     The type of FPS limiter that is activated
        /// </summary>
        internal static Bindable<FpsLimitType> FpsLimiterType { get; private set; }

        /// <summary>
        ///     The custom value for FPS limiting
        /// </summary>
        internal static BindableInt CustomFpsLimit { get; private set; }

        /// <summary>
        ///     Determines if we should show the song time progress display in the
        ///     gameplay screen.
        /// </summary>
        internal static Bindable<bool> DisplaySongTimeProgress { get; private set; }

        /// <summary>
        ///     The scroll speed for mania 4k
        /// </summary>
        internal static BindableInt ScrollSpeed4K { get; private set; }

        /// <summary>
        ///     The scroll speed for mania 7k
        /// </summary>
        internal static BindableInt ScrollSpeed7K { get; private set; }

        /// <summary>
        ///     Direction in which hit objects will be moving for 4K gamemode
        /// </summary>
        internal static Bindable<ScrollDirection> ScrollDirection4K { get; private set; }

        /// <summary>
        ///     Direction in which hit objects will be moving for 7K gamemode
        /// </summary>
        internal static Bindable<ScrollDirection> ScrollDirection7K { get; private set; }

        /// <summary>
        ///     The offset of the notes compared to the song start.
        /// </summary>
        internal static BindableInt GlobalAudioOffset { get; private set; }

        /// <summary>
        ///     Dictates whether or not the song audio is pitched while using the ManiaModSpeed gameplayModifier.
        /// </summary>
        internal static Bindable<bool> Pitched { get; private set; }

        /// <summary>
        ///     The path of the osu!.db file
        /// </summary>
        internal static Bindable<string> OsuDbPath { get; private set; }

        /// <summary>
        ///     Dictates where or not we should load osu! maps from osu!.db on game start
        /// </summary>
        internal static Bindable<bool> AutoLoadOsuBeatmaps { get; private set; }

        /// <summary>
        ///     If the scoreboard is currently visible.
        /// </summary>
        internal static Bindable<bool> ScoreboardVisible { get; private set; }

        /// <summary>
        ///     Dictates how to order the mapsets during song select.
        /// </summary>
        internal static Bindable<OrderMapsetsBy> SelectOrderMapsetsBy { get; private set; }

        /// <summary>
        ///     The currently selected game mode.
        /// </summary>
        internal static Bindable<GameMode> SelectedGameMode { get; private set; }

        /// <summary>
        ///     How the user is currently filtering their online users.
        /// </summary>
        internal static Bindable<OnlineUserFilterType> SelectedOnlineUserFilterType { get; private set; }

        /// <summary>
        ///     The type of leaderboard that is displayed during song select.
        /// </summary>
        internal static Bindable<LeaderboardType> LeaderboardSection { get; private set; }

        /// <summary>
        ///     If true, the user will be auto logged into the server.
        /// </summary>
        internal static Bindable<bool> AutoLoginToServer { get; private set; }

        /// <summary>
        ///     If true, timing lines will be displayed during gameplay
        /// </summary>
        internal static Bindable<bool> DisplayTimingLines { get; private set; }

        /// <summary>
        ///     If true, the audio visualizer in the menus will be displayed.
        /// </summary>
        internal static Bindable<bool> DisplayMenuAudioVisualizer { get; private set; }

        /// <summary>
        ///     If true, hitsounds in gameplay will be played.
        /// </summary>
        internal static Bindable<bool> EnableHitsounds { get; private set; }

        /// <summary>
        ///     If true, keysounds in gameplay will be played.
        /// </summary>
        internal static Bindable<bool> EnableKeysounds { get; private set; }

        /// <summary>
        ///     If enabled, the user's background will be blurred in gameplay.
        /// </summary>
        internal static Bindable<bool> BlurBackgroundInGameplay { get; private set; }

        /// <summary>
        ///     If enabled, the user will be able to tap to pause instead of having to hold for 500ms to pause.
        /// </summary>
        internal static Bindable<bool> TapToPause { get; private set; }

        /// <summary>
        ///     The top lane cover's adjustable height between levels 0-50
        /// </summary>
        internal static BindableInt LaneCoverTopHeight { get; private set; }

        /// <summary>
        ///     The bottom lane cover's adjustable height between levels 0-50
        /// </summary>
        internal static BindableInt LaneCoverBottomHeight { get; private set; }

        /// <summary>
        ///     If enabled, gameplay will have a top lane cover using the adjustable height.
        /// </summary>
        internal static Bindable<bool> LaneCoverTop { get; private set; }

        /// <summary>
        ///     If enabled, gameplay will have a bottom lane cover using the adjustable height.
        /// </summary>
        internal static Bindable<bool> LaneCoverBottom { get; private set; }

        /// <summary>
        ///     If enabled, the lane covers will be displayed under the ui elements.
        /// </summary>
        internal static Bindable<bool> UIElementsOverLaneCover { get; private set; }

        /// <summary>
        ///     If enabled, failed scores will not show in local scores.
        /// </summary>
        internal static Bindable<bool> DisplayFailedLocalScores { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> DisplayComboAlerts { get; private set; }

        /// <summary>
        ///     If enabled, accuracy display is smoothed.
        /// </summary>
        internal static Bindable<bool> SmoothAccuracyChanges { get; private set; }

        /// <summary>
        ///     The scroll speed used in the editor.
        /// </summary>
        internal static BindableInt EditorScrollSpeedKeys { get; private set; }

        /// <summary>
        ///     Whether or not to play hitsounds in the editor.
        /// </summary>
        internal static Bindable<bool> EditorEnableHitsounds { get; private set; }

        /// <summary>
        ///     Whether or not to play keysounds in the editor.
        /// </summary>
        internal static Bindable<bool> EditorEnableKeysounds { get; private set; }

        /// <summary>
        ///     The type of beat snap colors that'll be displayed in the editor.
        /// </summary>
        internal static Bindable<EditorBeatSnapColor> EditorBeatSnapColorType { get; private set; }

        /// <summary>
        ///     Whether or not the user only wants to display measure lines while editing.
        /// </summary>
        internal static Bindable<bool> EditorOnlyShowMeasureLines { get; private set; }

        /// <summary>
        ///     Whether or not the user would like to display the lines that divide the lanes.
        /// </summary>
        internal static Bindable<bool> EditorShowLaneDividerLines { get; private set; }

        /// <summary>
        ///     Anchors HitObjects to the middle, so the snap lines are in the middle of the object.
        /// </summary>
        internal static Bindable<bool> EditorHitObjectsMidpointAnchored { get; private set; }

        /// <summary>
        ///     Whether or jot the user wants to play the metronome in the editor
        /// </summary>
        internal static Bindable<bool> EditorPlayMetronome { get; private set; }

        /// <summary>
        ///     If the metronome in the editor will play half beats.
        /// </summary>
        internal static Bindable<bool> EditorMetronomePlayHalfBeats { get; private set; }

        /// <summary>
        ///     If true, it'll display the numbers for the song time progress
        /// </summary>
        internal static Bindable<bool> DisplaySongTimeProgressNumbers { get; private set; }

        /// <summary>
        ///
        /// </summary>
        internal static Bindable<bool> DisplayJudgementCounter { get; private set; }

        /// <summary></summary>
        ///     If true, the user will skip the results screen after quitting the game.
        /// </summary>
        internal static Bindable<bool> SkipResultsScreenAfterQuit { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<EditorVisualizationGraphType> EditorVisualizationGraph { get; private set; }

        /// <summary>
        ///     If true, it'll use hitobjects specifically for viewing layers in the editor.
        /// </summary>
        internal static Bindable<bool> EditorViewLayers { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> LobbyFilterHasPassword { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> LobbyFilterFullGame { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> LobbyFilterOwnsMap { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> LobbyFilterHasFriends { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> EnableBattleRoyaleBackgroundFlashing { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> EnableBattleRoyaleAlerts { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> ShowSpectators { get; private set; }

        /// <summary>
        ///     The selected judgement window preset
        /// </summary>
        internal static Bindable<string> JudgementWindows { get; private set; }

        /// <summary>
        ///     Keybinding for leftward navigation.
        /// </summary>
        internal static Bindable<Keys> KeyNavigateLeft { get; private set; }

        /// <summary>
        ///     Keybinding for rightward navigation.
        /// </summary>
        internal static Bindable<Keys> KeyNavigateRight { get; private set; }

        /// <summary>
        ///     Keybinding for upward navigation.
        /// </summary>
        internal static Bindable<Keys> KeyNavigateUp { get; private set; }

        /// <summary>
        ///     Keybinding for downward navigation.
        /// </summary>
        internal static Bindable<Keys> KeyNavigateDown { get; private set; }

        /// <summary>
        ///     Keybinding for backward navigation.
        /// </summary>
        internal static Bindable<Keys> KeyNavigateBack { get; private set; }

        /// <summary>
        ///     Keybinding for selection in navigation interface.
        /// </summary>
        internal static Bindable<Keys> KeyNavigateSelect { get; private set; }

        /// <summary>
        ///     Keybindings for 4K
        /// </summary>
        internal static Bindable<Keys> KeyMania4K1 { get; private set; }
        internal static Bindable<Keys> KeyMania4K2 { get; private set; }
        internal static Bindable<Keys> KeyMania4K3 { get; private set; }
        internal static Bindable<Keys> KeyMania4K4 { get; private set; }

        /// <summary>
        ///     Keybindings for 7K
        /// </summary>
        internal static Bindable<Keys> KeyMania7K1 { get; private set; }

        internal static Bindable<Keys> KeyMania7K2 { get; private set; }
        internal static Bindable<Keys> KeyMania7K3 { get; private set; }
        internal static Bindable<Keys> KeyMania7K4 { get; private set; }
        internal static Bindable<Keys> KeyMania7K5 { get; private set; }
        internal static Bindable<Keys> KeyMania7K6 { get; private set; }
        internal static Bindable<Keys> KeyMania7K7 { get; private set; }

        /// <summary>
        ///     The key pressed to pause and menu-back.
        /// </summary>
        internal static Bindable<Keys> KeyPause { get; private set; }

        /// <summary>
        ///     The key pressed to skip the song introduction
        /// </summary>
        internal static Bindable<Keys> KeySkipIntro { get; private set; }

        /// <summary>
        ///     The key to toggle the overlay
        /// </summary>
        internal static Bindable<Keys> KeyToggleOverlay { get; private set; }

        /// <summary>
        ///     The key pressed to restart the map.
        /// </summary>
        internal static Bindable<Keys> KeyRestartMap { get; private set; }

        /// <summary>
        ///     The keys to increase/decrease scroll speed.
        /// </summary>
        internal static Bindable<Keys> KeyIncreaseScrollSpeed { get; private set; }
        internal static Bindable<Keys> KeyDecreaseScrollSpeed { get; private set; }

        /// <summary>
        ///     The keys to increase/decrease map offset.
        /// </summary>
        internal static Bindable<Keys> KeyIncreaseMapOffset { get; private set; }
        internal static Bindable<Keys> KeyDecreaseMapOffset { get; private set; }

        /// <summary>
        ///     The key to hide the scoreboard in-game.
        /// </summary>
        internal static Bindable<Keys> KeyScoreboardVisible { get; private set; }

        /// <summary>
        ///     The key to quickly exit the map.
        /// </summary>
        internal static Bindable<Keys> KeyQuickExit { get; private set; }

        /// <summary>
        ///     The key to pause/play the track in the editor.
        /// </summary>
        internal static Bindable<Keys> KeyEditorPausePlay { get; private set; }

        /// <summary>
        ///     The key to lower the audio rate in the editor.
        /// </summary>
        internal static Bindable<Keys> KeyEditorDecreaseAudioRate { get; private set; }

        /// <summary>
        ///     The key to increase the audio rate in the editor.
        /// </summary>
        internal static Bindable<Keys> KeyEditorIncreaseAudioRate { get; private set; }

        /// <summary>
        ///     Dictates whether or not this is the first write of the file for the current game session.
        ///     (Not saved in Config)
        /// </summary>
        private static bool FirstWrite { get; set; }

        /// <summary>
        ///     The last time we've wrote config.
        /// </summary>
        private static long LastWrite { get; set; }

        /// <summary>
        ///     Important!
        ///     Responsible for initializing directory properties,
        ///     writing a new config file if it doesn't exist and also reading config files.
        ///     This should be the one of the first things that is called upon game launch.
        /// </summary>
        public static void Initialize()
        {
            // When initializing, we manually set the directory fields rather than the props,
            // because we only want to write the config file one time at this stage.
            // Usually when a property is modified, it will automatically write the config file again,
            // so that's what we're preventing here.
            _gameDirectory = Directory.GetCurrentDirectory();

            _skinDirectory = _gameDirectory + "/Skins";
            Directory.CreateDirectory(_skinDirectory);

            _screenshotDirectory = _gameDirectory + "/Screenshots";
            Directory.CreateDirectory(_screenshotDirectory);

            _logsDirectory = _gameDirectory + "/Logs";
            Directory.CreateDirectory(_logsDirectory);

            _replayDirectory = _gameDirectory + "/Replays";
            Directory.CreateDirectory(_replayDirectory);

            _dataDirectory = _gameDirectory + "/Data";
            Directory.CreateDirectory(_dataDirectory);
            Directory.CreateDirectory(_dataDirectory + "/r/");

            _songDirectory = _gameDirectory + "/Songs";
            Directory.CreateDirectory(_songDirectory);

            // If we already have a config file, we'll just want to read that.
            ReadConfigFile();
            Logger.Important("Config file has been successfully read.", LogType.Runtime);
        }

        /// <summary>
        ///     Reads a quaver.cfg file and sets all of the successfully read values.
        ///     At the end of reading, we write the config file, changing any invalid data/
        /// </summary>
        private static void ReadConfigFile()
        {
            // We'll want to write a quaver.cfg file if it doesn't already exist.
            // There's no need to read the config file afterwards, since we already have
            // all of the default values.
            if (!File.Exists(_gameDirectory + "/quaver.cfg"))
                File.WriteAllText(_gameDirectory + "/quaver.cfg", "; Quaver Configuration File");

            var data = new IniFileParser.IniFileParser(new ConcatenateDuplicatedKeysIniDataParser()).ReadFile(_gameDirectory + "/quaver.cfg")["Config"];

            // Read / Set Config Values
            // NOTE: MAKE SURE TO SET THE VALUE TO AUTO-SAVE WHEN CHANGING! THIS ISN'T DONE AUTOMATICALLY.
            // YOU CAN DO THIS DOWN BELOW, AFTER THE CONFIG HAS WRITTEN FOR THE FIRST TIME.
            GameDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"GameDirectory", _gameDirectory, data);
            SkinDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"SkinDirectory", _skinDirectory, data);
            ScreenshotDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"ScreenshotDirectory",
                _screenshotDirectory, data);
            ReplayDirectory =
                ReadSpecialConfigType(SpecialConfigType.Directory, @"ReplayDirectory", _replayDirectory, data);
            LogsDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"LogsDirectory", _logsDirectory, data);
            DataDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"DataDirectory", _dataDirectory, data);
            SongDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"SongDirectory", _songDirectory, data);
            SelectedGameMode = ReadValue(@"SelectedGameMode", GameMode.Keys4, data);
            Username = ReadValue(@"Username", "Player", data);
            VolumeGlobal = ReadInt(@"VolumeGlobal", 50, 0, 100, data);
            VolumeEffect = ReadInt(@"VolumeEffect", 20, 0, 100, data);
            VolumeMusic = ReadInt(@"VolumeMusic", 50, 0, 100, data);
            DevicePeriod = ReadInt(@"DevicePeriod", Bass.GetConfig(Configuration.DevicePeriod), 1, 100, data);
            DeviceBufferLengthMultiplier = ReadInt(@"DeviceBufferLengthMultiplier", Bass.GetConfig(Configuration.DeviceBufferLength) / DevicePeriod.Value, 2, 10, data);
            BackgroundBrightness = ReadInt(@"BackgroundBrightness", 50, 0, 100, data);
            WindowHeight = ReadInt(@"WindowHeight", 768, 600, short.MaxValue, data);
            WindowWidth = ReadInt(@"WindowWidth", 1366, 800, short.MaxValue, data);
            DisplaySongTimeProgress = ReadValue(@"DisplaySongTimeProgress", true, data);
            WindowFullScreen = ReadValue(@"WindowFullScreen", false, data);
            FpsCounter = ReadValue(@"FpsCounter", false, data);
            FpsLimiterType = ReadValue(@"FpsLimiterType", FpsLimitType.Unlimited, data);
            CustomFpsLimit = ReadInt(@"CustomFpsLimit", 240, 60, int.MaxValue, data);
            ScrollSpeed4K = ReadInt(@"ScrollSpeed4K", 15, 5, 100, data);
            ScrollSpeed7K = ReadInt(@"ScrollSpeed7K", 15, 5, 100, data);
            ScrollDirection4K = ReadValue(@"ScrollDirection4K", ScrollDirection.Down, data);
            ScrollDirection7K = ReadValue(@"ScrollDirection7K", ScrollDirection.Down, data);
            GlobalAudioOffset = ReadInt(@"GlobalAudioOffset", 0, -300, 300, data);
            Skin = ReadSpecialConfigType(SpecialConfigType.Skin, @"Skin", "", data);
            DefaultSkin = ReadValue(@"DefaultSkin", DefaultSkins.Bar, data);
            Pitched = ReadValue(@"Pitched", true, data);
            ScoreboardVisible = ReadValue(@"ScoreboardVisible", true, data);
            SelectOrderMapsetsBy = ReadValue(@"SelectOrderMapsetsBy", OrderMapsetsBy.Artist, data);
            SelectedOnlineUserFilterType = ReadValue(@"OnlineUserFilterType", OnlineUserFilterType.All, data);
            LeaderboardSection = ReadValue(@"LeaderboardSection", LeaderboardType.Local, data);
            OsuDbPath = ReadSpecialConfigType(SpecialConfigType.Path, @"OsuDbPath", "", data);
            AutoLoadOsuBeatmaps = ReadValue(@"AutoLoadOsuBeatmaps", false, data);
            AutoLoginToServer = ReadValue(@"AutoLoginToServer", true, data);
            DisplayTimingLines = ReadValue(@"DisplayTimingLines", true, data);
            DisplayMenuAudioVisualizer = ReadValue(@"DisplayMenuAudioVisualizer", true, data);
            EnableHitsounds = ReadValue(@"EnableHitsounds", true, data);
            EnableKeysounds = ReadValue(@"EnableKeysounds", true, data);
            KeyNavigateLeft = ReadValue(@"KeyNavigateLeft", Keys.Left, data);
            KeyNavigateRight = ReadValue(@"KeyNavigateRight", Keys.Right, data);
            KeyNavigateUp = ReadValue(@"KeyNavigateUp", Keys.Up, data);
            KeyNavigateDown = ReadValue(@"KeyNavigateDown", Keys.Down, data);
            KeyNavigateBack = ReadValue(@"KeyNavigateBack", Keys.Escape, data);
            KeyNavigateSelect = ReadValue(@"KeyNavigateSelect", Keys.Enter, data);
            KeyMania4K1 = ReadValue(@"KeyMania4K1", Keys.A, data);
            KeyMania4K2 = ReadValue(@"KeyMania4K2", Keys.S, data);
            KeyMania4K3 = ReadValue(@"KeyMania4K3", Keys.K, data);
            KeyMania4K4 = ReadValue(@"KeyMania4K4", Keys.L, data);
            KeyMania7K1 = ReadValue(@"KeyMania7K1", Keys.A, data);
            KeyMania7K2 = ReadValue(@"KeyMania7K2", Keys.S, data);
            KeyMania7K3 = ReadValue(@"KeyMania7K3", Keys.D, data);
            KeyMania7K4 = ReadValue(@"KeyMania7K4", Keys.Space, data);
            KeyMania7K5 = ReadValue(@"KeyMania7K5", Keys.J, data);
            KeyMania7K6 = ReadValue(@"KeyMania7K6", Keys.K, data);
            KeyMania7K7 = ReadValue(@"KeyMania7K7", Keys.L, data);
            KeySkipIntro = ReadValue(@"KeySkipIntro", Keys.RightAlt, data);
            KeyPause = ReadValue(@"KeyPause", Keys.Escape, data);
            KeyToggleOverlay = ReadValue(@"KeyToggleOverlay", Keys.F8, data);
            KeyRestartMap = ReadValue(@"KeyRestartMap", Keys.OemTilde, data);
            KeyDecreaseScrollSpeed = ReadValue(@"KeyDecreaseScrollSpeed", Keys.F3, data);
            KeyIncreaseScrollSpeed = ReadValue(@"KeyIncreaseScrollSpeed", Keys.F4, data);
            KeyDecreaseMapOffset = ReadValue(@"KeyDecreaseMapOffset", Keys.OemMinus, data);
            KeyIncreaseMapOffset = ReadValue(@"KeyIncreaseMapOffset", Keys.OemPlus, data);
            KeyScoreboardVisible = ReadValue(@"KeyHideScoreboard", Keys.Tab, data);
            KeyQuickExit = ReadValue(@"KeyQuickExit", Keys.F1, data);
            BlurBackgroundInGameplay = ReadValue(@"BlurBackgroundInGameplay", false, data);
            TapToPause = ReadValue(@"TapToPause", false, data);
            DisplayFailedLocalScores = ReadValue(@"DisplayFailedLocalScores", true, data);
            EditorScrollSpeedKeys = ReadInt(@"EditorScrollSpeedKeys", 16, 5, 100, data);
            KeyEditorPausePlay = ReadValue(@"KeyEditorPausePlay", Keys.Space, data);
            KeyEditorDecreaseAudioRate = ReadValue(@"KeyEditorDecreaseAudioRate", Keys.OemMinus, data);
            KeyEditorIncreaseAudioRate = ReadValue(@"KeyEditorIncreaseAudioRate", Keys.OemPlus, data);
            EditorEnableHitsounds = ReadValue(@"EditorEnableHitsounds", true, data);
            EditorEnableKeysounds = ReadValue(@"EditorEnableKeysounds", true, data);
            EditorBeatSnapColorType = ReadValue(@"EditorBeatSnapColorType", EditorBeatSnapColor.Default, data);
            EditorOnlyShowMeasureLines = ReadValue(@"EditorOnlyShowMeasureLines", false, data);
            EditorShowLaneDividerLines = ReadValue(@"EditorShowDividerLines", true, data);
            EditorHitObjectsMidpointAnchored = ReadValue(@"EditorHitObjectsMidpointAnchored", false, data);
            EditorPlayMetronome = ReadValue(@"EditorPlayMetronome", true, data);
            EditorMetronomePlayHalfBeats = ReadValue(@"EditorMetronomePlayHalfBeats", false, data);
            DisplaySongTimeProgressNumbers = ReadValue(@"DisplaySongTimeProgressNumbers", true, data);
            DisplayJudgementCounter = ReadValue(@"DisplayJudgementCounter", true, data);
            SkipResultsScreenAfterQuit = ReadValue(@"SkipResultsScreenAfterQuit", false, data);
            DisplayComboAlerts = ReadValue(@"DisplayComboAlerts", true, data);
            LaneCoverTopHeight = ReadInt(@"LaneCoverTopHeight", 25, 0, 75, data);
            LaneCoverBottomHeight = ReadInt(@"LaneCoverBottomHeight", 25, 0, 75, data);
            LaneCoverTop = ReadValue(@"LaneCoverTop", false, data);
            LaneCoverBottom = ReadValue(@"LaneCoverBottom", false, data);
            UIElementsOverLaneCover = ReadValue(@"UIElementsOverLaneCover", true, data);
            SmoothAccuracyChanges = ReadValue(@"SmoothAccuracyChanges", true, data);
            EditorVisualizationGraph = ReadValue(@"EditorVisualizationGraph", EditorVisualizationGraphType.Tick, data);
            EditorViewLayers = ReadValue(@"EditorViewLayers", false, data);
            LobbyFilterHasPassword = ReadValue(@"LobbyFilterHasPassword", true, data);
            LobbyFilterFullGame = ReadValue(@"LobbyFilterFullGame", false, data);
            LobbyFilterOwnsMap = ReadValue(@"LobbyFilterOwnsMap", false, data);
            LobbyFilterHasFriends = ReadValue(@"LobbyFilterHasFriends", false, data);
            EnableBattleRoyaleBackgroundFlashing = ReadValue(@"EnableBattleRoyaleBackgroundFlashing", true, data);
            EnableBattleRoyaleAlerts = ReadValue(@"EnableBattleRoyaleAlerts", true, data);
            ShowSpectators = ReadValue(@"ShowSpectators", true, data);
            JudgementWindows = ReadValue("JudgementWindows", "", data);

            // Have to do this manually.
            if (string.IsNullOrEmpty(Username.Value))
                Username.Value = "Player";

            // Write the config file with all of the changed/invalidated data.
            Task.Run(async () => await WriteConfigFileAsync())
                .ContinueWith(t =>
                {
                    // SET AUTO-SAVE FUNCTIONALITY FOR EACH BINDED VALUE.
                    // This is so shit tbcfh, lol.
                    GameDirectory.ValueChanged += AutoSaveConfiguration;
                    SkinDirectory.ValueChanged += AutoSaveConfiguration;
                    ScreenshotDirectory.ValueChanged += AutoSaveConfiguration;
                    ReplayDirectory.ValueChanged += AutoSaveConfiguration;
                    LogsDirectory.ValueChanged += AutoSaveConfiguration;
                    DataDirectory.ValueChanged += AutoSaveConfiguration;
                    SongDirectory.ValueChanged += AutoSaveConfiguration;
                    OsuDbPath.ValueChanged += AutoSaveConfiguration;
                    AutoLoadOsuBeatmaps.ValueChanged += AutoSaveConfiguration;
                    Username.ValueChanged += AutoSaveConfiguration;
                    VolumeGlobal.ValueChanged += AutoSaveConfiguration;
                    VolumeEffect.ValueChanged += AutoSaveConfiguration;
                    VolumeMusic.ValueChanged += AutoSaveConfiguration;
                    DevicePeriod.ValueChanged += AutoSaveConfiguration;
                    DeviceBufferLengthMultiplier.ValueChanged += AutoSaveConfiguration;
                    BackgroundBrightness.ValueChanged += AutoSaveConfiguration;
                    WindowHeight.ValueChanged += AutoSaveConfiguration;
                    WindowWidth.ValueChanged += AutoSaveConfiguration;
                    WindowFullScreen.ValueChanged += AutoSaveConfiguration;
                    FpsCounter.ValueChanged += AutoSaveConfiguration;
                    FpsLimiterType.ValueChanged += AutoSaveConfiguration;
                    CustomFpsLimit.ValueChanged += AutoSaveConfiguration;
                    DisplaySongTimeProgress.ValueChanged += AutoSaveConfiguration;
                    ScrollSpeed4K.ValueChanged += AutoSaveConfiguration;
                    ScrollSpeed7K.ValueChanged += AutoSaveConfiguration;
                    ScrollDirection4K.ValueChanged += AutoSaveConfiguration;
                    ScrollDirection7K.ValueChanged += AutoSaveConfiguration;
                    GlobalAudioOffset.ValueChanged += AutoSaveConfiguration;
                    Skin.ValueChanged += AutoSaveConfiguration;
                    DefaultSkin.ValueChanged += AutoSaveConfiguration;
                    Pitched.ValueChanged += AutoSaveConfiguration;
                    ScoreboardVisible.ValueChanged += AutoSaveConfiguration;
                    AutoLoginToServer.ValueChanged += AutoSaveConfiguration;
                    DisplayTimingLines.ValueChanged += AutoSaveConfiguration;
                    DisplayMenuAudioVisualizer.ValueChanged += AutoSaveConfiguration;
                    EnableHitsounds.ValueChanged += AutoSaveConfiguration;
                    EnableKeysounds.ValueChanged += AutoSaveConfiguration;
                    KeyNavigateLeft.ValueChanged += AutoSaveConfiguration;
                    KeyNavigateRight.ValueChanged += AutoSaveConfiguration;
                    KeyNavigateUp.ValueChanged += AutoSaveConfiguration;
                    KeyNavigateDown.ValueChanged += AutoSaveConfiguration;
                    KeyNavigateBack.ValueChanged += AutoSaveConfiguration;
                    KeyNavigateSelect.ValueChanged += AutoSaveConfiguration;
                    KeyMania4K1.ValueChanged += AutoSaveConfiguration;
                    KeyMania4K2.ValueChanged += AutoSaveConfiguration;
                    KeyMania4K3.ValueChanged += AutoSaveConfiguration;
                    KeyMania4K4.ValueChanged += AutoSaveConfiguration;
                    KeyMania7K1.ValueChanged += AutoSaveConfiguration;
                    KeyMania7K2.ValueChanged += AutoSaveConfiguration;
                    KeyMania7K3.ValueChanged += AutoSaveConfiguration;
                    KeyMania7K4.ValueChanged += AutoSaveConfiguration;
                    KeyMania7K5.ValueChanged += AutoSaveConfiguration;
                    KeyMania7K6.ValueChanged += AutoSaveConfiguration;
                    KeyMania7K7.ValueChanged += AutoSaveConfiguration;
                    KeySkipIntro.ValueChanged += AutoSaveConfiguration;
                    KeyPause.ValueChanged += AutoSaveConfiguration;
                    KeyToggleOverlay.ValueChanged += AutoSaveConfiguration;
                    KeyRestartMap.ValueChanged += AutoSaveConfiguration;
                    KeyIncreaseScrollSpeed.ValueChanged += AutoSaveConfiguration;
                    KeyDecreaseScrollSpeed.ValueChanged += AutoSaveConfiguration;
                    KeyIncreaseMapOffset.ValueChanged += AutoSaveConfiguration;
                    KeyDecreaseMapOffset.ValueChanged += AutoSaveConfiguration;
                    KeyScoreboardVisible.ValueChanged += AutoSaveConfiguration;
                    SelectOrderMapsetsBy.ValueChanged += AutoSaveConfiguration;
                    KeyQuickExit.ValueChanged += AutoSaveConfiguration;
                    SelectedGameMode.ValueChanged += AutoSaveConfiguration;
                    SelectedOnlineUserFilterType.ValueChanged += AutoSaveConfiguration;
                    BlurBackgroundInGameplay.ValueChanged += AutoSaveConfiguration;
                    TapToPause.ValueChanged += AutoSaveConfiguration;
                    DisplayFailedLocalScores.ValueChanged += AutoSaveConfiguration;
                    EditorScrollSpeedKeys.ValueChanged += AutoSaveConfiguration;
                    KeyEditorPausePlay.ValueChanged += AutoSaveConfiguration;
                    KeyEditorDecreaseAudioRate.ValueChanged += AutoSaveConfiguration;
                    KeyEditorIncreaseAudioRate.ValueChanged += AutoSaveConfiguration;
                    EditorBeatSnapColorType.ValueChanged += AutoSaveConfiguration;
                    EditorShowLaneDividerLines.ValueChanged += AutoSaveConfiguration;
                    EditorHitObjectsMidpointAnchored.ValueChanged += AutoSaveConfiguration;
                    EditorPlayMetronome.ValueChanged += AutoSaveConfiguration;
                    EditorMetronomePlayHalfBeats.ValueChanged += AutoSaveConfiguration;
                    DisplaySongTimeProgressNumbers.ValueChanged += AutoSaveConfiguration;
                    DisplayJudgementCounter.ValueChanged += AutoSaveConfiguration;
                    SkipResultsScreenAfterQuit.ValueChanged += AutoSaveConfiguration;
                    DisplayComboAlerts.ValueChanged += AutoSaveConfiguration;
                    LaneCoverTopHeight.ValueChanged += AutoSaveConfiguration;
                    LaneCoverBottomHeight.ValueChanged += AutoSaveConfiguration;
                    LaneCoverTop.ValueChanged += AutoSaveConfiguration;
                    LaneCoverBottom.ValueChanged += AutoSaveConfiguration;
                    EditorViewLayers.ValueChanged += AutoSaveConfiguration;
                    UIElementsOverLaneCover.ValueChanged += AutoSaveConfiguration;
                    SmoothAccuracyChanges.ValueChanged += AutoSaveConfiguration;
                    EditorVisualizationGraph.ValueChanged += AutoSaveConfiguration;
                    LobbyFilterHasPassword.ValueChanged += AutoSaveConfiguration;
                    LobbyFilterFullGame.ValueChanged += AutoSaveConfiguration;
                    LobbyFilterOwnsMap.ValueChanged += AutoSaveConfiguration;
                    LobbyFilterHasFriends.ValueChanged += AutoSaveConfiguration;
                    EnableBattleRoyaleBackgroundFlashing.ValueChanged += AutoSaveConfiguration;
                    EnableBattleRoyaleAlerts.ValueChanged += AutoSaveConfiguration;
                    ShowSpectators.ValueChanged += AutoSaveConfiguration;
                    JudgementWindows.ValueChanged += AutoSaveConfiguration;
                });
        }

        /// <summary>
        ///     Reads a Bindable<T>. Works on all types.
        /// </summary>
        /// <returns></returns>
        private static Bindable<T> ReadValue<T>(string name, T defaultVal, KeyDataCollection ini)
        {
            var binded = new Bindable<T>(name, defaultVal);
            var converter = TypeDescriptor.GetConverter(typeof(T));

            // Attempt to parse the value and default it if it can't.
            try
            {
                binded.Value = (T)converter.ConvertFromString(null, CultureInfo.InvariantCulture, ini[name]);
            }
            catch (Exception e)
            {
                binded.Value = defaultVal;
            }

            return binded;
        }

        /// <summary>
        ///     Reads an Int32 to a BindableInt
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultVal"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="ini"></param>
        /// <returns></returns>
        private static BindableInt ReadInt(string name, int defaultVal, int min, int max, KeyDataCollection ini)
        {
            var binded = new BindableInt(name, defaultVal, min, max);

            // Try to read the int.
            try
            {
                binded.Value = int.Parse(ini[name]);
            }
            catch (Exception e)
            {
                binded.Value = defaultVal;
            }

            return binded;
        }

        /// <summary>
        ///     Reads a special configuration string type. These values need to be read and written in a
        ///     certain way.
        /// </summary>
        /// <returns></returns>
        private static Bindable<string> ReadSpecialConfigType(SpecialConfigType type, string name, string defaultVal, KeyDataCollection ini)
        {
            var binded = new Bindable<string>(name, defaultVal);

            try
            {
                // Get parsed config value.
                var parsedVal = ini[name];

                switch (type)
                {
                    case SpecialConfigType.Directory:
                        if (Directory.Exists(parsedVal))
                            binded.Value = parsedVal;
                        else
                        {
                            // Make sure the default directory is created.
                            Directory.CreateDirectory(defaultVal);
                            throw new ArgumentException();
                        }

                        break;
                    case SpecialConfigType.Path:
                        if (File.Exists(parsedVal))
                            binded.Value = parsedVal;
                        else
                            throw new ArgumentException();
                        break;
                    case SpecialConfigType.Skin:
                        if (Directory.Exists(SkinDirectory + "/" + parsedVal))
                            binded.Value = parsedVal;
                        else
                            throw new ArgumentException();
                        break;
                    default:
                        throw new InvalidEnumArgumentException();
                }
            }
            catch (Exception e)
            {
                binded.Value = defaultVal;
            }

            return binded;
        }

        /// <summary>
        ///     Config Autosave functionality for Bindable<T>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="d"></param>
        private static void AutoSaveConfiguration<T>(object sender, BindableValueChangedEventArgs<T> d)
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            CommonTaskScheduler.Add(CommonTask.WriteConfig);
        }

        /// <summary>
        ///     Takes all of the current values from the ConfigManager class and creates a file with them.
        ///     This will automatically be called whenever a configuration value is changed in the code.
        /// </summary>
        internal static async Task WriteConfigFileAsync()
        {
            // Tracks the number of attempts to write the file it has made.
            var attempts = 0;

            // Don't do anything if the file isn't ready.
            while (!IsFileReady(GameDirectory + "/quaver.cfg") && !FirstWrite)
            {
            }

            var sb = new StringBuilder();

            // Top file information
            // sb.AppendLine("; Quaver Configuration File");
            sb.AppendLine("; Last Updated On: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine();
            sb.AppendLine("[Config]");
            sb.AppendLine("; Quaver Configuration Values");

            // For every line we want to append "PropName = PropValue" to the string
            foreach (var prop in typeof(ConfigManager).GetProperties(BindingFlags.Static | BindingFlags.NonPublic))
            {
                if (prop.Name == "FirstWrite" || prop.Name == "LastWrite")
                    continue;

                try
                {
                    sb.AppendLine(prop.Name + " = " + prop.GetValue(null));
                }
                catch (Exception e)
                {
                    sb.AppendLine(prop.Name + " = ");
                }
            }

            try
            {
                // Create a new stream
                var sw = new StreamWriter(GameDirectory + "/quaver.cfg")
                {
                    AutoFlush = true
                };

                // Write to file and close it.;
                await sw.WriteLineAsync(sb.ToString());
                sw.Close();

                FirstWrite = false;
            }
            catch (Exception e)
            {
                // Try to write the file again 3 times.
                while (attempts != 2)
                {
                    attempts++;

                    // Create a new stream
                    var sw = new StreamWriter(GameDirectory + "/quaver.cfg")
                    {
                        AutoFlush = true
                    };

                    // Write to file and close it.
                    await sw.WriteLineAsync(sb.ToString());
                    sw.Close();
                }

                // If too many attempts were made.
                if (attempts == 2)
                    Logger.Error("Too many write attempts to the config file have been made.", LogType.Runtime);
            }

            LastWrite = GameBase.Game.TimeRunning;
        }

        /// <summary>
        ///     Checks if the file is ready to be written to.
        /// </summary>
        /// <param name="sFilename"></param>
        /// <returns></returns>
        public static bool IsFileReady(string sFilename)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (var inputStream = File.Open(sFilename, FileMode.Open, FileAccess.Read, FileShare.None))
                    return (inputStream.Length > 0);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    /// <summary>
    ///     Enum containing special config types. We want to read and default these in
    ///     a very particular way.
    /// </summary>
    internal enum SpecialConfigType
    {
        Directory,
        Path,
        Skin
    }

    /// <summary>
    ///     Enum containing a number representation of the default skins we have available
    /// </summary>
    public enum DefaultSkins
    {
        Bar,
        Arrow,
        Circle,
        Barv2
    }
}
