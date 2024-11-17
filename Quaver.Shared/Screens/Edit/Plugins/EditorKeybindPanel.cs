using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Screens.Edit.Input;
using Wobble;
using Wobble.Graphics.ImGUI;
using Wobble.Input;

namespace Quaver.Shared.Screens.Edit.Plugins;

public class EditorKeybindPanel : SpriteImGui, IEditorPlugin
{
    public EditorKeybindPanel(EditScreen screen) : base(false, GetOptions(), screen.ImGuiScale)
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

    private GenericKeyState PreviousKeyState { get; set; }

    /// <summary>
    /// </summary>
    private EditScreen Screen { get; }

    private KeybindActions? SelectedAction { get; set; }

    private Keybind RebindingKeybind { get; set; }

    private readonly Keybind _emptyKeybind = new(Keys.None);

    public void Initialize()
    {
    }

    protected override void RenderImguiLayout()
    {
        ImGui.SetNextWindowSizeConstraints(new Vector2(450, 0), new Vector2(450, float.MaxValue));
        ImGui.PushFont(Options.Fonts.First().Context);
        ImGui.Begin(Name);
        IsWindowHovered = ImGui.IsWindowHovered() || ImGui.IsAnyItemFocused();

        DrawDescription();

        ImGui.Dummy(new Vector2(10, 10));
        
        DrawEdit();

        DrawTable();

        HandleInput();

        ImGui.End();
    }

    private void DrawDescription()
    {
        ImGui.TextWrapped("To change the keybind of an action, first click on the action.");
        ImGui.TextWrapped("You can then left click on a key to change the keybind, or right click to remove it.");
        ImGui.TextWrapped("You can also add a key by clicking on the '+' button.");
    }

    private void HandleInput()
    {
        var currentKeyState = new GenericKeyState(GenericKeyManager.GetPressedKeys());
        var keys = currentKeyState.UniqueKeyPresses(PreviousKeyState ?? new GenericKeyState(Array.Empty<GenericKey>()));
        PreviousKeyState = currentKeyState;
        if (RebindingKeybind == null || SelectedAction == null)
            return;
        if (keys.Count > 0)
        {
            var inputConfigKeybind = Screen.InputManager.InputConfig.Keybinds[SelectedAction.Value];
            inputConfigKeybind.Remove(RebindingKeybind);
            inputConfigKeybind.Add(keys.First());
            FlushConfig();
            RebindingKeybind = null;
        }
    }

    private void DrawEdit()
    {
        var selected = SelectedAction.HasValue;
        ImGui.TextWrapped($"Selected Keybind: {(selected ? SelectedAction.ToString() : "None")}");
        ImGui.BeginDisabled(!selected);

        var keybindDictionary = Screen.InputManager.InputConfig.Keybinds;
        if (ImGui.Button("Reset to Default"))
        {
            keybindDictionary[SelectedAction!.Value] = EditorInputConfig.DefaultKeybinds[SelectedAction!.Value];
            FlushConfig();
        }

        if (ImGui.BeginTable("Keys", 1, ImGuiTableFlags.None))
        {
            if (selected)
            {
                ImGui.TableSetupColumn("Keys");
                ImGui.TableHeadersRow();
                var keybinds = keybindDictionary[SelectedAction.Value].ToList();
                if (Equals(RebindingKeybind, _emptyKeybind))
                    keybinds.Add(_emptyKeybind);
                foreach (var keybind in keybinds)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    var isKeybindRebinding = Equals(RebindingKeybind, keybind);
                    if (isKeybindRebinding)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                        ImGui.BulletText("Please enter a new keybind...");
                        ImGui.PopStyleColor();
                    }
                    else
                    {
                        ImGui.BulletText(keybind.ToString());
                        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                        {
                            RebindingKeybind = keybind;
                            PreviousKeyState = new GenericKeyState(new GenericKey[]
                                { new() { MouseButton = MouseButton.Left } });
                        }

                        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                        {
                            keybindDictionary[SelectedAction.Value].Remove(keybind);
                            FlushConfig();
                        }
                    }
                }

                if (!Equals(RebindingKeybind, _emptyKeybind))
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    if (ImGui.Button("+"))
                    {
                        RebindingKeybind = _emptyKeybind;
                    }
                }
            }

            IsWindowHovered = IsWindowHovered || ImGui.IsWindowHovered() || ImGui.IsAnyItemFocused();
            ImGui.EndTable();
        }


        ImGui.EndDisabled();
    }

    private void FlushConfig()
    {
        Screen.InputManager.InputConfig.SaveToConfig();
        Screen.ResetInputManager();
    }

    private unsafe void DrawTable()
    {
        if (!ImGui.BeginTable("Keybinds", 2, ImGuiTableFlags.ScrollY | ImGuiTableFlags.BordersH)) return;
        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("Keybind");
        ImGui.TableHeadersRow();
        var clipperRaw = new ImGuiListClipper();
        var clipper = new ImGuiListClipperPtr(&clipperRaw);
        var inputConfigKeybinds = Screen.InputManager.InputConfig.Keybinds.ToList();
        clipper.Begin(inputConfigKeybinds.Count);
        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var (action, keybindList) = inputConfigKeybinds[i];
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
        }

        IsWindowHovered = IsWindowHovered || ImGui.IsWindowHovered() || ImGui.IsAnyItemFocused();

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