using Microsoft.Xna.Framework;
using Quaver.Graphics.Base;
using Quaver.Graphics.Colors;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Graphics.UniversalDim;
using Quaver.Main;

namespace Quaver.Graphics.Overlays.Options
{
    internal class OptionsHeader
    {
        /// <summary>
        ///     Container for the header.
        /// </summary>
        internal QuaverSprite Container { get; set; }

        /// <summary>
        ///     The header's title.
        /// </summary>
        internal QuaverTextbox Title { get; set; }
        
        /// <summary>
        ///     Reference to the header's icon.
        /// </summary>
        internal QuaverSprite Icon { get; set; }

        /// <summary>
        ///     The description text 
        /// </summary>
        internal QuaverTextbox Description { get; set; }

        /// <summary>
        ///     The line displayed under the header.
        /// </summary>
        internal QuaverSprite Underline { get; set; }

        /// <summary>
        ///     Reference to the actual overlay.
        /// </summary>
        private OptionsOverlay Overlay { get; set; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="???"></param>
        /// <param name="overlay"></param>
        internal OptionsHeader(OptionsOverlay overlay)
        {
            Overlay = overlay;
            
            // Container for this header.
            Container = new QuaverSprite()
            {
                Position = new UDim2D(0, Nav.Height),
                SizeX = 300,
                SizeY = 150,
                Alignment = Alignment.TopCenter,
                Tint = new Color(0f, 0f, 0f, 0f),
                Parent = overlay,
                Visible = true
            };
            
            // Header Title.
            Title = new QuaverTextbox()
            {
                Text = "Settings",
                Font = QuaverFonts.Medium24,
                Size = new UDim2D(30, 30, 1, 0),
                PosX = Container.SizeX / 2f - 25,
                PosY = 20,
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.BotLeft,
                TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                TextColor = Color.White,
                Parent = Container
            };
            
            // Header icon
            Icon = new QuaverSprite()
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                Image = FontAwesome.Cog,
                SizeX = 30,
                SizeY = 30,
                PosY = Title.PosY,
                PosX = Title.PosX - 40
            };
            
            // Header Description.
            Description = new QuaverTextbox()
            {
                Text = "Change the way Quaver looks, sounds, feels... and tastes?",
                Font = QuaverFonts.Medium24,
                Size = new UDim2D(100, 100, 1, 0),
                PosY = Title.PosY + 40,
                Alignment = Alignment.TopCenter,
                TextAlignment = Alignment.TopCenter,
                TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                TextColor = QuaverColors.MainAccent,
                Parent = Container
            };
            
            // Add a line under the header description
            Underline = new QuaverSprite()
            {
                Parent = Container,
                Tint = Color.White,
                SizeY = 1f,
                SizeX = 200,
                PosY = Description.PosY + 40,
                Alignment = Alignment.TopCenter
            };
        }

        /// <summary>
        ///     Updates the header accordingly.
        /// </summary>
        /// <param name="dt"></param>
        internal void Update(double dt)
        {
            if (!Overlay.Active)
                return;
            
            // Rotate the gear icon, just for some extra oomph
            Icon.Rotation = (float)(MathHelper.ToDegrees(Icon.Rotation) + 5 * dt / 30f);
        }
    }
}