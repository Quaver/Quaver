using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Selection.UI.FilterPanel
{
    public class SelectFilterPanel : Sprite
    {
        /// <summary>
        ///     The banner that displays the current map's background
        /// </summary>
        private FilterPanelBanner Banner { get; }

        /// <summary>
        ///     Displays the current map information
        /// </summary>
        private FilterPanelMapInfo MapInfo { get; }

        /// <summary>
        /// </summary>
        public SelectFilterPanel()
        {
            Size = new ScalableVector2(WindowManager.Width, 82);
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
        }
    }
}