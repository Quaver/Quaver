using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using NAudio.Wave;
using Quaver.Config;
using Quaver.Logging;

namespace Quaver.Commands
{
    internal class ConvertOsuSkin : ICommand
    {
        public string Name { get; set; } = "OSK";

        public int Args { get; set; } = 2;

        public string Description { get; set; } = "Converts an osu! skin (.osk) to Quaver.";

        public string Usage { get; set; } = "osk <file path>";

        private enum ElementType
        {
            Image,
            AnimatableImage,
            Sound
        }

        private struct QuaverOsuElementMap
        {
            /// <summary>
            ///     The Quaver skin element file name
            /// </summary>
            public string QuaverElement { get; }

            /// <summary>
            ///     The osu! skin element file name
            /// </summary>
            public string OsuElement { get; }

            /// <summary>
            ///     The type of element
            /// </summary>
            public ElementType Type { get; }

            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="quaverElement"></param>
            /// <param name="osuElement"></param>
            /// <param name="type"></param>
            public QuaverOsuElementMap(string quaverElement, string osuElement, ElementType type)
            {
                QuaverElement = quaverElement;
                OsuElement = osuElement;
                Type = type;
            }
        }

        /// <summary>
        ///     Contains a map of Quaver's skin elements to osu!'s skin elements
        ///     List of 
        /// </summary>
        private readonly QuaverOsuElementMap[] QuaverOsuSkinMap = new QuaverOsuElementMap[]
        {
            // Grades
            new QuaverOsuElementMap("grade-small-a", "ranking-a", ElementType.Image),
            new QuaverOsuElementMap("grade-small-b", "ranking-b", ElementType.Image),
            new QuaverOsuElementMap("grade-small-c", "ranking-c", ElementType.Image),
            new QuaverOsuElementMap("grade-small-d", "ranking-d", ElementType.Image),
            //new QuaverOsuElementMap("grade-small-f", "", ElementType.Image), // Not applicable
            new QuaverOsuElementMap("grade-small-s", "ranking-s", ElementType.Image),
            new QuaverOsuElementMap("grade-small-ss", "ranking-sh", ElementType.Image),
            new QuaverOsuElementMap("grade-small-x", "ranking-x", ElementType.Image),
            new QuaverOsuElementMap("grade-small-xx", "ranking-xh", ElementType.Image),
            //new QuaverOsuElementMap("grade-small-xxx", "", ElementType.Image), // Not applicable

            // 4k HitObjects
            new QuaverOsuElementMap("4k-note-hitobject-1", "mania-note1", ElementType.Image),
            new QuaverOsuElementMap("4k-note-hitobject-2", "mania-note2", ElementType.Image),
            new QuaverOsuElementMap("4k-note-hitobject-3", "mania-note2", ElementType.Image),
            new QuaverOsuElementMap("4k-note-hitobject-4", "mania-note1", ElementType.Image),

            // 7k HitObjects
            new QuaverOsuElementMap("7k-note-hitobject-1", "mania-note1", ElementType.Image),
            new QuaverOsuElementMap("7k-note-hitobject-2", "mania-note2", ElementType.Image),
            new QuaverOsuElementMap("7k-note-hitobject-3", "mania-note1", ElementType.Image),
            new QuaverOsuElementMap("7k-note-hitobject-4", "mania-noteS", ElementType.Image),
            new QuaverOsuElementMap("7k-note-hitobject-5", "mania-note1", ElementType.Image),
            new QuaverOsuElementMap("7k-note-hitobject-6", "mania-note2", ElementType.Image),
            new QuaverOsuElementMap("7k-note-hitobject-7", "mania-note1", ElementType.Image),

            // 4k Hit Object Hold Ends
            new QuaverOsuElementMap("4k-note-holdend-1", "mania-note1T", ElementType.Image), 
            new QuaverOsuElementMap("4k-note-holdend-2", "mania-note2T", ElementType.Image), 
            new QuaverOsuElementMap("4k-note-holdend-3", "mania-note2T", ElementType.Image),
            new QuaverOsuElementMap("4k-note-holdend-4", "mania-note1T", ElementType.Image),

            // 7k Hit Object Hold Ends
            new QuaverOsuElementMap("7k-note-holdend-1", "mania-note1T", ElementType.Image),
            new QuaverOsuElementMap("7k-note-holdend-2", "mania-note2T", ElementType.Image),
            new QuaverOsuElementMap("7k-note-holdend-3", "mania-note1T", ElementType.Image),
            new QuaverOsuElementMap("7k-note-holdend-4", "mania-noteST", ElementType.Image),
            new QuaverOsuElementMap("7k-note-holdend-5", "mania-note1T", ElementType.Image),
            new QuaverOsuElementMap("7k-note-holdend-6", "mania-note2T", ElementType.Image),
            new QuaverOsuElementMap("7k-note-holdend-7", "mania-note1T", ElementType.Image),

            // 4k Hit Object Hold Bodies
            // The reason why there are two, is because osu takes either:
            //  - mania-note1L (Non Animated)
            //  - mania-note1L-0 (Animated)
            new QuaverOsuElementMap("4k-note-holdbody-1", "mania-note1L", ElementType.Image),
            new QuaverOsuElementMap("4k-note-holdbody-2", "mania-note2L", ElementType.Image),
            new QuaverOsuElementMap("4k-note-holdbody-3", "mania-note2L", ElementType.Image),
            new QuaverOsuElementMap("4k-note-holdbody-4", "mania-note1L", ElementType.Image),

            new QuaverOsuElementMap("4k-note-holdbody-1", "mania-note1L", ElementType.AnimatableImage),
            new QuaverOsuElementMap("4k-note-holdbody-2", "mania-note2L", ElementType.AnimatableImage),
            new QuaverOsuElementMap("4k-note-holdbody-3", "mania-note2L", ElementType.AnimatableImage),
            new QuaverOsuElementMap("4k-note-holdbody-4", "mania-note1L", ElementType.AnimatableImage),

            // 7k Hit Object Hold Bodies
            // The reason why there are two, is because osu takes either:
            //  - mania-note1L (Non Animated)
            //  - mania-note1L-0 (Animated)
            new QuaverOsuElementMap("7k-note-holdbody-1", "mania-note1L", ElementType.Image),
            new QuaverOsuElementMap("7k-note-holdbody-2", "mania-note2L", ElementType.Image),
            new QuaverOsuElementMap("7k-note-holdbody-3", "mania-note1L", ElementType.Image),
            new QuaverOsuElementMap("7k-note-holdbody-4", "mania-noteSL", ElementType.Image),
            new QuaverOsuElementMap("7k-note-holdbody-5", "mania-note1L", ElementType.Image),
            new QuaverOsuElementMap("7k-note-holdbody-6", "mania-note2L", ElementType.Image),
            new QuaverOsuElementMap("7k-note-holdbody-7", "mania-note1L", ElementType.Image),

            new QuaverOsuElementMap("7k-note-holdbody-1", "mania-note1L", ElementType.AnimatableImage),
            new QuaverOsuElementMap("7k-note-holdbody-2", "mania-note2L", ElementType.AnimatableImage),
            new QuaverOsuElementMap("7k-note-holdbody-3", "mania-note1L", ElementType.AnimatableImage),
            new QuaverOsuElementMap("7k-note-holdbody-4", "mania-noteSL", ElementType.AnimatableImage),
            new QuaverOsuElementMap("7k-note-holdbody-5", "mania-note1L", ElementType.AnimatableImage),
            new QuaverOsuElementMap("7k-note-holdbody-6", "mania-note2L", ElementType.AnimatableImage),
            new QuaverOsuElementMap("7k-note-holdbody-7", "mania-note1L", ElementType.AnimatableImage),

            // 4k Note Receptors
            new QuaverOsuElementMap("4k-receptor-up-1", "mania-key1", ElementType.Image),
            new QuaverOsuElementMap("4k-receptor-up-2", "mania-key2", ElementType.Image),
            new QuaverOsuElementMap("4k-receptor-up-3", "mania-key2", ElementType.Image),
            new QuaverOsuElementMap("4k-receptor-up-4", "mania-key1", ElementType.Image),

            // 4k Note Receptors Down
            new QuaverOsuElementMap("4k-receptor-down-1", "mania-key1D", ElementType.Image),
            new QuaverOsuElementMap("4k-receptor-down-2", "mania-key2D", ElementType.Image),
            new QuaverOsuElementMap("4k-receptor-down-3", "mania-key2D", ElementType.Image),
            new QuaverOsuElementMap("4k-receptor-down-4", "mania-key1D", ElementType.Image),

            // 7k Note Receptors
            new QuaverOsuElementMap("7k-receptor-up-1", "mania-key1", ElementType.Image),
            new QuaverOsuElementMap("7k-receptor-up-2", "mania-key2", ElementType.Image),
            new QuaverOsuElementMap("7k-receptor-up-3", "mania-key1", ElementType.Image),
            new QuaverOsuElementMap("7k-receptor-up-4", "mania-keyS", ElementType.Image),
            new QuaverOsuElementMap("7k-receptor-up-5", "mania-key1", ElementType.Image),
            new QuaverOsuElementMap("7k-receptor-up-6", "mania-key2", ElementType.Image),
            new QuaverOsuElementMap("7k-receptor-up-7", "mania-key1", ElementType.Image),

            // 7k Note Receptors Down
            new QuaverOsuElementMap("7k-receptor-down-1", "mania-key1D", ElementType.Image),
            new QuaverOsuElementMap("7k-receptor-down-2", "mania-key2D", ElementType.Image),
            new QuaverOsuElementMap("7k-receptor-down-3", "mania-key1D", ElementType.Image),
            new QuaverOsuElementMap("7k-receptor-down-4", "mania-keySD", ElementType.Image),
            new QuaverOsuElementMap("7k-receptor-down-5", "mania-key1D", ElementType.Image),
            new QuaverOsuElementMap("7k-receptor-down-6", "mania-key2D", ElementType.Image),
            new QuaverOsuElementMap("7k-receptor-down-7", "mania-key1D", ElementType.Image),

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

            //  Cursor
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

        public void Execute(string[] args)
        {
            var argsList = new List<string>(args);
            argsList.RemoveAt(0);
            var path = string.Join(" ", argsList);

            var extractPath = $@"{Configuration.DataDirectory}/Temp/{Path.GetFileNameWithoutExtension(path)}/";
            Directory.CreateDirectory(extractPath);
          
            try
            {
                Logger.Log("Commencing .osk extraction...", LogColors.GameInfo);

                using (var archive = new ZipFile(path))
                {
                    archive.ExtractAll(extractPath, ExtractExistingFileAction.OverwriteSilently);
                }
                    
                Logger.Log(".osk extraction has completed!", LogColors.GameSuccess);

                var newSkinDirPath = Configuration.SkinDirectory + "/" + Path.GetFileNameWithoutExtension(path);
                Directory.CreateDirectory(newSkinDirPath);

                foreach (var map in QuaverOsuSkinMap)
                {
                    // The extension for the file we'll look for based on the element type
                    var extension = (map.Type == ElementType.Image || map.Type == ElementType.AnimatableImage) ? ".png" : ".wav";

                    // The full path of the osu! skin file
                    var fullPath = extractPath + map.OsuElement.ToLower() + extension;

                    // The base path of the new Quaver skin file.
                    var newPath = newSkinDirPath + "/" + map.QuaverElement + extension;

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
                            catch (Exception e) { Logger.Log(e.Message, LogColors.GameError); }
                            break;
                        // .png image files
                        case ElementType.Image:
                            if (!File.Exists(fullPath)) continue;
                            File.Copy(fullPath, newPath, true);
                            break;
                        // .png animation element image files
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

                Logger.Log("Finished copying over default skin elements. Now proceeding to read skin.ini...", LogColors.GameInfo);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, LogColors.GameError);
            }
            finally
            {
                Directory.Delete(extractPath);
            }
        }
    }
}
