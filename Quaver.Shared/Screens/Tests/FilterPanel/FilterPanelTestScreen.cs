using System;
using System.Collections.Generic;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Screens.Tests.UI.Borders;
using Wobble.Bindables;
using Wobble.Extended.HotReload.Screens;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.FilterPanel
{
    public class FilterPanelTestScreen : Screen
    {
        public override ScreenView View { get; protected set; }

        public Bindable<List<Mapset>> AvailableMapsets { get; }

        public FilterPanelTestScreen()
        {
            AvailableMapsets = new Bindable<List<Mapset>>(null)
            {
                Value = MapsetHelper.OrderMapsetsByConfigValue(MapsetHelper.SearchMapsets(MapManager.Mapsets, ""))
            };

            View = new FilterPanelTestScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            AvailableMapsets.Dispose();
            base.Destroy();
        }
    }
}