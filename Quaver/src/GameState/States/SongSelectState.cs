using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Audio;
using Quaver.Beatmaps;
using Quaver.Discord;
using Quaver.GameState.States;
using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;
using Quaver.Logging;
using Quaver.Main;

namespace Quaver.GameState.States
{
    internal class SongSelectState : IGameState
    {
        public State CurrentState { get; set; } = State.MainMenu;
        public bool UpdateReady { get; set; }

        //TODO: update later   TEST.
        private List<Button> Buttons { get; set; } = new List<Button>();
        private Button PlayButton { get; set; }
        private Boundary Boundary { get; set; } = new Boundary();
        private int ButtonPos { get; set; } = 50;

        public void Initialize()
        {
            // Update Discord Presence
            GameBase.ChangeDiscordPresence("In song select");

            //Create buttons for every beatmap set TODO: Use beatmap set instead of beatmaps
            foreach (KeyValuePair<string, List<Beatmap>> mapset in GameBase.VisibleBeatmaps)
            {
                //Create Song Buttons
                foreach (var map in mapset.Value)
                {
                    var newButton = new TextButton(new Vector2(300, 20),
                        map.Artist + " - " + map.Title + " [" + map.DifficultyName + "]")
                    {
                        Image = GameBase.UI.BlankBox,
                        Alignment = Alignment.TopLeft,
                        PositionY = ButtonPos,
                        Parent = Boundary
                    };
                    newButton.TextSprite.TextAlignment = Alignment.MidLeft;

                    var currentMap = map;
                    newButton.Clicked += (sender, e) => ButtonClick(sender, e, map.Artist + " - " + map.Title + " [" + map.DifficultyName + "]", currentMap);
                    Buttons.Add(newButton);
                    ButtonPos += 20;
                }
            }

            // Create play button
            PlayButton = new TextButton(new Vector2(200, 30), "Play")
            {
                Image = GameBase.UI.BlankBox,
                Alignment = Alignment.TopRight,
                Parent = Boundary
            };
            PlayButton.Clicked += PlayMap;

            //Add map selected text TODO: remove later
            LogManager.AddLogger("MapSelected",Color.Yellow);
            LogManager.UpdateLogger("MapSelected", "Map Selected: "+GameBase.SelectedBeatmap.Artist + " - " + GameBase.SelectedBeatmap.Title + " [" + GameBase.SelectedBeatmap.DifficultyName + "]");
            UpdateReady = true;
        }

        public void UnloadContent()
        {
            UpdateReady = false;
            PlayButton.Clicked -= PlayMap;

            //TODO: Remove button delegates ?
            foreach (TextButton button in Buttons)
            {
                //button.Clicked -= Delegate;
            }

            Boundary.Destroy();
        }

        public void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
            Boundary.Update(dt);

            // Repeat the song preview if necessary
            RepeatSongPreview();;
        }

        public void Draw()
        {
            Boundary.Draw();
        }

        //TODO: Remove
        public void ButtonClick(object sender, EventArgs e, string text, Beatmap map)
        {
            LogManager.UpdateLogger("MapSelected","Map Selected: "+text);

            //Select map
            GameBase.ChangeBeatmap(map);
            GameBase.SelectedBeatmap.Song.Play(GameBase.SelectedBeatmap.AudioPreviewTime);

            // Load background asynchronously.
            Task.Run(() => GameBase.SelectedBeatmap.LoadBackground())
                .ContinueWith(t => BackgroundManager.Change(GameBase.SelectedBeatmap.Background));
        }

        //TODO: Remove
        public void PlayMap(object sender, EventArgs e)
        {
            GameBase.SelectedBeatmap.Song.Stop();
            GameStateManager.Instance.ChangeState(new SongLoadingState());
        }

        /// <summary>
        ///     Responsible for repeating the song preview in song select once the song is over.
        /// </summary>
        private void RepeatSongPreview()
        {
            if (GameBase.SelectedBeatmap.Song.GetAudioPosition() < GameBase.SelectedBeatmap.Song.GetAudioLength())
                return;

            GameBase.SelectedBeatmap.Song.Stop();
            GameBase.SelectedBeatmap.LoadAudio();
            GameBase.SelectedBeatmap.Song.Play(GameBase.SelectedBeatmap.AudioPreviewTime);
        }
    }
}
