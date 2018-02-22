using Quaver.Audio;
using Quaver.Database.Beatmaps;
using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;
using Quaver.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState.SongSelect
{
    /// <summary>
    ///     This class organizes the Song+Difficulty Selection Buttons
    /// </summary>
    class ButtonOrganizer : IHelper
    {
        /// <summary>
        ///     Reference to the list of song select buttons
        /// </summary>
        private List<MapsetSelectButton> SongSelectButtons { get; set; } = new List<MapsetSelectButton>();

        private List<MapDifficultySelectButton> DiffSelectButtons { get; set; } = new List<MapDifficultySelectButton>();

        private List<EventHandler> SongSelectEvents { get; set; } = new List<EventHandler>();

        private List<EventHandler> DiffSelectEvents { get; set; } = new List<EventHandler>();

        private Boundary Boundary { get; set; }

        private int MaxButtonsOnScreen { get; set; }

        // Indexing
        private int SelectedSongIndex { get; set; }
        private int SelectedDiffIndex { get; set; }
        private int CurrentPoolIndex { get; set; }

        private const int INDEX_OFFSET_AMOUNT = 2;


        public void Initialize(IGameState state)
        {
            Console.WriteLine(GameBase.VisibleMapsets.Count + ", " + GameBase.Mapsets.Count);
            Boundary = new Boundary();
            GenerateButtonPool();
        }

        public void UnloadContent()
        {
            DeleteMapDiffButtons();
            DeleteMapsetButtons();
            Boundary.Destroy();
        }

        //todo: temp
        private int prevScrollPos = 0;
        private int curScrollPos = 0;
        public void Update(double dt)
        {
            curScrollPos = GameBase.MouseState.ScrollWheelValue;

            if (Math.Abs(curScrollPos - prevScrollPos) > 0)
            {
                MoveButtonPool(curScrollPos - prevScrollPos);
            }
            prevScrollPos = curScrollPos;

            Boundary.Update(dt);
        }

        public void Draw()
        {
            Boundary.Draw();
        }

        /// <summary>
        ///     Generates a button pool depending on your screen size
        /// </summary>
        public void GenerateButtonPool()
        {
            MaxButtonsOnScreen = (int)(GameBase.WindowRectangle.Height / (MapsetSelectButton.BUTTON_OFFSET_PADDING * GameBase.WindowUIScale)) + INDEX_OFFSET_AMOUNT;
            int targetPoolSize = MaxButtonsOnScreen * 2;
            Console.WriteLine("Button Pool Size: "+targetPoolSize);

            for (var i = 0; i < targetPoolSize && i < GameBase.VisibleMapsets.Count; i++)
            {
                var newButton = new MapsetSelectButton(GameBase.WindowUIScale, i, GameBase.Mapsets[i])
                {
                    Image = GameBase.UI.BlankBox,
                    Alignment = Alignment.TopRight,
                    Position = new UDim2(-5, 0),
                    Parent = Boundary
                };

                var pos = i;
                EventHandler newEvent = (sender, e) => OnSongSelectButtonClicked(sender, e, pos);
                newButton.Clicked += newEvent;
                SongSelectButtons.Add(newButton);
                SongSelectEvents.Add(newEvent);
                //todo: use index for song select button

                UpdateMapsetButtonOffsets();
            }
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
                index = i + CurrentPoolIndex - INDEX_OFFSET_AMOUNT;
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

        public void MoveButtonPool(int amount)
        {
            Boundary.PosY += amount;

            if (Math.Abs(Boundary.PosY) > MapsetSelectButton.BUTTON_OFFSET_PADDING * GameBase.WindowUIScale)
            {
                int shiftAmt = (int)(Math.Floor(Boundary.PosY / (MapsetSelectButton.BUTTON_OFFSET_PADDING * GameBase.WindowUIScale)));
                Boundary.PosY -= shiftAmt * (MapsetSelectButton.BUTTON_OFFSET_PADDING * GameBase.WindowUIScale);
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
                        ? (i - MaxButtonsOnScreen) * GameBase.WindowUIScale * MapsetSelectButton.BUTTON_OFFSET_PADDING
                        : GameBase.WindowUIScale * (((i - MaxButtonsOnScreen) * MapsetSelectButton.BUTTON_OFFSET_PADDING) + (DiffSelectButtons.Count * MapDifficultySelectButton.BUTTON_OFFSET_PADDING));
                }
            }

            var posOffset = ((SelectedSongIndex - CurrentPoolIndex + 1) * GameBase.WindowUIScale * MapsetSelectButton.BUTTON_OFFSET_PADDING);
            for (var i = 0; i < DiffSelectButtons.Count; i++)
            {
                DiffSelectButtons[i].PosY = posOffset + (GameBase.WindowUIScale * MapDifficultySelectButton.BUTTON_OFFSET_PADDING * i);
            }
        }

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

        private void OnSongSelectButtonClicked(object sender, EventArgs e, int index)
        {
            if (SongSelectButtons[index].Visible) //todo: implement click visibility check in button
            {
                //SongSelectButtons[SelectedSongIndex].Selected = false;
                //SongSelectButtons[index].Selected = true;

                SelectedSongIndex = index + CurrentPoolIndex - MaxButtonsOnScreen;
                SelectedDiffIndex = 0;

                // Delete Diff Select Buttons
                DeleteMapDiffButtons();

                // Create Diff Select Buttons
                var mapset = GameBase.Mapsets[SelectedSongIndex];
                var posOffset = ((SelectedSongIndex + 1) * GameBase.WindowUIScale * MapsetSelectButton.BUTTON_OFFSET_PADDING);
                for (var i = 0; i < mapset.Beatmaps.Count; i++)
                {
                    var newButton = new MapDifficultySelectButton(GameBase.WindowUIScale, i, mapset.Beatmaps[i])
                    {
                        Image = GameBase.UI.BlankBox,
                        Alignment = Alignment.TopRight,
                        Position = new UDim2(-5 - 400, posOffset + (GameBase.WindowUIScale * MapDifficultySelectButton.BUTTON_OFFSET_PADDING * i)),
                        Parent = Boundary
                    };

                    var pos = i;
                    //OrganizerSize += newButton.SizeY;
                    EventHandler newEvent = (newSender, newE) => OnSongSelectButtonClicked(newSender, newE, pos);
                    newButton.Clicked += (newSender, newE) => OnDiffSelectButtonClicked(newSender, newE, pos);
                    DiffSelectButtons.Add(newButton);
                    DiffSelectEvents.Add(newEvent);
                    //todo: use index for song select button
                }

                // Update Button Offsets
                UpdateMapsetButtonOffsets();
                //MoveButtonPool(10);
            }
        }

        private void OnDiffSelectButtonClicked(object sender, EventArgs e, int index)
        {
            DiffSelectButtons[SelectedDiffIndex].Selected = false;
            DiffSelectButtons[index].Selected = true;
            SelectedDiffIndex = index;

            // Select map
            var map = GameBase.Mapsets[SelectedSongIndex].Beatmaps[SelectedDiffIndex];
            Logger.Update("MapSelected", "Map Selected: " + map.Artist + " - " + map.Title + " [" + map.DifficultyName + "]");

            //SongSelectButtons[SelectedMapIndex].Selected = false;
            //SongSelectButtons[index].Selected = true;
            //SelectedMapIndex = index;
            //TargetPosition = (GameBase.WindowRectangle.Height / 2f) - ((float)index / SongSelectButtons.Count) * OrganizerSize;

            var oldMapAudioPath = GameBase.SelectedBeatmap.Directory + "/" + GameBase.SelectedBeatmap.AudioPath;
            Beatmap.ChangeBeatmap(map);

            // Only load the audio again if the new map's audio isn't the same as the old ones.
            if (oldMapAudioPath != map.Directory + "/" + map.AudioPath)
                SongManager.ReloadSong(true);

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
    }
}
