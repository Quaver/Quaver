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
using Quaver.Config;
using Quaver.Discord;
using Quaver.GameState.States;
using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;
using Quaver.Logging;

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
        private TextButton SpeedModButton { get; set; }

        // Another test mod button
        private TextButton TogglePitch { get; set; }

        public void Initialize()
        {
            GameBase.GameWindow.Title = "Quaver";

            // Update Discord Presence
            GameBase.ChangeDiscordPresence("In song select");

            CreateSongSelectButtons();

            // Create play button
            PlayButton = new TextButton(new Vector2(200, 30), "Play")
            {
                Alignment = Alignment.TopRight,
                Parent = Boundary
            };
            PlayButton.Clicked += PlayMap;

            // Create back button
            BackButton = new TextButton(new Vector2(200, 50), "Back")
            {
                Alignment = Alignment.BotCenter,
                Parent = Boundary
            };
            BackButton.Clicked += OnBackButtonClick;

            // Create Speed Mod Button
            SpeedModButton = new TextButton(new Vector2(200, 50), $"Add Speed Mod {GameBase.GameClock}x")
            {
                Alignment = Alignment.BotRight,
                Parent = Boundary
            };
            SpeedModButton.Clicked += OnSpeedModButtonClick;

            // Create Speed Mod Button
            TogglePitch = new TextButton(new Vector2(200, 50), $"Toggle Pitch: {Configuration.Pitched}")
            {
                Alignment = Alignment.MidRight,
                Parent = Boundary
            };
            TogglePitch.Clicked += OnTogglePitchButtonClick;

            //Add map selected text TODO: remove later
            Logger.Add("MapSelected", "Map Selected: " + GameBase.SelectedBeatmap.Artist + " - " + GameBase.SelectedBeatmap.Title + " [" + GameBase.SelectedBeatmap.DifficultyName + "]", Color.Yellow);
            UpdateReady = true;
        }

        public void UnloadContent()
        {
            Logger.Remove("MapSelected");

           UpdateReady = false;
            PlayButton.Clicked -= PlayMap;

            //TODO: Remove button delegates ?
            for (int i=0; i<Buttons.Count; i++)
            {
                Buttons[i].Clicked -= ClickEvents[i];
            }
            ClickEvents.Clear();
            Boundary.Destroy();
            Buttons.Clear();
        }

        public void Update(double dt)
        {
            Boundary.Update(dt);

            // Repeat the song preview if necessary
            RepeatSongPreview();
        }

        public void Draw()
        {
            Boundary.Draw();
        }

        public void CreateSongSelectButtons()
        {
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
        }

        //TODO: Remove
        public void ButtonClick(object sender, EventArgs e, string text, Beatmap map)
        {
            Logger.Update("MapSelected","Map Selected: "+text);

            //Select map
            GameBase.ChangeBeatmap(map);

            // Change Pitch Text
            TogglePitch.TextSprite.Text = $"Toggle Pitch: {Configuration.Pitched}";

            // Play Song
            if (GameBase.SelectedBeatmap.Song != null)
                GameBase.SelectedBeatmap.Song.Play(GameBase.SelectedBeatmap.AudioPreviewTime);

            // Load background asynchronously.
            Task.Run(() => GameBase.SelectedBeatmap.LoadBackground())
                .ContinueWith(t => BackgroundManager.Change(GameBase.SelectedBeatmap.Background));
        }

        //TODO: Remove
        public void PlayMap(object sender, EventArgs e)
        {
            GameBase.GameStateManager.ChangeState(new SongLoadingState());
        }

        /// <summary>
        ///     Responsible for repeating the song preview in song select once the song is over.
        /// </summary>
        private void RepeatSongPreview()
        {
            if (GameBase.SelectedBeatmap.Song != null)
            {
                if (GameBase.SelectedBeatmap.Song.GetAudioPosition() < GameBase.SelectedBeatmap.Song.GetAudioLength())
                    return;

                GameBase.SelectedBeatmap.Song.Stop();
                GameBase.SelectedBeatmap.LoadAudio();
                GameBase.SelectedBeatmap.Song.Play(GameBase.SelectedBeatmap.AudioPreviewTime);
            }
        }

        /// <summary>
        ///     Whenever the back button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackButtonClick(object sender, EventArgs e)
        {
            GameBase.GameStateManager.ChangeState(new MainMenuState());
        }

        /// <summary>
        ///     Adds speed mod to game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpeedModButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Activate the current speed mod depending on the (current game clock + 0.1)
                switch ((float)Math.Round(GameBase.GameClock + 0.1f, 1))
                {
                    // In this case, 2.1 really means 0.5x, given that we're checking
                    // for the current GameClock + 0.1. If it's 2.1, we reset it back to 0.5x
                    case 2.1f:
                        ModManager.AddMod(ModIdentifier.Speed05X);
                        break;
                    // If it ends up being 1.0x, we'll just go ahead and remove all the mods.
                    case 1.0f:
                        ModManager.RemoveSpeedMods();
                        break;
                    case 0.6f:
                        ModManager.AddMod(ModIdentifier.Speed06X);
                        break;
                    case 0.7f:
                        ModManager.AddMod(ModIdentifier.Speed07X);
                        break;
                    case 0.8f:
                        ModManager.AddMod(ModIdentifier.Speed08X);
                        break;
                    case 0.9f:
                        ModManager.AddMod(ModIdentifier.Speed09X);
                        break;
                    case 1.1f:
                        ModManager.AddMod(ModIdentifier.Speed11X);
                        break;
                    case 1.2f:
                        ModManager.AddMod(ModIdentifier.Speed12X);
                        break;
                    case 1.3f:
                        ModManager.AddMod(ModIdentifier.Speed13X);
                        break;
                    case 1.4f:
                        ModManager.AddMod(ModIdentifier.Speed14X);
                        break;
                    case 1.5f:
                        ModManager.AddMod(ModIdentifier.Speed15X);
                        break;
                    case 1.6f:
                        ModManager.AddMod(ModIdentifier.Speed16X);
                        break;
                    case 1.7f:
                        ModManager.AddMod(ModIdentifier.Speed17X);
                        break;
                    case 1.8f:
                        ModManager.AddMod(ModIdentifier.Speed18X);
                        break;
                    case 1.9f:
                        ModManager.AddMod(ModIdentifier.Speed19X);
                        break;
                    case 2.0f:
                        ModManager.AddMod(ModIdentifier.Speed20X);
                        break;
                }
            }
            catch (Exception ex)
            {
                ModManager.RemoveSpeedMods();
            }

            // Change the song speed directly.
            SpeedModButton.TextSprite.Text = $"Add Speed Mod {GameBase.GameClock}x";
        }

        /// <summary>
        ///     Toggles pitching for speed modifications
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTogglePitchButtonClick(object sender, EventArgs e)
        {
            GameBase.SelectedBeatmap.Song.ToggleSongPitch();
            TogglePitch.TextSprite.Text = $"Toggle Pitch: {Configuration.Pitched}";
        }
    }
}
