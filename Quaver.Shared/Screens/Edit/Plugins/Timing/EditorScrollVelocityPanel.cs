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
    public class EditorScrollVelocityPanel : SpriteImGui, IEditorPlugin
    {
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
        private bool NeedsToScroll { get; set; }

        /// <summary>
        /// </summary>
        private List<SliderVelocityInfo> SelectedScrollVelocities { get; } = new List<SliderVelocityInfo>();

        /// <summary>
        /// </summary>
        private List<SliderVelocityInfo> Clipboard { get; } = new List<SliderVelocityInfo>();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public EditorScrollVelocityPanel(EditScreen screen) : base(false, GetOptions())
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

                if (point != Screen.WorkingMap.SliderVelocities.First())
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

            if (SelectedScrollVelocities.Count <= 1)
                DrawTimeTextbox();
            else
                DrawMoveOffsetByTextbox();

            ImGui.Dummy(new Vector2(0, 10));
            DrawMultiplierTextbox();

            ImGui.Dummy(new Vector2(0, 10));

            DrawTable();

            ImGui.End();
        }

        /// <summary>
        /// </summary>
        private void DrawHeaderText()
        {
            ImGui.TextWrapped("Scroll Velocities (SV) allow you to dynamically change the speed and direction at which the objects fall.");
            ImGui.Dummy(new Vector2(0, 10));
            ImGui.TextWrapped("You can click on an individual SV point to edit it and double-click to go to its position in time.");
        }

        /// <summary>
        /// </summary>
        private void DrawAddButton()
        {
            if (ImGui.Button("Add"))
            {
                var currentPoint = Screen.WorkingMap.GetScrollVelocityAt(Screen.Track.Time);
                var multiplier = currentPoint?.Multiplier ?? 1;

                SelectedScrollVelocities.Clear();

                var sv = new SliderVelocityInfo()
                {
                    StartTime = (float) Screen.Track.Time,
                    Multiplier = multiplier
                };

                Screen.ActionManager.PlaceScrollVelocity(sv);
                SelectedScrollVelocities.Add(sv);
                NeedsToScroll = true;
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

                Screen.ActionManager.RemoveScrollVelocityBatch(new List<SliderVelocityInfo>(SelectedScrollVelocities));

                var newPoint = Screen.WorkingMap.SliderVelocities.FindLast(x => x.StartTime <= lastPoint.StartTime);

                SelectedScrollVelocities.Clear();

                if (newPoint != null)
                {
                    if (!SelectedScrollVelocities.Contains(newPoint))
                        SelectedScrollVelocities.Add(newPoint);
                }
                else if (Screen.WorkingMap.SliderVelocities.Count > 0)
                {
                    var point = Screen.WorkingMap.SliderVelocities.First();

                    if (!SelectedScrollVelocities.Contains(point))
                        SelectedScrollVelocities.Add(point);
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

            if (SelectedScrollVelocities.Count == 1)
            {
                var point = SelectedScrollVelocities.First();

                time = point.StartTime;
                format = $"{time}";
            }

            ImGui.TextWrapped("Time");

            if (ImGui.InputFloat("", ref time, 1, 0.1f, format, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                if (SelectedScrollVelocities.Count == 1)
                {
                    var sv = SelectedScrollVelocities.First();

                    Screen.ActionManager.ChangeScrollVelocityOffsetBatch(new List<SliderVelocityInfo> {sv},
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
                Screen.ActionManager.ChangeScrollVelocityOffsetBatch(new List<SliderVelocityInfo>(SelectedScrollVelocities), time);
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
            else if (SelectedScrollVelocities.Count > 1 && SelectedScrollVelocities.All(x => x.Multiplier == SelectedScrollVelocities.First().Multiplier))
            {
                multiplier = SelectedScrollVelocities.First().Multiplier;
                format = $"{multiplier:0.00}";
            }

            ImGui.TextWrapped("Multiplier");

            if (ImGui.InputFloat(" ", ref multiplier, 1, 0.1f, format, ImGuiInputTextFlags.EnterReturnsTrue))
                Screen.ActionManager.ChangeScrollVelocityMultiplierBatch(new List<SliderVelocityInfo>(SelectedScrollVelocities), multiplier);
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
            ImGui.TextWrapped("Multiplier");
            ImGui.Columns();
            ImGui.Separator();
        }

        /// <summary>
        /// </summary>
        private void DrawTableColumns()
        {
            ImGui.BeginChild("Scroll Velocity Area");
            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, 160);

            if (NeedsToScroll && SelectedScrollVelocities.Count != 0  && Screen.WorkingMap.TimingPoints.Count == 0)
            {
                ImGui.SetScrollHereY(-0.05f);
                NeedsToScroll = false;
            }

            foreach (var sv in Screen.WorkingMap.SliderVelocities)
            {
                var isSelected = SelectedScrollVelocities.Contains(sv);

                if (!isSelected)
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(100, 100, 100, 0));

                if (NeedsToScroll && SelectedScrollVelocities.Count != 0 && SelectedScrollVelocities.First() == sv)
                {
                    ImGui.SetScrollHereY(-0.05f);
                    NeedsToScroll = false;
                }

                if (ImGui.Button($"{TimeSpan.FromMilliseconds(sv.StartTime):mm\\:ss\\.fff}"))
                {
                    // User holds down control, so add/remove it from the currently list of selected points
                    if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) || KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl))
                    {
                        if (isSelected)
                            SelectedScrollVelocities.Remove(sv);
                        else
                        {
                            if (!SelectedScrollVelocities.Contains(sv))
                                SelectedScrollVelocities.Add(sv);
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

                ImGui.NextColumn();
                ImGui.TextWrapped($"{sv.Multiplier:0.00}x");
                ImGui.NextColumn();
            }

            IsWindowHovered = ImGui.IsWindowHovered() || ImGui.IsAnyItemFocused();
            HandleInput();
            ImGui.EndChild();
        }

        private void HandleInput()
        {
            if (!IsWindowHovered)
                return;

            if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) ||
                KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl))
            {
                if (KeyboardManager.IsUniqueKeyPress(Keys.A))
                {
                    SelectedScrollVelocities.Clear();
                    SelectedScrollVelocities.AddRange(Screen.WorkingMap.SliderVelocities);
                }
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Delete))
            {
                if (SelectedScrollVelocities.Count != 0)
                {
                    Screen.ActionManager.RemoveScrollVelocityBatch(new List<SliderVelocityInfo>(SelectedScrollVelocities));
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
            Screen.ActionManager.RemoveScrollVelocityBatch(new List<SliderVelocityInfo>(SelectedScrollVelocities));
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
            var clonedObjects = new List<SliderVelocityInfo>();

            var difference = (int) Math.Round(Screen.Track.Time - Clipboard.First().StartTime, MidpointRounding.AwayFromZero);

            foreach (var obj in Clipboard)
            {
                var point = new SliderVelocityInfo()
                {
                    StartTime = obj.StartTime + difference,
                    Multiplier = obj.Multiplier
                };

                clonedObjects.Add(point);
            }

            clonedObjects = clonedObjects.OrderBy(x => x.StartTime).ToList();

            Screen.ActionManager.PlaceScrollVelocityBatch(clonedObjects);
            SelectedScrollVelocities.Clear();
            SelectedScrollVelocities.AddRange(clonedObjects);
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