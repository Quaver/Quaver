using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Dialogs.Menu;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.Layers.Move;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.MoveObjectsToTimingGroup;
using Quaver.Shared.Screens.Edit.Dialogs;
using Quaver.Shared.Screens.Edit.Dialogs.Metadata;
using Quaver.Shared.Screens.Edit.Plugins;
using Quaver.Shared.Screens.Edit.UI.Playfield;
using Quaver.Shared.Screens.Edit.UI.Playfield.Spectrogram;
using Quaver.Shared.Screens.Edit.UI.Playfield.Waveform;
using Quaver.Shared.Screens.Editor;
using Wobble;
using Wobble.Audio.Samples;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics.ImGUI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;
using Wobble.Window;
using Color = System.Drawing.Color;
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

        public EditorFileMenuBar(EditScreen screen) : base(DestroyContext, GetOptions(), screen.ImGuiScale) => Screen = screen;


        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void RenderImguiLayout()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 2 * Screen.ImGuiScale);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 10) * Screen.ImGuiScale);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(12, 4) * Screen.ImGuiScale);
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

            if (ImGui.MenuItem("New Song", "CTRL + N"))
                DialogManager.Show(new EditorNewSongDialog());

            if (ImGui.BeginMenu("Switch Difficulty"))
            {
                foreach (var map in Screen.Map.Mapset.Maps)
                {
                    var color = ColorHelper.DifficultyToColor((float)map.Difficulty10X);
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, 1));

                    if (ImGui.MenuItem(map.DifficultyName, map != Screen.Map))
                        Screen.SwitchToMap(map);
                    ImGui.PopStyleColor();
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Create New Difficulty", Screen.Map.Game == MapGame.Quaver))
            {
                if (ImGui.MenuItem("New Map"))
                    Screen.CreateNewDifficulty(false);

                if (ImGui.MenuItem("Copy Current Map"))
                    Screen.CreateNewDifficulty();

                if (ImGui.MenuItem("From .qua File"))
                    DialogManager.Show(new EditorAddDifficultyFromQuaDialog(Screen));

                ImGui.EndMenu();
            }

            if (ImGui.MenuItem("Save", "CTRL + S", false, Screen.ActionManager.HasUnsavedChanges))
                Screen.Save();

            if (ImGui.MenuItem("Refresh File Cache", "CTRL + R", false, Screen.Map.Game == MapGame.Quaver))
                Screen.RefreshFileCache();

            ImGui.Separator();

            if (ImGui.MenuItem("Upload", "CTRL + U", false, Screen.Map.Game == MapGame.Quaver))
            {
                Screen.UploadMapset();
            }

            if (ImGui.MenuItem("Submit For Rank", "", false, Screen.Map.Game == MapGame.Quaver
                                                             && Screen.Map.RankedStatus != RankedStatus.Ranked && Screen.Map.MapId != -1))
            {
                Screen.SubmitForRank();
            }

            if (ImGui.MenuItem("Export", "CTRL + E", false))
            {
                Screen.ExportToZip();
            }


            ImGui.Separator();

            if (ImGui.MenuItem("Open Song Folder", "CTRL + W", false, Screen.Map.Game == MapGame.Quaver))
            {
                try
                {
                    Utils.NativeUtils.OpenNatively($"{ConfigManager.SongDirectory.Value}/{Screen.Map.Directory}");
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }

            if (ImGui.MenuItem("Open .qua File", "CTRL + Q", false, Screen.Map.Game == MapGame.Quaver))
            {
                try
                {
                    Utils.NativeUtils.OpenNatively($"{ConfigManager.SongDirectory.Value}/{Screen.Map.Directory}/{Screen.Map.Path}");
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Exit", "ESC", false))
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

            if (ImGui.MenuItem("Undo", "CTRL + Z", false, Screen.ActionManager.UndoStack.Count != 0))
                Screen.ActionManager.Undo();

            if (ImGui.MenuItem("Redo", "CTRL + Y", false, Screen.ActionManager.RedoStack.Count != 0))
                Screen.ActionManager.Redo();

            ImGui.Separator();

            if (ImGui.MenuItem("Copy", "CTRL + C", false, Screen.SelectedHitObjects.Value.Count > 0))
                Screen.CopySelectedObjects();

            if (ImGui.MenuItem("Cut", "CTRL + X", false, Screen.SelectedHitObjects.Value.Count > 0))
                Screen.CutSelectedObjects();

            if (ImGui.MenuItem("Paste (snapped)", "CTRL + V", false, Screen.Clipboard.Count > 0))
                Screen.PasteCopiedObjects(true);

            if (ImGui.MenuItem("Paste (unsnapped)", "CTRL + SHIFT + V", false, Screen.Clipboard.Count > 0))
                Screen.PasteCopiedObjects(false);

            if (ImGui.MenuItem("Delete", "DEL", false, Screen.SelectedHitObjects.Value.Count > 0))
                Screen.DeleteSelectedObjects();

            ImGui.Separator();

            if (ImGui.MenuItem("Select All", "CTRL + A", false))
                Screen.SelectAllObjects();

            if (ImGui.MenuItem("Select All In Layer", $"CTRL + ALT + A", false))
                Screen.SelectAllObjectsInLayer();

            ImGui.Separator();

            if (ImGui.MenuItem("Flip Objects", "CTRL + H", false, Screen.SelectedHitObjects.Value.Count > 0))
                Screen.FlipSelectedObjects();

            if (ImGui.BeginMenu("Swap Lanes of Objects", Screen.SelectedHitObjects.Value.Count > 0))
            {
                for (var i = 1; i <= Screen.WorkingMap.GetKeyCount(); i++)
                {
                    if (ImGui.BeginMenu($"Lane {i}"))
                    { 
                        for (var j = 1; j <= Screen.WorkingMap.GetKeyCount(); j++)
                        {
                            if (i == j) continue;
                            if (ImGui.MenuItem($"Lane {j}", $"ALT + {i} + {j}"))
                            {
                                Screen.SwapSelectedObjects(i, j);
                            }
                        }

                        ImGui.EndMenu();
                    }
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu($"Move Objects To Layer", Screen.SelectedHitObjects.Value.Count > 0))
            {
                if (ImGui.MenuItem("Default Layer", ""))
                {
                    Screen.ActionManager.Perform(new EditorActionMoveObjectsToLayer(Screen.ActionManager, Screen.WorkingMap, null,
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

            if (ImGui.BeginMenu($"Move Objects To Timing Group", Screen.SelectedHitObjects.Value.Count > 0))
            {
                if (ImGui.MenuItem("Default Timing Group", ""))
                {
                    Screen.ActionManager.Perform(new EditorActionMoveObjectsToTimingGroup(Screen.ActionManager, Screen.WorkingMap,
                        new List<HitObjectInfo>(Screen.SelectedHitObjects.Value), Qua.DefaultScrollGroupId));
                }

                ImGui.Separator();

                foreach ((string id, TimingGroup timingGroup) in Screen.WorkingMap.TimingGroups)
                {
                    if (id == Qua.DefaultScrollGroupId)
                        continue;

                    var color = timingGroup.GetColor();
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, 1f));
                    if (ImGui.MenuItem(id))
                    {
                        Screen.ActionManager.Perform(new EditorActionMoveObjectsToTimingGroup(Screen.ActionManager, Screen.WorkingMap,
                            new List<HitObjectInfo>(Screen.SelectedHitObjects.Value), id));
                    }
                    ImGui.PopStyleColor();
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Edit Metadata", "F1"))
                DialogManager.Show(new EditorMetadataDialog(Screen));

            var timingPointPlugin = Screen.BuiltInPlugins[EditorBuiltInPlugin.TimingPointEditor];

            if (ImGui.MenuItem("Edit Timing Points", "F5", timingPointPlugin.IsActive))
            {
                timingPointPlugin.IsActive = !timingPointPlugin.IsActive;

                if (timingPointPlugin.IsActive)
                    timingPointPlugin.Initialize();
            }

            var scrollVelocityPlugin = Screen.BuiltInPlugins[EditorBuiltInPlugin.ScrollVelocityEditor];

            if (ImGui.MenuItem("Edit Scroll Velocities", "F6", scrollVelocityPlugin.IsActive))
            {
                scrollVelocityPlugin.IsActive = !scrollVelocityPlugin.IsActive;

                if (scrollVelocityPlugin.IsActive)
                    scrollVelocityPlugin.Initialize();
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Add Background Image"))
                NotificationManager.Show(NotificationLevel.Info, "To add a background image, drag a .jpg or .png file into the window.");

            if (ImGui.MenuItem("Set Song Select Preview Time"))
                Screen.ActionManager.SetPreviewTime((int) Screen.Track.Time);

            ImGui.Separator();

            var view = Screen.View as EditScreenView;

            if (ImGui.MenuItem("Enable AutoMod", "", view?.AutoMod.IsActive.Value ?? false))
            {
                if (view != null)
                    view.AutoMod.IsActive.Value = !view.AutoMod.IsActive.Value;
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Apply Offset To Map"))
                DialogManager.Show(new EditorApplyOffsetDialog(Screen));

            if (ImGui.BeginMenu("Resnap All Notes"))
            {
                if (ImGui.MenuItem($"Resnap to currently selected snap (1/{Screen.BeatSnap.Value})"))
                    Screen.ActionManager.ResnapNotes(new List<int> { Screen.BeatSnap.Value }, Screen.WorkingMap.HitObjects);
                if (ImGui.MenuItem("Resnap to 1/16 and 1/12 snaps"))
                    Screen.ActionManager.ResnapNotes(new List<int> { 16, 12 }, Screen.WorkingMap.HitObjects);
                if (ImGui.BeginMenu("Resnap to custom divisions"))
                {
                    if (ImGui.MenuItem("Resnap"))
                    {
                        if (EditScreen.CustomSnapDivisions.Count == 0)
                            NotificationManager.Show(NotificationLevel.Warning,
                                "You have not selected any divisions to snap!");
                        else
                            Screen.ActionManager.ResnapNotes(EditScreen.CustomSnapDivisions.ToList(),
                                Screen.WorkingMap.HitObjects);
                    }

                    ImGui.Separator();
                    foreach (var availableBeatSnap in EditScreen.AvailableBeatSnaps)
                    {
                        var color = ColorHelper.BeatSnapToColor(availableBeatSnap);
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, 1));

                        if (ImGui.MenuItem($"1/{StringHelper.AddOrdinal(availableBeatSnap)}", "",
                                EditScreen.CustomSnapDivisions.Contains(availableBeatSnap)))
                        {
                            if (!EditScreen.CustomSnapDivisions.Add(availableBeatSnap))
                                EditScreen.CustomSnapDivisions.Remove(availableBeatSnap);
                        }

                        ImGui.PopStyleColor();
                    }

                    ImGui.EndMenu();
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Resnap Selected Notes"))
            {
                if (ImGui.MenuItem($"Resnap to currently selected snap (1/{Screen.BeatSnap.Value})"))
                    Screen.ActionManager.ResnapNotes(new List<int> { Screen.BeatSnap.Value }, Screen.SelectedHitObjects.Value);
                if (ImGui.MenuItem("Resnap to 1/16 and 1/12 snaps"))
                    Screen.ActionManager.ResnapNotes(new List<int> { 16, 12 }, Screen.SelectedHitObjects.Value);
                if (ImGui.BeginMenu("Resnap to custom divisions"))
                {
                    if (ImGui.MenuItem("Resnap"))
                    {
                        if (EditScreen.CustomSnapDivisions.Count == 0)
                            NotificationManager.Show(NotificationLevel.Warning,
                                "You have not selected any divisions to snap!");
                        else
                            Screen.ActionManager.ResnapNotes(EditScreen.CustomSnapDivisions.ToList(),
                                Screen.SelectedHitObjects.Value);
                    }

                    ImGui.Separator();
                    foreach (var availableBeatSnap in EditScreen.AvailableBeatSnaps)
                    {
                        var color = ColorHelper.BeatSnapToColor(availableBeatSnap);
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, 1));

                        if (ImGui.MenuItem($"1/{StringHelper.AddOrdinal(availableBeatSnap)}", "",
                                EditScreen.CustomSnapDivisions.Contains(availableBeatSnap)))
                        {
                            if (!EditScreen.CustomSnapDivisions.Add(availableBeatSnap))
                                EditScreen.CustomSnapDivisions.Remove(availableBeatSnap);
                        }

                        ImGui.PopStyleColor();
                    }

                    ImGui.EndMenu();
                }
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

            if (ImGui.MenuItem("Display Gameplay Preview", "", Screen.DisplayGameplayPreview.Value))
                Screen.DisplayGameplayPreview.Value = !Screen.DisplayGameplayPreview.Value;

            if (ImGui.BeginMenu("Reference Difficulty"))
            {
                if (ImGui.MenuItem("None", "", Screen.UneditableMap.Value == null))
                    Screen.UneditableMap.Value = null;

                foreach (var map in Screen.Map.Mapset.Maps)
                {
                    var color = ColorHelper.DifficultyToColor((float) map.Difficulty10X);

                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, 1));

                    if (ImGui.MenuItem(map.DifficultyName, "",
                        Screen.UneditableMap?.Value?.DifficultyName == map.DifficultyName))
                    {
                        ThreadScheduler.Run(() =>
                        {
                            if (Screen.UneditableMap.Value != null)
                            {
                                lock (Screen.UneditableMap.Value)
                                    Screen.UneditableMap.Value = map.LoadQua();
                            }
                            else
                                Screen.UneditableMap.Value = map.LoadQua();
                        });
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
                foreach (var snap in EditScreen.AvailableBeatSnaps)
                {
                    if (ImGui.MenuItem($"1/{StringHelper.AddOrdinal(snap)}", "", Screen.BeatSnap.Value == snap))
                        Screen.BeatSnap.Value = snap;
                }

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

            if (ImGui.MenuItem("Place Objects With Top Row Numbers", "", Screen.LiveMapping.Value))
                Screen.LiveMapping.Value = !Screen.LiveMapping.Value;

            if (ImGui.MenuItem("Snap Notes When Live Mapping", "", ConfigManager.EditorLiveMapSnap.Value))
                ConfigManager.EditorLiveMapSnap.Value = !ConfigManager.EditorLiveMapSnap.Value;
            
            if (ImGui.MenuItem("Set Offset For Notes Placed During Live Mapping"))
                DialogManager.Show(new EditorSetLiveMapOffsetDialog(Screen));

            if (ImGui.MenuItem("Place Long Notes When Live Mapping", "", ConfigManager.EditorLiveMapLongNote.Value))
                ConfigManager.EditorLiveMapLongNote.Value = !ConfigManager.EditorLiveMapLongNote.Value;

            if (ImGui.MenuItem("Set Minimum Length Of Long Notes Placed During Live Mapping"))
                DialogManager.Show(new EditorSetLiveMapLongNoteThresholdDialog(Screen));

            if (ImGui.MenuItem("Invert Beat Snap Scroll", "", Screen.InvertBeatSnapScroll.Value))
                Screen.InvertBeatSnapScroll.Value = !Screen.InvertBeatSnapScroll.Value;

            ImGui.Separator();

            if (ImGui.BeginMenu("Waveform"))
            {
                if (ImGui.MenuItem("Visible", "", Screen.ShowWaveform.Value))
                {
                    if (!Screen.ShowWaveform.Value) Screen.ShowSpectrogram.Value = false;
                    Screen.ShowWaveform.Value = !Screen.ShowWaveform.Value;
                }

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
                        if (ImGui.MenuItem($"{type}", "", Screen.WaveformFilter.Value == type))
                            Screen.WaveformFilter.Value = type;
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.MenuItem("Color"))
                    DialogManager.Show(new EditorChangeWaveformColorDialog());

                ImGui.EndMenu();
            }
            
            if (ImGui.BeginMenu("Spectrogram"))
            {
                if (ImGui.MenuItem("Visible", "", Screen.ShowSpectrogram.Value))
                {
                    if (!Screen.ShowSpectrogram.Value) Screen.ShowWaveform.Value = false;
                    Screen.ShowSpectrogram.Value = !Screen.ShowSpectrogram.Value;
                }

                if (ImGui.BeginMenu("Brightness"))
                {
                    for (var i = 0; i < 11; i++)
                    {
                        var value = i * 10;

                        if (ImGui.MenuItem($"{value}%", "", Screen.SpectrogramBrightness.Value == value))
                            Screen.SpectrogramBrightness.Value = value;
                    }

                    ImGui.EndMenu();
                }
                
                if (ImGui.BeginMenu("Precision"))
                {
                    for (var interleaveCount = 1; interleaveCount <= 16; interleaveCount *= 2)
                    {
                        if (ImGui.MenuItem($"{interleaveCount}x", "",
                                ConfigManager.EditorSpectrogramInterleaveCount.Value == interleaveCount))
                        {
                            ConfigManager.EditorSpectrogramInterleaveCount.Value = interleaveCount;
                        }
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("FFT Size"))
                {
                    for (var size = 256; size <= 16384; size *= 2)
                    {
                        if (ImGui.MenuItem($"{size}", "", Screen.SpectrogramFftSize.Value == size))
                            Screen.SpectrogramFftSize.Value = size;
                    }
                    ImGui.EndMenu();
                }
                
                if (ImGui.BeginMenu("Frequency Scale"))
                {
                    foreach (var scale in Enum.GetValues<EditorPlayfieldSpectrogramFrequencyScale>())
                    {
                        if (ImGui.MenuItem($"{scale}", "", scale == ConfigManager.EditorSpectrogramFrequencyScale.Value))
                            ConfigManager.EditorSpectrogramFrequencyScale.Value = scale;
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Cutoff Factor"))
                {
                    for (var i = 0; i <= 10; i++)
                    {
                        var f = 0.2f + 0.02f * i;
                        if (ImGui.MenuItem($"{f:0.00}", "", Math.Abs(f - ConfigManager.EditorSpectrogramCutoffFactor.Value) < 0.01f))
                            ConfigManager.EditorSpectrogramCutoffFactor.Value = f;
                    }
                    ImGui.EndMenu();
                }
                
                if (ImGui.BeginMenu("Intensity Factor"))
                {
                    for (var i = 0; i < 10; i++)
                    {
                        var f = 0.5f * i + 5.0f;
                        if (ImGui.MenuItem($"{f}", "", Math.Abs(f - ConfigManager.EditorSpectrogramIntensityFactor.Value) < 0.01f))
                            ConfigManager.EditorSpectrogramIntensityFactor.Value = f;
                    }
                    ImGui.EndMenu();
                }
                
                if (ImGui.BeginMenu("Minimum Frequency"))
                {
                    for (var f = 0; f <= 1500; f += 125)
                    {
                        if (ImGui.MenuItem($"{f}", "", ConfigManager.EditorSpectrogramMinimumFrequency.Value == f,
                                ConfigManager.EditorSpectrogramMaximumFrequency.Value > f)) 
                            ConfigManager.EditorSpectrogramMinimumFrequency.Value = f;
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Maximum Frequency"))
                {
                    for (var f = 5000; f <= 10000; f += 1000)
                    {
                        if (ImGui.MenuItem($"{f}", "", ConfigManager.EditorSpectrogramMaximumFrequency.Value == f,
                                ConfigManager.EditorSpectrogramMinimumFrequency.Value < f)) 
                            ConfigManager.EditorSpectrogramMaximumFrequency.Value = f;
                    }
                    ImGui.EndMenu();
                }
                
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

            if (ImGui.MenuItem($"View Layers", "", Screen.ObjectColoring.Value == HitObjectColoring.Layer))
                Screen.ObjectColoring.Value = Screen.ObjectColoring.Value == HitObjectColoring.Layer
                    ? HitObjectColoring.None
                    : HitObjectColoring.Layer;

            if (ImGui.MenuItem($"View Timing Groups", "", Screen.ObjectColoring.Value == HitObjectColoring.TimingGroup))
                Screen.ObjectColoring.Value = Screen.ObjectColoring.Value == HitObjectColoring.TimingGroup
                    ? HitObjectColoring.None
                    : HitObjectColoring.TimingGroup;

            if (ImGui.MenuItem($"Color SV Lines By Timing Group", "", ConfigManager.EditorColorSvLineByTimingGroup.Value))
                ConfigManager.EditorColorSvLineByTimingGroup.Value = !ConfigManager.EditorColorSvLineByTimingGroup.Value;

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

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Hitsounds"))
            {
                if (ImGui.MenuItem("Enable", "", Screen.EnableHitsounds.Value))
                    Screen.EnableHitsounds.Value = !Screen.EnableHitsounds.Value;

                if (ImGui.BeginMenu("Volume"))
                {
                    if (ImGui.MenuItem($"Default ({(int) AudioSample.GlobalVolume}%)", "", Screen.HitsoundVolume.Value == -1))
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
                if (ImGui.MenuItem($"Enable", "", Screen.EnableMetronome.Value))
                    Screen.EnableMetronome.Value = !Screen.EnableMetronome.Value;

                if (ImGui.MenuItem("Play Half Beats", "", Screen.MetronomePlayHalfBeats.Value))
                    Screen.MetronomePlayHalfBeats.Value = !Screen.MetronomePlayHalfBeats.Value;

                ImGui.EndMenu();
            }

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
