using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Enums;
using Quaver.Graphics.UniversalDim;
using Quaver.Helpers;

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
        private const int ButtonSpacing = 25;

        /// <summary>
        ///     The current x position of the button.
        /// </summary>
        private float PositionX { get; set; }

        /// <summary>
        ///     The alignment of the button in the navbar (left, right);
        /// </summary>
        private NavbarAlignment NavAlignment { get; }

        /// <summary>
        ///     The width of the button icon
        /// </summary>
        private int IconWidth { get; } = 22;
        
        /// <summary>
        ///     The height of the button icon.
        /// </summary>
        private int IconHeight { get; } = 22;

        /// <summary>
        ///     The color of the icon when hovering over it.
        /// </summary>
        internal static Color MouseOverColor { get; } = new Color(124, 224, 255);

        /// <summary>
        ///     The color of the icon when not hovering.
        /// </summary>
        internal static Color MouseOutColor { get; } = new Color(255, 255, 255);

        /// <inheritdoc />
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
            Size = new UDim2D(IconWidth, IconHeight);
            Tint = MouseOutColor;
         
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
            
            Position = new UDim2D(PositionX, Container.Nav.SizeY / 2 - IconHeight / 2f);
        }

        /// <summary>
        ///     Update - Handles animations and stuff for the button/tooltipbox.
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            // Perform animations for tooltip box.
            if (Container.TooltipBoxInAnimation)
            {
                if (Container.ToolTipBoxEntering)
                    PerformTooltipEnterAnimation(dt);
                else
                    PerformTooltipExitAnimation(dt);
            }
          
            base.Update(dt);
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
             Container.TooltipIcon.Image = Image;
             Tint = MouseOverColor;
              
             // The scale at which the image increases when moused over.
             const float scale = 1.15f;
             
             // Increase the size and normalize the position.
             Size = new UDim2D(IconWidth * scale, IconHeight * scale);
             Position = new UDim2D(PositionX, Container.Nav.SizeY / 2 - IconHeight * scale / 2f);
             
             // Make tooltip box visible if it isn't already
             Container.TooltipBoxInAnimation = true;
             Container.TooltipBox.Visible = true;
             Container.ToolTipBoxEntering = true;
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
             Tint = MouseOutColor;
             
             // Set the size and position back to normal.
             Size = new UDim2D(IconWidth, IconHeight);
             Position = new UDim2D(PositionX, Container.Nav.SizeY / 2 - IconHeight / 2f);

             Container.TooltipBox.PosX = -50;
             
             if (Container.TooltipBox.Visible)
             {
                 // TODO: Perform FadeOut animation
                 Container.TooltipBoxInAnimation = true;
                 Container.ToolTipBoxEntering = false;
             }
         }

        /// <summary>
        ///     Performs an animation for the tooltip when the mouse hovers over the button.
        /// </summary>
        /// <param name="dt"></param>
        private void PerformTooltipEnterAnimation(double dt)
        {             
            // The original position of the navbar
            const int newPos = 0;
          
            // Don't perform the animation anymore after reaching a certain height.
            if (Math.Abs(Container.TooltipBox.PosX - newPos) < 0.1)
            {
                Container.TooltipBoxInAnimation = false;
                return;
            }
            
            Container.TooltipBox.PosX = GraphicsHelper.Tween(newPos, Container.TooltipBox.PosX, Math.Min(dt / 120, 1));
        }
        
        /// <summary>
        ///     Performs an animation for the tooltip when the mouse exits the button.
        /// </summary>
        /// <param name="dt"></param>
        private void PerformTooltipExitAnimation(double dt)
        {             
            // The original position of the 
            const int origPos = -400;
          
            // Don't perform the animation anymore after reaching a certain height.
            if (Math.Abs(Container.TooltipBox.PosX - origPos) < 0.1)
            {
                Container.TooltipBoxInAnimation = false;
                return;
            }
            
            Container.TooltipBox.PosX = GraphicsHelper.Tween(origPos, Container.TooltipBox.PosX, Math.Min(dt / 240, 1));
        }
    }
}