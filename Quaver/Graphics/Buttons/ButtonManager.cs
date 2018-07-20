using System.Collections.Generic;

namespace Quaver.Graphics.Buttons
{
    internal static class ButtonManager
    {
        internal static List<Button> Buttons { get; } = new List<Button>();

        internal static void Add(Button btn) => Buttons.Add(btn);

        internal static void Remove(Button btn) => Buttons.Remove(btn);
    }
}