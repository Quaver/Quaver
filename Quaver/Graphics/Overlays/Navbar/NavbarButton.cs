using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Enums;
using Quaver.Graphics.UniversalDim;

namespace Quaver.Graphics.Overlays.Navbar
{
    internal class NavbarButton : QuaverButton
    {
        /// <summary>
        ///     The name of name of the tooltip when you hover over the button
        /// </summary>
        internal string TooltipName { get; }

        /// <summary>
        ///     The description of the button when you hover over the button.
        /// </summary>
        internal string TooltipDescription { get; }

        /// <summary>
        ///     The parent navbar for this button
        /// </summary>
        private Navbar Container { get; }

        /// <summary>
        ///     Initializes the navbar button
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="tex"></param>
        /// <param name="tooltipName"></param>
        /// <param name="tooltipDesc"></param>
        internal NavbarButton(Navbar nav, Texture2D tex, string tooltipName, string tooltipDesc)
        {
            Container = nav;
            TooltipName = tooltipName;
            TooltipDescription = tooltipDesc;
            Image = tex;
            Size = new UDim2D(tex.Width, tex.Height);
            Alignment = Alignment.TopLeft;

            // Set the button's offset based on the size/pos of the last button.
            var lastButton = Container.Nav.Children.Count > 0 ? Container.Nav.Children.Last() : null;
            var buttonOffset = lastButton?.PosX + lastButton?.SizeX + 20 ?? 20;
            
            Position = new UDim2D(Container.Nav.SizeX + buttonOffset, Container.Nav.SizeY / 2 - tex.Height / 2f);
        }
        
         /// <inheritdoc />
        /// <summary>
        ///     When the user's mouse goes over the button.
        /// </summary>
        internal override void MouseOver()
        {
        }

         /// <inheritdoc />
         /// <summary>
         ///   When the user's mouse goes out of the button.  
         /// </summary>
        internal override void MouseOut()
        {
        }
    }
}