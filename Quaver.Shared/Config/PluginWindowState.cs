using System.IO;
using System.Numerics;
using ImGuiNET;
using Quaver.Shared.Screens.Edit;
using Quaver.Shared.Screens.Edit.Plugins;

namespace Quaver.Shared.Config;

/// <summary>
///     We don't need to store position here. ImGui stores them automatically in imgui.ini 
/// </summary>
public class PluginWindowState : IWindowState
{
    public string PluginName { get; set; }
    public bool Enabled { get; set; }
}