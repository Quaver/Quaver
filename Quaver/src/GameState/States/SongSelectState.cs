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
using Quaver.Modifiers;

namespace Quaver.GameState.States
{
    internal class SongSelectState : IGameState
    {
        public State CurrentState { get; set; } = State.MainMenu;
        public bool UpdateReady { get; set; }

        //TODO: update later   TEST.
        private List<Button> Buttons { get; set; } = new List<Button>();
        private List<EventHandler> ClickEvents { get; set; } = new List<EventHandler>();
        private Button PlayButton { get; set; }
        private Boundary Boundary { get; set; } = new Boundary();
        private int ButtonPos { get; set; } = 50;
        private Button BackButton { get; set; }

        // Test Mod Button
        private Button SpeedModButton { get; set; }

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
                    EventHandler curEvent = (sender, e) => ButtonClick(sender, e, map.Artist + " - " + map.Title + " [" + map.DifficultyName + "]", currentMap);
                    ClickEvents.Add(curEvent);
                    newButton.Clicked += curEvent;
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

            // Create back button
            BackButton = new TextButton(new Vector2(200, 50), "Back")
            {
                Image = GameBase.UI.BlankBox,
                Alignment = Alignment.BotLeft,
                Parent = Boundary
            };
            BackButton.Clicked += OnBackButtonClick;

            // Create Speed Mod Button
            SpeedModButton = new TextButton(new Vector2(200, 50), "Add Speed Mod")
            {
                Image = GameBase.UI.BlankBox,
                Alignment = Alignment.BotCenter,
                Parent = Boundary
            };
            SpeedModButton.Clicked += OnSpeedModButtonClick;

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
            for (int i=0; i<Buttons.Count; i++)
            {
                Buttons[i].Clicked -= ClickEvents[i];
            }
            ClickEvents.Clear();
            Buttons.Clear();
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

        /// <summary>
        ///     Whenever the back button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackButtonClick(object sender, EventArgs e)
        {
            GameStateManager.Instance.ChangeState(new MainMenuState());
        }

        /// <summary>
        ///     Adds speed mod to game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpeedModButtonClick(object sender, EventArgs e)
        {
            // Add the mod, but make sure it can only be between 0.75 and 2.0x speed.
            if (GameBase.GameClock < 2.0)
                ModManager.AddMod(ModIdentifier.Speed, GameBase.GameClock + 0.1f);
            else
                ModManager.AddMod(ModIdentifier.Speed, 0.75f);

            // Change the song speed directly.
            if (GameBase.SelectedBeatmap.Song != null)
                GameBase.SelectedBeatmap.Song.ChangeSongSpeed();
        }
    }
}
