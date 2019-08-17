using System;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Screens.Selection.Components;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Quaver.Shared.Screens.Tests.FilterPanel;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Tests.DrawableMapsetsMultiple
{
    public class TestMapsetsMultipleScreenView : FilterPanelTestScreenView
    {
        private Random Rng;

        public TestMapsetsMultipleScreenView(TestMapsetsMultipleScreen screen) : base(screen)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new SelectJukebox {Parent = Container};

            Rng = new Random();

            if (MapManager.Mapsets?.Count == 0)
            {
                new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoHeavy),
                    "You need to have mapsets imported in order to view this test", 22)
                {
                    Parent = Container,
                    Alignment = Alignment.MidCenter
                };
            }

            for (var i = 0; i < 10; i++)
            {
                var mapset = GetMapset();

                mapset.Parent = Container;
                mapset.Alignment = Alignment.TopCenter;
                mapset.Y = 100 * i + 50;
            }
        }

        private DrawableMapset GetMapset()
        {
            var num = Rng.Next(0, MapManager.Mapsets.Count - 1);
            return new DrawableMapset(null, MapManager.Mapsets[num], num);
        }
    }
}