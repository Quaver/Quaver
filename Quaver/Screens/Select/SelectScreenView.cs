using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Graphics.Overlays.Toolbar;
using Quaver.Screens.Menu;
using Quaver.Screens.Menu.UI.BottomToolbar;
using Wobble;
using Wobble.Graphics.UI;
using Wobble.Screens;

namespace Quaver.Screens.Select
{
    public class SelectScreenView : ScreenView
    {
        /// <summary>
        ///     The currently selected map's background.
        /// </summary>
        public BackgroundImage Background { get; }

        /// <summary>
        ///     The top toolbar for this screen.
        /// </summary>
        private Toolbar Toolbar { get; set;  }

        /// <summary>
        ///     The bottom toolbar for this screen.
        /// </summary>
        private BottomBar BottomToolbar { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public SelectScreenView(Screen screen) : base(screen)
        {
            Background = new BackgroundImage(UserInterface.MenuBackground, 20) {Parent = Container};

            Toolbar = new Toolbar(new List<ToolbarItem>
            {
                new ToolbarItem("Home", () => ScreenManager.ChangeScreen(new MainMenuScreen()))
            }, new List<ToolbarItem>())
            {
                Parent = Container
            };

            BottomToolbar = new BottomBar() {Parent = Container};
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
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}