using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Editor.Actions.Rulesets.Universal;
using Quaver.Shared.Screens.Editor.UI.Dialogs.SV;
using Wobble;
using Wobble.Graphics.Animations;
using Wobble.Graphics.ImGUI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Window;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.Timing
{
    public class EditorTimingChanger : SpriteImGui
    {
         /// <summary>
        /// </summary>
        public bool Shown { get; set; }

        /// <summary>
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private string TextTime = "";

        /// <summary>
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private string TextBpm = "";

        /// <summary>
        /// </summary>
        public EditorBpmTimingDialog Dialog { get; }

        /// <summary>
        /// </summary>
        public EditorBpmCalculator Calculator { get; }

        /// <summary>
        /// </summary>
        public Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        public List<TimingPointInfo> SelectedTimingPoints { get; } = new List<TimingPointInfo>();

        /// <summary>
        /// </summary>
        private bool NeedsToScroll { get; set; }

        /// <summary>
        /// </summary>
        private static Regex DigitsOnly { get; } = new Regex(@"[^\d]");

        /// <summary>
        /// </summary>
        private static Regex DecimalRegex { get; } = new Regex(@"/^\d*\.?\d*$/");

        /// <summary>
        /// </summary>
        private bool TextTimeFocusedInLastFrame { get; set; }

        /// <summary>
        /// </summary>
        private bool TextBpmFocusedInLastFrame { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        public EditorTimingChanger(Qua map)
        {
            Calculator = new EditorBpmCalculator();
            Dialog = new EditorBpmTimingDialog(this);
            WorkingMap = map;
            SelectClosestPoint();
        }

        /// <summary>
        /// </summary>
        private void SelectClosestPoint()
        {
            if (WorkingMap.TimingPoints.Count == 0)
                return;

            // Find the closest timing point so it can be selected upon opening.
            TimingPointInfo closestTimingPoint = null;
            var minDiff = double.MaxValue;

            WorkingMap.TimingPoints.ForEach(x =>
            {
                var diff = Math.Abs(AudioEngine.Track.Time - x.StartTime);

                if (!(diff < minDiff))
                    return;

                minDiff = diff;
                closestTimingPoint = x;
            });

            if (closestTimingPoint == null)
                return;

            SelectedTimingPoints.Clear();
            SelectedTimingPoints.Add(closestTimingPoint);
            NeedsToScroll = true;

            TextTime = closestTimingPoint.StartTime.ToString(CultureInfo.InvariantCulture);
            TextBpm = $"{closestTimingPoint.Bpm:0.00}";
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Select all input
            if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) ||
                KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl))
            {
                if (KeyboardManager.IsUniqueKeyPress(Keys.A))
                {
                    SelectedTimingPoints.Clear();
                    SelectedTimingPoints.AddRange(WorkingMap.TimingPoints);

                    if (SelectedTimingPoints.Count >= 1 || SelectedTimingPoints.Count == 0)
                    {
                        TextTime = "";
                        TextBpm = $"";
                    }
                    else
                    {
                        SetInputTextToFirstSelected();
                    }
                }
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void RenderImguiLayout()
        {
            SetWindowSize();

            ImGui.Begin("Timing Points", ImGuiWindowFlags.Modal | ImGuiWindowFlags.NoCollapse |
                                             ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.AlwaysAutoResize);

            AddHeaderText();
            HandleAddRemoveButtons();
            HandleTextboxes();

            AddTableHeaders();
            AddSvTable();

            ImGui.End();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static Vector2 GetSize()
        {
            var nativeSize = new Vector2(390, 500);

            var scale = WindowManager.DetermineDrawScaling();
            return new Vector2(nativeSize.X * scale.X, nativeSize.Y * scale.Y);
        }

        /// <summary>
        /// </summary>
        private static void SetWindowSize()
        {
            var size = GetSize();

            ImGui.SetNextWindowSize(size);
            ImGui.SetNextWindowPos(new Vector2(ConfigManager.WindowWidth.Value - size.X, ConfigManager.WindowHeight.Value/ 2f - size.Y / 2f));
        }

        /// <summary>
        /// </summary>
        private static void AddHeaderText()
        {
            ImGui.TextWrapped($"Timing Points are individual BPM sections within your map. This will allow you to place objects at the correct times " +
                              $"in the song.\n\n" +
                              $"You can click on an individual Timing Point to edit it or double-click it to go to its position in time.");

            ImGui.Dummy(new Vector2(0, 10));
        }

        /// <summary>
        /// </summary>
        private void HandleAddRemoveButtons()
        {
            if (ImGui.Button("Add"))
            {
                var tp = new TimingPointInfo()
                {
                    StartTime = (int) AudioEngine.Track.Time,
                    Bpm = 0
                };

                var game = GameBase.Game as QuaverGame;
                var screen = game?.CurrentScreen as EditorScreen;
                screen?.Ruleset.ActionManager.Perform(new EditorActionAddTimingPoint(WorkingMap, tp));

                SelectedTimingPoints.Clear();
                SelectedTimingPoints.Add(tp);
                NeedsToScroll = true;

                TextTime = tp.StartTime.ToString(CultureInfo.InvariantCulture);
                TextBpm = $"{tp.Bpm:0.00}";
            }

            ImGui.SameLine();

            if (ImGui.Button("Remove"))
            {
                if (SelectedTimingPoints.Count == 0)
                    return;

                var game = GameBase.Game as QuaverGame;
                var screen = game?.CurrentScreen as EditorScreen;

                var lastTp = SelectedTimingPoints.Last();

                screen?.Ruleset.ActionManager.Perform(new EditorActionRemoveTimingPoints(WorkingMap, new List<TimingPointInfo>(SelectedTimingPoints)));

                SelectedTimingPoints.Clear();

                if (WorkingMap.TimingPoints.Count != 0)
                {
                    var sv = WorkingMap.TimingPoints.FindLast(x => x.StartTime <= lastTp.StartTime);

                    if (sv != null)
                    {
                        TextTime = sv.StartTime.ToString(CultureInfo.InvariantCulture);
                        TextBpm = $"{sv.Bpm:0.00}";
                        SelectedTimingPoints.Add(sv);
                    }
                    else
                    {
                        TextTime = "";
                        TextBpm = $"";
                    }
                }
                else
                {
                    TextTime = "";
                    TextBpm = $"";
                }

                NeedsToScroll = true;
            }
        }

        /// <summary>
        /// </summary>
        private void HandleTextboxes()
        {
            ImGui.Dummy(new Vector2(0, 10));

            ImGui.Text("Time");

            if (ImGui.InputText("", ref TextTime, 100,
                SelectedTimingPoints.Count == 0 ? ImGuiInputTextFlags.ReadOnly : ImGuiInputTextFlags.CharsDecimal))
            {
                if (string.IsNullOrEmpty(TextTime) || string.IsNullOrWhiteSpace(TextTime))
                    TextTime = "0";

                TextTime = OnlyDigits(TextTime);
            }

            // User stopped typing in the time field, so it needs to be updated
            if (!ImGui.IsItemActive() && TextTimeFocusedInLastFrame)
                UpdateSelectedTimingPoints();

            TextTimeFocusedInLastFrame = ImGui.IsItemActive();

            ImGui.SameLine();

            if (SelectedTimingPoints.Count == 1)
            {
                var game = GameBase.Game as QuaverGame;
                var screen = game?.CurrentScreen as EditorScreen;

                if (ImGui.ArrowButton("1", ImGuiDir.Left))
                {
                    var tp = SelectedTimingPoints.First();

                    screen?.Ruleset.ActionManager.Perform(new EditorActionChangeTimingPoint(WorkingMap, new List<EditorTimingPointChangeInfo>
                    {
                        new EditorTimingPointChangeInfo(tp, tp.StartTime - 1, tp.Bpm)
                    }));

                    TextTime = $"{tp.StartTime:0.00}";
                }

                ImGui.SameLine();

                if (ImGui.ArrowButton("2", ImGuiDir.Right))
                {
                    var tp = SelectedTimingPoints.First();

                    screen?.Ruleset.ActionManager.Perform(new EditorActionChangeTimingPoint(WorkingMap, new List<EditorTimingPointChangeInfo>
                    {
                        new EditorTimingPointChangeInfo(tp, tp.StartTime + 1, tp.Bpm)
                    }));

                    TextTime = $"{tp.StartTime:0.00}";
                }
            }

            ImGui.Dummy(new Vector2(0, 10));

            ImGui.Text("BPM");

            if (ImGui.InputText(" ", ref TextBpm, 100,
                SelectedTimingPoints.Count == 0 ? ImGuiInputTextFlags.ReadOnly : ImGuiInputTextFlags.CharsDecimal))
            {
                if (string.IsNullOrEmpty(TextBpm) || string.IsNullOrWhiteSpace(TextBpm))
                    TextBpm = "0";

                TextBpm = Decimal(TextBpm);
            }

            // User stopped typing in multiplier frame
            if (!ImGui.IsItemActive() && TextBpmFocusedInLastFrame)
                UpdateSelectedTimingPoints();

            TextBpmFocusedInLastFrame = ImGui.IsItemActive();
        }

        /// <summary>
        /// </summary>
        private static void AddTableHeaders()
        {
            ImGui.Dummy(new Vector2(0, 10));

            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, 105);
            ImGui.Text("Time");
            ImGui.NextColumn();
            ImGui.Text("BPM");
            ImGui.Columns();
            ImGui.Separator();
        }

        /// <summary>
        /// </summary>
        private void AddSvTable()
        {
            ImGui.BeginChild("TP Area");

            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, 105);

            if (NeedsToScroll && SelectedTimingPoints.Count != 0  && WorkingMap.TimingPoints.Count == 0)
            {
                ImGui.SetScrollHereY(-0.05f);
                NeedsToScroll = false;
            }

            foreach (var tp in WorkingMap.TimingPoints)
            {
                var isSelected = SelectedTimingPoints.Contains(tp);

                if (!isSelected)
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(100, 100, 100, 0));

                if (NeedsToScroll && SelectedTimingPoints.Count != 0 && SelectedTimingPoints.First() == tp)
                {
                    ImGui.SetScrollHereY(-0.05f);
                    NeedsToScroll = false;
                }

                OnTpButtonClicked(tp, isSelected);

                if (!isSelected)
                    ImGui.PopStyleColor();

                ImGui.NextColumn();
                ImGui.Text($"{tp.Bpm:0.00}");
                ImGui.NextColumn();
            }

            ImGui.EndChild();
        }

        /// <summary>
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="isSelected"></param>
        private void OnTpButtonClicked(TimingPointInfo tp, bool isSelected)
        {
            var t = TimeSpan.FromMilliseconds(tp.StartTime);

            if (!ImGui.Button($"{t.Minutes:00}:{t.Seconds:00}:{t.Milliseconds:000}", new Vector2()))
                return;

            // User is holding control down, so add this SV to the selected ones
            if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) ||
                KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl))
            {
                if (isSelected)
                {
                    if (SelectedTimingPoints.Count != 1)
                        SelectedTimingPoints.Remove(tp);

                    if (SelectedTimingPoints.Count == 1)
                        SetInputTextToFirstSelected();
                }
                else
                {
                    SelectedTimingPoints.Add(tp);

                    if (SelectedTimingPoints.Count == 1)
                        SetInputTextToFirstSelected();
                    else
                    {
                        TextTime = "";
                        TextBpm = "";
                    }
                }
            }
            // Clicking the currently selected button seeks the user to that point in time
            else
            {
                if (isSelected && !AudioEngine.Track.IsDisposed)
                {
                    AudioEngine.Track.Seek(tp.StartTime);

                    var game = GameBase.Game as QuaverGame;
                    var screen = game?.CurrentScreen as EditorScreen;
                    screen?.SetHitSoundObjectIndex();
                }

                SelectedTimingPoints.Clear();
                SelectedTimingPoints.Add(tp);

                TextTime = tp.StartTime.ToString(CultureInfo.InvariantCulture);
                TextBpm = $"{tp.Bpm:0.00}";
            }
        }

        /// <summary>
        /// </summary>
        public void Show()
        {
            Shown = true;
            NeedsToScroll = true;
            SelectClosestPoint();
            DialogManager.Show(Dialog);

            if (!ButtonManager.Buttons.Contains(Dialog))
                ButtonManager.Add(Dialog);

            Dialog.Animations.Clear();
            Dialog.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Dialog.Alpha, 0.75f, 100));

            /*Dialog.BpmCalculator.ClearAnimations();
            Dialog.BpmCalculator.MoveToX(0, Easing.OutQuint, 500);*/
        }

        /// <summary>
        /// </summary>
        public void Hide()
        {
            Shown = false;
            Dialog.Close();
        }

        /// <summary>
        /// </summary>
        private void SetInputTextToFirstSelected()
        {
            TextTime = SelectedTimingPoints.First().StartTime.ToString(CultureInfo.InvariantCulture);
            TextBpm = $"{SelectedTimingPoints.First().Bpm:0.00}";
        }

        /// <summary>
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string OnlyDigits(string num) => DigitsOnly.Replace(num, "");

        /// <summary>
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string Decimal(string num) => DecimalRegex.Replace(num, "");

        /// <summary>
        /// </summary>
        private void UpdateSelectedTimingPoints()
        {
            if (SelectedTimingPoints.Count == 0)
                return;

            var changes = new List<EditorTimingPointChangeInfo>();

            foreach (var tp in SelectedTimingPoints)
            {
                var time = OnlyDigits(TextTime);
                var bpm = Decimal(TextBpm);

                float newTime;

                try
                {
                    newTime = string.IsNullOrEmpty(TextTime) ? tp.StartTime : float.Parse(time);
                }
                catch (Exception)
                {
                    newTime = tp.StartTime;
                }

                float newBpm;

                try
                {
                    newBpm = string.IsNullOrEmpty(TextBpm) ? tp.Bpm : float.Parse(bpm);
                }
                catch (Exception e)
                {
                    newBpm = tp.Bpm;
                }

                // ReSharper disable twice CompareOfFloatsByEqualityOperator
                if (tp.StartTime != newTime || tp.Bpm!= newBpm)
                    changes.Add(new EditorTimingPointChangeInfo(tp, newTime, newBpm));
            }

            var game = GameBase.Game as QuaverGame;
            var screen = game?.CurrentScreen as EditorScreen;

            screen?.Ruleset.ActionManager.Perform(new EditorActionChangeTimingPoint(WorkingMap, changes));

            if (changes.Count != 0)
            {
                TextTime = "";
                TextBpm = $"{changes.First().NewBpm:0.00}";
            }
        }
    }
}