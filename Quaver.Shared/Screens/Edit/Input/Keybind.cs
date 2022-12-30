using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Wobble.Input;
using Wobble.Logging;
using YamlDotNet.Serialization;

namespace Quaver.Shared.Screens.Edit.Input
{
    [Serializable]
    public class Keybind
    {
        [YamlIgnore] public static Keybind None = new Keybind(Keys.None);

        public HashSet<KeyModifiers> Modifiers { get; } = new HashSet<KeyModifiers>();
        public GenericKey Key { get; } = new GenericKey() {KeyboardKey = Keys.None};

        public Keybind(string notation)
        {
            var keys = notation.Trim().Split("+").Select(s => s.Trim());
            foreach (var keyString in keys)
            {
                KeyModifiers mod;
                if (Enum.TryParse(keyString, out mod))
                    Modifiers.Add(mod);
                else if (Key.KeyboardKey == Keys.None)
                {
                    GenericKey parsed;
                    if (GenericKey.TryParse(keyString, out parsed))
                        Key = parsed;
                    else
                        Logger.Error($"Encountered unknown key name {keyString} during keybind parsing", LogType.Runtime);
                }
            }
        }

        public Keybind(Keys key) => Key = new GenericKey() {KeyboardKey = key};

        public Keybind(KeyModifiers mod, Keys key)
        {
            Key = new GenericKey() {KeyboardKey = key};
            Modifiers.Add(mod);
        }

        public Keybind(ICollection<KeyModifiers> mods, Keys key)
        {
            Key = new GenericKey() {KeyboardKey = key};
            Modifiers = new HashSet<KeyModifiers>(mods);
        }

        public Keybind(ICollection<KeyModifiers> mods, GenericKey key)
        {
            Key = key;
            Modifiers = new HashSet<KeyModifiers>(mods);
        }

        public HashSet<Keybind> MatchingKeybinds()
        {
            var set = new HashSet<Keybind>();

            if (!Modifiers.Contains(KeyModifiers.Free))
                set.Add(this);
            else
            {
                var allModifiers = Enum.GetValues(typeof(KeyModifiers)).Cast<KeyModifiers>();
                var freeModifiers = allModifiers.Except(Modifiers).ToList();
                foreach (var modifiers in PowerSetOfModifiers(freeModifiers))
                    set.Add(new Keybind(modifiers, Key));
            }

            return set;
        }

        private HashSet<HashSet<KeyModifiers>> PowerSetOfModifiers(List<KeyModifiers> modifiers)
        {
            var powerSetLength = (int)Math.Pow(2, modifiers.Count());
            var powerSet = new HashSet<HashSet<KeyModifiers>>(powerSetLength);

            for (int bitMask = 0; bitMask < powerSetLength; bitMask++)
            {
                var subSet = new HashSet<KeyModifiers>(modifiers.Where(x => ((1 << modifiers.IndexOf(x)) & bitMask) != 0));
                powerSet.Add(subSet);
            }

            return powerSet;
        }

        private bool ModifiersAreCorrect()
        {
            var currentState = new HashSet<KeyModifiers>();

            if (KeyboardManager.IsCtrlDown()) currentState.Add(KeyModifiers.Ctrl);
            if (KeyboardManager.IsAltDown()) currentState.Add(KeyModifiers.Alt);
            if (KeyboardManager.IsShiftDown()) currentState.Add(KeyModifiers.Shift);

            if (!Modifiers.Contains(KeyModifiers.Free))
            {
                return Modifiers.SetEquals(currentState);
            }
            else
            {
                currentState.Add(KeyModifiers.Free);
                return Modifiers.IsSubsetOf(currentState);
            }
        }

        public bool IsUniquePress() => ModifiersAreCorrect() && GenericKeyManager.IsUniquePress(Key);
        public bool IsUniqueRelease() => ModifiersAreCorrect() && GenericKeyManager.IsUniqueRelease(Key);

        public bool IsDown() => ModifiersAreCorrect() && GenericKeyManager.IsDown(Key);

        public bool IsUp() => !IsDown();

        public override string ToString()
        {
            var keys = Modifiers.Select(m => m.ToString()).ToList();
            keys.Add(Key.ToString());
            return String.Join('+', keys);
        }

        protected bool Equals(Keybind other) => ToString() == other.ToString();

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Keybind)obj);
        }

        public override int GetHashCode() => ToString().GetHashCode();
    }
}