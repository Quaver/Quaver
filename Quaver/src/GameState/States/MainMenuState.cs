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
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;
using Quaver.Logging;
using Quaver.Modifiers;
using Button = Quaver.Graphics.Button.Button;
using Quaver.API.Maps;
using Quaver.Commands;
using Quaver.Peppy;
using Quaver.Steam;
using Quaver.Utility;

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
        ///     Boundary
        /// </summary>
        public Boundary Boundary { get; set; }

        /// <summary>
        ///     Button to switch to the song select state
        /// </summary>
        public Button SwitchSongSelectButton { get; set; }

        /// <summary>
        ///     Button to switch to the options menu
        /// </summary>
        public Button OptionsMenuButton { get; set; }

        /// <summary>
        ///     Button to import .qp
        /// </summary>
        public Button ImportQpButton { get; set; }

        /// <summary>
        ///     Button to export .qp
        /// </summary>
        public Button ExportQpButton { get; set; }

        /// <summary>
        ///     Button to import .osz
        /// </summary>
        public Button ImportPeppyButton { get; set; }

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
            Boundary = new Boundary();

            // Initialize the UI buttons
            CreateOszImportButton();
            CreateQpImportButton();
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
            SwitchSongSelectButton.Clicked -= OnSongSelectButtonClick;
            OptionsMenuButton.Clicked -= OnOptionsSelectButtonClick;
            Boundary.Destroy();
        }

        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(double dt)
        {
            // Play Random Maps during the main menu
            //MenuAudioPlayer.PlayRandomBeatmaps();
            //Console.WriteLine(SwitchSongSelectButton.GlobalRectangle.X + ", " + SwitchSongSelectButton.GlobalRectangle.Y + ", " + SwitchSongSelectButton.GlobalRectangle.Width + ", " + SwitchSongSelectButton.GlobalRectangle.Height);

            //Update Menu Screen Boundary
            Boundary.Update(dt);
        }
        
        /// <summary>
        ///     Draw
        /// </summary>
        public void Draw()
        {
            GameBase.SpriteBatch.Begin();
            //BackgroundManager.Draw();
            Boundary.Draw();
            GameBase.SpriteBatch.End();
        }

        /// <summary>
        ///     Responsible for creating the button to move to the song select screen state
        /// </summary>
        private void CreateMenuButtons()
        {
            // Switch Song Select Button
            SwitchSongSelectButton = new TextButton(new Vector2(200, 40), "Song Select")
            {
                Alignment = Alignment.MidCenter,
                Parent = Boundary
            };
            SwitchSongSelectButton.Clicked += OnSongSelectButtonClick;

            OptionsMenuButton = new TextButton(new Vector2(200, 40), "Options")
            {
                Alignment = Alignment.MidCenter,
                PosY = 50,
                Parent = Boundary
            };
            OptionsMenuButton.Clicked += OnOptionsSelectButtonClick;
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
            // Import .osz Button
            ImportPeppyButton = new TextButton(new Vector2(200, 40), "Import .osz")
            {
                Alignment = Alignment.TopCenter,
                Parent = Boundary
            };

            ImportPeppyButton.Clicked += OnImportOsuButtonClick;
        }

        /// <summary>
        ///     Responsible for creating the import .qp button
        /// </summary>
        private void CreateQpImportButton()
        {
            // Import .osz Button
            ImportQpButton = new TextButton(new Vector2(200, 40), "Import Quaver Mapset")
            {
                Alignment = Alignment.BotCenter,
                Parent = Boundary
            };

            ImportQpButton.Clicked += OnImportQpButtonClick;
        }

        /// <summary>
        ///     Responsible for creating the import .qp button
        /// </summary>
        private void CreateQpExportButton()
        {
            // Import .osz Button
            ExportQpButton = new TextButton(new Vector2(200, 40), "Export Current Mapset")
            {
                Alignment = Alignment.BotRight,
                Parent = Boundary
            };

            ExportQpButton.Clicked += OnExportButtonClick;
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
            var extractPath = $@"{Config.Configuration.SongDirectory}/{Path.GetFileNameWithoutExtension(fileName)}/";

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
            var dirInfo = new DirectoryInfo(Configuration.SongDirectory + "/" + GameBase.SelectedBeatmap.Directory + "/");
            var files = dirInfo.GetFiles();

            foreach (var file in files)
                zip.AddFile(Configuration.SongDirectory + "/" + GameBase.SelectedBeatmap.Directory + "/" + file, "");

            // Create the Data/Maps directory if it doesn't exist already.
            Directory.CreateDirectory($"{Configuration.DataDirectory}/Maps/");

            // Save the file
            var outputPath = $"{Configuration.DataDirectory}/Maps/{GameBase.GameTime.ElapsedMilliseconds} {Util.FileNameSafeString(GameBase.SelectedBeatmap.Artist)} - {Util.FileNameSafeString(GameBase.SelectedBeatmap.Title)}.qp";
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
    }
}
