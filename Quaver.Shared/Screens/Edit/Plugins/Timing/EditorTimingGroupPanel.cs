using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Force.DeepCloner;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.Create;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.Remove;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.Rename;
using Quaver.Shared.Screens.Edit.Dialogs;
using Wobble;
using Wobble.Graphics.ImGUI;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace Quaver.Shared.Screens.Edit.Plugins.Timing
{
    public class EditorTimingGroupPanel : SpriteImGui, IEditorPlugin
    {
        private float _progress;

        /// <summary>
        /// </summary>
        private EditScreen Screen { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public string Name { get; } = "Timing Group Editor";

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public string Author { get; } = "WilliamQiufeng";

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public string Description { get; set; } = "";

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool IsBuiltIn { get; set; } = true;

        public string Directory { get; set; }

        public bool IsWorkshop { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool IsActive { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool IsWindowHovered { get; private set; }

        /// <summary>
        /// </summary>
        private HashSet<string> SelectedTimingGroups { get; } = new();

        /// <summary>
        /// </summary>
        private HashSet<string> Clipboard { get; } = new();

        private bool _lastNameEditError;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public EditorTimingGroupPanel(EditScreen screen) : base(false, GetOptions(), screen.ImGuiScale)
        {
            Screen = screen;
            Initialize();
            Screen.ActionManager.TimingGroupRenamed += ActionManagerOnTimingGroupRenamed;
            Screen.ActionManager.TimingGroupDeleted += ActionManagerOnTimingGroupDeleted;
        }

        private void ActionManagerOnTimingGroupDeleted(object sender, EditorTimingGroupRemovedEventArgs e)
        {
            SelectedTimingGroups.Remove(e.Id);
            Clipboard.Remove(e.Id);
        }

        private void ActionManagerOnTimingGroupRenamed(object sender, EditorTimingGroupRenamedEventArgs e)
        {
            if (SelectedTimingGroups.Contains(e.OldId))
            {
                SelectedTimingGroups.Remove(e.OldId);
                SelectedTimingGroups.Add(e.NewId);
            }

            if (Clipboard.Contains(e.OldId))
            {
                Clipboard.Remove(e.OldId);
                Clipboard.Add(e.NewId);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Initialize()
        {
            SelectedTimingGroups.Clear();

            SelectedTimingGroups.Add(Qua.DefaultScrollGroupId);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void RenderImguiLayout()
        {
            ImGui.SetNextWindowSizeConstraints(new Vector2(356, 0), new Vector2(600, float.MaxValue));
            ImGui.PushFont(Options.Fonts.First().Context);
            ImGui.Begin(Name);

            DrawHeaderText();
            ImGui.Dummy(new Vector2(0, 10));

            DrawAddButton();
            ImGui.SameLine();
            DrawRemoveButton();
            ImGui.SameLine();
            DrawSelectNotesButton();

            if (SelectedTimingGroups.All(x =>
                    Screen.WorkingMap.TimingGroups.TryGetValue(x, out var v) && v is ScrollGroup))
                DrawScrollGroupOptions();

            ImGui.Dummy(new Vector2(0, 10));

            DrawNameInput();

            var isHovered = ImGui.IsWindowHovered() || ImGui.IsAnyItemFocused();

            ImGui.Dummy(new Vector2(0, 10));
            DrawSelectedCountLabel();

            ImGui.Dummy(new Vector2(0, 10));
            DrawTable();

            IsWindowHovered = IsWindowHovered || isHovered;

            ImGui.End();
        }

        private void DrawSelectNotesButton()
        {
            if (ImGui.Button($"Select Notes"))
            {
                if (KeyboardManager.IsCtrlDown() || KeyboardManager.IsShiftDown())
                {
                    Screen.SelectedHitObjects.AddRange(Screen.WorkingMap.HitObjects
                        .Where(note => SelectedTimingGroups.Contains(note.TimingGroup)
                                       && !Screen.SelectedHitObjects.Value.Contains(note)).ToList());
                }
                else
                {
                    Screen.SelectedHitObjects.Clear();
                    Screen.SelectedHitObjects.AddRange(Screen.WorkingMap.HitObjects
                        .Where(note => SelectedTimingGroups.Contains(note.TimingGroup)).ToList());
                }
            }
        }

        /// <summary>
        /// </summary>
        private void DrawHeaderText()
        {
            ImGui.TextWrapped(
                "Timing Groups group together a set of notes. Each group has their own visual behavior applied to the notes.");
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.TextWrapped(
                "You can click on an individual timing group to edit it and double-click to go to the first note in the group.");
        }

        /// <summary>
        /// </summary>
        private void DrawAddButton()
        {
            if (ImGui.Button("Add"))
            {
                var newGroupId = EditorPluginUtils.GenerateTimingGroupId();

                var rgb = new byte[3];
                Random.Shared.NextBytes(rgb);

                var timingGroup = new ScrollGroup
                {
                    ScrollVelocities =
                        new List<SliderVelocityInfo> { new() { Multiplier = 1, StartTime = 0 } },
                    ColorRgb = $"{rgb[0]},{rgb[1]},{rgb[2]}"
                };

                Screen.ActionManager.CreateTimingGroup(newGroupId, timingGroup, Screen.SelectedHitObjects.Value);
                SelectedTimingGroups.Clear();
                SelectedTimingGroups.Add(newGroupId);

                ImGui.SetKeyboardFocusHere(3); // Focus third input after the button, which is the multiplier
            }
        }

        /// <summary>
        /// </summary>
        private void DrawRemoveButton()
        {
            ImGui.BeginDisabled(SelectedTimingGroups.Count == 0 ||
                                SelectedTimingGroups.Contains(Qua.DefaultScrollGroupId) ||
                                SelectedTimingGroups.Contains(Qua.GlobalScrollGroupId));
            if (ImGui.Button("Remove"))
            {
                Screen.ActionManager.PerformBatch(SelectedTimingGroups.Select(
                        IEditorAction (id) =>
                            new EditorActionRemoveTimingGroup(Screen.ActionManager, Screen.WorkingMap, id,
                                Screen.WorkingMap.TimingGroups[id], null))
                    .ToList());

                SelectedTimingGroups.Clear();
            }

            ImGui.EndDisabled();
        }

        private void DrawScrollGroupOptions()
        {
            if (SelectedTimingGroups.Count == 1)
            {
                if (ImGui.Button("Edit in SV Editor"))
                {
                    var svPanel =
                        (EditorScrollVelocityPanel)Screen.BuiltInPlugins[EditorBuiltInPlugin.ScrollVelocityEditor];
                    svPanel.SelectTimingGroup(SelectedTimingGroups.First());
                    svPanel.IsActive = true;
                }
            }
        }

        private void DrawNameInput()
        {
            if (SelectedTimingGroups.Count != 1)
                return;

            ImGui.TextWrapped("Name");

            if (_lastNameEditError)
            {
                ImGui.TextColored(new Vector4(255, 0, 0, 255), "Invalid name!");
            }

            var id = SelectedTimingGroups.First();
            var newId = id;
            var hint = "Alphanumeric characters or underscore are allowed";
            var inputTextFlags = ImGuiInputTextFlags.EnterReturnsTrue;

            if (id is Qua.DefaultScrollGroupId or Qua.GlobalScrollGroupId)
            {
                inputTextFlags |= ImGuiInputTextFlags.ReadOnly;
                hint = "This timing group cannot be renamed!";
            }

            ImGui.BeginDisabled(id is Qua.DefaultScrollGroupId or Qua.GlobalScrollGroupId);
            if (ImGui.InputTextWithHint("##Name", hint, ref newId, 256,
                    inputTextFlags))
            {
                _lastNameEditError = !Screen.ActionManager.RenameTimingGroup(id, newId);
            }

            ImGui.EndDisabled();
        }

        private void DrawColorEdit(string id, TimingGroup timingGroup, ImGuiColorEditFlags colorOptions, string prefix)
        {
            var color = timingGroup.GetColor();
            var colorVec3 = new Vector4(color.R, color.G, color.B, 255) / 256;

            if (ImGui.ColorButton($"##{prefix}", colorVec3, colorOptions))
            {
                if (id != Qua.DefaultScrollGroupId)
                    DialogManager.Show(new EditorChangeTimingGroupColorDialog(id, timingGroup, Screen.ActionManager));
            }

            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                if (id != Qua.DefaultScrollGroupId)
                {
                    var rng = Random.Shared;
                    var col = new Color(rng.Next(255), rng.Next(255), rng.Next(255));
                    Screen.ActionManager.ChangeTimingGroupColor(id, col);
                }
                    
            }
        }

        /// <summary>
        /// </summary>
        private void DrawSelectedCountLabel()
        {
            var count = SelectedTimingGroups.Count;
            var labelText = count > 1 ? $"{count} timing groups selected" : "";
            ImGui.Text(labelText);
        }

        /// <summary>
        /// </summary>
        private void DrawTable()
        {
            DrawTableHeader();
            DrawTableColumns();
        }

        /// <summary>
        /// </summary>
        private void DrawTableHeader()
        {
            ImGui.Columns(3);
            ImGui.SetColumnWidth(0, 160);
            ImGui.TextWrapped("ID");
            ImGui.NextColumn();
            ImGui.TextWrapped("Type");
            ImGui.NextColumn();
            ImGui.TextWrapped("Color");
            ImGui.Separator();
            ImGui.Columns();
        }

        /// <summary>
        /// </summary>
        private void DrawTableColumns()
        {
            ImGui.BeginChild("Timing Group Area");
            ImGui.Columns(3);
            ImGui.SetColumnWidth(0, 160);

            const int elementBaseHeight = 12;
            const int numberOfColumns = 3;
            var elementHeight = Screen.ImGuiScale * elementBaseHeight;
            var y = ImGui.GetContentRegionAvail().Y;

            var start = Math.Min(
                (int)(_progress * Screen.WorkingMap.TimingGroups.Count - 1),
                Screen.WorkingMap.TimingGroups.Count - (int)(y / elementHeight)
            );

            for (var j = 0; j < numberOfColumns; j++)
            {
                ImGui.Dummy(new(0, start * elementHeight));
                ImGui.NextColumn();
            }

            var end = Math.Min((int)(y / elementHeight) + start + 1, Screen.WorkingMap.TimingGroups.Count);

            foreach (var (id, timingGroup) in Screen.WorkingMap.TimingGroups)
            {
                // https://github.com/ocornut/imgui/blob/master/docs/FAQ.md#q-why-is-my-widget-not-reacting-when-i-click-on-it
                // allows all SVs with same truncated time to be selected, instead of just the first in list
                ImGui.PushID(id);

                var isSelected = SelectedTimingGroups.Contains(id);

                if (!isSelected)
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(100, 100, 100, 0));

                if (ImGui.Button(id == Qua.DefaultScrollGroupId ? "$Default" : id))
                {
                    // User holds down control, so add/remove it from the currently list of selected points
                    if (KeyboardManager.IsCtrlDown())
                    {
                        if (isSelected)
                            SelectedTimingGroups.Remove(id);
                        else
                            SelectedTimingGroups.Add(id);
                    }
                    else
                    {
                        if (isSelected)
                        {
                            var hitObjectInfo = Screen.WorkingMap.GetTimingGroupChildren(id).FirstOrDefault();
                            if (hitObjectInfo != null)
                                Screen.Track.Seek(hitObjectInfo.StartTime);
                        }

                        SelectedTimingGroups.Clear();
                        SelectedTimingGroups.Add(id);
                    }
                }

                if (!isSelected)
                    ImGui.PopStyleColor();

                ImGui.NextColumn();
                ImGui.TextWrapped($"{timingGroup.GetType().Name}");
                ImGui.NextColumn();
                const ImGuiColorEditFlags colorOptions = ImGuiColorEditFlags.Float | ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoPicker;
                DrawColorEdit(id, timingGroup, colorOptions, $"Column_{id}");
                ImGui.NextColumn();

                ImGui.PopID();
            }

            for (var j = 0; j < numberOfColumns; j++)
            {
                ImGui.Dummy(new(0, (Screen.WorkingMap.TimingGroups.Count - end) * elementHeight));
                ImGui.NextColumn();
            }

            _progress = ImGui.GetScrollY() / ImGui.GetScrollMaxY();

            if (float.IsNaN(_progress))
                _progress = 0;

            IsWindowHovered = ImGui.IsWindowHovered() || ImGui.IsAnyItemFocused();
            HandleInput();
            ImGui.Columns();
            ImGui.EndChild();
        }

        private void HandleInput()
        {
            if (!IsWindowHovered)
                return;

            if (KeyboardManager.IsCtrlDown())
            {
                // Select all
                if (KeyboardManager.IsUniqueKeyPress(Keys.A))
                {
                    SelectedTimingGroups.Clear();
                    SelectedTimingGroups.UnionWith(Screen.WorkingMap.TimingGroups.Keys);
                }
                // Deselect
                else if (KeyboardManager.IsUniqueKeyPress(Keys.D))
                {
                    SelectedTimingGroups.Clear();
                }
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Delete))
            {
                if (SelectedTimingGroups.Count != 0)
                {
                    Screen.ActionManager.PerformBatch(SelectedTimingGroups.Select(
                            IEditorAction (id) =>
                                new EditorActionRemoveTimingGroup(Screen.ActionManager, Screen.WorkingMap, id,
                                    Screen.WorkingMap.TimingGroups[id], null))
                        .ToList());
                    SelectedTimingGroups.Clear();
                }
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.C))
                CopyToClipboard();

            if (KeyboardManager.IsUniqueKeyPress(Keys.V))
                PasteClipboard();
        }

        /// <summary>
        /// </summary>
        private void CopyToClipboard()
        {
            Clipboard.Clear();

            if (SelectedTimingGroups.Count != 0)
                Clipboard.UnionWith(SelectedTimingGroups);
        }

        /// <summary>
        /// </summary>
        private void PasteClipboard()
        {
            var clonedObjects = new Dictionary<string, TimingGroup>();

            foreach (var id in Clipboard)
            {
                if (!Screen.WorkingMap.TimingGroups.TryGetValue(id, out var point))
                    continue;

                var clone = point.DeepClone();
                var newIdIndex = 2;

                string newId;
                while (Screen.WorkingMap.TimingGroups.ContainsKey(newId = $"{id}_{newIdIndex}"))
                    newIdIndex++;

                clonedObjects.Add(newId, clone);
            }


            Screen.ActionManager.PerformBatch(clonedObjects.Select(
                    IEditorAction (pair) =>
                        new EditorActionCreateTimingGroup(Screen.ActionManager, Screen.WorkingMap, pair.Key,
                            pair.Value, null))
                .ToList());
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static ImGuiOptions GetOptions() => new(
            new List<ImGuiFont> { new($"{WobbleGame.WorkingDirectory}/Fonts/lato-black.ttf", 14), }, false);

        public override void Destroy()
        {
            Screen.ActionManager.TimingGroupRenamed -= ActionManagerOnTimingGroupRenamed;
            Screen.ActionManager.TimingGroupDeleted -= ActionManagerOnTimingGroupDeleted;
            base.Destroy();
        }
    }
}