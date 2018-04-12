using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using Microsoft.Xna.Framework;
using NAudio.Wave;
using Quaver.API.Maps;
using Quaver.API.Osu;
using Quaver.Config;
using Quaver.Logging;

namespace Quaver.Peppy
{
    internal class Osu
    {
        // Contains the list of osu! skin elements that match up to Quaver
        private static readonly QuaverOsuElementMap[] QuaverOsuSkinMap = new QuaverOsuElementMap[]
        {
            // Grades
            new QuaverOsuElementMap("grade-small-a", "ranking-a", ElementType.Image),
            new QuaverOsuElementMap("grade-small-b", "ranking-b", ElementType.Image),
            new QuaverOsuElementMap("grade-small-c", "ranking-c", ElementType.Image),
            new QuaverOsuElementMap("grade-small-d", "ranking-d", ElementType.Image),
            new QuaverOsuElementMap("grade-small-f", "ranking-d", ElementType.Image), // D
            new QuaverOsuElementMap("grade-small-s", "ranking-s", ElementType.Image),
            new QuaverOsuElementMap("grade-small-ss", "ranking-sh", ElementType.Image),
            new QuaverOsuElementMap("grade-small-x", "ranking-x", ElementType.Image),
            new QuaverOsuElementMap("grade-small-xx", "ranking-xh", ElementType.Image),

            // 4k HitObjects
            new QuaverOsuElementMap("4k-note-hitobject-1", "mania-note1", ElementType.Image, "NoteImage0"),
            new QuaverOsuElementMap("4k-note-hitobject-2", "mania-note2", ElementType.Image, "NoteImage1"),
            new QuaverOsuElementMap("4k-note-hitobject-3", "mania-note2", ElementType.Image, "NoteImage2"),
            new QuaverOsuElementMap("4k-note-hitobject-4", "mania-note1", ElementType.Image, "NoteImage3"),

            // 7k HitObjects
            new QuaverOsuElementMap("7k-note-hitobject-1", "mania-note1", ElementType.Image, "NoteImage0"),
            new QuaverOsuElementMap("7k-note-hitobject-2", "mania-note2", ElementType.Image, "NoteImage1"),
            new QuaverOsuElementMap("7k-note-hitobject-3", "mania-note1", ElementType.Image, "NoteImage2"),
            new QuaverOsuElementMap("7k-note-hitobject-4", "mania-noteS", ElementType.Image, "NoteImage3"),
            new QuaverOsuElementMap("7k-note-hitobject-5", "mania-note1", ElementType.Image, "NoteImage4"),
            new QuaverOsuElementMap("7k-note-hitobject-6", "mania-note2", ElementType.Image, "NoteImage5"),
            new QuaverOsuElementMap("7k-note-hitobject-7", "mania-note1", ElementType.Image, "NoteImage6"),

            // 4k ManiaHitObject Hold Heads
            new QuaverOsuElementMap("4k-note-holdhitobject-1", "mania-note1H", ElementType.Image, "NoteImage0H"),
            new QuaverOsuElementMap("4k-note-holdhitobject-2", "mania-note2H", ElementType.Image, "NoteImage1H"),
            new QuaverOsuElementMap("4k-note-holdhitobject-3", "mania-note2H", ElementType.Image, "NoteImage2H"),
            new QuaverOsuElementMap("4k-note-holdhitobject-4", "mania-note1H", ElementType.Image, "NoteImage3H"),

            // 7k ManiaHitObject Hold Heads
            new QuaverOsuElementMap("7k-note-holdhitobject-1", "mania-note1H", ElementType.Image, "NoteImage0H"),
            new QuaverOsuElementMap("7k-note-holdhitobject-2", "mania-note2H", ElementType.Image, "NoteImage1H"),
            new QuaverOsuElementMap("7k-note-holdhitobject-3", "mania-note1H", ElementType.Image, "NoteImage2H"),
            new QuaverOsuElementMap("7k-note-holdhitobject-4", "mania-noteSH", ElementType.Image, "NoteImage3H"),
            new QuaverOsuElementMap("7k-note-holdhitobject-5", "mania-note1H", ElementType.Image, "NoteImage4H"),
            new QuaverOsuElementMap("7k-note-holdhitobject-6", "mania-note2H", ElementType.Image, "NoteImage5H"),
            new QuaverOsuElementMap("7k-note-holdhitobject-7", "mania-note1H", ElementType.Image, "NoteImage6H"),

            // 4k Hit Object Hold Ends
            new QuaverOsuElementMap("4k-note-holdend-1", "mania-note1T", ElementType.Image, "NoteImage0T"),
            new QuaverOsuElementMap("4k-note-holdend-2", "mania-note2T", ElementType.Image, "NoteImage1T"),
            new QuaverOsuElementMap("4k-note-holdend-3", "mania-note2T", ElementType.Image, "NoteImage2T"),
            new QuaverOsuElementMap("4k-note-holdend-4", "mania-note1T", ElementType.Image, "NoteImage3T"),

            // 7k Hit Object Hold Ends
            new QuaverOsuElementMap("7k-note-holdend-1", "mania-note1T", ElementType.Image, "NoteImage0T"),
            new QuaverOsuElementMap("7k-note-holdend-2", "mania-note2T", ElementType.Image, "NoteImage1T"),
            new QuaverOsuElementMap("7k-note-holdend-3", "mania-note1T", ElementType.Image, "NoteImage2T"),
            new QuaverOsuElementMap("7k-note-holdend-4", "mania-noteST", ElementType.Image, "NoteImage3T"),
            new QuaverOsuElementMap("7k-note-holdend-5", "mania-note1T", ElementType.Image, "NoteImage4T"),
            new QuaverOsuElementMap("7k-note-holdend-6", "mania-note2T", ElementType.Image, "NoteImage5T"),
            new QuaverOsuElementMap("7k-note-holdend-7", "mania-note1T", ElementType.Image, "NoteImage6T"),

            // 4k Hit Object Hold Bodies
            // The reason why there are two, is because osu takes either:
            //  - mania-note1L (Non Animated)
            //  - mania-note1L-0 (Animated)
            new QuaverOsuElementMap("4k-note-holdbody-1", "mania-note1L", ElementType.Image, "NoteImage0L"),
            new QuaverOsuElementMap("4k-note-holdbody-2", "mania-note2L", ElementType.Image, "NoteImage1L"),
            new QuaverOsuElementMap("4k-note-holdbody-3", "mania-note2L", ElementType.Image, "NoteImage2L"),
            new QuaverOsuElementMap("4k-note-holdbody-4", "mania-note1L", ElementType.Image, "NoteImage3L"),

            new QuaverOsuElementMap("4k-note-holdbody-1", "mania-note1L", ElementType.AnimatableImage),
            new QuaverOsuElementMap("4k-note-holdbody-2", "mania-note2L", ElementType.AnimatableImage),
            new QuaverOsuElementMap("4k-note-holdbody-3", "mania-note2L", ElementType.AnimatableImage),
            new QuaverOsuElementMap("4k-note-holdbody-4", "mania-note1L", ElementType.AnimatableImage),

            // 7k Hit Object Hold Bodies
            // The reason why there are two, is because osu takes either:
            //  - mania-note1L (Non Animated)
            //  - mania-note1L-0 (Animated)
            new QuaverOsuElementMap("7k-note-holdbody-1", "mania-note1L", ElementType.Image, "NoteImage0L"),
            new QuaverOsuElementMap("7k-note-holdbody-2", "mania-note2L", ElementType.Image, "NoteImage1L"),
            new QuaverOsuElementMap("7k-note-holdbody-3", "mania-note1L", ElementType.Image, "NoteImage2L"),
            new QuaverOsuElementMap("7k-note-holdbody-4", "mania-noteSL", ElementType.Image, "NoteImage3L"),
            new QuaverOsuElementMap("7k-note-holdbody-5", "mania-note1L", ElementType.Image, "NoteImage4L"),
            new QuaverOsuElementMap("7k-note-holdbody-6", "mania-note2L", ElementType.Image, "NoteImage5L"),
            new QuaverOsuElementMap("7k-note-holdbody-7", "mania-note1L", ElementType.Image, "NoteImage6L"),

            new QuaverOsuElementMap("7k-note-holdbody-1", "mania-note1L", ElementType.AnimatableImage),
            new QuaverOsuElementMap("7k-note-holdbody-2", "mania-note2L", ElementType.AnimatableImage),
            new QuaverOsuElementMap("7k-note-holdbody-3", "mania-note1L", ElementType.AnimatableImage),
            new QuaverOsuElementMap("7k-note-holdbody-4", "mania-noteSL", ElementType.AnimatableImage),
            new QuaverOsuElementMap("7k-note-holdbody-5", "mania-note1L", ElementType.AnimatableImage),
            new QuaverOsuElementMap("7k-note-holdbody-6", "mania-note2L", ElementType.AnimatableImage),
            new QuaverOsuElementMap("7k-note-holdbody-7", "mania-note1L", ElementType.AnimatableImage),

            // 4k Note Receptors
            new QuaverOsuElementMap("4k-receptor-up-1", "mania-key1", ElementType.Image, "KeyImage0"),
            new QuaverOsuElementMap("4k-receptor-up-2", "mania-key2", ElementType.Image, "KeyImage1"),
            new QuaverOsuElementMap("4k-receptor-up-3", "mania-key2", ElementType.Image, "KeyImage2"),
            new QuaverOsuElementMap("4k-receptor-up-4", "mania-key1", ElementType.Image, "KeyImage3"),

            // 4k Note Receptors Down
            new QuaverOsuElementMap("4k-receptor-down-1", "mania-key1D", ElementType.Image, "KeyImage0D"),
            new QuaverOsuElementMap("4k-receptor-down-2", "mania-key2D", ElementType.Image, "KeyImage1D"),
            new QuaverOsuElementMap("4k-receptor-down-3", "mania-key2D", ElementType.Image, "KeyImage2D"),
            new QuaverOsuElementMap("4k-receptor-down-4", "mania-key1D", ElementType.Image, "KeyImage3D"),

            // 7k Note Receptors
            new QuaverOsuElementMap("7k-receptor-up-1", "mania-key1", ElementType.Image, "KeyImage0"),
            new QuaverOsuElementMap("7k-receptor-up-2", "mania-key2", ElementType.Image, "KeyImage1"),
            new QuaverOsuElementMap("7k-receptor-up-3", "mania-key1", ElementType.Image, "KeyImage2"),
            new QuaverOsuElementMap("7k-receptor-up-4", "mania-keyS", ElementType.Image, "KeyImage3"),
            new QuaverOsuElementMap("7k-receptor-up-5", "mania-key1", ElementType.Image, "KeyImage4"),
            new QuaverOsuElementMap("7k-receptor-up-6", "mania-key2", ElementType.Image, "KeyImage5"),
            new QuaverOsuElementMap("7k-receptor-up-7", "mania-key1", ElementType.Image, "KeyImage6"),

            // 7k Note Receptors Down
            new QuaverOsuElementMap("7k-receptor-down-1", "mania-key1D", ElementType.Image, "KeyImage0D"),
            new QuaverOsuElementMap("7k-receptor-down-2", "mania-key2D", ElementType.Image, "KeyImage1D"),
            new QuaverOsuElementMap("7k-receptor-down-3", "mania-key1D", ElementType.Image, "KeyImage2D"),
            new QuaverOsuElementMap("7k-receptor-down-4", "mania-keySD", ElementType.Image, "KeyImage3D"),
            new QuaverOsuElementMap("7k-receptor-down-5", "mania-key1D", ElementType.Image, "KeyImage4D"),
            new QuaverOsuElementMap("7k-receptor-down-6", "mania-key2D", ElementType.Image, "KeyImage5D"),
            new QuaverOsuElementMap("7k-receptor-down-7", "mania-key1D", ElementType.Image, "KeyImage6D"),

            // Judge
            // The reason why there are two, is because osu takes either:
            //  - mania-hit0 (Non Animated)
            //  - mania-hit0-0 (Animated)
            new QuaverOsuElementMap("judge-miss", "mania-hit0", ElementType.Image),
            new QuaverOsuElementMap("judge-bad", "mania-hit50", ElementType.Image),
            new QuaverOsuElementMap("judge-good", "mania-hit100", ElementType.Image),
            new QuaverOsuElementMap("judge-great", "mania-hit200", ElementType.Image),
            new QuaverOsuElementMap("judge-perfect", "mania-hit300", ElementType.Image),
            new QuaverOsuElementMap("judge-marv", "mania-hit300g", ElementType.Image),

            new QuaverOsuElementMap("judge-miss", "mania-hit0", ElementType.AnimatableImage),
            new QuaverOsuElementMap("judge-bad", "mania-hit50", ElementType.AnimatableImage),
            new QuaverOsuElementMap("judge-good", "mania-hit100", ElementType.AnimatableImage),
            new QuaverOsuElementMap("judge-great", "mania-hit200", ElementType.AnimatableImage),
            new QuaverOsuElementMap("judge-perfect", "mania-hit300", ElementType.AnimatableImage),
            new QuaverOsuElementMap("judge-marv", "mania-hit300g", ElementType.AnimatableImage),

            // Mania Stage
            new QuaverOsuElementMap("stage-left-border", "mania-stage-left", ElementType.Image),
            new QuaverOsuElementMap("stage-right-border", "mania-stage-right", ElementType.Image),
            new QuaverOsuElementMap("stage-hitposition-overlay", "mania-stage-hint", ElementType.Image),
            
            // Lighting
            // TODO: This should be animatable and have an unlimited amount of elements
            //  new QuaverOsuElementMap("4k-note-hiteffect-1", "lightingN-0", ElementType.AnimatableImage), 
            //  new QuaverOsuElementMap("4k-column-lighting", "mania-stage-light-0", ElementType.AnimatableImage)

            //  QuaverCursor
            new QuaverOsuElementMap("main-cursor", "cursor", ElementType.Image),

            // Sound Effects
            new QuaverOsuElementMap("sound-hit", "normal-hitnormal", ElementType.Sound),
            new QuaverOsuElementMap("sound-hitclap", "normal-hitclap", ElementType.Sound),
            new QuaverOsuElementMap("sound-hitwhistle", "normal-hitwhistle", ElementType.Sound),
            new QuaverOsuElementMap("sound-hitfinish", "normal-hitfinish", ElementType.Sound),
            new QuaverOsuElementMap("sound-combobreak", "combobreak", ElementType.Sound),
            new QuaverOsuElementMap("sound-applause", "applause", ElementType.Sound),
            new QuaverOsuElementMap("sound-screenshot", "", ElementType.Sound),
            new QuaverOsuElementMap("sound-click", "menuhit", ElementType.Sound),
            new QuaverOsuElementMap("sound-back", "menuback", ElementType.Sound)
        };

        /// <summary>
        ///     Responsible for converting a .osz file to a new song directory full of .qua
        /// </summary>
        /// <param name="fileName"></param>
        internal static void ConvertOsz(string fileName, int num)
        {
            // Extract the .osu & relevant audio files, and attempt to convert them.
            // Once fully converted, create a new directory in the songs folder and 
            // tell GameBase that the import queue is ready. Depending on the current state,
            // we may import them automatically.
            var extractPath = $@"{ConfigManager.DataDirectory}/Temp/{num}";

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

                var newSongDir = $"{ConfigManager.SongDirectory}/{new DirectoryInfo(fileName).Name}";

                if (newSongDir.Length > 200)
                    newSongDir =
                        $"{ConfigManager.SongDirectory}/{new DirectoryInfo(fileName).Name.Substring(0, 20)}";

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
            }
            catch (Exception e)
            {
                Logger.LogError($"Error: There was an issue converting the .osz", LogType.Runtime, 3f);
                Logger.LogError(e, LogType.Runtime);
            }
            // Delete the entire temp directory regardless of the outcome.
            finally
            {
                Directory.Delete(extractPath, true);
            }
        }

        /// <summary>
        ///     Converts an osu! skin (.osk) file to Quaver
        /// </summary>
        internal static void ConvertOsk(string path)
        {
            try
            {
                Logger.LogImportant("Commencing .osk conversion. Please standby...", LogType.Runtime);

                if (!File.Exists(path))
                    throw new FileNotFoundException();

                // Set and create the temporary extraction path
                var extractPath = $@"{ConfigManager.DataDirectory}/Temp/{Path.GetFileNameWithoutExtension(path)}/";
                Directory.CreateDirectory(extractPath);

                using (var archive = new ZipFile(path))
                    archive.ExtractAll(extractPath, ExtractExistingFileAction.OverwriteSilently);

                // Create the new directory for the Quaver skin.
                var newSkinDirPath = ConfigManager.SkinDirectory + "/" + Path.GetFileNameWithoutExtension(path);
                Directory.CreateDirectory(newSkinDirPath);

                // Begin copying skin elements over to the new directory.
                foreach (var map in QuaverOsuSkinMap)
                {
                    // The extension for the file we'll look for based on the element type
                    var extension = (map.Type == ElementType.Image || map.Type == ElementType.AnimatableImage) ? ".png" : ".wav";

                    // The full path of the osu! skin file
                    var fullPath = extractPath + map.OsuElement.ToLower() + extension;

                    // The base path of the new Quaver skin file.
                    var newPath = newSkinDirPath + "/" + map.QuaverElement + extension;

                    // Copy skin elements over to the new directory
                    switch (map.Type)
                    {
                        // .wav audio files
                        case ElementType.Sound:
                            if (!File.Exists(fullPath)) continue;

                            // Convert .wav files to 16-bit
                            try
                            {
                                if (map.Type == ElementType.Sound)
                                {
                                    using (var reader = new WaveFileReader(fullPath))
                                    {
                                        var newFormat = new WaveFormat();
                                        using (var conversionStream = new WaveFormatConversionStream(newFormat, reader))
                                        {
                                            WaveFileWriter.CreateWaveFile(newPath, conversionStream);
                                        }
                                    }
                                }
                            }
                            catch (Exception e) { Logger.LogError(e, LogType.Runtime); }
                            break;
                        case ElementType.Image:
                            if (!File.Exists(fullPath)) continue;
                            File.Copy(fullPath, newPath, true);
                            break;
                        case ElementType.AnimatableImage:
                            // Copy animation elements over until the sequence is broken
                            for (var i = 0; File.Exists(extractPath + map.OsuElement.ToLower() + "-" + i + extension); i++)
                            {
                                if (i == 0)
                                    // element (It's just the element name if it's the first animation frame.)
                                    newPath = newSkinDirPath + "/" + map.QuaverElement + extension;
                                else
                                    // element@1 (It contains @i if the element frame isn't the first one.)
                                    newPath = newSkinDirPath + "/" + map.QuaverElement + "@" + i + extension;

                                File.Copy(extractPath + map.OsuElement.ToLower() + "-" + i + extension, newPath, true);
                            }
                            break;
                        default:
                            continue;
                    }
                }

                // Lastly parse the skin.ini file 
                var osuIni = new OsuSkinConfig(extractPath + "/skin.ini");

                // Since you can specify the specific image paths in osu!'s skin.ini for notes/receptors,
                // we read and copy them over here.
                foreach (var map in QuaverOsuSkinMap)
                {
                    if (string.IsNullOrWhiteSpace(map.SkinIniValue))
                        continue;


                    try
                    {
                        if (map.QuaverElement.Contains("4k"))
                        {
                            var val = osuIni.Keys4Config.GetType().GetField(map.SkinIniValue).GetValue(osuIni.Keys4Config).ToString();
                            val = val.Replace(".png", "");

                            var elementPath = $"{extractPath}/{val}.png";
                            File.Copy(elementPath, newSkinDirPath + "/" + map.QuaverElement + ".png", true);
                        }
                        else if (map.QuaverElement.Contains("7k"))
                        {
                            var val = osuIni.Keys7Config.GetType().GetField(map.SkinIniValue).GetValue(osuIni.Keys7Config).ToString();
                            val = val.Replace(".png", "");

                            var elementPath = $"{extractPath}/{val}.png";
                            File.Copy(elementPath, newSkinDirPath + "/" + map.QuaverElement + ".png", true);
                        }
                    }
                    catch (Exception e) { }
                }

                // Create the skin.ini file
                CreateSkinIniFile(osuIni, newSkinDirPath + "/skin.ini");
                Logger.LogSuccess("Skin conversion has completed!", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Creates a skin.ini file based on an osu! config
        /// </summary>
        /// <param name="config"></param>
        /// <param name="path"></param>
        private static void CreateSkinIniFile(OsuSkinConfig config, string path)
        {
            using (var file = new StreamWriter(path))
            {
                // [General]
                file.WriteLine("[General]");
                file.WriteLine($"Name = {config.Name}");
                file.WriteLine($"Author = {config.Author}");

                file.WriteLine("");

                file.WriteLine("[Gameplay]");
                file.WriteLine($"ReceptorsOverObjects4K = {!config.Keys4Config.KeysUnderNotes}");
                file.WriteLine($"ReceptorsOverObjects7K = {!config.Keys7Config.KeysUnderNotes}");
                file.WriteLine($"ColourObjectsBySnapDistance = false");
                //for (var i = 0; i < 4; i++) file.WriteLine($"ColumnColor4K{i} = {GraphicsHelper.ColorToString(config.Keys4Config.Colours[i])}");
                //for (var i = 0; i < 7; i++) file.WriteLine($"ColumnColor7K{i} = {GraphicsHelper.ColorToString(config.Keys7Config.Colours[i])}");
            }
        }
    }
}
