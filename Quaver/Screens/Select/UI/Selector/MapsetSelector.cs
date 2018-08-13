using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Assets;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Backgrounds;
using Quaver.Scheduling;
using Quaver.Screens.Edit.UI;
using Wobble;
using Wobble.Assets;
using Wobble.Audio.Tracks;
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
        public const int MapsetPoolSize = 24;

        /// <summary>
        ///     The amount of space between each mapset.
        /// </summary>
        public const int SetSpacingY = 88;

        /// <summary>
        ///     The buttons that are currently in the mapset pool.
        /// </summary>
        public List<MapsetSelectorItem> MapsetButtons { get; private set; }

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

        /// <summary>
        ///     Time since the map was selected.
        /// </summary>
        private double TimeSinceMapSelected { get; set; }

        /// <summary>
        ///     Keeps track of if we've already initiated to load the background.
        /// </summary>
        private bool MapAssetLoadInitiated { get; set; }

        /// <summary>
        ///     The current map assets to load when selecting new map
        ///     Bitwise combination enum.
        /// </summary>
        private MapAssetsToLoad AssetsToLoad { get; set; } = 0;

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

            // Stop default input.
            InputEnabled = false;

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

            // Make the background fully black.
            BackgroundManager.Background.BrightnessSprite.Alpha = 1;
            StartLoadingMapAssets(MapAssetsToLoad.Audio | MapAssetsToLoad.Background);

            GenerateSetPool();
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            ShiftPoolWhenScrolling();
            PreviousContentContainerY = ContentContainer.Y;

            var game = (QuaverGame) GameBase.Game;

            // Determine when or not to have the scrolling input active.
            if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt) ||
                KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt) || game.VolumeController.IsActive)
            {
            }
            else
            {
                // Handle input here.
                if (MouseManager.CurrentState.ScrollWheelValue < MouseManager.PreviousState.ScrollWheelValue)
                    SelectMap(SelectedSet.Value + 1);

                if (MouseManager.CurrentState.ScrollWheelValue > MouseManager.PreviousState.ScrollWheelValue)
                    SelectMap(SelectedSet.Value - 1);
            }

            // Start counting up time to load the new background and song.
            // User has to be on the selected map for a period of 500 seconds.
            TimeSinceMapSelected += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (TimeSinceMapSelected >= 300 && !MapAssetLoadInitiated)
            {
                MapAssetLoadInitiated = true;

                if (AssetsToLoad.HasFlag(MapAssetsToLoad.Background))
                    BackgroundManager.Load(MapManager.Selected.Value);

                // Load new track, go to preview time, and start fading it in.

                if (AssetsToLoad.HasFlag(MapAssetsToLoad.Audio))
                {
                    try
                    {
                        AudioEngine.LoadCurrentTrack();
                        AudioEngine.Track.Seek(MapManager.Selected.Value.AudioPreviewTime);
                        AudioEngine.Track.Volume = 0;
                        AudioEngine.Track.Play();
                        AudioEngine.Track.Fade(ConfigManager.VolumeMusic.Value, 300);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            SelectedSet.UnHookEventHandlers();
            MapsetButtons.ForEach(x => x.Destroy());
            base.Destroy();
        }

        /// <summary>
        ///     Generates the pool of SongSelectorSets. This should only be used
        ///     when initially creating
        /// </summary>
        private void GenerateSetPool()
        {
            ContentContainer.Size = Size;
            ContentContainer.Height += 1 + (Screen.AvailableMapsets.Count - 8) * SetSpacingY + MapsetSelectorItem.BUTTON_HEIGHT / 2f + 200;

            // Destroy previous buttons if there are any in the poool.
            if (MapsetButtons != null && MapsetButtons.Count > 0)
                MapsetButtons.ForEach(x => x.Destroy());

            // Find and set the selected set.
            SelectedSet.Value = Screen.AvailableMapsets.FindIndex(x => x.Maps.Contains(MapManager.Selected.Value));

            // Set the map pool starting index
            if (SelectedSet.Value - MapsetPoolSize / 2 > 0)
                PoolStartingIndex = SelectedSet.Value - MapsetPoolSize / 2;

            // Make sure container is at the Y of the selected mapset.
            ContentContainer.Y = (-SelectedSet.Value + 2) * SetSpacingY;
            TargetY = ContentContainer.Y;
            PreviousTargetY = ContentContainer.Y;

            MapsetButtons = new List<MapsetSelectorItem>();

            for (var i = 0; i < Screen.AvailableMapsets.Count; i++)
            {
                var button = new MapsetSelectorItem(this, i)
                {
                    Y = i * SetSpacingY + 200,
                    DestroyIfParentIsNull = false
                };

                // Fire click handler for this button if it is indeed the initial selected mapset.
                if (i == SelectedSet.Value)
                    button.FireButtonClickEvent();
                else
                    button.DisplayAsDeselected();

                // Only add a certain amount of objects as contained and displayed drawables
                if (Math.Abs(i - PoolStartingIndex) <= MapsetPoolSize)
                    AddContainedDrawable(button);

                MapsetButtons.Add(button);
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

            var foundButtonIndex = MapsetButtons.FindIndex(x => x.MapsetIndex == index);

            // If the button is already in the pool and is visible, select that button
            if (foundButtonIndex == -1)
                return;

            MapsetButtons[foundButtonIndex].Select();
            ScrollTo((-SelectedSet.Value + 2) * SetSpacingY, 300);
        }

        /// <summary>
        ///     Shifts the amount of mapset buttons that are displayed based on how far
        ///     we've scrolled
        /// </summary>
        public void ShiftPoolWhenScrolling()
        {
            if (MapsetButtons == null || MapsetButtons.Count == 0)
                return;

            if (ContentContainer.Y < PreviousContentContainerY)
                ShiftPoolWhenScrollingDown();
            else if (ContentContainer.Y > PreviousContentContainerY)
                ShiftPoolWhenScrollingUp();
        }

        /// <summary>
        ///     Shifts the mapset pool when we're scrolling down.
        /// </summary>
        private void ShiftPoolWhenScrollingDown()
        {
             // Based on the content container's y position, calculate how many buttons are off-screen
            // (on the top of the window)
            var neededButtons = (int) Math.Round(Math.Abs(ContentContainer.Y + 600 + SetSpacingY * PoolStartingIndex) / SetSpacingY);

            if (neededButtons <= 0)
                return;

            if (Screen.AvailableMapsets.ElementAtOrDefault(MapsetPoolSize + PoolStartingIndex + neededButtons) == null)
                return;

            for (var i = 0; i < neededButtons; i++)
            {
                // Add the next buttons as a contained drawable.
                AddContainedDrawable(MapsetButtons[PoolStartingIndex + MapsetPoolSize + i]);

                if (PoolStartingIndex - i < 0)
                    break;

                // Remove old contained drawables.
                RemoveContainedDrawable(MapsetButtons[PoolStartingIndex - i]);
            }

            PoolStartingIndex += neededButtons;
        }

        /// <summary>
        ///     Shifts the button pool when scrolling upwards.
        /// </summary>
        private void ShiftPoolWhenScrollingUp()
        {
            // Calculate how many buttons need to be recycled.
            var neededButtons = (int) Math.Round((ContentContainer.Y + 600 + SetSpacingY * PoolStartingIndex) / SetSpacingY);

            if (neededButtons <= 0)
                return;

            for (var i = 0; i < neededButtons; i++)
            {
                RemoveContainedDrawable(MapsetButtons[PoolStartingIndex + MapsetPoolSize + i]);

                if (PoolStartingIndex - i < 0)
                    break;

                // Add the next buttons as a contained drawable.
                AddContainedDrawable(MapsetButtons[PoolStartingIndex - i]);
            }

            PoolStartingIndex -= neededButtons;
        }

        /// <summary>
        ///     Starts the background/audio load process.
        /// </summary>
        public void StartLoadingMapAssets(MapAssetsToLoad assets)
        {
            AssetsToLoad = assets;

            if (assets.HasFlag(MapAssetsToLoad.Audio))
                AudioEngine.Track?.Fade(0, 100);

            if (assets.HasFlag(MapAssetsToLoad.Background))
                BackgroundManager.FadeOut();

            TimeSinceMapSelected = 0;
            MapAssetLoadInitiated = false;
        }
    }
}
