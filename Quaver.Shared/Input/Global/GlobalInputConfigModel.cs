using System;
using System.Collections.Generic;

namespace Quaver.Shared.Input.Global;

[Serializable]
public class GlobalInputConfigModel
{
    public Dictionary<GlobalKeybindActions, KeybindList> Keybinds { get; set; } = [];

    public GlobalInputConfigModel()
    {
    }

    public GlobalInputConfigModel(Dictionary<GlobalKeybindActions, KeybindList> keybinds)
    {
        Keybinds = keybinds;
    }
}