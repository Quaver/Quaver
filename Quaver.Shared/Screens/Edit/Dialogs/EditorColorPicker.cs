using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Quaver.Shared.Config;
using Wobble.Graphics.ImGUI;
using Wobble.Window;
using NumericsVector2 = System.Numerics.Vector2;
using NumericsVector3 = System.Numerics.Vector3;
using NumericsVector4 = System.Numerics.Vector4;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    internal sealed class EditorColorPicker : SpriteImGui
    {
        private const int PresetsPerRow = 5;

        private const ImGuiColorEditFlags PickerFlags =
            ImGuiColorEditFlags.DisplayRGB |
            ImGuiColorEditFlags.DisplayHex |
            ImGuiColorEditFlags.InputRGB |
            ImGuiColorEditFlags.Uint8;

        private static readonly Color[] BuiltInPresetColors =
        {
            new(255, 0, 0),
            new(255, 255, 0),
            new(0, 255, 0),
            new(0, 128, 255),
            new(170, 0, 255)
        };

        private readonly string title;
        private readonly Action<Color> changed;
        private readonly Action closeRequested;
        private readonly List<Color> customPresetColors;

        private NumericsVector3 color;
        private Color? pendingColor;
        private int? selectedCustomPresetIndex;
        private bool positionWindow = true;
        private bool outsideClickReady;

        public bool IsOpen { get; private set; } = true;

        public EditorColorPicker(string title, Color initialColor,
            Action<Color> onChanged, Action onCloseRequested)
            : base(true, EditorImGuiOptions.GetOptions(),
                ConfigManager.EditorImGuiScalePercentage.Value / 100f)
        {
            this.title = title;
            changed = onChanged;
            closeRequested = onCloseRequested;
            customPresetColors = LoadCustomPresetColors();
            SetColor(initialColor);
        }

        public void SetColor(Color value)
        {
            color = new NumericsVector3(
                value.R / (float)byte.MaxValue,
                value.G / (float)byte.MaxValue,
                value.B / (float)byte.MaxValue);
            pendingColor = null;
        }

        public void Close()
        {
            CommitPendingColor();
            IsOpen = false;
        }

        protected override void RenderImguiLayout()
        {
            if (!IsOpen)
                return;

            if (positionWindow)
            {
                ImGui.SetNextWindowPos(
                    new NumericsVector2(WindowManager.Width / 2f, WindowManager.Height / 2f),
                    ImGuiCond.Appearing, new NumericsVector2(0.5f, 0.5f));
                positionWindow = false;
            }

            var open = IsOpen;
            ImGui.SetNextWindowSizeConstraints(
                new NumericsVector2(300, 0),
                new NumericsVector2(float.MaxValue, float.MaxValue));

            var contentsVisible = ImGui.Begin(title + "###EditorColorPicker", ref open,
                ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings);
            var windowHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow);

            if (contentsVisible)
            {
                if (ImGui.ColorPicker3("##EditorColor", ref color, PickerFlags))
                    pendingColor = ToColor(color);

                if (ImGui.IsItemDeactivatedAfterEdit() ||
                    pendingColor.HasValue &&
                    !ImGui.IsMouseDown(ImGuiMouseButton.Left) &&
                    !ImGui.IsAnyItemActive())
                    CommitPendingColor();

                DrawPresetColors();
            }

            ImGui.End();

            if (!open)
            {
                closeRequested?.Invoke();
                return;
            }

            if (!outsideClickReady)
            {
                outsideClickReady = !ImGui.IsMouseDown(ImGuiMouseButton.Left);
                return;
            }

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !windowHovered)
                closeRequested?.Invoke();
        }

        private void DrawPresetColors()
        {
            ImGui.Text("Presets");
            DrawPresetGrid(BuiltInPresetColors, "BuiltIn", false);

            ImGui.Spacing();
            ImGui.Text("Custom presets");

            if (customPresetColors.Count > 0)
                DrawPresetGrid(customPresetColors, "Custom", true);
            else
                ImGui.TextDisabled("No custom presets");

            if (ImGui.Button("Add current"))
                AddCurrentColorPreset();

            ImGui.SameLine();
            var hasSelectedPreset = selectedCustomPresetIndex.HasValue;
            ImGui.BeginDisabled(!hasSelectedPreset);

            if (ImGui.Button("Update"))
                UpdateSelectedColorPreset();

            ImGui.SameLine();

            if (ImGui.Button("Remove"))
                RemoveSelectedColorPreset();

            ImGui.EndDisabled();
        }

        private void DrawPresetGrid(IReadOnlyList<Color> presets, string idPrefix, bool custom)
        {
            var spacing = ImGui.GetStyle().ItemSpacing.X;
            var availableWidth = ImGui.GetContentRegionAvail().X - spacing * (PresetsPerRow - 1);
            var buttonWidth = availableWidth / PresetsPerRow;
            var buttonSize = new NumericsVector2(buttonWidth / 2, buttonWidth / 2);

            for (var i = 0; i < presets.Count; i++)
            {
                if (i % PresetsPerRow != 0)
                    ImGui.SameLine();

                var preset = presets[i];
                var presetVector = new NumericsVector4(
                    preset.R / (float)byte.MaxValue,
                    preset.G / (float)byte.MaxValue,
                    preset.B / (float)byte.MaxValue,
                    1);
                var isSelected = custom && selectedCustomPresetIndex == i;

                if (isSelected)
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 2);
                    ImGui.PushStyleColor(ImGuiCol.Border, new NumericsVector4(1, 1, 1, 1));
                }

                var clicked = ImGui.ColorButton($"##EditorColorPreset{idPrefix}{i}", presetVector,
                    ImGuiColorEditFlags.NoDragDrop, buttonSize);

                if (isSelected)
                {
                    ImGui.PopStyleColor();
                    ImGui.PopStyleVar();
                }

                if (!clicked)
                    continue;

                selectedCustomPresetIndex = custom ? i : null;
                ApplyPresetColor(preset);
            }
        }

        private void ApplyPresetColor(Color preset)
        {
            SetColor(preset);
            pendingColor = preset;
            CommitPendingColor();
        }

        private void AddCurrentColorPreset()
        {
            var currentColor = ToColor(color);
            var existingIndex = customPresetColors.IndexOf(currentColor);

            if (existingIndex >= 0)
            {
                selectedCustomPresetIndex = existingIndex;
                return;
            }

            customPresetColors.Add(currentColor);
            selectedCustomPresetIndex = customPresetColors.Count - 1;
            SaveCustomPresetColors();
        }

        private void UpdateSelectedColorPreset()
        {
            if (!selectedCustomPresetIndex.HasValue)
                return;

            customPresetColors[selectedCustomPresetIndex.Value] = ToColor(color);
            SaveCustomPresetColors();
        }

        private void RemoveSelectedColorPreset()
        {
            if (!selectedCustomPresetIndex.HasValue)
                return;

            var index = selectedCustomPresetIndex.Value;
            customPresetColors.RemoveAt(index);
            selectedCustomPresetIndex = customPresetColors.Count == 0
                ? null
                : Math.Min(index, customPresetColors.Count - 1);
            SaveCustomPresetColors();
        }

        private static List<Color> LoadCustomPresetColors()
        {
            var presets = new List<Color>();

            foreach (var value in ConfigManager.EditorColorPresets.Value.Split(',',
                         StringSplitOptions.RemoveEmptyEntries))
            {
                var hex = value.Trim();
                if (hex.Length != 6 ||
                    !uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
                    continue;

                var preset = new Color(
                    (byte)(rgb >> 16),
                    (byte)(rgb >> 8),
                    (byte)rgb);

                if (!presets.Contains(preset))
                    presets.Add(preset);
            }

            return presets;
        }

        private void SaveCustomPresetColors() => ConfigManager.EditorColorPresets.Value =
            string.Join(",", customPresetColors.Select(x => $"{x.R:X2}{x.G:X2}{x.B:X2}"));

        private void CommitPendingColor()
        {
            if (!pendingColor.HasValue)
                return;

            changed?.Invoke(pendingColor.Value);
            pendingColor = null;
        }

        private static Color ToColor(NumericsVector3 value) => new Color(
            ToByte(value.X),
            ToByte(value.Y),
            ToByte(value.Z));

        private static byte ToByte(float value) =>
            (byte)Math.Round(Math.Clamp(value, 0, 1) * byte.MaxValue,
                MidpointRounding.AwayFromZero);
    }
}
