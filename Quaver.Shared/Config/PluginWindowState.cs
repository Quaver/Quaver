using Quaver.Shared.Screens.Edit.Plugins;

namespace Quaver.Shared.Config;

/// <summary>
///     We don't need to store position here. ImGui stores them automatically in imgui.ini 
/// </summary>
public class PluginWindowState
{
    public bool Enabled { get; set; }

    public void ApplyState(IEditorPlugin plugin)
    {
        plugin.IsActive = Enabled;
    }

    public void RetrieveState(IEditorPlugin plugin)
    {
        Enabled = plugin.IsActive;
    }
}