using System;
using System.Collections.Generic;
using IniParser;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting.Channels;
using System.Threading.Tasks;
using Quaver.Audio;
using Quaver.Utility;

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

        /// <summary>
        ///     The padding (Positional offset) of the notes relative from the bg mask.
        /// </summary>
        internal int BgMaskPadding { get; set; } = 7;

        /// <summary>
        ///     The padding (Positional offset) of the notes relative from eachother.
        /// </summary>
        internal int NotePadding { get; set; } = 1;

        /// <summary>
        ///     The size of the timing pars (in pixels).
        /// </summary>
        internal int TimingBarPixelSize { get; set; } = 2;

        /// <summary>
        ///     The scale of the hitlighting objects.
        /// </summary>
        internal float HitLightingScale { get; set; } = 4.0f;

        /// <summary>
        /// Size of each lane in pixels.
        /// </summary>
        internal int ColumnSize { get; set; } = 80;

        /// <summary>
        /// The offset of the hit receptor
        /// </summary>
        internal int ReceptorYOffset { get; set; } = 50;

        /// <summary>
        ///     The alignment of the column
        /// 
        ///     "When read, it'll be between 0 and 1
        ///     if it's 0, it'll snap to the left, 1 will snap to right
        ///     0.5 to the middle"
        /// </summary>
        internal byte ColumnAlignment { get; set; } = 50;

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
        ///     We first attempt to load the selected skin's elements, however if we can't,
        ///     it'll result it to the default.
        /// </summary>
        internal Texture2D ColumnBgMask { get; set; }
        internal Texture2D ColumnHitLighting { get; set; }
        internal Texture2D ColumnTimingBar { get; set; }
        internal Texture2D NoteHitObject1 { get; set; }
        internal Texture2D NoteHitObject2 { get; set; }
        internal Texture2D NoteHitObject3 { get; set; }
        internal Texture2D NoteHitObject4 { get; set; }
        internal Texture2D GradeSmallA { get; set; }
        internal Texture2D GradeSmallB { get; set; }
        internal Texture2D GradeSmallC { get; set; }
        internal Texture2D GradeSmallD { get; set; }
        internal Texture2D GradeSmallS { get; set; }
        internal Texture2D GradeSmallSS { get; set; }
        internal Texture2D GradeSmallX { get; set; }
        internal Texture2D GradeSmallXX { get; set; }
        internal Texture2D GradeSmallXXX { get; set; }
        internal Texture2D NoteHoldEnd { get; set; }
        internal Texture2D NoteHoldBody { get; set; }
        internal Texture2D NoteReceptor { get; set; }
        internal Texture2D JudgeMiss { get; set; }
        internal Texture2D JudgeBad { get; set; }
        internal Texture2D JudgeGood { get; set; }
        internal Texture2D JudgeGreat { get; set; }
        internal Texture2D JudgePerfect { get; set; }
        internal Texture2D JudgeMarv { get; set; }

        internal GameEffect Hit { get; set; }

        // Contains the file names of all skin elements
        private readonly string[] skinElements = new[]
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
                @"ranking-xx",
                @"ranking-xxx",
                @"note-holdend",
                @"note-holdbody",
                @"note-receptor",
                @"judge-miss",
                @"judge-bad",
                @"judge-good",
                @"judge-great",
                @"judge-perfect",
                @"judge-marv",
                @"hit"
        };

        /// <summary>
        ///     Constructor, 
        /// </summary>
        /// <param name="directory"></param>
        internal Skin(string directory)
        {
            // The skin dir
            var skinDirectory = Configuration.SkinDirectory + "/" + directory;

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
            foreach (var element in skinElements)
            {
                var skinElementPath = skinDir + $"/{element}.png";
                    
                // Load up all the skin elements.
                switch (element)
                {
                    case @"column-bgmask":
                        ColumnBgMask = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"column-hitlighting":
                        ColumnHitLighting = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"column-timingbar":
                        ColumnTimingBar = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-hitobject1":
                        NoteHitObject1 = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-hitobject2":
                        NoteHitObject2 = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-hitobject3":
                        NoteHitObject3 = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-hitobject4":
                        NoteHitObject4 = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"grade-small-a":
                        GradeSmallA = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"grade-small-b":
                        GradeSmallB = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"grade-small-c":
                        GradeSmallC = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"grade-small-d":
                        GradeSmallD = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"grade-small-s":
                        GradeSmallS = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"grade-small-ss":
                        GradeSmallSS = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"grade-small-x":
                        GradeSmallX = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"grade-small-xx":
                        GradeSmallXX = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"grade-small-xxx":
                        GradeSmallXXX = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdend":
                        NoteHoldEnd = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdbody":
                        NoteHoldBody = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-receptor":
                        NoteReceptor = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"judge-miss":
                        JudgeMiss = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"judge-bad":
                        JudgeBad = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"judge-good":
                        JudgeGood = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"judge-great":
                        JudgeGreat = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"judge-perfect":
                        JudgePerfect = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"judge-marv":
                        JudgeMarv = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"hit":
                        Hit = LoadGameEffectElement(Assembly.GetExecutingAssembly().GetManifestResourceStream("Quaver.Resources.Default_Skin.hit.mp3"), "hit.mp3");
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
        private Texture2D LoadIndividualElement(string element, string path)
        {
            // If the image file exists, go ahead and load it into a texture.
            if (File.Exists(path))
                return ImageLoader.Load(path);

            // Otherwise, we'll have to change the path to that of the default element and load that instead.
            path = $"{element}";
            //Console.WriteLine($"[SKIN LOADER] Skin element: {element}.png could not be found. Resulting to default: {path}");

            return GameBase.Content.Load<Texture2D>(path);
        }

        /// <summary>
        ///     Used for loading game effects on the skin.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private GameEffect LoadGameEffectElement(Stream stream, string path)
        {
            var fullPath = Configuration.SkinDirectory + "/" + Configuration.Skin + "/" + path;

            return (File.Exists(fullPath)) ? new GameEffect(fullPath) : new GameEffect(stream);
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
            BgMaskPadding = ConfigHelper.ReadInt32(BgMaskPadding, data["Gameplay"]["BgMaskPadding"]);
            NotePadding = ConfigHelper.ReadInt32(NotePadding, data["Gameplay"]["NotePadding"]);
            TimingBarPixelSize = ConfigHelper.ReadInt32(TimingBarPixelSize, data["Gameplay"]["TimingBarPixelSize"]);
            HitLightingScale = ConfigHelper.ReadFloat(HitLightingScale, data["Gameplay"]["HitLightingScale"]);
            ColumnSize = ConfigHelper.ReadInt32(ColumnSize, data["Gameplay"]["ColumnSize"]);
            ReceptorYOffset = ConfigHelper.ReadInt32(ReceptorYOffset, data["Gameplay"]["ReceptorYOffset"]);
            ColumnAlignment = ConfigHelper.ReadPercentage(ColumnAlignment, data["Gameplay"]["ColumnAlignment"]);
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
