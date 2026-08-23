using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Edit;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics.ImGUI;
using Wobble.Managers;
using Wobble.Window;
using NumericsVector2 = System.Numerics.Vector2;
using NumericsVector4 = System.Numerics.Vector4;

namespace Quaver.Shared.Screens.V2.SkinEditor
{
    internal sealed class SkinEditorColorPicker : SpriteImGui
    {
        private const ImGuiColorEditFlags PickerFlags =
            ImGuiColorEditFlags.DisplayRGB |
            ImGuiColorEditFlags.InputRGB |
            ImGuiColorEditFlags.Uint8;

        private Action<string> changed;
        private NumericsVector4 color;
        private string pendingHex;
        private bool positionWindow;
        private bool allowAlpha;

        public bool IsOpen { get; private set; }

        public SkinEditorColorPicker()
            : base(true, EditorImGuiOptions.GetOptions(),
                ConfigManager.EditorImGuiScalePercentage.Value / 100f)
        {
        }

        public void Open(Color initialColor, Action<string> onChanged, bool allowAlpha = true)
        {
            this.allowAlpha = allowAlpha;
            color = new NumericsVector4(
                initialColor.R / 255f,
                initialColor.G / 255f,
                initialColor.B / 255f,
                allowAlpha ? initialColor.A / 255f : 1);
            changed = onChanged;
            pendingHex = null;
            positionWindow = true;
            IsOpen = true;
        }

        public void Close()
        {
            CommitPendingColor();
            IsOpen = false;
            changed = null;
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
            var visible = ImGui.Begin(LocalizationManager.Get("SkinEditor_Property_Color") +
                                      "###SkinEditorColorPicker", ref open,
                ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings);
            var hovered = ImGui.IsWindowHovered();
            if (visible)
            {
                var flags = PickerFlags | (allowAlpha
                    ? ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.AlphaPreviewHalf
                    : ImGuiColorEditFlags.NoAlpha);
                if (ImGui.ColorPicker4("##SkinEditorColor", ref color, flags))
                    pendingHex = ToHex(color, allowAlpha);

                if (ImGui.IsItemDeactivatedAfterEdit() ||
                    pendingHex != null &&
                    !ImGui.IsMouseDown(ImGuiMouseButton.Left) &&
                    !ImGui.IsAnyItemActive())
                    CommitPendingColor();
            }
            ImGui.End();

            if (!open || ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !hovered)
                Close();
        }

        private void CommitPendingColor()
        {
            if (pendingHex == null)
                return;

            changed?.Invoke(pendingHex);
            pendingHex = null;
        }

        private static string ToHex(NumericsVector4 value, bool includeAlpha)
        {
            var color = new Color(
                ToByte(value.X),
                ToByte(value.Y),
                ToByte(value.Z),
                includeAlpha ? ToByte(value.W) : byte.MaxValue);
            return includeAlpha
                ? SkinV2Color.ToHexAlpha(color)
                : $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private static byte ToByte(float value) =>
            (byte) Math.Round(Math.Clamp(value, 0, 1) * byte.MaxValue,
                MidpointRounding.AwayFromZero);
    }
}
