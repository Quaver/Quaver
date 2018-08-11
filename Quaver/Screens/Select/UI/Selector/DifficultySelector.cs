using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Audio;
using Quaver.Database.Maps;
using Wobble.Audio;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI.Buttons;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Screens.Select.UI.Selector
{
    public class DifficultySelector : ScrollContainer
    {
        /// <summary>
        ///     Reference to the mapsets elector.
        /// </summary>
        public MapsetSelector MapsetSelector { get; }

        /// <summary>
        ///     Parent Screen
        /// </summary>
        public SelectScreen Screen => MapsetSelector.Screen;

        /// <summary>
        ///     Parent ScreenView
        /// </summary>
        public SelectScreenView ScreenView => MapsetSelector.ScreenView;

        /// <summary>
        ///     The container that holds the previously selected mapset's difficulties.
        /// </summary>
        private List<DifficultySelectorContainer> PreviousContainers { get; }

        /// <summary>
        ///     The container that holds the currently selected mapset's difficulties.
        /// </summary>
        private DifficultySelectorContainer CurrentContainer { get; set; }

        /// <summary>
        ///     Invoked when a new difficulty has been selected.
        /// </summary>
        public event EventHandler<DifficultySelectedEventArgs> NewDifficultySelected;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="mapsetSelector"></param>
        public DifficultySelector(MapsetSelector mapsetSelector)
            : base(new ScalableVector2(500, 145), new ScalableVector2(500, 146))
        {
            MapsetSelector = mapsetSelector;

            Tint = Color.Black;
            Alpha = 0.45f;
            InputEnabled = false;
            Scrollbar.Tint = Color.White;
            Scrollbar.Width = 8;

            CurrentContainer = new DifficultySelectorContainer(this, Screen.AvailableMapsets[MapsetSelector.SelectedSet.Value]);
            CurrentContainer.X = CurrentContainer.Width + 5;
            CurrentContainer.Transformations.Add(new Transformation(TransformationProperty.X, Easing.Linear, CurrentContainer.X, 0, 200));
            CalculateScrollContainerHeight(Screen.AvailableMapsets[MapsetSelector.SelectedSet.Value]);

            AddContainedDrawable(CurrentContainer);

            PreviousContainers = new List<DifficultySelectorContainer>();
            MapsetSelector.SelectedSet.ValueChanged += OnMapsetChanged;
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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            MapsetSelector.SelectedSet.ValueChanged -= OnMapsetChanged;

            base.Destroy();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapsetChanged(object sender, BindableValueChangedEventArgs<int> e)
        {
            var previousContainer = CurrentContainer;

            previousContainer.Transformations.Clear();
            previousContainer.Transformations.Add(new Transformation(TransformationProperty.X, Easing.Linear,
                                                        previousContainer.X, -previousContainer.Width * 2, 1));
            previousContainer.SetChildrenAlpha = true;
            previousContainer.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear, 1, 0, 1));

            PreviousContainers.Add(previousContainer);

            // Create a new container.
            CurrentContainer = new DifficultySelectorContainer(this, Screen.AvailableMapsets[e.Value])
            {
                SetChildrenVisibility = true,
                Visible = true
            };
            CurrentContainer.X = CurrentContainer.Width + 5;
            CurrentContainer.Transformations.Add(new Transformation(TransformationProperty.X, Easing.Linear, CurrentContainer.X, 0, 200));
            CalculateScrollContainerHeight(Screen.AvailableMapsets[MapsetSelector.SelectedSet.Value]);

            AddContainedDrawable(CurrentContainer);
            SnapToSelectedDifficulty(Screen.AvailableMapsets[MapsetSelector.SelectedSet.Value].Maps.FindIndex(x => x == MapManager.Selected.Value));
        }

        /// <summary>
        ///     Removes all of the previous and unused containers
        ///     (After switching mapsets, they need to be destroyed.)
        /// </summary>
        private void RemovePreviousContainers()
        {
            var containersToRemove = new List<DifficultySelectorContainer>();
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
            const int contentContainerBaseHeight = 146;
            ContentContainer.Height = contentContainerBaseHeight + (DifficultySelectorItem.HEIGHT + 3) * (mapset.Maps.Count - 1);
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

            // Change the selected map.
            MapManager.Selected.Value = map;

            ScrollToSelectedDifficulty(index, 350);

            // In the event that the difficulties are from the same mapset
            // we need to run some checks on if they have different audio/backgrounds.
            // so we can load those in accordingly.
            if (oldDifficulty.Directory == map.Directory)
            {
                if (MapManager.GetBackgroundPath(oldDifficulty) != MapManager.GetBackgroundPath(map))
                    MapsetSelector.LoadBackground(map);

                // Load new audio file if we need to.
                if (oldDifficulty.AudioPath != map.AudioPath)
                    AudioEngine.PlaySelectedTrackAtPreview();
            }

            NewDifficultySelected?.Invoke(this, new DifficultySelectedEventArgs(set, MapsetSelector.SelectedSet.Value, map, index));
        }

        /// <summary>
        ///     Selects the next difficulty in a mapset.
        /// </summary>
        /// <param name="direction"></param>
        public void SelectNextDifficulty(Direction direction)
        {
            // Short reference to the current mapset.
            var mapset = MapsetSelector.Screen.AvailableMapsets[MapsetSelector.SelectedSet.Value];

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
                var targetY = (-mapIndex + 2) * (DifficultySelectorItem.HEIGHT + 3);

                if (Math.Abs(ContentContainer.Y - targetY) > 0.05)
                    ScrollTo(targetY, time);
            }
            // Scroll back to top.
            else
            {
                var targetY = DifficultySelectorItem.HEIGHT + 3;

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
                ContentContainer.Y = (-mapIndex + 2 ) * (DifficultySelectorItem.HEIGHT + 3);
            else
                ContentContainer.Y = DifficultySelectorItem.HEIGHT + 3;

            TargetY = ContentContainer.Y;
            PreviousTargetY = ContentContainer.Y;
        }
    }
}