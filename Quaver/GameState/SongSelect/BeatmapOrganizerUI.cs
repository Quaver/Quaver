using Quaver.Graphics;
using Quaver.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Database.Beatmaps;
using Quaver.Audio;
using Quaver.Database;
using Quaver.Database.Scores;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UserInterface;
using Quaver.Helpers;

namespace Quaver.GameState.SongSelect
{
    internal class BeatmapOrganizerUI : IHelper
    {
        /// <summary>
        ///     Reference to the list of song select buttons
        /// </summary>
        private List<QuaverSongSelectButton> SongSelectButtons { get; set; } = new List<QuaverSongSelectButton>();

        private List<EventHandler> SongSelectEvents { get; set; } = new List<EventHandler>();

        private Boundary Boundary { get; set; }

        public bool ScrollingDisabled { get; set; }

        public object TogglePitch { get; private set; }

        private float OrganizerSize { get; set; }

        private float TargetPosition { get; set; }

        private int SelectedMapIndex { get; set; } = 0;

        private float SelectedMapTween { get; set; } = 0;

        /// <summary>
        ///     Keeps track if this state has already been loaded. (Used for audio loading.)
        /// </summary>
        private bool FirstLoad { get; set; }

        public void Draw()
        {
            Boundary.Draw();
        }

        public void Initialize(IGameState state)
        {
            Boundary = new Boundary();
            CreateSongSelectButtons();

            if (GameBase.SelectedBeatmap == null)
                BeatmapHelper.SelectRandomBeatmap();

            // Select the currently selected map.
            SelectMap(SongSelectButtons.FindIndex(x => x.Map == GameBase.SelectedBeatmap));
        }

        public void UnloadContent()
        {
            //Logger.Log("UNLOADED", LogColors.GameError);
            for (var i=0; i<SongSelectButtons.Count; i++)
                SongSelectButtons[i].Clicked -= SongSelectEvents[i];
            SongSelectButtons.Clear();
            SongSelectEvents.Clear();
            Boundary.Destroy();
        }

        public void Update(double dt)
        {
            var tween = Math.Min(dt / 70, 1);

            // Update Position of Boundary
            var posDifference = GraphicsHelper.Tween(TargetPosition, Boundary.PosY, tween) - Boundary.PosY;

            if (Math.Abs(posDifference) > 0.5f)
                Boundary.PosY += posDifference;

            Boundary.Update(dt);
        }

        /// <summary>
        ///     Creates the song select buttons
        /// </summary>
        public void CreateSongSelectButtons()
        {
            OrganizerSize = 50f;
            //Create buttons for every beatmap set TODO: Use beatmap set instead of beatmaps
            foreach (var mapset in GameBase.Mapsets)
            {
                //Create Song Buttons
                foreach (var map in mapset.Beatmaps)
                {
                    var index = SongSelectButtons.Count;

                    // Create the new button
                    var newButton = new QuaverSongSelectButton(map, GameBase.WindowUIScale)
                    {
                        Map = map,
                        Image = GameBase.UI.BlankBox,
                        Alignment = Alignment.TopRight,
                        Position = new UDim2(-5, OrganizerSize),
                        Parent = Boundary
                    };

                    // Define event handler for the button
                    EventHandler newEvent = (sender, e) => OnSongSelectButtonClick(sender, e, index);
                    newButton.Clicked += newEvent;

                    // Add the4 button the current list
                    SongSelectButtons.Add(newButton);
                    SongSelectEvents.Add(newEvent);

                    // Change the Y value
                    OrganizerSize += newButton.SizeY + 2;
                }
            }
        }

        /// <summary>
        ///     Changes the map when a song select button is clicked.
        /// </summary>
        private void OnSongSelectButtonClick(object sender, EventArgs e, int index)
        {
            SelectMap(index);
        }

        private void SelectMap(int index)
        {
            ScrollingDisabled = true;
            var map = SongSelectButtons[index].Map;
            Logger.Update("MapSelected", "Map Selected: " + map.Artist + " - " + map.Title + " [" + map.DifficultyName + "]");

            SongSelectButtons[SelectedMapIndex].Selected = false;
            SongSelectButtons[index].Selected = true;
            SelectedMapIndex = index;
            TargetPosition = (GameBase.WindowRectangle.Height / 2f) - ((float)index / SongSelectButtons.Count) * OrganizerSize;

            var oldMapAudioPath = GameBase.SelectedBeatmap.Directory + "/" + GameBase.SelectedBeatmap.AudioPath;
            Beatmap.ChangeBeatmap(map);

            // Only load the audio again if the new map's audio isn't the same as the old ones.
            if (oldMapAudioPath != map.Directory + "/" + map.AudioPath || !FirstLoad)
            {
                try
                {
                    GameBase.AudioEngine.ReloadStream();
                    GameBase.AudioEngine.Play(GameBase.SelectedBeatmap.AudioPreviewTime);
                    FirstLoad = true;
                } catch (Exception e)
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
            Task.Run(async () => await LocalScoreCache.SelectBeatmapScores(GameBase.SelectedBeatmap.Md5Checksum))
                .ContinueWith(t => Logger.LogSuccess($"Successfully loaded {t.Result.Count} local scores for this map.", LogType.Runtime));

            //TODO: make it so scrolling is disabled until background has been loaded
            ScrollingDisabled = false;
        }

        public void SetBeatmapOrganizerPosition(float scale)
        {
            TargetPosition = scale * OrganizerSize;
        }

        public void OffsetBeatmapOrganizerPosition(float offset)
        {
            TargetPosition += offset * 2;
        }

        public void OffsetBeatmapOrganizerIndex(int offset)
        {
            var newIndex = SelectedMapIndex + offset;
            if (newIndex >= 0 && newIndex < SongSelectButtons.Count)
            {
                SelectMap(newIndex);
            }
        }
    }
}
