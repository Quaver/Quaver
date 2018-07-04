using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Graphics.Sprites;
using Quaver.Main;

namespace Quaver.Graphics.Overlays.Toolbar
{
    internal class Toolbar : Sprite
    {
        /// <summary>
        ///     The line at the bottom of the toolbar.
        /// </summary>
        private Sprite BottomLine { get; set; }

        /// <summary>
        ///     The buttons displayed on the toolbar.
        /// </summary>
        private List<ToolbarItem> Buttons { get; }

        private List<ToolbarItem> Icons { get; }

        internal Toolbar(List<ToolbarItem> buttons, List<ToolbarItem> icons)
        {
            Buttons = buttons;
            Icons = icons;
            Size = new UDim2D(GameBase.WindowRectangle.Width, 80);
            Tint = Color.Black;
            PosY = 0;
            Alpha = 0f;

            BottomLine = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Tint = Color.White,
                SizeX = SizeX - 160,
                SizeY = 1,
                Alpha = 0.3f,
                PosY = SizeY
            };
            
            InitializeToolbarItems();
            InitializeIcons();
        }

        private void InitializeToolbarItems()
        {
            for (var i = 0; i < Buttons.Count; i++)
            {
                var button = Buttons[i];

                button.Parent = BottomLine;
                button.PosY = -button.SizeY;
                
                if (i != 0)
                    button.PosX = Buttons[i - 1].SizeX * i;
            }
        }

        private void InitializeIcons()
        {
            for (var i = 0; i < Icons.Count; i++)
            {
                var icon = Icons[i];

                icon.Parent = BottomLine;
                icon.PosY = -icon.SizeY;

                icon.PosX = BottomLine.SizeX - icon.SizeX * i - icon.SizeX;
            }
        }
    }
}