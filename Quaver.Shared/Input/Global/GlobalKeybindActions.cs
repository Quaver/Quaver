using System;

namespace Quaver.Shared.Input.Global;

[Flags]
public enum GlobalKeybindActions : ulong
{
#pragma warning disable format // @formatter:off
    Screenshot                       = 0  | LayerGlobal,
    OpenOptions                      = 1  | LayerExceptGameplay,
    ToggleFullscreen                 = 2  | LayerExceptGameplay,
    TogglePause                      = 3  | LayerExceptGameplay,
    CycleFpsLimiter                  = 4  | LayerExceptGameplay,
    ToggleOnlineHub                  = 5  | LayerExceptGameplay,
    ReloadSkin                       = 6  | LayerExceptGameplay,
    Back                             = 7  | LayerExceptGameplay,
    IncreaseRate                     = 8  | LayerExceptGameplay,
    TogglePitch                      = 9  | LayerExceptGameplay,
    RemoveMods                       = 10 | LayerExceptGameplay,
    ToggleMirror                     = 11 | LayerExceptGameplay,
    IncreaseScrollSpeed              = 12 | LayerGameplay,
    IncreaseOffset                   = 13 | LayerGameplay,
    GameplayPause                    = 14 | LayerGameplay,
    GameplayToggleScoreboard         = 15 | LayerGameplayNormal,
    GameplayToggleOverlay            = 16 | LayerGameplay,
    GameplayRetry                    = 17 | LayerGameplay,
    GameplayQuickExit                = 18 | LayerGameplay,
    GameplaySkipIntro                = 19 | LayerGameplay,
    GameplayTogglePlaytestAutoplay   = 20 | LayerGameplayReplay,
    ResetOffset                      = 21 | LayerGlobal,
    NavigateRight                    = 22 | LayerGlobal,
    NavigateUp                       = 23 | LayerGlobal,
    NavigateSelect                   = 24 | LayerGlobal,
    ResultsTab                       = 25 | LayerExceptGameplay,
    SelectionToggleModifiers         = 26 | LayerSelection,
    SelectionSelectRandomMap         = 27 | LayerSelection,
    SelectionToggleMapPreview        = 28 | LayerSelection,
    SelectionToggleUserProfile       = 29 | LayerSelection,
    SelectionRefresh                 = 30 | LayerSelection,
#pragma warning enable format // @formatter:on

    BaseActionMask = (1 << 16) - 1, // bit 15-0
    LayerMask = ((1ul << 16) - 1) << 16, // bit 31-16
    ModifierMask = ((1ul << 16) - 1) << 48, // bit 63-48

    // Conflict Detection
    LayerGlobal = LayerMask,
    LayerGameplayNormal = 1 << 16,
    LayerGameplayReplay = 1 << 17,
    LayerGameplay = LayerGameplayNormal | LayerGameplayReplay,
    LayerSelection = 1 << 18,
    LayerExceptGameplay = LayerGlobal & ~LayerGameplay,

    // Modifiers
    Reverse = 1ul << 60,
    Visual = 1ul << 61,
    Local = 1ul << 62,
    Small = 1ul << 63,

    DecreaseRate = IncreaseRate | Reverse,
    IncreaseRateSmall = IncreaseRate | Small,
    DecreaseRateSmall = IncreaseRateSmall | Reverse,
    DecreaseScrollSpeed = IncreaseScrollSpeed | Reverse,
    IncreaseLocalScrollSpeed = IncreaseScrollSpeed | Local,
    DecreaseLocalScrollSpeed = DecreaseScrollSpeed | Local,
    IncreaseScrollSpeedSmall = IncreaseScrollSpeed | Small,
    DecreaseScrollSpeedSmall = IncreaseScrollSpeedSmall | Reverse,
    IncreaseLocalScrollSpeedSmall = IncreaseScrollSpeedSmall | Local,
    DecreaseLocalScrollSpeedSmall = IncreaseLocalScrollSpeedSmall | Reverse,
    DecreaseOffset = IncreaseOffset | Reverse,
    IncreaseOffsetSmall = IncreaseOffset | Small,
    DecreaseOffsetSmall = DecreaseOffset | Small,
    IncreaseVisualOffset = IncreaseOffset | Visual,
    DecreaseVisualOffset = IncreaseVisualOffset | Reverse,
    IncreaseVisualOffsetSmall = IncreaseVisualOffset | Small,
    DecreaseVisualOffsetSmall = IncreaseVisualOffsetSmall | Reverse,
    ResetVisualOffset = ResetOffset | Visual,
    NavigateDown = NavigateUp | Reverse,
    NavigateLeft = NavigateRight | Reverse,
    SelectionSelectPreviousRandomMap = SelectionSelectRandomMap | Reverse,
}

public static class GlobalKeybindActionsExtensions
{
    public static GlobalKeybindActions BaseWithLayer(this GlobalKeybindActions value) =>
        value & (GlobalKeybindActions.BaseActionMask | GlobalKeybindActions.LayerMask);

    public static GlobalKeybindActions Layer(this GlobalKeybindActions value) =>
        value & GlobalKeybindActions.LayerMask;
}