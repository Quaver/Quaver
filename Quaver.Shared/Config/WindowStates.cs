using System;
using System.Collections.Generic;
using Quaver.Shared.Screens.Edit;

namespace Quaver.Shared.Config;

public class WindowStates
{
    public Dictionary<EditorPanelType, EditorPanelState> EditorPanelStates { get; set; } = new();
    public Dictionary<string, PluginWindowState> PluginWindowStates { get; set; } = new();

    /// <summary>
    ///     Order needs to be preserved so panels will consistently be over another
    /// </summary>
    private static readonly EditorPanelType[] EditorPanelLoadOrder =
    {
        EditorPanelType.Details,
        EditorPanelType.CompositionTools,
        EditorPanelType.Hitsounds,
        EditorPanelType.Layers
    };

    public void ApplyState(EditScreenView editScreenView)
    {
        var editScreen = (EditScreen)editScreenView.Screen;

        foreach (var editorPanelType in EditorPanelLoadOrder)
        {
            if (!EditorPanelStates.TryGetValue(editorPanelType, out var editorPanelState))
                continue;

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
        foreach (var editorPanelType in EditorPanelLoadOrder)
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