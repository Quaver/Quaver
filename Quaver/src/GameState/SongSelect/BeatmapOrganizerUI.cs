using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Beatmaps;
using Quaver.Graphics.Sprite;
using Quaver.Audio;

namespace Quaver.GameState.SongSelect
{
    class BeatmapOrganizerUI : IHelper
    {

        /// <summary>
        ///     Reference to the list of song select buttons
        /// </summary>
        private List<Button> SongSelectButtons { get; set; } = new List<Button>();

        private Boundary Boundary { get; set; }

        public object TogglePitch { get; private set; }

        private float OrganizerSize { get; set; }

        public void Draw()
        {
            Boundary.Draw();
        }

        public void Initialize(IGameState state)
        {

            Boundary = new Boundary();

            CreateSongSelectButtons();
        }

        public void UnloadContent()
        {
            SongSelectButtons.Clear();
        }

        public void Update(double dt)
        {
            Boundary.Update(dt);
        }

        /// <summary>
        ///     Creates the song select buttons
        /// </summary>
        public void CreateSongSelectButtons()
        {
            OrganizerSize = 50f;
            //Create buttons for every beatmap set TODO: Use beatmap set instead of beatmaps
            foreach (var mapset in GameBase.VisibleBeatmaps)
            {
                //Create Song Buttons
                foreach (var map in mapset.Beatmaps)
                {
                    var mapText = map.Artist + " - " + map.Title + " [" + map.DifficultyName + "]";

                    // Create the new button
                    var newButton = new SongSelectButton(map, 1)
                    {
                        Image = GameBase.UI.BlankBox,
                        Alignment = Alignment.TopLeft,
                        PositionY = OrganizerSize,
                        PositionX = 5,
                        Parent = Boundary
                    };

                    // Define event handler for the button
                    newButton.Clicked += (sender, e) => OnSongSelectButtonClick(sender, e, mapText, map);

                    // Add the4 button the current list
                    SongSelectButtons.Add(newButton);

                    // Change the Y value
                    OrganizerSize += newButton.SizeY + 2;
                }
            }
        }

        /// <summary>
        ///     Changes the map when a song select button is clicked.
        /// </summary>
        private void OnSongSelectButtonClick(object sender, EventArgs e, string text, Beatmap map)
        {
            Logger.Update("MapSelected", "Map Selected: " + text);

            // Get the background path from the previous map
            var oldMapBgPath = GameBase.SelectedBeatmap.Directory + "/" + GameBase.SelectedBeatmap.BackgroundPath;
            var oldMapAudioPath = GameBase.SelectedBeatmap.Directory + "/" + GameBase.SelectedBeatmap.AudioPath;
            GameBase.ChangeBeatmap(map);

            // Only load the audio again if the new map's audio isn't the same as the old ones.
            if (oldMapAudioPath != map.Directory + "/" + map.AudioPath)
                SongManager.ReloadSong(true);

            // Load background asynchronously if the backgrounds actually do differ
            if (oldMapBgPath != map.Directory + "/" + map.BackgroundPath)
                Task.Run(() => GameBase.LoadBackground())
                    .ContinueWith(t => BackgroundManager.Change(GameBase.CurrentBackground));
                    
        }

        public void SetBeatmapOrganizerPosition(float scale)
        {
            Boundary.PositionY = scale * -OrganizerSize;
        }
    }
}
