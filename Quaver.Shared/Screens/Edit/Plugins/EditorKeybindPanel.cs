using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Quaver.Shared.Screens.Edit.Input;
using Wobble;
using Wobble.Graphics.ImGUI;

namespace Quaver.Shared.Screens.Edit.Plugins;

public class EditorKeybindPanel : SpriteImGui, IEditorPlugin
{
    public EditorKeybindPanel(EditScreen screen): base(false, GetOptions(), screen.ImGuiScale)
    {
        Screen = screen;
        Initialize();
    }

    public bool IsActive { get; set; }
    public bool IsWindowHovered { get; private set; }
    public string Name => "Keybind Editor";
    public string Author => "WilliamQiufeng";
    public string Description { get; set; } = "Change the keymap of the editor";
    public bool IsBuiltIn { get; set; } = true;
    public string Directory { get; set; }
    public bool IsWorkshop { get; set; }

    /// <summary>
    /// </summary>
    private EditScreen Screen { get; }

    private KeybindActions? SelectedAction { get; set; }

    public void Initialize()
    {
    }

    protected override void RenderImguiLayout()
    {
        ImGui.SetNextWindowSizeConstraints(new Vector2(450, 0), new Vector2(450, float.MaxValue)); 
        ImGui.PushFont(Options.Fonts.First().Context);
        ImGui.Begin(Name);

        DrawTable();

        IsWindowHovered = ImGui.IsWindowHovered() || ImGui.IsAnyItemFocused();
        
        ImGui.End();
    }

    private void DrawTable()
    {
        if (!ImGui.BeginTable("Keybinds", 2, ImGuiTableFlags.None)) return;
        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("Keybind");
        ImGui.TableHeadersRow();
        foreach (var (action, keybindList) in Screen.InputManager.InputConfig.Keybinds)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            var selected = SelectedAction == action;
            if (!selected)
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(100, 100, 100, 0));

            if (ImGui.Button($"{action}"))
            {
                SelectedAction = action;
            }
            if (!selected)
                ImGui.PopStyleColor();
            ImGui.TableNextColumn();
            ImGui.TextWrapped(string.Join(", ", keybindList));
        }
        ImGui.EndTable();
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public static ImGuiOptions GetOptions() => new ImGuiOptions(new List<ImGuiFont>
    {
        new ImGuiFont($@"{WobbleGame.WorkingDirectory}/Fonts/lato-black.ttf", 14),
    }, false);
}