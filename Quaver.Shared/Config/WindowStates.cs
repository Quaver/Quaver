using System;
using System.Collections.Generic;
using Quaver.Shared.Screens.Edit;

namespace Quaver.Shared.Config;

public class WindowStates
{
    public Dictionary<EditorPanelType, EditorPanelState> EditorPanelStates { get; set; } = new();
    public Dictionary<string, PluginWindowState> PluginWindowStates { get; set; } = new();

    public void ApplyState(EditScreenView editScreenView)
    {
        var editScreen = (EditScreen)editScreenView.Screen;

        foreach (var (editorPanelType, editorPanelState) in EditorPanelStates)
        {
            editorPanelState.ApplyState(editorPanelType, editScreenView);
        }

        foreach (var plugin in editScreen.Plugins)
        {
            if (PluginWindowStates.TryGetValue(plugin.Name, out var pluginWindowState))
            {
                pluginWindowState.ApplyState(plugin);
            }
        }
    }

    public void RetrieveState(EditScreenView editScreenView)
    {
        var editScreen = (EditScreen)editScreenView.Screen;

        foreach (EditorPanelType editorPanelType in Enum.GetValues(typeof(EditorPanelType)))
        {
            EditorPanelStates[editorPanelType] = new EditorPanelState();
            EditorPanelStates[editorPanelType].RetrieveState(editorPanelType, editScreenView);
        }

        foreach (var plugin in editScreen.Plugins)
        {
            PluginWindowStates[plugin.Name] = new PluginWindowState();
            PluginWindowStates[plugin.Name].RetrieveState(plugin);
        }
    }
}