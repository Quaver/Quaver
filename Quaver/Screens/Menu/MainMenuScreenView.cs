using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Graphics.Overlays.Toolbar;
using Wobble;
using Wobble.Graphics.UI;
using Wobble.Screens;

namespace Quaver.Screens.Menu
{
    public class MainMenuScreenView : ScreenView
    {
        /// <summary>
        ///     The background image for this screen.
        /// </summary>
        private BackgroundImage Background { get; }

        /// <summary>
        ///     The top toolbar for this screen.
        /// </summary>
        private Toolbar Toolbar { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public MainMenuScreenView(Screen screen) : base(screen)
        {
            Background = new BackgroundImage(UserInterface.MenuBackground, 30) { Parent = Container };

            // Create the toolbar for this screen.
            Toolbar = new Toolbar(new List<ToolbarItem>
            {
                new ToolbarItem("Home", () => Console.WriteLine("Already Home!"), true)
            }, new List<ToolbarItem>
            {
                new ToolbarItem(FontAwesome.PowerOff, GameBase.Game.Exit),
                new ToolbarItem(FontAwesome.Cog, () => { Console.WriteLine("Settings"); })
            })
            {
                Parent = Container
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
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            Container?.Draw(gameTime);

            try
            {
                GameBase.Game.SpriteBatch.End();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}
