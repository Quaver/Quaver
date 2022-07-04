using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Visualizer;
using Quaver.Shared.Screens.Selection.Components;
using Quaver.Shared.Screens.Selection.UI;
using Quaver.Shared.Screens.Selection.UI.Leaderboard;
using Quaver.Shared.Screens.Selection.UI.Maps;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Quaver.Shared.Screens.Selection.UI.Modifiers;
using Quaver.Shared.Screens.Tests.FilterPanel;
using Quaver.Shared.Screens.Tests.MapScrollContainers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Input;
using Wobble.Window;

namespace Quaver.Shared.Screens.Tests.LeaderboardWithMaps
{
    public class TestLeaderboardWithMapsScreenView : FilterPanelTestScreenView
    {
        private MapsetScrollContainer MapsetContainer { get; }

        private MapScrollContainer MapContainer { get; }

        private Bindable<SelectScrollContainerType> ActiveScrollContainer { get; }

        private Bindable<SelectContainerPanel> ActivePanel { get; }

        private ModifierSelectorContainer ModifierSelector { get; }

        private LeaderboardContainer LeaderboardContainer { get; }

        public TestLeaderboardWithMapsScreenView(FilterPanelTestScreen screen) : base(screen)
        {
            var visualizer = new MenuAudioVisualizer((int) WindowManager.Width, 600, 65, 6)
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

            new SelectJukebox() {Parent = Container};

            MapsetContainer  = new MapsetScrollContainer(screen.AvailableMapsets, ActiveScrollContainer)
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

            LeaderboardContainer = new LeaderboardContainer()
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                X = 50,
                Y = FilterPanel.Y + FilterPanel.Height + 24
            };

            ModifierSelector = new ModifierSelectorContainer(null)
            {
                Parent = Container,
                Y = LeaderboardContainer.Y
            };

            ModifierSelector.X = -ModifierSelector.Width - 50;

            ActivePanel = new Bindable<SelectContainerPanel>(SelectContainerPanel.Leaderboard)
            {
                Value = SelectContainerPanel.Leaderboard
            };

            ActivePanel.ValueChanged += OnActivePanelChanged;
        }

        public override void Update(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
            {
                switch (ActivePanel.Value)
                {
                    case SelectContainerPanel.Leaderboard:
                        if (ActiveScrollContainer.Value == SelectScrollContainerType.Maps)
                            ActiveScrollContainer.Value = SelectScrollContainerType.Mapsets;
                        break;
                    case SelectContainerPanel.Modifiers:
                        ActivePanel.Value = SelectContainerPanel.Leaderboard;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Enter))
            {
                if (ActiveScrollContainer.Value == SelectScrollContainerType.Mapsets)
                    ActiveScrollContainer.Value = SelectScrollContainerType.Maps;
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.F1) && ActivePanel.Value != SelectContainerPanel.Modifiers)
                ActivePanel.Value = SelectContainerPanel.Modifiers;

            base.Update(gameTime);
        }

        public override void Destroy()
        {
            var screen = (TestScreenMapScrollContainer) Screen;
            // ReSharper disable once DelegateSubtraction
            screen.AvailableMapsets.ValueChanged -= OnAvailableMapsetsChanged;

            ActiveScrollContainer.Dispose();
            ActivePanel?.Dispose();

            base.Destroy();
        }

        private void OnActivePanelChanged(object sender, BindableValueChangedEventArgs<SelectContainerPanel> e)
        {
            LeaderboardContainer.ClearAnimations();
            ModifierSelector.ClearAnimations();

            const int activePos = 50;
            var inactivePos = -LeaderboardContainer.Width - activePos;
            const int animTime = 400;

            switch (e.Value)
            {
                case SelectContainerPanel.Leaderboard:
                    LeaderboardContainer.MoveToX(activePos, Easing.OutQuint, animTime);
                    ModifierSelector.MoveToX(inactivePos, Easing.OutQuint, animTime);
                    break;
                case SelectContainerPanel.Modifiers:
                    LeaderboardContainer.MoveToX(inactivePos, Easing.OutQuint, animTime);
                    ModifierSelector.MoveToX(activePos, Easing.OutQuint, animTime);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnAvailableMapsetsChanged(object sender, BindableValueChangedEventArgs<List<Mapset>> e)
        {
            ActiveScrollContainer.Value = SelectScrollContainerType.Mapsets;

            MapsetContainer.ClearAnimations();
            MapsetContainer.MoveToX(MapsetContainer.Width + 50, Easing.OutQuint, 500);
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