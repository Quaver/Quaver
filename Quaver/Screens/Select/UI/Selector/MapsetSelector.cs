using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Audio;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Backgrounds;
using Quaver.Scheduling;
using Quaver.Screens.Edit.UI;
using Wobble;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Input;
using Wobble.Window;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Quaver.Screens.Select.UI.Selector
{
    public class MapsetSelector : ScrollContainer
    {
        /// <summary>
        ///     Reference to the song select screen itself.
        /// </summary>
        public SelectScreen Screen { get; }

        /// <summary>
        ///     Reference to the song select screen view.
        /// </summary>
        public SelectScreenView ScreenView { get; }

        /// <summary>
        ///     Interface to select the difficulties of the map.
        /// </summary>
        public DifficultySelector DifficultySelector { get; }

        /// <summary>
        ///     The amount of maps available in the pool.
        /// </summary>
        public const int MapsetPoolSize = 55;

        /// <summary>
        ///     The amount of space between each mapset.
        /// </summary>
        public const int SetSpacingY = 88;

        /// <summary>
        ///     The buttons that are currently in the mapset pool.
        /// </summary>
        public List<MapsetSelectorItem> MapsetButtonPool { get; private set; }

        /// <summary>
        ///     The starting index of the mapset button pool.
        /// </summary>
        private int PoolStartingIndex { get; set; }

        /// <summary>
        ///     The previous value of the content container, used for
        ///     scrolling upwards.
        /// </summary>
        private float PreviousContentContainerY { get; set; }

        /// <summary>
        ///     The selected mapset.
        /// </summary>
        public BindableInt SelectedSet { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public MapsetSelector(SelectScreen screen, SelectScreenView view)
            : base(new ScalableVector2(550, WindowManager.Height), new ScalableVector2(550, WindowManager.Height))
        {
            Screen = screen;
            ScreenView = view;

            Alignment = Alignment.MidRight;
            X = 0;
            Alpha = 0f;
            Tint = Color.Black;

            Scrollbar.Width = 10;
            Scrollbar.Tint = Color.White;

            ScrollSpeed = 150;
            EasingType = Easing.EaseOutQuint;
            TimeToCompleteScroll = 2100;

            // Find the index of the current mapset
            SelectedSet = new BindableInt(0, -1, int.MaxValue);

            // TODO: Set parent and all that jazz in the map info section.
            DifficultySelector = new DifficultySelector(this)
            {
                Parent = ScreenView.Container,
                Alignment = Alignment.MidCenter,
                X = -100
            };

            if (MapManager.Selected.Value == null)
                MapManager.Selected.Value = MapManager.Mapsets.First().Maps.First();

            GenerateSetPool();

            BackgroundManager.Load(MapManager.Selected.Value);

            if (AudioEngine.Track != null && !AudioEngine.Track.IsPlaying)
                AudioEngine.PlaySelectedTrackAtPreview();
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            ShiftPoolBasedOnScroll();
            PreviousContentContainerY = ContentContainer.Y;

            var game = (QuaverGame) GameBase.Game;

            // Determine when or not to have the scrolling input active.
            if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt) ||
                KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt) || game.VolumeController.IsActive)
            {
                InputEnabled = false;
            }
            else
            {
                InputEnabled = true;
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            SelectedSet.UnHookEventHandlers();
            base.Destroy();
        }

        /// <summary>
        ///     Generates the pool of SongSelectorSets. This should only be used
        ///     when initially creating
        /// </summary>
        private void GenerateSetPool()
        {
            // Reset the ContentContainer size to dynamically add onto the size later.
            ContentContainer.Size = Size;

            // Calculate the actual height of the container based on the amount of available sets
            // there are.
            ContentContainer.Height += 1 + (Screen.AvailableMapsets.Count - 8) * SetSpacingY + MapsetSelectorItem.BUTTON_HEIGHT / 2f;

            // Destroy previous buttons if there are any in the poool.
            if (MapsetButtonPool != null && MapsetButtonPool.Count > 0)
                MapsetButtonPool.ForEach(x => x.Destroy());

            // Create the initial list of buttons for the pool.
            MapsetButtonPool = new List<MapsetSelectorItem>();

            // Find and set the selected set.
            SelectedSet.Value = Screen.AvailableMapsets.FindIndex(x => x.Maps.Contains(MapManager.Selected.Value));

            // Set the map pool starting index
            if (SelectedSet.Value - 4 > 0)
                PoolStartingIndex = SelectedSet.Value - 4;

            // Make sure the ContentContainer's y is scrolled to where the mapset is supposed to be
            // upon loading.

            ContentContainer.Y = (-SelectedSet.Value + 4) * SetSpacingY;
            TargetY = ContentContainer.Y;
            PreviousTargetY = ContentContainer.Y;

            // Create the mapset buttons
            for (var i = PoolStartingIndex; i < PoolStartingIndex + MapsetPoolSize && i < Screen.AvailableMapsets.Count; i++)
            {
                var button = new MapsetSelectorItem(this, i)
                {
                    Y = i * SetSpacingY + 10,
                };

                // Fire click handler for this button if it is indeed the initial selected mapset.
                if (i == SelectedSet.Value)
                    button.FireButtonClickEvent();
                else
                    button.DisplayAsDeselected();

                AddContainedDrawable(button);
                MapsetButtonPool.Add(button);
            }
        }

        /// <summary>
        ///     Shifts the pool of buttons based on the the amount of scrolling the user has done.
        /// </summary>
        private void ShiftPoolBasedOnScroll()
        {
            if (MapsetButtonPool == null || MapsetButtonPool.Count == 0)
                return;

            if (ContentContainer.Y < PreviousContentContainerY)
                ShiftPoolWhenScrollingDown();
            else if (ContentContainer.Y > PreviousContentContainerY)
                ShiftPoolWhenScrollingUp();
        }

        /// <summary>
        ///     When scrolling down, this will handle recycling SelectorSet objects from off the top
        ///     of the screen, to the bottom.
        /// </summary>
        private void ShiftPoolWhenScrollingDown()
        {
             // Based on the content container's y position, calculate how many buttons are off-screen
            // (on the top of the window)
            var neededButtons = (int) Math.Round(Math.Abs(ContentContainer.Y + SetSpacingY * PoolStartingIndex) / SetSpacingY);

            // Here we check for if the amount of buttons scrolled up are greater than 0, it should always either be
            // 1 or 0 just by the nature of the scrolling, since it scrolls one at a time.
            // there should NEVER be a circumstance where we scroll more than 1 in a given frame.
            if (neededButtons > 0)
            {
                if (Screen.AvailableMapsets.ElementAtOrDefault(MapsetPoolSize + PoolStartingIndex) == null)
                    return;

                // Grab the button that had went off-screen
                var btn = MapsetButtonPool.First();

                // Change the y position of the button.
                btn.Y = 10 + (MapsetPoolSize + PoolStartingIndex) * SetSpacingY;

                // Since we're pooling change the associated mapset.
                btn.ChangeAssociatedMapset(MapsetPoolSize + PoolStartingIndex);

                if (btn.MapsetIndex == SelectedSet.Value)
                {
                    btn.DisplayAsSelected();
                    btn.Thumbnail.Image = MapManager.CurrentBackground;
                    btn.Thumbnail.Alpha = 1;
                }
                else
                    btn.DisplayAsDeselected();

                // Reorganize the buttons in the pool
                var reorganizedPoolList = new List<MapsetSelectorItem>();

                // Start with index 1. Since we're pooling forwards, the first item becomes the last,
                // last becomes the second to last, etc. So here we are essentially
                // creating that portion of the list. (tl;dr, just -1 to each index)
                for (var i = 1; i < MapsetButtonPool.Count; i++)
                    reorganizedPoolList.Add(MapsetButtonPool[i]);

                // Add the button that was pooled back to the list in the end.
                reorganizedPoolList.Add(btn);

                // Set the new list.
                MapsetButtonPool = reorganizedPoolList;
                PoolStartingIndex += neededButtons;
            }
        }

        /// <summary>
        ///     When scrolling up, this will handle recycling SelectorSet objects and placing the ones
        ///     from the bottom, back up to the top.
        /// </summary>
        private void ShiftPoolWhenScrollingUp()
        {
            if (Screen.AvailableMapsets.ElementAtOrDefault(PoolStartingIndex - 1) == null)
                return;

            // Calculate how many buttons need to be recycled.
            var neededButtons = (int) Math.Round((ContentContainer.Y + SetSpacingY * PoolStartingIndex) / SetSpacingY);

            if (neededButtons > 0)
            {
                // Grab the last button out of the pool, so we can recycle it at the top.
                var btn = MapsetButtonPool.Last();

                // The new index of the button.
                // since we're pooling backwards, it'd be 1 less than the current starting index.
                var newPoolIndex = PoolStartingIndex - 1;

                // Change the y position of the button to 1 before the pool index, (since we're pooling backwards)
                btn.Y = 10 + newPoolIndex * SetSpacingY;

                // Since we're pooling change the associated mapset.
                btn.ChangeAssociatedMapset(newPoolIndex);

                if (btn.MapsetIndex == SelectedSet.Value)
                {
                    btn.DisplayAsSelected();
                    btn.Thumbnail.Image = MapManager.CurrentBackground;
                    btn.Thumbnail.Alpha = 1;
                }
                else
                    btn.DisplayAsDeselected();

                // Reorganize the buttons in the pool.
                var reorganizedPoolList = new List<MapsetSelectorItem>();

                // Add the button at the first index, since it's needed at the top.
                reorganizedPoolList.Add(btn);

                for (var i = 0; i < MapsetButtonPool.Count - 1; i++)
                    reorganizedPoolList.Add(MapsetButtonPool[i]);

                // Set the new list.
                MapsetButtonPool = reorganizedPoolList;
                PoolStartingIndex -= neededButtons;
            }
        }

        /// <summary>
        ///     Selects a map of a given index.
        /// </summary>
        /// <param name="index"></param>
        public void SelectMap(int index)
        {
            if (Screen.AvailableMapsets.ElementAtOrDefault(index) == null)
                return;

            var foundButtonIndex = MapsetButtonPool.FindIndex(x => x.MapsetIndex == index);

            // If the button is already in the pool and is visible, select that button
            if (foundButtonIndex != -1 && foundButtonIndex < 8)
            {
                MapsetButtonPool[foundButtonIndex].Select();
                ScrollTo((-SelectedSet.Value + 4) * SetSpacingY, 2100);
            }
            // If it isn't, that must mean the scroll is too far away to see the next map,
            // so scroll back to the existing one.
            else
            {
                ScrollTo((-SelectedSet.Value + 4) * SetSpacingY, 2100);
            }
        }
    }
}
