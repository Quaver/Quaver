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
using Quaver.Beatmaps;
using Quaver.GameState.States;
using Quaver.Graphics.Sprite;
using Quaver.Logging;

namespace Quaver.QuaFile
{
    internal class Qum
    {
        /// <summary>
        ///     Upon clicking the button to import a .qum archive, this will handle
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
                Filter = "Quaver Mapset (*.qum)|*.qum",
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
                    ImportQum(fileName);

            // When all the maps have been converted, select the last imported map and make that the selected one.
            }).ContinueWith(async t =>
            {
                await MapImportLoadingState.AfterImport();
            });
        }

        /// <summary>
        ///     Responsible for extracting the files from the .qum 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="num"></param>
        internal static void ImportQum(string fileName)
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
    }
}
