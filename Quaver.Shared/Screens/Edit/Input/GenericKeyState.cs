using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Wobble.Input;

namespace Quaver.Shared.Screens.Edit.Input
{
    public class GenericKeyState
    {
        public HashSet<KeyModifiers> Modifiers {get;}
        public HashSet<GenericKey> Pressed {get;}

        public GenericKeyState(IEnumerable<GenericKey> keys)
        {
            Modifiers = new HashSet<KeyModifiers>();
            Pressed = new HashSet<GenericKey>(keys);

            CheckForModifier(Keys.LeftControl, KeyModifiers.Ctrl);
            CheckForModifier(Keys.RightControl, KeyModifiers.Ctrl);
            CheckForModifier(Keys.LeftAlt, KeyModifiers.Alt);
            CheckForModifier(Keys.RightAlt, KeyModifiers.Alt);
            CheckForModifier(Keys.LeftShift, KeyModifiers.Shift);
            CheckForModifier(Keys.RightShift, KeyModifiers.Shift);
        }

        public GenericKeyState(HashSet<KeyModifiers> modifiers, IEnumerable<GenericKey> pressed)
        {
            Modifiers = modifiers;
            Pressed = new HashSet<GenericKey>(pressed);
        }

        public HashSet<Keybind> PressedKeybinds()
        {
            var set = new HashSet<Keybind>();
            foreach (var key in Pressed) set.Add(new Keybind(Modifiers, key));
            return set;
        }

        public HashSet<Keybind> UniqueKeyPresses(GenericKeyState previousState)
        {
            var newKeyPresses = Pressed.Except(previousState.Pressed);
            var uniqueKeyState = new GenericKeyState(Modifiers, newKeyPresses);
            return uniqueKeyState.PressedKeybinds();
        }

        private void CheckForModifier(Keys key, KeyModifiers modifier)
        {
            var genericKey = new GenericKey() {KeyboardKey = key};
            if (Pressed.Contains(genericKey))
            {
                Pressed.Remove(genericKey);
                Modifiers.Add(modifier);
            }
        }
    }
}