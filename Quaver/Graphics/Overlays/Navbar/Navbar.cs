using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.GameState;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Graphics.UniversalDim;
using Quaver.Helpers;
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
        internal QuaverContainer Container { get; set; }

        /// <summary>
        ///     If the navbar is shown
        /// </summary>
        private bool IsShown { get; set; }

        /// <summary>
        ///     If the navbar is currently in an animation
        /// </summary>
        private bool InAnimation { get; set; }

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
                Tint = new Color(0f, 0f, 0f, 0.35f),
                Parent = Container
            };

            // Create tool tip name
            TooltipName = new QuaverTextbox()
            {
                Text = "",
                Font = QuaverFonts.Medium24,
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
                Font = QuaverFonts.Medium24,
                Size = new UDim2D(15, 15, 1, 0),
                Position = new UDim2D(20, 90),
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.BotLeft,
                TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                TextColor = Color.White,
                Parent = Container
            };
            
            // Replace with actual sprites
            var home = CreateNavbarButton(NavbarAlignment.Left, GameBase.QuaverUserInterface.BlankBox, "Home", "Go to main menu", OnHomeButtonClicked);         
            var play = CreateNavbarButton(NavbarAlignment.Left, GameBase.QuaverUserInterface.BlankBox, "Play", "Play Quaver", OnPlayButtonClicked);
            var keys4 = CreateNavbarButton(NavbarAlignment.Left, GameBase.QuaverUserInterface.BlankBox, "Game Mode: 4 Keys", "Change your selected game mode to 4K", OnPlayButtonClicked);
            var keys7 = CreateNavbarButton(NavbarAlignment.Left, GameBase.QuaverUserInterface.BlankBox, "Game Mode: 7 Keys", "Change your selected game mode to 7K", OnPlayButtonClicked);
            var quit = CreateNavbarButton(NavbarAlignment.Right, GameBase.QuaverUserInterface.BlankBox, "Quit", "Bye Bye.", OnPlayButtonClicked);
            var settings = CreateNavbarButton(NavbarAlignment.Right, GameBase.QuaverUserInterface.BlankBox, "Settings", "Configure the game.", OnPlayButtonClicked);
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
            if (GameBase.KeyboardState.IsKeyDown(Keys.Z))
                PerformHideAnimation(dt);
            
            if (GameBase.KeyboardState.IsKeyDown(Keys.X))
                PerformShowAnimation(dt);
            
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
        /// <param name="clickAction"></param>
        private NavbarButton CreateNavbarButton(NavbarAlignment alignment, Texture2D tex, string name, string description, EventHandler clickAction)
        {       
            var button = new NavbarButton(this, tex, alignment, name, description, clickAction) { Parent = Container };   
            Buttons[alignment].Add(button);
            return button;
        }

        /// <summary>
        ///     Peforms an animation which hides the navbar.
        /// </summary>
        /// <param name="dt"></param>
        private void PerformHideAnimation(double dt)
        {
            // The position in which the navbar is considered hidden.
            const float hiddenPos = -50f;

            // Don't perform the animation anymore after reaching a certain height.
            if (Math.Abs(Container.PosY - hiddenPos) < 0.1)
            {
                IsShown = false;
                InAnimation = false;
                Container.Visible = false;
                return;
            }
            
            Container.PosY = GraphicsHelper.Tween(hiddenPos, Container.PosY, Math.Min(dt / 30, 1));
        }

         /// <summary>
        ///     Performs an animation which shows the navbar.
        /// </summary>
        /// <param name="dt"></param>
        private void PerformShowAnimation(double dt)
         {
             // Make the container visible again when performing this animation.
             Container.Visible = true;
             
            // The original position of the navbar
            const int origPos = 0;
          
            // Don't perform the animation anymore after reaching a certain height.
            if (Math.Abs(Container.PosY - origPos) < 0.1)
            {
                IsShown = true;
                InAnimation = false;
                return;
            }
            
            Container.PosY = GraphicsHelper.Tween(origPos, Container.PosY, Math.Min(dt / 30, 1));
        }
        
        /// <summary>
        ///     Called when the home button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHomeButtonClicked(object sender, EventArgs e)
        {
            Console.WriteLine("HOME BUTTON CLICKED");
        }
        
        /// <summary>
        ///     Called when the home button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayButtonClicked(object sender, EventArgs e)
        {
            Console.WriteLine("PLAY BUTTON CLICKED");
        }
    }
}