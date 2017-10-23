using System;
using System.Collections.Generic;
using IniParser;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using System.IO;
using System.Net.Mime;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Quaver.Main;
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

            /*Console.WriteLine($"[SKIN LOADER] Skin successfully loaded!\n-----\n" +
                              $"Name: {Name}\n" +
                              $"Author: {Author}\n" +
                              $"Version: {Version}\n" +
                              $"CustomBackground: {CustomBackground}\n" +
                              $"CursorRotate: {CursorRotate}\n" +
                              $"CursorTrailRotate: {CursorTrailRotate}\n" +
                              $"CursorExpand: {CursorExpand}\n" +
                              $"BgMaskBufferSize: {BgMaskBufferSize}\n" +
                              $"NoteBufferSpacing: {NoteBufferSpacing}\n" +
                              $"TimingBarPixelSize: {TimingBarPixelSize}\n" +
                              $"HitLightingScale: {HitLightingScale}\n" +
                              $"ColumnSize: {ColumnSize}\n" +
                              $"ReceptorYOffset: {ReceptorYOffset}\n" +
                              $"ColourLight1: {ColourLight1}\n" +
                              $"ColourLight2: {ColourLight2}\n" +
                              $"ColourLight3: {ColourLight3}\n" +
                              $"ColourLight4: {ColourLight4}\n" +
                              $"Colour1: {Colour1}\n" +
                              $"Colour2: {Colour2}\n" +
                              $"Colour3: {Colour3}\n" +
                              $"Colour4: {Colour4}\n" +
                              $"ColumnBgMask: {ColumnBgMask.Bounds}\n" +
                              $"ColumnHitLighting: {ColumnHitLighting.Bounds}\n" +
                              $"ColumnTimingBar: {ColumnTimingBar.Bounds}\n" +
                              $"NoteHitObject1: {NoteHitObject1.Bounds}\n" +
                              $"NoteHitObject2: {NoteHitObject2.Bounds}\n" +
                              $"NoteHitObject3: {NoteHitObject3.Bounds}\n" +
                              $"NoteHitObject4: {NoteHitObject4.Bounds}\n" +
                              $"RankingA: {RankingA.Bounds}\n" +
                              $"RankingB: {RankingB.Bounds}\n" +
                              $"RankingC: {RankingC.Bounds}\n" +
                              $"RankingD: {RankingD.Bounds}\n" +
                              $"RankingS: {RankingS.Bounds}\n" +
                              $"RankingSS: {RankingSS.Bounds}\n" +
                              $"RankingX: {RankingX.Bounds}\n" +
                              $"NoteHoldEnd: {NoteHoldEnd.Bounds}\n" +
                              $"NoteHoldBody: {NoteHoldBody.Bounds}\n" +
                              $"NoteReceptor: {NoteReceptor.Bounds}\n" +
                              $"JudgeMiss: {JudgeMiss.Bounds}\n" +
                              $"JudgeBad: {JudgeBad.Bounds}\n" +
                              $"JudgeGood: {JudgeGood.Bounds}\n" +
                              $"JudgeGreat: {JudgeGreat.Bounds}\n" +
                              $"JudgePerfect: {JudgePerfect.Bounds}\n" +
                              $"JudgeMarv: {JudgeMarv.Bounds}\n");*/
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
                    case @"ranking-a":
                        RankingA = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"ranking-b":
                        RankingB = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"ranking-c":
                        RankingC = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"ranking-d":
                        RankingD = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"ranking-s":
                        RankingS = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"ranking-ss":
                        RankingSS = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"ranking-x":
                        RankingX = LoadIndividualElement(element, skinElementPath);
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
            Console.WriteLine($"[SKIN LOADER] Skin element: {element}.png could not be found. Resulting to default: {path}");

            return GameBase.Content.Load<Texture2D>(path);
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
