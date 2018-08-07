using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Quaver.Database.Maps;
using Quaver.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Screens.Select.UI.Selector
{
    public class SongSelector : ScrollContainer
    {
        /// <summary>
        ///     Reference to the song select screen itself.
        /// </summary>
        private SelectScreen Screen { get; }

        /// <summary>
        ///     The amount of maps available in the pool.
        /// </summary>
        public const int MapsetPoolSize = 35;

        /// <summary>
        ///     The amount of space between each mapset.
        /// </summary>
        public const int SetSpacingY = 60;

        /// <summary>
        ///     The buttons that are currently in the mapset pool.
        /// </summary>
        public List<SongSelectorSet> MapsetButtonPool { get; private set; }

        /// <summary>
        ///     The starting index of the mapset button pool.
        /// </summary>
        private int PoolStartingIndex { get; set; } = 0;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public SongSelector(SelectScreen screen) : base(new ScalableVector2(600, 610), new ScalableVector2(600, 610 + SetSpacingY))
        {
            Screen = screen;

            Alignment = Alignment.MidRight;
            X = 0;
            Alpha = 0f;
            Tint = Color.Black;

            Scrollbar.Width = 10;
            Scrollbar.Tint = Color.White;

            GenerateSetPool();
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            ShiftPoolBasedOnScroll();

            base.Update(gameTime);
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
            ContentContainer.Height += 1 + (Screen.AvailableMapsets.Count - 10) * SetSpacingY + SongSelectorSet.BUTTON_HEIGHT / 2f;

            // Destroy previous buttons if there are any in the poool.
            if (MapsetButtonPool != null && MapsetButtonPool.Count > 0)
                MapsetButtonPool.ForEach(x => x.Destroy());

            // Create the initial list of buttons for the pool.
            MapsetButtonPool = new List<SongSelectorSet>();

            // Create set buttons.
            for (var i = PoolStartingIndex; i < PoolStartingIndex + MapsetPoolSize && i < Screen.AvailableMapsets.Count; i++)
            {
                var set = Screen.AvailableMapsets[i];

                var button = new SongSelectorSet(this, set) { Y = i * SetSpacingY };
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

            // Based on the content container's y position, calculate how many buttons are off-screen
            // (on the top of the window)
            var buttonsScrolledUp = (int) Math.Round(Math.Abs(ContentContainer.Y + SetSpacingY * PoolStartingIndex) / SetSpacingY);

            // Here we check for if the amount of buttons scrolled up are greater than 0, it should always either be
            // 1 or 0 just by the nature of the scrolling, since it scrolls one at a time.
            // there should NEVER be a circumstance where we scroll more than 1 in a given frame.
            if (buttonsScrolledUp > 0)
            {
                if (Screen.AvailableMapsets.ElementAtOrDefault(MapsetPoolSize + PoolStartingIndex) == null)
                    return;

                // Grab the button that had went off-screen
                var btn = MapsetButtonPool.First();

                // Change the y position of the button.
                btn.Y = (MapsetPoolSize + PoolStartingIndex) * SetSpacingY;

                // Since we're pooling change the associated mapset.
                btn.ChangeAssociatedMapset(Screen.AvailableMapsets[MapsetPoolSize + PoolStartingIndex]);

                // Reorganize the buttons in the pool
                var reorganizedPoolList = new List<SongSelectorSet>();

                // Start with index 1. Since we're pooling forwards, the first item becomes the last,
                // last becomes the second to last, etc. So here we are essentially
                // creating that portion of the list. (tl;dr, just -1 to each index)
                for (var j = 1; j < MapsetButtonPool.Count; j++)
                    reorganizedPoolList.Add(MapsetButtonPool[j]);

                // Add the button that was pooled back to the list in the end.
                reorganizedPoolList.Add(btn);

                // Set the new list.
                MapsetButtonPool = reorganizedPoolList;
                PoolStartingIndex += buttonsScrolledUp;
            }
        }
    }
}
