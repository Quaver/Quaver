using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Timing.AddBatch;
using TagLib.Matroska;
using TagLib.Riff;
using Wobble;
using Wobble.Graphics.ImGUI;
using Wobble.Input;

namespace Quaver.Shared.Screens.Edit.Plugins.Timing
{
    public class EditorTimingPointPanel : SpriteImGui, IEditorPlugin
    {
        /// <summary>
        /// </summary>
        private EditScreen Screen { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public string Name { get; } = "Timing Point Editor";

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public string Author { get; } = "The Quaver Team";

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

        /// <summary>
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// </summary>
        public bool IsWindowHovered { get; set; }

        /// <summary>
        /// </summary>
        private List<TimingPointInfo> SelectedTimingPoints { get; } = new List<TimingPointInfo>();

        /// <summary>
        /// </summary>
        private List<TimingPointInfo> Clipboard { get; } = new List<TimingPointInfo>();

        /// <summary>
        ///     If the panel has to scroll to the correct position
        /// </summary>
        private bool NeedsToScrollToFirstSelectedPoint { get; set; }

        /// <summary>
        /// </summary>
        private bool NeedsToScrollToLastSelectedPoint { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public EditorTimingPointPanel(EditScreen screen) : base(false, GetOptions())
        {
            Screen = screen;
            Initialize();
        }

        /// <summary>
        /// </summary>
        public void Initialize()
        {
            SelectedTimingPoints.Clear();

            var point = Screen.WorkingMap.GetTimingPointAt(Screen.Track.Time);

            if (point != null)
            {
                SelectedTimingPoints.Add(point);

                if (point != Screen.WorkingMap.TimingPoints.First())
                    NeedsToScrollToFirstSelectedPoint = true;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void RenderImguiLayout()
        {
            ImGui.SetNextWindowSizeConstraints(new Vector2(450, 0), new Vector2(450, float.MaxValue));
            ImGui.PushFont(Options.Fonts.First().Context);
            ImGui.Begin(Name);

            DrawHeaderText();
            ImGui.Dummy(new Vector2(0, 10));

            DrawSelectCurrentTimingPointButton();
            ImGui.Dummy(new Vector2(0, 10));

            DrawAddButton();
            ImGui.SameLine();
            DrawRemoveButton();

            ImGui.Dummy(new Vector2(0, 10));

            if (SelectedTimingPoints.Count <= 1)
                DrawTimeTextbox();
            else
                DrawMoveOffsetByTextbox();

            ImGui.Dummy(new Vector2(0, 10));
            DrawBpmTextbox();

            ImGui.Dummy(new Vector2(0, 10));
            DrawSignatureTextbox();

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
            ImGui.TextWrapped("Timing Points are individual BPM sections within your map. This will allow you to place objects at the correct times in the song.");
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.TextWrapped("You can click on an individual Timing Point to edit it or double-click to go to its position in time.");
        }

        /// <summary>
        /// </summary>
        private void DrawAddButton()
        {
            if (ImGui.Button("Add"))
            {
                var currentPoint = Screen.WorkingMap.GetTimingPointAt(Screen.Track.Time);
                var bpm = currentPoint?.Bpm ?? 0;

                SelectedTimingPoints.Clear();

                var point = new TimingPointInfo
                {
                    StartTime = (float)Screen.Track.Time,
                    Bpm = bpm,
                    Signature = TimeSignature.Quadruple
                };

                Screen.ActionManager.PlaceTimingPoint(point);
                SelectedTimingPoints.Add(point);
                NeedsToScrollToFirstSelectedPoint = true;
                ImGui.SetKeyboardFocusHere(3);
            }
        }

        /// <summary>
        /// </summary>
        private void DrawRemoveButton()
        {
            if (ImGui.Button("Remove"))
            {
                if (SelectedTimingPoints.Count == 0)
                    return;

                var lastPoint = SelectedTimingPoints.Last();

                Screen.ActionManager.RemoveTimingPointBatch(new List<TimingPointInfo>(SelectedTimingPoints));

                var newPoint = Screen.WorkingMap.TimingPoints.FindLast(x => x.StartTime <= lastPoint.StartTime);

                SelectedTimingPoints.Clear();

                if (newPoint != null)
                {
                    if (!SelectedTimingPoints.Contains(newPoint))
                        SelectedTimingPoints.Add(newPoint);
                }
                else if (Screen.WorkingMap.TimingPoints.Count > 0)
                {
                    var point = Screen.WorkingMap.TimingPoints.First();

                    if (!SelectedTimingPoints.Contains(point))
                        SelectedTimingPoints.Add(point);
                }

                NeedsToScrollToFirstSelectedPoint = true;
            }
        }

        /// <summary>
        /// </summary>
        private void DrawSelectCurrentTimingPointButton()
        {
            if (ImGui.Button("Select current timing point"))
            {
                var currentPoint = Screen.WorkingMap.GetTimingPointAt(Screen.Track.Time);
                if (currentPoint != null)
                {
                    NeedsToScrollToLastSelectedPoint = true;

                    var newSelection = new List<TimingPointInfo> { currentPoint };

                    if (KeyboardManager.IsCtrlDown() || KeyboardManager.IsShiftDown())
                        newSelection.AddRange(SelectedTimingPoints);

                    if (KeyboardManager.IsShiftDown() && SelectedTimingPoints.Count > 0)
                    {
                        var sorted = SelectedTimingPoints.OrderBy(x => x.StartTime);
                        var min = sorted.First().StartTime;
                        var max = sorted.Last().StartTime;
                        if (currentPoint.StartTime < min)
                        {
                            var tpsInRange = Screen.WorkingMap.TimingPoints
                                .Where(tp => tp.StartTime >= currentPoint.StartTime && tp.StartTime <= min);
                            newSelection.AddRange(tpsInRange);
                        }
                        else if (currentPoint.StartTime > max)
                        {
                            var tpsInRange = Screen.WorkingMap.TimingPoints
                                .Where(tp => tp.StartTime >= max && tp.StartTime <= currentPoint.StartTime);
                            newSelection.AddRange(tpsInRange);
                        }
                    }

                    SelectedTimingPoints.Clear();
                    SelectedTimingPoints.AddRange(newSelection.Distinct());
                }
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 25);
                ImGui.Text("This will select the timing point at the current editor timestamp. If Ctrl is held, it will add it to your selection instead. If Shift is held, it will select all timing points up to that range, if one is selected already.");
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

            if (SelectedTimingPoints.Count == 1)
            {
                var point = SelectedTimingPoints.First();

                time = point.StartTime;
                format = $"{time}";
            }

            ImGui.TextWrapped("Time");

            if (ImGui.InputFloat("", ref time, 1, 0.1f, format, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll))
            {
                if (SelectedTimingPoints.Count == 1)
                    Screen.ActionManager.ChangeTimingPointOffset(SelectedTimingPoints.First(), time);
            }
        }

        private void DrawMoveOffsetByTextbox()
        {
            var time = 0f;
            var format = "";

            ImGui.TextWrapped("Move Times By");

            if (ImGui.InputFloat("   ", ref time, 1, 0.1f, format, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll))
                Screen.ActionManager.ChangeTimingPointOffsetBatch(SelectedTimingPoints, time);
        }

        /// <summary>
        /// </summary>
        private void DrawBpmTextbox()
        {
            var bpm = 0f;
            var format = "";

            if (SelectedTimingPoints.Count == 1)
            {
                var point = SelectedTimingPoints.First();

                bpm = point.Bpm;
                format = $"{bpm:0.00}";
            }
            // All points are the same bpm
            else if (SelectedTimingPoints.Count > 1 && SelectedTimingPoints.All(x => x.Bpm == SelectedTimingPoints.First().Bpm))
            {
                bpm = SelectedTimingPoints.First().Bpm;
                format = $"{bpm:0.00}";
            }

            ImGui.TextWrapped("BPM");

            if (ImGui.InputFloat("##bpm", ref bpm, 1, 0.1f, format, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll))
            {
                if (SelectedTimingPoints.Count == 1)
                    Screen.ActionManager.ChangeTimingPointBpm(SelectedTimingPoints.First(), bpm);
                else
                    Screen.ActionManager.ChangeTimingPointBpmBatch(SelectedTimingPoints, bpm);
            }
        }

        /// <summary>
        /// </summary>
        private void DrawSignatureTextbox()
        {
            var signature = 0;

            if (SelectedTimingPoints.Count == 1)
            {
                var point = SelectedTimingPoints.First();
                signature = (int)point.Signature;
            }
            // All points are the same signature
            else if (SelectedTimingPoints.Count > 1 && SelectedTimingPoints.All(x => x.Signature == SelectedTimingPoints.First().Signature))
            {
                signature = (int)SelectedTimingPoints.First().Signature;
            }

            ImGui.TextWrapped("Signature");

            if (ImGui.InputInt("##signature", ref signature, 1, 1, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll))
            {
                signature = Math.Max(signature, 1);

                if (SelectedTimingPoints.Count == 1)
                    Screen.ActionManager.ChangeTimingPointSignature(SelectedTimingPoints.First(), signature);
                else
                    Screen.ActionManager.ChangeTimingPointSignatureBatch(SelectedTimingPoints, signature);
            }
        }

        /// <summary>
        /// </summary>
        private void DrawSelectedCountLabel()
        {
            var count = SelectedTimingPoints.Count;
            var labelText = count > 1 ? $"{count} timing points selected" : "";
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
            ImGui.Columns(4);
            ImGui.SetColumnWidth(0, 160);
            ImGui.TextWrapped("Time");
            ImGui.NextColumn();
            ImGui.TextWrapped("BPM");
            ImGui.NextColumn();
            ImGui.TextWrapped("Signature");
            ImGui.NextColumn();
            ImGui.TextWrapped("Hide Lines");
            ImGui.Columns();
            ImGui.Separator();
        }

        /// <summary>
        /// </summary>
        private void DrawTableColumns()
        {
            ImGui.BeginChild("Timing Point Area");

            ImGui.Columns(4);
            ImGui.SetColumnWidth(0, 160);

            if (
                (NeedsToScrollToFirstSelectedPoint || NeedsToScrollToLastSelectedPoint)
                && SelectedTimingPoints.Count != 0
                && Screen.WorkingMap.TimingPoints.Count == 0)
            {
                ImGui.SetScrollHereY(-0.025f);
                NeedsToScrollToFirstSelectedPoint = false;
                NeedsToScrollToLastSelectedPoint = false;
            }

            for (int i = 0; i < Screen.WorkingMap.TimingPoints.Count; i++)
            {
                // https://github.com/ocornut/imgui/blob/master/docs/FAQ.md#q-why-is-my-widget-not-reacting-when-i-click-on-it
                // allows all timing points with same truncated time to be selected, instead of just the first in list
                ImGui.PushID(i);

                var point = Screen.WorkingMap.TimingPoints[i];

                var isSelected = SelectedTimingPoints.Contains(point);

                if (!isSelected)
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(100, 100, 100, 0));

                if (SelectedTimingPoints.Count != 0)
                {
                    // Last selected takes precedence over first selected, since it's initiated via a button press
                    if (NeedsToScrollToLastSelectedPoint && SelectedTimingPoints.Last() == point && !NeedsToScrollToFirstSelectedPoint)
                    {
                        ImGui.SetScrollHereY(-0.025f);
                        NeedsToScrollToLastSelectedPoint = false;
                    }
                    else if (NeedsToScrollToFirstSelectedPoint && SelectedTimingPoints.First() == point)
                    {
                        ImGui.SetScrollHereY(-0.025f);
                        NeedsToScrollToFirstSelectedPoint = false;
                    }

                }

                if (ImGui.Button($"{TimeSpan.FromMilliseconds(point.StartTime):mm\\:ss\\.fff}"))
                {
                    // User holds down control, so add/remove it from the currently list of selected points
                    if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) || KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl))
                    {
                        if (isSelected)
                            SelectedTimingPoints.Remove(point);
                        else
                        {
                            if (!SelectedTimingPoints.Contains(point))
                                SelectedTimingPoints.Add(point);
                        }
                    }
                    // User holds down shift, so range select if the clicked element is outside of the bounds of the currently selected points
                    else if ((KeyboardManager.CurrentState.IsKeyDown(Keys.LeftShift) || KeyboardManager.CurrentState.IsKeyDown(Keys.RightShift)) && SelectedTimingPoints.Count > 0)
                    {
                        var sorted = SelectedTimingPoints.OrderBy(tp => tp.StartTime);
                        var min = sorted.First().StartTime;
                        var max = sorted.Last().StartTime;
                        if (point.StartTime < min)
                        {
                            var pointsInRange = Screen.WorkingMap.TimingPoints
                                .Where(v => v.StartTime >= point.StartTime && v.StartTime < min);
                            SelectedTimingPoints.AddRange(pointsInRange);
                        }
                        else if (point.StartTime > max)
                        {
                            var pointsInRange = Screen.WorkingMap.TimingPoints
                                .Where(v => v.StartTime > max && v.StartTime <= point.StartTime);
                            SelectedTimingPoints.AddRange(pointsInRange);
                        }
                    }
                    else
                    {
                        if (isSelected)
                            Screen.Track.Seek(point.StartTime);

                        SelectedTimingPoints.Clear();
                        SelectedTimingPoints.Add(point);
                    }
                }

                if (!isSelected)
                    ImGui.PopStyleColor();

                ImGui.NextColumn();
                ImGui.TextWrapped($"{point.Bpm:0.00}");

                ImGui.NextColumn();
                ImGui.TextWrapped($"{(int)point.Signature}/4");

                ImGui.NextColumn();
                var hidden = point.Hidden;
                if (ImGui.Checkbox($"##{point.StartTime}", ref hidden))
                    Screen.ActionManager.ChangeTimingPointHidden(point, hidden);

                ImGui.NextColumn();
                ImGui.PopID();
            }

            IsWindowHovered = ImGui.IsWindowHovered() || ImGui.IsAnyItemFocused();
            HandleInput();
            ImGui.EndChild();
        }

        /// <summary>
        /// </summary>
        private void HandleInput()
        {
            if (!IsWindowHovered)
                return;

            if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) ||
                KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl))
            {
                // Select all
                if (KeyboardManager.IsUniqueKeyPress(Keys.A))
                {
                    SelectedTimingPoints.Clear();
                    SelectedTimingPoints.AddRange(Screen.WorkingMap.TimingPoints);
                }
                // Deselect
                else if (KeyboardManager.IsUniqueKeyPress(Keys.D))
                {
                    SelectedTimingPoints.Clear();
                }
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Delete))
            {
                if (SelectedTimingPoints.Count != 0)
                {
                    Screen.ActionManager.RemoveTimingPointBatch(new List<TimingPointInfo>(SelectedTimingPoints));
                    SelectedTimingPoints.Clear();
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
            Clipboard.AddRange(SelectedTimingPoints);
            Screen.ActionManager.RemoveTimingPointBatch(new List<TimingPointInfo>(SelectedTimingPoints));
            SelectedTimingPoints.Clear();
        }

        /// <summary>
        /// </summary>
        private void CopyToClipboard()
        {
            Clipboard.Clear();

            if (SelectedTimingPoints.Count != 0)
                Clipboard.AddRange(SelectedTimingPoints);
        }

        /// <summary>
        /// </summary>
        private void PasteClipboard()
        {
            var clonedObjects = new List<TimingPointInfo>();

            var pasteTime = Clipboard.Select(x => x.StartTime).Min();
            var difference = (int)Math.Round(Screen.Track.Time - pasteTime, MidpointRounding.AwayFromZero);

            foreach (var obj in Clipboard)
            {
                var point = new TimingPointInfo()
                {
                    StartTime = obj.StartTime + difference,
                    Bpm = obj.Bpm,
                    Signature = obj.Signature,
                    Hidden = obj.Hidden
                };

                clonedObjects.Add(point);
            }

            clonedObjects = clonedObjects.OrderBy(x => x.StartTime).ToList();

            Screen.ActionManager.PlaceTimingPointBatch(clonedObjects);
            SelectedTimingPoints.Clear();
            SelectedTimingPoints.AddRange(clonedObjects);
            NeedsToScrollToFirstSelectedPoint = true;
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
