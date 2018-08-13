using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Overlays.Toolbar;
using Quaver.Screens.Menu;
using Quaver.Screens.Menu.UI.BottomToolbar;
using Quaver.Screens.Select.UI;
using Quaver.Screens.Select.UI.Selector;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Transformations;
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

        /// <summary>
        ///     The interface to select mapsets
        /// </summary>
        public MapsetSelector MapsetSelector { get; private set; }

        /// <summary>
        ///     The UI to select an individual map in the set.
        /// </summary>
        public DifficultySelector DifficultySelector { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public SelectScreenView(Screen screen) : base(screen)
        {
            Toolbar = new Toolbar(new List<ToolbarItem>
            {
                new ToolbarItem("Home", () => ScreenManager.ChangeScreen(new MainMenuScreen()))
            }, new List<ToolbarItem>())
            {
                Parent = Container
            };

            BottomToolbar = new BottomBar() {Parent = Container};

            MapsetSelector = new MapsetSelector((SelectScreen) Screen, this)
            {
                Parent = Container,
                X = 200,
                Transformations =
                {
                    new Transformation(TransformationProperty.X, Easing.EaseOutBounce, 200, 0, 1200)
                }
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
            GameBase.Game.GraphicsDevice.Clear(Color.Black);
            BackgroundManager.Draw(gameTime);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();
    }
}