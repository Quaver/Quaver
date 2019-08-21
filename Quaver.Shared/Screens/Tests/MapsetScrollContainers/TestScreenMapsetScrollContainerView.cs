using FontStashSharp;
using Microsoft.Xna.Framework;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Selection.Components;
using Quaver.Shared.Screens.Selection.UI.FilterPanel;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Quaver.Shared.Screens.Tests.FilterPanel;
using Wobble.Graphics.UI.Buttons;
using Alignment = Wobble.Graphics.Alignment;

namespace Quaver.Shared.Screens.Tests.MapsetScrollContainers
{
    public class TestScreenMapsetScrollContainerView : FilterPanelTestScreenView
    {
        private bool Reordered { get; set; }

        public TestScreenMapsetScrollContainerView(TestScreenMapsetScrollContainer screen) : base(screen)
        {
            new SelectJukebox() {Parent = Container};
            var container = new MapsetScrollContainer(screen.AvailableMapsets)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Y = FilterPanel.Y + FilterPanel.Height,
                X = -50
            };

            var containerIndex = Container.Children.IndexOf(container);

            ListHelper.Swap(Container.Children, containerIndex, Container.Children.IndexOf(FilterPanel));

            Header.Parent = Container;
            Footer.Parent = Container;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}