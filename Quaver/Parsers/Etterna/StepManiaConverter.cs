using System;
using System.IO;
using Quaver.API.Maps.Parsers;
using Quaver.Config;
using Quaver.Helpers;
using Wobble;
using Wobble.Logging;

namespace Quaver.Parsers.Etterna
{
    public static class StepManiaConverter
    {
        /// <summary>
        ///     Converts a .sm file to Quaver's format.
        /// </summary>
        internal static void ConvertSm(string path)
        {
            try
            {

                var quaverMaps = StepManiaFile.Parse(path).ToQua();

                // Create a new directory in the songs folder
                var quaverDir = $"{ConfigManager.SongDirectory}/StepMania - {new DirectoryInfo(path).Name} - {GameBase.Game.TimeRunning}/";
                Directory.CreateDirectory(quaverDir);

                foreach (var map in quaverMaps)
                    map.Save($"{quaverDir}/{StringHelper.FileNameSafeString(map.Artist)} - {StringHelper.FileNameSafeString(map.Title)} [{StringHelper.FileNameSafeString(map.DifficultyName)}].qua");

                // Now copy over the background + audio file
                try
                {
                    File.Copy(Path.GetDirectoryName(path) + "/" + quaverMaps[0].AudioFile, quaverDir + "/" + quaverMaps[0].AudioFile);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }

                try
                {
                    File.Copy(Path.GetDirectoryName(path) + "/" + quaverMaps[0].BackgroundFile, quaverDir + "/" + quaverMaps[0].BackgroundFile);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }

                Logger.Important("StepMania file has been successfully converted!", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }
    }
}