using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;
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
        private bool NeedsToScroll { get; set; }

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
                    NeedsToScroll = true;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void RenderImguiLayout()
        {
            ImGui.SetNextWindowSize(new Vector2(356, 480));
            ImGui.PushFont(Options.Fonts.First().Context);
            ImGui.Begin(Name, ImGuiWindowFlags.NoResize);

            DrawHeaderText();

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

            var isHovered = ImGui.IsWindowHovered() || ImGui.IsAnyItemFocused();

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
                    StartTime = (float) Screen.Track.Time,
                    Bpm = bpm
                };

                Screen.ActionManager.PlaceTimingPoint(point);
                SelectedTimingPoints.Add(point);
                NeedsToScroll = true;
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

                NeedsToScroll = true;
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

            if (ImGui.InputFloat("", ref time, 1, 0.1f, format, ImGuiInputTextFlags.EnterReturnsTrue))
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

            if (ImGui.InputFloat("   ", ref time, 1, 0.1f, format, ImGuiInputTextFlags.EnterReturnsTrue))
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

            if (ImGui.InputFloat(" ", ref bpm, 1, 0.1f, format, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                if (SelectedTimingPoints.Count == 1)
                    Screen.ActionManager.ChangeTimingPointBpm(SelectedTimingPoints.First(), bpm);
                else
                    Screen.ActionManager.ChangeTimingPointBpmBatch(SelectedTimingPoints, bpm);
            }
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
            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, 160);
            ImGui.TextWrapped("Time");
            ImGui.NextColumn();
            ImGui.TextWrapped("BPM");
            ImGui.Columns();
            ImGui.Separator();
        }

        /// <summary>
        /// </summary>
        private void DrawTableColumns()
        {
            ImGui.BeginChild("Timing Point Area");

            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, 160);

            if (NeedsToScroll && SelectedTimingPoints.Count != 0  && Screen.WorkingMap.TimingPoints.Count == 0)
            {
                ImGui.SetScrollHereY(-0.05f);
                NeedsToScroll = false;
            }

            foreach (var point in Screen.WorkingMap.TimingPoints)
            {
                var isSelected = SelectedTimingPoints.Contains(point);

                if (!isSelected)
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(100, 100, 100, 0));

                if (NeedsToScroll && SelectedTimingPoints.Count != 0 && SelectedTimingPoints.First() == point)
                {
                    ImGui.SetScrollHereY(-0.05f);
                    NeedsToScroll = false;
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
                if (KeyboardManager.IsUniqueKeyPress(Keys.A))
                {
                    SelectedTimingPoints.Clear();
                    SelectedTimingPoints.AddRange(Screen.WorkingMap.TimingPoints);
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

            var difference = (int) Math.Round(Screen.Track.Time - Clipboard.First().StartTime, MidpointRounding.AwayFromZero);

            foreach (var obj in Clipboard)
            {
                var point = new TimingPointInfo()
                {
                    StartTime = obj.StartTime + difference,
                    Bpm = obj.Bpm
                };

                clonedObjects.Add(point);
            }

            clonedObjects = clonedObjects.OrderBy(x => x.StartTime).ToList();

            Screen.ActionManager.PlaceTimingPointBatch(clonedObjects);
            SelectedTimingPoints.Clear();
            SelectedTimingPoints.AddRange(clonedObjects);
            NeedsToScroll = true;
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