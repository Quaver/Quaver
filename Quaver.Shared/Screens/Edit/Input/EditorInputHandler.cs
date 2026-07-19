using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Config;
using Quaver.Shared.Input;
using Quaver.Shared.Screens.Edit.Dialogs;
using Quaver.Shared.Screens.Edit.Plugins;
using Quaver.Shared.Screens.Edit.UI;
using Quaver.Shared.Screens.Edit.UI.Playfield.Waveform;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Platform;

namespace Quaver.Shared.Screens.Edit.Input;

public class EditorInputHandler : IInputHandler<EditorKeybindActions>
{
    private readonly Dictionary<EditorKeybindActions, Bindable<bool>> _invertScrollingActions =
        new()
        {
            [EditorKeybindActions.IncreaseSnap] = ConfigManager.EditorInvertBeatSnapScroll,
            [EditorKeybindActions.DecreaseSnap] = ConfigManager.EditorInvertBeatSnapScroll,
            [EditorKeybindActions.SeekForwards] = ConfigManager.InvertEditorScrolling,
            [EditorKeybindActions.SeekForwardsLarge] = ConfigManager.InvertEditorScrolling,
            [EditorKeybindActions.SeekForwards1ms] = ConfigManager.InvertEditorScrolling,
            [EditorKeybindActions.SeekBackwards] = ConfigManager.InvertEditorScrolling,
            [EditorKeybindActions.SeekBackwardsLarge] = ConfigManager.InvertEditorScrolling,
            [EditorKeybindActions.SeekBackwards1ms] = ConfigManager.InvertEditorScrolling
        };

    private static readonly HashSet<EditorKeybindActions> HoldRepeatActions =
    [
        EditorKeybindActions.ZoomIn,
        EditorKeybindActions.ZoomInLarge,
        EditorKeybindActions.ZoomOut,
        EditorKeybindActions.ZoomOutLarge,
        EditorKeybindActions.SeekForwards,
        EditorKeybindActions.SeekForwardsAndSelect,
        EditorKeybindActions.SeekForwardsAndMove,
        EditorKeybindActions.SeekBackwards,
        EditorKeybindActions.SeekBackwardsAndSelect,
        EditorKeybindActions.SeekBackwardsAndMove,
        EditorKeybindActions.SeekForwardsLarge,
        EditorKeybindActions.SeekBackwardsLarge,
        EditorKeybindActions.SeekForwards1ms,
        EditorKeybindActions.SeekBackwards1ms,
        EditorKeybindActions.IncreasePlaybackRate,
        EditorKeybindActions.DecreasePlaybackRate,
        EditorKeybindActions.ChangeToolUp,
        EditorKeybindActions.ChangeToolDown,
        EditorKeybindActions.Undo,
        EditorKeybindActions.Redo
    ];

    private static readonly HashSet<EditorKeybindActions> HoldAndReleaseActionsSet =
    [
        EditorKeybindActions.PlaceNoteAtLane1,
        EditorKeybindActions.PlaceNoteAtLane2,
        EditorKeybindActions.PlaceNoteAtLane3,
        EditorKeybindActions.PlaceNoteAtLane4,
        EditorKeybindActions.PlaceNoteAtLane5,
        EditorKeybindActions.PlaceNoteAtLane6,
        EditorKeybindActions.PlaceNoteAtLane7,
        EditorKeybindActions.PlaceNoteAtLane8,
        EditorKeybindActions.PlaceNoteAtLane9,
        EditorKeybindActions.PlaceNoteAtLane10
    ];

    /// <inheritdoc />
    public bool? InvertedScrolling(EditorKeybindActions action)
    {
        if (!_invertScrollingActions.TryGetValue(action, out var bindable))
            return null;
        return bindable.Value;
    }

    /// <inheritdoc />
    public void HandleAction(EditorKeybindActions action, bool isKeyPress = true,
        bool isRelease = false)
    {
        switch (action)
        {
            case EditorKeybindActions.ExitEditor:
                Screen.LeaveEditor();
                break;
            case EditorKeybindActions.PlayPause:
                Screen.TogglePlayPause();
                break;
            case EditorKeybindActions.ZoomIn:
                Screen.AdjustZoom(1);
                break;
            case EditorKeybindActions.ZoomInLarge:
                Screen.AdjustZoom(5);
                break;
            case EditorKeybindActions.ZoomOut:
                Screen.AdjustZoom(-1);
                break;
            case EditorKeybindActions.ZoomOutLarge:
                Screen.AdjustZoom(-5);
                break;
            case EditorKeybindActions.SeekForwards:
                Screen.SeekInDirection(Direction.Forward);
                break;
            case EditorKeybindActions.SeekForwardsAndSelect:
                Screen.SeekInDirection(Direction.Forward, enableSelection: true);
                break;
            case EditorKeybindActions.SeekForwardsAndMove:
                Screen.SeekInDirection(Direction.Forward, enableMoving: true);
                break;
            case EditorKeybindActions.SeekBackwards:
                Screen.SeekInDirection(Direction.Backward);
                break;
            case EditorKeybindActions.SeekBackwardsAndSelect:
                Screen.SeekInDirection(Direction.Backward, enableSelection: true);
                break;
            case EditorKeybindActions.SeekBackwardsAndMove:
                Screen.SeekInDirection(Direction.Backward, enableMoving: true);
                break;
            case EditorKeybindActions.SeekForwardsLarge:
                Screen.SeekInDirection(Direction.Forward, 0.25f);
                break;
            case EditorKeybindActions.SeekBackwardsLarge:
                Screen.SeekInDirection(Direction.Backward, 0.25f);
                break;
            case EditorKeybindActions.SeekForwards1ms:
                if (!Screen.Track.IsPlaying)
                    Screen.SeekTo(Screen.Track.Time + 1);
                break;
            case EditorKeybindActions.SeekBackwards1ms:
                if (!Screen.Track.IsPlaying)
                    Screen.SeekTo(Screen.Track.Time - 1);
                break;
            case EditorKeybindActions.SeekForwards1msAndMove:
                if (!Screen.Track.IsPlaying)
                    Screen.SeekTo(Screen.Track.Time + 1, enableMoving: true);
                break;
            case EditorKeybindActions.SeekBackwards1msAndMove:
                if (!Screen.Track.IsPlaying)
                    Screen.SeekTo(Screen.Track.Time - 1, enableMoving: true);
                break;
            case EditorKeybindActions.SeekForwards1msAndSelect:
                if (!Screen.Track.IsPlaying)
                    Screen.SeekTo(Screen.Track.Time + 1, enableSelection: true);
                break;
            case EditorKeybindActions.SeekBackwards1msAndSelect:
                if (!Screen.Track.IsPlaying)
                    Screen.SeekTo(Screen.Track.Time - 1, enableSelection: true);
                break;
            case EditorKeybindActions.SeekToStartOfSelection:
                Screen.SeekToStartOfSelection();
                break;
            case EditorKeybindActions.SeekToEndOfSelection:
                Screen.SeekToEndOfSelection();
                break;
            case EditorKeybindActions.SeekToStartOfSelectionAndSelect:
                Screen.SeekToStartOfSelection(true);
                break;
            case EditorKeybindActions.SeekToEndOfSelectionAndSelect:
                Screen.SeekToEndOfSelection(true);
                break;
            case EditorKeybindActions.SeekToStart:
                Screen.SeekToStart();
                break;
            case EditorKeybindActions.SeekToEnd:
                Screen.SeekToEnd();
                break;
            case EditorKeybindActions.SeekToStartAndSelect:
                Screen.SeekToStart(true);
                break;
            case EditorKeybindActions.SeekToEndAndSelect:
                Screen.SeekToEnd(true);
                break;
            case EditorKeybindActions.IncreasePlaybackRate:
                Screen.ChangeAudioPlaybackRate(Direction.Forward);
                break;
            case EditorKeybindActions.DecreasePlaybackRate:
                Screen.ChangeAudioPlaybackRate(Direction.Backward);
                break;
            case EditorKeybindActions.SetPreviewTime:
                Screen.ActionManager.SetPreviewTime((int)Screen.Track.Time);
                break;
            case EditorKeybindActions.ChangeToolUp:
                Screen.ChangeTool(Direction.Backward);
                break;
            case EditorKeybindActions.ChangeToolDown:
                Screen.ChangeTool(Direction.Forward);
                break;
            case EditorKeybindActions.ChangeToolToSelect:
                Screen.ChangeToolTo(EditorCompositionTool.Select);
                break;
            case EditorKeybindActions.ChangeToolToNote:
                Screen.ChangeToolTo(EditorCompositionTool.Note);
                break;
            case EditorKeybindActions.ChangeToolToLongNote:
                Screen.ChangeToolTo(EditorCompositionTool.LongNote);
                break;
            case EditorKeybindActions.ChangeToolToMine:
                Screen.ChangeToolTo(EditorCompositionTool.Mine);
                break;
            case EditorKeybindActions.IncreaseSnap:
                Screen.ChangeBeatSnap(Direction.Forward);
                break;
            case EditorKeybindActions.DecreaseSnap:
                Screen.ChangeBeatSnap(Direction.Backward);
                break;
            case EditorKeybindActions.ChangeSnapTo1:
                Screen.BeatSnap.Value = 1;
                break;
            case EditorKeybindActions.ChangeSnapTo2:
                Screen.BeatSnap.Value = 2;
                break;
            case EditorKeybindActions.ChangeSnapTo3:
                Screen.BeatSnap.Value = 3;
                break;
            case EditorKeybindActions.ChangeSnapTo4:
                Screen.BeatSnap.Value = 4;
                break;
            case EditorKeybindActions.ChangeSnapTo5:
                Screen.BeatSnap.Value = 5;
                break;
            case EditorKeybindActions.ChangeSnapTo6:
                Screen.BeatSnap.Value = 6;
                break;
            case EditorKeybindActions.ChangeSnapTo7:
                Screen.BeatSnap.Value = 7;
                break;
            case EditorKeybindActions.ChangeSnapTo8:
                Screen.BeatSnap.Value = 8;
                break;
            case EditorKeybindActions.ChangeSnapTo9:
                Screen.BeatSnap.Value = 9;
                break;
            case EditorKeybindActions.ChangeSnapTo10:
                Screen.BeatSnap.Value = 10;
                break;
            case EditorKeybindActions.ChangeSnapTo12:
                Screen.BeatSnap.Value = 12;
                break;
            case EditorKeybindActions.ChangeSnapTo16:
                Screen.BeatSnap.Value = 16;
                break;
            case EditorKeybindActions.OpenCustomSnapDialog:
                Screen.OpenCustomSnapDialog();
                break;
            case EditorKeybindActions.OpenMetadataDialog:
                Screen.OpenMetadataDialog();
                break;
            case EditorKeybindActions.OpenModifiersDialog:
                Screen.OpenModifiersDialog();
                break;
            case EditorKeybindActions.OpenQuaFile:
                try
                {
                    Utils.NativeUtils.OpenNatively(
                        $"{ConfigManager.SongDirectory.Value}/{Screen.Map.Directory}/{Screen.Map.Path}");
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }

                break;
            case EditorKeybindActions.OpenFolder:
                try
                {
                    Utils.NativeUtils.OpenNatively(
                        $"{ConfigManager.SongDirectory.Value}/{Screen.Map.Directory}");
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }

                break;
            case EditorKeybindActions.CreateNewDifficulty:
                Screen.CreateNewDifficulty(false);
                break;
            case EditorKeybindActions.CreateNewDifficultyFromCurrent:
                Screen.CreateNewDifficulty();
                break;
            case EditorKeybindActions.Export:
                Screen.ExportToZip();
                break;
            case EditorKeybindActions.Upload:
                Screen.UploadMapset();
                break;
            case EditorKeybindActions.UploadAndSubmitForRank:
                Screen.UploadMapset();
                Screen.SubmitForRank();
                break;
            case EditorKeybindActions.ToggleBpmPanel:
                Screen.ToggleBuiltinPlugin(EditorBuiltInPlugin.TimingPointEditor);
                break;
            case EditorKeybindActions.ToggleSvPanel:
                Screen.ToggleBuiltinPlugin(EditorBuiltInPlugin.ScrollVelocityEditor);
                break;
            case EditorKeybindActions.ToggleAutoMod:
                View.AutoMod.IsActive.Value = !View.AutoMod.IsActive.Value;
                break;
            case EditorKeybindActions.ToggleGotoPanel:
                Screen.ToggleBuiltinPlugin(EditorBuiltInPlugin.GoToObjects);
                break;
            case EditorKeybindActions.ToggleGameplayPreview:
                Screen.DisplayGameplayPreview.Value = !Screen.DisplayGameplayPreview.Value;
                break;
            case EditorKeybindActions.ToggleHitsounds:
                Screen.EnableHitsounds.Value = !Screen.EnableHitsounds.Value;
                break;
            case EditorKeybindActions.ToggleMetronome:
                Screen.EnableMetronome.Value = !Screen.EnableMetronome.Value;
                break;
            case EditorKeybindActions.TogglePitchRate:
                ConfigManager.Pitched.Value = !ConfigManager.Pitched.Value;
                break;
            case EditorKeybindActions.ToggleWaveform:
                Screen.ShowWaveform.Value = !Screen.ShowWaveform.Value;
                break;
            case EditorKeybindActions.ToggleWaveformLowPass:
                Screen.WaveformFilter.Value = Screen.WaveformFilter.Value ==
                                              EditorPlayfieldWaveformFilter.LowPass
                    ? EditorPlayfieldWaveformFilter.None
                    : EditorPlayfieldWaveformFilter.LowPass;
                break;
            case EditorKeybindActions.ToggleWaveformHighPass:
                Screen.WaveformFilter.Value = Screen.WaveformFilter.Value ==
                                              EditorPlayfieldWaveformFilter.HighPass
                    ? EditorPlayfieldWaveformFilter.None
                    : EditorPlayfieldWaveformFilter.HighPass;
                break;
            case EditorKeybindActions.ToggleReferenceDifficulty:
                if (Screen.UneditableMap.Value != null)
                    Screen.UneditableMap.Value = null;
                else
                    Screen.ShowReferenceDifficulty();
                break;
            case EditorKeybindActions.NextReferenceDifficulty:
                Screen.ReferenceDifficultyIndex.Value++;
                break;
            case EditorKeybindActions.PreviousReferenceDifficulty:
                Screen.ReferenceDifficultyIndex.Value--;
                break;
            case EditorKeybindActions.PlayTest:
                Screen.ExitToTestPlay();
                break;
            case EditorKeybindActions.PlayTestFromBeginning:
                Screen.ExitToTestPlay(true);
                break;
            case EditorKeybindActions.ToggleLayerViewMode:
                Screen.ToggleViewLayers();
                break;
            case EditorKeybindActions.ToggleTimingGroupViewMode:
                Screen.ToggleViewTimingGroups();
                break;
            case EditorKeybindActions.ChangeSelectedLayerUp:
                Screen.ChangeSelectedLayer(Direction.Backward);
                break;
            case EditorKeybindActions.ChangeSelectedLayerDown:
                Screen.ChangeSelectedLayer(Direction.Forward);
                break;
            case EditorKeybindActions.ToggleCurrentLayerVisibility:
                Screen.ToggleSelectedLayerVisibility();
                break;
            case EditorKeybindActions.ToggleAllLayersVisibility:
                Screen.ToggleAllLayerVisibility();
                break;
            case EditorKeybindActions.MoveSelectedNotesToCurrentLayer:
                Screen.MoveSelectedNotesToCurrentLayer();
                break;
            case EditorKeybindActions.CreateNewLayer:
                Screen.AddNewLayer();
                break;
            case EditorKeybindActions.DeleteCurrentLayer:
                Screen.DeleteLayer();
                break;
            case EditorKeybindActions.RenameCurrentLayer:
                Screen.RenameLayer();
                break;
            case EditorKeybindActions.RecolorCurrentLayer:
                Screen.RecolorLayer();
                break;
            case EditorKeybindActions.MoveSelectedNotesToCurrentTimingGroup:
                Screen.MoveSelectedNotesToCurrentTimingGroup();
                break;
            case EditorKeybindActions.CreateNewTimingGroup:
                Screen.AddNewTimingGroup();
                break;
            case EditorKeybindActions.DeleteCurrentTimingGroup:
                Screen.DeleteTimingGroup();
                break;
            case EditorKeybindActions.RecolorCurrentTimingGroup:
                Screen.RecolorTimingGroup();
                break;
            case EditorKeybindActions.Undo:
                Screen.ActionManager.Undo();
                break;
            case EditorKeybindActions.Redo:
                Screen.ActionManager.Redo();
                break;
            case EditorKeybindActions.Copy:
                Screen.CopySelectedObjects();
                break;
            case EditorKeybindActions.Paste:
                Screen.PasteCopiedObjects(false);
                break;
            case EditorKeybindActions.PasteResnap:
                Screen.PasteCopiedObjects(true);
                break;
            case EditorKeybindActions.SelectNotesAtCurrentTime:
                Screen.SelectObjectsAtCurrentTime();
                break;
            case EditorKeybindActions.SelectAllNotes:
                Screen.SelectAllObjects();
                break;
            case EditorKeybindActions.SelectAllNotesInLayer:
                Screen.SelectAllObjectsInLayer();
                break;
            case EditorKeybindActions.SelectAllNotesInTimingGroup:
                Screen.SelectAllObjectsInTimingGroup();
                break;
            case EditorKeybindActions.MirrorNotesLeftRight:
                Screen.FlipSelectedObjects();
                break;
            case EditorKeybindActions.Deselect:
                Screen.SelectedHitObjects.Clear();
                break;
            case EditorKeybindActions.Cut:
                Screen.CutSelectedObjects();
                break;
            case EditorKeybindActions.DeleteSelection:
                Screen.DeleteSelectedObjects();
                break;
            case EditorKeybindActions.Save:
                Screen.Save();
                break;
            case EditorKeybindActions.ApplyOffsetToMap:
                DialogManager.Show(new EditorApplyOffsetDialog(Screen));
                break;
            case EditorKeybindActions.ResnapToCurrentBeatSnap:
                Screen.ActionManager.ResnapNotes(new List<int> { Screen.BeatSnap.Value },
                    Screen.SelectedHitObjects.Value);
                break;
            case EditorKeybindActions.AddBookmark:
                DialogManager.Show(new EditorBookmarkDialog(Screen.ActionManager, Screen.Track,
                    null));
                break;
            case EditorKeybindActions.SeekToLastBookmark:
                Screen.SeekToNearestBookmark(Direction.Backward);
                break;
            case EditorKeybindActions.SeekToNextBookmark:
                Screen.SeekToNearestBookmark(Direction.Forward);
                break;
            case EditorKeybindActions.PlaceNoteAtLane1:
            case EditorKeybindActions.PlaceNoteAtLane2:
            case EditorKeybindActions.PlaceNoteAtLane3:
            case EditorKeybindActions.PlaceNoteAtLane4:
            case EditorKeybindActions.PlaceNoteAtLane5:
            case EditorKeybindActions.PlaceNoteAtLane6:
            case EditorKeybindActions.PlaceNoteAtLane7:
            case EditorKeybindActions.PlaceNoteAtLane8:
            case EditorKeybindActions.PlaceNoteAtLane9:
            case EditorKeybindActions.PlaceNoteAtLane10:
                if (!ConfigManager.EditorLiveMapping.Value)
                    break;
                var lane = (int)(action ^ EditorKeybindActions.PlaceNoteAtLane);
                if (lane <= Screen.WorkingMap.GetKeyCount())
                    Screen.HandleHitObjectPlacement(lane, isKeyPress, isRelease);
                break;
            case EditorKeybindActions.PlaceNoteAtLane:
            case EditorKeybindActions.SwapNoteAtLane:
            case EditorKeybindActions.SwapNoteAtLane1:
            case EditorKeybindActions.SwapNoteAtLane2:
            case EditorKeybindActions.SwapNoteAtLane3:
            case EditorKeybindActions.SwapNoteAtLane4:
            case EditorKeybindActions.SwapNoteAtLane5:
            case EditorKeybindActions.SwapNoteAtLane6:
            case EditorKeybindActions.SwapNoteAtLane7:
            case EditorKeybindActions.SwapNoteAtLane8:
            case EditorKeybindActions.SwapNoteAtLane9:
            case EditorKeybindActions.SwapNoteAtLane10:
            default:
                return;
        }
    }

    /// <inheritdoc />
    public void HandleCustomActions(GenericKeyState keyState, GenericKeyState previousKeyState,
        HashSet<Keybind> uniqueKeyPresses)
    {
        HandlePluginKeyPresses();
    }

    private void HandlePluginKeyPresses()
    {
        foreach (var (pluginName, keybinds) in EditorInputConfig.PluginKeybinds)
        {
            if (keybinds.IsUniquePress())
            {
                // Toggle plugin
                foreach (var plugin in Screen.Plugins)
                {
                    if (plugin.Name != pluginName)
                        continue;

                    Screen.TogglePlugin(plugin);
                }
            }
        }
    }

    private void HandleSwapLane(Dictionary<Keybind, HashSet<EditorKeybindActions>> keybindActions,
        HashSet<Keybind> uniqueKeyPresses)
    {
        var heldLane = -1;
        var uniquePressLane = -1;
        var success = false;
        foreach (var (keybind, actionsSet) in keybindActions)
        {
            foreach (var action in actionsSet)
            {
                if (!action.HasFlag(EditorKeybindActions.SwapNoteAtLane))
                    continue;

                var lane = (int)(action ^ EditorKeybindActions.SwapNoteAtLane);
                if (lane > Screen.WorkingMap.GetKeyCount())
                    continue;

                if (uniqueKeyPresses.Contains(keybind))
                {
                    uniquePressLane = lane;
                    // If two actions are uniquely pressed at the same time, both will be registered
                    if (heldLane == -1)
                        heldLane = uniquePressLane;
                }
                else
                {
                    heldLane = lane;
                }

                if (uniquePressLane == -1 || heldLane == -1 || heldLane == uniquePressLane)
                    continue;

                success = true;
                break;
            }

            if (success) break;
        }

        if (!success)
            return;

        Screen.SwapSelectedObjects(heldLane, uniquePressLane);
    }

    /// <inheritdoc />
    public void HandleActionCombination(Dictionary<Keybind, HashSet<EditorKeybindActions>> actions,
        HashSet<Keybind> uniqueKeyPresses)
    {
        HandleSwapLane(actions, uniqueKeyPresses);
    }

    /// <inheritdoc />
    public bool IsHoldRepeat(EditorKeybindActions action) => HoldRepeatActions.Contains(action);

    /// <inheritdoc />
    public bool IsHoldAndRelease(EditorKeybindActions action) =>
        HoldAndReleaseActionsSet.Contains(action);

    /// <inheritdoc />
    public IEnumerable<EditorKeybindActions> HoldAndReleaseActions => HoldAndReleaseActionsSet;

    public EditorInputHandler(EditScreen screen, EditorInputConfig editorInputConfig)
    {
        Screen = screen;
        EditorInputConfig = editorInputConfig;
        View = (EditScreenView)screen.View;
    }

    /// <inheritdoc />
    public bool IsKeybindBlocked(GenericKey key)
    {
        if (View.MapPreview?.IsPlayTesting.Value ?? false)
        {
            return (Screen.WorkingMap.HasScratchKey
                ? ConfigManager.ScratchKeyLayouts
                : ConfigManager.KeyLayouts)[Screen.WorkingMap.Mode].Any(x => x.Value.Equals(key));
        }

        return false;
    }

    /// <inheritdoc />
    public bool InFocus => DialogManager.Dialogs.Count != 0 || View.IsImGuiHovered;

    private EditScreenView View { get; set; }

    private EditScreen Screen { get; set; }

    private EditorInputConfig EditorInputConfig { get; set; }

    public void Destroy()
    {
        Screen = null;
        View = null;
    }
}