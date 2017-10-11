using IniParser;
using Microsoft.Xna.Framework;
using Quaver.Config;
using System.IO;
using System.Threading.Tasks;

namespace Quaver.Skins
{
    /// <summary>
    /// This class has everything to do with parsing skin.ini files
    /// </summary>
    internal class Skin
    {
        /// <summary>
        /// Name of the skin
        /// </summary>
        internal string Name { get; set; } = "Default";

        /// <summary>
        /// Author of the skin
        /// </summary>
        internal string Author { get; set; } = "Quaver Team";

        /// <summary>
        /// Version number of the skin
        /// </summary>
        internal string Version { get; set; } = "1.0";

        /// <summary>
        /// Should we display the skin's custom menu background?
        /// </summary>
        internal bool CustomBackground { get; set; }

        /// <summary>
        /// Should the skin's cursor rotate?
        /// </summary>
        internal bool CursorRotate { get; set; } = true;

        /// <summary>
        /// Should the skin's cursor trail rotate?
        /// </summary>
        internal bool CursorTrailRotate { get; set; }

        /// <summary>
        /// Should the cursor expand when the mouse is clicked?
        /// </summary>
        internal bool CursorExpand { get; set; } = true;

        internal int BgMaskBufferSize { get; set; } = 12;
        internal int NoteBufferSpacing { get; set; } = 1;
        internal int TimingBarPixelSize { get; set; } = 2;
        internal float HitLightingScale { get; set; } = 4.0f;

        /// <summary>
        /// Size of each lane in pixels.
        /// </summary>
        internal int ColumnSize { get; set; } = 250;

        /// <summary>
        /// The offset of the hit receptor
        /// </summary>
        internal int ReceptorYOffset { get; set; } = 50;

        /// <summary>
        /// The colour that is used for the column's lighting.
        /// </summary>
        internal Color ColourLight1 { get; set; } = new Color(new Vector4(255, 255, 255, 1));
        internal Color ColourLight2 { get; set; } = new Color(new Vector4(255, 255, 255, 1));
        internal Color ColourLight3 { get; set; } = new Color(new Vector4(255, 255, 255, 1));
        internal Color ColourLight4 { get; set; } = new Color(new Vector4(255, 255, 255, 1));

        /// <summary>
        /// The colour of the actual lane
        /// </summary>
        internal Color Colour1 { get; set; } = new Color(new Vector4(255, 25, 255, 1));
        internal Color Colour2 { get; set; } = new Color(new Vector4(255, 255, 255, 1));
        internal Color Colour3 { get; set; } = new Color(new Vector4(255, 255, 255, 1));
        internal Color Colour4 { get; set; } = new Color(new Vector4(255, 255, 255, 1));

        /// <summary>
        ///     Constructor, 
        /// </summary>
        /// <param name="directory"></param>
        internal Skin(string directory)
        {
            // The skin dir
            var skinDirectory = Configuration.SkinDirectory + "/" + directory;

            // Check if skin dir exists
            if (!Directory.Exists(skinDirectory))
                return;

            // Read Skin.ini
            ReadSkinConfig(skinDirectory);
        }

        /// <summary>
        ///     Reads a skin.ini file
        /// </summary>
        /// <param name="skinDir"></param>
        private  void ReadSkinConfig(string skinDir)
        {
            // Check if skin.ini file exists.
            if (!File.Exists(skinDir + "/skin.ini"))
                return;

            // Begin Parsing skin.ini if it does.
            var data = new FileIniDataParser().ReadFile(skinDir + "/skin.ini");

            Name = ConfigHelper.ReadString(Name, data["General"]["Name"]);
            Author = ConfigHelper.ReadString(Author, data["General"]["Author"]);
            Version = ConfigHelper.ReadString(Version, data["General"]["Version"]);
            CustomBackground = ConfigHelper.ReadBool(CustomBackground, data["Menu"]["CustomBackground"]);
            CursorRotate = ConfigHelper.ReadBool(CursorRotate, data["Cursor"]["CursorRotate"]);
            CursorTrailRotate = ConfigHelper.ReadBool(CursorTrailRotate, data["Cursor"]["CursorTrailRotate"]);
            CursorExpand = ConfigHelper.ReadBool(CursorExpand, data["Cursor"]["CursorExpand"]);
            BgMaskBufferSize = ConfigHelper.ReadInt32(BgMaskBufferSize, data["Gameplay"]["BgMaskBufferSize"]);
            NoteBufferSpacing = ConfigHelper.ReadInt32(NoteBufferSpacing, data["Gameplay"]["NoteBufferSpacing"]);
            TimingBarPixelSize = ConfigHelper.ReadInt32(TimingBarPixelSize, data["Gameplay"]["TimingBarPixelSize"]);
            HitLightingScale = ConfigHelper.ReadFloat(HitLightingScale, data["Gameplay"]["HitLightingScale"]);
            ColumnSize = ConfigHelper.ReadInt32(ColumnSize, data["Gameplay"]["ColumnSize"]);
            ReceptorYOffset = ConfigHelper.ReadInt32(ReceptorYOffset, data["Gameplay"]["ReceptorYOffset"]);
            ColourLight1 = ConfigHelper.ReadColor(ColourLight1, data["Colours"]["ColourLight1"]);
            ColourLight2 = ConfigHelper.ReadColor(ColourLight1, data["Colours"]["ColourLight2"]);
            ColourLight3 = ConfigHelper.ReadColor(ColourLight1, data["Colours"]["ColourLight3"]);
            ColourLight4 = ConfigHelper.ReadColor(ColourLight1, data["Colours"]["ColourLight4"]);
            Colour1 = ConfigHelper.ReadColor(ColourLight1, data["Colours"]["Colour1"]);
            Colour2 = ConfigHelper.ReadColor(ColourLight1, data["Colours"]["Colour2"]);
            Colour3 = ConfigHelper.ReadColor(ColourLight1, data["Colours"]["Colour3"]);
            Colour4 = ConfigHelper.ReadColor(ColourLight1, data["Colours"]["Colour4"]);
        }
    }
}
