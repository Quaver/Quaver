using Quaver.Screens.Select.UI.MapInfo.Actions;
using Quaver.Screens.Select.UI.MapInfo.Banner;
using Quaver.Screens.Select.UI.MapInfo.DifficultySelection;
using Quaver.Screens.Select.UI.MapInfo.Leaderboards;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Screens.Select.UI.MapInfo
{
    public class MapInfoContainer : Sprite
    {
        /// <summary>
        ///     Reference to the select screen itself
        /// </summary>
        public SelectScreen Screen { get; }

        /// <summary>
        ///     Reference to the select screenview
        /// </summary>
        public SelectScreenView View { get; }

        /// <summary>
        ///     The banner for the currently selected map.
        /// </summary>
        public MapBanner Banner { get; }

        /// <summary>
        ///     Displays the leaderboard for a given map.
        /// </summary>
        public Leaderboard Leaderboard { get; }

        /// <summary>
        ///     Banner that just displays "Actions"
        /// </summary>
        public ActionsBanner ActionsBanner { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public MapInfoContainer(SelectScreen screen, SelectScreenView view)
        {
            Screen = screen;
            View = view;

            Alignment = Alignment.MidLeft;
            Size = new ScalableVector2(780, 608);
            Tint = Color.Black;
            Alpha = 0f;
            Y = 1;
            X = 5;

            Banner = new MapBanner(Screen, View) {Parent = this};
            Leaderboard = new Leaderboard(Screen, View, this) {Parent = this};
            ActionsBanner = new ActionsBanner(Screen, View, this) {Parent = this};
        }
    }
}