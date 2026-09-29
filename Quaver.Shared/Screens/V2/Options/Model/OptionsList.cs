using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Form;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Helpers;
using Quaver.Shared.Input.Global;
using Quaver.Shared.Options;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Options.Model
{
    internal static class OptionsList
    {
        private const int RecentOptionsLimit = 15;

        public static IReadOnlyList<OptionsDefinition> All { get; } = CreateAll();

        private static IReadOnlyDictionary<string, OptionsDefinition> ById { get; } =
            All.ToDictionary(option => option.Id, StringComparer.Ordinal);

        public static IReadOnlyList<OptionsDefinition> Recent =>
            GroupBySection(GetRecentIds().Select(id => ById[id]));

        public static IReadOnlyList<OptionsDefinition> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Array.Empty<OptionsDefinition>();

            var term = query.Trim();
            return GroupBySection(All.Where(option =>
                MatchesSearch(GetOptionLabel(option), term) ||
                MatchesSearch(LocalizationManager.Get(option.SectionName), term) ||
                MatchesSearch(LocalizationManager.Get($"Screen_Options_{option.Category}"), term)));
        }

        private static IReadOnlyList<OptionsDefinition> GroupBySection(IEnumerable<OptionsDefinition> options) =>
            options.GroupBy(option => (option.Category, option.SectionName))
                .SelectMany(section => section)
                .ToList();

        private static string GetOptionLabel(OptionsDefinition option) => option.LabelKeyCount.HasValue
            ? LocalizationManager.Get(option.LabelName, option.LabelKeyCount.Value)
            : LocalizationManager.Get(option.LabelName);

        private static bool MatchesSearch(string text, string term) =>
            text.IndexOf(term, StringComparison.CurrentCultureIgnoreCase) >= 0;

        public static void RecordChanged(string id)
        {
            if (ConfigManager.RecentlyChangedOptions == null || !ById.ContainsKey(id))
                return;

            var ids = GetRecentIds().Where(recentId => recentId != id).Prepend(id).Take(RecentOptionsLimit);
            var value = string.Join(",", ids);
            if (ConfigManager.RecentlyChangedOptions.Value != value)
                ConfigManager.RecentlyChangedOptions.Value = value;
        }

        public static Drawable CreateControl(OptionsDefinition option, Container host)
        {
            var control = option.ControlFactory?.Invoke(host);
            switch (control)
            {
                case ToggleV2 toggle:
                    toggle.ValueEdited += (_, _) => RecordChanged(option.Id);
                    break;
                case SliderV2 slider:
                    slider.ValueEdited += (_, _) => RecordChanged(option.Id);
                    break;
                case InputRecorderV2 recorder:
                    recorder.ValueEdited += (_, _) => RecordChanged(option.Id);
                    break;
                case V2DropdownBase dropdown:
                    dropdown.ValueEdited += (_, _) => RecordChanged(option.Id);
                    break;
            }

            return control;
        }

        private static IReadOnlyList<string> GetRecentIds() =>
            (ConfigManager.RecentlyChangedOptions?.Value ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => id.Trim())
            .Where(ById.ContainsKey)
            .Distinct(StringComparer.Ordinal)
            .Take(RecentOptionsLimit)
            .ToList();

        private static IReadOnlyList<OptionsDefinition> CreateAll()
        {
            var options = new List<OptionsDefinition>();

            // Video
            AddSection(options, OptionCategory.Video, "Window",
                ("video.window.screen-resolution", "ScreenResolution", OptionsV2Dropdowns.ScreenResolution),
                ("video.window.fullscreen", "EnableFullscreen", Toggle(() => ConfigManager.WindowFullScreen)),
                ("video.window.borderless", "EnableBorderlessWindow", Toggle(() => ConfigManager.WindowBorderless)));

            AddSection(options, OptionCategory.Video, "FrameTime",
                ("video.frame-time.limiter", "FrameLimiter", OptionsV2Dropdowns.FrameLimiter),
                ("video.frame-time.custom-fps", "SetCustomFPS", ActionButton("SET FPS", () => OptionsActions.SetCustomFps(onChanged: () => RecordChanged("video.frame-time.custom-fps")))),
                ("video.frame-time.fps-counter", "DisplayFPSCounter", Toggle(() => ConfigManager.FpsCounter)));

            // Audio
            AddSection(options, OptionCategory.Audio, "Output",
                ("audio.output.device", "AudioOutputDevice", OptionsV2Dropdowns.AudioOutputDevice));

            AddSection(options, OptionCategory.Audio, "Volume",
                ("audio.volume.master", "MasterVolume", Slider(() => ConfigManager.VolumeGlobal)),
                ("audio.volume.music", "MusicVolume", Slider(() => ConfigManager.VolumeMusic)),
                ("audio.volume.menu-music", "MenuMusicVolume", Slider(() => ConfigManager.VolumeMenuMusic)),
                ("audio.volume.effects", "EffectVolume", Slider(() => ConfigManager.VolumeEffect)),
                ("audio.volume.mute-unfocused", "MuteAudioWhenUnfocused", Toggle(() => ConfigManager.MuteAudioOnWindowInactive)));

            AddSection(options, OptionCategory.Audio, "Offset",
                ("audio.offset.global", "GlobalAudioOffset", Slider(() => ConfigManager.GlobalAudioOffset)),
                ("audio.offset.visual", "VisualOffset", Slider(() => ConfigManager.VisualOffset)),
                ("audio.offset.calibrate", "CalibrateOffset", ActionButton("CALIBRATE", OptionsActions.CalibrateOffset)));

            AddSection(options, OptionCategory.Audio, "Effects",
                ("audio.effects.pitch-with-rate", "PitchAudioWithPlaybackRate", Toggle(() => ConfigManager.Pitched)));

            // Gameplay
            AddSection(options, OptionCategory.Gameplay, "Background",
                ("gameplay.background.brightness", "BackgroundBrightness", Slider(() => ConfigManager.BackgroundBrightness)));

            AddSection(options, OptionCategory.Gameplay, "Visuals",
                ("gameplay.visuals.long-note-shrink", "LongNoteShrinkAmount", Slider(() => ConfigManager.PercyAmount)));

            AddSection(options, OptionCategory.Gameplay, "Sound",
                ("gameplay.sound.hitsounds", "EnableHitsounds", Toggle(() => ConfigManager.EnableHitsounds)),
                ("gameplay.sound.ln-release-hitsounds", "EnableLongNoteReleaseHitsounds", Toggle(() => ConfigManager.EnableLongNoteReleaseHitsounds)),
                ("gameplay.sound.keysounds", "EnableKeysounds", Toggle(() => ConfigManager.EnableKeysounds)));

            AddSection(options, OptionCategory.Gameplay, "Input",
                ("gameplay.input.tap-to-pause", "EnableTapToPause", Toggle(() => ConfigManager.TapToPause)),
                ("gameplay.input.tap-to-restart", "EnableTapToRestart", Toggle(() => ConfigManager.TapToRestart)),
                ("gameplay.input.skip-results-after-quit", "SkipResultsScreenAfterQuitting", Toggle(() => ConfigManager.SkipResultsScreenAfterQuit)),
                ("gameplay.input.lock-windows-key", "LockWindowsKeyduringgameplay", Toggle(() => ConfigManager.LockWinkeyDuringGameplay)));

            AddSection(options, OptionCategory.Gameplay, "UserInterface",
                ("gameplay.ui.timing-lines", "DisplayTimingLines", Toggle(() => ConfigManager.DisplayTimingLines)),
                ("gameplay.ui.hit-bubbles", "DisplayHitBubbles", Toggle(() => ConfigManager.DisplayHitBubbles)),
                ("gameplay.ui.judgement-counter", "DisplayJudgementCounter", Toggle(() => ConfigManager.DisplayJudgementCounter)));

            AddSection(options, OptionCategory.Gameplay, "Scoreboard",
                ("gameplay.scoreboard.visible", "DisplayScoreboard", Toggle(() => ConfigManager.ScoreboardVisible)));

            AddSection(options, OptionCategory.Gameplay, "ProgressBar",
                ("gameplay.progress-bar.visible", "DisplaySongTimeProgressBar", Toggle(() => ConfigManager.DisplaySongTimeProgress)),
                ("gameplay.progress-bar.time-numbers", "DisplaySongTimeProgressBarTimeNumbers", Toggle(() => ConfigManager.DisplaySongTimeProgressNumbers)));

            AddSection(options, OptionCategory.Gameplay, "LaneCover",
                ("gameplay.lane-cover.top", "EnableTopLaneCover", Toggle(() => ConfigManager.LaneCoverTop)),
                ("gameplay.lane-cover.top-height", "TopLaneCoverHeight", Slider(() => ConfigManager.LaneCoverTopHeight)),
                ("gameplay.lane-cover.bottom", "EnableBottomLaneCover", Toggle(() => ConfigManager.LaneCoverBottom)),
                ("gameplay.lane-cover.bottom-height", "BottomLaneCoverHeight", Slider(() => ConfigManager.LaneCoverBottomHeight)),
                ("gameplay.lane-cover.ui-over", "DisplayUIElementsOverLaneCovers", Toggle(() => ConfigManager.UIElementsOverLaneCover)),
                ("gameplay.lane-cover.receptors-over", "DisplayReceptorsOverLaneCovers", Toggle(() => ConfigManager.ReceptorsOverLaneCover)));

            // Skin
            AddSection(options, OptionCategory.Skin, "Selection",
                ("skin.selection.custom", "CustomSkin", host => OptionsV2Dropdowns.CustomSkin(host, ConfigManager.Skin)),
                ("skin.selection.coop-player-2", "CoopPlayer2Skin", host => OptionsV2Dropdowns.CustomSkin(host, ConfigManager.TournamentPlayer2Skin)),
                ("skin.selection.default", "DefaultSkin", OptionsV2Dropdowns.DefaultSkin));

            AddSection(options, OptionCategory.Skin, "Navigation",
                ("skin.navigation.open-folder", "OpenSkinFolder", ActionButton("OPEN FOLDER", OptionsActions.OpenSkinFolder)),
                ("skin.navigation.generate-v2-config", "GenerateEditableskinyml", ActionButton("GENERATE", () => OptionsActions.GenerateSkinConfig())));

            AddSection(options, OptionCategory.Skin, "Sharing",
                ("skin.sharing.export", "ExportSkin", ActionButton("EXPORT SKIN", OptionsActions.ExportSkin)),
                ("skin.sharing.upload-workshop", "UploadSkinToSteamWorkshop", ActionButton("UPLOAD", OptionsActions.UploadSkin, "#27B06E")));

            AddSection(options, OptionCategory.Skin, "Configuration",
                ("skin.configuration.note-scale", "NoteReceptorSizeScale", Slider(() => ConfigManager.GameplayNoteScale, 2, 100)),
                ("skin.configuration.playfield-scale", "PlayfieldScale", Slider(() => ConfigManager.PlayfieldScale, 2, 100)),
                ("skin.configuration.song-select-banners", "DisplaySongSelectBanners", Toggle(() => ConfigManager.DisplaySongSelectBanners)),
                ("skin.configuration.judgement-hitlighting", "TintHitlightingBasedOnJudgementColor", Toggle(() => ConfigManager.TintHitLightingBasedOnJudgementColor)));

            // Input
            AddSection(options, OptionCategory.Input, "GameplayControls",
                ("input.gameplay.pause", "Pause", Keybind(GlobalKeybindActions.GameplayPause)),
                ("input.gameplay.quick-restart", "QuickRestart", Keybind(GlobalKeybindActions.GameplayRetry)),
                ("input.gameplay.quick-exit", "QuickExit", Keybind(GlobalKeybindActions.GameplayQuickExit)),
                ("input.gameplay.skip-intro", "SkipSongIntro", Keybind(GlobalKeybindActions.GameplaySkipIntro)),
                ("input.gameplay.decrease-scroll-1", "DecreaseScrollSpeed1", Keybind(GlobalKeybindActions.DecreaseScrollSpeed)),
                ("input.gameplay.increase-scroll-1", "IncreaseScrollSpeed1", Keybind(GlobalKeybindActions.IncreaseScrollSpeed)),
                ("input.gameplay.decrease-scroll-01", "DecreaseScrollSpeed01", Keybind(GlobalKeybindActions.DecreaseScrollSpeedSmall)),
                ("input.gameplay.increase-scroll-01", "IncreaseScrollSpeed01", Keybind(GlobalKeybindActions.IncreaseScrollSpeedSmall)),
                ("input.gameplay.decrease-local-scroll-1", "DecreaseLocalScrollSpeed1", Keybind(GlobalKeybindActions.DecreaseLocalScrollSpeed)),
                ("input.gameplay.increase-local-scroll-1", "IncreaseLocalScrollSpeed1", Keybind(GlobalKeybindActions.IncreaseLocalScrollSpeed)),
                ("input.gameplay.decrease-local-scroll-01", "DecreaseLocalScrollSpeed01", Keybind(GlobalKeybindActions.DecreaseLocalScrollSpeedSmall)),
                ("input.gameplay.increase-local-scroll-01", "IncreaseLocalScrollSpeed01", Keybind(GlobalKeybindActions.IncreaseLocalScrollSpeedSmall)),
                ("input.gameplay.decrease-map-offset-5", "DecreaseMapOffset5ms", Keybind(GlobalKeybindActions.DecreaseOffset)),
                ("input.gameplay.increase-map-offset-5", "IncreaseMapOffset5ms", Keybind(GlobalKeybindActions.IncreaseOffset)),
                ("input.gameplay.decrease-map-offset-1", "DecreaseMapOffset1ms", Keybind(GlobalKeybindActions.DecreaseOffsetSmall)),
                ("input.gameplay.increase-map-offset-1", "IncreaseMapOffset1ms", Keybind(GlobalKeybindActions.IncreaseOffsetSmall)),
                ("input.gameplay.decrease-visual-offset-5", "DecreaseVisualOffset5ms", Keybind(GlobalKeybindActions.DecreaseVisualOffset)),
                ("input.gameplay.increase-visual-offset-5", "IncreaseVisualOffset5ms", Keybind(GlobalKeybindActions.IncreaseVisualOffset)),
                ("input.gameplay.decrease-visual-offset-1", "DecreaseVisualOffset1ms", Keybind(GlobalKeybindActions.DecreaseVisualOffsetSmall)),
                ("input.gameplay.increase-visual-offset-1", "IncreaseVisualOffset1ms", Keybind(GlobalKeybindActions.IncreaseVisualOffsetSmall)),
                ("input.gameplay.reset-map-offset", "ResetMapOffset", Keybind(GlobalKeybindActions.ResetOffset)),
                ("input.gameplay.reset-visual-offset", "ResetVisualOffset", Keybind(GlobalKeybindActions.ResetVisualOffset)));

            AddSection(options, OptionCategory.Input, "GameplayUserInterface",
                ("input.gameplay-ui.toggle-scoreboard", "ToggleScoreboardVisibility", Keybind(GlobalKeybindActions.GameplayToggleScoreboard)));

            AddSection(options, OptionCategory.Input, "UserInterface",
                ("input.ui.open-options", "OpenOptions", Keybind(GlobalKeybindActions.OpenOptions)),
                ("input.ui.toggle-fullscreen", "ToggleFullscreen", Keybind(GlobalKeybindActions.ToggleFullscreen)),
                ("input.ui.toggle-chat", "ToggleChatOverlay", Keybind(GlobalKeybindActions.GameplayToggleOverlay)),
                ("input.ui.toggle-online-hub", "ToggleOnlineHub", Keybind(GlobalKeybindActions.ToggleOnlineHub)),
                ("input.ui.pause-music", "PauseUnpauseMusic", Keybind(GlobalKeybindActions.TogglePause)));

            AddSection(options, OptionCategory.Input, "SongSelection",
                ("input.song-selection.decrease-rate", "DecreaseGameplayRate", Keybind(GlobalKeybindActions.DecreaseRate)),
                ("input.song-selection.increase-rate", "IncreaseGameplayRate", Keybind(GlobalKeybindActions.IncreaseRate)),
                ("input.song-selection.decrease-rate-half", "DecreaseGameplayRateHalfStep", Keybind(GlobalKeybindActions.DecreaseRateSmall)),
                ("input.song-selection.increase-rate-half", "IncreaseGameplayRateHalfStep", Keybind(GlobalKeybindActions.IncreaseRateSmall)),
                ("input.song-selection.toggle-mirror", "ToggleMirrorMod", Keybind(GlobalKeybindActions.ToggleMirror)),
                ("input.song-selection.toggle-pitch", "TogglePitch", Keybind(GlobalKeybindActions.TogglePitch)),
                ("input.song-selection.remove-mods", "RemoveAllMods", Keybind(GlobalKeybindActions.RemoveMods)),
                ("input.song-selection.toggle-modifiers", "ToggleModifiersPanel", Keybind(GlobalKeybindActions.SelectionToggleModifiers)),
                ("input.song-selection.random-map", "SelectRandomMap", Keybind(GlobalKeybindActions.SelectionSelectRandomMap)),
                ("input.song-selection.toggle-map-preview", "ToggleMapPreviewPanel", Keybind(GlobalKeybindActions.SelectionToggleMapPreview)),
                ("input.song-selection.toggle-profile", "ToggleUserProfilePanel", Keybind(GlobalKeybindActions.SelectionToggleUserProfile)),
                ("input.song-selection.refresh-songs", "RefreshSongs", Keybind(GlobalKeybindActions.SelectionRefresh)));

            AddSection(options, OptionCategory.Input, "Misc",
                ("input.misc.screenshot", "TakeScreenshot", Keybind(GlobalKeybindActions.Screenshot)),
                ("input.misc.cycle-fps-limiter", "CycleFPSLimiter", Keybind(GlobalKeybindActions.CycleFpsLimiter)),
                ("input.misc.reload-skin", "ReloadSkin", Keybind(GlobalKeybindActions.ReloadSkin)));

            foreach (var mode in ModeHelper.AllModes)
            {
                var keyCount = ModeHelper.ToKeyCount(mode);
                var section = $"Screen_Options_{keyCount}Keys";
                var idPrefix = $"input.{keyCount}k";
                options.Add(new OptionsDefinition($"{idPrefix}.gameplay-layout", OptionCategory.Input, section, "Screen_Options_KeyCountGameplayLayout", keyCount, Keys(() => ConfigManager.KeyLayouts[mode])));
                options.Add(new OptionsDefinition($"{idPrefix}.scroll-speed", OptionCategory.Input, section, "Screen_Options_KeyCountScrollSpeed", keyCount, Slider(() => ConfigManager.ScrollSpeeds[mode], 1, 10)));
                options.Add(new OptionsDefinition($"{idPrefix}.scroll-direction", OptionCategory.Input, section, "Screen_Options_KeyCountScrollDirection", keyCount, host => OptionsV2Dropdowns.ScrollDirection(host, mode)));
                options.Add(new OptionsDefinition($"{idPrefix}.scratch-layout", OptionCategory.Input, section, "Screen_Options_KeyCountScratchLayout", keyCount, Keys(() => ConfigManager.ScratchKeyLayouts[mode])));
                options.Add(new OptionsDefinition($"{idPrefix}.scratch-lane-left", OptionCategory.Input, section, "Screen_Options_KeyCountPlaceScratchLaneOnLeft", keyCount, Toggle(() => ConfigManager.ScratchLanesLeft[mode])));
                options.Add(new OptionsDefinition($"{idPrefix}.coop-layout", OptionCategory.Input, section, "Screen_Options_KeyCountCoopLayout", keyCount, Keys(() => ConfigManager.CoopKeyLayouts[mode])));
            }

            // Miscellaneous
            AddSection(options, OptionCategory.Miscellaneous, "Localization",
                ("misc.localization.language", "Language", OptionsV2Dropdowns.Language));

            AddSection(options, OptionCategory.Miscellaneous, "NavigationMaintenance",
                ("misc.navigation.open-game-folder", "OpenGameFolder", ActionButton("OPEN FOLDER", OptionsActions.OpenGameFolder)),
                ("misc.navigation.update-ranked-statuses", "UpdateMapRankedStatuses", ActionButton("UPDATE", () => OptionsActions.UpdateRankedStatuses(), "#F2994A")),
                ("misc.navigation.update-online-offsets", "UpdateMapOnlineOffsets", ActionButton("UPDATE", OptionsActions.UpdateOnlineOffsets, "#F2994A")));

            AddSection(options, OptionCategory.Miscellaneous, "InstalledGames",
                ("misc.installed-games.auto-load", "LoadSongsFromOtherInstalledGames", Toggle(() => ConfigManager.AutoLoadOsuBeatmaps)),
                ("misc.installed-games.detect", "DetectSongsFromOtherInstalledGames", ActionButton("DETECT", () => OptionsActions.DetectOtherGames(() => RecordChanged("misc.installed-games.detect")))));

            AddSection(options, OptionCategory.Miscellaneous, "Notifications",
                ("misc.notifications.bottom-to-top", "DisplayNotificationsFromBottomToTop", Toggle(() => ConfigManager.DisplayNotificationsBottomToTop)),
                ("misc.notifications.friends-online", "DisplayOnlineFriendNotifications", Toggle(() => ConfigManager.DisplayFriendOnlineNotifications)));

            AddSection(options, OptionCategory.Miscellaneous, "SongSelect",
                ("misc.song-select.prioritized-mode", "PrioritizedGameMode", OptionsV2Dropdowns.GameMode),
                ("misc.song-select.suggest-difficulty", "SuggestDifficultyfromOverallRating", ActionButton("CALIBRATE", () => OptionsActions.SuggestDifficulty(() => RecordChanged("misc.song-select.suggest-difficulty")))));

            foreach (var mode in ModeHelper.AllModes)
            {
                var count = ModeHelper.ToKeyCount(mode);
                options.Add(new OptionsDefinition($"misc.song-select.prioritized-{count}k-difficulty",
                    OptionCategory.Miscellaneous, "Screen_Options_SongSelect",
                    "Screen_Options_KeyCountPrioritizedDifficulty", count,
                    Slider(() => ConfigManager.PrioritizedMapDifficulty[mode], 1, 10)));
            }

            // Advanced
            AddSection(options, OptionCategory.Advanced, "Video",
                ("advanced.video.lower-fps-unfocused", "LowerFPSOnInactiveWindow", Toggle(() => ConfigManager.LowerFpsOnWindowInactive)),
                ("advanced.video.high-process-priority", "EnableHighProcessPriority", Toggle(() => ConfigManager.EnableHighProcessPriority)));

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                options.Add(new OptionsDefinition("advanced.video.prefer-wayland", OptionCategory.Advanced,
                    "Screen_Options_Video", "Screen_Options_PreferWayland", controlFactory: Toggle(() => ConfigManager.PreferWayland)));
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                options.Add(new OptionsDefinition("advanced.video.prefer-macos-input", OptionCategory.Advanced,
                    "Screen_Options_Video", "Screen_Options_PrefermacOSInputHandling", controlFactory: Toggle(() => ConfigManager.PreferCocoaEventLoop)));

            AddSection(options, OptionCategory.Advanced, "Video",
                ("advanced.video.editor-imgui-scale", "EditorImGuiScale", Slider(() => ConfigManager.EditorImGuiScalePercentage)),
                ("advanced.video.editor-imgui-font-size", "EditorImGuiFontSize", Slider(() => ConfigManager.EditorImGuiFontSize)));
            AddSection(options, OptionCategory.Advanced, "Audio",
                ("advanced.audio.smooth-timing", "UseSmoothAudioFrameTimingDuringGameplay", Toggle(() => ConfigManager.SmoothAudioTimingGameplay)));

            AddSection(options, OptionCategory.Advanced, "Gameplay",
                ("advanced.gameplay.overlay", "DisplayGameplayOverlayShiftF6", Toggle(() => ConfigManager.DisplayGameplayOverlay)),
                ("advanced.gameplay.notifications", "DisplayNotificationsDuringGameplay", Toggle(() => ConfigManager.DisplayNotificationsInGameplay)),
                ("advanced.gameplay.spectators", "ShowSpectators", Toggle(() => ConfigManager.ShowSpectators)),
                ("advanced.gameplay.ranked-accuracy-custom-judgements", "DisplayRankedAccuracyWithCustomJudgements", Toggle(() => ConfigManager.DisplayRankedAccuracy)),
                ("advanced.gameplay.hit-error-fade-time", "HitErrorFadeTime", Slider(() => ConfigManager.HitErrorFadeTime, 1, 1000)),
                ("advanced.gameplay.hit-error-timing-color", "HitErrorEarlyLateColoring", Toggle(() => ConfigManager.ColorHitErrorByTiming)),
                ("advanced.gameplay.hit-error-timing-window", "HitErrorEarlyLateWindow", Slider(() => ConfigManager.HitErrorEarlyLateWindow)),
                ("advanced.gameplay.combo-alerts", "EnableComboAlerts", Toggle(() => ConfigManager.DisplayComboAlerts)),
                ("advanced.gameplay.unbeatable-scores", "DisplayUnbeatableScores", Toggle(() => ConfigManager.DisplayUnbeatableScoresDuringGameplay)),
                ("advanced.gameplay.keep-playing-on-fail", "KeepPlayingUponFailing", Toggle(() => ConfigManager.KeepPlayingUponFailing)),
                ("advanced.gameplay.normalize-scroll-by-rate", "NormaliseScrollVelocityByRatePercentage", Slider(() => ConfigManager.NormaliseScrollVelocityByRatePercentage)));

            AddSection(options, OptionCategory.Advanced, "Skin",
                ("advanced.skin.tournament-overlay", "Display1v1TournamentOverlay", Toggle(() => ConfigManager.Display1v1TournamentOverlay)),
                ("advanced.skin.playfield-scores", "Display1v1PlayfieldScores", Toggle(() => ConfigManager.TournamentDisplay1v1PlayfieldScores)),
                ("advanced.skin.reload-on-change", "ReloadSkinOnChange", Toggle(() => ConfigManager.ReloadSkinOnChange)));

            AddSection(options, OptionCategory.Advanced, "Input",
                ("advanced.input.toggle-playtest-autoplay", "TogglePlaytestAutoplay", Keybind(GlobalKeybindActions.GameplayTogglePlaytestAutoplay)),
                ("advanced.input.invert-editor-scrolling", "InvertEditorScrolling", Toggle(() => ConfigManager.InvertEditorScrolling)),
                ("advanced.input.invert-scrolling", "InvertScrolling", Toggle(() => ConfigManager.InvertScrolling)));

            AddSection(options, OptionCategory.Advanced, "Miscellaneous",
                ("advanced.misc.auto-login", "AutomaticallyLogIntoTheServer", Toggle(() => ConfigManager.AutoLoginToServer)),
                ("advanced.misc.song-request-notifications", "DisplaySongRequestNotifications", Toggle(() => ConfigManager.DisplaySongRequestNotifications)),
                ("advanced.misc.pause-warning", "DisplayWarningForPausing", Toggle(() => ConfigManager.DisplayPauseWarning)),
                ("advanced.misc.fail-warning", "DisplayWarningForFailing", Toggle(() => ConfigManager.DisplayFailWarning)),
                ("advanced.misc.epilepsy-warning", "DisplayEpilepsyWarning", Toggle(() => ConfigManager.DisplayEpilepsyWarning)),
                ("advanced.misc.menu-audio-visualizer", "DisplayMenuAudioVisualizer", Toggle(() => ConfigManager.DisplayMenuAudioVisualizer)),
                ("advanced.misc.failed-local-scores", "DisplayFailedLocalScores", Toggle(() => ConfigManager.DisplayFailedLocalScores)),
                ("advanced.misc.delete-original-on-import", "DeleteOriginalFileAfterImport", Toggle(() => ConfigManager.DeleteOriginalFileAfterImport)),
                ("advanced.misc.discord-presence", "DiscordRichPresence", Toggle(() => ConfigManager.DiscordRichPresence)),
                ("advanced.misc.local-leaderboard-ranked-accuracy", "DisplayRankedAccuracyForLocalLeaderboards", Toggle(() => ConfigManager.LeaderboardRankedAccuracy)));

            return options.AsReadOnly();
        }

        private static void AddSection(List<OptionsDefinition> options, OptionCategory category, string sectionName, params (string Id, string LabelName, Func<Container, Drawable> ControlFactory)[] entries)
        {
            var sectionKey = "Screen_Options_" + sectionName;
            foreach (var entry in entries)
                options.Add(new OptionsDefinition(entry.Id, category, sectionKey, "Screen_Options_" + entry.LabelName, controlFactory: entry.ControlFactory));
        }

        private static Func<Container, Drawable> Toggle(Func<Bindable<bool>> value) => _ => new ToggleV2(value());

        private static Func<Container, Drawable> Slider(Func<BindableInt> value, int precision = 0, float displayScale = 1f) => _ => new SliderV2(value(), new ScalableVector2(180, 22), precision, displayScale);

        private static Func<Container, Drawable> Keybind(GlobalKeybindActions action) => _ => new InputRecorderV2(action);

        private static Func<Container, Drawable> Keys(Func<List<Bindable<GenericKey>>> values) => _ => new InputRecorderV2(values());

        private static Func<Container, Drawable> ActionButton(string label, Action action, string tint = "#0FBAE5") => _ =>
        {
            var button = new RoundedButton
            {
                Size = new ScalableVector2(183, 31),
                CornerRadius = 6,
                Tint = ColorHelper.HexToColor(tint)
            };
            button.SetLabel(FontManager.GetWobbleFont(Fonts.InterSemiBold), label, 18, Color.White);
            button.Clicked += (sender, args) => action();
            return button;
        };
    }
}
