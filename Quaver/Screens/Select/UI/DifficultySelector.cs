using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Database.Maps;
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
        ///     Holds the previously selected mapset's difficulties.
        /// </summary>
        private List<DifficultyButtonContainer> PreviousContainers { get; }

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
            
            CalculateScrollContainerHeight(Screen.AvailableMapsets[MapsetContainer.SelectedMapsetIndex]);
            SnapToSelectedDifficulty(MapsetContainer.SelectedMapIndex);

            AddContainedDrawable(CurrentContainer);

            PreviousContainers = new List<DifficultyButtonContainer>();
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            RemovePreviousContainers();
            base.Update(gameTime);
        }

        /// <summary>
        ///    Selects a map difficulty from the set.
        /// </summary>
        public void SelectDifficulty(Mapset set, Map map)
        {
            var index = set.Maps.FindIndex(x => x == map);

            if (index == -1)
                throw new ArgumentNullException($"map: {map.MapId} does not exist in mapset: {set.Directory}");

            var oldDifficulty = MapManager.Selected.Value;

            // Don't bother changing if its the same map.
            // this can occur if the user is using the keyboard to
            if (oldDifficulty == map)
                return;

            MapsetContainer.SelectMap(MapsetContainer.SelectedMapsetIndex, map);
            ScrollToSelectedDifficulty(index, 350);
        }

        /// <summary>
        ///     Selects the next difficulty in a mapset.
        /// </summary>
        /// <param name="direction"></param>
        public void SelectNextDifficulty(Direction direction)
        {
            // Short reference to the current mapset.
            var mapset = MapsetContainer.Screen.AvailableMapsets[MapsetContainer.SelectedMapsetIndex];

            // Find the currently selected map in the mapset.
            var index = mapset.Maps.FindIndex(x => x == MapManager.Selected.Value);

            switch (direction)
            {
                case Direction.Forward:
                    // if the map exists in the set going forward, select the next one
                    // otherwise select the first map in the set to go backwards.
                    SelectDifficulty(mapset, index + 1 < mapset.Maps.Count ? mapset.Maps[index + 1] : mapset.Maps[0]);
                    break;
                case Direction.Backward:
                    // if the map exists in the set going backwards, select the previous one
                    // otherwise select the last map in the set if we reach a bad index.
                    SelectDifficulty(mapset, index - 1 >= 0 ? mapset.Maps[index - 1] : mapset.Maps.Last());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        ///  <summary>
        ///    When a new mapset is changed, this'll make sure the
        ///     new mapset is asscoiated with the selector.
        ///  </summary>
        /// <param name="set"></param>
        public void ChangeAssociatedMapsetMapset(Mapset set)
        {
            var previousContainer = CurrentContainer;
            previousContainer.X = -previousContainer.Width * 2;
            PreviousContainers.Add(previousContainer);

            // Create a new container.
            CurrentContainer = new DifficultyButtonContainer(this, set)
            {
                SetChildrenVisibility = true,
                Visible = true
            };

            CalculateScrollContainerHeight(set);
            SnapToSelectedDifficulty(set.Maps.FindIndex(x => x == MapManager.Selected.Value));
            AddContainedDrawable(CurrentContainer);
        }

        /// <summary>
        ///     Removes all of the previous and unused containers
        ///     (After switching mapsets, they need to be destroyed.)
        /// </summary>
        private void RemovePreviousContainers()
        {
            var containersToRemove = new List<DifficultyButtonContainer>();
            PreviousContainers.ForEach(x =>
            {
                // If the exit transformation is done.
                if (x.Transformations.Count == 0)
                {
                    x.Destroy();
                    containersToRemove.Add(x);
                }
            });

            containersToRemove.ForEach(x => PreviousContainers.Remove(x));
        }

        /// <summary>
        ///    Calculates the height of the scroll container's height based on how many
        ///     maps there are in the set.
        /// </summary>
        private void CalculateScrollContainerHeight(Mapset mapset)
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            ContentContainer.Height = CONTAINER_SIZE.Y.Value + 1 + (DifficultyButton.HEIGHT + 3) * (mapset.Maps.Count - 1);
        }

        /// <summary>
        ///     Scrolls to the selected difficulty
        /// </summary>
        /// <param name="mapIndex"></param>
        /// <param name="time"></param>
        private void ScrollToSelectedDifficulty(int mapIndex, int time)
        {
            // Change the y of the container if need-be
            // There's only 4 (index 3) maps that are able to be shown in this case,
            // we only want to move up the container if the index of the map is off-screen
            if (mapIndex > 2)
            {
                var targetY = (-mapIndex + 2) * (DifficultyButton.HEIGHT + 3);

                if (Math.Abs(ContentContainer.Y - targetY) > 0.05)
                    ScrollTo(targetY, time);
            }
            // Scroll back to top.
            else
            {
                var targetY = DifficultyButton.HEIGHT + 3;

                if (Math.Abs(ContentContainer.Y - targetY) > 0.05)
                    ScrollTo(targetY, time);
            }
        }

        /// <summary>
        ///     Snaps to the selected difficulty.
        /// </summary>
        private void SnapToSelectedDifficulty(int mapIndex)
        {
            if (mapIndex > 2)
                ContentContainer.Y = (-mapIndex + 2) * (DifficultyButton.HEIGHT + 3);
            else
                ContentContainer.Y = DifficultyButton.HEIGHT + 3;

            TargetY = ContentContainer.Y;
            PreviousTargetY = ContentContainer.Y;
        }
    }
}