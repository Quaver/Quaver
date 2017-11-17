using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ManagedBass;
using Ionic.Zip;
using Microsoft.Xna.Framework;
using Quaver.Beatmaps;
using Quaver.Discord;
using Quaver.GameState;
using Quaver.GameState.States;
using Quaver.Graphics.Sprite;
using Quaver.Logging;

namespace Quaver.Peppy
{
    internal class Osz
    {
        /// <summary>
        ///     Upon clicking the import button, we'll want to display an open file dialog,
        ///     then attempt to convert all 4k & 7k beatmaps and create a new song directory
        ///     for the import queue.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnImportButtonClick(object sender, EventArgs e)
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
                // Update the selected beatmap with the new one.
                // This button should only be on the song select state, so no need to check for states here.
                var oldMaps = GameBase.Beatmaps;

                // Load the beatmaps again automatically.
                await GameBase.LoadAndSetBeatmaps();

                var newMap = GameBase.Beatmaps.Where(x => !oldMaps.ContainsKey(x.Key))
                    .ToDictionary(x => x.Key, x => x.Value);

                // In the event that the user imports maps when there weren't any maps previously.
               if (oldMaps.Count == 0)
                {
                    BeatmapUtils.SelectRandomBeatmap();

                    if (GameBase.SelectedBeatmap != null)
                    {
                        GameBase.SelectedBeatmap.LoadAudio();

                        if (GameBase.SelectedBeatmap.Song != null)
                            GameBase.SelectedBeatmap.Song.Play();
                    }
                }
                else if (newMap.Count > 0)
                {
                    var map = newMap.Values.Last().Last();

                    // Stop the currently selected beatmap's song.
                    if (GameBase.SelectedBeatmap != null)
                        GameBase.SelectedBeatmap.Song.Stop();

                    // Switch map and load audio for song and play it.
                    GameBase.ChangeBeatmap(map);

                    // Load and change background after import
                    GameBase.SelectedBeatmap.LoadBackground();
                    BackgroundManager.Change(GameBase.SelectedBeatmap.Background);

                    if (GameBase.SelectedBeatmap.Song != null)
                    {
                        GameBase.SelectedBeatmap.Song.Play();
                        // Set Rich Presence
                        GameBase.DiscordController.presence.details =
                            $"In the main menu listening to: {GameBase.SelectedBeatmap.Artist} - {GameBase.SelectedBeatmap.Title}";
                        DiscordRPC.UpdatePresence(ref GameBase.DiscordController.presence);
                    }
                }

                Logger.Log("Successfully completed the conversion task. Stopping loader.", Color.Cyan);
                GameBase.GameStateManager.RemoveState();
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
                        return;

                    // Convert the map to .qua
                    map.ConvertToQua();
                }

                // Now that all of them are converted, we'll create a new directory with all of the files except for .osu

                var newSongDir = $"{Config.Configuration.SongDirectory}/{new DirectoryInfo(fileName).Name}";

                if (newSongDir.Length > 200)
                    newSongDir = $"{Config.Configuration.SongDirectory}/{new DirectoryInfo(fileName).Name.Substring(0, 20)}";

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
                        default:
                            File.Move(filesInDir[i], $"{newSongDir}/{Path.GetFileName(filesInDir[i])}");
                            break;
                    }               
                }
                
                // Delete the entire temp directory.
                Directory.Delete(extractPath, true);
                Logger.Log($".osz has been successfully converted.", Color.Cyan, 2f);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Log($"Error: There was an issue converting the .osz", Color.Red, 2f);
                Logger.Log(e.Message, Color.Red);
            }
        }
    }
}
