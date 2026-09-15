using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.Shared.Config;
using Quaver.Shared.Input.Global;
using ModeHelper = Quaver.API.Helpers.ModeHelper;

namespace Quaver.Shared.Screens.V2.Options.Catalog.Categories
{
    internal static class InputOptions
    {
        /// <summary>
        ///     4K and 7K first because they are played the most, then the other key counts.
        /// </summary>
        private static readonly GameMode[] GameModeOrder =
        {
            GameMode.Keys4, GameMode.Keys7, GameMode.Keys1, GameMode.Keys2, GameMode.Keys3,
            GameMode.Keys5, GameMode.Keys6, GameMode.Keys8, GameMode.Keys9, GameMode.Keys10
        };

        internal static OptionsRowGroup[] Create() => new[]
        {
            new OptionsRowGroup(OptionsCategoryId.Input, "Screen_Options_GameplayControls",
                OptionsRowDefinition.Keybind("Screen_Options_Pause", GlobalKeybindActions.GameplayPause),
                OptionsRowDefinition.Keybind("Screen_Options_QuickRestart", GlobalKeybindActions.GameplayRetry),
                OptionsRowDefinition.Keybind("Screen_Options_QuickExit", GlobalKeybindActions.GameplayQuickExit),
                OptionsRowDefinition.Keybind("Screen_Options_SkipSongIntro", GlobalKeybindActions.GameplaySkipIntro),
                OptionsRowDefinition.Keybind("Screen_Options_DecreaseScrollSpeed1",
                    GlobalKeybindActions.DecreaseScrollSpeed),
                OptionsRowDefinition.Keybind("Screen_Options_IncreaseScrollSpeed1",
                    GlobalKeybindActions.IncreaseScrollSpeed),
                OptionsRowDefinition.Keybind("Screen_Options_DecreaseScrollSpeedSmall",
                    GlobalKeybindActions.DecreaseScrollSpeedSmall),
                OptionsRowDefinition.Keybind("Screen_Options_IncreaseScrollSpeedSmall",
                    GlobalKeybindActions.IncreaseScrollSpeedSmall),
                OptionsRowDefinition.Keybind("Screen_Options_DecreaseLocalScrollSpeed1",
                    GlobalKeybindActions.DecreaseLocalScrollSpeed),
                OptionsRowDefinition.Keybind("Screen_Options_IncreaseLocalScrollSpeed1",
                    GlobalKeybindActions.IncreaseLocalScrollSpeed),
                OptionsRowDefinition.Keybind("Screen_Options_DecreaseLocalScrollSpeedSmall",
                    GlobalKeybindActions.DecreaseLocalScrollSpeedSmall),
                OptionsRowDefinition.Keybind("Screen_Options_IncreaseLocalScrollSpeedSmall",
                    GlobalKeybindActions.IncreaseLocalScrollSpeedSmall),
                OptionsRowDefinition.Keybind("Screen_Options_DecreaseMapOffset5ms", GlobalKeybindActions.DecreaseOffset),
                OptionsRowDefinition.Keybind("Screen_Options_IncreaseMapOffset5ms", GlobalKeybindActions.IncreaseOffset),
                OptionsRowDefinition.Keybind("Screen_Options_DecreaseMapOffset1ms",
                    GlobalKeybindActions.DecreaseOffsetSmall),
                OptionsRowDefinition.Keybind("Screen_Options_IncreaseMapOffset1ms",
                    GlobalKeybindActions.IncreaseOffsetSmall),
                OptionsRowDefinition.Keybind("Screen_Options_DecreaseVisualOffset5ms",
                    GlobalKeybindActions.DecreaseVisualOffset),
                OptionsRowDefinition.Keybind("Screen_Options_IncreaseVisualOffset5ms",
                    GlobalKeybindActions.IncreaseVisualOffset),
                OptionsRowDefinition.Keybind("Screen_Options_DecreaseVisualOffset1ms",
                    GlobalKeybindActions.DecreaseVisualOffsetSmall),
                OptionsRowDefinition.Keybind("Screen_Options_IncreaseVisualOffset1ms",
                    GlobalKeybindActions.IncreaseVisualOffsetSmall),
                OptionsRowDefinition.Keybind("Screen_Options_ResetMapOffset", GlobalKeybindActions.ResetOffset),
                OptionsRowDefinition.Keybind("Screen_Options_ResetVisualOffset",
                    GlobalKeybindActions.ResetVisualOffset)),
            new OptionsRowGroup(OptionsCategoryId.Input, "Screen_Options_Gamemodespecific",
                CreateGameModeRows()),
            new OptionsRowGroup(OptionsCategoryId.Input, "Screen_Options_GameplayUserInterface",
                OptionsRowDefinition.Keybind("Screen_Options_ToggleScoreboardVisibility",
                    GlobalKeybindActions.GameplayToggleScoreboard)),
            new OptionsRowGroup(OptionsCategoryId.Input, "Screen_Options_UserInterface",
                OptionsRowDefinition.Keybind("Screen_Options_OpenOptions", GlobalKeybindActions.OpenOptions),
                OptionsRowDefinition.Keybind("Screen_Options_ToggleFullscreen", GlobalKeybindActions.ToggleFullscreen),
                OptionsRowDefinition.Keybind("Screen_Options_ToggleChatOverlay",
                    GlobalKeybindActions.GameplayToggleOverlay),
                OptionsRowDefinition.Keybind("Screen_Options_ToggleOnlineHub", GlobalKeybindActions.ToggleOnlineHub),
                OptionsRowDefinition.Keybind("Screen_Options_PauseUnpauseMusic", GlobalKeybindActions.TogglePause)),
            new OptionsRowGroup(OptionsCategoryId.Input, "Screen_Options_SongSelection",
                OptionsRowDefinition.Keybind("Screen_Options_DecreaseGameplayRate", GlobalKeybindActions.DecreaseRate),
                OptionsRowDefinition.Keybind("Screen_Options_IncreaseGameplayRate", GlobalKeybindActions.IncreaseRate),
                OptionsRowDefinition.Keybind("Screen_Options_DecreaseGameplayRateHalfStep",
                    GlobalKeybindActions.DecreaseRateSmall),
                OptionsRowDefinition.Keybind("Screen_Options_IncreaseGameplayRateHalfStep",
                    GlobalKeybindActions.IncreaseRateSmall),
                OptionsRowDefinition.Keybind("Screen_Options_ToggleMirrorMod", GlobalKeybindActions.ToggleMirror),
                OptionsRowDefinition.Keybind("Screen_Options_TogglePitch", GlobalKeybindActions.TogglePitch),
                OptionsRowDefinition.Keybind("Screen_Options_RemoveAllMods", GlobalKeybindActions.RemoveMods),
                OptionsRowDefinition.Keybind("Screen_Options_ToggleModifiersPanel",
                    GlobalKeybindActions.SelectionToggleModifiers),
                OptionsRowDefinition.Keybind("Screen_Options_SelectRandomMap",
                    GlobalKeybindActions.SelectionSelectRandomMap),
                OptionsRowDefinition.Keybind("Screen_Options_ToggleMapPreviewPanel",
                    GlobalKeybindActions.SelectionToggleMapPreview),
                OptionsRowDefinition.Keybind("Screen_Options_ToggleUserProfilePanel",
                    GlobalKeybindActions.SelectionToggleUserProfile),
                OptionsRowDefinition.Keybind("Screen_Options_RefreshSongs", GlobalKeybindActions.SelectionRefresh)),
            new OptionsRowGroup(OptionsCategoryId.Input, "Screen_Options_Misc",
                OptionsRowDefinition.Keybind("Screen_Options_TakeScreenshot", GlobalKeybindActions.Screenshot),
                OptionsRowDefinition.Keybind("Screen_Options_CycleFPSLimiter", GlobalKeybindActions.CycleFpsLimiter),
                OptionsRowDefinition.Keybind("Screen_Options_ReloadSkin", GlobalKeybindActions.ReloadSkin))
        };

        /// <summary>
        ///     The rows for every game mode, with a divider between modes.
        /// </summary>
        private static OptionsRowDefinition[] CreateGameModeRows()
        {
            var rows = new List<OptionsRowDefinition>();

            foreach (var mode in GameModeOrder)
            {
                if (rows.Count > 0)
                    rows.Add(OptionsRowDefinition.Divider());

                rows.Add(OptionsRowDefinition.GameModeSettings(mode));
                rows.Add(OptionsRowDefinition.KeyLayout("Screen_Options_ScratchLayout", mode,
                    ConfigManager.ScratchKeyLayouts[mode]));
                rows.Add(OptionsRowDefinition.Toggle("Screen_Options_PlaceScratchLayoutOnLeft",
                    ConfigManager.ScratchLanesLeft[mode], new object[] { ModeHelper.ToShortHand(mode) }));
                rows.Add(OptionsRowDefinition.KeyLayout("Screen_Options_CoopLayout", mode,
                    ConfigManager.CoopKeyLayouts[mode]));
            }

            return rows.ToArray();
        }
    }
}
