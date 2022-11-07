using System;
using System.Collections.Generic;
using Wobble;

namespace Quaver.Shared.Screens.Edit.Input
{
    public class EditorInputManager
    {
        public EditorInputConfig InputConfig { get; }
        public EditScreen EditScreen { get; }

        private const int HoldRepeatActionDelay = 200;
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
            KeybindActions.ChangeSelectedComparisonDifficultyPrevious,
            KeybindActions.ChangeSelectedComparisonDifficultyNext
        };

        public EditorInputManager(EditScreen screen)
        {
            InputConfig = EditorInputConfig.LoadFromConfig();
            EditScreen = screen;
        }

        public void HandleInput()
        {
            HandleKeypresses();
            HandlePluginKeypresses();
        }

        private void HandleKeypresses()
        {
            foreach (var (action, keybinds) in InputConfig.Keybinds)
            {
                if (keybinds.IsUniqueKeypress() || (keybinds.IsDown() && CanRepeat(action)))
                    HandleAction(action);
            }
        }

        private bool CanRepeat(KeybindActions action)
        {
            if (!HoldActions.Contains(action))
                return false;

            var currentTime = GameBase.Game.TimeRunning;
            var lastPress = LastActionTime.GetValueOrDefault(action, currentTime);
            var timeSinceLastPress = currentTime - lastPress;

            return timeSinceLastPress > HoldRepeatActionDelay;
        }

        private void HandleAction(KeybindActions action)
        {
            switch (action)
            {
                case KeybindActions.ExitEditor:
                    EditScreen.ExitToSongSelect();
                    break;
                case KeybindActions.PlayPause:
                    EditScreen.TogglePlayPause();
                    break;
                case KeybindActions.ZoomIn:
                    EditScreen.AdjustZoom(1);
                    break;
                case KeybindActions.ZoomInLarge:
                    EditScreen.AdjustZoom(5);
                    break;
                case KeybindActions.ZoomOut:
                    EditScreen.AdjustZoom(-1);
                    break;
                case KeybindActions.ZoomOutLarge:
                    EditScreen.AdjustZoom(-5);
                    break;
                case KeybindActions.OpenGoToDialog: break;
                case KeybindActions.SeekForwards: break;
                case KeybindActions.SeekForwardsWithSelection: break;
                case KeybindActions.SeekForwardsWithMove: break;
                case KeybindActions.SeekBackwards: break;
                case KeybindActions.SeekBackwardsWithSelection: break;
                case KeybindActions.SeekBackwardsWithMove: break;
                case KeybindActions.SeekForwards1ms: break;
                case KeybindActions.SeekForwards1msWithSelection: break;
                case KeybindActions.SeekForwards1msWithMove: break;
                case KeybindActions.SeekBackwards1ms: break;
                case KeybindActions.SeekBackwards1msWithSelection: break;
                case KeybindActions.SeekBackwards1msWithMove: break;
                case KeybindActions.SeekToStartOfSelection: break;
                case KeybindActions.SeekToStartOfSelectionWithMove: break;
                case KeybindActions.SeekToEndOfSelection: break;
                case KeybindActions.SeekToEndOfSelectionWithMove: break;
                case KeybindActions.SeekToBeginning: break;
                case KeybindActions.SeekToBeginningWithSelection: break;
                case KeybindActions.SeekToBeginningWithMove: break;
                case KeybindActions.SeekToEnd: break;
                case KeybindActions.SeekToEndWithSelection: break;
                case KeybindActions.SeekToEndWithMove: break;
                case KeybindActions.IncreasePlaybackRate: break;
                case KeybindActions.DecreasePlaybackRate: break;
                case KeybindActions.ChangeToolUp: break;
                case KeybindActions.ChangeToolDown: break;
                case KeybindActions.ChangeToolToSelect: break;
                case KeybindActions.ChangeToolToNote: break;
                case KeybindActions.ChangeToolToLongNote: break;
                case KeybindActions.IncreaseSnap: break;
                case KeybindActions.IncreaseSnapLarge: break;
                case KeybindActions.DecreaseSnap: break;
                case KeybindActions.DecreaseSnapLarge: break;
                case KeybindActions.ChangeSnapTo1: break;
                case KeybindActions.ChangeSnapTo2: break;
                case KeybindActions.ChangeSnapTo3: break;
                case KeybindActions.ChangeSnapTo4: break;
                case KeybindActions.ChangeSnapTo5: break;
                case KeybindActions.ChangeSnapTo6: break;
                case KeybindActions.ChangeSnapTo7: break;
                case KeybindActions.ChangeSnapTo8: break;
                case KeybindActions.ChangeSnapTo9: break;
                case KeybindActions.ChangeSnapTo10: break;
                case KeybindActions.ChangeSnapTo12: break;
                case KeybindActions.ChangeSnapTo16: break;
                case KeybindActions.OpenCustomSnapDialog: break;
                case KeybindActions.ToggleLayerColorMode: break;
                case KeybindActions.ChangeSelectedLayerUp: break;
                case KeybindActions.ChangeSelectedLayerDown: break;
                case KeybindActions.ToggleCurrentLayerVisibility: break;
                case KeybindActions.ToggleAllLayersVisibility: break;
                case KeybindActions.MoveSelectedNotesToCurrentLayer: break;
                case KeybindActions.MoveSelectedNotesToLayerUp: break;
                case KeybindActions.MoveSelectedNotesToLayerDown: break;
                case KeybindActions.CreateNewLayer: break;
                case KeybindActions.DeleteCurrentLayer: break;
                case KeybindActions.RenameCurrentLayer: break;
                case KeybindActions.RecolorCurrentLayer: break;
                case KeybindActions.CreateNewMapset: break;
                case KeybindActions.CreateNewDifficulty: break;
                case KeybindActions.CreateNewDifficultyFromCurrent: break;
                case KeybindActions.RefreshFileCache: break;
                case KeybindActions.OpenMapFile: break;
                case KeybindActions.OpenMapDirectory: break;
                case KeybindActions.Export: break;
                case KeybindActions.UndoAction: break;
                case KeybindActions.RedoAction: break;
                case KeybindActions.SaveMap: break;
                case KeybindActions.UploadMapset: break;
                case KeybindActions.SetPreviewPoint: break;
                case KeybindActions.SubmitForRanked: break;
                case KeybindActions.ToggleAutomod: break;
                case KeybindActions.ToggleGameplayPreview: break;
                case KeybindActions.ToggleDifficultyComparison: break;
                case KeybindActions.ChangeSelectedComparisonDifficultyPrevious: break;
                case KeybindActions.ChangeSelectedComparisonDifficultyNext: break;
                case KeybindActions.ToggleWaveform: break;
                case KeybindActions.ToggleWaveformLowPassFilter: break;
                case KeybindActions.ToggleWaveformHighPassFilter: break;
                case KeybindActions.ToggleHitsounds: break;
                case KeybindActions.ToggleMetronome: break;
                case KeybindActions.TogglePitchWithRate: break;
                case KeybindActions.ToggleLivePlaytest: break;
                case KeybindActions.OpenGameplayTestModifiers: break;
                case KeybindActions.StartGameplayTest: break;
                case KeybindActions.StartGameplayTestFromBeginning: break;
                case KeybindActions.CutNotes: break;
                case KeybindActions.CopyNotes: break;
                case KeybindActions.PasteNotes: break;
                case KeybindActions.PasteNoResnap: break;
                case KeybindActions.DeleteCurrentNotesOrSelection: break;
                case KeybindActions.DeleteCurrentNotesOrSelectionAndMove: break;
                case KeybindActions.SelectAll: break;
                case KeybindActions.SelectAllInLayer: break;
                case KeybindActions.SelectToEnd: break;
                case KeybindActions.SelectToBeginning: break;
                case KeybindActions.MirrorNotesLeftRight: break;
                case KeybindActions.MirrorNotesUpDown: break;
                case KeybindActions.ResnapAllNotes: break;
                case KeybindActions.ResnapModifiedOrSelectedNotes: break;
                case KeybindActions.PlaceNoteLane1: break;
                case KeybindActions.PlaceNoteLane2: break;
                case KeybindActions.PlaceNoteLane3: break;
                case KeybindActions.PlaceNoteLane4: break;
                case KeybindActions.PlaceNoteLane5: break;
                case KeybindActions.PlaceNoteLane6: break;
                case KeybindActions.PlaceNoteLane7: break;
                case KeybindActions.PlaceTimingPoint: break;
                case KeybindActions.PlaceScrollVelocity: break;
                case KeybindActions.ToggleMetadataPanel: break;
                case KeybindActions.ToggleTimingPointPanel: break;
                case KeybindActions.ToggleScrollVelocityPanel: break;
                case KeybindActions.ToggleGoToObjectsPanel: break;
                case KeybindActions.CloseAllPlugins: break;
                case KeybindActions.ReloadPlugins: break;
                default:
                    return;
            }

            LastActionTime[action] = GameBase.Game.TimeRunning;
        }

        private void HandlePluginKeypresses()
        {
            // TODO
        }
    }
}