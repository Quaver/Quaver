using Quaver.Shared.Config;

namespace Quaver.Shared.Screens.V2.Options.Catalog.Categories
{
    internal static class GameplayOptions
    {
        internal static OptionsRowGroup[] Create() => new[]
        {
            new OptionsRowGroup(OptionsCategoryId.Gameplay, "Screen_Options_Background",
                OptionsRowDefinition.Slider("Screen_Options_BackgroundBrightness", ConfigManager.BackgroundBrightness,
                    value => $"{value}%")),
            new OptionsRowGroup(OptionsCategoryId.Gameplay, "Screen_Options_Visuals",
                OptionsRowDefinition.Slider("Screen_Options_LongNoteShrinkAmount", ConfigManager.PercyAmount,
                    value => value.ToString())),
            new OptionsRowGroup(OptionsCategoryId.Gameplay, "Screen_Options_Sound",
                OptionsRowDefinition.Toggle("Screen_Options_EnableHitsounds", ConfigManager.EnableHitsounds),
                OptionsRowDefinition.Toggle("Screen_Options_EnableLongNoteReleaseHitsounds",
                    ConfigManager.EnableLongNoteReleaseHitsounds),
                OptionsRowDefinition.Toggle("Screen_Options_EnableKeysounds", ConfigManager.EnableKeysounds)),
            new OptionsRowGroup(OptionsCategoryId.Gameplay, "Screen_Options_Input",
                OptionsRowDefinition.Toggle("Screen_Options_EnableTapToPause", ConfigManager.TapToPause),
                OptionsRowDefinition.Toggle("Screen_Options_EnableTapToRestart", ConfigManager.TapToRestart),
                OptionsRowDefinition.Toggle("Screen_Options_SkipResultsScreenAfterQuitting",
                    ConfigManager.SkipResultsScreenAfterQuit),
                OptionsRowDefinition.Toggle("Screen_Options_LockWindowsKeyduringgameplay",
                    ConfigManager.LockWinkeyDuringGameplay)),
            new OptionsRowGroup(OptionsCategoryId.Gameplay, "Screen_Options_UserInterface",
                OptionsRowDefinition.Toggle("Screen_Options_DisplayTimingLines", ConfigManager.DisplayTimingLines),
                OptionsRowDefinition.Toggle("Screen_Options_DisplayHitBubbles", ConfigManager.DisplayHitBubbles),
                OptionsRowDefinition.Toggle("Screen_Options_DisplayJudgementCounter",
                    ConfigManager.DisplayJudgementCounter)),
            new OptionsRowGroup(OptionsCategoryId.Gameplay, "Screen_Options_Scoreboard",
                OptionsRowDefinition.Toggle("Screen_Options_DisplayScoreboard", ConfigManager.ScoreboardVisible)),
            new OptionsRowGroup(OptionsCategoryId.Gameplay, "Screen_Options_ProgressBar",
                OptionsRowDefinition.Toggle("Screen_Options_DisplaySongTimeProgressBar",
                    ConfigManager.DisplaySongTimeProgress),
                OptionsRowDefinition.Toggle("Screen_Options_DisplaySongTimeProgressBarTimeNumbers",
                    ConfigManager.DisplaySongTimeProgressNumbers)),
            new OptionsRowGroup(OptionsCategoryId.Gameplay, "Screen_Options_LaneCover",
                OptionsRowDefinition.Toggle("Screen_Options_EnableTopLaneCover", ConfigManager.LaneCoverTop),
                OptionsRowDefinition.Slider("Screen_Options_TopLaneCoverHeight", ConfigManager.LaneCoverTopHeight,
                    value => $"{value}%"),
                OptionsRowDefinition.Toggle("Screen_Options_EnableBottomLaneCover", ConfigManager.LaneCoverBottom),
                OptionsRowDefinition.Slider("Screen_Options_BottomLaneCoverHeight",
                    ConfigManager.LaneCoverBottomHeight, value => $"{value}%"),
                OptionsRowDefinition.Toggle("Screen_Options_DisplayUIElementsOverLaneCovers",
                    ConfigManager.UIElementsOverLaneCover),
                OptionsRowDefinition.Toggle("Screen_Options_DisplayReceptorsOverLaneCovers",
                    ConfigManager.ReceptorsOverLaneCover))
        };
    }
}
