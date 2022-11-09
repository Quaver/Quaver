using System.Collections.Generic;
using System.Linq;
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
        private EditScreenView View { get; }

        private Dictionary<Keybind, HashSet<KeybindActions>> KeybindDictionary;
        private GenericKeyState PreviousKeyState;

        private const int HoldRepeatActionDelay = 250;
        private const int HoldRepeatActionInterval = 25;
        private Dictionary<KeybindActions, long> LastActionPress = new Dictionary<KeybindActions, long>();
        private Dictionary<KeybindActions, long> LastActionTime = new Dictionary<KeybindActions, long>();

        private static HashSet<KeybindActions> HoldRepeatActions = new HashSet<KeybindActions>()
        {
            KeybindActions.ZoomIn,
            KeybindActions.ZoomInLarge,
            KeybindActions.ZoomOut,
            KeybindActions.ZoomOutLarge,
            KeybindActions.SeekForwards,
            KeybindActions.SeekBackwards,
            KeybindActions.SeekForwards1ms,
            KeybindActions.SeekBackwards1ms,
            KeybindActions.IncreasePlaybackRate,
            KeybindActions.DecreasePlaybackRate,
            KeybindActions.ChangeToolUp,
            KeybindActions.ChangeToolDown,
            KeybindActions.ToggleLayerColorMode,
            KeybindActions.ChangeSelectedLayerUp,
            KeybindActions.ChangeSelectedLayerDown,
        };

        private static HashSet<KeybindActions> HoldActions = new HashSet<KeybindActions>()
        {
            KeybindActions.PlaceNoteLane1,
            KeybindActions.PlaceNoteLane2,
            KeybindActions.PlaceNoteLane3,
            KeybindActions.PlaceNoteLane4,
            KeybindActions.PlaceNoteLane5,
            KeybindActions.PlaceNoteLane6,
            KeybindActions.PlaceNoteLane7,
        };

        private static HashSet<KeybindActions> EnabledActionsDuringGameplayPreview = new HashSet<KeybindActions>()
        {
            KeybindActions.ToggleLivePlaytest
        };

        public EditorInputManager(EditScreen screen)
        {
            InputConfig = EditorInputConfig.LoadFromConfig();
            KeybindDictionary = InputConfig.ReverseDictionary();
            PreviousKeyState = new GenericKeyState(GenericKeyManager.GetPressedKeys());
            Screen = screen;
        }

        public void HandleInput()
        {
            if (DialogManager.Dialogs.Count != 0) return;
            var view = (EditScreenView)Screen.View;
            if (view.IsImGuiHovered) return;

            HandleKeypresses();
            HandlePluginKeypresses();

            HandleMouseInputs();
        }

        private void HandleKeypresses()
        {
            var keyState = new GenericKeyState(GenericKeyManager.GetPressedKeys());

            var uniqueKeypresses = keyState.UniqueKeypresses(PreviousKeyState);
            foreach (var pressedKeybind in keyState.PressedKeybinds())
            {
                HashSet<KeybindActions> actions;

                if (!KeybindDictionary.TryGetValue(pressedKeybind, out actions)) continue;

                foreach (var action in actions)
                {
                    // Disable most keybinds when gameplay preview is active
                    var view = (EditScreenView)Screen.View;
                    if (view.MapPreview != null && !view.MapPreview.InReplayMode)
                        if (!EditorInputManager.EnabledActionsDuringGameplayPreview.Contains(action))
                            continue;

                    if (uniqueKeypresses.Contains(pressedKeybind))
                    {
                        HandleAction(action);
                        LastActionPress[action] = GameBase.Game.TimeRunning;
                    }
                    else if (CanRepeat(action))
                    {
                        HandleAction(action);
                    }
                    else if (CanHold(action))
                    {
                        HandleAction(action, false);
                    }
                }
            }

            PreviousKeyState = keyState;
        }

        private bool CanRepeat(KeybindActions action)
        {
            if (!HoldRepeatActions.Contains(action))
                return false;

            var currentTime = GameBase.Game.TimeRunning;
            var lastPress = LastActionPress.GetValueOrDefault(action, currentTime);
            var lastAction = LastActionTime.GetValueOrDefault(action, currentTime);
            var timeSinceLastPress = currentTime - lastPress;
            var timeSinceLastAction = currentTime - lastAction;

            return timeSinceLastPress > HoldRepeatActionDelay && timeSinceLastAction > HoldRepeatActionInterval;
        }

        private bool CanHold(KeybindActions action)
        {
            if (!HoldActions.Contains(action))
                return false;

            var currentTime = GameBase.Game.TimeRunning;
            var lastPress = LastActionPress.GetValueOrDefault(action, currentTime);
            var timeSinceLastPress = currentTime - lastPress;
            return timeSinceLastPress > HoldRepeatActionDelay;
        }

        private void HandleAction(KeybindActions action, bool isKeypress = true)
        {
            switch (action)
            {
                case KeybindActions.ExitEditor:
                    Screen.LeaveEditor();
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
                case KeybindActions.SeekBackwards:
                    Screen.SeekInDirection(Direction.Backward);
                    break;
                case KeybindActions.SeekForwards1ms:
                    if (!Screen.Track.IsPlaying)
                        Screen.SeekTo(Screen.Track.Time + 1);
                    break;
                case KeybindActions.SeekBackwards1ms:
                    if (!Screen.Track.IsPlaying)
                        Screen.SeekTo(Screen.Track.Time - 1);
                    break;
                case KeybindActions.SeekToStartOfSelection:
                    if (Screen.SelectedHitObjects.Value.Count > 0)
                        Screen.SeekTo(Screen.SelectedHitObjects.Value.Min(h => h.StartTime), false);
                    break;
                case KeybindActions.SeekToEndOfSelection:
                    if (Screen.SelectedHitObjects.Value.Count > 0)
                        Screen.SeekTo(Screen.SelectedHitObjects.Value.Max(h => h.StartTime), false);
                    break;
                case KeybindActions.SeekToBeginning:
                    Screen.SeekToBeginning();
                    break;
                case KeybindActions.SeekToEnd:
                    Screen.SeekToEnd();
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
                case KeybindActions.DecreaseSnap:
                    Screen.ChangeBeatSnap(Direction.Backward);
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
                    Screen.ToggleViewLayers();
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
                    Screen.MoveSelectedNotesToCurrentLayer();
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
                case KeybindActions.AdjustOffset:
                    DialogManager.Show(new EditorApplyOffsetDialog(Screen));
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
                    Screen.ToggleWaveformFilter(EditorPlayfieldWaveformFilter.LowPass);
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
                case KeybindActions.SelectAll:
                    Screen.SelectAllObjects();
                    break;
                case KeybindActions.SelectAllInLayer:
                    Screen.SelectAllObjectsInLayer();
                    break;
                case KeybindActions.DeselectAll:
                    Screen.DeselectAllObjects();
                    break;
                case KeybindActions.MirrorNotesLeftRight:
                    Screen.FlipSelectedObjects();
                    break;
                case KeybindActions.MirrorNotesUpDown:
                    Screen.ReverseSelectedObjects();
                    break;
                case KeybindActions.ResnapAllNotes:
                    Screen.ResnapAllOrSelectedNotes(new List<int> {16, 12, 5, 9, 7, 11, 13, 15});
                    break;
                case KeybindActions.PlaceNoteLane1:
                    Screen.ToolInputInLane(1, isKeypress);
                    break;
                case KeybindActions.PlaceNoteLane2:
                    Screen.ToolInputInLane(2, isKeypress);
                    break;
                case KeybindActions.PlaceNoteLane3:
                    Screen.ToolInputInLane(3, isKeypress);
                    break;
                case KeybindActions.PlaceNoteLane4:
                    Screen.ToolInputInLane(4, isKeypress);
                    break;
                case KeybindActions.PlaceNoteLane5:
                    Screen.ToolInputInLane(5, isKeypress);
                    break;
                case KeybindActions.PlaceNoteLane6:
                    Screen.ToolInputInLane(6, isKeypress);
                    break;
                case KeybindActions.PlaceNoteLane7:
                    Screen.ToolInputInLane(7, isKeypress);
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
            else if (!KeyboardManager.IsAltDown() && (scrolledForward || scrolledBackward))
            {
                var forward = scrolledForward;
                if (InputConfig.ReverseScrollSeekDirection)
                    forward = !forward;

                if (forward) Screen.SeekInDirection(Direction.Forward);
                else Screen.SeekInDirection(Direction.Backward);
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