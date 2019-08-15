using System;
using System.Collections.Generic;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.Search;
using Quaver.Shared.Screens.Tests.UI.Borders;
using Wobble.Bindables;
using Wobble.Extended.HotReload.Screens;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.FilterPanel
{
    public class FilterPanelTestScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public Bindable<List<Mapset>> AvailableMapsets { get; private set; }

        public Bindable<string> CurrentSearchQuery { get; private set; }

        public FilterPanelTestScreen()
        {
            Setup();
            SetView(false);
        }

        public FilterPanelTestScreen(bool customScreenView)
        {
            Setup();
            SetView(customScreenView);
        }

        private void Setup()
        {
            CurrentSearchQuery = new Bindable<string>(null)
            {
                Value = FilterPanelSearchBox.PreviousSearchTerm
            };

            AvailableMapsets = new Bindable<List<Mapset>>(null)
            {
                Value = MapsetHelper.OrderMapsetsByConfigValue(MapsetHelper.SearchMapsets(MapManager.Mapsets, CurrentSearchQuery.Value))
            };
        }

        private void SetView(bool inheritedCustomScreenView)
        {
            if (!inheritedCustomScreenView)
                View = new FilterPanelTestScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            AvailableMapsets.Dispose();
            CurrentSearchQuery.Dispose();
            base.Destroy();
        }
    }
}