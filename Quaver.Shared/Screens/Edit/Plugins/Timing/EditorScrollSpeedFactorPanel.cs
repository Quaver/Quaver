using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Maps.Structures;
using Wobble;
using Wobble.Graphics.ImGUI;
using Wobble.Input;

namespace Quaver.Shared.Screens.Edit.Plugins.Timing
{
    public class EditorScrollSpeedFactorPanel : SpriteImGui, IEditorPlugin
    {
        /// <summary>
        /// </summary>
        private EditScreen Screen { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public string Name => "Scroll Speed Factor Editor";

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public string Author => "The Quaver Team";

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
        private bool NeedsToScrollToFirstSelectedSf { get; set; }

        /// <summary>
        /// </summary>
        private bool NeedsToScrollToLastSelectedSf { get; set; }

        /// <summary>
        /// </summary>
        private List<ScrollSpeedFactorInfo> SelectedScrollSpeedFactors { get; } = new();

        /// <summary>
        /// </summary>
        private List<ScrollSpeedFactorInfo> Clipboard { get; } = new();

        /// <summary>
        /// </summary>
        private bool[] _lanesToggle;

        /// <summary>
        /// </summary>
        private float _time;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public EditorScrollSpeedFactorPanel(EditScreen screen) : base(false, GetOptions(), screen.ImGuiScale)
        {
            Screen = screen;
            Initialize();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Initialize()
        {
            var keyCount = Screen.WorkingMap.GetKeyCount();
            _lanesToggle = new bool[keyCount];

            SelectedScrollSpeedFactors.Clear();

            var point = Screen.WorkingMap.GetScrollSpeedFactorAt(Screen.Track.Time);

            if (point != null)
            {
                SelectedScrollSpeedFactors.Add(point);

                if (point != Screen.WorkingMap.ScrollSpeedFactors.First())
                    NeedsToScrollToFirstSelectedSf = true;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void RenderImguiLayout()
        {
            ImGui.SetNextWindowSizeConstraints(new Vector2(356, 0), new Vector2(500, float.MaxValue));
            ImGui.PushFont(Options.Fonts.First().Context);
            ImGui.Begin(Name);

            DrawHeaderText();
            ImGui.Dummy(new Vector2(0, 10));

            DrawSelectCurrentSFButton();
            ImGui.Dummy(new Vector2(0, 10));

            DrawAddButton();
            ImGui.SameLine();
            DrawRemoveButton();

            ImGui.Dummy(new Vector2(0, 10));

            if (SelectedScrollSpeedFactors.Count <= 1)
                DrawTimeTextbox();
            else
                DrawMoveOffsetByTextbox();

            ImGui.Dummy(new Vector2(0, 10));
            DrawMultiplierTextbox();

            ImGui.Dummy(new Vector2(0, 10));
            DrawLaneMaskTextbox();

            var isHovered = ImGui.IsWindowHovered() || ImGui.IsAnyItemFocused();

            ImGui.Dummy(new Vector2(0, 10));
            DrawSelectedCountLabel();

            ImGui.Dummy(new Vector2(0, 10));
            DrawTable();

            IsWindowHovered = IsWindowHovered || isHovered;

            ImGui.End();
        }

        /// <summary>
        /// </summary>
        private void DrawHeaderText()
        {
            ImGui.TextWrapped(
                "Scroll Speed Factors (SF) allow you to dynamically change the speed and direction at which the objects fall.");
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.TextWrapped(
                "You can click on an individual SF point to edit it and double-click to go to its position in time.");
        }

        /// <summary>
        /// </summary>
        private void DrawAddButton()
        {
            if (ImGui.Button("Add"))
            {
                var currentPoint = Screen.WorkingMap.GetScrollSpeedFactorAt(Screen.Track.Time);
                var multiplier = currentPoint?.Factor ?? 1;

                SelectedScrollSpeedFactors.Clear();

                var sv = new ScrollSpeedFactorInfo()
                {
                    StartTime = (float)Screen.Track.Time,
                    Factor = multiplier
                };

                Screen.ActionManager.PlaceScrollSpeedFactor(sv);
                SelectedScrollSpeedFactors.Add(sv);
                NeedsToScrollToFirstSelectedSf = true;
                ImGui.SetKeyboardFocusHere(3); // Focus third input after the button, which is the multiplier
            }
        }

        /// <summary>
        /// </summary>
        private void DrawRemoveButton()
        {
            if (ImGui.Button("Remove"))
            {
                if (SelectedScrollSpeedFactors.Count == 0)
                    return;

                var lastPoint = SelectedScrollSpeedFactors.Last();

                Screen.ActionManager.RemoveScrollSpeedFactorBatch(
                    new List<ScrollSpeedFactorInfo>(SelectedScrollSpeedFactors));

                var newPoint = Screen.WorkingMap.ScrollSpeedFactors.FindLast(x => x.StartTime <= lastPoint.StartTime);

                SelectedScrollSpeedFactors.Clear();

                if (newPoint != null)
                {
                    if (!SelectedScrollSpeedFactors.Contains(newPoint))
                        SelectedScrollSpeedFactors.Add(newPoint);
                }
                else if (Screen.WorkingMap.ScrollSpeedFactors.Count > 0)
                {
                    var point = Screen.WorkingMap.ScrollSpeedFactors.First();

                    if (!SelectedScrollSpeedFactors.Contains(point))
                        SelectedScrollSpeedFactors.Add(point);
                }

                NeedsToScrollToFirstSelectedSf = true;
            }
        }

        /// <summary>
        /// </summary>
        private void DrawSelectCurrentSFButton()
        {
            if (ImGui.Button("Select current SF"))
            {
                var currentPoint = Screen.WorkingMap.GetScrollSpeedFactorAt(Screen.Track.Time);
                if (currentPoint != null)
                {
                    NeedsToScrollToLastSelectedSf = true;

                    var newSelection = new List<ScrollSpeedFactorInfo>() { currentPoint };

                    if (KeyboardManager.IsCtrlDown() || KeyboardManager.IsShiftDown())
                        newSelection.AddRange(SelectedScrollSpeedFactors);

                    if (KeyboardManager.IsShiftDown() && SelectedScrollSpeedFactors.Count > 0)
                    {
                        var sorted = SelectedScrollSpeedFactors.OrderBy(x => x.StartTime);
                        var min = sorted.First().StartTime;
                        var max = sorted.Last().StartTime;
                        if (currentPoint.StartTime < min)
                        {
                            var svsInRange = Screen.WorkingMap.ScrollSpeedFactors
                                .Where(v => v.StartTime >= currentPoint.StartTime && v.StartTime <= min);
                            newSelection.AddRange(svsInRange);
                        }
                        else if (currentPoint.StartTime > max)
                        {
                            var svsInRange = Screen.WorkingMap.ScrollSpeedFactors
                                .Where(v => v.StartTime >= max && v.StartTime <= currentPoint.StartTime);
                            newSelection.AddRange(svsInRange);
                        }
                    }

                    SelectedScrollSpeedFactors.Clear();
                    SelectedScrollSpeedFactors.AddRange(newSelection.Distinct());
                }
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 25);
                ImGui.Text(
                    "This will select the SF at the current editor timestamp. If Ctrl is held, it will add it to your selection instead. If Shift is held, it will select all SFs up to that range, if one is selected already.");
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }

        /// <summary>
        /// </summary>
        private void DrawTimeTextbox()
        {
            var format = "";

            if (SelectedScrollSpeedFactors.Count == 1)
            {
                var point = SelectedScrollSpeedFactors.First();

                _time = point.StartTime;
                format = $"{_time}";
            }

            ImGui.TextWrapped("Time");

            if (ImGui.InputFloat("", ref _time, 1, 0.1f, format,
                    ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll))
            {
                if (SelectedScrollSpeedFactors.Count == 1)
                {
                    var sv = SelectedScrollSpeedFactors.First();

                    Screen.ActionManager.ChangeScrollSpeedFactorOffsetBatch(new List<ScrollSpeedFactorInfo> { sv },
                        _time - sv.StartTime);
                }
            }
        }

        /// <summary>
        /// </summary>
        private void DrawLaneMaskTextbox()
        {
            var keyCount = Screen.WorkingMap.GetKeyCount();

            if (SelectedScrollSpeedFactors.Count == 1)
            {
                var point = SelectedScrollSpeedFactors.First();
                Array.Clear(_lanesToggle);

                foreach (var lane in point.GetLaneMaskLanes(keyCount))
                {
                    _lanesToggle[lane] = true;
                }
            }

            ImGui.TextWrapped("Lane Mask");

            for (var i = 0; i < keyCount; i++)
            {
                if (ImGui.Checkbox($"##LaneMask{i}", ref _lanesToggle[i]))
                {
                    if (SelectedScrollSpeedFactors.Count == 1)
                    {
                        var sv = SelectedScrollSpeedFactors.First();
                        var laneMask = -1;
                        for (var j = 0; j < keyCount; j++)
                        {
                            if (!_lanesToggle[j]) laneMask ^= 1 << j;
                        }

                        Screen.ActionManager.ChangeScrollSpeedFactorLaneMaskBatch(
                            new List<ScrollSpeedFactorInfo> { sv },
                            laneMask);
                    }
                }

                ImGui.SameLine();
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
                Screen.ActionManager.ChangeScrollSpeedFactorOffsetBatch(
                    new List<ScrollSpeedFactorInfo>(SelectedScrollSpeedFactors), time);
        }

        /// <summary>
        /// </summary>
        private void DrawMultiplierTextbox()
        {
            var multiplier = 0f;
            var format = "";

            if (SelectedScrollSpeedFactors.Count == 1)
            {
                var point = SelectedScrollSpeedFactors.First();

                multiplier = point.Factor;
                format = $"{multiplier:0.00}";
            }
            // All points are the same bpm
            else if (SelectedScrollSpeedFactors.Count > 1 &&
                     SelectedScrollSpeedFactors.All(x => x.Factor == SelectedScrollSpeedFactors.First().Factor))
            {
                multiplier = SelectedScrollSpeedFactors.First().Factor;
                format = $"{multiplier:0.00}";
            }

            ImGui.TextWrapped("Multiplier");

            if (ImGui.InputFloat(" ", ref multiplier, 1, 0.1f, format,
                    ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll))
                Screen.ActionManager.ChangeScrollSpeedFactorMultiplierBatch(
                    new List<ScrollSpeedFactorInfo>(SelectedScrollSpeedFactors), multiplier);
        }

        /// <summary>
        /// </summary>
        private void DrawSelectedCountLabel()
        {
            var count = SelectedScrollSpeedFactors.Count;
            var labelText = count > 1 ? $"{count} scroll velocities selected" : "";
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
            ImGui.TextWrapped("Time");
            ImGui.NextColumn();
            ImGui.TextWrapped("Multiplier");
            ImGui.NextColumn();
            ImGui.TextWrapped("Lane Mask");
            ImGui.Separator();
            ImGui.Columns();
        }

        /// <summary>
        /// </summary>
        private void DrawTableColumns()
        {
            ImGui.BeginChild("Scroll Speed Factor Area");
            ImGui.Columns(3);
            ImGui.SetColumnWidth(0, 160);

            if (
                (NeedsToScrollToFirstSelectedSf || NeedsToScrollToLastSelectedSf)
                && SelectedScrollSpeedFactors.Count != 0
                && Screen.WorkingMap.TimingPoints.Count == 0)
            {
                ImGui.SetScrollHereY(-0.025f);
                NeedsToScrollToFirstSelectedSf = false;
                NeedsToScrollToLastSelectedSf = false;
            }

            for (int i = 0; i < Screen.WorkingMap.ScrollSpeedFactors.Count; i++)
            {
                // https://github.com/ocornut/imgui/blob/master/docs/FAQ.md#q-why-is-my-widget-not-reacting-when-i-click-on-it
                // allows all SFs with same truncated time to be selected, instead of just the first in list
                ImGui.PushID(i);

                var sv = Screen.WorkingMap.ScrollSpeedFactors[i];

                var isSelected = SelectedScrollSpeedFactors.Contains(sv);

                if (!isSelected)
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(100, 100, 100, 0));

                if (SelectedScrollSpeedFactors.Count != 0)
                {
                    // Last selected takes precedence over first selected, since it's initiated via a button press
                    if (NeedsToScrollToLastSelectedSf && SelectedScrollSpeedFactors.Last() == sv &&
                        !NeedsToScrollToFirstSelectedSf)
                    {
                        ImGui.SetScrollHereY(-0.025f);
                        NeedsToScrollToLastSelectedSf = false;
                    }
                    else if (NeedsToScrollToFirstSelectedSf && SelectedScrollSpeedFactors.First() == sv)
                    {
                        ImGui.SetScrollHereY(-0.025f);
                        NeedsToScrollToFirstSelectedSf = false;
                    }
                }

                if (ImGui.Button($"{TimeSpan.FromMilliseconds(sv.StartTime):mm\\:ss\\.fff}"))
                {
                    // User holds down control, so add/remove it from the currently list of selected points
                    if (KeyboardManager.IsCtrlDown())
                    {
                        if (isSelected)
                            SelectedScrollSpeedFactors.Remove(sv);
                        else
                        {
                            if (!SelectedScrollSpeedFactors.Contains(sv))
                                SelectedScrollSpeedFactors.Add(sv);
                        }
                    }
                    // User holds down shift, so range select if the clicked element is outside of the bounds of the currently selected points
                    else if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftShift) ||
                             KeyboardManager.CurrentState.IsKeyDown(Keys.RightShift))
                    {
                        var sorted = SelectedScrollSpeedFactors.OrderBy(tp => tp.StartTime).ToList();
                        var min = sorted.First().StartTime;
                        var max = sorted.Last().StartTime;
                        if (sv.StartTime < min)
                        {
                            var svsInRange = Screen.WorkingMap.ScrollSpeedFactors
                                .Where(v => v.StartTime >= sv.StartTime && v.StartTime < min);
                            SelectedScrollSpeedFactors.AddRange(svsInRange);
                        }
                        else if (sv.StartTime > max)
                        {
                            var svsInRange = Screen.WorkingMap.ScrollSpeedFactors
                                .Where(v => v.StartTime > max && v.StartTime <= sv.StartTime);
                            SelectedScrollSpeedFactors.AddRange(svsInRange);
                        }
                    }
                    else
                    {
                        if (isSelected)
                            Screen.Track.Seek(sv.StartTime);

                        SelectedScrollSpeedFactors.Clear();
                        SelectedScrollSpeedFactors.Add(sv);
                    }
                }

                if (!isSelected)
                    ImGui.PopStyleColor();

                ImGui.NextColumn();
                ImGui.TextWrapped($"{sv.Factor:0.00}x");
                ImGui.NextColumn();
                ImGui.TextWrapped($"{sv.MaskRepresentation(Screen.WorkingMap.GetKeyCount())}");
                ImGui.NextColumn();

                ImGui.PopID();
            }

            IsWindowHovered = ImGui.IsWindowHovered() || ImGui.IsAnyItemFocused();
            HandleInput();
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
                    SelectedScrollSpeedFactors.Clear();
                    SelectedScrollSpeedFactors.AddRange(Screen.WorkingMap.ScrollSpeedFactors);
                }
                // Deselect
                else if (KeyboardManager.IsUniqueKeyPress(Keys.D))
                {
                    SelectedScrollSpeedFactors.Clear();
                }
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Delete))
            {
                if (SelectedScrollSpeedFactors.Count != 0)
                {
                    Screen.ActionManager.RemoveScrollSpeedFactorBatch(
                        new List<ScrollSpeedFactorInfo>(SelectedScrollSpeedFactors));
                    SelectedScrollSpeedFactors.Clear();
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
            Clipboard.AddRange(SelectedScrollSpeedFactors);
            Screen.ActionManager.RemoveScrollSpeedFactorBatch(
                new List<ScrollSpeedFactorInfo>(SelectedScrollSpeedFactors));
            SelectedScrollSpeedFactors.Clear();
        }

        /// <summary>
        /// </summary>
        private void CopyToClipboard()
        {
            Clipboard.Clear();

            if (SelectedScrollSpeedFactors.Count != 0)
                Clipboard.AddRange(SelectedScrollSpeedFactors);
        }

        /// <summary>
        /// </summary>
        private void PasteClipboard()
        {
            var clonedObjects = new List<ScrollSpeedFactorInfo>();

            var pasteTime = Clipboard.Select(x => x.StartTime).Min();
            var difference = (int)Math.Round(Screen.Track.Time - pasteTime, MidpointRounding.AwayFromZero);

            foreach (var obj in Clipboard)
            {
                var point = new ScrollSpeedFactorInfo()
                {
                    StartTime = obj.StartTime + difference,
                    Factor = obj.Factor
                };

                clonedObjects.Add(point);
            }

            clonedObjects = clonedObjects.OrderBy(x => x.StartTime).ToList();

            Screen.ActionManager.PlaceScrollSpeedFactorBatch(clonedObjects);
            SelectedScrollSpeedFactors.Clear();
            SelectedScrollSpeedFactors.AddRange(clonedObjects);
            NeedsToScrollToFirstSelectedSf = true;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static ImGuiOptions GetOptions() => new(new List<ImGuiFont>
        {
            new($@"{WobbleGame.WorkingDirectory}/Fonts/lato-black.ttf", 14),
        }, false);
    }
}