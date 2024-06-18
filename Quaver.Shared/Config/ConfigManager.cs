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
using IniFileParser.Exceptions;
using IniFileParser.Model;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.Server.Common.Helpers;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Overlays.Hub.OnlineUsers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Edit.UI.Playfield.Spectrogram;
using Quaver.Shared.Screens.Edit.UI.Playfield.Waveform;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Filter;
using Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs;
using Quaver.Shared.Screens.Selection.UI.Leaderboard;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics.Sprites;
using Wobble.Input;
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
        ///     The temp directory
        /// </summary>
        internal static string BackupDirectory => Path.Join(DataDirectory.Value, "Backups");

        /// <summary>
        ///     The temp directory
        /// </summary>
        internal static string MapBackupDirectory => Path.Join(BackupDirectory, "Maps");

        /// <summary>
        ///     The temp directory
        /// </summary>
        internal static string TempDirectory => Path.Join(DataDirectory.Value, "Temp");

        /// <summary>
        ///     The song directory
        /// </summary>
        private static string _songDirectory;
        internal static Bindable<string> SongDirectory { get; private set; }

        /// <summary>
        ///     The directory of the Steam workshop
        /// </summary>
        private static string _steamWorkshopDirectory;
        internal static Bindable<string> SteamWorkshopDirectory { get; private set; }

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
        /// </summary>
        internal static Bindable<bool> WindowBorderless { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> PreferWayland { get; private set; }

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
        ///     Whether to use frame time or audio time for notes.
        /// </summary>
        internal static Bindable<bool> SmoothAudioTimingGameplay { get; private set; }

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
        ///     The path of the etterna cache.db file
        /// </summary>
        internal static Bindable<string> EtternaDbPath { get; private set; }

        /// <summary>
        ///     Dictates where or not we should load osu! maps from osu!.db on game start
        /// </summary>
        internal static Bindable<bool> AutoLoadOsuBeatmaps { get; private set; }

        /// <summary>
        ///     If the scoreboard is currently visible.
        /// </summary>
        internal static Bindable<bool> ScoreboardVisible { get; private set; }

        /// <summary>
        ///     Display the ranked accuracy in gameplay instead of the custom judgement windows accuracy
        /// </summary>
        internal static Bindable<bool> DisplayRankedAccuracy { get; private set; }

        /// <summary>
        ///     If true, the hitlighting will be tinted to the judgement color in the skin
        /// </summary>
        internal static Bindable<bool> TintHitLightingBasedOnJudgementColor { get; private set; }

        /// <summary>
        ///     Dictates how to order the mapsets during song select.Get
        /// </summary>
        internal static Bindable<OrderMapsetsBy> SelectOrderMapsetsBy { get; private set; }

        /// <summary>
        ///     Dictates how to group mapsets in song select
        /// </summary>
        internal static Bindable<GroupMapsetsBy> SelectGroupMapsetsBy { get; private set; }

        /// <summary>
        ///     Dictates how to filter song select mpas
        /// </summary>
        internal static Bindable<SelectFilterGameMode> SelectFilterGameModeBy { get; private set; }

        /// <summary>
        ///     The currently selected game mode.
        /// </summary>
        internal static Bindable<GameMode> SelectedGameMode { get; private set; }

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
        ///     If true, a hitsound will be played when releasing a long note
        /// </summary>
        internal static Bindable<bool> EnableLongNoteReleaseHitsounds { get; private set; }

        /// <summary>
        ///     If true, keysounds in gameplay will be played.
        /// </summary>
        internal static Bindable<bool> EnableKeysounds { get; private set; }

        /// <summary>
        ///     If enabled, the user will be able to tap to pause instead of having to hold for 500ms to pause.
        /// </summary>
        internal static Bindable<bool> TapToPause { get; private set; }

        /// <summary>
        ///     If enabled, the user will be able to continue playing the map when dying, but with No Fail mod enabled.
        /// </summary>
        internal static Bindable<bool> KeepPlayingUponFailing { get; private set; }

        /// <summary>
        ///     If enabled, the user will be able to tap to restart instead of having to hold for 200ms to restart.
        /// </summary>
        internal static Bindable<bool> TapToRestart { get; private set; }

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
        ///	    If enabled, automatically skip the beta splash screen.
        /// </summary>
        internal static Bindable<bool> SkipSplashScreen { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> DisplayComboAlerts { get; private set; }

        /// <summary>
        ///     Scaling of ImGui windows and texts
        /// </summary>
        internal static BindableInt EditorImGuiScalePercentage { get; private set; }

        /// <summary>
        ///     The scroll speed used in the editor.
        /// </summary>
        internal static BindableInt EditorScrollSpeedKeys { get; private set; }

        internal static Bindable<bool> EditorLiveMapSnap { get; private set; }

        internal static BindableInt EditorLiveMapOffset { get; private set; }

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

        /// <summary>
        /// The amount of time in milliseconds a hit in the hiterror takes to disappear
        /// </summary>
        internal static BindableInt HitErrorFadeTime { get; private set; }

        /// <summary></summary>
        ///     If true, the user will skip the results screen after quitting the game.
        /// </summary>
        internal static Bindable<bool> SkipResultsScreenAfterQuit { get; private set; }

        /// <summary>
        /// If true, the windows key is locked during gameplay
        /// </summary>
        internal static Bindable<bool> LockWinkeyDuringGameplay { get; private set; }

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
        internal static Bindable<bool> DisplayUnbeatableScoresDuringGameplay { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> ShowSpectators { get; private set; }

        /// <summary>
        ///     The selected judgement window preset
        /// </summary>
        internal static Bindable<string> JudgementWindows { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<OrderMapsetsBy> MusicPlayerOrderMapsBy { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<OnlineUserListFilter> OnlineUserListFilterType { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> DisplayFriendOnlineNotifications { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> DisplaySongRequestNotifications { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<MultiplayerLobbyRuleset> MultiplayerLobbyRulesetType { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<MultiplayerLobbyGameMode> MultiplayerLobbyGameModeType { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<MultiplayerLobbyMapStatus> MultiplayerLobbyMapStatusType { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<MultiplayerLobbyRoomVisibility> MultiplayerLobbyVisibilityType { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> UseSteamWorkshopSkin { get; private set; }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        internal static Bindable<bool> LowerFpsOnWindowInactive { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> DownloadDisplayOwnedMapsets { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> DownloadReverseSort { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> DisplayNotificationsBottomToTop { get; private set; }

        /// <summary>
        /// </summary>
        internal static BindableInt SelectedProfileId { get; private set; }

        /// <summary>
        /// </summary>
        internal static BindableInt EditorBackgroundBrightness { get; private set; }

        /// <summary>
        /// </summary>
        internal static BindableInt EditorHitsoundVolume { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> EditorScaleSpeedWithRate { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<EditorPlayfieldWaveformFilter> EditorAudioFilter { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> EditorShowWaveform { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> EditorShowSpectrogram { get; private set; }

        internal static Bindable<int> EditorSpectrogramMaximumFrequency { get; private set; }

        internal static Bindable<int> EditorSpectrogramMinimumFrequency { get; private set; }

        internal static Bindable<float> EditorSpectrogramCutoffFactor { get; private set; }

        internal static Bindable<float> EditorSpectrogramIntensityFactor { get; private set; }

        internal static Bindable<EditorPlayfieldSpectrogramFrequencyScale> EditorSpectrogramFrequencyScale { get; private set; }

        internal static BindableInt EditorSpectrogramFftSize { get; private set; }

        /// <summary>
        ///     The number of times the song's fft will be taken. Linearly increases the time to load
        /// </summary>
        internal static BindableInt EditorSpectrogramInterleaveCount { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<EditorPlayfieldWaveformAudioDirection> EditorAudioDirection { get; private set; }

        /// <summary>
        /// </summary>
        internal static BindableInt EditorWaveformColorR { get; private set; }

        /// <summary>
        /// </summary>
        internal static BindableInt EditorWaveformColorG { get; private set; }

        /// <summary>
        /// </summary>
        internal static BindableInt EditorWaveformColorB { get; private set; }

        /// <summary>
        /// </summary>
        internal static BindableInt EditorWaveformBrightness { get; private set; }

        /// <summary>
        /// </summary>
        internal static BindableInt EditorSpectrogramBrightness { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> EditorPlaceObjectsOnNearestTick { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> EditorLiveMapping { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> EditorInvertBeatSnapScroll { get; private set; }

        /// <summary>
        /// </summary>
        internal static BindableInt EditorLongNoteOpacity { get; private set; }

        /// <summary>
        /// </summary>
        internal static BindableInt GameplayNoteScale { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> EditorDisplayGameplayPreview { get; private set; }

        /// <summary>
        /// </summary>
        internal static BindableInt VisualOffset { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> Display1v1TournamentOverlay { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> TournamentDisplay1v1PlayfieldScores { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> ReloadSkinOnChange { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> EnableRealtimeOnlineScoreboard { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> ScratchLaneLeft4K { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> ScratchLaneLeft7K { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> AcceptedTermsAndPrivacyPolicy { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> DisplayGameplayOverlay { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> EnableHighProcessPriority { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> DisplayNotificationsInGameplay { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> DisplayPauseWarning { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<bool> DisplayFailWarning { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<string> TournamentPlayer2Skin { get; private set; }

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
        internal static Bindable<GenericKey> KeyMania4K1 { get; private set; }
        internal static Bindable<GenericKey> KeyMania4K2 { get; private set; }
        internal static Bindable<GenericKey> KeyMania4K3 { get; private set; }
        internal static Bindable<GenericKey> KeyMania4K4 { get; private set; }

        /// <summary>
        ///     Keybindings for 7K
        /// </summary>
        internal static Bindable<GenericKey> KeyMania7K1 { get; private set; }
        internal static Bindable<GenericKey> KeyMania7K2 { get; private set; }
        internal static Bindable<GenericKey> KeyMania7K3 { get; private set; }
        internal static Bindable<GenericKey> KeyMania7K4 { get; private set; }
        internal static Bindable<GenericKey> KeyMania7K5 { get; private set; }
        internal static Bindable<GenericKey> KeyMania7K6 { get; private set; }
        internal static Bindable<GenericKey> KeyMania7K7 { get; private set; }

        /// <summary>
        ///     Keybindings for 4K (co-op 2 player)
        /// </summary>
        internal static Bindable<GenericKey> KeyCoop2P4K1 { get; private set; }
        internal static Bindable<GenericKey> KeyCoop2P4K2 { get; private set; }
        internal static Bindable<GenericKey> KeyCoop2P4K3 { get; private set; }
        internal static Bindable<GenericKey> KeyCoop2P4K4 { get; private set; }

        /// <summary>
        ///     Keybindings for 7K (co-op 2 player)
        /// </summary>
        internal static Bindable<GenericKey> KeyCoop2P7K1 { get; private set; }
        internal static Bindable<GenericKey> KeyCoop2P7K2 { get; private set; }
        internal static Bindable<GenericKey> KeyCoop2P7K3 { get; private set; }
        internal static Bindable<GenericKey> KeyCoop2P7K4 { get; private set; }
        internal static Bindable<GenericKey> KeyCoop2P7K5 { get; private set; }
        internal static Bindable<GenericKey> KeyCoop2P7K6 { get; private set; }
        internal static Bindable<GenericKey> KeyCoop2P7K7 { get; private set; }

        /// <summary>
        ///     Scratch key layout for 4K+1
        /// </summary>
        internal static Bindable<GenericKey> KeyLayout4KScratch1 { get; private set; }
        internal static Bindable<GenericKey> KeyLayout4KScratch2 { get; private set; }
        internal static Bindable<GenericKey> KeyLayout4KScratch3 { get; private set; }
        internal static Bindable<GenericKey> KeyLayout4KScratch4 { get; private set; }
        internal static Bindable<GenericKey> KeyLayout4KScratch5 { get; private set; }

        /// <summary>
        ///     Scratch key layout for 7K+1
        /// </summary>
        internal static Bindable<GenericKey> KeyLayout7KScratch1 { get; private set; }
        internal static Bindable<GenericKey> KeyLayout7KScratch2 { get; private set; }
        internal static Bindable<GenericKey> KeyLayout7KScratch3 { get; private set; }
        internal static Bindable<GenericKey> KeyLayout7KScratch4 { get; private set; }
        internal static Bindable<GenericKey> KeyLayout7KScratch5 { get; private set; }
        internal static Bindable<GenericKey> KeyLayout7KScratch6 { get; private set; }
        internal static Bindable<GenericKey> KeyLayout7KScratch7 { get; private set; }
        internal static Bindable<GenericKey> KeyLayout7KScratch8 { get; private set; }
        internal static Bindable<GenericKey> KeyLayout7KScratch9 { get; private set; }

        /// <summary>
        ///     The key pressed to pause and menu-back.
        /// </summary>
        internal static Bindable<GenericKey> KeyPause { get; private set; }

        /// <summary>
        ///     The key pressed to skip the song introduction
        /// </summary>
        internal static Bindable<GenericKey> KeySkipIntro { get; private set; }

        /// <summary>
        ///     The key to toggle the overlay
        /// </summary>
        internal static Bindable<Keys> KeyToggleOverlay { get; private set; }

        /// <summary>
        ///     The key to toggle the mirror mod while in song select
        /// </summary>
        internal static Bindable<Keys> KeyToggleMirror { get; private set; }

        /// <summary>
        ///     The key to decrease the gameplay rate while in song select
        /// </summary>
        internal static Bindable<Keys> KeyDecreaseGameplayAudioRate { get; private set; }

        /// <summary>
        ///     The key to increase the gameplay rate while in song select
        /// </summary>
        internal static Bindable<Keys> KeyIncreaseGameplayAudioRate { get; private set; }

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
        ///     The keys to toggle autoplay during playtesting
        /// </summary>
        internal static Bindable<Keys> KeyTogglePlaytestAutoplay { get; private set; }

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
        ///     Whether global scrolling is inverted.
        /// </summary>
        internal static Bindable<bool> InvertScrolling { get; private set; }

        /// <summary>
        ///     Whether scrolling in editor is inverted.
        /// </summary>
        internal static Bindable<bool> InvertEditorScrolling { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<Keys> KeyScreenshot { get; private set; }

        /// <summary>
        /// </summary>
        internal static Bindable<ResultGraphs> ResultGraph { get; private set; }

        internal static Bindable<string> AudioOutputDevice { get; private set; }

        /// <summary>
        ///     Target difficulty used for selecting a default map in a mapset.
        ///     Stored as an integer, divide by 10 for actual target difficulty.
        /// </summary>
        internal static BindableInt PrioritizedMapDifficulty4K { get; private set; }

        /// <summary>
        ///     Target difficulty used for selecting a default map in a mapset.
        ///     Stored as an integer, divide by 10 for actual target difficulty.
        /// </summary>
        internal static BindableInt PrioritizedMapDifficulty7K { get; private set; }

        /// <summary>
        ///     Prioritize which keymode when selecting a default map in a mapset.
        /// </summary>
        internal static Bindable<GameMode> PrioritizedGameMode { get; private set; }

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

            Directory.CreateDirectory($"{WobbleGame.WorkingDirectory}/Plugins");
            Directory.CreateDirectory($"{WobbleGame.WorkingDirectory}/Tournament");

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
            var configFilePath = _gameDirectory + "/quaver.cfg";

            if (File.Exists(configFilePath))
            {
                try
                {
                    // Delete the config file if we catch an exception.
                    var _ = new IniFileParser.IniFileParser(new ConcatenateDuplicatedKeysIniDataParser()).ReadFile(configFilePath)["Config"];
                }
                catch (ParsingException)
                {
                    Logger.Important("Config file couldn't be read.", LogType.Runtime);
                    File.Copy(configFilePath, _gameDirectory + "/quaver.corrupted." + TimeHelper.GetUnixTimestampMilliseconds() + ".cfg");
                    File.Delete(configFilePath);
                }
            }

            // We'll want to write a quaver.cfg file if it doesn't already exist.
            // There's no need to read the config file afterwards, since we already have
            // all of the default values.
            if (!File.Exists(configFilePath))
            {
                File.WriteAllText(configFilePath, "; Quaver Configuration File");
                Logger.Important("Creating a new config file...", LogType.Runtime);
            }

            var data = new IniFileParser.IniFileParser(new ConcatenateDuplicatedKeysIniDataParser()).ReadFile(configFilePath, Encoding.UTF8)["Config"];

            // Read / Set Config Values
            // NOTE: MAKE SURE TO SET THE VALUE TO AUTO-SAVE WHEN CHANGING! THIS ISN'T DONE AUTOMATICALLY.
            // YOU CAN DO THIS DOWN BELOW, AFTER THE CONFIG HAS WRITTEN FOR THE FIRST TIME.
            GameDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"GameDirectory", _gameDirectory, data);
            SkinDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"SkinDirectory", _skinDirectory, data);
            ScreenshotDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"ScreenshotDirectory", _screenshotDirectory, data);
            ReplayDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"ReplayDirectory", _replayDirectory, data);
            LogsDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"LogsDirectory", _logsDirectory, data);
            DataDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"DataDirectory", _dataDirectory, data);
            SongDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"SongDirectory", _songDirectory, data);
            _steamWorkshopDirectory = $"{GameDirectory.Value}/../../workshop/content/{SteamManager.ApplicationId}";
            SteamWorkshopDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"SteamWorkshopDirectory", _steamWorkshopDirectory, data);
            SelectedGameMode = ReadValue(@"SelectedGameMode", GameMode.Keys4, data);
            Username = ReadValue(@"Username", "Player", data);
            VolumeGlobal = ReadInt(@"VolumeGlobal", 20, 0, 100, data);
            VolumeEffect = ReadInt(@"VolumeEffect", 20, 0, 100, data);
            VolumeMusic = ReadInt(@"VolumeMusic", 50, 0, 100, data);
            DevicePeriod = ReadInt(@"DevicePeriod", 2, 1, 100, data);
            DeviceBufferLengthMultiplier = ReadInt(@"DeviceBufferLengthMultiplier", 4, 2, 10, data);
            BackgroundBrightness = ReadInt(@"BackgroundBrightness", 50, 0, 100, data);
            WindowHeight = ReadInt(@"WindowHeight", 768, 360, short.MaxValue, data);
            WindowWidth = ReadInt(@"WindowWidth", 1366, 640, short.MaxValue, data);
            WindowBorderless = ReadValue(@"WindowBorderless", false, data);
            PreferWayland = ReadValue(@"PreferWayland", false, data);
            DisplaySongTimeProgress = ReadValue(@"DisplaySongTimeProgress", true, data);
            WindowFullScreen = ReadValue(@"WindowFullScreen", false, data);
            FpsCounter = ReadValue(@"FpsCounter", false, data);
            FpsLimiterType = ReadValue(@"FpsLimiterType", FpsLimitType.Unlimited, data);
            CustomFpsLimit = ReadInt(@"CustomFpsLimit", 240, 60, 5000, data);
            SmoothAudioTimingGameplay = ReadValue(@"SmoothAudioTimingGameplay", false, data);
            ScrollSpeed4K = ReadInt(@"ScrollSpeed4K", 150, 50, 1000, data);
            ScrollSpeed7K = ReadInt(@"ScrollSpeed7K", 150, 50, 1000, data);
            ScrollDirection4K = ReadValue(@"ScrollDirection4K", ScrollDirection.Down, data);
            ScrollDirection7K = ReadValue(@"ScrollDirection7K", ScrollDirection.Down, data);
            GlobalAudioOffset = ReadInt(@"GlobalAudioOffset", 0, -500, 500, data);
            Skin = ReadValue(@"Skin", "", data);
            DefaultSkin = ReadValue(@"DefaultSkin", DefaultSkins.Bar, data);
            Pitched = ReadValue(@"Pitched", true, data);
            ScoreboardVisible = ReadValue(@"ScoreboardVisible", true, data);
            DisplayRankedAccuracy = ReadValue(@"DisplayRankedAccuracy", false, data);
            SelectOrderMapsetsBy = ReadValue(@"SelectOrderMapsetsBy", OrderMapsetsBy.Artist, data);
            LeaderboardSection = ReadValue(@"LeaderboardSection", LeaderboardType.Local, data);
            OsuDbPath = ReadSpecialConfigType(SpecialConfigType.Path, @"OsuDbPath", "", data);
            EtternaDbPath = ReadSpecialConfigType(SpecialConfigType.Path, @"EtternaDbPath", "", data);
            AutoLoadOsuBeatmaps = ReadValue(@"AutoLoadOsuBeatmaps", false, data);
            AutoLoginToServer = ReadValue(@"AutoLoginToServer", true, data);
            DisplayTimingLines = ReadValue(@"DisplayTimingLines", true, data);
            DisplayMenuAudioVisualizer = ReadValue(@"DisplayMenuAudioVisualizer", true, data);
            EnableHitsounds = ReadValue(@"EnableHitsounds", true, data);
            EnableLongNoteReleaseHitsounds = ReadValue(@"EnableLongNoteReleaseHitsounds", false, data);
            EnableKeysounds = ReadValue(@"EnableKeysounds", true, data);
            KeyNavigateLeft = ReadValue(@"KeyNavigateLeft", Keys.Left, data);
            KeyNavigateRight = ReadValue(@"KeyNavigateRight", Keys.Right, data);
            KeyNavigateUp = ReadValue(@"KeyNavigateUp", Keys.Up, data);
            KeyNavigateDown = ReadValue(@"KeyNavigateDown", Keys.Down, data);
            KeyNavigateBack = ReadValue(@"KeyNavigateBack", Keys.Escape, data);
            KeyNavigateSelect = ReadValue(@"KeyNavigateSelect", Keys.Enter, data);
            KeyMania4K1 = ReadGenericKey(@"KeyMania4K1", new GenericKey { KeyboardKey = Keys.A }, data);
            KeyMania4K2 = ReadGenericKey(@"KeyMania4K2", new GenericKey { KeyboardKey = Keys.S }, data);
            KeyMania4K3 = ReadGenericKey(@"KeyMania4K3", new GenericKey { KeyboardKey = Keys.K }, data);
            KeyMania4K4 = ReadGenericKey(@"KeyMania4K4", new GenericKey { KeyboardKey = Keys.L }, data);
            KeyMania7K1 = ReadGenericKey(@"KeyMania7K1", new GenericKey { KeyboardKey = Keys.A }, data);
            KeyMania7K2 = ReadGenericKey(@"KeyMania7K2", new GenericKey { KeyboardKey = Keys.S }, data);
            KeyMania7K3 = ReadGenericKey(@"KeyMania7K3", new GenericKey { KeyboardKey = Keys.D }, data);
            KeyMania7K4 = ReadGenericKey(@"KeyMania7K4", new GenericKey { KeyboardKey = Keys.Space }, data);
            KeyMania7K5 = ReadGenericKey(@"KeyMania7K5", new GenericKey { KeyboardKey = Keys.J }, data);
            KeyMania7K6 = ReadGenericKey(@"KeyMania7K6", new GenericKey { KeyboardKey = Keys.K }, data);
            KeyMania7K7 = ReadGenericKey(@"KeyMania7K7", new GenericKey { KeyboardKey = Keys.L }, data);
            KeyCoop2P4K1 = ReadGenericKey(@"KeyCoop2P4K1", new GenericKey { KeyboardKey = Keys.Z }, data);
            KeyCoop2P4K2 = ReadGenericKey(@"KeyCoop2P4K2", new GenericKey { KeyboardKey = Keys.X }, data);
            KeyCoop2P4K3 = ReadGenericKey(@"KeyCoop2P4K3", new GenericKey { KeyboardKey = Keys.OemComma }, data);
            KeyCoop2P4K4 = ReadGenericKey(@"KeyCoop2P4K4", new GenericKey { KeyboardKey = Keys.OemPeriod }, data);
            KeyCoop2P7K1 = ReadGenericKey(@"KeyCoop2P7K1", new GenericKey { KeyboardKey = Keys.Z }, data);
            KeyCoop2P7K2 = ReadGenericKey(@"KeyCoop2P7K2", new GenericKey { KeyboardKey = Keys.X }, data);
            KeyCoop2P7K3 = ReadGenericKey(@"KeyCoop2P7K3", new GenericKey { KeyboardKey = Keys.C }, data);
            KeyCoop2P7K4 = ReadGenericKey(@"KeyCoop2P7K4", new GenericKey { KeyboardKey = Keys.V }, data);
            KeyCoop2P7K5 = ReadGenericKey(@"KeyCoop2P7K5", new GenericKey { KeyboardKey = Keys.M }, data);
            KeyCoop2P7K6 = ReadGenericKey(@"KeyCoop2P7K6", new GenericKey { KeyboardKey = Keys.OemComma }, data);
            KeyCoop2P7K7 = ReadGenericKey(@"KeyCoop2P7K7", new GenericKey { KeyboardKey = Keys.OemPeriod }, data);

            KeyLayout4KScratch1 = ReadGenericKey(@"KeyLayout4KScratch1", new GenericKey { KeyboardKey = Keys.A }, data);
            KeyLayout4KScratch2 = ReadGenericKey(@"KeyLayout4KScratch2", new GenericKey { KeyboardKey = Keys.S }, data);
            KeyLayout4KScratch3 = ReadGenericKey(@"KeyLayout4KScratch3", new GenericKey { KeyboardKey = Keys.D }, data);
            KeyLayout4KScratch4 = ReadGenericKey(@"KeyLayout4KScratch4", new GenericKey { KeyboardKey = Keys.K }, data);
            KeyLayout4KScratch5 = ReadGenericKey(@"KeyLayout4KScratch5", new GenericKey { KeyboardKey = Keys.L }, data);

            KeyLayout7KScratch1 = ReadGenericKey(@"KeyLayout7KScratch1", new GenericKey { KeyboardKey = Keys.A }, data);
            KeyLayout7KScratch2 = ReadGenericKey(@"KeyLayout7KScratch2", new GenericKey { KeyboardKey = Keys.S }, data);
            KeyLayout7KScratch3 = ReadGenericKey(@"KeyLayout7KScratch3", new GenericKey { KeyboardKey = Keys.D }, data);
            KeyLayout7KScratch4 = ReadGenericKey(@"KeyLayout7KScratch4", new GenericKey { KeyboardKey = Keys.Space }, data);
            KeyLayout7KScratch5 = ReadGenericKey(@"KeyLayout7KScratch5", new GenericKey { KeyboardKey = Keys.J }, data);
            KeyLayout7KScratch6 = ReadGenericKey(@"KeyLayout7KScratch6", new GenericKey { KeyboardKey = Keys.K }, data);
            KeyLayout7KScratch7 = ReadGenericKey(@"KeyLayout7KScratch7", new GenericKey { KeyboardKey = Keys.L }, data);
            KeyLayout7KScratch8 = ReadGenericKey(@"KeyLayout7KScratch8", new GenericKey { KeyboardKey = Keys.CapsLock }, data);
            KeyLayout7KScratch9 = ReadGenericKey(@"KeyLayout7KScratch9", new GenericKey { KeyboardKey = Keys.OemColon }, data);

            KeySkipIntro = ReadGenericKey(@"KeySkipIntro", new GenericKey { KeyboardKey = Keys.Space }, data);
            KeyPause = ReadGenericKey(@"KeyPause", new GenericKey { KeyboardKey = Keys.Escape }, data);
            KeyToggleOverlay = ReadValue(@"KeyToggleOverlay", Keys.F8, data);
            KeyToggleMirror = ReadValue(@"KeyToggleMirror", Keys.H, data);
            KeyDecreaseGameplayAudioRate = ReadValue(@"KeyDecreaseGameplayAudioRate", Keys.OemMinus, data);
            KeyIncreaseGameplayAudioRate = ReadValue(@"KeyIncreaseGameplayAudioRate", Keys.OemPlus, data);
            KeyRestartMap = ReadValue(@"KeyRestartMap", Keys.OemTilde, data);
            KeyDecreaseScrollSpeed = ReadValue(@"KeyDecreaseScrollSpeed", Keys.F3, data);
            KeyIncreaseScrollSpeed = ReadValue(@"KeyIncreaseScrollSpeed", Keys.F4, data);
            KeyDecreaseMapOffset = ReadValue(@"KeyDecreaseMapOffset", Keys.OemMinus, data);
            KeyIncreaseMapOffset = ReadValue(@"KeyIncreaseMapOffset", Keys.OemPlus, data);
            KeyTogglePlaytestAutoplay = ReadValue(@"KeyTogglePlaytestAutoplay", Keys.Tab, data);
            KeyScoreboardVisible = ReadValue(@"KeyScoreboardVisible", Keys.Tab, data);
            KeyQuickExit = ReadValue(@"KeyQuickExit", Keys.F1, data);
            KeyScreenshot = ReadValue(@"KeyScreenshot", Keys.F12, data);
            TapToPause = ReadValue(@"TapToPause", false, data);
            KeepPlayingUponFailing = ReadValue(@"KeepPlayingUponFailing", false, data);
            TapToRestart = ReadValue(@"TapToRestart", false, data);
            DisplayFailedLocalScores = ReadValue(@"DisplayFailedLocalScores", true, data);
            EditorScrollSpeedKeys = ReadInt(@"EditorScrollSpeedKeys", 16, 5, 100, data);
            EditorImGuiScalePercentage = ReadInt(@"EditorImGuiScalePercentage", 100, 25, 300, data);
            KeyEditorPausePlay = ReadValue(@"KeyEditorPausePlay", Keys.Space, data);
            KeyEditorDecreaseAudioRate = ReadValue(@"KeyEditorDecreaseAudioRate", Keys.OemMinus, data);
            KeyEditorIncreaseAudioRate = ReadValue(@"KeyEditorIncreaseAudioRate", Keys.OemPlus, data);
            InvertScrolling = ReadValue(@"InvertScrolling", false, data);
            InvertEditorScrolling = ReadValue(@"InvertEditorScrolling", true, data);
            EditorLiveMapSnap = ReadValue(@"EditorLiveMapSnap", false, data);
            EditorLiveMapOffset = ReadInt(@"EditorLiveMapOffset", 0, -200, 200, data);
            EditorEnableHitsounds = ReadValue(@"EditorEnableHitsounds", true, data);
            EditorEnableKeysounds = ReadValue(@"EditorEnableKeysounds", true, data);
            EditorBeatSnapColorType = ReadValue(@"EditorBeatSnapColorType", EditorBeatSnapColor.Default, data);
            EditorOnlyShowMeasureLines = ReadValue(@"EditorOnlyShowMeasureLines", false, data);
            EditorShowLaneDividerLines = ReadValue(@"EditorShowLaneDividerLines", true, data);
            EditorHitObjectsMidpointAnchored = ReadValue(@"EditorHitObjectsMidpointAnchored", false, data);
            EditorPlayMetronome = ReadValue(@"EditorPlayMetronome", true, data);
            EditorMetronomePlayHalfBeats = ReadValue(@"EditorMetronomePlayHalfBeats", false, data);
            DisplaySongTimeProgressNumbers = ReadValue(@"DisplaySongTimeProgressNumbers", true, data);
            DisplayJudgementCounter = ReadValue(@"DisplayJudgementCounter", true, data);
            HitErrorFadeTime = ReadInt(@"HitErrorFadeTime", 1000, 100, 5000, data);
            SkipResultsScreenAfterQuit = ReadValue(@"SkipResultsScreenAfterQuit", false, data);
            LockWinkeyDuringGameplay = ReadValue(@"LockWinkeyDuringGameplay", true, data);
            DisplayComboAlerts = ReadValue(@"DisplayComboAlerts", true, data);
            LaneCoverTopHeight = ReadInt(@"LaneCoverTopHeight", 25, 0, 75, data);
            LaneCoverBottomHeight = ReadInt(@"LaneCoverBottomHeight", 25, 0, 75, data);
            LaneCoverTop = ReadValue(@"LaneCoverTop", false, data);
            LaneCoverBottom = ReadValue(@"LaneCoverBottom", false, data);
            UIElementsOverLaneCover = ReadValue(@"UIElementsOverLaneCover", true, data);
            EditorViewLayers = ReadValue(@"EditorViewLayers", false, data);
            LobbyFilterHasPassword = ReadValue(@"LobbyFilterHasPassword", true, data);
            LobbyFilterFullGame = ReadValue(@"LobbyFilterFullGame", false, data);
            LobbyFilterOwnsMap = ReadValue(@"LobbyFilterOwnsMap", false, data);
            LobbyFilterHasFriends = ReadValue(@"LobbyFilterHasFriends", false, data);
            EnableBattleRoyaleBackgroundFlashing = ReadValue(@"EnableBattleRoyaleBackgroundFlashing", true, data);
            EnableBattleRoyaleAlerts = ReadValue(@"EnableBattleRoyaleAlerts", true, data);
            SelectFilterGameModeBy = ReadValue(@"SelectFilterGameModeBy", SelectFilterGameMode.All, data);
            DisplayUnbeatableScoresDuringGameplay = ReadValue(@"DisplayUnbeatableScoresDuringGameplay", true, data);
            ShowSpectators = ReadValue(@"ShowSpectators", true, data);
            JudgementWindows = ReadValue("JudgementWindows", "", data);
            SelectGroupMapsetsBy = ReadValue(@"SelectGroupMapsetsBy", GroupMapsetsBy.None, data);
            MusicPlayerOrderMapsBy = ReadValue(@"MusicPlayerOrderMapsBy", OrderMapsetsBy.Artist, data);
            OnlineUserListFilterType = ReadValue(@"OnlineUserListFilterType", OnlineUserListFilter.All, data);
            DisplayFriendOnlineNotifications = ReadValue(@"DisplayFriendOnlineNotifications", true, data);
            DisplaySongRequestNotifications = ReadValue(@"DisplaySongRequestNotifications", true, data);
            MultiplayerLobbyRulesetType = ReadValue(@"MultiplayerLobbyRulesetType", MultiplayerLobbyRuleset.All, data);
            MultiplayerLobbyGameModeType = ReadValue(@"MultiplayerLobbyGameModeType", MultiplayerLobbyGameMode.All, data);
            MultiplayerLobbyMapStatusType = ReadValue(@"MultiplayerLobbyMapStatusType", MultiplayerLobbyMapStatus.All, data);
            MultiplayerLobbyVisibilityType = ReadValue(@"MultiplayerLobbyVisibilityType", MultiplayerLobbyRoomVisibility.All, data);
            UseSteamWorkshopSkin = ReadValue(@"UseSteamWorkshopSkin", false, data);
            LowerFpsOnWindowInactive = ReadValue(@"LowerFpsOnWindowInactive", true, data);
            DownloadDisplayOwnedMapsets = ReadValue(@"DownloadDisplayOwnedMapsets", true, data);
            DownloadReverseSort = ReadValue(@"DownloadReverseSort", false, data);
            DisplayNotificationsBottomToTop = ReadValue(@"DisplayNotificationsBottomToTop", false, data);
            SelectedProfileId = ReadInt(@"SelectedProfileId", -1, -1, int.MaxValue, data);
            EditorBackgroundBrightness = ReadInt(@"EditorBackgroundBrightness", 40, 0, 100, data);
            EditorHitsoundVolume = ReadInt(@"EditorHitsoundVolume", -1, -1, 100, data);
            EditorScaleSpeedWithRate = ReadValue(@"EditorScaleSpeedWithRate", true, data);
            EditorLongNoteOpacity = ReadInt(@"EditorLongNoteOpacity", 100, 30, 100, data);
            GameplayNoteScale = ReadInt(@"GameplayNoteScale", 100, 25, 100, data);
            EditorDisplayGameplayPreview = ReadValue(@"EditorDisplayGameplayPreview", false, data);
            EditorPlaceObjectsOnNearestTick = ReadValue(@"EditorPlaceObjectsOnNearestTick", true, data);
            EditorInvertBeatSnapScroll = ReadValue(@"EditorInvertBeatSnapScroll", false, data);
            EditorLiveMapping = ReadValue(@"EditorLiveMapping", true, data);
            EditorAudioFilter = ReadValue(@"EditorAudioFilter", EditorPlayfieldWaveformFilter.None, data);
            EditorShowWaveform = ReadValue(@"EditorShowWaveform", true, data);
            EditorShowSpectrogram = ReadValue(@"EditorShowSpectrogram", false, data);
            EditorSpectrogramMaximumFrequency = ReadInt(@"EditorSpectrogramMaximumFrequency", 7000, 5000, 10000, data);
            EditorSpectrogramMinimumFrequency = ReadInt("EditorSpectrogramMinimumFrequency", 125, 0, 1500, data);
            EditorSpectrogramCutoffFactor = ReadValue("EditorSpectrogramCutoffFactor", 0.34f, data);
            EditorSpectrogramIntensityFactor = ReadValue("EditorSpectrogramIntensityFactor", 9.5f, data);
            EditorSpectrogramFrequencyScale = ReadValue("EditorSpectrogramFrequencyScale", EditorPlayfieldSpectrogramFrequencyScale.Linear, data);
            EditorSpectrogramFftSize = ReadInt(@"EditorSpectrumFftSize", 512, 256, 16384, data);
            EditorSpectrogramInterleaveCount = ReadInt(@"EditorSpectrogramInterleaveCount", 4, 1, 16, data);
            EditorAudioDirection = ReadValue(@"EditorAudioDirection", EditorPlayfieldWaveformAudioDirection.Both, data);
            EditorWaveformColorR = ReadInt(@"EditorWaveformColorR", 0, 0, 255, data);
            EditorWaveformColorG = ReadInt(@"EditorWaveformColorG", 200, 0, 255, data);
            EditorWaveformColorB = ReadInt(@"EditorWaveformColorB", 255, 0, 255, data);
            EditorWaveformBrightness = ReadInt(@"EditorWaveformBrightness", 50, 0, 100, data);
            EditorSpectrogramBrightness = ReadInt(@"EditorSpectrogramBrightness", 50, 0, 100, data);
            VisualOffset = ReadInt(@"VisualOffset", 0, -500, 500, data);
            TintHitLightingBasedOnJudgementColor = ReadValue(@"TintHitLightingBasedOnJudgementColor", false, data);
            Display1v1TournamentOverlay = ReadValue(@"Display1v1TournamentOverlay", true, data);
            TournamentDisplay1v1PlayfieldScores = ReadValue(@"TournamentDisplay1v1PlayfieldScores", true, data);
            ReloadSkinOnChange = ReadValue(@"ReloadSkinOnChange", false, data);
            EnableRealtimeOnlineScoreboard = ReadValue(@"EnableRealtimeOnlineScoreboard", false, data);
            ScratchLaneLeft4K = ReadValue(@"ScratchLaneLeft4K", true, data);
            ScratchLaneLeft7K = ReadValue(@"ScratchLaneLeft7K", true, data);
            AcceptedTermsAndPrivacyPolicy = ReadValue(@"AcceptedTermsAndPrivacyPolicy", false, data);
            SkipSplashScreen = ReadValue(@"SkipSplashScreen", false, data);
            DisplayGameplayOverlay = ReadValue(@"DisplayGameplayOverlay", true, data);
            EnableHighProcessPriority = ReadValue(@"EnableHighProcessPriority", false, data);
            DisplayNotificationsInGameplay = ReadValue(@"DisplayNotificationsInGameplay", false, data);
            DisplayPauseWarning = ReadValue(@"DisplayPauseWarning", true, data);
            DisplayFailWarning = ReadValue(@"DisplayFailWarning", true, data);
            TournamentPlayer2Skin = ReadValue(@"TournamentPlayer2Skin", "", data);
            ResultGraph = ReadValue(@"ResultGraph", ResultGraphs.Deviance, data);
            AudioOutputDevice = ReadValue(@"AudioOutputDevice", "Default", data);
            PrioritizedMapDifficulty4K = ReadInt(@"PrioritizedMapDifficulty4K", 0, 0, 1000, data);
            PrioritizedMapDifficulty7K = ReadInt(@"PrioritizedMapDifficulty7K", 0, 0, 1000, data);
            PrioritizedGameMode = ReadValue(@"PrioritizedGameMode", (GameMode)0, data);

            // Bind global inverted scrolling so ScrollContainers get InvertScrolling setting too
            ScrollContainer.GlobalInvertedScrolling = InvertScrolling;

            // Have to do this manually.
            if (string.IsNullOrEmpty(Username.Value))
                Username.Value = "Player";

            WriteConfigFileAsync().Wait();
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

            binded.ValueChanged += AutoSaveConfiguration;
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
            binded.Value = int.TryParse(ini[name], out var value) ? value : defaultVal;
            binded.ValueChanged += AutoSaveConfiguration;
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
                            binded.Value = defaultVal;
                        }

                        break;
                    case SpecialConfigType.Path:
                        binded.Value = File.Exists(parsedVal) ? parsedVal : defaultVal;
                        break;
                    case SpecialConfigType.Skin:
                        break;
                    default:
                        binded.Value = defaultVal;
                        break;
                }
            }
            catch (Exception e)
            {
                binded.Value = defaultVal;
            }

            binded.ValueChanged += AutoSaveConfiguration;
            return binded;
        }

        /// <summary>
        ///     Reads a Bindable<GenericKey>.
        /// </summary>
        /// <returns></returns>
        private static Bindable<GenericKey> ReadGenericKey(string name, GenericKey defaultVal, KeyDataCollection ini)
        {
            var binded = new Bindable<GenericKey>(name, defaultVal);

            GenericKey key;

            if (GenericKey.TryParse(ini[name], out key))
                binded.Value = key;

            binded.ValueChanged += AutoSaveConfiguration;
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
                var sw = new StreamWriter(_gameDirectory + "/quaver.cfg")
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
                    var sw = new StreamWriter(_gameDirectory + "/quaver.cfg")
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

            LastWrite = GameBase.Game?.TimeRunning ?? -1;
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
        Arrow,
        Bar,
        Circle,
    }
}
