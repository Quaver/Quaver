using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using IniParser;
using IniParser.Model;
using Quaver.Config;

namespace Quaver.Skinning
{
    /// <summary>
    /// This class has everything to do with parsing skin.ini files
    /// </summary>
    internal class Skin
    {
        /// <summary>
        /// Name of the skin
        /// </summary>
        internal readonly string Name = "Default";

        /// <summary>
        /// Author of the skin
        /// </summary>
        internal readonly string Author = "Quaver Team";

        /// <summary>
        /// Version number of the skin
        /// </summary>
        internal readonly string Version = "1.0";

        /// <summary>
        /// Should we display the skin's custom menu background?
        /// </summary>
        internal readonly bool CustomBackground = false;

        /// <summary>
        /// Should the skin's cursor rotate?
        /// </summary>
        internal readonly bool CursorRotate = true;

        /// <summary>
        /// Should the skin's cursor trail rotate?
        /// </summary>
        internal readonly bool CursorTrailRotate = false;

        /// <summary>
        /// Should the cursor expand when the mouse is clicked?
        /// </summary>
        internal readonly bool CursorExpand = true;

        internal readonly int BgMaskBufferSize = 12;
        internal readonly int NoteBufferSpacing = 1;
        internal readonly int TimingBarPixelSize = 2;
        internal readonly float HitLightingScale = 4.0f;

        /// <summary>
        /// Size of each lane in pixels.
        /// </summary>
        internal readonly int ColumnSize = 250;

        /// <summary>
        /// The offset of the hit receptor
        /// </summary>
        internal readonly int ReceptorYOffset = 50;

        /// <summary>
        /// The colour that is used for the column's lighting.
        /// </summary>
        internal readonly Color ColourLight1 = new Color(new Vector4(255, 255, 255, 1));
        internal readonly Color ColourLight2 = new Color(new Vector4(255, 255, 255, 1));
        internal readonly Color ColourLight3 = new Color(new Vector4(255, 255, 255, 1));
        internal readonly Color ColourLight4 = new Color(new Vector4(255, 255, 255, 1));

        /// <summary>
        /// The colour of the actual lane
        /// </summary>
        internal readonly Color Colour1 = new Color(new Vector4(255, 25, 255, 1));
        internal readonly Color Colour2 = new Color(new Vector4(255, 255, 255, 1));
        internal readonly Color Colour3 = new Color(new Vector4(255, 255, 255, 1));
        internal readonly Color Colour4 = new Color(new Vector4(255, 255, 255, 1));

        internal Skin(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            // Begin Parsing skin.ini
            var data = new FileIniDataParser().ReadFile(filePath);

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

        /// <summary>
        /// Asynchronously parses and creates a new skin object.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static async Task<Skin> Create(string filePath)
        {
            return await Task.Run(() => new Skin(filePath));
        }
    }
}
