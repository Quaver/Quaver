using ImGuiNET;
using Microsoft.Xna.Framework;

namespace Quaver.Shared.Screens.Edit.Plugins.Timing;

public interface IColoredImGuiTitle
{
    Color TitleColor { get; }

    void ImGuiPushTitleColors()
    {
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, TitleColor.PackedValue);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, TitleColor.PackedValue);
        ImGui.PushStyleColor(ImGuiCol.TitleBgCollapsed, new Color(TitleColor, 0.3f).PackedValue);
    }

    void ImGuiPopTitleColors()
    {
        ImGui.PopStyleColor();
        ImGui.PopStyleColor();
        ImGui.PopStyleColor();
    }
}