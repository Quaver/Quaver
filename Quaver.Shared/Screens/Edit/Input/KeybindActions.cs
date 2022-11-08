﻿using System;

namespace Quaver.Shared.Screens.Edit.Input
{
    [Serializable]
    public enum KeybindActions
    {
        ExitEditor,
        PlayPause,
        ZoomIn,
        ZoomInLarge,
        ZoomOut,
        ZoomOutLarge,
        SeekForwards,
        SeekBackwards,
        SeekForwards1ms,
        SeekBackwards1ms,
        SeekToStartOfSelection,
        SeekToEndOfSelection,
        SeekToBeginning,
        SeekToEnd,
        IncreasePlaybackRate,
        DecreasePlaybackRate,
        ChangeToolUp,
        ChangeToolDown,
        ChangeToolToSelect,
        ChangeToolToNote,
        ChangeToolToLongNote,
        IncreaseSnap,
        IncreaseSnapLarge,
        DecreaseSnap,
        DecreaseSnapLarge,
        ChangeSnapTo1,
        ChangeSnapTo2,
        ChangeSnapTo3,
        ChangeSnapTo4,
        ChangeSnapTo5,
        ChangeSnapTo6,
        ChangeSnapTo7,
        ChangeSnapTo8,
        ChangeSnapTo9,
        ChangeSnapTo10,
        ChangeSnapTo12,
        ChangeSnapTo16,
        OpenCustomSnapDialog,
        ToggleLayerColorMode,
        ChangeSelectedLayerUp,
        ChangeSelectedLayerDown,
        ToggleCurrentLayerVisibility,
        ToggleAllLayersVisibility,
        MoveSelectedNotesToCurrentLayer,
        MoveSelectedNotesToLayerUp,
        MoveSelectedNotesToLayerDown,
        CreateNewLayer,
        DeleteCurrentLayer,
        RenameCurrentLayer,
        RecolorCurrentLayer,
        CreateNewMapset,
        CreateNewDifficulty,
        CreateNewDifficultyFromCurrent,
        RefreshFileCache,
        OpenMapFile,
        OpenMapDirectory,
        Export,
        UndoAction,
        RedoAction,
        SaveMap,
        UploadMapset,
        SetPreviewPoint,
        AdjustOffset,
        SubmitForRanked,
        ToggleAutomod,
        ToggleGameplayPreview,
        ToggleReferenceDifficulty,
        ChangeReferenceDifficultyPrevious,
        ChangeReferenceDifficultyNext,
        ToggleWaveform,
        ToggleWaveformLowPassFilter,
        ToggleWaveformHighPassFilter,
        ToggleHitsounds,
        ToggleMetronome,
        TogglePitchWithRate,
        ToggleLivePlaytest,
        OpenGameplayTestModifiers,
        StartGameplayTest,
        StartGameplayTestFromBeginning,
        CutNotes,
        CopyNotes,
        PasteNotes,
        PasteNoResnap,
        DeleteCurrentNotesOrSelection,
        DeleteCurrentNotesOrSelectionAndMove,
        SelectAll,
        SelectAllInLayer,
        DeselectAll,
        MirrorNotesLeftRight,
        MirrorNotesUpDown,
        ResnapAllNotes,
        ResnapModifiedOrSelectedNotes,
        PlaceNoteLane1,
        PlaceNoteLane2,
        PlaceNoteLane3,
        PlaceNoteLane4,
        PlaceNoteLane5,
        PlaceNoteLane6,
        PlaceNoteLane7,
        PlaceTimingPoint,
        PlaceScrollVelocity,
        ToggleMetadataPanel,
        ToggleTimingPointPanel,
        ToggleScrollVelocityPanel,
        ToggleGoToObjectsPanel,
        CloseAllPlugins,
        ReloadPlugins,
    }
}