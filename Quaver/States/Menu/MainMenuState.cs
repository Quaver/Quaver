using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ionic.Zip;
using Microsoft.Xna.Framework;
using Quaver.Commands;
using Quaver.Config;
using Quaver.Discord;
using Quaver.GameState;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Helpers;
using Quaver.Logging;
using Quaver.Main;
using Quaver.Modifiers;
using Quaver.States.Enums;
using Quaver.States.Options;
using Quaver.States.Select;
using Quaver.States.Tests;

namespace Quaver.States.Menu
{
    internal class MainMenuState : IGameState
    {
        /// <summary>
        ///     State
        /// </summary>
        public State CurrentState { get; set; } = State.MainMenu;

        /// <summary>
        ///     Update Ready
        /// </summary>
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     QuaverContainer
        /// </summary>
        public QuaverContainer QuaverContainer { get; set; }

        /// <summary>
        ///     QuaverButton to switch to the song select state
        /// </summary>
        public QuaverButton SwitchSongSelectQuaverButton { get; set; }

        /// <summary>
        ///     QuaverButton to switch to the options menu
        /// </summary>
        public QuaverButton OptionsMenuQuaverButton { get; set; }

        /// <summary>
        ///     QuaverButton to export .qp
        /// </summary>
        public QuaverButton ExportQpQuaverButton { get; set; }

        /// <summary>
        ///     Initialize
        /// </summary>
        public void Initialize()
        {
            GameBase.GameWindow.Title = "Quaver";

            // Remove speed mods upon going to the main menu so songs can be played at normal speed.
            if (GameBase.CurrentGameModifiers.Count > 0)
                ModManager.RemoveSpeedMods();

            // Set Discord RP
            DiscordController.ChangeDiscordPresence("Main Menu", "In the menus");

#if DEBUG
            // Enable console commands (Only applicable if on debug release)
            CommandHandler.HandleConsoleCommand();
#endif

            //Initialize Menu Screen
            QuaverContainer = new QuaverContainer();

            var test = new QuaverTextButton(new Vector2(200, 40), "Menu Nav Test")
            {
                Alignment = Alignment.MidCenter,
                PosY = 100,
                Parent = QuaverContainer
            };

            test.Clicked += (sender, args) => GameBase.GameStateManager.ChangeState(new NavbarTestState());
            
            // Initialize the QuaverUserInterface buttons
            CreateMenuButtons();
            CreateQpExportButton();

            UpdateReady = true;
        }

        /// <summary>
        ///     Unload
        /// </summary>
        public void UnloadContent()
        {
            UpdateReady = false;
            QuaverContainer.Destroy();
        }

        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(double dt)
        {
            //Update Menu Screen QuaverContainer
            QuaverContainer.Update(dt);
        }
        
        /// <summary>
        ///     Draw
        /// </summary>
        public void Draw()
        {
            GameBase.SpriteBatch.Begin();
            //BackgroundManager.Draw();
            QuaverContainer.Draw();
            GameBase.SpriteBatch.End();
        }

        /// <summary>
        ///     Responsible for creating the button to move to the song select screen state
        /// </summary>
        private void CreateMenuButtons()
        {
            // Switch Song Select QuaverButton
            SwitchSongSelectQuaverButton = new QuaverTextButton(new Vector2(200, 40), "Song Select")
            {
                Alignment = Alignment.MidCenter,
                Parent = QuaverContainer
            };
            SwitchSongSelectQuaverButton.Clicked += OnSongSelectButtonClick;

            OptionsMenuQuaverButton = new QuaverTextButton(new Vector2(200, 40), "Options")
            {
                Alignment = Alignment.MidCenter,
                PosY = 50,
                Parent = QuaverContainer
            };
            OptionsMenuQuaverButton.Clicked += OnOptionsSelectButtonClick;
        }

        /// <summary>
        ///     The event handler that switches to the song select screen
        /// </summary>
        public void OnSongSelectButtonClick(object sender, EventArgs e)
        {
            //Change to SongSelectState
            GameBase.LoadedSkin.SoundClick.Play(GameBase.SoundEffectVolume, 0, 0);

            // Don't proceed to song select if the user doesn't have any mapsets.
            if (GameBase.Mapsets.Count == 0)
            {
                Logger.LogImportant("Cannot go to song select with 0 loaded mapsets.", LogType.Runtime);
                return;
            }

            GameBase.GameStateManager.ChangeState(new SongSelectState());
        }

        public void OnOptionsSelectButtonClick(object sender, EventArgs e)
        {
            GameBase.LoadedSkin.SoundClick.Play(GameBase.SoundEffectVolume, 0, 0);
            GameBase.GameStateManager.ChangeState(new OptionsState());
        }
        
        /// <summary>
        ///     Responsible for creating the import .qp button
        /// </summary>
        private void CreateQpExportButton()
        {
            // Import .osz QuaverButton
            ExportQpQuaverButton = new QuaverTextButton(new Vector2(200, 40), "Export Current Mapset")
            {
                Alignment = Alignment.BotRight,
                Parent = QuaverContainer
            };

            ExportQpQuaverButton.Clicked += OnExportButtonClick;
        }

        /// <summary>
        ///     Responsible for zipping the selected mapset
        /// </summary>
        private static void OnExportButtonClick(object sender, EventArgs e)
        {
            var zip = new ZipFile();

            // Get all the files in the current selected map's directory.
            var dirInfo = new DirectoryInfo(ConfigManager.SongDirectory + "/" + GameBase.SelectedMap.Directory + "/");
            var files = dirInfo.GetFiles();

            foreach (var file in files)
                zip.AddFile(ConfigManager.SongDirectory + "/" + GameBase.SelectedMap.Directory + "/" + file, "");

            // Create the Data/Maps directory if it doesn't exist already.
            Directory.CreateDirectory($"{ConfigManager.DataDirectory}/Maps/");

            // Save the file
            var outputPath = $"{ConfigManager.DataDirectory}/Maps/{GameBase.GameTime.ElapsedMilliseconds} {StringHelper.FileNameSafeString(GameBase.SelectedMap.Artist)} - {StringHelper.FileNameSafeString(GameBase.SelectedMap.Title)}.qp";
            zip.Save(outputPath);

            Logger.LogSuccess($"Successfully exported {outputPath}", LogType.Runtime);

            // Open the folder where the file is contained.
            if (!File.Exists(outputPath))
                return;

            // TODO: Fix for linux/mac.
            try
            {
                Console.WriteLine(outputPath);
                System.Diagnostics.Process.Start("explorer.exe", "/select," + "\"" + $@"{outputPath.Replace("/", "\\")}" + "\"");
            }
            catch (Exception ex) 
            { 
                //Logger.Error(ex);
            }
        }
    }
}
