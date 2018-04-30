using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;

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
        ///     The container of the options section.
        /// </summary>
        internal QuaverSprite Container { get; set; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        internal OptionsSection(Drawable overlay, string name, Texture2D icon)
        {
            Name = name;
            Icon = icon;
            
            Container = new QuaverSprite()
            {
                Parent = overlay,               
                Size = new UDim2D(800, 450),
                Alignment = Alignment.MidCenter,
                PosY = 90,
                Tint = new Color(0f, 0f, 0f, 1f),
                Visible = false
            };        
        }
    }
}