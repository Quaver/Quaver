using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Selection.UI.FilterPanel;
using Quaver.Shared.Screens.Tests.UI.Borders;
using Wobble;
using Wobble.Graphics;
using Wobble.Input;
using Wobble.Screens;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Tests.FilterPanel
{
    public class FilterPanelTestScreenView : ScreenView
    {
        private Random RNG { get; }

        public FilterPanelTestScreenView(FilterPanelTestScreen screen) : base(screen)
        {
            // ReSharper disable twice ObjectCreationAsStatement
            var header = new TestMenuBorderHeader {Parent = Container};
            new TestMenuBorderFooter
            {
                Parent = Container,
                Alignment = Alignment.BotLeft
            };

            new SelectFilterPanel(screen.AvailableMapsets)
            {
                Parent = Container,
                Y = header.Height + header.ForegroundLine.Height
            };

            RNG = new Random();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Random map selection
            if (KeyboardManager.IsUniqueKeyPress(Keys.D1) && MapManager.Mapsets?.Count != 0)
            {
                MapManager.Selected.Value = MapManager.Mapsets?[RNG.Next(0, MapManager.Mapsets.Count - 1)].Maps?.First();
                BackgroundHelper.Load(MapManager.Selected.Value);
            }

            // Mods
            if (KeyboardManager.IsUniqueKeyPress(Keys.D2))
            {
                if (ModManager.Mods == 0)
                {
                    ModManager.AddMod(ModIdentifier.Speed05X);
                    ModManager.AddMod(ModIdentifier.Autoplay);
                    ModManager.AddMod(ModIdentifier.Mirror);
                }
                else
                    ModManager.RemoveAllMods();
            }

            Container?.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#2f2f2f"));
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Container?.Destroy();
        }
    }
}