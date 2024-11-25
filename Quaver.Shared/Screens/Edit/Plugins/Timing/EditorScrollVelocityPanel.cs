using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.Rename;
using Wobble;
using Wobble.Graphics.ImGUI;
using Wobble.Input;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace Quaver.Shared.Screens.Edit.Plugins.Timing
{
    public class EditorScrollVelocityPanel : SpriteImGui, IEditorPlugin, IColoredImGuiTitle
    {
        private float _progress;

        /// <summary>
        /// </summary>
        private EditScreen Screen { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public string Name { get; } = "Scroll Velocity Editor";

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public string Author { get; } = "Swan";

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

        public Dictionary<string, EditorPluginStorageValue> Storage { get; set; } = new();

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
        private int? NeedsToScrollToFirstSelectedSv { get; set; }

        /// <summary>
        /// </summary>
        private int? NeedsToScrollToLastSelectedSv { get; set; }

        /// <summary>
        /// </summary>
        private List<SliderVelocityInfo> SelectedScrollVelocities { get; } = new List<SliderVelocityInfo>();

        /// <summary>
        /// </summary>
        private List<SliderVelocityInfo> Clipboard { get; } = new List<SliderVelocityInfo>();

        public string SelectedScrollGroupId
        {
            get => Screen.SelectedScrollGroupId;
            set => Screen.SelectedScrollGroupId = value;
        }

        /// <summary>
        ///     When not null, <see cref="SelectedScrollGroupId"/> will be set to this value next
        /// </summary>
        /// <seealso cref="SelectTimingGroup"/>
        private string PendingSelectScrollGroupId { get; set; }

        /// <summary>
        ///     ImGui saves the last tab selection, so we add this to prevent selection cycling.
        /// </summary>
        private bool SelectionCooldown { get; set; }

        /// <summary>
        ///     Keep track of the last selected id since draw.
        ///     This is used to detect if the selected group has been changed
        ///     in EditScreen
        /// </summary>
        private string LastSelectedScrollGroupId { get; set; } = Qua.DefaultScrollGroupId;

        public ScrollGroup SelectedScrollGroup => Screen.SelectedScrollGroup;

        public Color TitleColor => PaulToulColorGenerator.ColorScheme[13];

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public EditorScrollVelocityPanel(EditScreen screen) : base(false, GetOptions(), screen.ImGuiScale)
        {
            Screen = screen;
            Initialize();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Initialize()
        {
            SelectedScrollVelocities.Clear();

            var point = Screen.WorkingMap.GetScrollVelocityAt(Screen.Track.Time);

            if (point != null)
            {
                SelectedScrollVelocities.Add(point);

                if (point != SelectedScrollGroup.ScrollVelocities.First())
                {
                    NeedsToScrollToFirstSelectedSv = 0;
                }
            }
        }

        public void OnStorageLoaded()
        {
        }

        public void OnStorageSave()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void RenderImguiLayout()
        {
            ImGui.SetNextWindowSizeConstraints(new Vector2(356, 0), new Vector2(600, float.MaxValue));
            ImGui.PushFont(Options.Fonts.First().Context);
            ((IColoredImGuiTitle)this).ImGuiPushTitleColors();
            ImGui.Begin(Name);

            DrawHeaderText();
            ImGui.Dummy(new Vector2(0, 10));

            DrawTabBar();

            DrawSelectCurrentSVButton();
            ImGui.Dummy(new Vector2(0, 10));

            DrawAddButton();
            ImGui.SameLine();
            DrawRemoveButton();

            ImGui.Dummy(new Vector2(0, 10));

            if (SelectedScrollVelocities.Count <= 1)
                DrawTimeTextbox();
            else
                DrawMoveOffsetByTextbox();

            ImGui.Dummy(new Vector2(0, 10));
            DrawMultiplierTextbox();

            var isHovered = ImGui.IsWindowHovered() || ImGui.IsAnyItemFocused();

            ImGui.Dummy(new Vector2(0, 10));
            DrawSelectedCountLabel();

            DrawTable();

            IsWindowHovered = IsWindowHovered || isHovered;

            ImGui.End();
            ((IColoredImGuiTitle)this).ImGuiPopTitleColors();
        }


        private void DrawTabBar()
        {
            if (Screen.WorkingMap.TimingGroups == null)
                return;

            if (LastSelectedScrollGroupId != SelectedScrollGroupId)
                SelectionCooldown = true;

            if (ImGui.BeginTabBar("Groups", ImGuiTabBarFlags.FittingPolicyScroll))
            {
                PendingSelectScrollGroupId = SelectedScrollGroupId;
                foreach (var (id, timingGroup) in Screen.WorkingMap.TimingGroups)
                {
                    if (timingGroup is not ScrollGroup)
                        continue;

                    var flags = SelectedScrollGroupId == id
                        ? ImGuiTabItemFlags.SetSelected
                        : ImGuiTabItemFlags.None;

                    if (id is Qua.DefaultScrollGroupId or Qua.GlobalScrollGroupId)
                        flags |= ImGuiTabItemFlags.Leading;

                    if (ImGuiFix.BeginTabItem($"{id}##TabItem", ref Unsafe.NullRef<bool>(), flags))
                    {
                        if (PendingSelectScrollGroupId != id
                            && SelectedScrollGroupId != id
                            && !SelectionCooldown)
                            SelectTimingGroup(id);
                        ImGui.EndTabItem();
                    }
                }

                if (ImGui.TabItemButton("+##CreateGroup", ImGuiTabItemFlags.Trailing))
                {
                    var newGroupId = EditorPluginUtils.GenerateTimingGroupId();

                    var rgb = new byte[3];
                    Random.Shared.NextBytes(rgb);

                    Screen.ActionManager.CreateTimingGroup(newGroupId,
                        new ScrollGroup
                        {
                            InitialScrollVelocity = 1,
                            ScrollVelocities =
                                new List<SliderVelocityInfo> { new() { Multiplier = 1, StartTime = 0 } },
                            ColorRgb = $"{rgb[0]},{rgb[1]},{rgb[2]}"
                        },
                        Screen.SelectedHitObjects.Value);
                    SelectTimingGroup(newGroupId);
                }

                ImGui.EndTabBar();
            }

            if (SelectionCooldown)
                SelectionCooldown = false;

            if (PendingSelectScrollGroupId != null && PendingSelectScrollGroupId != SelectedScrollGroupId)
            {
                SelectedScrollGroupId = PendingSelectScrollGroupId;
                PendingSelectScrollGroupId = null;
                SelectionCooldown = true;
            }

            LastSelectedScrollGroupId = SelectedScrollGroupId;
        }

        public void SelectTimingGroup(string id)
        {
            SelectionCooldown = true;
            PendingSelectScrollGroupId = id;
            SelectedScrollVelocities.Clear();
        }

        /// <summary>
        /// </summary>
        private void DrawHeaderText()
        {
            ImGui.TextWrapped("Scroll Velocities (SV) allow you to dynamically change the speed and direction at which the objects fall.");
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.TextWrapped("You can click on an individual SV point to edit it and double-click to go to its position in time.");
            ShowDifferenceText();
        }

        private static void ShowDifferenceText()
        {
            ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1), "(Difference from SF)");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(300);
                ImGui.TextWrapped("SV will not move the notes but only change its speed, " +
                                  "whereas SF will directly change both their position and speed");
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }

        /// <summary>
        /// </summary>
        private void DrawAddButton()
        {
            if (ImGui.Button("Add"))
            {
                var currentPoint = SelectedScrollGroup.GetScrollVelocityAt(Screen.Track.Time);
                var multiplier = currentPoint?.Multiplier ?? 1;

                SelectedScrollVelocities.Clear();

                var sv = new SliderVelocityInfo() { StartTime = (float)Screen.Track.Time, Multiplier = multiplier };

                Screen.ActionManager.PlaceScrollVelocity(sv, SelectedScrollGroup);
                SelectedScrollVelocities.Add(sv);
                NeedsToScrollToFirstSelectedSv = SelectedScrollGroup.ScrollVelocities.IndexOf(sv);
                ImGui.SetKeyboardFocusHere(3); // Focus third input after the button, which is the multiplier
            }
        }

        /// <summary>
        /// </summary>
        private void DrawRemoveButton()
        {
            if (ImGui.Button("Remove"))
            {
                if (SelectedScrollVelocities.Count == 0)
                    return;

                var lastPoint = SelectedScrollVelocities.Last();

                Screen.ActionManager.RemoveScrollVelocityBatch(new List<SliderVelocityInfo>(SelectedScrollVelocities),
                    SelectedScrollGroup);

                var newPoint = SelectedScrollGroup.ScrollVelocities.FindLast(x => x.StartTime <= lastPoint.StartTime);

                SelectedScrollVelocities.Clear();

                if (newPoint != null)
                {
                    if (!SelectedScrollVelocities.Contains(newPoint))
                        SelectedScrollVelocities.Add(newPoint);

                    NeedsToScrollToFirstSelectedSv = SelectedScrollGroup.ScrollVelocities.IndexAtTime(newPoint.StartTime);
                }
                else if (SelectedScrollGroup.ScrollVelocities.Count > 0)
                {
                    var point = SelectedScrollGroup.ScrollVelocities.First();

                    if (!SelectedScrollVelocities.Contains(point))
                        SelectedScrollVelocities.Add(point);

                    NeedsToScrollToFirstSelectedSv = 0;
                }
            }
        }

        /// <summary>
        /// </summary>
        private void DrawSelectCurrentSVButton()
        {
            if (ImGui.Button("Select current SV"))
            {
                var currentPointIndex = SelectedScrollGroup.ScrollVelocities.IndexAtTime((float)Screen.Track.Time);
                if (currentPointIndex >= 0)
                {
                    var currentPoint = SelectedScrollGroup.ScrollVelocities[currentPointIndex];
                    NeedsToScrollToLastSelectedSv = currentPointIndex;

                    var newSelection = new List<SliderVelocityInfo>() { currentPoint };

                    if (KeyboardManager.IsCtrlDown() || KeyboardManager.IsShiftDown())
                        newSelection.AddRange(SelectedScrollVelocities);

                    if (KeyboardManager.IsShiftDown() && SelectedScrollVelocities.Count > 0)
                    {
                        var sorted = SelectedScrollVelocities.OrderBy(x => x.StartTime);
                        var min = sorted.First().StartTime;
                        var max = sorted.Last().StartTime;
                        if (currentPoint.StartTime < min)
                        {
                            var svsInRange = SelectedScrollGroup.ScrollVelocities
                                .Where(v => v.StartTime >= currentPoint.StartTime && v.StartTime <= min);
                            newSelection.AddRange(svsInRange);
                        }
                        else if (currentPoint.StartTime > max)
                        {
                            var svsInRange = SelectedScrollGroup.ScrollVelocities
                                .Where(v => v.StartTime >= max && v.StartTime <= currentPoint.StartTime);
                            newSelection.AddRange(svsInRange);
                        }
                    }

                    SelectedScrollVelocities.Clear();
                    SelectedScrollVelocities.AddRange(newSelection.Distinct());
                }
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 25);
                ImGui.Text(
                    "This will select the SV at the current editor timestamp. If Ctrl is held, it will add it to your selection instead. If Shift is held, it will select all SVs up to that range, if one is selected already.");
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }

        /// <summary>
        /// </summary>
        private void DrawTimeTextbox()
        {
            var time = 0f;
            var format = "";

            if (SelectedScrollVelocities.Count == 1)
            {
                var point = SelectedScrollVelocities.First();

                time = point.StartTime;
                format = $"{time}";
            }

            ImGui.TextWrapped("Time");

            if (ImGui.InputFloat("", ref time, 1, 0.1f, format,
                    ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll))
            {
                if (SelectedScrollVelocities.Count == 1)
                {
                    var sv = SelectedScrollVelocities.First();

                    Screen.ActionManager.ChangeScrollVelocityOffsetBatch(new List<SliderVelocityInfo> { sv },
                        time - sv.StartTime);
                }
            }
        }

        /// <summary>
        /// </summary>
        private void DrawMoveOffsetByTextbox()
        {
            var time = 0f;
            var format = "";

            ImGui.TextWrapped("Move Times By");

            if (ImGui.InputFloat("   ", ref time, 1, 0.1f, format, ImGuiInputTextFlags.EnterReturnsTrue))
                Screen.ActionManager.ChangeScrollVelocityOffsetBatch(
                    new List<SliderVelocityInfo>(SelectedScrollVelocities), time);
        }

        /// <summary>
        /// </summary>
        private void DrawMultiplierTextbox()
        {
            var multiplier = 0f;
            var format = "";

            if (SelectedScrollVelocities.Count == 1)
            {
                var point = SelectedScrollVelocities.First();

                multiplier = point.Multiplier;
                format = $"{multiplier:0.00}";
            }
            // All points are the same bpm
            else if (SelectedScrollVelocities.Count > 1 &&
                     SelectedScrollVelocities.All(x => x.Multiplier == SelectedScrollVelocities.First().Multiplier))
            {
                multiplier = SelectedScrollVelocities.First().Multiplier;
                format = $"{multiplier:0.00}";
            }

            ImGui.TextWrapped("Multiplier");

            if (ImGui.InputFloat(" ", ref multiplier, 1, 0.1f, format,
                    ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll))
                Screen.ActionManager.ChangeScrollVelocityMultiplierBatch(
                    new List<SliderVelocityInfo>(SelectedScrollVelocities), multiplier);
        }

        /// <summary>
        /// </summary>
        private void DrawSelectedCountLabel()
        {
            var count = SelectedScrollVelocities.Count;
            var labelText = count > 1 ? $"{count} scroll velocities selected" : "";
            ImGui.Text(labelText);
        }

        /// <summary>
        /// </summary>
        private unsafe void DrawTable()
        {
            if (!ImGui.BeginTable("##SVTable", 2, ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchSame))
            {
                IsWindowHovered = ImGui.IsWindowHovered() || ImGui.IsAnyItemFocused();
                return;
            }

            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn("Time");
            ImGui.TableSetupColumn("Multiplier");
            ImGui.TableHeadersRow();
            if ((NeedsToScrollToFirstSelectedSv.HasValue || NeedsToScrollToLastSelectedSv.HasValue) &&
                SelectedScrollVelocities.Count != 0 &&
                Screen.WorkingMap.TimingPoints.Count == 0)
            {
                ImGui.SetScrollHereY(-0.025f);
                NeedsToScrollToFirstSelectedSv = null;
                NeedsToScrollToLastSelectedSv = null;
            }

            var clipperRaw = new ImGuiListClipper();
            var clipper = new ImGuiListClipperPtr(&clipperRaw);
            clipper.Begin(SelectedScrollGroup.ScrollVelocities.Count);

            if (NeedsToScrollToLastSelectedSv.HasValue)
            {
                clipper.IncludeItemByIndex(NeedsToScrollToLastSelectedSv.Value);
            }

            if (NeedsToScrollToFirstSelectedSv.HasValue)
            {
                clipper.IncludeItemByIndex(NeedsToScrollToFirstSelectedSv.Value);
            }

            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    // https://github.com/ocornut/imgui/blob/master/docs/FAQ.md#q-why-is-my-widget-not-reacting-when-i-click-on-it
                    // allows all SVs with same truncated time to be selected, instead of just the first in list
                    ImGui.PushID(i);

                    var sv = SelectedScrollGroup.ScrollVelocities[i];

                    var isSelected = SelectedScrollVelocities.Contains(sv);

                    if (!isSelected)
                        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(100, 100, 100, 0));

                    if (SelectedScrollVelocities.Count != 0)
                    {
                        // Last selected takes precedence over first selected, since it's initiated via a button press
                        if (NeedsToScrollToLastSelectedSv.HasValue &&
                            SelectedScrollVelocities[^1] == sv &&
                            !NeedsToScrollToFirstSelectedSv.HasValue)
                        {
                            ImGui.SetScrollHereY(-0.025f);
                            NeedsToScrollToLastSelectedSv = null;
                        }
                        else if (NeedsToScrollToFirstSelectedSv.HasValue && SelectedScrollVelocities[0] == sv)
                        {
                            ImGui.SetScrollHereY(-0.025f);
                            NeedsToScrollToFirstSelectedSv = null;
                        }
                    }

                    if (ImGui.Button($@"{TimeSpan.FromMilliseconds(sv.StartTime):mm\:ss\.fff}"))
                    {
                        // User holds down control, so add/remove it from the currently list of selected points
                        if (KeyboardManager.IsCtrlDown())
                        {
                            if (isSelected)
                                SelectedScrollVelocities.Remove(sv);
                            else if (!SelectedScrollVelocities.Contains(sv))
                                SelectedScrollVelocities.Add(sv);
                        }
                        // User holds down shift, so range select if the clicked element is outside of the bounds of the currently selected points
                        else if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftShift) ||
                                 KeyboardManager.CurrentState.IsKeyDown(Keys.RightShift))
                        {
                            var min = SelectedScrollVelocities.MinBy(s => s.StartTime).StartTime;
                            var max = SelectedScrollVelocities.MaxBy(s => s.StartTime).StartTime;

                            if (sv.StartTime < min)
                            {
                                var svsInRange = SelectedScrollGroup.ScrollVelocities
                                    .Where(v => v.StartTime >= sv.StartTime && v.StartTime < min);

                                SelectedScrollVelocities.AddRange(svsInRange);
                            }
                            else if (sv.StartTime > max)
                            {
                                var svsInRange = SelectedScrollGroup.ScrollVelocities
                                    .Where(v => v.StartTime > max && v.StartTime <= sv.StartTime);

                                SelectedScrollVelocities.AddRange(svsInRange);
                            }
                        }
                        else
                        {
                            if (isSelected)
                                Screen.Track.Seek(sv.StartTime);

                            SelectedScrollVelocities.Clear();
                            SelectedScrollVelocities.Add(sv);
                        }
                    }

                    if (!isSelected)
                        ImGui.PopStyleColor();

                    ImGui.TableNextColumn();
                    ImGui.TextWrapped($"{sv.Multiplier:0.00}x");

                    ImGui.PopID();
                }
            }

            _progress = ImGui.GetScrollY() / ImGui.GetScrollMaxY();

            if (float.IsNaN(_progress))
                _progress = 0;

            IsWindowHovered = ImGui.IsWindowHovered() || ImGui.IsAnyItemFocused();
            HandleInput();
            ImGui.EndTable();
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
                    SelectedScrollVelocities.Clear();
                    SelectedScrollVelocities.AddRange(SelectedScrollGroup.ScrollVelocities);
                }
                // Deselect
                else if (KeyboardManager.IsUniqueKeyPress(Keys.D))
                {
                    SelectedScrollVelocities.Clear();
                }
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Delete))
            {
                if (SelectedScrollVelocities.Count != 0)
                {
                    Screen.ActionManager.RemoveScrollVelocityBatch(
                        new List<SliderVelocityInfo>(SelectedScrollVelocities), SelectedScrollGroup);
                    SelectedScrollVelocities.Clear();
                }
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.X))
                CutClipboard();

            if (KeyboardManager.IsUniqueKeyPress(Keys.C))
                CopyToClipboard();

            if (KeyboardManager.IsUniqueKeyPress(Keys.V))
                PasteClipboard();
        }

        /// <summary>
        /// </summary>
        private void CutClipboard()
        {
            Clipboard.Clear();
            Clipboard.AddRange(SelectedScrollVelocities);
            Screen.ActionManager.RemoveScrollVelocityBatch(new List<SliderVelocityInfo>(SelectedScrollVelocities),
                SelectedScrollGroup);
            SelectedScrollVelocities.Clear();
        }

        /// <summary>
        /// </summary>
        private void CopyToClipboard()
        {
            Clipboard.Clear();

            if (SelectedScrollVelocities.Count != 0)
                Clipboard.AddRange(SelectedScrollVelocities);
        }

        /// <summary>
        /// </summary>
        private void PasteClipboard()
        {
            if (Clipboard.Count == 0)
                return;

            var clonedObjects = new List<SliderVelocityInfo>();

            var pasteTime = Clipboard.Select(x => x.StartTime).Min();
            var difference = (int)Math.Round(Screen.Track.Time - pasteTime, MidpointRounding.AwayFromZero);

            foreach (var obj in Clipboard)
            {
                var point = new SliderVelocityInfo()
                {
                    StartTime = obj.StartTime + difference, Multiplier = obj.Multiplier
                };

                clonedObjects.Add(point);
            }

            clonedObjects = clonedObjects.OrderBy(x => x.StartTime).ToList();

            Screen.ActionManager.PlaceScrollVelocityBatch(clonedObjects, SelectedScrollGroup);
            SelectedScrollVelocities.Clear();
            SelectedScrollVelocities.AddRange(clonedObjects);
            NeedsToScrollToFirstSelectedSv = SelectedScrollGroup.ScrollVelocities.IndexOf(clonedObjects[0]);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static ImGuiOptions GetOptions() => new ImGuiOptions(
            new List<ImGuiFont> { new ImGuiFont($@"{WobbleGame.WorkingDirectory}/Fonts/lato-black.ttf", 14), }, false);
    }
}