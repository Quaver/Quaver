using System;
using System.Collections.Generic;
using Quaver.Shared.Input;

namespace Quaver.Shared.Screens.Edit.Input;

[Serializable]
public class EditorInputConfigModel
{
    public Dictionary<EditorKeybindActions, KeybindList> Keybinds { get; set; } = [];
    public Dictionary<string, KeybindList> PluginKeybinds { get; set; } = [];

    public EditorInputConfigModel()
    {
    }

    public EditorInputConfigModel(Dictionary<EditorKeybindActions, KeybindList> keybinds)
    {
        Keybinds = keybinds;
    }
}