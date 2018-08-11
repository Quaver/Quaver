using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Database.Maps;
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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="mapsetSelector"></param>
        public DifficultySelector(MapsetSelector mapsetSelector)
            : base(new ScalableVector2(400, 105), new ScalableVector2(400, 106))
        {
            MapsetSelector = mapsetSelector;

            Tint = Color.Black;
            Alpha = 0.45f;
            InputEnabled = false;
            Scrollbar.Tint = Color.White;
            Scrollbar.Width = 8;

            CurrentContainer = new DifficultySelectorContainer(this, MapManager.Mapsets[MapsetSelector.SelectedSet.Value]);
            CurrentContainer.X = CurrentContainer.Width + 5;
            CurrentContainer.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutBounce,
                                                    CurrentContainer.X, 0, 400));

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
                                                        previousContainer.X, -previousContainer.Width * 2, 200));
            previousContainer.SetChildrenAlpha = true;
            previousContainer.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear, 1, 0, 100));

            PreviousContainers.Add(previousContainer);

            // Create a new container.
            CurrentContainer = new DifficultySelectorContainer(this, MapManager.Mapsets[e.Value])
            {
                SetChildrenVisibility = true,
                Visible = true
            };
            CurrentContainer.X = CurrentContainer.Width + 5;
            CurrentContainer.Transformations.Add(new Transformation(TransformationProperty.X, Easing.EaseOutBounce,
                CurrentContainer.X, 0, 400));

            AddContainedDrawable(CurrentContainer);
            Console.WriteLine($"Mapset from {e.OldValue} to {e.Value} - {MapManager.Mapsets[e.Value].Artist} - {MapManager.Mapsets[e.Value].Title}");
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
    }
}