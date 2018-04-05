using System;
using System.ComponentModel;
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
        ///     The x position spacing between each navbar button.
        /// </summary>
        private const int ButtonSpacing = 20;

        /// <summary>
        ///     Initializes the navbar button
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="tex"></param>
        /// <param name="alignment"></param>
        /// <param name="tooltipName"></param>
        /// <param name="tooltipDesc"></param>
        internal NavbarButton(Navbar nav, Texture2D tex, NavbarAlignment alignment, string tooltipName, string tooltipDesc)
        {
            Container = nav;
            TooltipName = tooltipName;
            TooltipDescription = tooltipDesc;
            Image = tex;
            Size = new UDim2D(Image.Width, Image.Height);
         
            // Get the last button in the list of the current alignment.
            var lastButton = Container.Buttons[alignment].Count > 0 ? Container.Buttons[alignment].Last() : null;
         
            // Set the alignment and position of the navbar button.
            float buttonX;
            switch (alignment)
            {
                case NavbarAlignment.Left:
                    Alignment = Alignment.TopLeft;
                    buttonX = Container.Nav.SizeX + lastButton?.PosX + lastButton?.SizeX + ButtonSpacing ?? ButtonSpacing;
                    break;
                case NavbarAlignment.Right:
                    Alignment = Alignment.TopRight;
                    buttonX = Container.Nav.SizeX + lastButton?.PosX - lastButton?.SizeX - ButtonSpacing ?? -ButtonSpacing;
                    break;
                default:
                    throw new InvalidEnumArgumentException("Invalid NavbarAlignment given.");
            }
            
            Position = new UDim2D(buttonX, Container.Nav.SizeY / 2 - Image.Height / 2f);
        }
        
         /// <inheritdoc />
        /// <summary>
        ///     When the user's mouse goes over the button.
        /// </summary>
        internal override void MouseOver()
         {
             Tint = Color.Yellow;
         }

         /// <inheritdoc />
         /// <summary>
         ///   When the user's mouse goes out of the button.  
         /// </summary>
        internal override void MouseOut()
         {
             Tint = Color.White;
         }
    }
}