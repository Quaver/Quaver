using Microsoft.Xna.Framework;
using Quaver.Resources;
using Quaver.Online;
using Quaver.Online.Chat;
using Quaver.Server.Client.Structures;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Screens;

namespace Quaver.Screens.Test.Chat
{
    public class TestChatScreenView : ScreenView
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestChatScreenView(Screen screen) : base(screen) => DialogManager.Show(ChatManager.Dialog);

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
            GameBase.Game.SpriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}