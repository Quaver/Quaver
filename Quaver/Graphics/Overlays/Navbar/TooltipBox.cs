using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Quaver.GameState;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Graphics.UniversalDim;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.States;
using Container = Quaver.Graphics.Sprites.Container;

namespace Quaver.Graphics.Overlays.Navbar
{
    internal class TooltipBox : Sprite
    {
        /// <summary>
        ///     The container for the entire tooltip box.
        /// </summary>
        internal Container Container { get; }

        /// <summary>
        ///     Reference to the navbar sprite that this tooltip box is in accordance with.
        /// </summary>
        internal Sprite Nav{ get; }

        /// <summary>
        ///     The surrounding container box for the tooltip itself.
        /// </summary>
        internal Sprite ContainerBox { get; set; }

        /// <summary>
        ///     The icon displayed in the tooltip box.
        /// </summary>
        internal Sprite Icon { get; set; }

        /// <summary>
        ///     The name of currently highlighted button
        /// </summary>
        internal QuaverSpriteText Name { get; set; }

        /// <summary>
        ///     The description of the currently highlighted button.
        /// </summary>
        internal QuaverSpriteText Description { get; set; }

        /// <summary>
        ///     Dictates if the tooltip box is currently in an animation.
        /// </summary>
        internal bool InAnimation { get; set; }
        
        /// <summary>
        ///     Dictates if the box is entering the screen. If false, that means it's exiting.
        ///     This allows us to keep track of which way to set the tooltip box to go
        ///     during the animation.
        /// </summary>
        internal bool IsEnteringScreen { get; set; }
        
        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="nav"></param>
        internal TooltipBox(Container container, Sprite nav)
        {
            Container = container;
            Nav = nav;
            Parent = Container;
            
            // Create the tooltip box.
            ContainerBox = new Sprite()
            {
                Position = new UDim2D(-50, Nav.SizeY),
                Size = new UDim2D(0, 60, 0.250f, 0),
                Alignment = Alignment.TopLeft,
                Tint = new Color(0f, 0f, 0f, 0.1f),
                Parent = Container,
                Visible = false
            };
            
            // The x and y positions of the top line of the tooltip.
            const int tooltipTopLineX = 10;
            const int tooltipTopLineY = 5;
            
            // Create the icon sprite (displayed to the left of the text.)
            Icon = new Sprite()
            {
                Image = GameBase.QuaverUserInterface.BlankBox,
                Position = new UDim2D(tooltipTopLineX, tooltipTopLineY),
                Parent = ContainerBox
            };
            
            // Set the size of the icon.
            Icon.Size = new UDim2D(Icon.Image.Width, Icon.Image.Height);
            
            // Create Textbox for the name of the button.
            Name = new QuaverSpriteText()
            {
                Text = "",
                Font = QuaverFonts.Medium24,
                Size = new UDim2D(20, 20, 1, 0),
                Position = new UDim2D(tooltipTopLineX + Icon.Image.Width + 10, Icon.Image.Height / 2f - 8),
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.BotLeft,
                TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                TextColor = Color.White,
                Parent = ContainerBox
            };
            
            // Create Textbox for the description of the button.
            Description = new QuaverSpriteText()
            {
                Text = "",
                Font = QuaverFonts.Medium24,
                Size = new UDim2D(15, 16, 1, 0),
                Position = new UDim2D(Name.PosX, Name.PosY + Name.SizeY + 2),
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.BotLeft,
                TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                TextColor = Color.White,
                Parent = ContainerBox
            }; 
        }

        ///     Update
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            // Perform animations for tooltip box.
            if (InAnimation)
            {
                if (IsEnteringScreen)
                    PerformEnterAnimation(dt);
                else
                    PerformExitAnimation(dt);
            }   
            
            base.Update(dt);
        }
        
        /// <summary>
        ///     Performs an animation for the tooltip when the mouse hovers over the button.
        /// </summary>
        /// <param name="dt"></param>
        private void PerformEnterAnimation(double dt)
        {             
            // The original position of the navbar
            const int newPos = 0;
          
            // Don't perform the animation anymore after reaching a certain height.
            if (Math.Abs(ContainerBox.PosX - newPos) < 0.1)
            {
                InAnimation = false;
                return;
            }
            
            ContainerBox.PosX = GraphicsHelper.Tween(newPos, ContainerBox.PosX, Math.Min(dt / 60, 1));
        }
        
        /// <summary>
        ///     Performs an animation for the tooltip when the mouse exits the button.
        /// </summary>
        /// <param name="dt"></param>
        private void PerformExitAnimation(double dt)
        {             
            // The original position of the 
            const int origPos = -400;
          
            // Don't perform the animation anymore after reaching a certain height.
            if (Math.Abs(ContainerBox.PosX - origPos) < 0.1)
            {
                InAnimation = false;
                return;
            }
            
            ContainerBox.PosX = GraphicsHelper.Tween(origPos, ContainerBox.PosX, Math.Min(dt / 60, 1));
        }
    }
}