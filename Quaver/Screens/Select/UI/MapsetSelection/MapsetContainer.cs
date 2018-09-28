using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Graphics.Backgrounds;
using Quaver.Modifiers;
using Quaver.Screens.Select.UI.MapInfo.Leaderboards;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Screens;
using Wobble.Window;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Quaver.Screens.Select.UI.MapsetSelection
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
        public List<MapsetButton> MapsetButtons { get; private set; }

        /// <summary>
        ///     The original size of the mapset container.
        /// </summary>
        private static ScalableVector2 OriginalSize => new ScalableVector2(550, WindowManager.Height / 1.25f - 7);

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
            Alignment = Alignment.MidRight;
            Tint = Color.Black;
            Alpha = 0;

            Scrollbar.Width = 10;
            Scrollbar.Tint = Color.White;

            ScrollSpeed = 150;
            EasingType = Easing.EaseOutQuint;
            TimeToCompleteScroll = 2100;

            // Select the first map of the first mapset for now.
            if (MapManager.Selected.Value == null)
                MapManager.Selected.Value = Screen.AvailableMapsets.First().Maps.First();

            // BG all the way
            BackgroundManager.Background.BrightnessSprite.Alpha = 1;

            // Permit backgrounds to fade in now.
            BackgroundManager.PermittedToFadeIn = true;

            // Listen to when mods get changed
            ModManager.ModsChanged += OnModsChanged;

            InitializeMapsetButtons();
            SelectMap(SelectedMapsetIndex, Screen.AvailableMapsets[SelectedMapsetIndex].Maps[SelectedMapIndex], true);
        }

        /// <summary>
        ///     Whenever game modifiers changed, update the text of it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModsChanged(object sender, ModsChangedEventArgs e)
        {
            // Select current map and recalculate diff
            SelectMap(SelectedMapsetIndex, Screen.AvailableMapsets[SelectedMapsetIndex].Maps[SelectedMapIndex], true, true);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Don't handle if there are any dialogs up.
            if (DialogManager.Dialogs.Count != 0)
            {
                InputEnabled = false;
                base.Update(gameTime);
                return;
            }

            // Handle pool shifting when scrolling up or down.
            if (ContentContainer.Y < PreviousContentContainerY)
                HandlePoolShifting(Direction.Forward);
            else if (ContentContainer.Y > PreviousContentContainerY)
                HandlePoolShifting(Direction.Backward);

            // Update the previous y, AFTER checking and handling the pool shifting.
            PreviousContentContainerY = ContentContainer.Y;

            // Select next mapset.
            if (KeyboardManager.IsUniqueKeyPress(Keys.Right))
            {
                if (MapsetButtons.ElementAtOrDefault(SelectedMapsetIndex + 1) != null)
                    MapsetButtons[SelectedMapsetIndex + 1].FireButtonClickEvent();
            }

            // Select previous mapset.
            if (KeyboardManager.IsUniqueKeyPress(Keys.Left))
            {
                if (MapsetButtons.ElementAtOrDefault(SelectedMapsetIndex - 1) != null)
                    MapsetButtons[SelectedMapsetIndex - 1]?.FireButtonClickEvent();
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            ModManager.ModsChanged -= OnModsChanged;
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

            // If the mapset can't be found for some reason, default it to the first one.
            if (SelectedMapsetIndex == -1)
            {
                SelectedMapsetIndex = 0;
                SelectedMapIndex = 0;
            }

            SelectedMapIndex = Screen.AvailableMapsets[SelectedMapsetIndex].Maps.FindIndex(x => x == MapManager.Selected.Value);

            // If the map can't be found, then default it to the first one.
            if (SelectedMapIndex == -1)
                SelectedMapIndex = 0;

            // Console.WriteLine($"Set: {SelectedMapsetIndex}, Index: {SelectedMapIndex} - {MapManager.Selected.Value.Title}");

            // Based on the currently selected mapset, calculate starting index of which to update and draw
            // the mapset buttons in the container.
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
        public void SelectMap(int mapsetIndex, Map map, bool forceAssetLoad = false, bool forceDifficultySelectorUpdate = false)
        {
            // Grab the previous mapset & map indexes.
            var previousMapsetIndex = SelectedMapsetIndex;
            var previousMapIndex = SelectedMapIndex;

            // Grab the previous mapset and map for quick reference.
            var previousMapset = Screen.AvailableMapsets[previousMapsetIndex];
            var previousMap = Screen.AvailableMapsets[previousMapsetIndex].Maps[previousMapIndex];

            // If we're changing mapsets, display the old one as deselected.
            // and the new one as selected.
            if (mapsetIndex != previousMapsetIndex)
            {
                MapsetButtons[previousMapsetIndex].DisplayAsDeselected();
                MapsetButtons[mapsetIndex].DisplayAsSelected();
            }

            // Change the selected indexes.
            SelectedMapsetIndex = mapsetIndex;
            SelectedMapIndex = Screen.AvailableMapsets[SelectedMapsetIndex].Maps.FindIndex(x => x.Md5Checksum == map.Md5Checksum);

            // Calculate difficulty for every map in the mapset
            // TODO: there should already be a general difficulty of every map from the cache
            // TODO: StrainRatingData should only be applied to a single selected map, so we don't have to calculate difficulties here
            Console.WriteLine("------------------------------------");
            Console.WriteLine(Screen.AvailableMapsets[SelectedMapsetIndex].Title);
            foreach (var curMap in Screen.AvailableMapsets[SelectedMapsetIndex].Maps)
            {
                if (curMap.DifficultyRatingWithCurrentRate <= 0 || forceDifficultySelectorUpdate)
                {
                    var diff = (StrainSolverKeys)curMap.SolveDifficulty(ModHelper.GetRateFromMods(ModManager.Mods));
                    Console.WriteLine(curMap.DifficultyName);
                    Console.WriteLine(diff.OverallDifficulty + ", " + diff.AverageNoteDensity);
                    Console.WriteLine("Roll/Trill: " + diff.Roll);
                    Console.WriteLine("Simple Jack: " + diff.SJack);
                    Console.WriteLine("Tech Jack: " + diff.TJack);
                    Console.WriteLine("Bracket: " + diff.Bracket);
                    Console.WriteLine(diff.DebugString);
                    Console.WriteLine();
                }
            }

            // Happens when a user searches for a map that's already in the selected set.
            // TODO: Fix this.
            if (SelectedMapIndex == -1)
            {
                ScreenManager.ChangeScreen(new SelectScreen());
                return;
            }

            // Handle the clearing of scores.
            switch (ConfigManager.SelectLeaderboardSection.Value)
            {
                // Always clear scores if we're on local.
                case LeaderboardSectionType.Local:
                    if (MapManager.Selected.Value != map)
                        MapManager.Selected.Value?.ClearScores();
                    break;
                // TODO: If we're on global, we'll want to cache scores to reduce the amount
                // of requests made.
                case LeaderboardSectionType.Global:
                    break;
            }

            // Change the actual map.
            MapManager.Selected.Value = map;

            // Scroll to the new mapset.
            ScrollTo((-SelectedMapsetIndex + 1) * (MapsetButton.BUTTON_HEIGHT + MapsetButton.BUTTON_Y_SPACING), 2100);

            // Update the leaderboard with the new map.
            ConfigManager.SelectLeaderboardSection.Value = LeaderboardSectionType.DifficultySelection;
            View.MapInfoContainer?.Leaderboard?.UpdateLeaderboard();

            // If necessary, change the associated mapset with the difficulty leaderboard section.
            // Only necessary if we're changing mapsets and not maps.
            if (previousMapset != Screen.AvailableMapsets[SelectedMapsetIndex]|| forceDifficultySelectorUpdate)
                View.MapInfoContainer?.Leaderboard?.UpdateLeaderboard(Screen.AvailableMapsets[SelectedMapsetIndex]);

            // Load background if it doesn't have the same path, or if we're forcing it.
            if (MapManager.GetBackgroundPath(previousMap) != MapManager.GetBackgroundPath(MapManager.Selected.Value) ||
                forceAssetLoad)
            {
                BackgroundManager.Load(map, 60);

                // Update the banner.
                View.MapInfoContainer?.Banner?.UpdateSelectedMap(true);
            }
            else
            {
                // Update the banner.
                View.MapInfoContainer?.Banner?.UpdateSelectedMap(false);
            }


            // Load auto track if it doesn't have the same path, or if we're forcing the load.
            // ReSharper disable once InvertIf
            if (MapManager.GetAudioPath(previousMap) != MapManager.GetAudioPath(MapManager.Selected.Value) || forceAssetLoad)
            {
                if (AudioEngine.Track != null && AudioEngine.Track.IsPlaying)
                    AudioEngine.Track.Fade(0, 200);

                try
                {
                    AudioEngine.LoadCurrentTrack();

                    if (AudioEngine.Track == null)
                        return;

                    AudioEngine.Track.Seek(map.AudioPreviewTime);
                    AudioEngine.Track.Volume = 0;
                    AudioEngine.Track.Play();
                    AudioEngine.Track.Fade(ConfigManager.VolumeMusic.Value, 800);
                }
                catch (Exception)
                {
                    // ignored. We simply don't care if it can't be played in this case.
                }
            }
        }

        /// <summary>
        ///     Initializes the mapset buttons again with new mapsets. Used for searching
        ///     and ordering maps.
        /// </summary>
        /// <param name="sets"></param>
        public void ReInitializeMapsetButtonsWithNewSets(List<Mapset> sets)
        {
            // Don't continue if there aren't any mapsets.
            if (sets.Count == 0)
                return;

            // Set the new available sets, and reinitialize the mapset buttons.
            Screen.AvailableMapsets = sets;
            InitializeMapsetButtons();

            // Check to see if the current mapset is already in the new search.
            var foundMapset = sets.FindIndex(x => x.Directory == MapManager.Selected.Value.Mapset.Directory);

            // If the new map is in the search, go straight to it.
            if (foundMapset != -1)
            {
                SelectMap(foundMapset, MapManager.Selected.Value, false, true);
                ChangeMapsetButtonThumbnail();
            }
            // Select the first map in the first mapset, if it's a completely new mapset.
            else if (MapManager.Selected.Value != Screen.AvailableMapsets.First().Maps.First())
                SelectMap(0, Screen.AvailableMapsets.First().Maps.First(), true, true);
            // Otherwise just make sure the mapset thumbnail is up to date anyway.
            else
                ChangeMapsetButtonThumbnail();
        }

        /// <summary>
        ///     Makes sure the mapset button's thumbnail is up to date with the newly selected map.
        /// </summary>
        private void ChangeMapsetButtonThumbnail()
        {
            var thumbnail = MapsetButtons[SelectedMapsetIndex].Thumbnail;

            thumbnail.Image = BackgroundManager.Background.Image;
            thumbnail.Transformations.Clear();
            var t = new Transformation(TransformationProperty.Alpha, Easing.Linear, 0, 1, 250);
            thumbnail.Transformations.Add(t);
        }
    }
}