using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Wobble.Input;
using Wobble.Logging;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
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
            var keys = notation.Trim().Split("+").Select(s => s.ToLower().Trim());
            foreach (var keyString in keys)
            {
                if (keyString == "ctrl")
                    Modifiers.Add(KeyModifiers.Ctrl);
                else if (keyString == "shift")
                    Modifiers.Add(KeyModifiers.Shift);
                if (keyString == "alt")
                    Modifiers.Add(KeyModifiers.Alt);
                else if (Key.KeyboardKey == Keys.None)
                {
                    GenericKey parsed;
                    if (GenericKey.TryParse(keyString, out parsed))
                        Key = parsed;
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

        private bool ModifiersAreCorrect()
        {
            return (Modifiers.Contains(KeyModifiers.Ctrl) == KeyboardManager.IsCtrlDown())
                   && (Modifiers.Contains(KeyModifiers.Shift) == KeyboardManager.IsShiftDown())
                   && (Modifiers.Contains(KeyModifiers.Alt) == KeyboardManager.IsAltDown());
        }

        public bool IsUniqueKeypress()
        {
            if (!ModifiersAreCorrect()) return false;
            return GenericKeyManager.IsUniquePress(Key);
        }

        public bool IsDown()
        {
            if (!ModifiersAreCorrect()) return false;
            return GenericKeyManager.IsDown(Key);
        }

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

    internal sealed class KeybindYamlTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(Keybind);

        public object ReadYaml(IParser parser, Type type)
        {
            if (!parser.Accept<Scalar>()) return Keybind.None;
            var scalar = parser.Current as Scalar;
            if (scalar == null) return Keybind.None;

            Logger.Debug(scalar.Value, LogType.Runtime);

            var result = new Keybind(scalar.Value);
            parser.MoveNext();

            return result;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type) =>
            emitter.Emit(new Scalar(null, ((Keybind)value).ToString()));
    }
}