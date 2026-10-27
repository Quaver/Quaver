using System;
using System.Collections.Generic;
using Quaver.Shared.Input;

namespace Quaver.Shared.Screens.V2.Options.Model
{
    internal sealed class OptionsPreset
    {
        public int Version { get; set; } = 1;
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, string> Values { get; set; } = new();
        public Dictionary<string, KeybindList> GlobalKeybinds { get; set; } = new();
        public Dictionary<string, List<string>> KeyLayouts { get; set; } = new();
    }
}
