using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Visualizer;
using Quaver.Shared.Screens.Selection.Components;
using Quaver.Shared.Screens.Selection.UI.Maps;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Quaver.Shared.Screens.Tests.FilterPanel;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Input;
using Wobble.Window;

namespace Quaver.Shared.Screens.Tests.MapScrollContainers
{
    public class TestScreenMapScrollContainerView : FilterPanelTestScreenView
    {
        private MapsetScrollContainer MapsetContainer { get; }

        private MapScrollContainer MapContainer { get; }

        private Bindable<SelectScrollContainerType> ActiveScrollContainer { get; }

        public TestScreenMapScrollContainerView(FilterPanelTestScreen screen) : base(screen)
        {
            var visualizer = new MenuAudioVisualizer((int)WindowManager.Width, 600, 65, 6)
            {
                Parent = Container,
                Alignment = Alignment.BotLeft,
                Y = -Footer.Height
            };

            visualizer.Bars.ForEach(x =>
            {
                x.Alpha = 0.25f;
            });

            ActiveScrollContainer = new Bindable<SelectScrollContainerType>(SelectScrollContainerType.Mapsets)
            {
                Value = SelectScrollContainerType.Mapsets
            };

            new SelectJukebox() { Parent = Container };

            MapsetContainer = new MapsetScrollContainer(screen.AvailableMapsets, ActiveScrollContainer)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Y = FilterPanel.Y + FilterPanel.Height,
                X = -50
            };

            MapManager.Selected.Value = screen.AvailableMapsets.Value.First().Maps.First();

            MapContainer = new MapScrollContainer(screen.AvailableMapsets, MapsetContainer,
                MapManager.Selected.Value.Mapset.Maps, ActiveScrollContainer)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Y = MapsetContainer.Y,
            };

            MapContainer.X = MapContainer.Width + 50;

            ListHelper.Swap(Container.Children, Container.Children.IndexOf(MapContainer), Container.Children.IndexOf(FilterPanel));

            Header.Parent = Container;
            Footer.Parent = Container;

            screen.AvailableMapsets.ValueChanged += OnAvailableMapsetsChanged;

            ActiveScrollContainer.ValueChanged += OnActiveScrollContainerChanged;
        }

        public override void Update(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
            {
                if (ActiveScrollContainer.Value == SelectScrollContainerType.Maps)
                    ActiveScrollContainer.Value = SelectScrollContainerType.Mapsets;
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Enter))
            {
                if (ActiveScrollContainer.Value == SelectScrollContainerType.Mapsets)
                    ActiveScrollContainer.Value = SelectScrollContainerType.Maps;
            }

            base.Update(gameTime);
        }

        public override void Destroy()
        {
            var screen = (TestScreenMapScrollContainer)Screen;
            // ReSharper disable once DelegateSubtraction
            screen.AvailableMapsets.ValueChanged -= OnAvailableMapsetsChanged;

            ActiveScrollContainer.Dispose();

            base.Destroy();
        }

        private void OnAvailableMapsetsChanged(object sender, BindableValueChangedEventArgs<List<Mapset>> e)
        {
            ActiveScrollContainer.Value = SelectScrollContainerType.Mapsets;

            MapsetContainer.ClearAnimations();
            MapsetContainer.MoveToX(MapsetContainer.Width + 50, Easing.OutQuint, 550);
            MapsetContainer.Wait(50);
            MapsetContainer.MoveToX(-50, Easing.OutQuint, 600);
        }

        private void OnActiveScrollContainerChanged(object sender, BindableValueChangedEventArgs<SelectScrollContainerType> e)
        {
            var inactivePosition = MapsetContainer.Width + 50;
            var activePosition = -50;
            var animTime = 450;

            MapsetContainer.ClearAnimations();
            MapContainer.ClearAnimations();

            switch (e.Value)
            {
                case SelectScrollContainerType.Mapsets:
                    MapsetContainer.MoveToX(activePosition, Easing.OutQuint, animTime);
                    MapContainer.MoveToX(inactivePosition, Easing.OutQuint, animTime);
                    break;
                case SelectScrollContainerType.Maps:
                    MapsetContainer.MoveToX(inactivePosition, Easing.OutQuint, animTime);
                    MapContainer.MoveToX(activePosition, Easing.OutQuint, animTime);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}