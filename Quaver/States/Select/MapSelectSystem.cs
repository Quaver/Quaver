﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quaver.Database.Maps;
using Quaver.GameState;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Graphics.UserInterface;
using Quaver.Logging;
using Quaver.Main;

namespace Quaver.States.Select
{
    /// <summary>
    ///     This class organizes the Song+Difficulty Selection Buttons
    /// </summary>
    internal class MapSelectSystem : IGameStateComponent
    {
        private MapInfoWindow MapInfoWindow = new MapInfoWindow();

        private const int INDEX_OFFSET_AMOUNT = 2;
        /// <summary>
        ///     Reference to the list of song select buttons
        /// </summary>
        private List<QuaverMapsetSelectButton> SongSelectButtons { get; set; } = new List<QuaverMapsetSelectButton>();

        private List<QuaverMapDifficultySelectButton> DiffSelectButtons { get; set; } = new List<QuaverMapDifficultySelectButton>();

        private List<EventHandler> SongSelectEvents { get; set; } = new List<EventHandler>();

        private List<EventHandler> DiffSelectEvents { get; set; } = new List<EventHandler>();

        /// <summary>
        ///     The Button Container
        /// </summary>
        private QuaverContainer Boundary { get; set; }

        /// <summary>
        ///     Selected beatmap's index
        /// </summary>
        private int SelectedSongIndex { get; set; }

        /// <summary>
        ///     Selected map's index
        /// </summary>
        private int SelectedDiffIndex { get; set; }

        /// <summary>
        ///     Current index used for object pooling
        /// </summary>
        private int CurrentPoolIndex { get; set; }

        /// <summary>
        ///     Total amount of buttons visible on screen
        /// </summary>
        private int MaxButtonsOnScreen { get; set; }

        /// <summary>
        ///     Value to move the container by to reach its target spot
        /// </summary>
        private float TargetContainerOffset { get; set; }

        /// <summary>
        ///     Keeps track if this state has already been loaded. (Used for audio loading.)
        /// </summary>
        private bool FirstLoad { get; set; }

        /// <summary>
        ///     Initialize
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(IGameState state)
        {
            //Console.WriteLine(GameBase.VisibleMapsets.Count + ", " + GameBase.Mapsets.Count);
            Boundary = new QuaverContainer();
            MapInfoWindow.Initialize(state);
            GenerateButtonPool();
        }

        public void UnloadContent()
        {
            MapInfoWindow.UnloadContent();
            DeleteMapDiffButtons();
            DeleteMapsetButtons();
            Boundary.Destroy();
        }

        //todo: temp
        private float prevScrollPos = 0;
        private float curScrollPos = 0;
        public void Update(double dt)
        {
            var tween = Math.Min(dt / 50, 1);

            curScrollPos = GameBase.MouseState.ScrollWheelValue;
            TargetContainerOffset += (float)((curScrollPos - prevScrollPos - TargetContainerOffset) * tween);

            MoveButtonContainer(TargetContainerOffset);
            //TargetContainerOffset = 0;

            prevScrollPos = curScrollPos;

            MapInfoWindow.Update(dt);
            Boundary.Update(dt);
        }

        public void Draw()
        {
            Boundary.Draw();
            MapInfoWindow.Draw();
        }

        /// <summary>
        ///     Generates a button pool depending on your screen size
        /// </summary>
        public void GenerateButtonPool()
        {
            MaxButtonsOnScreen = (int)Math.Ceiling(GameBase.WindowRectangle.Height / (QuaverMapsetSelectButton.BUTTON_OFFSET_PADDING * GameBase.WindowUIScale)) + INDEX_OFFSET_AMOUNT;
            int targetPoolSize = MaxButtonsOnScreen * 2;

            for (var i = 0; i < targetPoolSize && i < GameBase.VisibleMapsets.Count; i++)
            {
                var newButton = new QuaverMapsetSelectButton(GameBase.WindowUIScale, i, GameBase.Mapsets[i])
                {
                    Image = GameBase.QuaverUserInterface.BlankBox,
                    Alignment = Alignment.TopRight,
                    Position = new UDim2D(-5, 0),
                    Parent = Boundary
                };

                var pos = i;
                EventHandler newEvent = (sender, e) => OnSongSelectButtonClicked(sender, e, pos);
                newButton.Clicked += newEvent;
                SongSelectButtons.Add(newButton);
                SongSelectEvents.Add(newEvent);
            }

            ShiftButtonPool(0);
        }

        /// <summary>
        ///     Will shift buttons down by 1 seemlessly
        /// </summary>
        /// <param name="amount"></param>
        public void ShiftButtonPool(int amount)
        {
            CurrentPoolIndex += amount;
            if (CurrentPoolIndex < 0) CurrentPoolIndex = 0;
            if (CurrentPoolIndex >= GameBase.VisibleMapsets.Count) CurrentPoolIndex = GameBase.VisibleMapsets.Count - 1;
            //Console.WriteLine(amount + ", " + CurrentPoolIndex);

            int index;
            for (var i=0; i<SongSelectButtons.Count; i++)
            {
                index = i + CurrentPoolIndex - MaxButtonsOnScreen;
                if (index >= 0 && index < GameBase.VisibleMapsets.Count)
                {
                    SongSelectButtons[i].Visible = true;
                    SongSelectButtons[i].UpdateButtonMapIndex(index, GameBase.VisibleMapsets[index]);
                }
                else
                {
                    SongSelectButtons[i].Visible = false;
                }
            }
            UpdateMapsetButtonOffsets();
        }

        /// <summary>
        ///     Move every button on the screen by moving the boundary
        /// </summary>
        /// <param name="amount"></param>
        public void MoveButtonContainer(float amount)
        {
            if (Math.Abs(amount) < 0.5) return;

            Boundary.PosY += amount;

            if (CurrentPoolIndex == 0 && Boundary.PosY > 0)
            {
                //todo: set min position
                return;
            }

            if (CurrentPoolIndex == GameBase.VisibleMapsets.Count - 1 && Boundary.PosY < 0)
            {
                //todo: set max position
                return;
            }

            if (Math.Abs(Boundary.PosY) > QuaverMapsetSelectButton.BUTTON_OFFSET_PADDING * GameBase.WindowUIScale)
            {
                int shiftAmt = (int)(Math.Floor(Boundary.PosY / (QuaverMapsetSelectButton.BUTTON_OFFSET_PADDING * GameBase.WindowUIScale)));
                Boundary.PosY -= shiftAmt * (QuaverMapsetSelectButton.BUTTON_OFFSET_PADDING * GameBase.WindowUIScale);
                ShiftButtonPool(-shiftAmt);
            }
        }

        /// <summary>
        ///     Moves the Mapset Buttons to their respective positions
        /// </summary>
        private void UpdateMapsetButtonOffsets()
        {
            int index = 0;
            for (var i = 0; i < SongSelectButtons.Count; i++)
            {
                index = i + CurrentPoolIndex - MaxButtonsOnScreen;
                if (index >= 0 && index < GameBase.VisibleMapsets.Count)
                {
                    SongSelectButtons[i].PosY = index - SelectedSongIndex <= 0
                        ? (i - MaxButtonsOnScreen) * GameBase.WindowUIScale * QuaverMapsetSelectButton.BUTTON_OFFSET_PADDING
                        : GameBase.WindowUIScale * (((i - MaxButtonsOnScreen) * QuaverMapsetSelectButton.BUTTON_OFFSET_PADDING) + (DiffSelectButtons.Count * QuaverMapDifficultySelectButton.BUTTON_OFFSET_PADDING));
                }
            }

            var posOffset = ((SelectedSongIndex - CurrentPoolIndex + 1) * GameBase.WindowUIScale * QuaverMapsetSelectButton.BUTTON_OFFSET_PADDING);
            for (var i = 0; i < DiffSelectButtons.Count; i++)
            {
                DiffSelectButtons[i].PosY = posOffset + (GameBase.WindowUIScale * QuaverMapDifficultySelectButton.BUTTON_OFFSET_PADDING * i);
            }
        }

        /// <summary>
        ///     Select the next mapset in according to ammount
        /// </summary>
        /// <param name="amount"></param>
        private void SelectNextMapset(int amount)
        {
            SelectedSongIndex += amount;

            while (SelectedSongIndex > GameBase.VisibleMapsets.Count)
            {
                SelectedSongIndex -= GameBase.VisibleMapsets.Count;
            }

            SelectMapset(SelectedSongIndex);
        }

        /// <summary>
        ///     Select the next difficulty in according to ammount
        /// </summary>
        /// <param name="amount"></param>
        private void SelectNextDifficulty(int amount)
        {
            SelectedDiffIndex += amount;

            while (SelectedDiffIndex > DiffSelectButtons.Count)
            {
                SelectedDiffIndex -= DiffSelectButtons.Count;
            }

            SelectDifficulty(SelectedDiffIndex);
        }

        /// <summary>
        ///     Get the offset amount needed in order to focus on the current map
        /// </summary>
        private float GetFocusOffsetOfCurrentMap()
        {
            // todo:temp
            float total = ((SelectedSongIndex - CurrentPoolIndex) * GameBase.WindowUIScale * QuaverMapsetSelectButton.BUTTON_OFFSET_PADDING) - DiffSelectButtons[0].PosY; //+ (GameBase.WindowRectangle.Height/2);
            Console.WriteLine(total);
            return 0; //total;

            // If difficulty is not selected, focus on mapset button

            // If difficulty is selected, focus on difficulty button
        }

        /// <summary>
        ///     Delete and unhook current mapset buttons event listeners
        /// </summary>
        private void DeleteMapsetButtons()
        {
            if (SongSelectButtons.Count > 0)
            {
                for (var i = 0; i < SongSelectButtons.Count; i++)
                {
                    SongSelectButtons[i].Clicked -= SongSelectEvents[i];
                    SongSelectButtons[i].Destroy();
                }
                SongSelectEvents.Clear();
                SongSelectButtons.Clear();
            }
        }

        /// <summary>
        ///     Delete and unhook current difficulty buttons listeners
        /// </summary>
        private void DeleteMapDiffButtons()
        {
            if (DiffSelectButtons.Count > 0)
            {
                for (var i = 0; i < DiffSelectButtons.Count; i++)
                {
                    DiffSelectButtons[i].Clicked -= DiffSelectEvents[i];
                    DiffSelectButtons[i].Destroy();
                }
                DiffSelectEvents.Clear();
                DiffSelectButtons.Clear();
            }
        }

        /// <summary>
        ///     Selects a mapset by index
        /// </summary>
        /// <param name="index"></param>
        private void SelectMapset(int index)
        {
            SelectedSongIndex = index;
            SelectedDiffIndex = 0;

            // Delete Diff Select Buttons
            DeleteMapDiffButtons();

            // Create Diff Select Buttons
            var mapset = GameBase.Mapsets[SelectedSongIndex];
            var posOffset = ((SelectedSongIndex + 1) * GameBase.WindowUIScale * QuaverMapsetSelectButton.BUTTON_OFFSET_PADDING);
            for (var i = 0; i < mapset.Maps.Count; i++)
            {
                var newButton = new QuaverMapDifficultySelectButton(GameBase.WindowUIScale, i, mapset.Maps[i])
                {
                    Image = GameBase.QuaverUserInterface.BlankBox,
                    Alignment = Alignment.TopRight,
                    Position = new UDim2D(-5, posOffset + (GameBase.WindowUIScale * QuaverMapDifficultySelectButton.BUTTON_OFFSET_PADDING * i)),
                    Parent = Boundary
                };

                var pos = i;
                EventHandler newEvent = (newSender, newE) => OnSongSelectButtonClicked(newSender, newE, pos);
                newButton.Clicked += (newSender, newE) => OnDiffSelectButtonClicked(newSender, newE, pos);
                DiffSelectButtons.Add(newButton);
                DiffSelectEvents.Add(newEvent);
            }

            // Update Button Offsets
            UpdateMapsetButtonOffsets();

            // Focus on button
            TargetContainerOffset = GetFocusOffsetOfCurrentMap();
        }

        /// <summary>
        ///     Selects a map difficulty by index
        /// </summary>
        /// <param name="index"></param>
        private void SelectDifficulty(int index)
        {
            // Update selected difficulty index
            SelectedDiffIndex = index;

            // Focus on button
            TargetContainerOffset = GetFocusOffsetOfCurrentMap();

            // Select map
            var map = GameBase.Mapsets[SelectedSongIndex].Maps[SelectedDiffIndex];

            var oldMapAudioPath = GameBase.SelectedMap.Directory + "/" + GameBase.SelectedMap.AudioPath;
            Map.ChangeSelected(map);
            MapInfoWindow.UpdateInfo(map);
            //Console.WriteLine(GameBase.CurrentAudioPath);

            // Only load the audio again if the new map's audio isn't the same as the old ones.
            if (oldMapAudioPath != map.Directory + "/" + map.AudioPath || !FirstLoad)
            {
                try
                {
                    GameBase.AudioEngine.ReloadStream();
                    GameBase.AudioEngine.Play(GameBase.SelectedMap.AudioPreviewTime);
                    FirstLoad = true;
                }
                catch (Exception e)
                {
                    Logger.LogWarning("User selected a map with audio that could not be loaded", LogType.Runtime);
                }
            }

            // Load background asynchronously if the backgrounds actually do differ
            if (GameBase.LastBackgroundPath != map.Directory + "/" + map.BackgroundPath)
            {
                Task.Run(() =>
                {
                    BackgroundManager.LoadBackground();
                }).ContinueWith(t =>
                {
                    // After loading, change the background
                    BackgroundManager.Change(GameBase.CurrentBackground);
                });
            }

            // Load all the local scores from this map 
            // TODO: Add filters, this should come after there's some sort of UI to do so
            // TODO #2: Actually display these scores on-screen somewhere. Add loading animation before running task.
            // TODO #3: Move this somewhere so that it automatically loads the scores upon first load as well.
            //Task.Run(async () => await LocalScoreCache.SelectBeatmapScores(GameBase.SelectedBeatmap.Md5Checksum))
            //    .ContinueWith(t => Logger.Log($"Successfully loaded {t.Result.Count} local scores for this map.", LogColors.GameInfo,0.2f));

            //TODO: make it so scrolling is disabled until background has been loaded
            //ScrollingDisabled = false;
        }

        /// <summary>
        ///     Triggered whenever a mapset mapset button gets pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="index"></param>
        private void OnSongSelectButtonClicked(object sender, EventArgs e, int index)
        {
            if (SongSelectButtons[index].Visible) //todo: implement click visibility check in button
            {
                //SongSelectButtons[SelectedSongIndex].Selected = false;
                //SongSelectButtons[index].Selected = true;
                SelectMapset(index + CurrentPoolIndex - MaxButtonsOnScreen);
            }
        }

        /// <summary>
        ///     Triggered whenever a difficulty button gets pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="index"></param>
        private void OnDiffSelectButtonClicked(object sender, EventArgs e, int index)
        {
            DiffSelectButtons[SelectedDiffIndex].Selected = false;
            DiffSelectButtons[index].Selected = true;
            SelectDifficulty(index);
        }
    }
}
