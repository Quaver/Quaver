using System.Collections.Generic;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Selection.Components;
using Quaver.Shared.Screens.Selection.UI.FilterPanel;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Quaver.Shared.Screens.Tests.FilterPanel;
using Wobble.Bindables;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Alignment = Wobble.Graphics.Alignment;

namespace Quaver.Shared.Screens.Tests.MapsetScrollContainers
{
    public class TestScreenMapsetScrollContainerView : FilterPanelTestScreenView
    {
        private bool Reordered { get; set; }

        private MapsetScrollContainer ScrollContainer { get; }

        public TestScreenMapsetScrollContainerView(TestScreenMapsetScrollContainer screen) : base(screen)
        {
            new SelectJukebox() {Parent = Container};

            ScrollContainer  = new MapsetScrollContainer(screen.AvailableMapsets)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Y = FilterPanel.Y + FilterPanel.Height,
                X = -50
            };

            var containerIndex = Container.Children.IndexOf(ScrollContainer);

            ListHelper.Swap(Container.Children, containerIndex, Container.Children.IndexOf(FilterPanel));

            Header.Parent = Container;
            Footer.Parent = Container;

            screen.AvailableMapsets.ValueChanged += OnAvailableMapsetsChanged;
        }

        public override void Destroy()
        {
            var screen = (TestScreenMapsetScrollContainer) Screen;
            // ReSharper disable once DelegateSubtraction
            screen.AvailableMapsets.ValueChanged -= OnAvailableMapsetsChanged;
            base.Destroy();
        }

        private void OnAvailableMapsetsChanged(object sender, BindableValueChangedEventArgs<List<Mapset>> e)
        {
            ScrollContainer.ClearAnimations();
            ScrollContainer.MoveToX(ScrollContainer.Width + 50, Easing.OutQuint, 450);
            ScrollContainer.Wait(50);
            ScrollContainer.MoveToX(-50, Easing.OutQuint, 450);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}