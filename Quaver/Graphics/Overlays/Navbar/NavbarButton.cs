using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics.Buttons;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.Graphics.Overlays.Navbar
{
    internal class NavbarButton : Button
    {
        /// <summary>
        ///     The name of name of the tooltip when you hover over the button
        /// </summary>
        internal string TooltipName { get; set; }

        /// <summary>
        ///     The description of the button when you hover over the button.
        /// </summary>
        internal string TooltipDescription { get; set; }

        /// <summary>
        ///     The parent navbar for this button
        /// </summary>
        internal Nav ParentNav { get; }

        /// <summary>
        ///     The x position spacing between each navbar button.
        /// </summary>
        private const int ButtonSpacing = 35;

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
        private int IconWidth { get; } = 23;
        
        /// <summary>
        ///     The height of the button icon.
        /// </summary>
        private int IconHeight { get; } = 23;

        /// <summary>
        ///     Dictates whether the MouseOver sound has already been played for this button.
        /// </summary>
        private bool MouseOverSoundPlayed { get; set; }

        /// <summary>
        ///     The color of the icon when hovering over it.
        /// </summary>
        private static Color MouseOverColor { get; } = new Color(165, 223, 255);

        /// <summary>
        ///     The color of the icon when not hovering.
        /// </summary>
        private static Color MouseOutColor { get; } = new Color(255, 255, 255);

        /// <inheritdoc />
        /// <summary>
        ///     Initializes the navbar button
        /// </summary>
        /// <param name="nav"></param>
        /// <param name="tex"></param>
        /// <param name="alignment"></param>
        /// <param name="tooltipName"></param>
        /// <param name="tooltipDesc"></param>
        internal NavbarButton(Nav nav, Texture2D tex, NavbarAlignment alignment, string tooltipName, string tooltipDesc, EventHandler action) 
            : base(action)
        {
            NavAlignment = alignment;
            ParentNav = nav;
            TooltipName = tooltipName;
            TooltipDescription = tooltipDesc;
            Image = tex;
            Size = new UDim2D(IconWidth, IconHeight);
            Tint = MouseOutColor;
         
            // Get the last button in the list of the current alignment.
            var lastButton = ParentNav.Buttons[alignment].Count > 0 ? ParentNav.Buttons[alignment].Last() : null;
         
            // Set the alignment and position of the navbar button.
            switch (NavAlignment)
            {
                case NavbarAlignment.Left:
                    Alignment = Alignment.TopLeft;
                    PositionX = ParentNav.NavbarSprite.SizeX + lastButton?.PosX + lastButton?.SizeX + ButtonSpacing ?? ButtonSpacing;
                    break;
                case NavbarAlignment.Right:
                    Alignment = Alignment.TopRight;
                    PositionX = ParentNav.NavbarSprite.SizeX + lastButton?.PosX - lastButton?.SizeX - ButtonSpacing ?? -ButtonSpacing;
                    break;
                default:
                    throw new InvalidEnumArgumentException("Invalid NavbarAlignment given.");
            }
            
            Position = new UDim2D(PositionX, ParentNav.NavbarSprite.SizeY / 2 - IconHeight / 2f);
        }

         /// <inheritdoc />
        /// <summary>
        ///     When the user's mouse goes over the button.
        /// </summary>
         protected override void MouseOver()
         {
             ParentNav.TooltipBox.Name.Text = TooltipName;
             ParentNav.TooltipBox.Description.Text = TooltipDescription;
             ParentNav.TooltipBox.Icon.Image = Image;
             Tint = MouseOverColor;
              
             // The scale at which the image increases when moused over.
             const float scale = 1.15f;
             
             // Increase the size and normalize the position.
             Size = new UDim2D(IconWidth * scale, IconHeight * scale);
             Position = new UDim2D(PositionX, ParentNav.NavbarSprite.SizeY / 2 - IconHeight * scale / 2f);
             
             // Make tooltip box visible if it isn't already
             ParentNav.TooltipBox.InAnimation = true;
             ParentNav.TooltipBox.ContainerBox.Visible = true;
             ParentNav.TooltipBox.IsEnteringScreen = true;

             // Play sound effect if necessary
             if (!MouseOverSoundPlayed)
             {
                 GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundHover);
                 MouseOverSoundPlayed = true;
             }
        }

         /// <inheritdoc />
         /// <summary>
         ///   When the user's mouse goes out of the button.  
         /// </summary>
         protected override void MouseOut()
         {
             ParentNav.TooltipBox.Name.Text = string.Empty;
             ParentNav.TooltipBox.Description.Text = string.Empty;
             Tint = MouseOutColor;
             
             // Set the size and position back to normal.
             Size = new UDim2D(IconWidth, IconHeight);
             Position = new UDim2D(PositionX, ParentNav.NavbarSprite.SizeY / 2 - IconHeight / 2f);

             if (ParentNav.TooltipBox.ContainerBox.Visible)
             {
                 ParentNav.TooltipBox.InAnimation = true;
                 ParentNav.TooltipBox.IsEnteringScreen = false;
             }

             // Reset MouseOverSoundPlayed for this particular button now that we've moused out.
             MouseOverSoundPlayed = false;
         }
    }
}