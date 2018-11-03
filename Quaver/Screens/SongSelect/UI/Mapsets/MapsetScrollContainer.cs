using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Backgrounds;
using Quaver.Graphics.Overlays.Chat.Components.Users;
using Quaver.Resources;
using Quaver.Scheduling;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Logging;
using Wobble.Window;

namespace Quaver.Screens.SongSelect.UI.Mapsets
{
    public class MapsetScrollContainer : ScrollContainer
    {
        /// <summary>
        ///     Reference to the parent screen.
        /// </summary>
        public SongSelectScreen Screen => View.Screen as SongSelectScreen;

        /// <summary>
        ///     Reference to the parent ScreenView.
        /// </summary>
        public SongSelectScreenView View { get; }

        /// <summary>
        ///     The original size of the container for reference.
        /// </summary>
        public ScalableVector2 OriginalContainerSize { get; }

        /// <summary>
        ///     The buffer of drawable mapsets displayed in the container.
        /// </summary>
        public List<DrawableMapset> MapsetBuffer { get; private set; }

        /// <summary>
        ///     The total amount of mapsets that are displayed at a time.
        /// </summary>
        public static int MAX_MAPSETS_SHOWN { get; } = 15;

        /// <summary>
        ///     The position (index) of available mapsets in which will be shown
        ///     + <see cref="MAX_MAPSETS_SHOWN"/>
        /// </summary>
        public int PoolStartingIndex { get; set; }

        /// <summary>
        ///     Keeps track of the Y position of the content container in the previous frame
        ///     So we can know how to shift the pool.
        /// </summary>
        private float PreviousContentContainerY { get; set; }

        /// <summary>
        ///     The amount of y spacing between each mapset.
        /// </summary>
        private static int YSpacing { get; } = 10;

        /// <summary>
        ///     The amount of space before the first mapset.
        /// </summary>
        private static int YSpaceBeforeFirstSet = 300;

        /// <summary>
        ///     The index of the currently selected maps in <see cref="Screen"/> AvaialableMapsets
        /// </summary>
        public int SelectedMapsetIndex { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="view"></param>
        public MapsetScrollContainer(SongSelectScreenView view) : base(
            new ScalableVector2(515, WindowManager.Height - 54 * 2 - 2),
            new ScalableVector2(515, WindowManager.Height - 54 * 2 - 2))
        {
            View = view;
            OriginalContainerSize = Size;

            Alpha = 0;

            InputEnabled = true;
            Scrollbar.Tint = Color.White;
            Scrollbar.Width = 5;
            Scrollbar.X += 10;
            ScrollSpeed = 150;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1500;

            // Find the index of the selected map.
            SelectedMapsetIndex = Screen.AvailableMapsets.FindIndex(x => x.Maps.Contains(MapManager.Selected.Value));

            BackgroundHelper.Background.Dim = 50;

            // Load the background of the map if necessary.
            if (BackgroundHelper.Map != MapManager.Selected.Value)
                LoadNewBackgroundIfNecessary(null);

            InitializeMapsetBuffer();
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

            base.Update(gameTime);
        }

        /// <summary>
        ///     Selects a new select map/mapset.
        /// </summary>
        public void SelectMap(int mapsetIndex, Map map, bool forceSelect = false)
        {
            var currentMapsetIndex = SelectedMapsetIndex;

            // Check if the same mapset is being selected.
            if (currentMapsetIndex == mapsetIndex && !forceSelect)
            {
                Logger.Debug("Changing to same mapset.", LogType.Runtime);
                return;
            }

            // Find the currently selected mapset.
            var selectedMapset = MapsetBuffer.Find(x => x.MapsetIndex == SelectedMapsetIndex);
            var nextMapset = MapsetBuffer.Find(x => x.MapsetIndex == mapsetIndex);

            // Set the new mapset.
            SelectedMapsetIndex = mapsetIndex;

            // Grab a reference to the previous map.
            var previousMap = MapManager.Selected.Value;

            // Update the newly selected map.
            MapManager.Selected.Value = map;

            selectedMapset?.DisplayAsDeselected();
            nextMapset?.DisplayAsSelected(MapManager.Selected.Value);

            // Scroll the the place where the map is.
            ScrollTo((-SelectedMapsetIndex -3) * DrawableMapset.HEIGHT + (-SelectedMapsetIndex - 3) * YSpacing + YSpaceBeforeFirstSet, 2100);

            // Load new audio if needed.
            LoadNewAudioTrackIfNecessary(previousMap);
            LoadNewBackgroundIfNecessary(previousMap);
        }

        /// <summary>
        ///     Selects one mapset to the left/right.
        /// </summary>
        /// <param name="direction"></param>
        public void SelectNextMapset(Direction direction)
        {
            switch (direction)
            {
                case Direction.Forward:
                    var nextMapset = SelectedMapsetIndex + 1;

                    if (Screen.AvailableMapsets.ElementAtOrDefault(nextMapset) == null)
                        return;

                    SelectMap(nextMapset, Screen.AvailableMapsets[nextMapset].PreferredMap
                                          ?? Screen.AvailableMapsets[nextMapset].Maps.First());
                    break;
                case Direction.Backward:
                    var previousMapset = SelectedMapsetIndex - 1;

                    if (Screen.AvailableMapsets.ElementAtOrDefault(previousMapset) == null)
                        return;

                    SelectMap(previousMapset, Screen.AvailableMapsets[previousMapset].PreferredMap
                                              ?? Screen.AvailableMapsets[previousMapset].Maps.First());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        /// <summary>
        ///     Initializes the container with new mapsets.
        /// </summary>
        public void InitializeWithNewSets()
        {
            lock (MapsetBuffer)
            {
                MapsetBuffer.ForEach(x => x.Destroy());
                MapsetBuffer.Clear();

                var selectedMapIndex = Screen.AvailableMapsets.FindIndex(x => x.Maps.Contains(MapManager.Selected.Value));

                if (selectedMapIndex != -1)
                {
                    SelectedMapsetIndex = selectedMapIndex;
                    SetPoolStartingIndex();
                    InitializeMapsetBuffer();
                    return;
                }

                if (Screen.AvailableMapsets.Count <= 0)
                    return;

                SelectedMapsetIndex = 0;
                SetPoolStartingIndex();
                InitializeMapsetBuffer();

                var mapset = Screen.AvailableMapsets[SelectedMapsetIndex];
                SelectMap(SelectedMapsetIndex, mapset.PreferredMap ?? mapset.Maps.First(), true);
            }
        }

        private void RefreshMapsetBuffer()
        {
            for (var i = 0; i < MapsetBuffer.Count; i++)
            {
                // Mapset exists, so we can safely update it.
                if (PoolStartingIndex + i < Screen.AvailableMapsets.Count)
                {
                    var mapset = MapsetBuffer[i];
                    mapset.UpdateWithNewMapset(Screen.AvailableMapsets[PoolStartingIndex + i], PoolStartingIndex + i);

                    mapset.Y = (PoolStartingIndex + i) * DrawableMapset.HEIGHT + (PoolStartingIndex + i) * YSpacing + YSpaceBeforeFirstSet;

                    if (i == SelectedMapsetIndex)
                        mapset.DisplayAsSelected(MapManager.Selected.Value);

                    if (mapset.Parent != this)
                        AddContainedDrawable(mapset);
                }
                else
                {
                    if (MapsetBuffer[i].Parent == this)
                        RemoveContainedDrawable(MapsetBuffer[i]);
                }
            }

            RecalculateContainerHeight();
            SnapToInitialMapset();
            UpdateButtonSelectedStatus();
        }

        /// <summary>
        ///     Initializes all of the mapsets in the set.
        /// </summary>
        private void InitializeMapsetBuffer()
        {
            MapsetBuffer = new List<DrawableMapset>(MAX_MAPSETS_SHOWN);

            SetPoolStartingIndex();

            // Create MAX_MAPSETS_SHOWN amount of DrawableMapsets.
            for (var i = 0; i < MAX_MAPSETS_SHOWN && i < Screen.AvailableMapsets.Count; i++)
            {
                var mapset = new DrawableMapset(this)
                {
                    Alignment = Alignment.TopRight,
                    Y = (PoolStartingIndex + i) * DrawableMapset.HEIGHT + (PoolStartingIndex + i) * YSpacing + YSpaceBeforeFirstSet,
                    DestroyIfParentIsNull = false
                };

                mapset.UpdateWithNewMapset(Screen.AvailableMapsets[PoolStartingIndex + i], PoolStartingIndex + i);
                MapsetBuffer.Add(mapset);

                if (i >= Screen.AvailableMapsets.Count)
                    continue;

                AddContainedDrawable(mapset);

                if (i == SelectedMapsetIndex)
                    mapset.DisplayAsSelected(MapManager.Selected.Value);
            }

            RecalculateContainerHeight();
            SnapToInitialMapset();
            UpdateButtonSelectedStatus();
        }

        /// <summary>
        ///    Based on the currently selected mapset, calculate starting index of which to update and draw
        ///    the mapset buttons in the container.
        /// </summary>
        private void SetPoolStartingIndex()
        {
            if (SelectedMapsetIndex < MAX_MAPSETS_SHOWN / 2)
                PoolStartingIndex = 0;
            else if (SelectedMapsetIndex + MAX_MAPSETS_SHOWN > Screen.AvailableMapsets.Count)
                PoolStartingIndex = Screen.AvailableMapsets.Count - MAX_MAPSETS_SHOWN;
            else
                PoolStartingIndex = SelectedMapsetIndex - MAX_MAPSETS_SHOWN / 2;
        }

        /// <summary>
        ///     Updates all the buttons in the buffer and makes it so that they display
        ///     the correct display status.
        /// </summary>
        private void UpdateButtonSelectedStatus() => MapsetBuffer.ForEach(x =>
        {
            if (x.MapsetIndex == SelectedMapsetIndex)
                x.DisplayAsSelected(MapManager.Selected.Value);
            else
                x.DisplayAsDeselected();
        });

        /// <summary>
        ///     Calculates the height of the container based on the amount of users.
        /// </summary>
        private void RecalculateContainerHeight()
        {
            var totalUserHeight = DrawableMapset.HEIGHT * Screen.AvailableMapsets.Count + Screen.AvailableMapsets.Count * YSpacing + YSpaceBeforeFirstSet * 2;

            if (totalUserHeight > Height)
                ContentContainer.Height = totalUserHeight;
            else
                ContentContainer.Height = Height;
        }

        /// <summary>
        ///     Handles shifting of the pool when scrolling.
        /// </summary>
        /// <param name="direction"></param>
        private void HandlePoolShifting(Direction direction)
        {
            switch (direction)
            {
                case Direction.Forward:
                    // If there are no available mapsets then there's no need to do anything.
                    if (Screen.AvailableMapsets.ElementAtOrDefault(PoolStartingIndex) == null
                        || Screen.AvailableMapsets.ElementAtOrDefault(PoolStartingIndex + MAX_MAPSETS_SHOWN) == null)
                        return;

                    var firstMapset = MapsetBuffer.First();

                    // Check if the object is in the rect of the ScrollContainer.
                    // If it is, then there's no updating that needs to happen.
                    if (!Rectangle.Intersect(firstMapset.ScreenRectangle.ToRectangle(), ScreenRectangle.ToRectangle()).IsEmpty)
                        return;

                    // Update the mapset's information and y position.
                    firstMapset.Y = (PoolStartingIndex + MAX_MAPSETS_SHOWN) * DrawableMapset.HEIGHT +
                                    (PoolStartingIndex + MAX_MAPSETS_SHOWN) * YSpacing + YSpaceBeforeFirstSet;

                    lock (Screen.AvailableMapsets)
                        firstMapset.UpdateWithNewMapset(Screen.AvailableMapsets[PoolStartingIndex + MAX_MAPSETS_SHOWN],
                            PoolStartingIndex + MAX_MAPSETS_SHOWN);

                    // Circuluarly Shift the list forward one.
                    MapsetBuffer.Remove(firstMapset);
                    MapsetBuffer.Add(firstMapset);

                    // Make sure the set is corrected selected/deselected
                    if (PoolStartingIndex + MAX_MAPSETS_SHOWN == SelectedMapsetIndex)
                        firstMapset.DisplayAsSelected(MapManager.Selected.Value);
                    else
                        firstMapset.DisplayAsDeselected();

                    PoolStartingIndex++;
                    break;
                case Direction.Backward:
                    // If there are no previous available user then there's no need to shift.
                    if (Screen.AvailableMapsets.ElementAtOrDefault(PoolStartingIndex - 1) == null)
                        return;

                    var lastMapset = MapsetBuffer.Last();

                    // Check if the object is in the rect of the ScrollContainer.
                    // If it is, then there's no updating that needs to happen.
                    if (!Rectangle.Intersect(lastMapset.ScreenRectangle.ToRectangle(), ScreenRectangle.ToRectangle()).IsEmpty)
                        return;

                    lastMapset.Y = (PoolStartingIndex - 1) * DrawableMapset.HEIGHT + (PoolStartingIndex - 1) * YSpacing + YSpaceBeforeFirstSet;

                    lock (Screen.AvailableMapsets)
                        lastMapset.UpdateWithNewMapset(Screen.AvailableMapsets[PoolStartingIndex - 1], PoolStartingIndex - 1);

                    MapsetBuffer.Remove(lastMapset);
                    MapsetBuffer.Insert(0, lastMapset);

                    // Make sure the set is correctly selected/deselected.
                    if (PoolStartingIndex - 1 == SelectedMapsetIndex)
                        lastMapset.DisplayAsSelected(MapManager.Selected.Value);
                    else
                        lastMapset.DisplayAsDeselected();

                    PoolStartingIndex--;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        /// <summary>
        ///     If necessary, it will load and play the selected map's audio.
        /// </summary>
        /// <param name="previousMap"></param>
        private static void LoadNewAudioTrackIfNecessary(Map previousMap = null)
        {
            if (previousMap != null && MapManager.GetAudioPath(previousMap) == MapManager.GetAudioPath(MapManager.Selected.Value))
                return;

            lock (AudioEngine.Track)
            {
                if (AudioEngine.Track != null && AudioEngine.Track.IsPlaying)
                    AudioEngine.Track.Fade(0, 200);

                Scheduler.RunThread(() =>
                {
                    try
                    {
                        AudioEngine.LoadCurrentTrack();

                        if (AudioEngine.Track == null)
                            return;

                        AudioEngine.Track.Seek(MapManager.Selected.Value.AudioPreviewTime);
                        AudioEngine.Track.Volume = 0;
                        AudioEngine.Track.Play();
                        AudioEngine.Track.Fade(ConfigManager.VolumeMusic.Value, 800);
                    }
                    catch (Exception)
                    {
                        // ignored.
                    }
                });
            }
        }

        /// <summary>
        ///     Loads the new background and performs a fade in animation
        /// </summary>
        private static void LoadNewBackgroundIfNecessary(Map previousMap)
        {
            if (previousMap != null && MapManager.GetBackgroundPath(previousMap) == MapManager.GetBackgroundPath(MapManager.Selected.Value))
                return;

            // Start the fadeout on the background.
            lock (BackgroundHelper.Background.Animations)
            {
                BackgroundHelper.Background.Animations.Clear();

                BackgroundHelper.Background.Animations.Add(new Animation(AnimationProperty.Alpha,
                    Easing.OutQuint, BackgroundHelper.Background.Alpha, 0, 500));
            }

            // Begin to load the new background.
            BackgroundHelper.QueueLoad((tex, map, previousTex) =>
            {
                BackgroundHelper.HasBlurred = false;

                // Get rid of the old texture
                if (previousTex != UserInterface.MenuBackground)
                    previousTex.Dispose();

                // If the map is still the same, perform an animation.
                if (map == MapManager.Selected.Value)
                {
                    BackgroundHelper.Background.Image = tex;

                    // Fade in bg
                    lock (BackgroundHelper.Background.Animations)
                    {
                        BackgroundHelper.Background.Animations.Clear();

                        BackgroundHelper.Background.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.OutQuint,
                            BackgroundHelper.Background.Alpha, 1, 500));
                    }
                }
                // Otherwise skip it and dispose of the texture, as its not needed anymore.
                else
                {
                    if (tex != UserInterface.MenuBackground)
                        tex.Dispose();
                }
            });
        }

        /// <summary>
        ///     Snaps the scroll container to the initial mapset.
        /// </summary>
        private void SnapToInitialMapset()
        {
            ContentContainer.Y = (-SelectedMapsetIndex - 3) * DrawableMapset.HEIGHT + (-SelectedMapsetIndex - 3) * YSpacing + YSpaceBeforeFirstSet;

            PreviousContentContainerY = ContentContainer.Y;
            TargetY = PreviousContentContainerY;
            PreviousTargetY = PreviousContentContainerY;
            ContentContainer.Animations.Clear();
        }
    }
}