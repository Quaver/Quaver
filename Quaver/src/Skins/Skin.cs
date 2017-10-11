using System;
using System.Collections.Generic;
using IniParser;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Quaver.Main;

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
        ///     All of the textures for the loaded skin elements. 
        ///     Here we initialize all of them to a new texture, however they'll be replaced by an actual skin element
        ///     texture upon loading. This is just to not have them null when creating the dictionary in LoadSkinElements();
        ///     Is this shit? Probably.
        /// </summary>
        internal Texture2D ColumnBgMask { get; set; }
        internal Texture2D ColumnHitLighting { get; set; }
        internal Texture2D ColumnTimingBar { get; set; }
        internal Texture2D NoteHitObject1 { get; set; }
        internal Texture2D NoteHitObject2 { get; set; }
        internal Texture2D NoteHitObject3 { get; set; }
        internal Texture2D NoteHitObject4 { get; set; }
        internal Texture2D RankingA { get; set; }
        internal Texture2D RankingB { get; set; }
        internal Texture2D RankingC { get; set; }
        internal Texture2D RankingD { get; set; }
        internal Texture2D RankingS { get; set; }
        internal Texture2D RankingSS { get; set; }
        internal Texture2D RankingX { get; set; }
        internal Texture2D NoteHoldEnd { get; set; }
        internal Texture2D NoteHoldBody { get; set; }
        internal Texture2D NoteReceptor { get; set; }
        internal Texture2D JudgeMiss { get; set; }
        internal Texture2D JudgeBad { get; set; }
        internal Texture2D JudgeGood { get; set; }
        internal Texture2D JudgeGreat { get; set; }
        internal Texture2D JudgePerfect { get; set; }
        internal Texture2D JudgeMarv { get; set; }

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

            // Load all skin elements
            LoadSkinElements(skinDirectory);
        }

        /// <summary>
        ///     Loads all the skin elements from disk.
        /// </summary>
        /// <param name="skinDir"></param>
        private void LoadSkinElements(string skinDir)
        {
            // Contains the file names of all skin elements
            var skinElements = new []
            {
                @"column-bgmask",
                @"column-hitlighting",
                @"column-timingbar",
                @"note-hitobject1",
                @"note-hitobject2",
                @"note-hitobject3",
                @"note-hitobject4",
                @"ranking-a",
                @"ranking-b",
                @"ranking-c",
                @"ranking-d",
                @"ranking-s",
                @"ranking-ss",
                @"ranking-x",
                @"note-holdend",
                @"note-holdbody",
                @"note-receptor",
                @"judge-miss",
                @"judge-bad",
                @"judge-good",
                @"judge-great",
                @"judge-perfect",
                @"judge-marv"
            };

            foreach (var element in skinElements)
            {
                var skinElementPath = skinDir + $"/{element}.png";
                    
                // Load up all the skin elements.
                switch (element)
                {
                    case @"column-bgmask":
                        ColumnBgMask = LoadIndividualElement(element, skinElementPath, ColumnBgMask);
                        break;
                    case @"column-hitlighting":
                        ColumnHitLighting = LoadIndividualElement(element, skinElementPath, ColumnHitLighting);
                        break;
                    case @"column-timingbar":
                        ColumnTimingBar = LoadIndividualElement(element, skinElementPath, ColumnTimingBar);
                        break;
                    case @"note-hitobject1":
                        NoteHitObject1 = LoadIndividualElement(element, skinElementPath, NoteHitObject1);
                        break;
                    case @"note-hitobject2":
                        NoteHitObject2 = LoadIndividualElement(element, skinElementPath, NoteHitObject2);
                        break;
                    case @"note-hitobject3":
                        NoteHitObject3 = LoadIndividualElement(element, skinElementPath, NoteHitObject3);
                        break;
                    case @"note-hitobject4":
                        NoteHitObject4 = LoadIndividualElement(element, skinElementPath, NoteHitObject4);
                        break;
                    case @"ranking-a":
                        RankingA = LoadIndividualElement(element, skinElementPath, RankingA);
                        break;
                    case @"ranking-b":
                        RankingB = LoadIndividualElement(element, skinElementPath, RankingB);
                        break;
                    case @"ranking-c":
                        RankingC = LoadIndividualElement(element, skinElementPath, RankingC);
                        break;
                    case @"ranking-d":
                        RankingD = LoadIndividualElement(element, skinElementPath, RankingD);
                        break;
                    case @"ranking-s":
                        RankingS = LoadIndividualElement(element, skinElementPath, RankingS);
                        break;
                    case @"ranking-ss":
                        RankingSS = LoadIndividualElement(element, skinElementPath, RankingSS);
                        break;
                    case @"ranking-x":
                        RankingX = LoadIndividualElement(element, skinElementPath, RankingX);
                        break;
                    case @"note-holdend":
                        NoteHoldEnd = LoadIndividualElement(element, skinElementPath, NoteHoldEnd);
                        break;
                    case @"note-holdbody":
                        NoteHoldBody = LoadIndividualElement(element, skinElementPath, NoteHoldBody);
                        break;
                    case @"note-receptor":
                        NoteReceptor = LoadIndividualElement(element, skinElementPath, NoteReceptor);
                        break;
                    case @"judge-miss":
                        JudgeMiss = LoadIndividualElement(element, skinElementPath, JudgeMiss);
                        break;
                    case @"judge-bad":
                        JudgeBad = LoadIndividualElement(element, skinElementPath, JudgeBad);
                        break;
                    case @"judge-good":
                        JudgeGood = LoadIndividualElement(element, skinElementPath, JudgeGood);
                        break;
                    case @"judge-great":
                        JudgeGreat = LoadIndividualElement(element, skinElementPath, JudgeGreat);
                        break;
                    case @"judge-perfect":
                        JudgePerfect = LoadIndividualElement(element, skinElementPath, JudgePerfect);
                        break;
                    case @"judge-marv":
                        JudgeMarv = LoadIndividualElement(element, skinElementPath, JudgeMarv);
                        break;
                    default:
                        break;
                }
            }            
        }

        /// <summary>
        ///     Loads an individual element.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tex"></param>
        private Texture2D LoadIndividualElement(string element, string path, Texture2D tex)
        {
            // Check if the skin file exists. If not, load and return the default skin element.
            if (!File.Exists(path))
            {
                path = $"Default Skin/{element}";
                Console.WriteLine($"[SKIN LOADER] Skin element: {element}.png could not be found. Resulting to default: {path}");
                return GameBase.Content.Load<Texture2D>(path);
            }

            // Load the skin element
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                return Texture2D.FromStream(GameBase.GraphicsDevice, fileStream);
            }
        }

        /// <summary>
        ///     Reads a skin.ini file
        /// </summary>
        /// <param name="skinDir"></param>
        private void ReadSkinConfig(string skinDir)
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
