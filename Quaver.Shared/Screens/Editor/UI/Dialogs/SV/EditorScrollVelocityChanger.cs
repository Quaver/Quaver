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
using Wobble;
using Wobble.Graphics.Animations;
using Wobble.Graphics.ImGUI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Window;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.SV
{
    public class EditorScrollVelocityChanger : SpriteImGui
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
        private string TextMultiplier = "";

        /// <summary>
        /// </summary>
        public EditorScrollVelocityDialog Dialog { get; }

        /// <summary>
        /// </summary>
        public Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        public List<SliderVelocityInfo> SelectedVelocities { get; } = new List<SliderVelocityInfo>();

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
        private bool TextMultiplierFocusedInLastFrame { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        public EditorScrollVelocityChanger(Qua map)
        {
            Dialog = new EditorScrollVelocityDialog(this);
            WorkingMap = map;
            SelectClosestVelocity();
        }

        /// <summary>
        /// </summary>
        private void SelectClosestVelocity()
        {
            if (WorkingMap.SliderVelocities.Count == 0)
                return;

            // Find the closest SV point so it can be selected upon opening.
            SliderVelocityInfo closestVelocity = null;
            var minDiff = double.MaxValue;

            WorkingMap.SliderVelocities.ForEach(x =>
            {
                var diff = Math.Abs(AudioEngine.Track.Time - x.StartTime);

                if (!(diff < minDiff))
                    return;

                minDiff = diff;
                closestVelocity = x;
            });

            if (closestVelocity == null)
                return;

            SelectedVelocities.Clear();
            SelectedVelocities.Add(closestVelocity);
            NeedsToScroll = true;

            TextTime = closestVelocity.StartTime.ToString(CultureInfo.InvariantCulture);
            TextMultiplier = $"{closestVelocity.Multiplier:0.00}";
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
                    SelectedVelocities.Clear();
                    SelectedVelocities.AddRange(WorkingMap.SliderVelocities);

                    if (SelectedVelocities.Count >= 1 || SelectedVelocities.Count == 0)
                    {
                        TextTime = "";
                        TextMultiplier = $"";
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

            ImGui.Begin("Scroll Velocities", ImGuiWindowFlags.Modal | ImGuiWindowFlags.NoCollapse |
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
            var nativeSize = new Vector2(390, 500);;

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
            ImGui.TextWrapped("Scroll Velocities (SV) allow you to dynamically change the speed and direction at which the objects fall. " +
                              "\n\nYou can click on an individual SV point to edit it and double-click to go to its position in time.\n\n" +
                              "Negative SV multipliers will make the playfield scroll backwards.");

            ImGui.Dummy(new Vector2(0, 10));
        }

        /// <summary>
        /// </summary>
        private void HandleAddRemoveButtons()
        {
            if (ImGui.Button("Add"))
            {
                var sv = new SliderVelocityInfo()
                {
                    StartTime = (int) AudioEngine.Track.Time,
                    Multiplier = 1.0f
                };

                var game = GameBase.Game as QuaverGame;
                var screen = game?.CurrentScreen as EditorScreen;
                screen?.Ruleset.ActionManager.Perform(new EditorActionAddSliderVelocity(WorkingMap, sv));

                SelectedVelocities.Clear();
                SelectedVelocities.Add(sv);
                NeedsToScroll = true;

                TextTime = sv.StartTime.ToString(CultureInfo.InvariantCulture);
                TextMultiplier = $"{sv.Multiplier:0.00}";
            }

            ImGui.SameLine();

            if (ImGui.Button("Remove"))
            {
                if (SelectedVelocities.Count == 0)
                    return;

                var game = GameBase.Game as QuaverGame;
                var screen = game?.CurrentScreen as EditorScreen;

                var lastSv = SelectedVelocities.Last();

                screen?.Ruleset.ActionManager.Perform(new EditorActionRemoveSliderVelocities(WorkingMap, new List<SliderVelocityInfo>(SelectedVelocities)));

                SelectedVelocities.Clear();

                if (WorkingMap.SliderVelocities.Count != 0)
                {
                    var sv = WorkingMap.SliderVelocities.FindLast(x => x.StartTime <= lastSv.StartTime);

                    if (sv != null)
                    {
                        TextTime = sv.StartTime.ToString(CultureInfo.InvariantCulture);
                        TextMultiplier = $"{sv.Multiplier:0.00}";
                        SelectedVelocities.Add(sv);
                    }
                    else
                    {
                        TextTime = "";
                        TextMultiplier = $"";
                    }
                }
                else
                {
                    TextTime = "";
                    TextMultiplier = $"";
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
                SelectedVelocities.Count == 0 ? ImGuiInputTextFlags.ReadOnly : ImGuiInputTextFlags.CharsDecimal))
            {
                if (string.IsNullOrEmpty(TextTime) || string.IsNullOrWhiteSpace(TextTime))
                    TextTime = "0";

                TextTime = OnlyDigits(TextTime);
            }

            // User stopped typing in the time field, so it needs to be updated
            if (!ImGui.IsItemActive() && TextTimeFocusedInLastFrame)
                UpdateSelectedVelocities();

            TextTimeFocusedInLastFrame = ImGui.IsItemActive();

            ImGui.Dummy(new Vector2(0, 10));

            ImGui.Text("Multiplier");

            if (ImGui.InputText(" ", ref TextMultiplier, 100,
                SelectedVelocities.Count == 0 ? ImGuiInputTextFlags.ReadOnly : ImGuiInputTextFlags.CharsDecimal))
            {
                if (string.IsNullOrEmpty(TextMultiplier) || string.IsNullOrWhiteSpace(TextMultiplier))
                    TextMultiplier = "1.0";

                TextMultiplier = Decimal(TextMultiplier);
            }

            // User stopped typing in multiplier frame
            if (!ImGui.IsItemActive() && TextMultiplierFocusedInLastFrame)
                UpdateSelectedVelocities();

            TextMultiplierFocusedInLastFrame = ImGui.IsItemActive();
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
            ImGui.Text("Multiplier");
            ImGui.Columns();
            ImGui.Separator();
        }

        /// <summary>
        /// </summary>
        private void AddSvTable()
        {
            ImGui.BeginChild("SV Area");

            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, 105);

            if (NeedsToScroll && SelectedVelocities.Count != 0  && WorkingMap.SliderVelocities.Count == 0)
            {
                ImGui.SetScrollHereY(-0.05f);
                NeedsToScroll = false;
            }

            foreach (var sv in WorkingMap.SliderVelocities)
            {
                var isSelected = SelectedVelocities.Contains(sv);

                if (!isSelected)
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(100, 100, 100, 0));

                if (NeedsToScroll && SelectedVelocities.Count != 0 && SelectedVelocities.First() == sv)
                {
                    ImGui.SetScrollHereY(-0.05f);
                    NeedsToScroll = false;
                }

                OnSvButtonClicked(sv, isSelected);

                if (!isSelected)
                    ImGui.PopStyleColor();

                ImGui.NextColumn();
                ImGui.Text($"{sv.Multiplier:0.00}x");
                ImGui.NextColumn();
            }

            ImGui.EndChild();
        }

        /// <summary>
        /// </summary>
        /// <param name="sv"></param>
        /// <param name="isSelected"></param>
        private void OnSvButtonClicked(SliderVelocityInfo sv, bool isSelected)
        {
            var t = TimeSpan.FromMilliseconds(sv.StartTime);

            if (!ImGui.Button($"{t.Minutes:00}:{t.Seconds:00}:{t.Milliseconds:000}", new Vector2()))
                return;

            // User is holding control down, so add this SV to the selected ones
            if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) ||
                KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl))
            {
                if (isSelected)
                {
                    if (SelectedVelocities.Count != 1)
                        SelectedVelocities.Remove(sv);

                    if (SelectedVelocities.Count == 1)
                        SetInputTextToFirstSelected();
                }
                else
                {
                    SelectedVelocities.Add(sv);

                    if (SelectedVelocities.Count == 1)
                        SetInputTextToFirstSelected();
                    else
                    {
                        TextTime = "";
                        TextMultiplier = "";
                    }
                }
            }
            // Clicking the currently selected button seeks the user to that point in time
            else
            {
                if (isSelected && !AudioEngine.Track.IsDisposed)
                {
                    AudioEngine.Track.Seek(sv.StartTime);

                    var game = GameBase.Game as QuaverGame;
                    var screen = game?.CurrentScreen as EditorScreen;
                    screen?.SetHitSoundObjectIndex();
                }

                SelectedVelocities.Clear();
                SelectedVelocities.Add(sv);

                TextTime = sv.StartTime.ToString(CultureInfo.InvariantCulture);
                TextMultiplier = $"{sv.Multiplier:0.00}";
            }
        }

        /// <summary>
        /// </summary>
        public void Show()
        {
            Shown = true;
            NeedsToScroll = true;
            SelectClosestVelocity();
            DialogManager.Show(Dialog);

            if (!ButtonManager.Buttons.Contains(Dialog))
                ButtonManager.Add(Dialog);

            Dialog.Animations.Clear();
            Dialog.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Dialog.Alpha, 0.75f, 100));
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
            TextTime = SelectedVelocities.First().StartTime.ToString(CultureInfo.InvariantCulture);
            TextMultiplier = $"{SelectedVelocities.First().Multiplier:0.00}";
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
        private void UpdateSelectedVelocities()
        {
            if (SelectedVelocities.Count == 0)
                return;

            var changes = new List<EditorScrollVelocityChangeInfo>();

            foreach (var sv in SelectedVelocities)
            {
                var time = OnlyDigits(TextTime);
                var multiplier = Decimal(TextMultiplier);

                float newTime = 0;

                try
                {
                    newTime = string.IsNullOrEmpty(TextTime) ? sv.StartTime : float.Parse(time);
                }
                catch (Exception)
                {
                    newTime = sv.StartTime;
                }

                float newMultiplier;

                try
                {
                    newMultiplier = string.IsNullOrEmpty(TextMultiplier) ? sv.Multiplier : float.Parse(multiplier);
                }
                catch (Exception e)
                {
                    newMultiplier = sv.Multiplier;
                }

                // ReSharper disable twice CompareOfFloatsByEqualityOperator
                if (sv.StartTime != newTime || sv.Multiplier != newMultiplier)
                    changes.Add(new EditorScrollVelocityChangeInfo(sv, newTime, newMultiplier));
            }

            var game = GameBase.Game as QuaverGame;
            var screen = game?.CurrentScreen as EditorScreen;

            screen?.Ruleset.ActionManager.Perform(new EditorActionChangeScrollVelocity(WorkingMap, changes));

            if (changes.Count != 0)
            {
                TextTime = "";
                TextMultiplier = $"{changes.First().NewMultiplier:0.00}";
            }
        }
    }
}