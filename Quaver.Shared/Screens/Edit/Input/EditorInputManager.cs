using System.Collections.Generic;
using Quaver.Shared.Screens.Edit.Dialogs;
using Quaver.Shared.Screens.Edit.Plugins;
using Quaver.Shared.Screens.Edit.UI;
using Quaver.Shared.Screens.Edit.UI.Playfield.Waveform;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Edit.Input
{
    public class EditorInputManager
    {
        public EditorInputConfig InputConfig { get; }
        public EditScreen Screen { get; }

        private const int HoldRepeatActionDelay = 250;
        private const int HoldRepeatActionInterval = 25;
        private Dictionary<KeybindActions, long> LastActionPress = new Dictionary<KeybindActions, long>();
        private Dictionary<KeybindActions, long> LastActionTime = new Dictionary<KeybindActions, long>();

        private static HashSet<KeybindActions> HoldActions = new HashSet<KeybindActions>()
        {
            KeybindActions.ZoomIn,
            KeybindActions.ZoomInLarge,
            KeybindActions.ZoomOut,
            KeybindActions.ZoomOutLarge,
            KeybindActions.SeekForwards,
            KeybindActions.SeekForwardsWithSelection,
            KeybindActions.SeekForwardsWithMove,
            KeybindActions.SeekBackwards,
            KeybindActions.SeekBackwardsWithSelection,
            KeybindActions.SeekBackwardsWithMove,
            KeybindActions.SeekForwards1ms,
            KeybindActions.SeekForwards1msWithSelection,
            KeybindActions.SeekForwards1msWithMove,
            KeybindActions.SeekBackwards1ms,
            KeybindActions.SeekBackwards1msWithSelection,
            KeybindActions.SeekBackwards1msWithMove,
            KeybindActions.SeekToStartOfSelection,
            KeybindActions.SeekToStartOfSelectionWithMove,
            KeybindActions.SeekToEndOfSelection,
            KeybindActions.SeekToEndOfSelectionWithMove,
            KeybindActions.SeekToBeginning,
            KeybindActions.SeekToBeginningWithSelection,
            KeybindActions.SeekToBeginningWithMove,
            KeybindActions.SeekToEnd,
            KeybindActions.SeekToEndWithSelection,
            KeybindActions.SeekToEndWithMove,
            KeybindActions.IncreasePlaybackRate,
            KeybindActions.DecreasePlaybackRate,
            KeybindActions.ChangeToolUp,
            KeybindActions.ChangeToolDown,
            KeybindActions.IncreaseSnap,
            KeybindActions.IncreaseSnapLarge,
            KeybindActions.DecreaseSnap,
            KeybindActions.DecreaseSnapLarge,
            KeybindActions.OpenCustomSnapDialog,
            KeybindActions.ToggleLayerColorMode,
            KeybindActions.ChangeSelectedLayerUp,
            KeybindActions.ChangeSelectedLayerDown,
            KeybindActions.UndoAction,
            KeybindActions.RedoAction,
            KeybindActions.ChangeReferenceDifficultyPrevious,
            KeybindActions.ChangeReferenceDifficultyNext
        };

        public EditorInputManager(EditScreen screen)
        {
            InputConfig = EditorInputConfig.LoadFromConfig();
            Screen = screen;
        }

        public void HandleInput()
        {
            HandleKeypresses();
            HandleMouseInputs();
            HandlePluginKeypresses();
        }

        private void HandleKeypresses()
        {
            foreach (var (action, keybinds) in InputConfig.Keybinds)
            {
                if (keybinds.IsUniqueKeypress())
                {
                    HandleAction(action);
                    LastActionPress[action] = GameBase.Game.TimeRunning;
                }
                else if (keybinds.IsDown() && CanRepeat(action))
                {
                    HandleAction(action);
                }
            }
        }

        private bool CanRepeat(KeybindActions action)
        {
            if (!HoldActions.Contains(action))
                return false;

            var currentTime = GameBase.Game.TimeRunning;
            var lastPress = LastActionPress.GetValueOrDefault(action, currentTime);
            var lastAction = LastActionTime.GetValueOrDefault(action, currentTime);
            var timeSinceLastPress = currentTime - lastPress;
            var timeSinceLastAction = currentTime - lastAction;

            return timeSinceLastPress > HoldRepeatActionDelay && timeSinceLastAction > HoldRepeatActionInterval;
        }

        private void HandleAction(KeybindActions action)
        {
            switch (action)
            {
                case KeybindActions.ExitEditor:
                    Screen.ExitToSongSelect();
                    break;
                case KeybindActions.PlayPause:
                    Screen.TogglePlayPause();
                    break;
                case KeybindActions.ZoomIn:
                    Screen.AdjustZoom(1);
                    break;
                case KeybindActions.ZoomInLarge:
                    Screen.AdjustZoom(5);
                    break;
                case KeybindActions.ZoomOut:
                    Screen.AdjustZoom(-1);
                    break;
                case KeybindActions.ZoomOutLarge:
                    Screen.AdjustZoom(-5);
                    break;
                case KeybindActions.SeekForwards:
                    Screen.SeekInDirection(Direction.Forward);
                    break;
                case KeybindActions.SeekForwardsWithSelection:
                    // TODO
                    break;
                case KeybindActions.SeekForwardsWithMove:
                    // TODO
                    break;
                case KeybindActions.SeekBackwards:
                    Screen.SeekInDirection(Direction.Backward);
                    break;
                case KeybindActions.SeekBackwardsWithSelection:
                    // TODO
                    break;
                case KeybindActions.SeekBackwardsWithMove:
                    // TODO
                    break;
                case KeybindActions.SeekForwards1ms:
                    // TODO
                    break;
                case KeybindActions.SeekForwards1msWithSelection:
                    // TODO
                    break;
                case KeybindActions.SeekForwards1msWithMove:
                    // TODO
                    break;
                case KeybindActions.SeekBackwards1ms:
                    // TODO
                    break;
                case KeybindActions.SeekBackwards1msWithSelection:
                    // TODO
                    break;
                case KeybindActions.SeekBackwards1msWithMove:
                    // TODO
                    break;
                case KeybindActions.SeekToStartOfSelection:
                    // TODO
                    break;
                case KeybindActions.SeekToStartOfSelectionWithMove:
                    // TODO
                    break;
                case KeybindActions.SeekToEndOfSelection:
                    // TODO
                    break;
                case KeybindActions.SeekToEndOfSelectionWithMove:
                    // TODO
                    break;
                case KeybindActions.SeekToBeginning:
                    // TODO
                    break;
                case KeybindActions.SeekToBeginningWithSelection:
                    // TODO
                    break;
                case KeybindActions.SeekToBeginningWithMove:
                    // TODO
                    break;
                case KeybindActions.SeekToEnd:
                    // TODO
                    break;
                case KeybindActions.SeekToEndWithSelection:
                    // TODO
                    break;
                case KeybindActions.SeekToEndWithMove:
                    // TODO
                    break;
                case KeybindActions.IncreasePlaybackRate:
                    Screen.ChangeAudioPlaybackRate(Direction.Forward);
                    break;
                case KeybindActions.DecreasePlaybackRate:
                    Screen.ChangeAudioPlaybackRate(Direction.Backward);
                    break;
                case KeybindActions.ChangeToolUp:
                    Screen.ChangeTool(Direction.Backward);
                    break;
                case KeybindActions.ChangeToolDown:
                    Screen.ChangeTool(Direction.Forward);
                    break;
                case KeybindActions.ChangeToolToSelect:
                    Screen.ChangeToolTo(EditorCompositionTool.Select);
                    break;
                case KeybindActions.ChangeToolToNote:
                    Screen.ChangeToolTo(EditorCompositionTool.Note);
                    break;
                case KeybindActions.ChangeToolToLongNote:
                    Screen.ChangeToolTo(EditorCompositionTool.LongNote);
                    break;
                case KeybindActions.IncreaseSnap:
                    Screen.ChangeBeatSnap(Direction.Forward);
                    break;
                case KeybindActions.IncreaseSnapLarge:
                    Screen.ChangeBeatSnap(Direction.Forward, true);
                    break;
                case KeybindActions.DecreaseSnap:
                    Screen.ChangeBeatSnap(Direction.Backward);
                    break;
                case KeybindActions.DecreaseSnapLarge:
                    Screen.ChangeBeatSnap(Direction.Backward, true);
                    break;
                case KeybindActions.ChangeSnapTo1:
                    Screen.BeatSnap.Value = 1;
                    break;
                case KeybindActions.ChangeSnapTo2:
                    Screen.BeatSnap.Value = 2;
                    break;
                case KeybindActions.ChangeSnapTo3:
                    Screen.BeatSnap.Value = 3;
                    break;
                case KeybindActions.ChangeSnapTo4:
                    Screen.BeatSnap.Value = 4;
                    break;
                case KeybindActions.ChangeSnapTo5:
                    Screen.BeatSnap.Value = 5;
                    break;
                case KeybindActions.ChangeSnapTo6:
                    Screen.BeatSnap.Value = 6;
                    break;
                case KeybindActions.ChangeSnapTo7:
                    Screen.BeatSnap.Value = 7;
                    break;
                case KeybindActions.ChangeSnapTo8:
                    Screen.BeatSnap.Value = 8;
                    break;
                case KeybindActions.ChangeSnapTo9:
                    Screen.BeatSnap.Value = 9;
                    break;
                case KeybindActions.ChangeSnapTo10:
                    Screen.BeatSnap.Value = 10;
                    break;
                case KeybindActions.ChangeSnapTo12:
                    Screen.BeatSnap.Value = 12;
                    break;
                case KeybindActions.ChangeSnapTo16:
                    Screen.BeatSnap.Value = 16;
                    break;
                case KeybindActions.OpenCustomSnapDialog:
                    Screen.OpenCustomSnapDialog();
                    break;
                case KeybindActions.ToggleLayerColorMode:
                    Screen.ViewLayers.Value = !Screen.ViewLayers.Value;
                    break;
                case KeybindActions.ChangeSelectedLayerUp:
                    Screen.ChangeSelectedLayer(Direction.Backward);
                    break;
                case KeybindActions.ChangeSelectedLayerDown:
                    Screen.ChangeSelectedLayer(Direction.Forward);
                    break;
                case KeybindActions.ToggleCurrentLayerVisibility:
                    Screen.ToggleSelectedLayerVisibility();
                    break;
                case KeybindActions.ToggleAllLayersVisibility:
                    // TODO
                    break;
                case KeybindActions.MoveSelectedNotesToCurrentLayer:
                    // TODO
                    break;
                case KeybindActions.MoveSelectedNotesToLayerUp:
                    // TODO
                    break;
                case KeybindActions.MoveSelectedNotesToLayerDown:
                    // TODO
                    break;
                case KeybindActions.CreateNewLayer:
                    Screen.AddNewLayer();
                    break;
                case KeybindActions.DeleteCurrentLayer:
                    Screen.DeleteLayer();
                    break;
                case KeybindActions.RenameCurrentLayer:
                    Screen.RenameLayer();
                    break;
                case KeybindActions.RecolorCurrentLayer:
                    Screen.RecolorLayer();
                    break;
                case KeybindActions.CreateNewMapset:
                    DialogManager.Show(new EditorNewSongDialog());
                    break;
                case KeybindActions.CreateNewDifficulty:
                    Screen.CreateNewDifficulty(false);
                    break;
                case KeybindActions.CreateNewDifficultyFromCurrent:
                    Screen.CreateNewDifficulty();
                    break;
                case KeybindActions.RefreshFileCache:
                    Screen.RefreshFileCache();
                    break;
                case KeybindActions.OpenMapFile:
                    Screen.Map.OpenFile();
                    break;
                case KeybindActions.OpenMapDirectory:
                    Screen.Map.OpenFolder();
                    break;
                case KeybindActions.Export:
                    Screen.ExportToZip();
                    break;
                case KeybindActions.UndoAction:
                    Screen.ActionManager.Undo();
                    break;
                case KeybindActions.RedoAction:
                    Screen.ActionManager.Redo();
                    break;
                case KeybindActions.SaveMap:
                    Screen.Save();
                    break;
                case KeybindActions.UploadMapset:
                    Screen.UploadMapset();
                    break;
                case KeybindActions.SetPreviewPoint:
                    Screen.SetPreviewPoint();
                    break;
                case KeybindActions.SubmitForRanked:
                    Screen.SubmitForRank();
                    break;
                case KeybindActions.ToggleAutomod:
                    Screen.ToggleAutomod();
                    break;
                case KeybindActions.ToggleGameplayPreview:
                    Screen.ToggleGameplayPreview();
                    break;
                case KeybindActions.ToggleReferenceDifficulty:
                    Screen.ToggleReferenceDifficulty();
                    break;
                case KeybindActions.ChangeReferenceDifficultyPrevious:
                    Screen.ChangeReferenceDifficultyInDirection(Direction.Backward);
                    break;
                case KeybindActions.ChangeReferenceDifficultyNext:
                    Screen.ChangeReferenceDifficultyInDirection(Direction.Forward);
                    break;
                case KeybindActions.ToggleWaveform:
                    Screen.ToggleWaveform();
                    break;
                case KeybindActions.ToggleWaveformLowPassFilter:
                    Screen.ToggleWaveformFilter(EditorPlayfieldWaveformFilter.HighPass);
                    break;
                case KeybindActions.ToggleWaveformHighPassFilter:
                    Screen.ToggleWaveformFilter(EditorPlayfieldWaveformFilter.HighPass);
                    break;
                case KeybindActions.ToggleHitsounds:
                    Screen.ToggleHitsounds();
                    break;
                case KeybindActions.ToggleMetronome:
                    Screen.ToggleMetronome();
                    break;
                case KeybindActions.TogglePitchWithRate:
                    Screen.TogglePitchWithRate();
                    break;
                case KeybindActions.ToggleLivePlaytest:
                    Screen.ToggleLivePlaytest();
                    break;
                case KeybindActions.OpenGameplayTestModifiers:
                    Screen.OpenGameplayModifiers();
                    break;
                case KeybindActions.StartGameplayTest:
                    Screen.ExitToTestPlay();
                    break;
                case KeybindActions.StartGameplayTestFromBeginning:
                    Screen.ExitToTestPlay(true);
                    break;
                case KeybindActions.CutNotes:
                    Screen.CutSelectedObjects();
                    break;
                case KeybindActions.CopyNotes:
                    Screen.CopySelectedObjects();
                    break;
                case KeybindActions.PasteNotes:
                    Screen.PasteCopiedObjects(false);
                    break;
                case KeybindActions.PasteNoResnap:
                    Screen.PasteCopiedObjects(true);
                    break;
                case KeybindActions.DeleteCurrentNotesOrSelection:
                    Screen.DeleteSelectedObjects();
                    break;
                case KeybindActions.DeleteCurrentNotesOrSelectionAndMove:
                    // TODO
                    break;
                case KeybindActions.SelectAll:
                    Screen.SelectAllObjects();
                    break;
                case KeybindActions.SelectAllInLayer:
                    Screen.SelectAllObjectsInLayer();
                    break;
                case KeybindActions.SelectToEnd:
                    // TODO
                    break;
                case KeybindActions.SelectToBeginning:
                    // TODO
                    break;
                case KeybindActions.MirrorNotesLeftRight:
                    Screen.FlipSelectedObjects();
                    break;
                case KeybindActions.MirrorNotesUpDown:
                    // TODO
                    break;
                case KeybindActions.ResnapAllNotes:
                    // TODO
                    break;
                case KeybindActions.ResnapModifiedOrSelectedNotes:
                    // TODO
                    break;
                case KeybindActions.PlaceNoteLane1:
                    Screen.PlaceHitObject(1);
                    break;
                case KeybindActions.PlaceNoteLane2:
                    Screen.PlaceHitObject(2);
                    break;
                case KeybindActions.PlaceNoteLane3:
                    Screen.PlaceHitObject(3);
                    break;
                case KeybindActions.PlaceNoteLane4:
                    Screen.PlaceHitObject(4);
                    break;
                case KeybindActions.PlaceNoteLane5:
                    Screen.PlaceHitObject(5);
                    break;
                case KeybindActions.PlaceNoteLane6:
                    Screen.PlaceHitObject(6);
                    break;
                case KeybindActions.PlaceNoteLane7:
                    Screen.PlaceHitObject(7);
                    break;
                case KeybindActions.PlaceTimingPoint:
                    Screen.PlaceTimingPoint();
                    break;
                case KeybindActions.PlaceScrollVelocity:
                    Screen.PlaceScrollVelocity();
                    break;
                case KeybindActions.ToggleMetadataPanel:
                    Screen.ShowMetadata();
                    break;
                case KeybindActions.ToggleTimingPointPanel:
                    Screen.ToggleBuiltInPlugin(EditorBuiltInPlugin.TimingPointEditor);
                    break;
                case KeybindActions.ToggleScrollVelocityPanel:
                    Screen.ToggleBuiltInPlugin(EditorBuiltInPlugin.ScrollVelocityEditor);
                    break;
                case KeybindActions.ToggleGoToObjectsPanel:
                    Screen.ToggleBuiltInPlugin(EditorBuiltInPlugin.GoToObjects);
                    break;
                case KeybindActions.CloseAllPlugins:
                    Screen.CloseAllPlugins();
                    break;
                case KeybindActions.ReloadPlugins:
                    Screen.LoadPlugins();
                    break;
                default:
                    return;
            }

            LastActionTime[action] = GameBase.Game.TimeRunning;
        }

        private void HandleMouseInputs()
        {
            // TODO Add mouse buttons/movements to generic key to customize

            var scrolledForward = MouseManager.CurrentState.ScrollWheelValue > MouseManager.PreviousState.ScrollWheelValue;
            var scrolledBackward = MouseManager.CurrentState.ScrollWheelValue < MouseManager.PreviousState.ScrollWheelValue;

            if (KeyboardManager.IsCtrlDown())
            {
                if (scrolledForward) Screen.ChangeBeatSnap(Direction.Forward);
                if (scrolledBackward) Screen.ChangeBeatSnap(Direction.Backward);
            }
            else
            {
                if (scrolledForward) Screen.SeekInDirection(Direction.Forward);
                if (scrolledBackward) Screen.SeekInDirection(Direction.Backward);
            }
        }

        private void HandlePluginKeypresses()
        {
            foreach (var (pluginName, keybinds) in InputConfig.PluginKeybinds)
            {
                if (keybinds.IsUniqueKeypress())
                    Screen.TogglePluginByName(pluginName);
            }
        }
    }
}