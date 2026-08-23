using ImGuiNET;

namespace Quaver.Shared.Screens.Edit.Plugins;

public static class EditorImGui
{
    public static bool Begin(IEditorPlugin plugin, string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None)
    {
        var isOpen = plugin.IsActive;
        var isVisible = ImGui.Begin(name, ref isOpen, flags);
        plugin.IsActive = isOpen;
        return isVisible;
    }
}
