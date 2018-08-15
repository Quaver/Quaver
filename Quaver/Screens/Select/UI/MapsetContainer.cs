using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Graphics.Backgrounds;
using Quaver.Graphics.Notifications;
using Quaver.Screens.Menu;
using Wobble.Audio;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Screens;
using Wobble.Window;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Quaver.Screens.Select.UI
{
    public class MapsetContainer : ScrollContainer
    {
        /// <summary>
        ///     Reference to the screen itself.
        /// </summary>
        public SelectScreen Screen { get; }

        /// <summary>
        ///     Reference to the SelectScreenView
        /// </summary>
        public SelectScreenView View { get; }

        /// <summary>
        ///     The buttons for each mapset.
        /// </summary>
        private List<MapsetButton> MapsetButtons { get; set; }

        /// <summary>
        ///     Interface to select the difficulties of the map.
        /// </summary>
        public DifficultySelector DifficultySelector { get; }

        /// <summary>
        ///     The original size of the mapset container.
        /// </summary>
        private static ScalableVector2 OriginalSize => new ScalableVector2(550, WindowManager.Height);

        /// <summary>
        ///     The y of the first button.
        /// </summary>
        private static int FIRST_BUTTON_Y => 250;

        /// <summary>
        ///     The amount of buttons available in the pool to be updated/drawn at once.
        /// </summary>
        private static int BUTTON_POOL_SIZE => 20;

        /// <summary>
        ///     The index of the mapset in which to start updating/drawing the button for.
        ///     This will shift in accordance to BUTTON_POOL_SIZE.
        /// </summary>
        public int PoolStartingIndex { get; private set; }

        /// <summary>
        ///     The index of the selected mapset.
        /// </summary>
        public int SelectedMapsetIndex { get; private set; }

        /// <summary>
        ///     The index of the selected map.
        /// </summary>
        public int SelectedMapIndex { get; private set; }

        /// <summary>
        ///     The previous ContentContainer's y posiiton, so we can compare it against
        ///     the current to determine how to shift the pool index.
        /// </summary>
        private float PreviousContentContainerY { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="view"></param>
        public MapsetContainer(SelectScreen screen, SelectScreenView view)
            : base(OriginalSize, OriginalSize)
        {
            Screen = screen;
            View = view;

            InputEnabled = true;
            Alignment = Alignment.TopRight;
            Tint = Color.Black;
            Alpha = 0;

            Scrollbar.Width = 10;
            Scrollbar.Tint = Color.White;

            ScrollSpeed = 150;
            EasingType = Easing.EaseOutQuint;
            TimeToCompleteScroll = 2100;

            // Select the first map of the first mapset for now.
            if (MapManager.Selected.Value == null)
                MapManager.Selected.Value = MapManager.Mapsets.First().Maps.First();

            // TODO: Set parent and all that jazz in the map info section.
            DifficultySelector = new DifficultySelector(this)
            {
                Parent = View.Container,
                Alignment = Alignment.MidCenter,
                X = -100
            };

            // BG all the way
            BackgroundManager.Background.BrightnessSprite.Alpha = 1;

            // Permit backgrounds to fade in now.
            BackgroundManager.PermittedToFadeIn = true;

            InitializeMapsetButtons();
            SelectMap(SelectedMapsetIndex, Screen.AvailableMapsets[SelectedMapsetIndex].Maps[SelectedMapIndex]);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Handle pool shifting when scrolling up or down.
            if (ContentContainer.Y < PreviousContentContainerY)
                HandlePoolShifting(Direction.Forward);
            else if (ContentContainer.Y > PreviousContentContainerY)
                HandlePoolShifting(Direction.Backward);

            // Update the previous y, AFTER checking and handling the pool shifting.
            PreviousContentContainerY = ContentContainer.Y;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Right))
            {
                if (MapsetButtons.ElementAtOrDefault(SelectedMapsetIndex + 1) != null)
                    MapsetButtons[SelectedMapsetIndex + 1].FireButtonClickEvent();
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Left))
            {
                if (MapsetButtons.ElementAtOrDefault(SelectedMapsetIndex - 1) != null)
                    MapsetButtons[SelectedMapsetIndex - 1].FireButtonClickEvent();
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            MapsetButtons.ForEach(x => x.Destroy());

            base.Destroy();
        }

        /// <summary>
        ///     Initializes all of the mapset buttons and determines which ones
        ///     to display.
        /// </summary>
        private void InitializeMapsetButtons()
        {
            ContentContainer.Size = Size;

            // Calculate the height of the content container based on how many available mapsets there are.
            ContentContainer.Height = 1 + Screen.AvailableMapsets.Count * (MapsetButton.BUTTON_Y_SPACING + MapsetButton.BUTTON_HEIGHT) + FIRST_BUTTON_Y * 2;

            // Destroy previous buttons if there are any in the pool.
            if (MapsetButtons != null && MapsetButtons.Count > 0)
                MapsetButtons.ForEach(x => x.Destroy());

            // Find the index of the selected mapset and map.
            SelectedMapsetIndex = Screen.AvailableMapsets.FindIndex(x => x.Maps.Contains(MapManager.Selected.Value));
            SelectedMapIndex = MapManager.Selected.Value.Mapset.Maps.FindIndex(x => x == MapManager.Selected.Value);

            // Based on the currently selected mapset, calculate starting index of which to update and draw
            // the mapset buttons in the container.
            Console.WriteLine($"Set: {SelectedMapsetIndex}, Index: {SelectedMapIndex} - {MapManager.Selected.Value.Title}");

            if (SelectedMapsetIndex < BUTTON_POOL_SIZE / 2)
                PoolStartingIndex = 0;
            else if (SelectedMapsetIndex + BUTTON_POOL_SIZE > Screen.AvailableMapsets.Count)
                PoolStartingIndex = Screen.AvailableMapsets.Count - BUTTON_POOL_SIZE;
            else
                PoolStartingIndex = SelectedMapsetIndex - BUTTON_POOL_SIZE / 2;

            MapsetButtons = new List<MapsetButton>();

            for (var i = 0; i < Screen.AvailableMapsets.Count; i++)
            {
                var button = new MapsetButton(this, Screen.AvailableMapsets[i], i)
                {
                    Y = i * (MapsetButton.BUTTON_HEIGHT + MapsetButton.BUTTON_Y_SPACING) + FIRST_BUTTON_Y
                };

                // Make sure the correct mapset is shown as selected
                if (i == SelectedMapsetIndex)
                    button.DisplayAsSelected();
                else
                    button.DisplayAsDeselected();

                // Add the button as a contained drawable to the container if it is in range of the shifted pool.
                if (i >= PoolStartingIndex && i < PoolStartingIndex + BUTTON_POOL_SIZE)
                    AddContainedDrawable(button);

                MapsetButtons.Add(button);
            }

            // Set the original ContentContainerY
            // Make sure container is at the Y of the selected mapset.
            // 1 = a couple mapsets afterwards to center the set.
            ContentContainer.Y = (-SelectedMapsetIndex + 1) * (MapsetButton.BUTTON_HEIGHT + MapsetButton.BUTTON_Y_SPACING);
            PreviousContentContainerY = ContentContainer.Y - 90;
            TargetY = PreviousContentContainerY;
            PreviousTargetY = PreviousContentContainerY;
            ContentContainer.Transformations.Clear();
        }

        /// <summary>
        ///     Handles shifting the pool based on the scroll direction.
        /// </summary>
        /// <param name="direction"></param>
        private void HandlePoolShifting(Direction direction)
        {
            var buttonTotalHeight = MapsetButton.BUTTON_HEIGHT + MapsetButton.BUTTON_Y_SPACING;

            switch (direction)
            {
                // User is scrolling down
                case Direction.Forward:
                    // First run a check of if we even have a single map on the bottom.
                    if (MapsetButtons.ElementAtOrDefault(PoolStartingIndex + BUTTON_POOL_SIZE) == null)
                        return;

                    // Calculate the amount of buttons needed since we're scrolling forward.
                    var neededFwdButtons = (int) Math.Round(ContentContainer.Y + FIRST_BUTTON_Y * 2 + buttonTotalHeight * PoolStartingIndex) / buttonTotalHeight;

                    // It's so backwards, be It returns a negative number in the beginning because of the extra space.
                    // so we'll just return here if we don't actually need buttons.
                    if (neededFwdButtons > 0)
                        return;

                    // Since we're shifting forward, we can safely remove the button that has gone off-screen.
                    RemoveContainedDrawable(MapsetButtons[PoolStartingIndex]);

                    // Now add the button that is forward.
                    AddContainedDrawable(MapsetButtons[PoolStartingIndex + BUTTON_POOL_SIZE]);

                    // Increment the starting index to shift it.
                    PoolStartingIndex++;
                    break;
                // User is scrolling up.
                case Direction.Backward:
                    // Run a check on if we have a putton previous to the PoolStartingIndex, before we even
                    // bother trying to shift the pool.
                    if (MapsetButtons.ElementAtOrDefault(PoolStartingIndex - 1) == null)
                        return;

                    // Calculate amount of buttons needed since scrolling backwards.
                    var neededBwdButtons = (int) Math.Round((ContentContainer.Y + FIRST_BUTTON_Y * 2 + buttonTotalHeight * PoolStartingIndex) / buttonTotalHeight);

                    if (neededBwdButtons < 0)
                        return;

                    // Since we're scrolling up, we need to shift backwards.
                    // Remove the drawable from the bottom one.
                    RemoveContainedDrawable(MapsetButtons[PoolStartingIndex + BUTTON_POOL_SIZE - 1]);
                    AddContainedDrawable(MapsetButtons[PoolStartingIndex - 1]);

                    PoolStartingIndex--;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        /// <summary>
        ///     Selects a map from a given mapset.
        /// </summary>
        public void SelectMap(int mapsetIndex, Map map)
        {
            // If we're changing mapsets, display the old one as deselected.
            // and the new one as selected.
            if (mapsetIndex != SelectedMapsetIndex)
            {
                MapsetButtons[SelectedMapsetIndex].DisplayAsDeselected();
                MapsetButtons[mapsetIndex].DisplayAsSelected();
            }

            SelectedMapsetIndex = mapsetIndex;
            SelectedMapIndex = Screen.AvailableMapsets[SelectedMapsetIndex].Maps.FindIndex(x => x == map);

            // Something must've really messed up. Shouldn't ever get this.
            if (SelectedMapIndex == -1)
            {
                NotificationManager.Show(NotificationLevel.Error, "Something went extremely wrong when selecting that map.");
                ScreenManager.ChangeScreen(new MainMenuScreen());
            }

            // Change the actual map.
            MapManager.Selected.Value = map;

            // Scroll to the new mapset.
            ScrollTo((-SelectedMapsetIndex + 1) * (MapsetButton.BUTTON_HEIGHT + MapsetButton.BUTTON_Y_SPACING), 2100);
            BackgroundManager.Load(map);

            if (AudioEngine.Track != null && AudioEngine.Track.IsPlaying)
                AudioEngine.Track.Fade(0, 200);

            try
            {
                AudioEngine.LoadCurrentTrack();
                AudioEngine.Track.Seek(map.AudioPreviewTime);
                AudioEngine.Track.Volume = 0;
                AudioEngine.Track.Play();
                AudioEngine.Track.Fade(ConfigManager.VolumeMusic.Value, 800);
            }
            catch (Exception e)
            {
                // ignored
            }
        }
    }
}