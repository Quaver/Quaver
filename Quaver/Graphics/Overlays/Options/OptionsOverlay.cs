using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Quaver.GameState;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Colors;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Graphics.UniversalDim;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.States;
using Quaver.States.Options;
using Steamworks;

namespace Quaver.Graphics.Overlays.Options
{
    internal class OptionsOverlay : QuaverSprite
    {
        /// <summary>
        ///     If the options overlay is currently active.
        /// </summary>
        internal bool Active { get; set; }

    #region HEADER
        
        /// <summary>
        ///     Container for the header.
        /// </summary>
        private QuaverSprite HeaderContainer { get; set; }

        /// <summary>
        ///     The header's title.
        /// </summary>
        private QuaverTextbox HeaderTitle { get; set; }
        
        /// <summary>
        ///     Reference to the header's icon.
        /// </summary>
        private QuaverSprite HeaderIcon { get; set; }

        /// <summary>
        ///     The description text 
        /// </summary>
        private QuaverTextbox HeaderDescription { get; set; }

        /// <summary>
        ///     The line displayed under the header.
        /// </summary>
        private QuaverSprite HeaderUnderline { get; set; }

    #endregion

    #region MENU_BAR

        /// <summary>
        ///     The container for the menu bar.
        /// </summary>
        private QuaverSprite MenuBarContainer { get; set; }

        /// <summary>
        ///     The button to go to the section to the left. (left-facing chevron.)
        /// </summary>
        private QuaverButton MenuBarLeftButton { get; set; }

        /// <summary>
        ///     The button to go to the section to the right. (right-facing chevron)
        /// </summary>
        private QuaverButton MenuBarRightButton { get; set; }

    #endregion
        
        /// <summary>
       ///     Ctor - 
       /// </summary>
        internal OptionsOverlay()
        {
            Alignment = Alignment.TopLeft;
            Tint = new Color(0f, 0f, 0f, 0.6f);
            Size = new UDim2D(0, GameBase.WindowRectangle.Height, 1);
            PosY = GameBase.WindowRectangle.Height;
            
            // Create the entire header's UI.
            CreateHeader(); 
            
            // Create the menu bar.
            CreateMenuBar();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            PerformShowAndHideAnimations(dt);
            UpdateHeader(dt);
            
            base.Update(dt);
        }

        /// <summary>
       ///     Performs a show and hide animation on the overlay
       /// </summary>
       /// <param name="dt"></param>
        private void PerformShowAndHideAnimations(double dt)
        {
            if (Active)
                PosY = GraphicsHelper.Tween(0, PosY, Math.Min(dt / 30, 1));
            else
                PosY = GraphicsHelper.Tween(GameBase.WindowRectangle.Height, PosY, Math.Min(dt / 30, 1));
        }

    #region HEADER_METHODS

        /// <summary>
       ///     Creates the header UI.
       /// </summary>
        private void CreateHeader()
        {
            // Container for this header.
            HeaderContainer = new QuaverSprite()
            {
                Position = new UDim2D(0, Nav.Height),
                SizeX = 300,
                SizeY = 150,
                Alignment = Alignment.TopCenter,
                Tint = new Color(0f, 0f, 0f, 0f),
                Parent = this,
                Visible = true
            };
            
            // Header Title.
            HeaderTitle = new QuaverTextbox()
            {
                Text = "Settings",
                Font = QuaverFonts.Medium24,
                Size = new UDim2D(30, 30, 1, 0),
                PosX = HeaderContainer.SizeX / 2f - 25,
                PosY = 20,
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.BotLeft,
                TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                TextColor = Color.White,
                Parent = HeaderContainer
            };
            
            // Header icon
            HeaderIcon = new QuaverSprite()
            {
                Parent = HeaderContainer,
                Alignment = Alignment.TopLeft,
                Image = FontAwesome.Cog,
                SizeX = 30,
                SizeY = 30,
                PosY = HeaderTitle.PosY,
                PosX = HeaderTitle.PosX - 40
            };
            
            // Header Description.
            HeaderDescription = new QuaverTextbox()
            {
                Text = "Change the way Quaver looks, sounds, feels... and tastes?",
                Font = QuaverFonts.Medium24,
                Size = new UDim2D(100, 100, 1, 0),
                PosY = HeaderTitle.PosY + 40,
                Alignment = Alignment.TopCenter,
                TextAlignment = Alignment.TopCenter,
                TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                TextColor = QuaverColors.MainAccent,
                Parent = HeaderContainer
            };
            
            // Add a line under the header description
            HeaderUnderline = new QuaverSprite()
            {
                Parent = HeaderContainer,
                Tint = Color.White,
                SizeY = 1f,
                SizeX = 200,
                PosY = HeaderDescription.PosY + 40,
                Alignment = Alignment.TopCenter
            };
        }

       /// <summary>
       ///     Updates the header. Applies any sort of animations that we want to do.
       /// </summary>
        private void UpdateHeader(double dt)
        {
            if (!Active)
                return;
            
            // Rotate the gear icon, just for some extra oomph
            HeaderIcon.Rotation = (float)(MathHelper.ToDegrees(HeaderIcon.Rotation) + 7 * dt / 30f);
        }

    #endregion

    #region MENU_BAR_METHODS

        /// <summary>
        ///     Creates the actual menu bar.
        /// </summary>
        private void CreateMenuBar()
        {
            // Create container.
            MenuBarContainer = new QuaverSprite()
            {
                Position = new UDim2D(0, HeaderUnderline.PosY + 60),
                Size = new UDim2D(800, 60),
                Alignment = Alignment.TopCenter,
                Tint = new Color(0f, 0f, 0f, 1f),
                Parent = this,
                Visible = true
            };
            
            // Create the left menu bar button (chevron)
            MenuBarLeftButton = new QuaverBasicButton()
            {
                Parent = MenuBarContainer,
                Image = FontAwesome.ChevronDown,
                Rotation = 90,
                Size = new UDim2D(20, 20),
                PosX = 5,
                Alignment = Alignment.MidLeft
            };
            
            // Create the left menu bar button (chevron)
            MenuBarRightButton = new QuaverBasicButton()
            {
                Parent = MenuBarContainer,
                Image = FontAwesome.ChevronDown,
                Rotation = 270,
                Size = new UDim2D(20, 20),
                PosX = -5,
                Alignment = Alignment.MidRight
            }; 
        }

    #endregion
    }
}