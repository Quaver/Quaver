using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Ionic.Zip;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Osu;
using Quaver.Audio;
using Quaver.Database.Beatmaps;
using Quaver.Config;
using Quaver.Discord;
using Quaver.GameState.Gameplay;
using Quaver.Graphics;
using Quaver.Logging;
using Quaver.Modifiers;
using Quaver.API.Maps;
using Quaver.Commands;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Helpers;
using Quaver.Peppy;
using Quaver.Steam;
using Quaver.StepMania;

namespace Quaver.GameState.States
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
        ///     QuaverButton to import .qp
        /// </summary>
        public QuaverButton ImportQpQuaverButton { get; set; }

        /// <summary>
        ///     QuaverButton to export .qp
        /// </summary>
        public QuaverButton ExportQpQuaverButton { get; set; }

        /// <summary>
        ///     QuaverButton to import .osz
        /// </summary>
        public QuaverButton ImportPeppyQuaverButton { get; set; }

        /// <summary>
        ///     QuaverButton to convert .sm files
        /// </summary>
        public QuaverButton ConvertStepManiaQuaverButton { get; set; }

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

            // Initialize the UI buttons
            CreateOszImportButton();
            CreateQpImportButton();
            CreateMenuButtons();
            CreateQpExportButton();
            CreateConvertSmButton();

            UpdateReady = true;
        }

        /// <summary>
        ///     Unload
        /// </summary>
        public void UnloadContent()
        {
            UpdateReady = false;
            SwitchSongSelectQuaverButton.Clicked -= OnSongSelectButtonClick;
            OptionsMenuQuaverButton.Clicked -= OnOptionsSelectButtonClick;
            QuaverContainer.Destroy();
        }

        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(double dt)
        {
            // Play Random Maps during the main menu
            //MenuAudioPlayer.PlayRandomBeatmaps();
            //Console.WriteLine(SwitchSongSelectQuaverButton.GlobalRectangle.X + ", " + SwitchSongSelectQuaverButton.GlobalRectangle.Y + ", " + SwitchSongSelectQuaverButton.GlobalRectangle.Width + ", " + SwitchSongSelectQuaverButton.GlobalRectangle.Height);

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
            GameBase.GameStateManager.ChangeState(new OptionsMenuState());
        }

        /// <summary>
        ///     Responsible for creating the import .osz button
        /// </summary>
        private void CreateOszImportButton()
        {
            // Import .osz QuaverButton
            ImportPeppyQuaverButton = new QuaverTextButton(new Vector2(200, 40), "Import .osz")
            {
                Alignment = Alignment.TopCenter,
                Parent = QuaverContainer
            };

            ImportPeppyQuaverButton.Clicked += OnImportOsuButtonClick;
        }

        /// <summary>
        ///     Responsible for creating the button to convert .sm files
        /// </summary>
        private void CreateConvertSmButton()
        {
            ConvertStepManiaQuaverButton = new QuaverTextButton(new Vector2(200, 400), "Convert StepMania file")
            {
                Alignment = Alignment.MidLeft,
                Parent = QuaverContainer
            };

            ConvertStepManiaQuaverButton.Clicked += OnConvertSmButtonClick;
        }

        /// <summary>
        ///     Responsible for creating the import .qp button
        /// </summary>
        private void CreateQpImportButton()
        {
            // Import .osz QuaverButton
            ImportQpQuaverButton = new QuaverTextButton(new Vector2(200, 40), "Import Quaver Mapset")
            {
                Alignment = Alignment.BotCenter,
                Parent = QuaverContainer
            };

            ImportQpQuaverButton.Clicked += OnImportQpButtonClick;
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
        ///     Upon clicking the button to import a .qp archive, this will handle
        ///     the dialog portion of it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void OnImportQpButtonClick(object sender, EventArgs e)
        {
            // Create the openFileDialog object.
            var openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = "c:\\",
                Filter = "Quaver Mapset (*.qp)|*.qp",
                FilterIndex = 0,
                RestoreDirectory = true,
                Multiselect = true
            };

            // If the dialog couldn't be shown, that's an issue, so we'll return for now.
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            // Proceed to extract and convert the map, show loading screen.
            GameBase.GameStateManager.AddState(new MapImportLoadingState());

            // Run the converter for all file names
            Task.Run(() =>
            {
                foreach (var fileName in openFileDialog.FileNames)
                    ImportQp(fileName);

                // When all the maps have been converted, select the last imported map and make that the selected one.
            }).ContinueWith(async t =>
            {
                await MapImportLoadingState.AfterImport();
            });
        }

        /// <summary>
        ///     Responsible for extracting the files from the .qp 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="num"></param>
        private static void ImportQp(string fileName)
        {
            var extractPath = $@"{Config.ConfigManager.SongDirectory}/{Path.GetFileNameWithoutExtension(fileName)}/";

            try
            {
                using (var archive = new ZipFile(fileName))
                {
                    archive.ExtractAll(extractPath, ExtractExistingFileAction.OverwriteSilently);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Responsible for zipping the selected mapset
        /// </summary>
        private static void OnExportButtonClick(object sender, EventArgs e)
        {
            var zip = new ZipFile();

            // Get all the files in the current selected map's directory.
            var dirInfo = new DirectoryInfo(ConfigManager.SongDirectory + "/" + GameBase.SelectedBeatmap.Directory + "/");
            var files = dirInfo.GetFiles();

            foreach (var file in files)
                zip.AddFile(ConfigManager.SongDirectory + "/" + GameBase.SelectedBeatmap.Directory + "/" + file, "");

            // Create the Data/Maps directory if it doesn't exist already.
            Directory.CreateDirectory($"{ConfigManager.DataDirectory}/Maps/");

            // Save the file
            var outputPath = $"{ConfigManager.DataDirectory}/Maps/{GameBase.GameTime.ElapsedMilliseconds} {StringHelper.FileNameSafeString(GameBase.SelectedBeatmap.Artist)} - {StringHelper.FileNameSafeString(GameBase.SelectedBeatmap.Title)}.qp";
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

        /// <summary>
        ///     Upon clicking the import button, we'll want to display an open file dialog,
        ///     then attempt to convert all 4k & 7k beatmaps and create a new song directory
        ///     for the import queue.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnImportOsuButtonClick(object sender, EventArgs e)
        {
            // Create the openFileDialog object.
            var openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = "c:\\",
                Filter = "Peppy Beatmap Set (*.osz)|*.osz",
                FilterIndex = 0,
                RestoreDirectory = true,
                Multiselect = true
            };

            // If the dialog couldn't be shown, that's an issue, so we'll return for now.
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            // Proceed to extract and convert the map, show loading screen.
            GameBase.GameStateManager.AddState(new MapImportLoadingState());

            // Run the converter for all file names
            Task.Run(() =>
            {
                for (var i = 0; i < openFileDialog.FileNames.Length; i++)
                    Osu.ConvertOsz(openFileDialog.FileNames[i], i);

                // When all the maps have been converted, select the last imported map and make that the selected one.
            }).ContinueWith(async t =>
            {
                await MapImportLoadingState.AfterImport();
            });
        }

        /// <summary>
        ///     Called when the user clicks to convert a StepMania (.sm) file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConvertSmButtonClick(object sender, EventArgs e)
        {
            // Create the openFileDialog object.
            var openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = "c:\\",
                Filter = "StepMania File (*.sm)|*.sm",
                FilterIndex = 0,
                RestoreDirectory = true,
                Multiselect = true
            };

            // If the dialog couldn't be shown, that's an issue, so we'll return for now.
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            // Proceed to extract and convert the map, show loading screen.
            GameBase.GameStateManager.AddState(new MapImportLoadingState());

            // Run the converter for all file names
            Task.Run(() =>
            {
                foreach (var t in openFileDialog.FileNames)
                    StepManiaConverter.ConvertSm(t);

                // When all the maps have been converted, select the last imported map and make that the selected one.
            }).ContinueWith(async t =>
            {
                await MapImportLoadingState.AfterImport();
            });
        }
    }
}
