using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

using Quaver.Config;
using Quaver.Logging;

namespace Quaver.Peppy
{
    /// <summary>
    ///     Deserialization class for osu!'s skin.ini
    /// </summary>
    public struct OsuSkinConfig
    {
        /// <summary>
        ///     Name of the skin
        /// </summary>
        public string Name;

        /// <summary>
        ///     Author of the skin
        /// </summary>
        public string Author;

        /// <summary>
        ///     osu! mania ini config for 4k
        /// </summary>
        public OsuManiaSkinConfig Keys4Config;

        /// <summary>
        ///     osu! mania ini config for 7k
        /// </summary>
        public OsuManiaSkinConfig Keys7Config;

        /// <summary>
        ///     Constructor - Reads an osu!mania skin.ini file
        /// </summary>
        public OsuSkinConfig(string path)
        {
            Name = "";
            Author = "";
            Keys4Config = new OsuManiaSkinConfig { Colours = new List<Color>() };
            Keys7Config = new OsuManiaSkinConfig { Colours = new List<Color>() };

            if (!File.Exists(path.ToLower()))
                return;

            var section = "";
            var currentKeyCount = 0;

            try
            {
                foreach (var line in File.ReadAllLines(path))
                {
                    switch (line.Trim())
                    {
                        case "[General]":
                            section = "[General]";
                            break;
                        case "[Mania]":
                            section = "[Mania]";
                            break;
                    }

                    if (!line.Contains(":"))
                        continue;

                    var key = line.Substring(0, line.IndexOf(':')).Trim();
                    var value = line.Split(':').Last().Trim();

                    switch (section)
                    {
                        case "[General]":
                            switch (key)
                            {
                                case "Name":
                                    Name = value;
                                    break;
                                case "Author":
                                    Author = value;
                                    break;
                            }
                            break;
                        case "[Mania]":
                            switch (key)
                            {
                                case "Keys":
                                    currentKeyCount = Convert.ToInt32(value);
                                    if (currentKeyCount == 4) Keys4Config.Keys = currentKeyCount; else if (currentKeyCount == 7) Keys7Config.Keys = currentKeyCount;
                                    break;
                                case "ColumnStart":
                                    var cs = Convert.ToInt32(value);
                                    if (currentKeyCount == 4) Keys4Config.ColumnStart = cs; else if (currentKeyCount == 7) Keys7Config.ColumnStart = cs;
                                    break;
                                case "LightFramePerSecond":
                                    var lfps = Convert.ToInt32(value);
                                    if (currentKeyCount == 4) Keys4Config.LightFramePersecond = lfps; else if (currentKeyCount == 7) Keys7Config.LightFramePersecond = lfps;
                                    break;
                                case "Colour1":
                                case "Colour2":
                                case "Colour3":
                                case "Colour4":
                                case "Colour5":
                                case "Colour6":
                                case "Colour7":
                                    var colour = ConfigHelper.ReadColor(new Color(0, 0, 0, 230), value);
                                    if (currentKeyCount == 4) Keys4Config.Colours.Add(colour); else if (currentKeyCount == 7) Keys4Config.Colours.Add(colour);
                                    break;
                                case "ColourHold":
                                    var colourHold = ConfigHelper.ReadColor(new Color(0, 0, 0, 230), value);
                                    if (currentKeyCount == 4) Keys4Config.ColourHold = colourHold; else if (currentKeyCount == 7) Keys4Config.ColourHold = colourHold;
                                    break;
                                case "ColumnWidth":
                                    break;
                                case "KeysUnderNotes":
                                    var kun = Convert.ToBoolean(value);
                                    if (currentKeyCount == 4) Keys4Config.KeysUnderNotes = kun; else Keys7Config.KeysUnderNotes = kun;
                                    break;
                                case "KeyImage0":
                                    if (currentKeyCount == 4) Keys4Config.KeyImage0 = value; else if (currentKeyCount == 7) Keys7Config.KeyImage0 = value;
                                    break;
                                case "KeyImage1":
                                    if (currentKeyCount == 4) Keys4Config.KeyImage1 = value; else if (currentKeyCount == 7) Keys7Config.KeyImage1 = value;
                                    break;
                                case "KeyImage2":
                                    if (currentKeyCount == 4) Keys4Config.KeyImage2 = value; else if (currentKeyCount == 7) Keys7Config.KeyImage2 = value;
                                    break;
                                case "KeyImage3":
                                    if (currentKeyCount == 4) Keys4Config.KeyImage3 = value; else if (currentKeyCount == 7) Keys7Config.KeyImage3 = value;
                                    break;
                                case "KeyImage4":
                                    if (currentKeyCount == 4) Keys4Config.KeyImage4 = value; else if (currentKeyCount == 7) Keys7Config.KeyImage4 = value;
                                    break;
                                case "KeyImage5":
                                    if (currentKeyCount == 4) Keys4Config.KeyImage5 = value; else if (currentKeyCount == 7) Keys7Config.KeyImage5 = value;
                                    break;
                                case "KeyImage6":
                                    if (currentKeyCount == 4) Keys4Config.KeyImage6 = value; else if (currentKeyCount == 7) Keys7Config.KeyImage6 = value;
                                    break;
                                case "KeyImage0D":
                                    if (currentKeyCount == 4) Keys4Config.KeyImage0D = value; else if (currentKeyCount == 7) Keys7Config.KeyImage0D = value;
                                    break;
                                case "KeyImage1D":
                                    if (currentKeyCount == 4) Keys4Config.KeyImage1D = value; else if (currentKeyCount == 7) Keys7Config.KeyImage1D = value;
                                    break;
                                case "KeyImage2D":
                                    if (currentKeyCount == 4) Keys4Config.KeyImage2D = value; else if (currentKeyCount == 7) Keys7Config.KeyImage2D = value;
                                    break;
                                case "KeyImage3D":
                                    if (currentKeyCount == 4) Keys4Config.KeyImage3D = value; else if (currentKeyCount == 7) Keys7Config.KeyImage3D = value;
                                    break;
                                case "KeyImage4D":
                                    if (currentKeyCount == 4) Keys4Config.KeyImage4D = value; else if (currentKeyCount == 7) Keys7Config.KeyImage4D = value;
                                    break;
                                case "KeyImage5D":
                                    if (currentKeyCount == 4) Keys4Config.KeyImage5D = value; else if (currentKeyCount == 7) Keys7Config.KeyImage5D = value;
                                    break;
                                case "KeyImage6D":
                                    if (currentKeyCount == 4) Keys4Config.KeyImage6D = value; else if (currentKeyCount == 7) Keys7Config.KeyImage6D = value;
                                    break;
                                case "NoteImage0":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage0 = value; else if (currentKeyCount == 7) Keys7Config.NoteImage0 = value;
                                    break;
                                case "NoteImage1":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage1 = value; else if (currentKeyCount == 7) Keys7Config.NoteImage1 = value;
                                    break;
                                case "NoteImage2":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage2 = value; else if (currentKeyCount == 7) Keys7Config.NoteImage2 = value;
                                    break;
                                case "NoteImage3":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage3 = value; else if (currentKeyCount == 7) Keys7Config.NoteImage3 = value;
                                    break;
                                case "NoteImage4":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage4 = value; else if (currentKeyCount == 7) Keys7Config.NoteImage4 = value;
                                    break;
                                case "NoteImage5":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage5 = value; else if (currentKeyCount == 7) Keys7Config.NoteImage5 = value;
                                    break;
                                case "NoteImage6":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage6 = value; else if (currentKeyCount == 7) Keys7Config.NoteImage6 = value;
                                    break;
                                case "NoteImage0L":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage0L = value; else if (currentKeyCount == 7) Keys7Config.NoteImage0L = value;
                                    break;
                                case "NoteImage1L":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage1L = value; else if (currentKeyCount == 7) Keys7Config.NoteImage1L = value;
                                    break;
                                case "NoteImage2L":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage2L = value; else if (currentKeyCount == 7) Keys7Config.NoteImage2L = value;
                                    break;
                                case "NoteImage3L":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage3L = value; else if (currentKeyCount == 7) Keys7Config.NoteImage3L = value;
                                    break;
                                case "NoteImage4L":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage4L = value; else if (currentKeyCount == 7) Keys7Config.NoteImage4L = value;
                                    break;
                                case "NoteImage5L":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage5L = value; else if (currentKeyCount == 7) Keys7Config.NoteImage5L = value;
                                    break;
                                case "NoteImage6L":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage6L = value; else if (currentKeyCount == 7) Keys7Config.NoteImage6L = value;
                                    break;
                                case "NoteImage0T":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage0T = value; else if (currentKeyCount == 7) Keys7Config.NoteImage0T = value;
                                    break;
                                case "NoteImage1T":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage1T = value; else if (currentKeyCount == 7) Keys7Config.NoteImage1T = value;
                                    break;
                                case "NoteImage2T":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage2T = value; else if (currentKeyCount == 7) Keys7Config.NoteImage2T = value;
                                    break;
                                case "NoteImage3T":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage3T = value; else if (currentKeyCount == 7) Keys7Config.NoteImage3T = value;
                                    break;
                                case "NoteImage4T":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage4T = value; else if (currentKeyCount == 7) Keys7Config.NoteImage4T = value;
                                    break;
                                case "NoteImage5T":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage5T = value; else if (currentKeyCount == 7) Keys7Config.NoteImage5T = value;
                                    break;
                                case "NoteImage6T":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage6T = value; else if (currentKeyCount == 7) Keys7Config.NoteImage6T = value;
                                    break;
                                case "NoteImage0H":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage0H = value; else if (currentKeyCount == 7) Keys7Config.NoteImage0H = value;
                                    break;
                                case "NoteImage1H":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage1H = value; else if (currentKeyCount == 7) Keys7Config.NoteImage1H = value;
                                    break;
                                case "NoteImage2H":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage2H = value; else if (currentKeyCount == 7) Keys7Config.NoteImage2H = value;
                                    break;
                                case "NoteImage3H":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage3H = value; else if (currentKeyCount == 7) Keys7Config.NoteImage3H = value;
                                    break;
                                case "NoteImage4H":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage4H = value; else if (currentKeyCount == 7) Keys7Config.NoteImage4H = value;
                                    break;
                                case "NoteImage5H":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage5H = value; else if (currentKeyCount == 7) Keys7Config.NoteImage5H = value;
                                    break;
                                case "NoteImage6H":
                                    if (currentKeyCount == 4) Keys4Config.NoteImage6H = value; else if (currentKeyCount == 7) Keys7Config.NoteImage6H = value;
                                    break;
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }
    }

    /// <summary>
    ///     An individual mania config setup in osu!'s skin.ini
    ///     Documentation on this can be found here:
    ///         https://osu.ppy.sh/help/wiki/Skinning/skin.ini
    /// </summary>
    public struct OsuManiaSkinConfig
    {
        public int Keys;
        public int ColumnStart;
        public int LightFramePersecond;
        public List<Color> Colours;
        public Color ColourHold;
        public bool KeysUnderNotes;
        public string KeyImage0;
        public string KeyImage1;
        public string KeyImage2;
        public string KeyImage3;
        public string KeyImage4;
        public string KeyImage5;
        public string KeyImage6;
        public string KeyImage0D;
        public string KeyImage1D;
        public string KeyImage2D;
        public string KeyImage3D;
        public string KeyImage4D;
        public string KeyImage5D;
        public string KeyImage6D;
        public string NoteImage0;
        public string NoteImage1;
        public string NoteImage2;
        public string NoteImage3;
        public string NoteImage4;
        public string NoteImage5;
        public string NoteImage6;
        public string NoteImage0L;
        public string NoteImage1L;
        public string NoteImage2L;
        public string NoteImage3L;
        public string NoteImage4L;
        public string NoteImage5L;
        public string NoteImage6L;
        public string NoteImage0T;
        public string NoteImage1T;
        public string NoteImage2T;
        public string NoteImage3T;
        public string NoteImage4T;
        public string NoteImage5T;
        public string NoteImage6T;
        public string NoteImage0H;
        public string NoteImage1H;
        public string NoteImage2H;
        public string NoteImage3H;
        public string NoteImage4H;
        public string NoteImage5H;
        public string NoteImage6H;
    }
}
