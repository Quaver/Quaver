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
        ///     The current x position of the button.
        /// </summary>
        private float PositionX { get; set; }

        /// <summary>
        ///     The alignment of the button in the navbar (left, right);
        /// </summary>
        private NavbarAlignment NavAlignment { get; }

        /// <summary>
        ///     Initializes the navbar button
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="tex"></param>
        /// <param name="alignment"></param>
        /// <param name="tooltipName"></param>
        /// <param name="tooltipDesc"></param>
        internal NavbarButton(Navbar nav, Texture2D tex, NavbarAlignment alignment, string tooltipName, string tooltipDesc, EventHandler action) 
            : base(action)
        {
            NavAlignment = alignment;
            Container = nav;
            TooltipName = tooltipName;
            TooltipDescription = tooltipDesc;
            Image = tex;
            Size = new UDim2D(Image.Width, Image.Height);
         
            // Get the last button in the list of the current alignment.
            var lastButton = Container.Buttons[alignment].Count > 0 ? Container.Buttons[alignment].Last() : null;
         
            // Set the alignment and position of the navbar button.
            switch (NavAlignment)
            {
                case NavbarAlignment.Left:
                    Alignment = Alignment.TopLeft;
                    PositionX = Container.Nav.SizeX + lastButton?.PosX + lastButton?.SizeX + ButtonSpacing ?? ButtonSpacing;
                    break;
                case NavbarAlignment.Right:
                    Alignment = Alignment.TopRight;
                    PositionX = Container.Nav.SizeX + lastButton?.PosX - lastButton?.SizeX - ButtonSpacing ?? -ButtonSpacing;
                    break;
                default:
                    throw new InvalidEnumArgumentException("Invalid NavbarAlignment given.");
            }
            
            Position = new UDim2D(PositionX, Container.Nav.SizeY / 2 - Image.Height / 2f);
        }
        
         /// <inheritdoc />
        /// <summary>
        ///     When the user's mouse goes over the button.
        /// </summary>
         protected override void MouseOver()
         {
             Container.HoveredButton = this;
             Container.TooltipName.Text = TooltipName;
             Container.TooltipDescription.Text = TooltipDescription;
             Tint = Color.Yellow;
                      
             // The scale at which the image increases when moused over.
             const float scale = 1.15f;
             
             // Increase the size and normalize the position.
             Size = new UDim2D(Image.Width * scale, Image.Height * scale);
             Position = new UDim2D(PositionX, Container.Nav.SizeY / 2 - Image.Height * scale / 2f);
             
             // Make tooltip box visible if it isn't already
             if (!Container.TooltipBox.Visible)
             {
                 // TODO: Perform FadeIn animation
                 Container.TooltipBox.Visible = true;
             }
         }

         /// <inheritdoc />
         /// <summary>
         ///   When the user's mouse goes out of the button.  
         /// </summary>
         protected override void MouseOut()
         {
             Container.TooltipName.Text = string.Empty;
             Container.TooltipDescription.Text = string.Empty;
             Container.HoveredButton = null;
             Tint = Color.White;
             
             // Set the size and position back to normal.
             Size = new UDim2D(Image.Width, Image.Height);
             Position = new UDim2D(PositionX, Container.Nav.SizeY / 2 - Image.Height / 2f);

             if (Container.TooltipBox.Visible)
             {
                 // TODO: Perform FadeOut animation
                 Container.TooltipBox.Visible = false;
             }
         }
    }
}