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

            // Initialize the main menu's audio player.
            MenuAudioPlayer.Initialize();

            //Initialize Menu Screen
            Boundary = new Boundary();

            // Initialize the UI buttons
            CreateOszImportButton();
            CreateQpImportButton();
            CreateSongSelectButton();
            CreateQpExportButton();

            UpdateReady = true;
        }

        /// <summary>
        ///     Unload
        /// </summary>
        public void UnloadContent()
        {
            UpdateReady = false;
            Boundary.Destroy();
        }

        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(double dt)
        {
            // Play Random Maps during the main menu
            MenuAudioPlayer.PlayRandomBeatmaps();
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
            BackgroundManager.Draw();
            SwitchSongSelectButton.Draw();
            ImportPeppyButton.Draw();
            ImportQpButton.Draw();
            ExportQpButton.Draw();
            GameBase.SpriteBatch.End();
        }

        /// <summary>
        ///     Responsible for creating the button to move to the song select screen state
        /// </summary>
        private void CreateSongSelectButton()
        {
            // Switch Song Select Button
            SwitchSongSelectButton = new TextButton(new Vector2(200, 40), "Song Select")
            {
                Alignment = Alignment.MidCenter,
                Parent = Boundary
            };

            SwitchSongSelectButton.Clicked += OnSongSelectButtonClick;
        }

        /// <summary>
        ///     The event handler that switches to the song select screen
        /// </summary>
        public void OnSongSelectButtonClick(object sender, EventArgs e)
        {
            //Change to SongSelectState
            GameBase.LoadedSkin.Click.Play((float)Configuration.VolumeGlobal / 100 * Configuration.VolumeEffect / 100, 0, 0);
            GameBase.GameStateManager.ChangeState(new SongSelectState());
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
                Logger.Log(e.Message, LogColors.GameError);
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

            Logger.Log($"Successfully exported {outputPath}", LogColors.GameSuccess);

            // Open the folder where the file is contained.
            if (!File.Exists(outputPath))
                return;

            // TODO: Fix for linux/mac.
            try
            {
                Console.WriteLine(outputPath);
                System.Diagnostics.Process.Start("explorer.exe", "/select," + "\"" + $@"{outputPath.Replace("/", "\\")}" + "\"");
            }
            catch (Exception ex) { }
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
                    ConvertOsz(openFileDialog.FileNames[i], i);

                // When all the maps have been converted, select the last imported map and make that the selected one.
            }).ContinueWith(async t =>
            {
                await MapImportLoadingState.AfterImport();
            });
        }

        /// <summary>
        ///     Responsible for converting a .osz file to a new song directory full of .qua
        /// </summary>
        /// <param name="fileName"></param>
        private static void ConvertOsz(string fileName, int num)
        {
            // Extract the .osu & relevant audio files, and attempt to convert them.
            // Once fully converted, create a new directory in the songs folder and 
            // tell GameBase that the import queue is ready. Depending on the current state,
            // we may import them automatically.
            var extractPath = $@"{Config.Configuration.DataDirectory}/Temp/{num}";

            try
            {
                using (var archive = new ZipFile(fileName))
                {
                    archive.ExtractAll(extractPath, ExtractExistingFileAction.OverwriteSilently);
                }

                // Now that we have them, proceed to convert them.
                foreach (var file in Directory.GetFiles(extractPath, "*.osu", SearchOption.AllDirectories))
                {
                    var map = new PeppyBeatmap(file);

                    if (!map.IsValid)
                        continue;

                    // Convert the map to .qua
                    var qua = Qua.ConvertOsuBeatmap(map);
                    qua.Save(map.OriginalFileName.Replace(".osu", ".qua"));
                }

                // Now that all of them are converted, we'll create a new directory with all of the files except for .osu

                var newSongDir = $"{Config.Configuration.SongDirectory}/{new DirectoryInfo(fileName).Name}";

                if (newSongDir.Length > 200)
                    newSongDir =
                        $"{Config.Configuration.SongDirectory}/{new DirectoryInfo(fileName).Name.Substring(0, 20)}";

                Directory.CreateDirectory(newSongDir);

                // Get the files that are currently in the extract path
                var filesInDir = Directory.GetFiles(extractPath);

                for (var i = 0; i < filesInDir.Length; i++)
                {
                    switch (Path.GetExtension(filesInDir[i]))
                    {
                        case ".osu":
                            // Ignore .osu files
                            continue;
                        case ".qua":
                            // Try to create a similar path to the original. 
                            // The reason we generate all these new file names is because
                            // the path may end up being too long, and that throws an error.
                            var newFile = $"{newSongDir}/{Path.GetFileName(filesInDir[i])}";

                            if (newFile.Length > 200)
                                newFile = $"{newSongDir}/{Path.GetFileName(filesInDir[i]).Substring(0, 60)}.qua";

                            if (newFile.Length > 200 || File.Exists(newFile))
                                newFile = $"{newSongDir}/{i}.qua";

                            File.Move(filesInDir[i], newFile);
                            break;
                        // We only allow certain file to be moved over, no .wav files, as those are usually
                        // hitsounds from osu!
                        case ".mp3":
                        case ".jpg":
                        case ".png":
                        case ".jpeg":
                        case ".ogg":
                            File.Move(filesInDir[i], $"{newSongDir}/{Path.GetFileName(filesInDir[i])}");
                            break;
                    }
                }

                Logger.Log($".osz has been successfully converted.", LogColors.GameSuccess, 2f);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Log($"Error: There was an issue converting the .osz", LogColors.GameError, 2f);
                Logger.Log(e.Message, Color.Red);
            }
            // Delete the entire temp directory regardless of the outcome.
            finally
            {
                Directory.Delete(extractPath, true);
            }
        }
    }
}
