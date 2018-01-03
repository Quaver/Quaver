using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Audio;
using Quaver.Database.Beatmaps;
using Quaver.Config;
using Quaver.Discord;
using Quaver.GameState.States;
using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;
using Quaver.Logging;

using Quaver.Modifiers;
using Quaver.GameState.SongSelect;
using Quaver.Input;
using Quaver.Utility;

namespace Quaver.GameState.States
{
    internal class SongSelectState : IGameState
    {
        /// <summary>
        ///     The current state
        /// </summary>
        public State CurrentState { get; set; } = State.MainMenu;

        /// <summary>
        ///     Update Ready?
        /// </summary>
        public bool UpdateReady { get; set; }

        private BeatmapOrganizerUI BeatmapOrganizerUI { get; set; }

        /// <summary>
        ///     Boundary
        /// </summary>
        private Boundary Boundary { get; set; } = new Boundary();

        /// <summary>
        ///     Reference to the play button
        /// </summary>        
        private Button PlayButton { get; set; }

        /// <summary>
        ///     Reference to the back button
        /// </summary>        
        private Button BackButton { get; set; }

        /// <summary>
        ///     Reference to the speed mod button
        /// </summary>
        private TextButton SpeedModButton { get; set; }

        /// <summary>
        ///     Reference to the toggle pitch button
        /// </summary>
        private TextButton TogglePitch { get; set; }

        private SongSelectInputManager SongSelectInputManager { get; set;}

        /// <summary>
        ///     Initialize
        /// </summary>
        public void Initialize()
        {
            GameBase.GameWindow.Title = "Quaver";

            //Initialize Helpers
            BeatmapOrganizerUI = new BeatmapOrganizerUI();
            BeatmapOrganizerUI.Initialize(this);
            SongSelectInputManager = new SongSelectInputManager();

            // Update Discord Presence
            GameBase.ChangeDiscordPresence("Song Select", "In the menus");

            // Initalize buttons
            CreatePlayMapButton();
            CreateBackButton();
            CreateSpeedModButton();
            CreateTogglePitchButton();

            //Add map selected text TODO: remove later
            Logger.Add("MapSelected", "Map Selected: " + GameBase.SelectedBeatmap.Artist + " - " + GameBase.SelectedBeatmap.Title + " [" + GameBase.SelectedBeatmap.DifficultyName + "]", Color.Yellow);
            UpdateReady = true;
        }

        /// <summary>
        ///     Unload
        /// </summary>
        public void UnloadContent()
        {
            Logger.Remove("MapSelected");

            UpdateReady = false;
            //PlayButton.Clicked -= OnPlayMapButtonClick;

            BeatmapOrganizerUI.UnloadContent();

            Boundary.Destroy();
        }

        /// <summary>
        ///     Update
        /// </summary>
        public void Update(double dt)
        {
            Boundary.Update(dt);
            BeatmapOrganizerUI.Update(dt);
            SongSelectInputManager.CheckInput();
            if (SongSelectInputManager.RightMouseIsDown) BeatmapOrganizerUI.SetBeatmapOrganizerPosition(GameBase.MouseState.Position.Y/(GameBase.Window.Y - GameBase.Window.Height));
            //SetBeatmapOrganizerPosition

                // Repeat the song preview if necessary
            RepeatSongPreview();
        }

        /// <summary>
        ///     Draw
        /// </summary>
        public void Draw()
        {
            Boundary.Draw();
            BeatmapOrganizerUI.Draw();
        }

        /// <summary>
        ///     Creates and initializes the play button
        /// </summary>
        private void CreatePlayMapButton()
        {
            // Create play button
            PlayButton = new TextButton(new Vector2(200, 30), "Play Map")
            {
                Alignment = Alignment.TopLeft,
                Parent = Boundary
            };

            PlayButton.Clicked += OnPlayMapButtonClick;
        }

        /// <summary>
        ///     Changes to the song loading state when the play map button is clicked.
        /// </summary>
        private void OnPlayMapButtonClick(object sender, EventArgs e)
        {
            GameBase.LoadedSkin.Click.Play((float) Configuration.VolumeGlobal / 100 * Configuration.VolumeEffect / 100,0, 0);
            GameBase.GameStateManager.ChangeState(new SongLoadingState());
        }

        /// <summary>
        ///     Responsible for repeating the song preview in song select once the song is over.
        /// </summary>
        private void RepeatSongPreview()
        {
            if (SongManager.Position < SongManager.Length)
                return;

            // Reload the audio and play at the song preview
            SongManager.ReloadSong(true);
        }

        /// <summary>
        ///     Creates the back button
        /// </summary>        
        private void CreateBackButton()
        {
            // Create back button
            BackButton = new TextButton(new Vector2(200, 50), "Back")
            {
                Alignment = Alignment.BotCenter,
                Parent = Boundary
            };
            BackButton.Clicked += OnBackButtonClick;
        }

        /// <summary>
        ///     Whenever the back button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackButtonClick(object sender, EventArgs e)
        {
            GameBase.LoadedSkin.Back.Play((float) Configuration.VolumeGlobal / 100 * Configuration.VolumeEffect / 100,0, 0);
            GameBase.GameStateManager.ChangeState(new MainMenuState());
        }

        /// <summary>
        ///     Creates the speed mod button
        /// </summary>
        private void CreateSpeedModButton()
        {
            // Create Speed Mod Button
            SpeedModButton = new TextButton(new Vector2(200, 50), $"Add Speed Mod {GameBase.GameClock}x")
            {
                Alignment = Alignment.BotLeft,
                Parent = Boundary
            };
            SpeedModButton.Clicked += OnSpeedModButtonClick;
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
        ///     Creates the toggle pitch button
        /// </summary>
        private void CreateTogglePitchButton()
        {
            TogglePitch = new TextButton(new Vector2(200, 50), $"Toggle Pitch: {Configuration.Pitched}")
            {
                Alignment = Alignment.MidLeft,
                Parent = Boundary
            };
            TogglePitch.Clicked += OnTogglePitchButtonClick;
        }

        /// <summary>
        ///     Toggles pitching for speed modifications
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTogglePitchButtonClick(object sender, EventArgs e)
        {
            SongManager.ToggleSongPitch();
            TogglePitch.TextSprite.Text = $"Toggle Pitch: {Configuration.Pitched}";
        }
    }
}
