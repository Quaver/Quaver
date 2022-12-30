using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace Quaver.Shared.Screens.Edit.Input
{
    [Serializable]
    public class KeybindList : HashSet<Keybind>
    {
        public KeybindList() => Add(new Keybind(Keys.None));

        public KeybindList(string notation) : base(notation.Split(',').Select(s => new Keybind(s)))
        {
        }

        public KeybindList(Keys key) => Add(new Keybind(key));
        public KeybindList(KeyModifiers mod, Keys key) => Add(new Keybind(mod, key));
        public KeybindList(Keybind keybind) => Add(keybind);

        public KeybindList(List<string> binds) : base(binds.Select(b => new Keybind(b)))
        {
        }

        public KeybindList(IEnumerable<Keybind> keybinds) : base(keybinds)
        {
        }

        new public void Add(Keybind keybind)
        {
            if (Equals(keybind, Keybind.None)) return;

            if (Count == 1 && Contains(Keybind.None)) Remove(Keybind.None);
            base.Add(keybind);
        }

        new public bool Remove(Keybind keybind)
        {
            if (Equals(keybind, Keybind.None)) return false;

            var result = base.Remove(keybind);
            if (Count == 0) Add(Keybind.None);

            return result;
        }

        public bool IsNotBound() => Count == 1 && Contains(Keybind.None);

        public HashSet<Keybind> MatchingKeybinds()
        {
            var binds = new HashSet<Keybind>();
            foreach (var keybind in this) binds.UnionWith(keybind.MatchingKeybinds());
            return binds;
        }

        public bool IsUniquePress() => this.Any(k => k.IsUniquePress());
        public bool IsUniqueRelease() => this.Any(k => k.IsUniqueRelease());
        public bool IsDown() => this.Any(k => k.IsDown());
        public bool IsUp() => this.Any(k => k.IsUp());
        public override string ToString() => String.Join(", ", this.Where(k => !k.Equals(Keybind.None)).Select(k => k.ToString()));
    }
}