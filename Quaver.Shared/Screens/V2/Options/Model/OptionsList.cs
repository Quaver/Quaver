using System.Collections.Generic;
using System.Runtime.InteropServices;
using Quaver.API.Helpers;

namespace Quaver.Shared.Screens.V2.Options.Model
{
    internal static class OptionsList
    {
        public static IReadOnlyList<OptionsDefinition> All { get; } = CreateAll();

        private static IReadOnlyList<OptionsDefinition> CreateAll()
        {
            var options = new List<OptionsDefinition>();

            // Video
            AddSection(options, OptionCategory.Video, "Window",
                ("video.window.screen-resolution", "ScreenResolution"),
                ("video.window.fullscreen", "EnableFullscreen"),
                ("video.window.borderless", "EnableBorderlessWindow"));

            AddSection(options, OptionCategory.Video, "FrameTime",
                ("video.frame-time.limiter", "FrameLimiter"),
                ("video.frame-time.custom-fps", "SetCustomFPS"),
                ("video.frame-time.fps-counter", "DisplayFPSCounter"));

            // Audio
            AddSection(options, OptionCategory.Audio, "Output",
                ("audio.output.device", "AudioOutputDevice"));

            AddSection(options, OptionCategory.Audio, "Volume",
                ("audio.volume.master", "MasterVolume"),
                ("audio.volume.music", "MusicVolume"),
                ("audio.volume.menu-music", "MenuMusicVolume"),
                ("audio.volume.effects", "EffectVolume"),
                ("audio.volume.mute-unfocused", "MuteAudioWhenUnfocused"));

            AddSection(options, OptionCategory.Audio, "Offset",
                ("audio.offset.global", "GlobalAudioOffset"),
                ("audio.offset.visual", "VisualOffset"),
                ("audio.offset.calibrate", "CalibrateOffset"));

            AddSection(options, OptionCategory.Audio, "Effects",
                ("audio.effects.pitch-with-rate", "PitchAudioWithPlaybackRate"));

            // Gameplay
            AddSection(options, OptionCategory.Gameplay, "Background",
                ("gameplay.background.brightness", "BackgroundBrightness"));

            AddSection(options, OptionCategory.Gameplay, "Visuals",
                ("gameplay.visuals.long-note-shrink", "LongNoteShrinkAmount"));

            AddSection(options, OptionCategory.Gameplay, "Sound",
                ("gameplay.sound.hitsounds", "EnableHitsounds"),
                ("gameplay.sound.ln-release-hitsounds", "EnableLongNoteReleaseHitsounds"),
                ("gameplay.sound.keysounds", "EnableKeysounds"));

            AddSection(options, OptionCategory.Gameplay, "Input",
                ("gameplay.input.tap-to-pause", "EnableTapToPause"),
                ("gameplay.input.tap-to-restart", "EnableTapToRestart"),
                ("gameplay.input.skip-results-after-quit", "SkipResultsScreenAfterQuitting"),
                ("gameplay.input.lock-windows-key", "LockWindowsKeyduringgameplay"));

            AddSection(options, OptionCategory.Gameplay, "UserInterface",
                ("gameplay.ui.timing-lines", "DisplayTimingLines"),
                ("gameplay.ui.hit-bubbles", "DisplayHitBubbles"),
                ("gameplay.ui.judgement-counter", "DisplayJudgementCounter"));

            AddSection(options, OptionCategory.Gameplay, "Scoreboard",
                ("gameplay.scoreboard.visible", "DisplayScoreboard"));

            AddSection(options, OptionCategory.Gameplay, "ProgressBar",
                ("gameplay.progress-bar.visible", "DisplaySongTimeProgressBar"),
                ("gameplay.progress-bar.time-numbers", "DisplaySongTimeProgressBarTimeNumbers"));

            AddSection(options, OptionCategory.Gameplay, "LaneCover",
                ("gameplay.lane-cover.top", "EnableTopLaneCover"),
                ("gameplay.lane-cover.top-height", "TopLaneCoverHeight"),
                ("gameplay.lane-cover.bottom", "EnableBottomLaneCover"),
                ("gameplay.lane-cover.bottom-height", "BottomLaneCoverHeight"),
                ("gameplay.lane-cover.ui-over", "DisplayUIElementsOverLaneCovers"),
                ("gameplay.lane-cover.receptors-over", "DisplayReceptorsOverLaneCovers"));

            // Skin
            AddSection(options, OptionCategory.Skin, "Selection",
                ("skin.selection.custom", "CustomSkin"),
                ("skin.selection.coop-player-2", "CoopPlayer2Skin"),
                ("skin.selection.default", "DefaultSkin"));

            AddSection(options, OptionCategory.Skin, "Navigation",
                ("skin.navigation.open-folder", "OpenSkinFolder"),
                ("skin.navigation.generate-v2-config", "GenerateEditableskinyml"));

            AddSection(options, OptionCategory.Skin, "Sharing",
                ("skin.sharing.export", "ExportSkin"),
                ("skin.sharing.upload-workshop", "UploadSkinToSteamWorkshop"));

            AddSection(options, OptionCategory.Skin, "Configuration",
                ("skin.configuration.note-scale", "NoteReceptorSizeScale"),
                ("skin.configuration.playfield-scale", "PlayfieldScale"),
                ("skin.configuration.song-select-banners", "DisplaySongSelectBanners"),
                ("skin.configuration.judgement-hitlighting", "TintHitlightingBasedOnJudgementColor"));

            // Input
            AddSection(options, OptionCategory.Input, "GameplayControls",
                ("input.gameplay.pause", "Pause"),
                ("input.gameplay.quick-restart", "QuickRestart"),
                ("input.gameplay.quick-exit", "QuickExit"),
                ("input.gameplay.skip-intro", "SkipSongIntro"),
                ("input.gameplay.decrease-scroll-1", "DecreaseScrollSpeed1"),
                ("input.gameplay.increase-scroll-1", "IncreaseScrollSpeed1"),
                ("input.gameplay.decrease-scroll-01", "DecreaseScrollSpeed01"),
                ("input.gameplay.increase-scroll-01", "IncreaseScrollSpeed01"),
                ("input.gameplay.decrease-local-scroll-1", "DecreaseLocalScrollSpeed1"),
                ("input.gameplay.increase-local-scroll-1", "IncreaseLocalScrollSpeed1"),
                ("input.gameplay.decrease-local-scroll-01", "DecreaseLocalScrollSpeed01"),
                ("input.gameplay.increase-local-scroll-01", "IncreaseLocalScrollSpeed01"),
                ("input.gameplay.decrease-map-offset-5", "DecreaseMapOffset5ms"),
                ("input.gameplay.increase-map-offset-5", "IncreaseMapOffset5ms"),
                ("input.gameplay.decrease-map-offset-1", "DecreaseMapOffset1ms"),
                ("input.gameplay.increase-map-offset-1", "IncreaseMapOffset1ms"),
                ("input.gameplay.decrease-visual-offset-5", "DecreaseVisualOffset5ms"),
                ("input.gameplay.increase-visual-offset-5", "IncreaseVisualOffset5ms"),
                ("input.gameplay.decrease-visual-offset-1", "DecreaseVisualOffset1ms"),
                ("input.gameplay.increase-visual-offset-1", "IncreaseVisualOffset1ms"),
                ("input.gameplay.reset-map-offset", "ResetMapOffset"),
                ("input.gameplay.reset-visual-offset", "ResetVisualOffset"));

            AddSection(options, OptionCategory.Input, "GameplayUserInterface",
                ("input.gameplay-ui.toggle-scoreboard", "ToggleScoreboardVisibility"));

            AddSection(options, OptionCategory.Input, "UserInterface",
                ("input.ui.open-options", "OpenOptions"),
                ("input.ui.toggle-fullscreen", "ToggleFullscreen"),
                ("input.ui.toggle-chat", "ToggleChatOverlay"),
                ("input.ui.toggle-online-hub", "ToggleOnlineHub"),
                ("input.ui.pause-music", "PauseUnpauseMusic"));

            AddSection(options, OptionCategory.Input, "SongSelection",
                ("input.song-selection.decrease-rate", "DecreaseGameplayRate"),
                ("input.song-selection.increase-rate", "IncreaseGameplayRate"),
                ("input.song-selection.decrease-rate-half", "DecreaseGameplayRateHalfStep"),
                ("input.song-selection.increase-rate-half", "IncreaseGameplayRateHalfStep"),
                ("input.song-selection.toggle-mirror", "ToggleMirrorMod"),
                ("input.song-selection.toggle-pitch", "TogglePitch"),
                ("input.song-selection.remove-mods", "RemoveAllMods"),
                ("input.song-selection.toggle-modifiers", "ToggleModifiersPanel"),
                ("input.song-selection.random-map", "SelectRandomMap"),
                ("input.song-selection.toggle-map-preview", "ToggleMapPreviewPanel"),
                ("input.song-selection.toggle-profile", "ToggleUserProfilePanel"),
                ("input.song-selection.refresh-songs", "RefreshSongs"));

            AddSection(options, OptionCategory.Input, "Misc",
                ("input.misc.screenshot", "TakeScreenshot"),
                ("input.misc.cycle-fps-limiter", "CycleFPSLimiter"),
                ("input.misc.reload-skin", "ReloadSkin"));

            foreach (var mode in ModeHelper.AllModes)
            {
                var keyCount = ModeHelper.ToKeyCount(mode);
                var section = $"Screen_Options_{keyCount}Keys";
                var idPrefix = $"input.{keyCount}k";
                options.Add(new OptionsDefinition($"{idPrefix}.gameplay-layout", OptionCategory.Input, section, "Screen_Options_KeyCountGameplayLayout", keyCount));
                options.Add(new OptionsDefinition($"{idPrefix}.scroll-speed", OptionCategory.Input, section, "Screen_Options_KeyCountScrollSpeed", keyCount));
                options.Add(new OptionsDefinition($"{idPrefix}.scroll-direction", OptionCategory.Input, section, "Screen_Options_KeyCountScrollDirection", keyCount));
                options.Add(new OptionsDefinition($"{idPrefix}.scratch-layout", OptionCategory.Input, section, "Screen_Options_KeyCountScratchLayout", keyCount));
                options.Add(new OptionsDefinition($"{idPrefix}.scratch-lane-left", OptionCategory.Input, section, "Screen_Options_KeyCountPlaceScratchLaneOnLeft", keyCount));
                options.Add(new OptionsDefinition($"{idPrefix}.coop-layout", OptionCategory.Input, section, "Screen_Options_KeyCountCoopLayout", keyCount));
            }

            // Miscellaneous
            AddSection(options, OptionCategory.Miscellaneous, "Localization",
                ("misc.localization.language", "Language"));

            AddSection(options, OptionCategory.Miscellaneous, "NavigationMaintenance",
                ("misc.navigation.open-game-folder", "OpenGameFolder"),
                ("misc.navigation.update-ranked-statuses", "UpdateMapRankedStatuses"),
                ("misc.navigation.update-online-offsets", "UpdateMapOnlineOffsets"));

            AddSection(options, OptionCategory.Miscellaneous, "InstalledGames",
                ("misc.installed-games.auto-load", "LoadSongsFromOtherInstalledGames"),
                ("misc.installed-games.detect", "DetectSongsFromOtherInstalledGames"));

            AddSection(options, OptionCategory.Miscellaneous, "Notifications",
                ("misc.notifications.bottom-to-top", "DisplayNotificationsFromBottomToTop"),
                ("misc.notifications.friends-online", "DisplayOnlineFriendNotifications"));

            AddSection(options, OptionCategory.Miscellaneous, "SongSelect",
                ("misc.song-select.prioritized-mode", "PrioritizedGameMode"),
                ("misc.song-select.suggest-difficulty", "SuggestDifficultyfromOverallRating"));

            for (var keyCount = 1; keyCount <= ModeHelper.MaxKeyCount; keyCount++)
                options.Add(new OptionsDefinition($"misc.song-select.prioritized-{keyCount}k-difficulty",
                    OptionCategory.Miscellaneous, "Screen_Options_SongSelect",
                    "Screen_Options_KeyCountPrioritizedDifficulty", keyCount));

            // Advanced
            AddSection(options, OptionCategory.Advanced, "Video",
                ("advanced.video.lower-fps-unfocused", "LowerFPSOnInactiveWindow"),
                ("advanced.video.high-process-priority", "EnableHighProcessPriority"));

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                options.Add(new OptionsDefinition("advanced.video.prefer-wayland", OptionCategory.Advanced,
                    "Screen_Options_Video", "Screen_Options_PreferWayland"));
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                options.Add(new OptionsDefinition("advanced.video.prefer-macos-input", OptionCategory.Advanced,
                    "Screen_Options_Video", "Screen_Options_PrefermacOSInputHandling"));

            AddSection(options, OptionCategory.Advanced, "Video",
                ("advanced.video.editor-imgui-scale", "EditorImGuiScale"),
                ("advanced.video.editor-imgui-font-size", "EditorImGuiFontSize"));
            AddSection(options, OptionCategory.Advanced, "Audio",
                ("advanced.audio.smooth-timing", "UseSmoothAudioFrameTimingDuringGameplay"));

            AddSection(options, OptionCategory.Advanced, "Gameplay",
                ("advanced.gameplay.overlay", "DisplayGameplayOverlayShiftF6"),
                ("advanced.gameplay.notifications", "DisplayNotificationsDuringGameplay"),
                ("advanced.gameplay.spectators", "ShowSpectators"),
                ("advanced.gameplay.ranked-accuracy-custom-judgements", "DisplayRankedAccuracyWithCustomJudgements"),
                ("advanced.gameplay.hit-error-fade-time", "HitErrorFadeTime"),
                ("advanced.gameplay.hit-error-timing-color", "HitErrorEarlyLateColoring"),
                ("advanced.gameplay.hit-error-timing-window", "HitErrorEarlyLateWindow"),
                ("advanced.gameplay.combo-alerts", "EnableComboAlerts"),
                ("advanced.gameplay.unbeatable-scores", "DisplayUnbeatableScores"),
                ("advanced.gameplay.keep-playing-on-fail", "KeepPlayingUponFailing"),
                ("advanced.gameplay.normalize-scroll-by-rate", "NormaliseScrollVelocityByRatePercentage"));

            AddSection(options, OptionCategory.Advanced, "Skin",
                ("advanced.skin.tournament-overlay", "Display1v1TournamentOverlay"),
                ("advanced.skin.playfield-scores", "Display1v1PlayfieldScores"),
                ("advanced.skin.reload-on-change", "ReloadSkinOnChange"));

            AddSection(options, OptionCategory.Advanced, "Input",
                ("advanced.input.toggle-playtest-autoplay", "TogglePlaytestAutoplay"),
                ("advanced.input.invert-editor-scrolling", "InvertEditorScrolling"),
                ("advanced.input.invert-scrolling", "InvertScrolling"));

            AddSection(options, OptionCategory.Advanced, "Miscellaneous",
                ("advanced.misc.auto-login", "AutomaticallyLogIntoTheServer"),
                ("advanced.misc.song-request-notifications", "DisplaySongRequestNotifications"),
                ("advanced.misc.pause-warning", "DisplayWarningForPausing"),
                ("advanced.misc.fail-warning", "DisplayWarningForFailing"),
                ("advanced.misc.epilepsy-warning", "DisplayEpilepsyWarning"),
                ("advanced.misc.menu-audio-visualizer", "DisplayMenuAudioVisualizer"),
                ("advanced.misc.failed-local-scores", "DisplayFailedLocalScores"),
                ("advanced.misc.delete-original-on-import", "DeleteOriginalFileAfterImport"),
                ("advanced.misc.discord-presence", "DiscordRichPresence"),
                ("advanced.misc.local-leaderboard-ranked-accuracy", "DisplayRankedAccuracyForLocalLeaderboards"));

            return options.AsReadOnly();
        }

        private static void AddSection(List<OptionsDefinition> options, OptionCategory category, string sectionName, params (string Id, string LabelName)[] entries)
        {
            var sectionKey = "Screen_Options_" + sectionName;
            foreach (var entry in entries)
                options.Add(new OptionsDefinition(entry.Id, category, sectionKey, "Screen_Options_" + entry.LabelName));
        }
    }
}
