using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Backgrounds;
using Quaver.Graphics.Overlays.Toolbar;
using Quaver.Screens.Menu;
using Quaver.Screens.Select.UI;
using Quaver.Screens.Select.UI.MapInfo;
using Quaver.Screens.Select.UI.MapsetSelection;
using Quaver.Screens.Select.UI.Search;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Form;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Screens.Select
{
    public class SelectScreenView : ScreenView
    {
        /// <summary>
        ///     The top toolbar for this screen.
        /// </summary>
        public Toolbar Toolbar { get; }

        /// <summary>
        ///     The scroll container for the mapsets.
        /// </summary>
        public MapsetContainer MapsetContainer { get; }

        /// <summary>
        ///     Mapset search interface.
        /// </summary>
        private MapsetSearchBar SearchBar { get; }

        /// <summary>
        ///     The map information container.
        /// </summary>
        public MapInfoContainer MapInfoContainer { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public SelectScreenView(Screen screen) : base(screen)
        {
            BackgroundManager.Background.Dim = 0;
            // BackgroundManager.Background.Strength = 8;

            Toolbar = new Toolbar(new List<ToolbarItem>
            {
                new ToolbarItem("Home", () => QuaverScreenManager.ChangeScreen(new MenuScreen())),
                new ToolbarItem("Play", () => {}, true)
            }, new List<ToolbarItem>(), new ScalableVector2(WindowManager.Width, 80))
            {
                Parent = Container
            };

            MapsetContainer = new MapsetContainer((SelectScreen) Screen, this)
            {
                Parent = Container,
                X = 200,
                Animations =
                {
                    new Animation(AnimationProperty.X, Easing.OutBounce, 200, 0, 1200)
                }
            };

            SearchBar = new MapsetSearchBar((SelectScreen) Screen, this) {Parent = Container};
            MapInfoContainer = new MapInfoContainer((SelectScreen) Screen, this) {Parent = Container };
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