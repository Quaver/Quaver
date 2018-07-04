using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Discord;
using Quaver.GameState;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UserInterface;
using Quaver.Input;
using Quaver.Logging;
using Quaver.Main;
using Quaver.Modifiers;
using Quaver.States.Loading.Map;
using Quaver.States.Menu;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Graphics;
using Quaver.Graphics.Base;

namespace Quaver.States.Select
{
    internal class SongSelectState : IGameState
    {
        /// <summary>
        ///     The current state
        /// </summary>
        public State CurrentState { get; set; } = State.Menu;

        /// <summary>
        ///     Update Ready?
        /// </summary>
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     The QuaverUserInterface that controls and displays map selection
        /// </summary>
        private MapSelectSystem MapSelectSystem { get; set; }

        /// <summary>
        ///     QuaverContainer
        /// </summary>
        private Container Container { get; set; } = new Container();

        /// <summary>
        ///     Reference to the play button
        /// </summary>        
        private TextButton PlayButton { get; set; }

        /// <summary>
        ///     Reference to the back button
        /// </summary>        
        private TextButton BackButton { get; set; }

        /// <summary>
        ///     Reference to the speed gameplayModifier button
        /// </summary>
        private TextButton SpeedModButton { get; set; }

        /// <summary>
        ///     Reference to the toggle pitch button
        /// </summary>
        private TextButton TogglePitch { get; set; }

        /// <summary>
        ///     Reference to the button to toggle the no pause mod.
        /// </summary>
        private TextButton ToggleNoPause { get; set; }

        /// <summary>
        ///     Button that toggles bots.
        /// </summary>
        private TextButton BotsEnabled { get; set; }

        /// <summary>
        ///     Button that dictates the bot count.
        /// </summary>
        private TextButton BotCount { get; set; }

        /// <summary>
        ///     Button that enables autoplay.
        /// </summary>
        private TextButton AutoplayEnabled { get; set; }

        /// <summary>
        ///     Search bar for song searching
        /// </summary>
        private TextInputField SearchField { get; set; }

        /// <summary>MapSelectSystem
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
        ///     Stops the Map Organizer from scrolling too fast on high framerate
        /// </summary>
        private float KeyboardScrollBuffer { get; set; }

        /// <summary>
        ///     The display for scores on the map.
        /// </summary>
        internal ScoresDisplay ScoreDisplay { get; set; }

        /// <summary>
        ///     Initialize
        /// </summary>
        public void Initialize()
        {
            GameBase.GameWindow.Title = "Quaver";

            if (GameBase.SelectedMap == null)
                MapsetHelper.SelectRandomMap();

            //Initialize Helpers
            MapSelectSystem = new MapSelectSystem(this);
            MapSelectSystem.Initialize(this);
            SongSelectInputManager = new SongSelectInputManager();

            // Update Discord Presence
            DiscordManager.Presence.Details = "Selecting a song";
            DiscordManager.Presence.State = "In the menus";
            DiscordManager.Presence.Timestamps = null;
            DiscordManager.Client.SetPresence(DiscordManager.Presence);

            // Initalize buttons
            CreatePlayMapButton();
            CreateBackButton();
            CreateSpeedModButton();
            CreateTogglePitchButton();
            CreateSearchField();
            CreateNoPause();
            CreateBotButtons();
            BackgroundManager.Change();
            BackgroundManager.Readjust();

            ScoreDisplay = new ScoresDisplay() { Parent = Container};
            
            LoadScores();
            UpdateReady = true;
        }

        /// <summary>
        ///     Unload
        /// </summary>
        public void UnloadContent()
        {
            Logger.Clear();

            UpdateReady = false;
            PlayButton.Clicked -= OnPlayMapButtonClick;
            BackButton.Clicked -= OnBackButtonClick;
            SpeedModButton.Clicked -= OnSpeedModButtonClick;
            TogglePitch.Clicked -= OnTogglePitchButtonClick;
            MapSelectSystem.UnloadContent();
            Container.Destroy();
        }

        /// <summary>
        ///     Update
        /// </summary>
        public void Update(double dt)
        {
            //Check input to update song select ui
            TimeElapsedSinceStartup += (float)dt;
            KeyboardScrollBuffer += (float)dt;
            GameBase.Navbar.PerformShowAnimation(dt);
            
            //Update Objects
            Container.Update(dt);
            MapSelectSystem.Update(dt);

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
            Container.Draw();
            MapSelectSystem.Draw();

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
                PosY = 300 * GameBase.WindowUIScale + 80,
                Alignment = Alignment.TopLeft,
                Parent = Container
            };

            PlayButton.Clicked += OnPlayMapButtonClick;
        }

        /// <summary>
        ///     Changes to the song loading state when the play map button is clicked.
        /// </summary>
        private void OnPlayMapButtonClick(object sender, EventArgs e)
        {
            GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundClick);
            GameBase.GameStateManager.ChangeState(new MapLoadingState(ScoreDisplay.Scores));
        }

        /// <summary>
        ///     Responsible for repeating the song preview in song select once the song is over.
        /// </summary>
        private static void RepeatSongPreview()
        {
            if (GameBase.AudioEngine.Position < GameBase.AudioEngine.Length || AudioEngine.Stream == 0)
                return;

            // Reload the audio and play at the song preview
            try
            {
                GameBase.AudioEngine.ReloadStream();
                GameBase.AudioEngine.Play(GameBase.SelectedMap.AudioPreviewTime);
            } catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Creates the back button
        /// </summary>        
        private void CreateBackButton()
        {
            // Create back button
            BackButton = new TextButton(new Vector2(200, 50), "Back")
            {
                PosY = -10,
                Alignment = Alignment.BotLeft,
                Parent = Container
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
            GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundBack);
            GameBase.GameStateManager.ChangeState(new MainMenuScreen());
        }

        /// <summary>
        ///     Creates the speed gameplayModifier button
        /// </summary>
        private void CreateSpeedModButton()
        {
            // Create ManiaModSpeed Mod QuaverButton
            SpeedModButton = new TextButton(new Vector2(200, 50), $"Add Speed Mod {GameBase.AudioEngine.PlaybackRate}x")
            {
                PosY = 300 * GameBase.WindowUIScale + 200,
                Alignment = Alignment.TopLeft,
                Parent = Container
            };
            SpeedModButton.Clicked += OnSpeedModButtonClick;
        }

        /// <summary>
        ///     Adds speed gameplayModifier to game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSpeedModButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Activate the current speed gameplayModifier depending on the (current game clock + 0.1)
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
                Logger.LogError(ex, LogType.Runtime);
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
            TogglePitch = new TextButton(new Vector2(200, 50), $"Toggle Pitch: {ConfigManager.Pitched.Value}")
            {
                Alignment = Alignment.TopLeft,
                PosY = 300 * GameBase.WindowUIScale + 140,
                Parent = Container
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
            TogglePitch.TextSprite.Text = $"Toggle Pitch: {ConfigManager.Pitched.Value}";
        }

        private void CreateSearchField()
        {
            SearchField = new TextInputField(new Vector2(300, 30), "Search Mapset", (search) => MapSelectSystem.OnSearchbarUpdated(search))
            {
                Alignment = Alignment.TopLeft,
                PosX = 5,
                PosY = 300 * GameBase.WindowUIScale + 45,
                Parent = Container
            };

            var name = (ConfigManager.Username.Value == "") ? "Enter Your Player Name!" : ConfigManager.Username.Value;
            var enterName = new TextInputField(new Vector2(300, 30), name, null)
            {
                Alignment = Alignment.TopLeft,
                PosX = 5,
                PosY = 300 * GameBase.WindowUIScale + 5,
                Parent = Container
            };
            
            enterName.OnTextInputSubmit += (term) =>
            {
                if (term == "")
                    return;

                ConfigManager.Username.Value = term;
                
                enterName.PlaceHolderText = (ConfigManager.Username.Value == "") ? "Enter Your Player Name!" : ConfigManager.Username.Value;
            };
        }

        /// <summary>
        ///     Creates button for no pause.
        /// </summary>
        private void CreateNoPause()
        {
            ToggleNoPause = new TextButton(new Vector2(200, 50), $"No Pause Mod: {ModManager.IsActivated(ModIdentifier.NoPause)}")
            {
                Alignment = Alignment.TopLeft,
                PosY = 300 * GameBase.WindowUIScale + 300,
                Parent = Container
            };
            
            ToggleNoPause.Clicked += (o, e) =>
            {
                if (!ModManager.IsActivated(ModIdentifier.NoPause))
                    ModManager.AddMod(ModIdentifier.NoPause);
                else
                    ModManager.RemoveMod(ModIdentifier.NoPause);
                
                ToggleNoPause.TextSprite.Text = $"No Pause Mod: {ModManager.IsActivated(ModIdentifier.NoPause)}";
            };
        }

        private void CreateBotButtons()
        {
            BotsEnabled = new TextButton(new Vector2(200, 50), $"Enable Bots: {ConfigManager.BotsEnabled.Value}")
            {
                Alignment = Alignment.TopLeft,
                PosY = 300 * GameBase.WindowUIScale - 100,
                Parent = Container
            };

            BotsEnabled.Clicked += (o, e) =>
            {
                ConfigManager.BotsEnabled.Value = !ConfigManager.BotsEnabled.Value;
                BotsEnabled.TextSprite.Text = $"Enable Bots: {ConfigManager.BotsEnabled.Value}";
            };
            
            BotCount = new TextButton(new Vector2(200, 50), $"Bot Count: {ConfigManager.BotCount.Value}")
            {
                Alignment = Alignment.TopLeft,
                PosY = 300 * GameBase.WindowUIScale - 175,
                Parent = Container
            };

            BotCount.Clicked += (o, e) =>
            {
                if (ConfigManager.BotCount.Value + 1 > ConfigManager.BotCount.MaxValue)
                    ConfigManager.BotCount.Value = ConfigManager.BotCount.MinValue;
                else
                    ConfigManager.BotCount.Value++;
                
                BotCount.TextSprite.Text = $"Bot Count: {ConfigManager.BotCount.Value}";
            };

            AutoplayEnabled = new TextButton(new Vector2(200, 50), $"Autoplay: {ModManager.IsActivated(ModIdentifier.Autoplay)}")
            {
                Alignment = Alignment.TopLeft,
                PosY = 50,
                Parent = Container
            };

            AutoplayEnabled.Clicked += (o, e) =>
            {
                Console.WriteLine("hi");
                if (ModManager.IsActivated(ModIdentifier.Autoplay))
                    ModManager.RemoveMod(ModIdentifier.Autoplay);
                else
                    ModManager.AddMod(ModIdentifier.Autoplay);

                AutoplayEnabled.TextSprite.Text = $"Autoplay: {ModManager.IsActivated(ModIdentifier.Autoplay)}";
            };
        }
        
        /// <summary>
        ///     Loads all user scores for this map.
        /// </summary>
        internal void LoadScores()
        {
            Task.Run(async () =>
            {
                try
                {   
                    var scores = await LocalScoreCache.FetchMapScores(GameBase.SelectedMap.Md5Checksum);
                    ScoreDisplay.UpdateDisplay(scores);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, LogType.Runtime);
                }
            }).Wait();
        }
    }
}
