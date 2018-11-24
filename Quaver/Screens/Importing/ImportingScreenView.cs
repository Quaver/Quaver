using Microsoft.Xna.Framework;
using Quaver.Assets;
using Wobble;
using Wobble.Graphics.UI;
using Wobble.Screens;

namespace Quaver.Screens.Importing
{
    public class ImportingScreenView : ScreenView
    {
        /// <summary>
        ///     The background image for the screen
        /// </summary>
        private BackgroundImage Background { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public ImportingScreenView(Screen screen) : base(screen) => CreateBackground();

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
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();

        /// <summary>
        /// </summary>
        private void CreateBackground() => Background = new BackgroundImage(UserInterface.MenuBackground, 10, false)
        {
            Parent = Container
        };
    }
}