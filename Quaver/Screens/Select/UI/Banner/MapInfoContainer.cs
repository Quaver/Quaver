using System.Drawing;
using Quaver.Screens.Select.UI.DifficultySelection;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Screens.Select.UI.Info
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
        ///     The container for difficulty selection.
        /// </summary>
        public DifficultySelectorContainer DifficultySelectorContainer { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public MapInfoContainer (SelectScreen screen, SelectScreenView view)
        {
            Screen = screen;
            View = view;

            Alignment = Alignment.MidLeft;
            Size = new ScalableVector2(625, 575);
            X = 80;
            Tint = Color.Black;
            Alpha = 0f;

            Banner = new MapBanner(Screen, View) {Parent = this};

            DifficultySelectorContainer = new DifficultySelectorContainer(Screen, View)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Y = Banner.Y + Banner.Height - 3
            };
        }
    }
}