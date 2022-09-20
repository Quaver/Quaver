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
using Quaver.Shared.Screens.Edit.Dialogs;
using Quaver.Shared.Screens.Edit.Dialogs.Metadata;
using Quaver.Shared.Screens.Edit.Plugins;
using Quaver.Shared.Screens.Edit.UI.Playfield;
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

        public EditorFileMenuBar(EditScreen screen) : base(DestroyContext, GetOptions()) => Screen = screen;


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
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Resnap Selected Notes"))
            {
                if (ImGui.MenuItem($"Resnap to currently selected snap (1/{Screen.BeatSnap.Value})"))
                    Screen.ActionManager.ResnapNotes(new List<int> { Screen.BeatSnap.Value }, Screen.SelectedHitObjects.Value);
                if (ImGui.MenuItem("Resnap to 1/16 and 1/12 snaps"))
                    Screen.ActionManager.ResnapNotes(new List<int> { 16, 12 }, Screen.SelectedHitObjects.Value);
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
                foreach (var snap in Screen.AvailableBeatSnaps)
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

            ImGui.Separator();

            if (ImGui.BeginMenu("Waveform"))
            {
                if (ImGui.MenuItem("Visible", "", Screen.ShowWaveform.Value))
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
                        if (ImGui.MenuItem($"{type}", "", Screen.WaveformFilter.Value == type))
                            Screen.WaveformFilter.Value = type;
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
                Screen.ViewLayers.Value = !Screen.ViewLayers.Value;

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
