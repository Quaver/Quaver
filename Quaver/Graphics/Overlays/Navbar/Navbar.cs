using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.GameState;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Graphics.UniversalDim;
using Quaver.Main;
using Quaver.States;

namespace Quaver.Graphics.Overlays.Navbar
{
    /// <summary>
    ///     A navbar overlay
    /// </summary>
    internal class Navbar : IGameStateComponent
    {
        /// <summary>
        ///     The actual navbar sprite
        /// </summary>
        internal QuaverSprite Nav { get; set; }

         /// <summary>
        ///     The navbar buttons that are currently implemented on this navbar with their assigned
        ///     alignments to the navbar.
        /// </summary>
        internal Dictionary<NavbarAlignment, List<NavbarButton>> Buttons { get; set; }
  
         /// <summary>
        ///     The currently hovered button.
        /// </summary>
        internal NavbarButton HoveredButton { get; set; }

         /// <summary>
        ///     When the button is hovered, it'll display the tooltip's name.
        ///     This text field holds that sprite.
        /// </summary>
         internal QuaverTextbox TooltipName { get; set; }

         /// <summary>
        ///     When the button is hovered, it'll display the tooltip's description.
        ///     This text field holds that sprite.
        /// </summary>
        internal QuaverTextbox TooltipDescription { get; set; }

        /// <summary>
        ///     The container for the navbar
        /// </summary>
        private QuaverContainer Container { get; set; }

         /// <summary>
        ///     Initialize
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(IGameState state)
        {
            Container = new QuaverContainer();
            
            // Setup the dictionary of navbar buttons.
            Buttons = new Dictionary<NavbarAlignment, List<NavbarButton>>()
            {
                { NavbarAlignment.Left, new List<NavbarButton>() },
                { NavbarAlignment.Right, new List<NavbarButton>() }
            };
            
            // Create navbar
            Nav = new QuaverSprite()
            {
                Size = new UDim2D(0, 50, 1, 0),
                Alignment = Alignment.TopLeft,
                Tint = new Color(0f, 0f, 0f, 0.5f),
                Parent = Container
            };

            // Create tool tip name
            TooltipName = new QuaverTextbox()
            {
                Text = "",
                Font = Fonts.Medium24,
                Size = new UDim2D(25, 25, 1, 0),
                Position = new UDim2D(20, 60),
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.BotLeft,
                TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                TextColor = Color.White,
                Parent = Container
            };
            
            // Create tool tip name
            TooltipDescription = new QuaverTextbox()
            {
                Text = "",
                Font = Fonts.Medium24,
                Size = new UDim2D(15, 15, 1, 0),
                Position = new UDim2D(20, 90),
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.BotLeft,
                TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                TextColor = Color.White,
                Parent = Container
            };
            
            // Replace with actual sprites
            var home = CreateNavbarButton(NavbarAlignment.Right, GameBase.QuaverUserInterface.BlankBox, "Home", "Go to main menu");
            home.Clicked += (sender, args) => Console.WriteLine("HOME BUTTON CLICKED");
            
            var home2 = CreateNavbarButton(NavbarAlignment.Right, GameBase.QuaverUserInterface.BlankBox, "Home", "Go to main menu");
            home2.Clicked += (sender, args) => Console.WriteLine("HOME BUTTON CLICKED");
            
            var play = CreateNavbarButton(NavbarAlignment.Left, GameBase.QuaverUserInterface.BlankBox, "Play", "Play Quaver");
            play.Clicked += (sender, args) => Console.WriteLine("PLAY BUTTON CLICKED");
        }

         /// <summary>
        ///     Unload
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void UnloadContent()
        {
            Container.Destroy();
        }

        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            Container.Update(dt);
        }

        /// <summary>
        ///     Draw
        /// </summary>
        public void Draw()
        {
            Container.Draw();
        }

        /// <summary>
        ///     Adds a button to the navbar with the correct alignment.
        ///     - USE THIS WHEN ADDING NAVBAR BUTTONS, as it does all the initialization for you.
        /// </summary>
        /// <param name="alignment"></param>
        /// <param name="tex"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        private NavbarButton CreateNavbarButton(NavbarAlignment alignment, Texture2D tex, string name, string description)
        {
            var button = new NavbarButton(this, tex, alignment, name, description)
            {
                Parent = Container
            };
            
            Buttons[alignment].Add(button);

            return button;
        }
    }
}