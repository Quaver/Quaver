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

    /// <summary>
    ///     If null, nothing happens.
    ///     Otherwise if <see cref="SelectedAction"/> is not null, we will record the next keybind.
    ///     The current value of <see cref="RebindingKeybind"/> will be changed to the next keybind
    ///     for the <see cref="SelectedAction"/>.
    /// </summary>
    private Keybind RebindingKeybind { get; set; }

    private readonly Keybind _emptyKeybind = new(Keys.None);

    private string _searchQuery = "";

    /// <summary>
    ///     If <see cref="_emptyKeybind"/>, we will record the next key input to be filtered.
    ///     If null, don't apply this filter.
    ///     Otherwise only actions with this keybind will be shown.
    /// </summary>
    private Keybind SearchKeybind { get; set; }

    /// <summary>
    /// </summary>
    private HashSet<Keybind> ConflictingKeybinds { get; set; } = new();

    /// <summary>
    ///     The entries to show in the table
    ///     If null, use Screen.InputManager.InputConfig.Keybinds
    ///     Otherwise use this
    /// </summary>
    private List<KeyValuePair<KeybindActions, KeybindList>> ShownInputConfigKeybinds { get; set; }

    /// <summary>
    ///     When the input system get reset, we need to know this.
    /// </summary>
    private Dictionary<KeybindActions, KeybindList> LastInputConfigReference { get; set; }

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

        if (!ReferenceEquals(LastInputConfigReference, Screen.InputManager.InputConfig.Keybinds))
        {
            SearchKeybind = null;
            _searchQuery = "";
            FindConflictingKeybinds();
        }

        DrawEdit();

        DrawTable();

        HandleInput();

        ImGui.End();
        LastInputConfigReference = Screen.InputManager.InputConfig.Keybinds;
    }

    private void DrawDescription()
    {
        ImGui.TextWrapped("To change the keybind of an action, first click on the action.");
        ImGui.TextWrapped("You can then choose to change or remove any keys from the action.");
        ImGui.TextWrapped("You can also add a key by clicking on the '+' button.");
    }

    private void HandleInput()
    {
        var currentKeyState = new GenericKeyState(GenericKeyManager.GetPressedKeys());
        var keys = currentKeyState.UniqueKeyPresses(PreviousKeyState ?? new GenericKeyState(Array.Empty<GenericKey>()));
        PreviousKeyState = currentKeyState;

        if (Equals(SearchKeybind, _emptyKeybind))
        {
            if (keys.Count > 0)
            {
                SearchKeybind = keys.First();
                ApplyFilter();
                return;
            }
        }

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

        if (ImGui.BeginTable("Keys", 2, ImGuiTableFlags.None))
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
                        ImGui.TextWrapped("Please enter a new keybind...");
                        ImGui.PopStyleColor();
                    }
                    else
                    {
                        ImGui.TextWrapped(keybind.ToString());

                        ImGui.TableNextColumn();

                        // Don't allow input when search keybind is being recorded
                        ImGui.BeginDisabled(Equals(SearchKeybind, _emptyKeybind) || RebindingKeybind != null);
                        if (ImGui.Button($"Change##{SelectedAction.Value}_{keybind}"))
                        {
                            RebindingKeybind = keybind;
                            PreviousKeyState = new GenericKeyState(new GenericKey[]
                                { new() { MouseButton = MouseButton.Left } });
                        }

                        ImGui.SameLine();
                        if (ImGui.Button($"Remove##{SelectedAction.Value}_{keybind}"))
                        {
                            keybindDictionary[SelectedAction.Value].Remove(keybind);
                            FlushConfig();
                        }

                        ImGui.SameLine();
                        var free = keybind.Modifiers.Contains(KeyModifiers.Free);
                        if (ImGui.Checkbox($"Free##{SelectedAction.Value}_{keybind}", ref free))
                        {
                            var newKeybind = new Keybind(keybind.Modifiers, keybind.Key);
                            if (!newKeybind.Modifiers.Add(KeyModifiers.Free))
                                newKeybind.Modifiers.Remove(KeyModifiers.Free);

                            keybindDictionary[SelectedAction.Value].Remove(newKeybind);
                            keybindDictionary[SelectedAction.Value].Remove(keybind);
                            keybindDictionary[SelectedAction.Value].Add(newKeybind);
                            FlushConfig();
                        }

                        ImGui.SetItemTooltip(
                            "Turning on Free means that pressing the key with additional modifiers (ctrl, alt, ...) will also trigger the action.");

                        ImGui.EndDisabled();
                    }
                }
            }

            IsWindowHovered = IsWindowHovered || ImGui.IsWindowHovered() || ImGui.IsAnyItemFocused();
            ImGui.EndTable();
        }

        if (!Equals(RebindingKeybind, _emptyKeybind))
        {
            if (ImGui.Button("+"))
            {
                RebindingKeybind = _emptyKeybind;
            }
        }


        ImGui.EndDisabled();
    }

    private void FlushConfig()
    {
        Screen.InputManager.InputConfig.SaveToConfig();
        Screen.ResetInputManager();
        ApplyFilter();
        FindConflictingKeybinds();
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(_searchQuery) && SearchKeybind == null)
        {
            ShownInputConfigKeybinds = null;
            return;
        }

        ShownInputConfigKeybinds = Screen.InputManager.InputConfig.Keybinds.Where(
            entry =>
                entry.Key.ToString().ToLower().Contains(_searchQuery.ToLower())
                && (SearchKeybind == null || Equals(SearchKeybind, _emptyKeybind) ||
                    entry.Value.Contains(SearchKeybind))
        ).ToList();
    }

    private void FindConflictingKeybinds()
    {
        ConflictingKeybinds.Clear();
        var existingKeybinds = new HashSet<Keybind>();
        foreach (var (_, keybinds) in Screen.InputManager.InputConfig.Keybinds)
        {
            ConflictingKeybinds.UnionWith(existingKeybinds.Intersect(keybinds));
            existingKeybinds.UnionWith(keybinds);
        }
    }

    private unsafe void DrawTable()
    {
        if (ImGui.InputTextWithHint("##Search", "Search Actions", ref _searchQuery, 100))
        {
            ApplyFilter();
        }

        DrawKeybindSearch();

        if (!ImGui.BeginTable("Keybinds", 2, ImGuiTableFlags.ScrollY | ImGuiTableFlags.BordersH)) return;
        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("Keybind");
        ImGui.TableHeadersRow();
        var clipperRaw = new ImGuiListClipper();
        var clipper = new ImGuiListClipperPtr(&clipperRaw);

        var inputConfigKeybinds = ShownInputConfigKeybinds ?? Screen.InputManager.InputConfig.Keybinds.ToList();
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

                var conflicting = ConflictingKeybinds.Overlaps(keybindList);
                if (conflicting)
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                ImGui.TextWrapped(string.Join(", ", keybindList));
                if (conflicting)
                    ImGui.PopStyleColor();
            }
        }

        IsWindowHovered = IsWindowHovered || ImGui.IsWindowHovered() || ImGui.IsAnyItemFocused();

        ImGui.EndTable();
    }

    private void DrawKeybindSearch()
    {
        var waitingForInput = Equals(SearchKeybind, _emptyKeybind);
        ImGui.BeginDisabled(waitingForInput);

        var str = SearchKeybind?.ToString() ?? "";
        var flags = ImGuiInputTextFlags.EnterReturnsTrue;
        if (waitingForInput)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
            flags |= ImGuiInputTextFlags.ReadOnly;
            str = "Input a keybind to search...";
        }

        if (ImGui.InputTextWithHint("##SearchKeybind", "Search Keybind", ref str, 20, flags))
        {
            SearchKeybind = string.IsNullOrWhiteSpace(str) || !Keybind.TryParse(str, out var newSearchKeybind)
                ? null
                : newSearchKeybind;
            ApplyFilter();
        }

        ImGui.PopStyleColor();
        ImGui.SameLine();

        // Don't allow input when a keybind is being rebound
        ImGui.BeginDisabled(RebindingKeybind != null);
        if (ImGui.Button("Input"))
        {
            SearchKeybind = _emptyKeybind;
            PreviousKeyState = new GenericKeyState(new GenericKey[]
                { new() { MouseButton = MouseButton.Left } });
        }

        ImGui.SameLine();
        if (ImGui.Button("Clear"))
        {
            SearchKeybind = null;
            ApplyFilter();
        }

        ImGui.EndDisabled();

        ImGui.EndDisabled();
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public static ImGuiOptions GetOptions() => new ImGuiOptions(new List<ImGuiFont>
    {
        new ImGuiFont($@"{WobbleGame.WorkingDirectory}/Fonts/lato-black.ttf", 14),
    }, false);
}