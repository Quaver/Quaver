using System.Collections.Generic;
using Quaver.Shared.Screens.Edit.Plugins;

namespace Quaver.Shared.Config;

/// <summary>
///     We don't need to store position here. ImGui stores them automatically in imgui.ini 
/// </summary>
public class PluginWindowState
{
    public bool Enabled { get; set; }

    public Dictionary<string, EditorPluginStorageValue> Storage { get; set; } = new();

    public void ApplyState(IEditorPlugin plugin)
    {
        plugin.IsActive = Enabled;
        plugin.Storage.Clear();
        foreach (var (key, value) in Storage)
        {
            plugin.Storage.Add(key, value);
        }
    }

    public void RetrieveState(IEditorPlugin plugin)
    {
        Enabled = plugin.IsActive;
        Storage.Clear();
        foreach (var (key, value) in plugin.Storage)
        {
            Storage.Add(key, value);
        }
    }
}