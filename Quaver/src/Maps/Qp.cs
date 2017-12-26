using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ionic.Zip;
using Microsoft.Xna.Framework;
using Quaver.Audio;
using Quaver.Database.Beatmaps;
using Quaver.Config;
using Quaver.GameState.States;
using Quaver.Graphics.Sprite;
using Quaver.Logging;
using Quaver.Utility;

namespace Quaver.Maps
{
    internal class Qp
    {
        /// <summary>
        ///     Upon clicking the button to import a .qp archive, this will handle
        ///     the dialog portion of it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void OnImportButtonClick(object sender, EventArgs e)
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
        internal static void ImportQp(string fileName)
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
                Logger.Log(e.Message, Color.Red);
            }
        }

        /// <summary>
        ///     Responsible for zipping the selected mapset
        /// </summary>
        internal static void OnExportButtonClick(object sender, EventArgs e)
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

            Logger.Log($"Successfully exported {outputPath}", Color.Cyan);

            // Open the folder where the file is contained.
            if (!File.Exists(outputPath))
                return;

            // TODO: Fix for linux/mac.
            try
            {
                Console.WriteLine(outputPath);
                System.Diagnostics.Process.Start("explorer.exe", "/select," + "\"" + $@"{outputPath.Replace("/", "\\")}" + "\"");
            } catch (Exception ex) { }
        }
    }
}
