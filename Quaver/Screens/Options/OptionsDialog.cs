using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Assets;
using Quaver.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Window;

namespace Quaver.Screens.Options
{
    public class OptionsDialog : DialogScreen
    {
        /// <summary>
        ///     The background in which the content lives.
        /// </summary>
        private Sprite ContentContainer { get; set; }

#region HEADER

        /// <summary>
        ///     The container for the header of the options menu
        /// </summary>
        private Sprite HeaderContainer { get; set; }

        /// <summary>
        ///     A flag icon displayed at the left of the header for stylistic purposes
        /// </summary>
        private Sprite HeaderFlag { get; set; }

        /// <summary>
        ///     The text for the header
        /// </summary>
        private SpriteText HeaderText { get; set; }

        /// <summary>
        ///     The button to exit the dialog.
        /// </summary>
        private ImageButton ExitButton { get; set; }

#endregion

#region FOOTER

        /// <summary>
        ///     The container for the footer of the options menu.
        /// </summary>
        private Sprite FooterContainer { get; set; }

        /// <summary>
        ///     The button to confirm changes for the options.
        /// </summary>
        private TextButton FooterOkButton { get; set; }

        /// <summary>
        ///     The button to cancel and close down the options menu.
        /// </summary>
        private TextButton FooterCancelButton { get; set; }

#endregion

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="backgroundAlpha"></param>
        public OptionsDialog(float backgroundAlpha) : base(backgroundAlpha)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void CreateContent()
        {
            // Create the container for the entire options menu.
            ContentContainer = new Sprite()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(WindowManager.Width / 1.4f, WindowManager.Height / 1.4f),
                Tint = Color.LightSlateGray,
            };

            CreateHeader();
            CreateFooter();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                DialogManager.Dismiss();
        }

        /// <summary>
        ///     Creates the header for the options menu.
        /// </summary>
        private void CreateHeader()
        {
            HeaderContainer = new Sprite()
            {
                Parent = ContentContainer,
                Size = new ScalableVector2(ContentContainer.Width, 45),
                Tint = Color.Black
            };

            HeaderFlag = new Sprite()
            {
                Parent = HeaderContainer,
                Size = new ScalableVector2(5, HeaderContainer.Height),
                Tint = Color.LightGray
            };

            HeaderText = new SpriteText(Fonts.Exo2Regular24, "Options Menu")
            {
                Parent = HeaderContainer,
                TextScale = 0.65f,
                Alignment = Alignment.MidLeft,
                X = HeaderFlag.X + 5
            };

            HeaderText.X += HeaderText.MeasureString().X / 1.5f;

            ExitButton = new ImageButton(FontAwesome.Times, (sender, args) => DialogManager.Dismiss())
            {
                Parent = HeaderContainer,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(25, 25)
            };

            ExitButton.X -= ExitButton.Width / 2f + 5;
        }

        /// <summary>
        ///     Creates the footer of the options dialog.
        /// </summary>
        private void CreateFooter()
        {
            FooterContainer = new Sprite()
            {
                Parent = ContentContainer,
                Size = new ScalableVector2(ContentContainer.Width, 50),
                Tint = Colors.DarkGray,
                Alignment = Alignment.BotLeft,
                Y = 1
            };

            FooterOkButton = new TextButton(UserInterface.BlankBox, Fonts.Exo2Regular24, "OK", 0.55f, (sender, args) => DialogManager.Dismiss())
            {
                Parent = FooterContainer,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(200, 30),
                X = -20,
                Tint = Colors.SecondaryAccent,
                Text =
                {
                    TextColor = Color.Black
                }
            };

            FooterCancelButton = new TextButton(UserInterface.BlankBox, Fonts.Exo2Regular24, "Cancel", 0.55f, (sender, args) => DialogManager.Dismiss())
            {
                Parent = FooterContainer,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(200, 30),
                X = FooterOkButton.X - FooterOkButton.Width - 20,
                Tint = Colors.SecondaryAccent,
                Text =
                {
                    TextColor = Color.Black
                }
            };
        }
    }
}
