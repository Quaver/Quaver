using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
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
using Quaver.States.Menu;
using Quaver.States.Options;
using Quaver.States.Select;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;

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
        ///     The container for the navbar
        /// </summary>
        internal QuaverContainer Container { get; set; }
        
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
        ///     The box surrounding the tooltip.
        /// </summary>
        internal QuaverSprite TooltipBox { get; set; }

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
        ///      The icon dislayed when hovering over a navbar button.
        /// </summary>
        internal QuaverSprite TooltipIcon { get; set; }

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

            #region tooltip

            // The box for the tool tip
            TooltipBox = new QuaverSprite()
            {
                PosY = Nav.SizeY,
                Size = new UDim2D(0, 60, 0.30f, 0),
                Alignment = Alignment.TopLeft,
                Tint = new Color(0f, 0f, 0f, 0.1f),
                Parent = Container,
                Visible = false
            };

            // The x and y positions of the top line of the tooltip.
            const int tooltipTopLineX = 10;
            const int tooltipTopLineY = 5;
            
            // The icon for the tooltip.
            TooltipIcon = new QuaverSprite()
            {
                Image = GameBase.QuaverUserInterface.BlankBox, // Set default blank box. Prevents exception.
                Position = new UDim2D(tooltipTopLineX, tooltipTopLineY),
                Parent = TooltipBox
            };
            
            TooltipIcon.Size = new UDim2D(TooltipIcon.Image.Width, TooltipIcon.Image.Height);
                
            // Create tool tip name
            TooltipName = new QuaverTextbox()
            {
                Text = "",
                Font = QuaverFonts.Medium24,
                Size = new UDim2D(25, 25, 1, 0),
                Position = new UDim2D(tooltipTopLineX + TooltipIcon.Image.Width + 10, TooltipIcon.Image.Height / 2f - 8),
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.BotLeft,
                TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                TextColor = Color.White,
                Parent = TooltipBox
            };
            
            // Create tool tip name
            TooltipDescription = new QuaverTextbox()
            {
                Text = "",
                Font = QuaverFonts.Medium24,
                Size = new UDim2D(15, 16, 1, 0),
                Position = new UDim2D(TooltipName.PosX, TooltipName.PosY + TooltipName.SizeY + 2),
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.BotLeft,
                TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                TextColor = Color.White,
                Parent = TooltipBox
            };            

            #endregion

            #region defaultNavButtons
            
            // Replace with actual sprites
            var home = CreateNavbarButton(NavbarAlignment.Left, GameBase.QuaverUserInterface.BlankBox, "Home", "Go to the main menu.", OnHomeButtonClicked);         
            var play = CreateNavbarButton(NavbarAlignment.Left, GameBase.QuaverUserInterface.BlankBox, "Play", "Smash some keys!", OnPlayButtonClicked);
            var keys4 = CreateNavbarButton(NavbarAlignment.Left, GameBase.QuaverUserInterface.BlankBox, "4 Keys", "Set your game mode to 4K.", (sender, args) => {});
            var keys7 = CreateNavbarButton(NavbarAlignment.Left, GameBase.QuaverUserInterface.BlankBox, "7 Keys", "Set your game mode to 7K.", (sender, args) => {});
            var quit = CreateNavbarButton(NavbarAlignment.Right, GameBase.QuaverUserInterface.BlankBox, "Exit", "Already? Come back soon! o/", OnExitButtonClicked);
            var settings = CreateNavbarButton(NavbarAlignment.Right, GameBase.QuaverUserInterface.BlankBox, "Settings", "Configure Quaver.", OnSettingsButtonClicked);
            var discord = CreateNavbarButton(NavbarAlignment.Right, GameBase.QuaverUserInterface.BlankBox, "Discord", "https://discord.gg/nJa8VFr", OnDiscordButtonClicked);
            #endregion
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
            GameBase.GameStateManager.ChangeState(new MainMenuState());
        }

        /// <summary>
        ///     Called when the settings button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSettingsButtonClicked(object sender, EventArgs e)
        {
            GameBase.GameStateManager.ChangeState(new OptionsState());
        }

        /// <summary>
        ///     Called when the exit button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExitButtonClicked(object sender, EventArgs e)
        {
            QuaverGame.Quit();
        }
        
        /// <summary>
        ///     Called when the home button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayButtonClicked(object sender, EventArgs e)
        {
            GameBase.GameStateManager.ChangeState(new SongSelectState());
        }

        /// <summary>
        ///     Called when the Discord button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDiscordButtonClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://discord.gg/nJa8VFr");
        }
    }
}