using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Screens.Username.UI;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Screens;

namespace Quaver.Screens.Username
{
    public class UsernameSelectionScreenView : ScreenView
    {
        /// <summary>
        ///     The background image for the screen.
        /// </summary>
        private BackgroundImage Background { get; set; }

        /// <summary>
        ///    The logo for Quaver.
        /// </summary>
        private Sprite QuaverLogo { get; set; }

        /// <summary>
        ///     Usernames must be text
        /// </summary>
        private SpriteText TextUsernameInfo { get; set; }

        /// <summary>
        ///     The text that displays the containing character requirements.
        /// </summary>
        private SpriteText TextContainingCharacters { get; set; }

        /// <summary>
        ///     The textbox to enter in the username.
        /// </summary>
        private UsernameSelectionTextbox Textbox { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public UsernameSelectionScreenView(Screen screen) : base(screen)
        {
            CreateBackground();
            CreateQuaverLogo();
            CreateUsernameInfoText();
            CreateTextbox();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.Black);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        public override void Destroy() => Container?.Destroy();

        /// <summary>
        ///     Creates the background image for the screen.
        /// </summary>
        private void CreateBackground() => Background = new BackgroundImage(UserInterface.UsernameSelectionBackground, 0, false)
        {
            Parent = Container
        };

        /// <summary>
        ///     Creates the Quaver logo for the screen.
        /// </summary>
        private void CreateQuaverLogo() => QuaverLogo = new Sprite()
        {
            Parent = Container,
            Alignment = Alignment.MidCenter,
            SpriteBatchOptions = new SpriteBatchOptions() { BlendState = BlendState.NonPremultiplied },
            Size = new ScalableVector2(261 * 0.85f, 246 * 0.85f),
            Image = UserInterface.QuaverLogoStylish,
            Y = -100
        };

        /// <summary>
        ///
        /// </summary>
        private void CreateUsernameInfoText()
        {
            TextUsernameInfo = new SpriteText(Fonts.Exo2Regular24, "Usernames must be between 3 and 15 characters in length.", 0.70f)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = 25
            };

            TextContainingCharacters = new SpriteText(Fonts.Exo2Regular24, "They must only contain letters [A-Z], numbers [0-9], and spaces.", 0.70f)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = TextUsernameInfo.Y + TextUsernameInfo.MeasureString().Y / 2f + 15
            };
        }

        /// <summary>
        ///     Create username textbox.
        /// </summary>
        private void CreateTextbox() => Textbox = new UsernameSelectionTextbox()
        {
            Parent = Container,
            Alignment = Alignment.MidCenter,
            Y = TextContainingCharacters.Y + TextContainingCharacters.MeasureString().Y / 2f + 25,
        };
    }
}