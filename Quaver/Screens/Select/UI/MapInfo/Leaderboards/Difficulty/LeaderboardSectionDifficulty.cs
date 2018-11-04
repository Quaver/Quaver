using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Database.Maps;
using Quaver.Graphics.Notifications;
using Quaver.Screens.Select.UI.MapsetSelection;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Input;

namespace Quaver.Screens.Select.UI.MapInfo.Leaderboards.Difficulty
{
    public class LeaderboardSectionDifficulty : LeaderboardSection
    {
        /// <summary>
        ///     The currently associated mapset.
        /// </summary>
        public Mapset Mapset { get; private set; }

        /// <summary>
        ///     The list of map difficulties.
        /// </summary>
        public List<LeaderboardDifficultyButton> Difficulties { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="leaderboard"></param>
        public LeaderboardSectionDifficulty(Leaderboard leaderboard)
            : base(LeaderboardSectionType.DifficultySelection, leaderboard, "Difficulty")
        {
            Difficulties = new List<LeaderboardDifficultyButton>();

            ScrollContainer.EasingType = Easing.OutQuint;
            ScrollContainer.TimeToCompleteScroll = 1500;
            ScrollContainer.Scrollbar.Tint = Color.White;
            ScrollContainer.Scrollbar.Width = 3;

            UpdateAsscoiatedMapset(Leaderboard.Screen.AvailableMapsets[Leaderboard.View.MapsetContainer.SelectedMapsetIndex]);
        }

        /// <summary>
        ///     Updates the associated mapset with the leaderboard section.
        /// </summary>
        /// <param name="set"></param>
        public void UpdateAsscoiatedMapset(Mapset set)
        {
            ClearDifficulties();

            Mapset = set;
            CreateDifficultyButtons();
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

            Leaderboard.View.MapsetContainer.SelectMap(Leaderboard.View.MapsetContainer.SelectedMapsetIndex, map);
            ScrollToSelectedDifficulty(index, 1500);
        }

        /// <summary>
        ///     Selects the next difficulty in a mapset.
        /// </summary>
        /// <param name="direction"></param>
        public void SelectNextDifficulty(Direction direction)
        {
            // Short reference to the current mapset.
            var mapset = Leaderboard.View.MapsetContainer.Screen.AvailableMapsets[Leaderboard.View.MapsetContainer.SelectedMapsetIndex];

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
        ///     Clears the container of all difficulties.
        /// </summary>
        private void ClearDifficulties()
        {
            Difficulties.ForEach(x => x.Destroy());
            Difficulties.Clear();
        }

        /// <summary>
        ///     Creates the difficulty buttons with the associated mapset.
        /// </summary>
        private void CreateDifficultyButtons()
        {
            // Shorter reference.
            var maps = Mapset.Maps;

            // Shouldn't ever happen, but we shouldn't do anything if there isn't any maps in the set.
            if (maps.Count == 0)
                return;

            // Start creating buttons.
            if (maps.Count > 0)
            {
                for (var i = 0; i < maps.Count; i++)
                {
                    var difficulty = new LeaderboardDifficultyButton(this, maps[i]);
                    difficulty.Y = i * difficulty.Height + i * 5;

                    difficulty.X = -difficulty.Width;

                    var t = new Animation(AnimationProperty.X, Easing.OutQuint, difficulty.X, 0, 600 + 90 * i);
                    difficulty.Animations.Add(t);

                    Difficulties.Add(difficulty);
                }
            }

            // Reset the height of the content container.
            ScrollContainer.ContentContainer.Animations.Clear();
            ScrollContainer.ContentContainer.Y = 0;
            ScrollContainer.TargetY = ScrollContainer.ContentContainer.Y;
            ScrollContainer.PreviousTargetY = ScrollContainer.ContentContainer.Y;

            var mapIndex = Leaderboard.View.MapsetContainer.SelectedMapIndex;

            // Snap to the selected difficulty.
            if (mapIndex > 3)
                SnapToSelectedDifficulty(mapIndex);

            // If there are more than 6 difficulties(only 6 can be displayed at a time),
            // Then calculate the actual size of the scroll container.
            if (Difficulties.Count > 6)
            {
                ScrollContainer.Scrollbar.Visible = true;
                ScrollContainer.ContentContainer.Height = Difficulties.Count * (Difficulties.First().Height + 5);
                return;
            }

            // In the event that there aren't more than 6 difficulties, we don't need scrolling,
            // So to reset it, set the ContentContainer's side back to the original.
            // the overall container (No need for scrolling)
            ScrollContainer.ContentContainer.Size = ScrollContainer.Size;
            ScrollContainer.Scrollbar.Visible = false;
        }

        /// <summary>
        ///     Scrolls to the selected difficulty
        /// </summary>
        /// <param name="mapIndex"></param>
        /// <param name="time"></param>
        private void ScrollToSelectedDifficulty(int mapIndex, int time)
        {
            if (mapIndex > 2)
            {
                var targetY = (-mapIndex + 2) * (Difficulties.First().Height + 5);

                if (Math.Abs(ScrollContainer.ContentContainer.Y - targetY) > 0.05)
                    ScrollContainer.ScrollTo(targetY, time);
            }
            // Scroll back to top.
            else
            {
                var targetY = Difficulties.First().Height + 5;

                if (Math.Abs(ScrollContainer.ContentContainer.Y - targetY) > 0.05)
                    ScrollContainer.ScrollTo(targetY, time);
            }
        }

        /// <summary>
        ///     Snaps to the selected difficulty.
        /// </summary>
        private void SnapToSelectedDifficulty(int mapIndex)
        {
            ScrollContainer.ContentContainer.Animations.Clear();

            if (mapIndex > 2)
                ScrollContainer.ContentContainer.Y = (-mapIndex + 2) * (Difficulties.First().Height + 5);
            else
                ScrollContainer.ContentContainer.Y = (Difficulties.First().Height + 5);

            ScrollContainer.TargetY = ScrollContainer.ContentContainer.Y;
            ScrollContainer.PreviousTargetY = ScrollContainer.ContentContainer.Y;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Handles the input for the container.
        /// </summary>
        protected override void HandleInput()
        {
            HandleScrollingInput();

            if (KeyboardManager.IsUniqueKeyPress(Keys.Up))
                SelectNextDifficulty(Direction.Backward);

            if (KeyboardManager.IsUniqueKeyPress(Keys.Down))
                SelectNextDifficulty(Direction.Forward);
        }
    }
}