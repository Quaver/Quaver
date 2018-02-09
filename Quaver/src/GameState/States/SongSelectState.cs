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

        /// <summary>
        ///     The UI that controls and displays beatmap selection
        /// </summary>
        private BeatmapOrganizerUI BeatmapOrganizerUI { get; set; }

        /// <summary>
        ///     Boundary
        /// </summary>
        private Boundary Boundary { get; set; } = new Boundary();

        /// <summary>
        ///     Reference to the play button
        /// </summary>        
        private TextButton PlayButton { get; set; }

        /// <summary>
        ///     Reference to the back button
        /// </summary>        
        private TextButton BackButton { get; set; }

        /// <summary>
        ///     Reference to the speed mod button
        /// </summary>
        private TextButton SpeedModButton { get; set; }

        /// <summary>
        ///     Reference to the toggle pitch button
        /// </summary>
        private TextButton TogglePitch { get; set; }

        /// <summary>
        ///     Position of mouse from previous frame
        /// </summary>
        private float PreviousMouseYPosition { get; set; }

        /// <summary>
        ///     Current Input Manager for this state
        /// </summary>
        private SongSelectInputManager SongSelectInputManager { get; set; }

        /// <summary>
        ///     Determines how much time has passed since initiation
        /// </summary>
        private float TimeElapsedSinceStartup { get; set; }

        /// <summary>
        ///     Stops the Beatmap Organizer from scrolling too fast on high framerate
        /// </summary>
        private float KeyboardScrollBuffer { get; set; }

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
            DiscordController.ChangeDiscordPresence("Song Select", "In the menus");

            // Initalize buttons
            CreatePlayMapButton();
            CreateBackButton();
            CreateSpeedModButton();
            CreateTogglePitchButton();

            // Update overlay
            GameBase.GameOverlay.OverlayActive = true;

            //Add map selected text TODO: remove later
            try
            {
                Logger.Add("MapSelected", "Map Selected: " + GameBase.SelectedBeatmap.Artist + " - " + GameBase.SelectedBeatmap.Title + " [" + GameBase.SelectedBeatmap.DifficultyName + "]", Color.Yellow);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }

            UpdateReady = true;
        }

        /// <summary>
        ///     Unload
        /// </summary>
        public void UnloadContent()
        {
            Logger.Remove("MapSelected");

            UpdateReady = false;
            PlayButton.Clicked -= OnPlayMapButtonClick;
            BackButton.Clicked -= OnBackButtonClick;
            SpeedModButton.Clicked -= OnSpeedModButtonClick;
            TogglePitch.Clicked -= OnTogglePitchButtonClick;

            BeatmapOrganizerUI.UnloadContent();
            Boundary.Destroy();
        }

        /// <summary>
        ///     Update
        /// </summary>
        public void Update(double dt)
        {
            //Check input to update song select ui
            TimeElapsedSinceStartup += (float)dt;
            KeyboardScrollBuffer += (float)dt;

            // It will ignore input until 250ms go by
            if (!BeatmapOrganizerUI.ScrollingDisabled && TimeElapsedSinceStartup > 250)
            {
                SongSelectInputManager.CheckInput();

                // Check and update any mouse input
                if (SongSelectInputManager.RightMouseIsDown)
                    BeatmapOrganizerUI.SetBeatmapOrganizerPosition(-SongSelectInputManager.MouseYPos / GameBase.WindowRectangle.Height);
                else if (SongSelectInputManager.LeftMouseIsDown)
                    BeatmapOrganizerUI.OffsetBeatmapOrganizerPosition(GameBase.MouseState.Position.Y - PreviousMouseYPosition);
                else if (SongSelectInputManager.CurrentScrollAmount != 0)
                    BeatmapOrganizerUI.OffsetBeatmapOrganizerPosition(SongSelectInputManager.CurrentScrollAmount);

                // Check and update any keyboard input
                int scroll = 0;
                if (SongSelectInputManager.UpArrowIsDown || SongSelectInputManager.LeftArrowIsDown)
                    scroll += 1;
                if (SongSelectInputManager.RightArrowIsDown || SongSelectInputManager.DownArrowIsDown)
                    scroll -= 1;

                if (scroll != 0 && KeyboardScrollBuffer > 100)
                {
                    KeyboardScrollBuffer = 0;
                    if (scroll > 0) ScrollUpMapIndex();
                    else if (scroll < 0) ScrollDownMapIndex();
                }
                PreviousMouseYPosition = SongSelectInputManager.MouseYPos;
            }

            //Update Objects
            Boundary.Update(dt);
            BeatmapOrganizerUI.Update(dt);

            // Repeat the song preview if necessary
            RepeatSongPreview();
        }

        /// <summary>
        ///     Draw
        /// </summary>
        public void Draw()
        {
            GameBase.SpriteBatch.Begin();
            BackgroundManager.Draw();
            Boundary.Draw();
            BeatmapOrganizerUI.Draw();
            GameBase.SpriteBatch.End();
        }

        /// <summary>
        ///     Creates and initializes the play button
        /// </summary>
        private void CreatePlayMapButton()
        {
            // Create play button
            PlayButton = new TextButton(new Vector2(200, 50), "Play Map")
            {
                PosY = 140,
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
            GameBase.LoadedSkin.SoundClick.Play(GameBase.SoundEffectVolume, 0, 0);
            GameBase.GameStateManager.ChangeState(new SongLoadingState());
        }

        private void ScrollUpMapIndex()
        {
            BeatmapOrganizerUI.OffsetBeatmapOrganizerIndex(-1);
        }

        private void ScrollDownMapIndex()
        {
            BeatmapOrganizerUI.OffsetBeatmapOrganizerIndex(1);
        }

        /// <summary>
        ///     Responsible for repeating the song preview in song select once the song is over.
        /// </summary>
        private void RepeatSongPreview()
        {
            if (GameBase.AudioEngine.Position < GameBase.AudioEngine.Length)
                return;

            // Reload the audio and play at the song preview
            GameBase.AudioEngine.ReloadStream();
            GameBase.AudioEngine.Play(GameBase.SelectedBeatmap.AudioPreviewTime);
        }

        /// <summary>
        ///     Creates the back button
        /// </summary>        
        private void CreateBackButton()
        {
            // Create back button
            BackButton = new TextButton(new Vector2(200, 50), "Back")
            {
                PosY = - 90,
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
            GameBase.LoadedSkin.SoundBack.Play(GameBase.SoundEffectVolume, 0, 0);
            GameBase.GameStateManager.ChangeState(new MainMenuState());
        }

        /// <summary>
        ///     Creates the speed mod button
        /// </summary>
        private void CreateSpeedModButton()
        {
            // Create Speed Mod Button
            SpeedModButton = new TextButton(new Vector2(200, 50), $"Add Speed Mod {GameBase.AudioEngine.PlaybackRate}x")
            {
                PosY = - 120,
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
                switch ((float)Math.Round(GameBase.AudioEngine.PlaybackRate + 0.1f, 1))
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
            SpeedModButton.TextSprite.Text = $"Add Speed Mod {GameBase.AudioEngine.PlaybackRate}x";
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
            GameBase.AudioEngine.TogglePitch();
            TogglePitch.TextSprite.Text = $"Toggle Pitch: {Configuration.Pitched}";
        }
    }
}
