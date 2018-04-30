using Microsoft.Xna.Framework;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Graphics.UniversalDim;
using Quaver.Main;

namespace Quaver.Graphics.Overlays.Options
{
    internal class OptionsMenuBar
    {
        /// <summary>
        ///     The container for the menu bar.
        /// </summary>
        private QuaverSprite Container { get; }

        /// <summary>
        ///     The button to go to the section to the left. (left-facing chevron.)
        /// </summary>
        private QuaverButton LeftSectionButton { get; }

        /// <summary>
        ///     The button to go to the section to the right. (right-facing chevron)
        /// </summary>
        private QuaverButton RightSectionButton { get; }

        /// <summary>
        ///     The text displaying the currently selected option.
        /// </summary>
        private QuaverTextbox Text { get; }

        /// <summary>
        ///     The icon of the menu text 
        /// </summary>
        private QuaverSprite Icon { get; }
        
        /// <summary>
        ///     Reference to the parent overlay.
        /// </summary>
        private OptionsOverlay Overlay { get; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="overlay"></param>
        internal OptionsMenuBar(OptionsOverlay overlay)
        {
            Overlay = overlay;
            
            // Create container.
            Container = new QuaverSprite()
            {
                Position = new UDim2D(0, Overlay.Header.Underline.PosY + 60),
                Size = new UDim2D(500, 60),
                Alignment = Alignment.TopCenter,
                Tint = new Color(0f, 0f, 0f, 0f),
                Parent = Overlay,
                Visible = true
            };
            
            // Create the left menu bar button (chevron)
            LeftSectionButton = new QuaverBasicButton()
            {
                Parent = Container,
                Image = FontAwesome.ChevronDown,
                Rotation = 90,
                Size = new UDim2D(20, 20),
                PosX = 5,
                Alignment = Alignment.MidLeft
            };
            
            // Create the left menu bar button (chevron)
            RightSectionButton = new QuaverBasicButton()
            {
                Parent = Container,
                Image = FontAwesome.ChevronDown,
                Rotation = 270,
                Size = new UDim2D(20, 20),
                PosX = -5,
                Alignment = Alignment.MidRight
            };

            // Create text in menu bar.
            Text = new QuaverTextbox()
            {
                Font = QuaverFonts.Medium12,
                Size = new UDim2D(70, 70),
                TextAlignment = Alignment.MidCenter,
                Alignment = Alignment.MidCenter,
                Parent = Container,
                Text = Overlay.SelectedSection.Name,
                TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                TextColor = Color.Yellow
            };

            // Create icon next to selected option text.
            Icon = new QuaverSprite
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Image = Overlay.SelectedSection.Icon,
                SizeX = 25,
                SizeY = 25,
                PosY = Text.PosY,
                PosX = Text.PosX - 80
            };
        }
    }
}