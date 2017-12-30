using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Database.Beatmaps;
using Quaver.Graphics.Sprite;
using Quaver.Audio;
using Quaver.Database;
using Quaver.Utility;

namespace Quaver.GameState.SongSelect
{
    class BeatmapOrganizerUI : IHelper
    {

        /// <summary>
        ///     Reference to the list of song select buttons
        /// </summary>
        private List<SongSelectButton> SongSelectButtons { get; set; } = new List<SongSelectButton>();

        private Boundary Boundary { get; set; }

        public object TogglePitch { get; private set; }

        private float OrganizerSize { get; set; }

        private float TargetPosition { get; set; }

        private int SelectedMapIndex { get; set; } = 0;

        private float SelectedMapTween { get; set; } = 0;

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
            var tween = Math.Min(dt / 70, 1);
            Boundary.PositionY = Util.Tween(TargetPosition, Boundary.PositionY, tween);
            SelectedMapTween = Util.Tween(SelectedMapIndex, SelectedMapTween, tween);
            //GameBase.Window.Y-(GameBase.Window.Z/2f)
            for (var i=0; i<SongSelectButtons.Count; i++)
            {
                var button = SongSelectButtons[i];
                var selectedOffset = Math.Abs(SelectedMapTween - i)+1;

                /*button.PositionX = 
                    (float)Math.Sin(Math.PI * (Boundary.PositionY + button.PositionY)/ GameBase.Window.Z) * 30 
                    + 50/ selectedOffset
                    - 30;*/
                button.PositionX = -(30 / selectedOffset) - 5;

            }
            Boundary.Update(dt);
        }

        /// <summary>
        ///     Creates the song select buttons
        /// </summary>
        public void CreateSongSelectButtons()
        {
            OrganizerSize = 50f;
            //Create buttons for every beatmap set TODO: Use beatmap set instead of beatmaps
            foreach (var mapset in GameBase.VisibleMapsets)
            {
                //Create Song Buttons
                foreach (var map in mapset.Beatmaps)
                {
                    var mapText = map.Artist + " - " + map.Title + " [" + map.DifficultyName + "]";
                    var index = SongSelectButtons.Count;

                    // Create the new button
                    var newButton = new SongSelectButton(map, GameBase.WindowYRatio)
                    {
                        Image = GameBase.UI.BlankBox,
                        Alignment = Alignment.TopRight,
                        PositionY = OrganizerSize,
                        PositionX = -5,
                        Parent = Boundary
                    };

                    // Define event handler for the button
                    newButton.Clicked += (sender, e) => OnSongSelectButtonClick(sender, e, mapText, map, index);

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
        private void OnSongSelectButtonClick(object sender, EventArgs e, string text, Beatmap map, int index)
        {
            Logger.Update("MapSelected", "Map Selected: " + text);
            SongSelectButtons[SelectedMapIndex].Selected = false;
            SongSelectButtons[index].Selected = true;
            SelectedMapIndex = index;
            TargetPosition = GameBase.Window.Y + (GameBase.Window.Z / 2) - ((float)index / SongSelectButtons.Count) * OrganizerSize;

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
            
            // Load all the local scores from this map 
            // TODO: Add filters, this should come after there's some sort of UI to do so
            // TODO #2: Actually display these scores on-screen somewhere. Add loading animation before running task.
            // TODO #3: Move this somewhere so that it automatically loads the scores upon first load as well.
            Task.Run(async () => await LocalScoreCache.SelectBeatmapScores(GameBase.SelectedBeatmap.Md5Checksum))
                .ContinueWith(t => Logger.Log($"Successfully loaded {t.Result.Count} local scores for this map.", LogColors.GameImportant));
        }

        public void SetBeatmapOrganizerPosition(float scale)
        {
            TargetPosition = scale * OrganizerSize;
        }
    }
}
