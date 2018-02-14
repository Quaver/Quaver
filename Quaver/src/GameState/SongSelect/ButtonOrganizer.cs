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

        private int SelectedSongIndex { get; set; }
        private int SelectedDiffIndex { get; set; }

        /// <summary>
        ///     Size of the button sorter. It is determined by how much buttons will be displayed on screen.
        /// </summary>
        private float OrganizerSize { get; set; }

        // Indexing
        private int SongButtonPoolSize { get; set; }
        private int BeatmapStartIndex { get; set; }
        private int SelectedBeatmapIndex { get; set; }

        private double DiffButtonsStart { get; set; }
        private double DiffButtonsEnd { get; set; }


        public void Initialize(IGameState state)
        {
            Boundary = new Boundary();
            GenerateButtonPool();
        }

        public void UnloadContent()
        {
            Boundary.Destroy();
        }

        public void Update(double dt)
        {
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
            int targetPoolSize = (int)(GameBase.WindowRectangle.Height / (MapsetSelectButton.BUTTON_Y_SIZE * GameBase.WindowUIScale)) + 10;
            Console.WriteLine("Button Pool Size: "+targetPoolSize);

            for (var i = 0; i < targetPoolSize && i < GameBase.Mapsets.Count; i++)
            {
                var newButton = new MapsetSelectButton(GameBase.WindowUIScale, i, GameBase.Mapsets[i])
                {
                    Image = GameBase.UI.BlankBox,
                    Alignment = Alignment.TopRight,
                    Position = new UDim2(-5, OrganizerSize + 50 ), // todo: +50 is temp, add buffer spacing later for boundary/songselectUI overlap
                    Parent = Boundary
                };

                var pos = i;
                OrganizerSize += newButton.SizeY;
                EventHandler newEvent = (sender, e) => OnSongSelectButtonClicked(sender, e, pos);
                newButton.Clicked += newEvent;
                SongSelectButtons.Add(newButton);
                SongSelectEvents.Add(newEvent);
                //todo: use index for song select button
            }
        }

        /// <summary>
        ///     Will shift buttons down by 1 seemlessly
        /// </summary>
        /// <param name="amount"></param>
        public void ShiftButtonPool(int amount)
        {

        }

        private void OnSongSelectButtonClicked(object sender, EventArgs e, int index)
        {
            // if index == SelectedSongIndex, remove diff select buttons + set selected song index to null
            SelectedSongIndex = index;
            SelectedDiffIndex = 0;
            
            // Delete Diff Select Buttons
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

            // Create Diff Select Buttons
            var mapset = GameBase.Mapsets[SelectedSongIndex];
            for (var i = 0; i < mapset.Beatmaps.Count; i++)
            {
                var newButton = new MapDifficultySelectButton(GameBase.WindowUIScale, i, mapset.Beatmaps[i])
                {
                    Image = GameBase.UI.BlankBox,
                    Alignment = Alignment.TopRight,
                    Position = new UDim2(-5, OrganizerSize + (GameBase.WindowUIScale * MapDifficultySelectButton.BUTTON_Y_SIZE * i) + 50), // todo: +50 is temp, add buffer spacing later for boundary/songselectUI overlap
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
        }

        private void OnDiffSelectButtonClicked(object sender, EventArgs e, int index)
        {
            // if index == SelectedDiffIndex, play map
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
