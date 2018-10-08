using Microsoft.Xna.Framework;
using Quaver.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Window;

namespace Quaver.Screens.Connecting.UI
{
    public class UsernameSelectionDialog : DialogScreen
    {
        /// <summary>
        ///     Reference to the parent ScreenView.
        /// </summary>
        private ConnectingScreenView View { get; }

        /// <summary>
        ///     The containing box for the dialog.
        /// </summary>
        private Sprite ContainingBox { get; set; }

        /// <summary>
        ///     The header text to create a username.
        /// </summary>
        private SpriteText Header { get; set;  }

        /// <summary>
        ///     The text content of the dialog which displays the requirements for usernames.
        /// </summary>
        private SpriteText TextContent { get; set; }

        /// <summary>
        ///     Second line for text content.
        /// </summary>
        private SpriteText TextContent2 { get; set; }

        /// <summary>
        ///     The textbox to enter a username.
        /// </summary>
        private UsernameSelectionTextbox Textbox { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="view"></param>
        /// <param name="backgroundAlpha"></param>
        public UsernameSelectionDialog(ConnectingScreenView view, float backgroundAlpha) : base(backgroundAlpha)
        {
            View = view;
            CreateContent();;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            ContainingBox = new Sprite()
            {
                Parent = Container,
                Size = new ScalableVector2(WindowManager.Width, 200),
                Alignment = Alignment.MidCenter,
                Tint = Color.Black,
                Alpha = 0.85f
            };

            Header = new SpriteText(Fonts.Exo2Italic24, "Create Username", 0.80f)
            {
                Parent = ContainingBox,
                Alignment = Alignment.TopCenter,
                Y = 25
            };

            Header.Y += Header.MeasureString().Y / 2f;

            TextContent = new SpriteText(Fonts.Exo2Regular24, "Usernames must be between 3 to 15 characters and may only contain", 0.50f)
            {
                Parent = ContainingBox,
                Alignment = Alignment.TopCenter,
            };

            TextContent.Y = Header.Y + TextContent.MeasureString().Y / 2f + 25;

            TextContent2 = new SpriteText(Fonts.Exo2Regular24, "letters (A-Z), numbers (0-9), hyphens (-), and spaces.", 0.50f)
            {
                Parent = ContainingBox,
                Alignment = Alignment.TopCenter,
            };

            TextContent2.Y = TextContent.Y + TextContent2.MeasureString().Y / 2f + 10;

            Textbox = new UsernameSelectionTextbox(View)
            {
                Parent = ContainingBox,
                Alignment = Alignment.TopCenter,
                Y = TextContent2.Y + 35
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
        }
    }
}