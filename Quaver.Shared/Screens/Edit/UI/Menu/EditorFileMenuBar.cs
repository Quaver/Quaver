using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ImGuiNET;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Edit.Actions.Layers.Move;
using Quaver.Shared.Screens.Edit.Dialogs;
using Quaver.Shared.Screens.Edit.Dialogs.Metadata;
using Quaver.Shared.Screens.Edit.Input;
using Quaver.Shared.Screens.Edit.Plugins;
using Quaver.Shared.Screens.Edit.UI.Playfield.Waveform;
using Wobble;
using Wobble.Audio.Samples;
using Wobble.Graphics;
using Wobble.Graphics.ImGUI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;
using Utils = Wobble.Platform.Utils;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace Quaver.Shared.Screens.Edit.UI.Menu
{
    public class EditorFileMenuBar : SpriteImGui
    {
        /// <summary>
        /// </summary>
        private EditScreen Screen { get; }

        private EditorInputConfig InputConfig { get; }

        /// <summary>
        /// </summary>
        public float Height { get; private set; }

        /// <summary>
        /// </summary>
        public bool IsActive { get; private set; }

#if VISUAL_TESTS
        private static bool DestroyContext { get; } = false;
#else
        private static bool DestroyContext { get; } = true;
#endif

        public EditorFileMenuBar(EditScreen screen) : base(DestroyContext, GetOptions())
        {
            Screen = screen;
            InputConfig = Screen.EditorInputManager.InputConfig;
        }


        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void RenderImguiLayout()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 2);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 10));
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(12, 4));
            ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0, 0, 24, 0));
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0, 0, 24, 0));

            if (!ImGui.BeginMainMenuBar())
                return;

            Height = ImGui.GetWindowSize().Y;

            CreateFileSection();
            CreateEditSection();
            CreateViewSection();
            CreateAudioSection();
            CreateWebSection();
            CreateToolsSection();
            CreatePluginsSection();
            CreateHelpSection();
            CreateKeybindsSection();

            ImGui.EndMenuBar();
            Button.IsGloballyClickable = !ImGui.IsAnyItemHovered();
            IsActive = ImGui.IsAnyItemActive() || ImGui.IsAnyItemHovered() || ImGui.IsAnyItemFocused();

            ImGui.PopStyleVar();
        }

        /// <summary>
        /// </summary>
        private void CreateFileSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu("File"))
                return;

            if (ImGui.MenuItem("New Song", InputConfig.GetOrDefault(KeybindActions.CreateNewMapset).ToString()))
                DialogManager.Show(new EditorNewSongDialog());

            if (ImGui.BeginMenu("Create New Difficulty", Screen.Map.Game == MapGame.Quaver))
            {
                if (ImGui.MenuItem("New Map", InputConfig.GetOrDefault(KeybindActions.CreateNewDifficulty).ToString()))
                    Screen.CreateNewDifficulty(false);

                if (ImGui.MenuItem("Copy Current Map", InputConfig.GetOrDefault(KeybindActions.CreateNewDifficultyFromCurrent).ToString()))
                    Screen.CreateNewDifficulty();

                if (ImGui.MenuItem("From .qua File"))
                    DialogManager.Show(new EditorAddDifficultyFromQuaDialog(Screen));

                ImGui.EndMenu();
            }

            if (ImGui.MenuItem("Save", InputConfig.GetOrDefault(KeybindActions.SaveMap).ToString(), false, Screen.ActionManager.HasUnsavedChanges))
                Screen.Save();

            if (ImGui.MenuItem("Refresh File Cache", InputConfig.GetOrDefault(KeybindActions.RefreshFileCache).ToString(), false, Screen.Map.Game == MapGame.Quaver))
                Screen.RefreshFileCache();

            ImGui.Separator();

            if (ImGui.MenuItem("Upload", InputConfig.GetOrDefault(KeybindActions.UploadMapset).ToString(), false, Screen.Map.Game == MapGame.Quaver))
            {
                Screen.UploadMapset();
            }

            if (ImGui.MenuItem("Submit For Rank", InputConfig.GetOrDefault(KeybindActions.SubmitForRanked).ToString(), false, Screen.Map.Game == MapGame.Quaver
                                                                                                                              && Screen.Map.RankedStatus != RankedStatus.Ranked && Screen.Map.MapId != -1))
            {
                Screen.SubmitForRank();
            }

            if (ImGui.MenuItem("Export", InputConfig.GetOrDefault(KeybindActions.Export).ToString(), false))
            {
                Screen.ExportToZip();
            }


            ImGui.Separator();

            if (ImGui.MenuItem("Open Song Folder", InputConfig.GetOrDefault(KeybindActions.OpenMapDirectory).ToString(), false, Screen.Map.Game == MapGame.Quaver))
            {
                Screen.Map.OpenFolder();
            }

            if (ImGui.MenuItem("Open .qua File", InputConfig.GetOrDefault(KeybindActions.OpenMapFile).ToString(), false, Screen.Map.Game == MapGame.Quaver))
            {
                Screen.Map.OpenFile();
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Exit", InputConfig.GetOrDefault(KeybindActions.ExitEditor).ToString(), false))
                Screen.LeaveEditor();

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreateEditSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu("Edit"))
                return;

            if (ImGui.MenuItem("Undo", InputConfig.GetOrDefault(KeybindActions.UndoAction).ToString(), false, Screen.ActionManager.UndoStack.Count != 0))
                Screen.ActionManager.Undo();

            if (ImGui.MenuItem("Redo", InputConfig.GetOrDefault(KeybindActions.RedoAction).ToString(), false, Screen.ActionManager.RedoStack.Count != 0))
                Screen.ActionManager.Redo();

            ImGui.Separator();

            if (ImGui.MenuItem("Copy", InputConfig.GetOrDefault(KeybindActions.CopyNotes).ToString(), false, Screen.SelectedHitObjects.Value.Count > 0))
                Screen.CopySelectedObjects();

            if (ImGui.MenuItem("Cut", InputConfig.GetOrDefault(KeybindActions.CutNotes).ToString(), false, Screen.SelectedHitObjects.Value.Count > 0))
                Screen.CutSelectedObjects();

            if (ImGui.MenuItem("Paste (snapped)", InputConfig.GetOrDefault(KeybindActions.PasteNotes).ToString(), false, Screen.Clipboard.Count > 0))
                Screen.PasteCopiedObjects(true);

            if (ImGui.MenuItem("Paste (unsnapped)", InputConfig.GetOrDefault(KeybindActions.PasteNoResnap).ToString(), false, Screen.Clipboard.Count > 0))
                Screen.PasteCopiedObjects(false);

            if (ImGui.MenuItem("Delete", InputConfig.GetOrDefault(KeybindActions.DeleteCurrentNotesOrSelection).ToString(), false, Screen.SelectedHitObjects.Value.Count > 0))
                Screen.DeleteSelectedObjects();

            ImGui.Separator();

            if (ImGui.MenuItem("Select All", InputConfig.GetOrDefault(KeybindActions.SelectAll).ToString(), false))
                Screen.SelectAllObjects();

            if (ImGui.MenuItem("Select All In Layer", InputConfig.GetOrDefault(KeybindActions.SelectAllInLayer).ToString(), false))
                Screen.SelectAllObjectsInLayer();

            ImGui.Separator();

            if (ImGui.MenuItem("Mirror Objects", InputConfig.GetOrDefault(KeybindActions.MirrorNotesLeftRight).ToString(), false, Screen.SelectedHitObjects.Value.Count > 0))
                Screen.FlipSelectedObjects();

            if (ImGui.MenuItem("Reverse Objects", InputConfig.GetOrDefault(KeybindActions.MirrorNotesUpDown).ToString(), false, Screen.SelectedHitObjects.Value.Count > 0))
                Screen.ReverseSelectedObjects();

            if (ImGui.BeginMenu($"Move Objects To Layer", Screen.SelectedHitObjects.Value.Count > 0))
            {
                if (ImGui.MenuItem("Default Layer", ""))
                {
                    Screen.ActionManager.Perform(new EditorActionMoveObjectsToLayer(Screen.ActionManager, Screen.WorkingMap, null,
                        new List<HitObjectInfo>(Screen.SelectedHitObjects.Value)));
                }

                if (ImGui.MenuItem("Current Layer", InputConfig.GetOrDefault(KeybindActions.MoveSelectedNotesToCurrentLayer).ToString()))
                {
                    Screen.ActionManager.Perform(new EditorActionMoveObjectsToLayer(Screen.ActionManager, Screen.WorkingMap, Screen.SelectedLayer.Value,
                        new List<HitObjectInfo>(Screen.SelectedHitObjects.Value)));
                }

                for (var i = 0; i < Screen.WorkingMap.EditorLayers.Count; i++)
                {
                    var layer = Screen.WorkingMap.EditorLayers[i];

                    if (ImGui.MenuItem(layer.Name))
                    {
                        Screen.ActionManager.Perform(new EditorActionMoveObjectsToLayer(Screen.ActionManager, Screen.WorkingMap, layer,
                            new List<HitObjectInfo>(Screen.SelectedHitObjects.Value)));
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Edit Metadata", InputConfig.GetOrDefault(KeybindActions.ToggleMetadataPanel).ToString()))
                DialogManager.Show(new EditorMetadataDialog(Screen));

            var timingPointPlugin = Screen.BuiltInPlugins[EditorBuiltInPlugin.TimingPointEditor];

            if (ImGui.MenuItem("Edit Timing Points", InputConfig.GetOrDefault(KeybindActions.ToggleTimingPointPanel).ToString(), timingPointPlugin.IsActive))
            {
                Screen.ToggleBuiltInPlugin(EditorBuiltInPlugin.TimingPointEditor);
            }

            var scrollVelocityPlugin = Screen.BuiltInPlugins[EditorBuiltInPlugin.ScrollVelocityEditor];

            if (ImGui.MenuItem("Edit Scroll Velocities", InputConfig.GetOrDefault(KeybindActions.ToggleScrollVelocityPanel).ToString(), scrollVelocityPlugin.IsActive))
            {
                Screen.ToggleBuiltInPlugin(EditorBuiltInPlugin.ScrollVelocityEditor);
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Add Background Image"))
                NotificationManager.Show(NotificationLevel.Info, "To add a background image, drag a .jpg or .png file into the window.");

            if (ImGui.MenuItem("Set Song Select Preview Time", InputConfig.GetOrDefault(KeybindActions.SetPreviewPoint).ToString()))
                Screen.ActionManager.SetPreviewTime((int)Screen.Track.Time);

            ImGui.Separator();

            var view = Screen.View as EditScreenView;

            if (ImGui.MenuItem("Enable AutoMod", InputConfig.GetOrDefault(KeybindActions.ToggleAutomod).ToString(), view?.AutoMod.IsActive.Value ?? false))
            {
                Screen.ToggleAutomod();
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Apply Offset To Map", InputConfig.GetOrDefault(KeybindActions.AdjustOffset).ToString()))
                DialogManager.Show(new EditorApplyOffsetDialog(Screen));

            var selectionNoteTypeString = Screen.SelectedHitObjects.Value.Count > 0 ? $"{Screen.SelectedHitObjects.Value.Count} Selected" : "All";
            if (ImGui.BeginMenu($"Resnap {selectionNoteTypeString} notes"))
            {
                if (ImGui.MenuItem("Resnap to common snaps", InputConfig.GetOrDefault(KeybindActions.ResnapAllNotes).ToString()))
                    Screen.ResnapAllOrSelectedNotes();
                if (ImGui.MenuItem($"Resnap to currently selected snap (1/{Screen.BeatSnap.Value})"))
                    Screen.ResnapAllOrSelectedNotes(new List<int> {Screen.BeatSnap.Value});
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu($"Apply Modifier To Map", Screen.Map.Game == MapGame.Quaver))
            {
                if (ImGui.MenuItem("Mirror"))
                    DialogManager.Show(new ApplyModifiersToMapDialog(Screen, ModIdentifier.Mirror));

                if (ImGui.MenuItem("No Long Notes"))
                    DialogManager.Show(new ApplyModifiersToMapDialog(Screen, ModIdentifier.NoLongNotes));

                if (ImGui.MenuItem("Full Long Notes"))
                    DialogManager.Show(new ApplyModifiersToMapDialog(Screen, ModIdentifier.FullLN));

                if (ImGui.MenuItem("Inverse"))
                    DialogManager.Show(new ApplyModifiersToMapDialog(Screen, ModIdentifier.Inverse));

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreateViewSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu("View"))
                return;

            if (ImGui.MenuItem("Display Gameplay Preview", InputConfig.GetOrDefault(KeybindActions.ToggleGameplayPreview).ToString(), Screen.DisplayGameplayPreview.Value))
                Screen.ToggleGameplayPreview();

            if (ImGui.BeginMenu("Reference Difficulty"))
            {
                if (ImGui.MenuItem("None", "", Screen.UneditableMap.Value == null))
                    Screen.UneditableMap.Value = null;

                foreach (var map in Screen.Map.Mapset.Maps)
                {
                    var color = ColorHelper.DifficultyToColor((float)map.Difficulty10X);

                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, 1));

                    if (ImGui.MenuItem(map.DifficultyName, "",
                            Screen.UneditableMap?.Value?.DifficultyName == map.DifficultyName))
                    {
                        Screen.ChangeReferenceDifficultyTo(map);
                    }

                    ImGui.PopStyleColor();
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.BeginMenu("Background Brightness"))
            {
                for (var i = 0; i < 11; i++)
                {
                    var value = i * 10;

                    if (ImGui.MenuItem($"{value}%", "", Screen.BackgroundBrightness.Value == value))
                        Screen.BackgroundBrightness.Value = value;
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.BeginMenu("Beat Snap Divisor"))
            {
                foreach (var snap in Screen.AvailableBeatSnaps)
                {
                    if (ImGui.MenuItem($"1/{StringHelper.AddOrdinal(snap)}", "", Screen.BeatSnap.Value == snap))
                        Screen.BeatSnap.Value = snap;
                }

                if (ImGui.MenuItem($"Open custom snap dialog", InputConfig.GetOrDefault(KeybindActions.OpenCustomSnapDialog).ToString()))
                    DialogManager.Show(new CustomBeatSnapDialog(Screen.BeatSnap, Screen.AvailableBeatSnaps));

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Beat Snap Color"))
            {
                foreach (EditorBeatSnapColor type in Enum.GetValues(typeof(EditorBeatSnapColor)))
                {
                    if (ImGui.MenuItem($"{type}", "", Screen.BeatSnapColor.Value == type))
                        Screen.BeatSnapColor.Value = type;
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Scale Scroll Speed", "", Screen.ScaleScrollSpeedWithRate.Value))
                Screen.ScaleScrollSpeedWithRate.Value = !Screen.ScaleScrollSpeedWithRate.Value;

            if (ImGui.MenuItem("Place Objects On Nearest Tick", "", Screen.PlaceObjectsOnNearestTick.Value))
                Screen.PlaceObjectsOnNearestTick.Value = !Screen.PlaceObjectsOnNearestTick.Value;

            ImGui.Separator();

            if (ImGui.BeginMenu("Waveform"))
            {
                if (ImGui.MenuItem("Visible", InputConfig.GetOrDefault(KeybindActions.ToggleWaveform).ToString(), Screen.ShowWaveform.Value))
                    Screen.ShowWaveform.Value = !Screen.ShowWaveform.Value;

                if (ImGui.BeginMenu("Brightness"))
                {
                    for (var i = 0; i < 11; i++)
                    {
                        var value = i * 10;

                        if (ImGui.MenuItem($"{value}%", "", Screen.WaveformBrightness.Value == value))
                            Screen.WaveformBrightness.Value = value;
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Audio Direction"))
                {
                    foreach (EditorPlayfieldWaveformAudioDirection type in Enum.GetValues(typeof(EditorPlayfieldWaveformAudioDirection)))
                    {
                        if (ImGui.MenuItem($"{type}", "", Screen.AudioDirection.Value == type))
                            Screen.AudioDirection.Value = type;
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Filter"))
                {
                    foreach (EditorPlayfieldWaveformFilter type in Enum.GetValues(typeof(EditorPlayfieldWaveformFilter)))
                    {
                        var shortcut = "";
                        if (type == EditorPlayfieldWaveformFilter.HighPass)
                            shortcut = InputConfig.GetOrDefault(KeybindActions.ToggleWaveformHighPassFilter).ToString();
                        else if (type == EditorPlayfieldWaveformFilter.LowPass)
                            shortcut = InputConfig.GetOrDefault(KeybindActions.ToggleWaveformLowPassFilter).ToString();

                        if (ImGui.MenuItem($"{type}", shortcut, Screen.WaveformFilter.Value == type))
                            Screen.ToggleWaveformFilter(type);
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.MenuItem("Color"))
                    DialogManager.Show(new EditorChangeWaveformColorDialog());

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.BeginMenu("Long Note Opacity"))
            {
                for (var i = 2; i < 10; i++)
                {
                    var val = (i + 1) * 10;

                    if (ImGui.MenuItem($"{val}%", "", Screen.LongNoteOpacity.Value == val))
                        Screen.LongNoteOpacity.Value = val;
                }

                ImGui.EndMenu();
            }

            if (ImGui.MenuItem("Center Objects", "", Screen.AnchorHitObjectsAtMidpoint.Value))
                Screen.AnchorHitObjectsAtMidpoint.Value = !Screen.AnchorHitObjectsAtMidpoint.Value;

            if (ImGui.MenuItem($"View Layers", "", Screen.ViewLayers.Value))
                Screen.ToggleViewLayers();

            ImGui.Separator();

            if (ImGui.MenuItem("Test Play Modifiers", ""))
                DialogManager.Show(new EditorModifierMenuDialog());

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreateToolsSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu("Tools"))
                return;

            foreach (var plugin in Screen.BuiltInPlugins)
            {
                if (ImGui.MenuItem(plugin.Value.Name, "", plugin.Value.IsActive))
                {
                    plugin.Value.IsActive = !plugin.Value.IsActive;

                    if (plugin.Value.IsActive)
                        plugin.Value.Initialize();
                }
            }

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreatePluginsSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu("Plugins"))
                return;

            if (ImGui.BeginMenu($"Local"))
            {
                var totalPlugins = 0;

                foreach (var plugin in Screen.Plugins)
                {
                    if (plugin.IsBuiltIn || plugin.IsWorkshop)
                        continue;

                    if (ImGui.BeginMenu(plugin.Name))
                    {
                        if (ImGui.MenuItem("Enabled", plugin.Author, plugin.IsActive))
                        {
                            plugin.IsActive = !plugin.IsActive;

                            if (plugin.IsActive)
                                plugin.Initialize();
                        }

                        Tooltip(plugin.Description);

                        if (ImGui.MenuItem("Upload To Workshop"))
                        {
                            var item = new SteamWorkshopItem(plugin.Name, $"{WobbleGame.WorkingDirectory}Plugins/{plugin.Directory}");

                            if (!item.HasUploaded && (SteamWorkshopItem.Current == null || SteamWorkshopItem.Current.HasUploaded))
                            {
                                NotificationManager.Show(NotificationLevel.Info, "Uploading plugin to the Steam Workshop...");

                                ThreadScheduler.Run(() =>
                                {
                                    item.Upload();

                                    while (!item.HasUploaded)
                                        Thread.Sleep(50);

                                    NotificationManager.Show(NotificationLevel.Success, "Successfully uploaded plugin to the workshop!");
                                });
                            }
                        }

                        if (ImGui.MenuItem("Open Folder"))
                            Utils.NativeUtils.OpenNatively($"{WobbleGame.WorkingDirectory}Plugins/{plugin.Directory}");

                        ImGui.EndMenu();
                    }

                    totalPlugins++;
                }

                if (totalPlugins == 0)
                {
                    if (ImGui.MenuItem("No Plugins Installed", "", false, false))
                    {
                    }
                }

                if (ImGui.MenuItem("Open local plugins folder"))
                {
                    Utils.NativeUtils.OpenNatively($"{WobbleGame.WorkingDirectory}Plugins");
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Steam Workshop"))
            {
                foreach (var plugin in Screen.Plugins)
                {
                    if (!plugin.IsWorkshop)
                        continue;

                    if (!ImGui.BeginMenu(plugin.Name))
                        continue;

                    if (ImGui.MenuItem("Enabled", plugin.Author, plugin.IsActive))
                    {
                        plugin.IsActive = !plugin.IsActive;

                        if (plugin.IsActive)
                            plugin.Initialize();
                    }

                    Tooltip(plugin.Description);

                    if (ImGui.MenuItem("Open Folder"))
                        Utils.NativeUtils.OpenNatively($"{ConfigManager.SteamWorkshopDirectory.Value}/{plugin.Directory}");

                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreateHelpSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu("Help"))
                return;

            if (ImGui.MenuItem("Wiki"))
                BrowserHelper.OpenURL($"https://wiki.quavergame.com/docs/editor");

            if (ImGui.MenuItem("Getting Started"))
                BrowserHelper.OpenURL($"https://wiki.quavergame.com/docs/editor/getting_started");

            if (ImGui.MenuItem("Editor Functions"))
                BrowserHelper.OpenURL("https://wiki.quavergame.com/docs/editor/editor_functions");

            if (ImGui.MenuItem("Editor Keybinds"))
                NotificationManager.Show(NotificationLevel.Error, "https://wiki.quavergame.com/docs/editor/editor_keybinds");

            if (ImGui.MenuItem("Plugins"))
                BrowserHelper.OpenURL("https://wiki.quavergame.com/docs/editor/plugins");

            if (ImGui.MenuItem("Ranking Criteria"))
                BrowserHelper.OpenURL("https://wiki.quavergame.com/docs/ranking/criteria");

            if (ImGui.MenuItem("Ranking Process"))
                BrowserHelper.OpenURL("https://wiki.quavergame.com/docs/ranking/process");

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreateWebSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu("Web"))
                return;

            if (ImGui.MenuItem($"View Online Listing", Screen.WorkingMap.MapId != -1))
                BrowserHelper.OpenURL($"https://quavergame.com/mapsets/map/{Screen.WorkingMap.MapId}");

            if (ImGui.MenuItem("Modding Discussion", Screen.WorkingMap.MapId != -1))
                BrowserHelper.OpenURL($"https://quavergame.com/mapsets/map/{Screen.WorkingMap.MapId}/mods");

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreateAudioSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu("Audio"))
                return;

            if (ImGui.BeginMenu("Playback Speed"))
            {
                for (var i = 0; i < 8; i++)
                {
                    var value = (i + 1) * 0.25f;

                    if (ImGui.MenuItem($"{value * 100}%", "", Math.Abs(Screen.Track.Rate - value) < 0.001))
                        Screen.Track.Rate = value;
                }

                if (ImGui.MenuItem("Increase Playback Speed", InputConfig.GetOrDefault(KeybindActions.IncreasePlaybackRate).ToString()))
                {
                    Screen.ChangeAudioPlaybackRate(Direction.Forward);
                }

                if (ImGui.MenuItem("Decrease Playback Speed", InputConfig.GetOrDefault(KeybindActions.IncreasePlaybackRate).ToString()))
                {
                    Screen.ChangeAudioPlaybackRate(Direction.Forward);
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Hitsounds"))
            {
                if (ImGui.MenuItem("Enable", InputConfig.GetOrDefault(KeybindActions.ToggleHitsounds).ToString(), Screen.EnableHitsounds.Value))
                    Screen.ToggleHitsounds();

                if (ImGui.BeginMenu("Volume"))
                {
                    if (ImGui.MenuItem($"Default ({(int)AudioSample.GlobalVolume}%)", "", Screen.HitsoundVolume.Value == -1))
                        Screen.HitsoundVolume.Value = -1;

                    for (var i = 0; i < 10; i++)
                    {
                        var val = (i + 1) * 10;

                        if (ImGui.MenuItem($"{val}%", "", Screen.HitsoundVolume.Value == val))
                            Screen.HitsoundVolume.Value = val;
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Metronome"))
            {
                if (ImGui.MenuItem($"Enable", InputConfig.GetOrDefault(KeybindActions.ToggleMetronome).ToString(), Screen.EnableMetronome.Value))
                    Screen.ToggleMetronome();

                if (ImGui.MenuItem("Play Half Beats", "", Screen.MetronomePlayHalfBeats.Value))
                    Screen.MetronomePlayHalfBeats.Value = !Screen.MetronomePlayHalfBeats.Value;

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

        private void CreateKeybindsSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu("Keybinds"))
                return;

            if (ImGui.MenuItem("Editor Keybinds"))
                NotificationManager.Show(NotificationLevel.Error, "https://wiki.quavergame.com/docs/editor/editor_keybinds");

            if (ImGui.MenuItem("Open Config File"))
                InputConfig.OpenConfigFile();

            if (ImGui.BeginMenu("Fill missing keybind actions"))
            {
                if (ImGui.MenuItem("Fill with default keys"))
                    DialogManager.Show(new EditorKeybindFillConfirmationDialog(Screen, true));

                if (ImGui.MenuItem("Fill with unbound keys"))
                    DialogManager.Show(new EditorKeybindFillConfirmationDialog(Screen, false));

                ImGui.EndMenu();
            }

            if (ImGui.MenuItem("Reset all keybinds"))
                DialogManager.Show(new EditorKeybindResetConfirmationDialog(Screen));

            ImGui.EndMenu();
        }

        private void Tooltip(string text)
        {
            if (!ImGui.IsItemHovered()) return;
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 25);
            ImGui.TextWrapped(text);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static ImGuiOptions GetOptions() => new ImGuiOptions(new List<ImGuiFont>
        {
            new ImGuiFont($@"{WobbleGame.WorkingDirectory}/Fonts/lato-black.ttf", 14),
        }, false);
    }
}