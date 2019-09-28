using System.Collections.Generic;
using System.Linq;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Selection.UI.Maps;
using Wobble;
using Wobble.Input;
using Wobble.Screens;
using Alignment = Wobble.Graphics.Alignment;

namespace Quaver.Shared.Screens.Tests.DrawableMaps
{
    public class TestDrawableMapScreenView : ScreenView
    {
        public TestDrawableMapScreenView(Screen screen) : base(screen)
        {
            var mapset = new Mapset()
            {
                Directory = "test",
                Maps = new List<Map>
                {
                    new Map
                    {
                        Md5Checksum = "1",
                        Artist = "Example Artist",
                        Title = "Example Title",
                        DifficultyName = "Example Difficulty",
                        Creator = "Swan",
                        Difficulty10X = 25,
                        Difficulty05X = 10
                    },
                    new Map
                    {
                        Md5Checksum = "2",
                        Artist = "Example Artist #2",
                        Title = "Example Title #2",
                        DifficultyName = "Example Difficulty #2",
                        Creator = "Swan",
                        Difficulty10X = 36,
                        Difficulty05X = 4
                    },
                }
            };

            MapManager.Selected.Value = mapset.Maps.First();

            // ReSharper disable once ObjectCreationAsStatement
            new DrawableMap(null, mapset.Maps.First(), 0)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
            };

            new DrawableMap(null, mapset.Maps[1], 1)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = 100
            };
        }

        public override void Update(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.PageDown))
            {
                if (ModManager.Mods <= 0)
                    ModManager.AddMod(ModIdentifier.Speed05X);
                else
                    ModManager.RemoveAllMods();
            }

            Container?.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#2f2f2f"));
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}