using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Screens;

namespace Quaver.Screens.Select.UI
{
    public class DifficultySelector : ScrollContainer
    {
        /// <summary>
        ///     Reference to the mapset container.
        /// </summary>
        public MapsetContainer MapsetContainer { get; }

        /// <summary>
        ///     Parent Screen
        /// </summary>
        public SelectScreen Screen => MapsetContainer.Screen;

        /// <summary>
        ///     Parent ScreenView
        /// </summary>
        public SelectScreenView ScreenView => MapsetContainer.View;


        /// <summary>
        ///     The container that holds the currently selected mapset's difficulties.
        /// </summary>
        private DifficultyButtonContainer CurrentContainer { get; set; }

        /// <summary>
        ///     The base size of the container.
        /// </summary>
        private static ScalableVector2 CONTAINER_SIZE { get; } = new ScalableVector2(500, 145);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        public DifficultySelector(MapsetContainer container)
            : base(CONTAINER_SIZE, new ScalableVector2(CONTAINER_SIZE.X.Value, CONTAINER_SIZE.Y.Value + 1))
        {
            MapsetContainer = container;

            Tint = Color.Black;
            Alpha = 0.45f;
            InputEnabled = false;
            Scrollbar.Tint = Color.White;
            Scrollbar.Width = 8;

            CurrentContainer = new DifficultyButtonContainer(this, Screen.AvailableMapsets[MapsetContainer.SelectedMapsetIndex]);
            CurrentContainer.X = CurrentContainer.Width + 5;
            CurrentContainer.Transformations.Add(new Transformation(TransformationProperty.X, Easing.Linear, CurrentContainer.X, 0, 200));
            // CalculateScrollContainerHeight(Screen.AvailableMapsets[MapsetSelector.SelectedSet.Value]);

            AddContainedDrawable(CurrentContainer);
        }
    }
}