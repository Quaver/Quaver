using System.Collections.Generic;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.Search;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Selection.UI.FilterPanel
{
    public class SelectFilterPanel : Sprite
    {
        /// <summary>
        ///     The mapsets that are currently available to play
        /// </summary>
        private Bindable<List<Mapset>> AvailableMapsets { get; }

        /// <summary>
        ///     The banner that displays the current map's background
        /// </summary>
        private FilterPanelBanner Banner { get; }

        /// <summary>
        ///     Displays the current map information
        /// </summary>
        private FilterPanelMapInfo MapInfo { get; }

        /// <summary>
        ///     The textbox to search for maps
        /// </summary>
        private FilterPanelSearchBox SearchBox { get; }

        /// <summary>
        ///     The text that displays how many maps are available
        /// </summary>
        private FilterPanelMapsAvailable MapsAvailable { get; }

        /// <summary>
        /// </summary>
        public SelectFilterPanel(Bindable<List<Mapset>> availableMapsets)
        {
            AvailableMapsets = availableMapsets;

            Size = new ScalableVector2(WindowManager.Width, 85);
            Tint = ColorHelper.HexToColor("#242424");

            Banner = new FilterPanelBanner(this)
            {
                Parent = this,
                Alignment = Alignment.MidLeft
            };

            MapInfo = new FilterPanelMapInfo
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 25
            };

            SearchBox = new FilterPanelSearchBox(AvailableMapsets, "", "Type to search...")
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 300
            };

            MapsAvailable = new FilterPanelMapsAvailable(AvailableMapsets)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = SearchBox.X + SearchBox.Width + 100
            };
        }
    }
}