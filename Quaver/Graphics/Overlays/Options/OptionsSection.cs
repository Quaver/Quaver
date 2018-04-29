using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics.Buttons;

namespace Quaver.Graphics.Overlays.Options
{
    internal class OptionsSection
    {        
        /// <summary>
        ///     The stringified name of the section.
        /// </summary>
        internal string Name { get; }

        /// <summary>
        ///     The icon displayed for this section.
        /// </summary>
        internal Texture2D Icon { get; }

        /// <summary>
        ///     The button displayed in the menu bar for this section.
        /// </summary>
        internal QuaverTextButton MenuBarButton { get; set; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        internal OptionsSection(string name, Texture2D icon)
        {
            Name = name;
            Icon = icon;
        }
    }
}