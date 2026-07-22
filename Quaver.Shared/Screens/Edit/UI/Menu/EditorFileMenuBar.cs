using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Dialogs.Menu;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Transitions;
using Quaver.Shared.Helpers;
using Quaver.Shared.Localization;
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
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Audio.Samples;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics.ImGUI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;
using Wobble.Managers;
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

        public EditorFileMenuBar(EditScreen screen) : base(DestroyContext, EditorImGuiOptions.GetOptions(), screen.ImGuiScale) => Screen = screen;


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
            CreateKeybindsSection();
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

            if (!ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_File")))
                return;

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_NewSong"), "CTRL + N"))
                DialogManager.Show(new EditorNewSongDialog());

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_SwitchDifficulty")))
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

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_CreateNewDifficulty"), Screen.Map.Game == MapGame.Quaver))
            {
                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_NewMap")))
                    Screen.CreateNewDifficulty(false);

                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_CopyCurrentMap")))
                    Screen.CreateNewDifficulty();

                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_FromQuaFile")))
                    DialogManager.Show(new EditorAddDifficultyFromQuaDialog(Screen));

                ImGui.EndMenu();
            }

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Save"), "CTRL + S", false, Screen.ActionManager.HasUnsavedChanges))
                Screen.Save();

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_RefreshFileCache"), "CTRL + R", false, Screen.Map.Game == MapGame.Quaver))
                Screen.RefreshFileCache();

            ImGui.Separator();

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Upload"), "CTRL + U", false, Screen.Map.Game == MapGame.Quaver))
            {
                Screen.UploadMapset();
            }

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_SubmitForRank"), "", false, Screen.Map.Game == MapGame.Quaver
                                                             && Screen.Map.RankedStatus != RankedStatus.Ranked && Screen.Map.MapId != -1))
            {
                Screen.SubmitForRank();
            }

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Export"), "CTRL + E", false))
            {
                Screen.ExportToZip();
            }


            ImGui.Separator();

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_OpenSongFolder"), "CTRL + W", false, Screen.Map.Game == MapGame.Quaver))
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

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_OpenQuaFile"), "CTRL + Q", false, Screen.Map.Game == MapGame.Quaver))
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

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Exit"), "ESC", false))
                Screen.LeaveEditor();

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreateEditSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_Edit")))
                return;

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Undo"), "CTRL + Z", false, Screen.ActionManager.UndoStack.Count != 0))
                Screen.ActionManager.Undo();

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Redo"), "CTRL + Y", false, Screen.ActionManager.RedoStack.Count != 0))
                Screen.ActionManager.Redo();

            ImGui.Separator();

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Copy"), "CTRL + C", false, Screen.SelectedHitObjects.Value.Count > 0))
                Screen.CopySelectedObjects();

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Cut"), "CTRL + X", false, Screen.SelectedHitObjects.Value.Count > 0))
                Screen.CutSelectedObjects();

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_PasteSnapped"), "CTRL + V", false, Screen.Clipboard.Count > 0))
                Screen.PasteCopiedObjects(true);

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_PasteUnsnapped"), "CTRL + SHIFT + V", false, Screen.Clipboard.Count > 0))
                Screen.PasteCopiedObjects(false);

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Delete"), "DEL", false, Screen.SelectedHitObjects.Value.Count > 0))
                Screen.DeleteSelectedObjects();

            ImGui.Separator();

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_SelectAll"), "CTRL + A", false))
                Screen.SelectAllObjects();

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_SelectAllInLayer"), $"CTRL + ALT + A", false))
                Screen.SelectAllObjectsInLayer();

            ImGui.Separator();

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_FlipObjects"), "CTRL + H", false, Screen.SelectedHitObjects.Value.Count > 0))
                Screen.FlipSelectedObjects();

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_SwapLanesOfObjects"), Screen.SelectedHitObjects.Value.Count > 0))
            {
                for (var i = 1; i <= Screen.WorkingMap.GetKeyCount(); i++)
                {
                    if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_Lane", i)))
                    {
                        for (var j = 1; j <= Screen.WorkingMap.GetKeyCount(); j++)
                        {
                            if (i == j) continue;
                            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Lane", j), $"ALT + {i} + {j}"))
                            {
                                Screen.SwapSelectedObjects(i, j);
                            }
                        }

                        ImGui.EndMenu();
                    }
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_MoveObjectsToLayer"), Screen.SelectedHitObjects.Value.Count > 0))
            {
                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_DefaultLayer"), ""))
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

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_MoveObjectsToTimingGroup"), Screen.SelectedHitObjects.Value.Count > 0))
            {
                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_DefaultTimingGroup"), ""))
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

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_EditMetadata"), "F1"))
                DialogManager.Show(new EditorMetadataDialog(Screen));

            var timingPointPlugin = Screen.BuiltInPlugins[EditorBuiltInPlugin.TimingPointEditor];

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_EditTimingPoints"), "F5", timingPointPlugin.IsActive))
            {
                timingPointPlugin.IsActive = !timingPointPlugin.IsActive;

                if (timingPointPlugin.IsActive)
                    timingPointPlugin.Initialize();
            }

            var scrollVelocityPlugin = Screen.BuiltInPlugins[EditorBuiltInPlugin.ScrollVelocityEditor];

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_EditScrollVelocities"), "F6", scrollVelocityPlugin.IsActive))
            {
                scrollVelocityPlugin.IsActive = !scrollVelocityPlugin.IsActive;

                if (scrollVelocityPlugin.IsActive)
                    scrollVelocityPlugin.Initialize();
            }

            var scrollSpeedFactorPlugin = Screen.BuiltInPlugins[EditorBuiltInPlugin.ScrollSpeedFactorEditor];

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_EditScrollSpeedFactors"), "F7", scrollSpeedFactorPlugin.IsActive))
            {
                scrollSpeedFactorPlugin.IsActive = !scrollSpeedFactorPlugin.IsActive;

                if (scrollSpeedFactorPlugin.IsActive)
                    scrollSpeedFactorPlugin.Initialize();
            }

            ImGui.Separator();

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_AddBackgroundImage")))
                NotificationManager.Show(NotificationLevel.Info,
                    LocalizationManager.Get("Screen_Editor_AddBackgroundImageInstruction"));

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_SetSongSelectPreviewTime")))
                Screen.ActionManager.SetPreviewTime((int) Screen.Track.Time);

            ImGui.Separator();

            var view = Screen.View as EditScreenView;

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_EnableAutoMod"), "", view?.AutoMod.IsActive.Value ?? false))
            {
                if (view != null)
                    view.AutoMod.IsActive.Value = !view.AutoMod.IsActive.Value;
            }

            ImGui.Separator();

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_ApplyOffsetToMap")))
                DialogManager.Show(new EditorApplyOffsetDialog(Screen));

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_ResnapAllNotes")))
            {
                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_ResnapToCurrentSnap", Screen.BeatSnap.Value)))
                    Screen.ActionManager.ResnapNotes(new List<int> { Screen.BeatSnap.Value }, Screen.WorkingMap.HitObjects);
                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_ResnapToCommonSnaps")))
                    Screen.ActionManager.ResnapNotes(new List<int> { 16, 12 }, Screen.WorkingMap.HitObjects);
                if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_ResnapToCustomDivisions")))
                {
                    if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Resnap")))
                    {
                        if (EditScreen.CustomSnapDivisions.Count == 0)
                            NotificationManager.Show(NotificationLevel.Warning,
                                LocalizationManager.Get("Screen_Editor_NoSnapDivisionsSelected"));
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

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_ResnapSelectedNotes")))
            {
                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_ResnapToCurrentSnap", Screen.BeatSnap.Value)))
                    Screen.ActionManager.ResnapNotes(new List<int> { Screen.BeatSnap.Value }, Screen.SelectedHitObjects.Value);
                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_ResnapToCommonSnaps")))
                    Screen.ActionManager.ResnapNotes(new List<int> { 16, 12 }, Screen.SelectedHitObjects.Value);
                if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_ResnapToCustomDivisions")))
                {
                    if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Resnap")))
                    {
                        if (EditScreen.CustomSnapDivisions.Count == 0)
                            NotificationManager.Show(NotificationLevel.Warning,
                                LocalizationManager.Get("Screen_Editor_NoSnapDivisionsSelected"));
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

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_ApplyModifierToMap"), Screen.Map.Game == MapGame.Quaver))
            {
                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Mirror")))
                    DialogManager.Show(new ApplyModifiersToMapDialog(Screen, ModIdentifier.Mirror));

                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_NoLongNotes")))
                    DialogManager.Show(new ApplyModifiersToMapDialog(Screen, ModIdentifier.NoLongNotes));

                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_FullLongNotes")))
                    DialogManager.Show(new ApplyModifiersToMapDialog(Screen, ModIdentifier.FullLN));

                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Inverse")))
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

            if (!ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_View")))
                return;

            if (Screen.View is EditScreenView view && ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_Panels")))
            {
                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Details"), "", view.Details.Visible))
                    view.Details.SetVisibility(!view.Details.Visible);

                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_CompositionTools"), "", view.CompositionTools.Visible))
                    view.CompositionTools.SetVisibility(!view.CompositionTools.Visible);

                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Layers"), "", view.Layers.Visible))
                    view.Layers.SetVisibility(!view.Layers.Visible);

                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Hitsounds"), "", view.Hitsounds.Visible))
                    view.Hitsounds.SetVisibility(!view.Hitsounds.Visible);

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_DisplayGameplayPreview"), "", Screen.DisplayGameplayPreview.Value))
                Screen.DisplayGameplayPreview.Value = !Screen.DisplayGameplayPreview.Value;

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_ReferenceDifficulty")))
            {
                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_None"), "", Screen.UneditableMap.Value == null))
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
            
            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_EditorNoteSkin")))
            {
                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_CurrentSkin"), "", ConfigManager.EditorNoteSkin.Value == null))
                {
                    Transitioner.FadeIn();
                    ConfigManager.EditorNoteSkin.Value = null;
                    SkinManager.TimeEditorSkinReloadRequested = GameBase.Game.TimeRunning;
                }

                foreach (var skinName in SkinStore.GetSkins().Where(skinName => ImGui.MenuItem(skinName, "",
                             ConfigManager.EditorNoteSkin.Value == skinName)))
                {
                    Transitioner.FadeIn();
                    ConfigManager.EditorNoteSkin.Value = skinName;
                    SkinManager.TimeEditorSkinReloadRequested = GameBase.Game.TimeRunning;
                }

                ImGui.EndMenu();
            }
            
            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_EditorDefaultSkin")))
            {
                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_CurrentDefaultSkin"), "", ConfigManager.DefaultEditorSkin.Value == null))
                {
                    Transitioner.FadeIn();
                    ConfigManager.DefaultEditorSkin.Value = null;
                    SkinManager.TimeEditorSkinReloadRequested = GameBase.Game.TimeRunning;
                }

                foreach (DefaultSkins defaultSkin in Enum.GetValues(typeof(DefaultSkins)))
                {
                    if (!ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_DefaultSkin_" + defaultSkin), "",
                            ConfigManager.DefaultEditorSkin.Value == defaultSkin))
                        continue;

                    Transitioner.FadeIn();
                    ConfigManager.DefaultEditorSkin.Value = defaultSkin;
                    SkinManager.TimeEditorSkinReloadRequested = GameBase.Game.TimeRunning;
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_BackgroundBrightness")))
            {
                for (var i = 0; i < 11; i++)
                {
                    var value = i * 10;

                    if (ImGui.MenuItem($"{value}%", "", Screen.BackgroundBrightness.Value == value))
                        Screen.BackgroundBrightness.Value = value;
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_PlayfieldOpacity")))
            {
                for (var i = 0; i <= 10; i++)
                {
                    var value = i * 10;

                    if (ImGui.MenuItem($"{value}%", "", ConfigManager.EditorPlayfieldAlpha.Value == value))
                    {
                        ConfigManager.EditorPlayfieldAlpha.Value = value;
                    }
                }
                
                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_BeatSnapDivisor")))
            {
                foreach (var snap in EditScreen.AvailableBeatSnaps)
                {
                    if (ImGui.MenuItem($"1/{StringHelper.AddOrdinal(snap)}", "", Screen.BeatSnap.Value == snap))
                        Screen.BeatSnap.Value = snap;
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_BeatSnapColor")))
            {
                foreach (EditorBeatSnapColor type in Enum.GetValues(typeof(EditorBeatSnapColor)))
                {
                    if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_BeatSnapColor_" + type), "",
                            Screen.BeatSnapColor.Value == type))
                        Screen.BeatSnapColor.Value = type;
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_ScaleScrollSpeed"), "", Screen.ScaleScrollSpeedWithRate.Value))
                Screen.ScaleScrollSpeedWithRate.Value = !Screen.ScaleScrollSpeedWithRate.Value;

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_PlaceObjectsOnNearestTick"), "", Screen.PlaceObjectsOnNearestTick.Value))
                Screen.PlaceObjectsOnNearestTick.Value = !Screen.PlaceObjectsOnNearestTick.Value;

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_PlaceObjectsWithTopRowNumbers"), "", Screen.LiveMapping.Value))
                Screen.LiveMapping.Value = !Screen.LiveMapping.Value;

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_SnapNotesWhenLiveMapping"), "", ConfigManager.EditorLiveMapSnap.Value))
                ConfigManager.EditorLiveMapSnap.Value = !ConfigManager.EditorLiveMapSnap.Value;

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_SetLiveMappingOffset")))
                DialogManager.Show(new EditorSetLiveMapOffsetDialog(Screen));

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_PlaceLongNotesWhenLiveMapping"), "", ConfigManager.EditorLiveMapLongNote.Value))
                ConfigManager.EditorLiveMapLongNote.Value = !ConfigManager.EditorLiveMapLongNote.Value;

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_SetLiveMappingLongNoteMinimumLength")))
                DialogManager.Show(new EditorSetLiveMapLongNoteThresholdDialog(Screen));

            ImGui.Separator();

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_Waveform")))
            {
                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Visible"), "", Screen.ShowWaveform.Value))
                {
                    if (!Screen.ShowWaveform.Value) Screen.ShowSpectrogram.Value = false;
                    Screen.ShowWaveform.Value = !Screen.ShowWaveform.Value;
                }

                if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_Brightness")))
                {
                    for (var i = 0; i < 11; i++)
                    {
                        var value = i * 10;

                        if (ImGui.MenuItem($"{value}%", "", Screen.WaveformBrightness.Value == value))
                            Screen.WaveformBrightness.Value = value;
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_AudioDirection")))
                {
                    foreach (EditorPlayfieldWaveformAudioDirection type in Enum.GetValues(typeof(EditorPlayfieldWaveformAudioDirection)))
                    {
                        if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_WaveformAudioDirection_" + type), "",
                                Screen.AudioDirection.Value == type))
                            Screen.AudioDirection.Value = type;
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_Filter")))
                {
                    foreach (EditorPlayfieldWaveformFilter type in Enum.GetValues(typeof(EditorPlayfieldWaveformFilter)))
                    {
                        if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_WaveformFilter_" + type), "",
                                Screen.WaveformFilter.Value == type))
                            Screen.WaveformFilter.Value = type;
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Color")))
                    DialogManager.Show(new EditorChangeWaveformColorDialog());

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_Spectrogram")))
            {
                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Visible"), "", Screen.ShowSpectrogram.Value))
                {
                    if (!Screen.ShowSpectrogram.Value) Screen.ShowWaveform.Value = false;
                    Screen.ShowSpectrogram.Value = !Screen.ShowSpectrogram.Value;
                }

                if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_Brightness")))
                {
                    for (var i = 0; i < 11; i++)
                    {
                        var value = i * 10;

                        if (ImGui.MenuItem($"{value}%", "", Screen.SpectrogramBrightness.Value == value))
                            Screen.SpectrogramBrightness.Value = value;
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_Precision")))
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

                if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_FftSize")))
                {
                    for (var size = 256; size <= 16384; size *= 2)
                    {
                        if (ImGui.MenuItem($"{size}", "", Screen.SpectrogramFftSize.Value == size))
                            Screen.SpectrogramFftSize.Value = size;
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_FrequencyScale")))
                {
                    foreach (var scale in Enum.GetValues<EditorPlayfieldSpectrogramFrequencyScale>())
                    {
                        if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_SpectrogramFrequencyScale_" + scale), "",
                                scale == ConfigManager.EditorSpectrogramFrequencyScale.Value))
                            ConfigManager.EditorSpectrogramFrequencyScale.Value = scale;
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_CutoffFactor")))
                {
                    for (var i = 0; i <= 10; i++)
                    {
                        var f = 0.2f + 0.02f * i;
                        if (ImGui.MenuItem($"{f:0.00}", "", Math.Abs(f - ConfigManager.EditorSpectrogramCutoffFactor.Value) < 0.01f))
                            ConfigManager.EditorSpectrogramCutoffFactor.Value = f;
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_IntensityFactor")))
                {
                    for (var i = 0; i < 10; i++)
                    {
                        var f = 0.5f * i + 5.0f;
                        if (ImGui.MenuItem($"{f}", "", Math.Abs(f - ConfigManager.EditorSpectrogramIntensityFactor.Value) < 0.01f))
                            ConfigManager.EditorSpectrogramIntensityFactor.Value = f;
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_MinimumFrequency")))
                {
                    for (var f = 0; f <= 1500; f += 125)
                    {
                        if (ImGui.MenuItem($"{f}", "", ConfigManager.EditorSpectrogramMinimumFrequency.Value == f,
                                ConfigManager.EditorSpectrogramMaximumFrequency.Value > f))
                            ConfigManager.EditorSpectrogramMinimumFrequency.Value = f;
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_MaximumFrequency")))
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

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_LongNoteOpacity")))
            {
                for (var i = 2; i < 10; i++)
                {
                    var val = (i + 1) * 10;

                    if (ImGui.MenuItem($"{val}%", "", Screen.LongNoteOpacity.Value == val))
                        Screen.LongNoteOpacity.Value = val;
                }

                ImGui.EndMenu();
            }

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_CenterObjects"), "", Screen.AnchorHitObjectsAtMidpoint.Value))
                Screen.AnchorHitObjectsAtMidpoint.Value = !Screen.AnchorHitObjectsAtMidpoint.Value;

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_ViewLayers"), "", Screen.ObjectColoring.Value == HitObjectColoring.Layer))
                Screen.ObjectColoring.Value = Screen.ObjectColoring.Value == HitObjectColoring.Layer
                    ? HitObjectColoring.None
                    : HitObjectColoring.Layer;

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_ViewTimingGroups"), "", Screen.ObjectColoring.Value == HitObjectColoring.TimingGroup))
                Screen.ObjectColoring.Value = Screen.ObjectColoring.Value == HitObjectColoring.TimingGroup
                    ? HitObjectColoring.None
                    : HitObjectColoring.TimingGroup;

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_ColorSvLinesByTimingGroup"), "", ConfigManager.EditorColorSvLineByTimingGroup.Value))
                ConfigManager.EditorColorSvLineByTimingGroup.Value = !ConfigManager.EditorColorSvLineByTimingGroup.Value;

            ImGui.Separator();

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_TestPlayModifiers"), ""))
                DialogManager.Show(new EditorModifierMenuDialog());

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreateToolsSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_Tools")))
                return;

            foreach (var plugin in Screen.BuiltInPlugins)
            {
                if (plugin.Key is EditorBuiltInPlugin.KeybindEditor)
                    continue;
                if (ImGui.MenuItem(plugin.Value.Name, "", plugin.Value.IsActive))
                {
                    plugin.Value.IsActive = !plugin.Value.IsActive;

                    if (plugin.Value.IsActive)
                        plugin.Value.Initialize();
                }
            }
            
            ImGui.Separator();

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_ResetWindowLayout")))
            {
                StructuredConfigManager.WindowStates.Value = new WindowStates();
                ((EditScreenView)Screen.View).SaveWindowLayoutOnExit = false;
                File.Delete("imgui.ini");
                NotificationManager.Show(NotificationLevel.Info,
                    LocalizationManager.Get("Screen_Editor_WindowLayoutResetOnNextOpen"));
            }

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreatePluginsSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_Plugins")))
                return;

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_Local")))
            {
                var totalPlugins = 0;

                foreach (var plugin in Screen.Plugins)
                {
                    if (plugin.IsBuiltIn || plugin.IsWorkshop)
                        continue;

                    if (ImGui.BeginMenu(plugin.Name))
                    {
                        if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Enabled"), plugin.Author, plugin.IsActive))
                        {
                            plugin.IsActive = !plugin.IsActive;

                            if (plugin.IsActive)
                                plugin.Initialize();
                        }

                        Tooltip(plugin.Description);

                        if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_UploadToWorkshop")))
                        {
                            var item = new SteamWorkshopItem(plugin.Name, $"{WobbleGame.WorkingDirectory}Plugins/{plugin.Directory}");

                            if (!item.HasUploaded && (SteamWorkshopItem.Current == null || SteamWorkshopItem.Current.HasUploaded))
                            {
                                NotificationManager.Show(NotificationLevel.Info,
                                    LocalizationManager.Get("Screen_Editor_UploadingPluginToWorkshop"));

                                ThreadScheduler.Run(() =>
                                {
                                    item.Upload();

                                    while (!item.HasUploaded)
                                        Thread.Sleep(50);

                                    NotificationManager.Show(NotificationLevel.Success,
                                        LocalizationManager.Get("Screen_Editor_PluginUploadedToWorkshop"));
                                });
                            }
                        }

                        if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_OpenFolder")))
                            Utils.NativeUtils.OpenNatively($"{WobbleGame.WorkingDirectory}Plugins/{plugin.Directory}");

                        ImGui.EndMenu();
                    }

                    totalPlugins++;
                }

                if (totalPlugins == 0)
                {
                    if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_NoPluginsInstalled"), "", false, false))
                    {
                    }
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_SteamWorkshop")))
            {
                foreach (var plugin in Screen.Plugins)
                {
                    if (!plugin.IsWorkshop)
                        continue;

                    if (!ImGui.BeginMenu(plugin.Name))
                        continue;

                    if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Enabled"), plugin.Author, plugin.IsActive))
                    {
                        plugin.IsActive = !plugin.IsActive;

                        if (plugin.IsActive)
                            plugin.Initialize();
                    }

                    Tooltip(plugin.Description);

                    if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_OpenFolder")))
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

            if (!ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_Help")))
                return;

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Wiki")))
                BrowserHelper.OpenURL($"https://wiki.quavergame.com/docs/editor");

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_GettingStarted")))
                BrowserHelper.OpenURL($"https://wiki.quavergame.com/docs/editor/getting_started");

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_EditorFunctions")))
                BrowserHelper.OpenURL("https://wiki.quavergame.com/docs/editor/editor_functions");

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Plugins")))
                BrowserHelper.OpenURL("https://wiki.quavergame.com/docs/editor/plugins");

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_RankingCriteria")))
                BrowserHelper.OpenURL("https://wiki.quavergame.com/docs/ranking/criteria");

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_RankingProcess")))
                BrowserHelper.OpenURL("https://wiki.quavergame.com/docs/ranking/process");

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreateWebSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_Web")))
                return;

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_ViewOnlineListing"), Screen.WorkingMap.MapId != -1))
                BrowserHelper.OpenURL($"https://quavergame.com/mapsets/map/{Screen.WorkingMap.MapId}");

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_ModdingDiscussion"), Screen.WorkingMap.MapId != -1))
                BrowserHelper.OpenURL($"https://quavergame.com/mapsets/map/{Screen.WorkingMap.MapId}/mods");

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreateAudioSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_Audio")))
                return;

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_PlaybackSpeed")))
            {
                for (var i = 0; i < 8; i++)
                {
                    var value = (i + 1) * 0.25f;

                    if (ImGui.MenuItem($"{value * 100}%", "", Math.Abs(Screen.Track.Rate - value) < 0.001))
                        Screen.Track.Rate = value;
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_Hitsounds")))
            {
                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Enable"), "", Screen.EnableHitsounds.Value))
                    Screen.EnableHitsounds.Value = !Screen.EnableHitsounds.Value;

                if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_Volume")))
                {
                    if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_DefaultPercentage", (int) AudioSample.GlobalVolume), "", Screen.HitsoundVolume.Value == -1))
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

            if (ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_Metronome")))
            {
                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_Enable"), "", Screen.EnableMetronome.Value))
                    Screen.EnableMetronome.Value = !Screen.EnableMetronome.Value;

                if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_PlayHalfBeats"), "", Screen.MetronomePlayHalfBeats.Value))
                    Screen.MetronomePlayHalfBeats.Value = !Screen.MetronomePlayHalfBeats.Value;

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreateKeybindsSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu(LocalizationManager.Get("Screen_Editor_Keybinds")))
                return;

            var keybindEditor = Screen.BuiltInPlugins[EditorBuiltInPlugin.KeybindEditor];
            if (ImGui.MenuItem(keybindEditor.Name, "", keybindEditor.IsActive))
            {
                keybindEditor.IsActive = !keybindEditor.IsActive;

                if (keybindEditor.IsActive)
                    keybindEditor.Initialize();
            }

            ImGui.Separator();

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_InvertBeatSnapScroll"), "", Screen.InvertBeatSnapScroll.Value))
                Screen.InvertBeatSnapScroll.Value = !Screen.InvertBeatSnapScroll.Value;

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_InvertScrolling"), "", ConfigManager.InvertEditorScrolling.Value))
                ConfigManager.InvertEditorScrolling.Value = !ConfigManager.InvertEditorScrolling.Value;

            ImGui.Separator();

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_FillMissingActions")))
            {
                var filledCount = Screen.InputManager.InputConfig.FillMissingKeys(true);
                Screen.InputManager.InputConfig.SaveToConfig();
                Screen.ResetInputManager();
                NotificationManager.Show(NotificationLevel.Info,
                    LocalizationManager.Get("Screen_Editor_FilledMissingActions", filledCount));
            }

            if (ImGui.MenuItem(LocalizationManager.Get("Screen_Editor_ResetAllKeybinds")))
            {
                DialogManager.Show(new YesNoDialog(LocalizationManager.Get("Screen_Editor_ResetAllKeybindsTitle"),
                    LocalizationManager.Get("Screen_Editor_ResetAllKeybindsConfirmation"),
                    () =>
                    {
                        Screen.InputManager.InputConfig.ResetConfigFile();
                        Screen.ResetInputManager();
                        NotificationManager.Show(NotificationLevel.Info,
                            LocalizationManager.Get("Screen_Editor_AllKeybindsReset"));
                    }));
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
    }
}
