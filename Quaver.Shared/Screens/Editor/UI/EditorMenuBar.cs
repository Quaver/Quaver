using System;
using System.Numerics;
using ImGuiNET;
using Quaver.Server.Client;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Editor.UI.Graphing;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Quaver.Shared.Screens.Settings;
using Wobble.Graphics;
using Wobble.Graphics.ImGUI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Window;

namespace Quaver.Shared.Screens.Editor.UI
{
    public class EditorMenuBar : SpriteImGui
    {
        /// <summary>
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// </summary>
        private EditorScreen Screen { get; }

        /// <summary>
        /// </summary>
        public float Height { get; private set; }

        /// <summary>
        /// </summary>
        private float[] AudioRates = {0.25f, 0.50f, 0.75f, 1.0f, 1.25f, 1.50f, 1.75f, 2.0f};

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public EditorMenuBar(EditorScreen screen) => Screen = screen;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void RenderImguiLayout()
        {
            if (!ImGui.BeginMainMenuBar())
                return;

            Height = ImGui.GetWindowSize().Y;

            if (DialogManager.Dialogs.Count == 0 || Screen.Exiting)
            {
                CreateFileSection();
                CreateEditSection();
                CreateViewSection();
                CreateAudioSection();
                CreateHelpSection();
            }

            ImGui.EndMenuBar();
            Button.IsGloballyClickable = !ImGui.IsAnyItemHovered();
            IsActive = ImGui.IsAnyItemActive() || ImGui.IsAnyItemHovered() || ImGui.IsAnyItemFocused();
        }

        /// <summary>
        /// </summary>
        private void CreateFileSection()
        {
            if (!ImGui.BeginMenu("File"))
                return;

            if (ImGui.MenuItem("New"))
                Screen.CreateNewDifficulty();

            if (ImGui.MenuItem("Save", "CTRL+S"))
                Screen.Save();

            ImGui.Separator();

            if (ImGui.MenuItem("Upload", "CTRL+U"))
                Screen.UploadMapset();

            if (ImGui.MenuItem("Export", "CTRL+E"))
                EditorScreen.ExportToZip();

            ImGui.Separator();

            if (ImGui.MenuItem("Open Folder", "CTRL+W"))
                MapManager.Selected.Value.OpenFolder();

            if (ImGui.MenuItem("Open .qua file", "CTRL+Q"))
                MapManager.Selected.Value.OpenFile();

            ImGui.Separator();

            if (ImGui.MenuItem("Visit Online Page", "CTRL+T"))
                MapManager.Selected.Value.VisitMapsetPage();

            ImGui.Separator();

            if (ImGui.MenuItem("Exit", "ESC"))
                Screen.HandleKeyPressEscape();

            ImGui.EndMenu();
        }
        /// <summary>
        /// </summary>
        private void CreateEditSection()
        {
            if (!ImGui.BeginMenu("Edit"))
                return;

            if (Screen.Ruleset.ActionManager.UndoStack.Count == 0)
                ImGui.MenuItem("Undo", false);
            else if (ImGui.MenuItem("Undo", "CTRL+Z"))
                Screen.Ruleset.ActionManager.Undo();

            if (Screen.Ruleset.ActionManager.RedoStack.Count == 0)
                ImGui.MenuItem("Redo", false);
            else if (ImGui.MenuItem("Redo", "CTRL+Y"))
                Screen.Ruleset.ActionManager.Redo();

            ImGui.Separator();

            if (ImGui.MenuItem("Cut", "CTRL+X"))
            {
                var ruleset = Screen.Ruleset as EditorRulesetKeys;
                ruleset?.CutHitObjects();
            }

            if (ImGui.MenuItem("Copy", "CTRL+C"))
            {
                var ruleset = Screen.Ruleset as EditorRulesetKeys;
                ruleset?.CopySelectedHitObjects();
            }

            if (ImGui.MenuItem("Paste", "CTRL+V"))
            {
                var ruleset = Screen.Ruleset as EditorRulesetKeys;
                ruleset?.PasteHitObjects();
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Select All", "CTRL+A"))
            {
                var ruleset = Screen.Ruleset as EditorRulesetKeys;
                ruleset?.SelectAllHitObjects();
            }

            if (ImGui.MenuItem("Select Layer", "CTRL+ALT+A"))
            {
                var ruleset = Screen.Ruleset as EditorRulesetKeys;
                ruleset?.SelectAllLayerHitObjects();
            }

            ImGui.Separator();

            if (((EditorRulesetKeys)Screen.Ruleset).SelectedHitObjects.Count == 0)
                ImGui.MenuItem("Mirror Selected Objects", false);
            else if (ImGui.MenuItem("Mirror Selected Objects", "CTRL+H"))
            {
                var ruleset = Screen.Ruleset as EditorRulesetKeys;
                ruleset?.FlipHitObjectsHorizontally();
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Metadata", "F1"))
                Screen.OpenMetadataDialog();

            if (ImGui.MenuItem("Timing Points", "F2"))
                Screen.OpenTimingPointDialog();

            if (ImGui.MenuItem("Scroll Velocities", "F3"))
                Screen.OpenScrollVelocityDialog();

            ImGui.Separator();

            if (ImGui.MenuItem("Set Audio Preview Time", "F4"))
                Screen.ChangePreviewTime((int) AudioEngine.Track.Time);

            ImGui.Separator();

            if (ImGui.MenuItem("Go To Objects...", "F5"))
                Screen.OpenGoToDialog();

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreateViewSection()
        {
            if (!ImGui.BeginMenu("View"))
                return;

            if (ImGui.BeginMenu("Beat Snap"))
            {
                foreach (var snap in Screen.AvailableBeatSnaps)
                {
                    if (ImGui.MenuItem($"1/{StringHelper.AddOrdinal(snap)}", null, snap == Screen.BeatSnap.Value))
                        Screen.BeatSnap.Value = snap;
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Snap Line Colors"))
            {
                foreach (EditorBeatSnapColor color in Enum.GetValues(typeof(EditorBeatSnapColor)))
                {
                    if (ImGui.MenuItem(color.ToString(), null, ConfigManager.EditorBeatSnapColorType.Value == color))
                        ConfigManager.EditorBeatSnapColorType.Value = color;
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.BeginMenu("Visualization Graph"))
            {
                var ruleset = (EditorRulesetKeys) Screen.Ruleset;

                foreach (EditorVisualizationGraphType graph in Enum.GetValues(typeof(EditorVisualizationGraphType)))
                {
                    if (ImGui.MenuItem(graph.ToString(), null, ConfigManager.EditorVisualizationGraph.Value == graph))
                        ConfigManager.EditorVisualizationGraph.Value = graph;
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Show Lane Divider Lines", null, ConfigManager.EditorShowLaneDividerLines.Value))
                ConfigManager.EditorShowLaneDividerLines.Value = !ConfigManager.EditorShowLaneDividerLines.Value;

            if (ImGui.MenuItem("Only Show Measure Lines", null, ConfigManager.EditorOnlyShowMeasureLines.Value))
                ConfigManager.EditorOnlyShowMeasureLines.Value = !ConfigManager.EditorOnlyShowMeasureLines.Value;

            if (ImGui.MenuItem("Anchor Objects At Midpoint", null, ConfigManager.EditorHitObjectsMidpointAnchored.Value))
                ConfigManager.EditorHitObjectsMidpointAnchored.Value = !ConfigManager.EditorHitObjectsMidpointAnchored.Value;

           ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreateAudioSection()
        {
            if (!ImGui.BeginMenu("Audio"))
                return;

            if (ImGui.BeginMenu("Playback Speed"))
            {
                foreach (var rate in AudioRates)
                {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (ImGui.MenuItem($"{rate * 100}%", null, AudioEngine.Track.Rate == rate))
                        AudioEngine.Track.Rate = rate;
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Enable Metronome", null, ConfigManager.EditorPlayMetronome.Value))
                ConfigManager.EditorPlayMetronome.Value = !ConfigManager.EditorPlayMetronome.Value;

            if (ImGui.MenuItem("Play Metronome Half-Beats", null, ConfigManager.EditorMetronomePlayHalfBeats.Value))
                ConfigManager.EditorMetronomePlayHalfBeats.Value = !ConfigManager.EditorMetronomePlayHalfBeats.Value;

            ImGui.Separator();

            if (ImGui.MenuItem("Enable Hitsounds", null, ConfigManager.EditorEnableHitsounds.Value))
                ConfigManager.EditorEnableHitsounds.Value = !ConfigManager.EditorEnableHitsounds.Value;

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreateHelpSection()
        {
            if (!ImGui.BeginMenu("Help"))
                return;

            if (ImGui.MenuItem("Editor Guide"))
                BrowserHelper.OpenURL($"{OnlineClient.WEBSITE_URL}/wiki/Editor");

            if (ImGui.MenuItem("Settings", "CTRL+O"))
                DialogManager.Show(new SettingsDialog());

            ImGui.EndMenu();
        }
    }
}