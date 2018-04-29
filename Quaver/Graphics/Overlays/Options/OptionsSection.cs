using Microsoft.Xna.Framework.Graphics;

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
        ///     Ctor - 
        /// </summary>
        internal OptionsSection(string name, Texture2D icon)
        {
            Name = name;
            Icon = icon;
        }
    }
}