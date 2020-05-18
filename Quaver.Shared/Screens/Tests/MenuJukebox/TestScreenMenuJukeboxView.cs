using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Menu;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Main.UI;
using Quaver.Shared.Screens.Main.UI.Jukebox;
using Wobble;
using Wobble.Graphics;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.MenuJukebox
{
    public class TestScreenMenuJukeboxView : ScreenView
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TestScreenMenuJukeboxView(Screen screen) : base(screen)
        {
            var footer = new MainMenuFooter()
            {
                Parent = Container,
                Alignment = Alignment.BotLeft
            };

            new FooterJukebox()
            {
                Parent = footer,
                Alignment = Alignment.BotCenter
            };
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
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#2F2F2F"));
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}