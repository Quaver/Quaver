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
using Microsoft.Xna.Framework.Audio;
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
        internal int ColumnSize7K { get; set; } = 65;

        /// <summary>
        /// The offset of the hit receptor
        /// </summary>
        internal int ReceptorYOffset { get; set; } = 50;

        /// <summary>
        /// The alignment of the playfield as a percentage. 
        /// </summary>
        internal byte ColumnAlignment { get; set; } = 50;

        /// <summary>
        ///     Determines whether or not to color the HitObjects by their snap distance
        /// </summary>
        internal bool ColourObjectsBySnapDistance { get; set; }

        /// <summary>
        ///     Determines the FPS of the animations
        ///     Max - 255fps.
        /// </summary>
        internal byte LightFramesPerSecond { get; set; } = 240;

        /// <summary>
        /// The colour that is used for the column's lighting.
        /// </summary>
        internal Color ColourLight1 { get; set; } = new Color(new Vector4(255, 255, 255, 1));
        internal Color ColourLight2 { get; set; } = new Color(new Vector4(255, 255, 255, 1));
        internal Color ColourLight3 { get; set; } = new Color(new Vector4(255, 255, 255, 1));
        internal Color ColourLight4 { get; set; } = new Color(new Vector4(255, 255, 255, 1));

        /// <summary>
        ///     All of the textures for the loaded skin elements. 
        ///     We first attempt to load the selected skin's elements, however if we can't,
        ///     it'll result it to the default.
        /// </summary>
        internal Texture2D ColumnBgMask { get; set; }
        internal Texture2D ColumnHitLighting { get; set; }
        internal Texture2D ColumnTimingBar { get; set; }

        // 4k - HitObjects, HoldBodies, HoldEndies, & NoteReceptors
        // defined for each key lane.
        internal Texture2D[] NoteHitObjects { get; set; } = new Texture2D[4];
        internal Texture2D[] NoteHoldBodies { get; set; } = new Texture2D[4];
        internal Texture2D[] NoteHoldEnds { get; set; } = new Texture2D[4];
        internal Texture2D[] NoteReceptors { get; set; } = new Texture2D[4];

        // 7k - HitObjects, HoldBodies, HoldEndies, & NoteReceptors
        // defined for each key lane.
        internal Texture2D[] NoteHitObjects7K { get; set; } = new Texture2D[7];
        internal Texture2D[] NoteHoldBodies7K { get; set; } = new Texture2D[7];
        internal Texture2D[] NoteHoldEnds7K { get; set; } = new Texture2D[7];
        internal Texture2D[] NoteReceptors7K { get; set; } = new Texture2D[7];

        /// <summary>
        ///     Grades
        /// </summary>
        internal Texture2D GradeSmallA { get; set; }
        internal Texture2D GradeSmallB { get; set; }
        internal Texture2D GradeSmallC { get; set; }
        internal Texture2D GradeSmallD { get; set; }
        internal Texture2D GradeSmallF { get; set; }
        internal Texture2D GradeSmallS { get; set; }
        internal Texture2D GradeSmallSS { get; set; }
        internal Texture2D GradeSmallX { get; set; }
        internal Texture2D GradeSmallXX { get; set; }
        internal Texture2D GradeSmallXXX { get; set; }

        /// <summary>
        ///     Judge
        /// </summary>
        internal Texture2D JudgeMiss { get; set; }
        internal Texture2D JudgeBad { get; set; }
        internal Texture2D JudgeGood { get; set; }
        internal Texture2D JudgeGreat { get; set; }
        internal Texture2D JudgePerfect { get; set; }
        internal Texture2D JudgeMarv { get; set; }

        /// <summary>
        ///     Cursor
        /// </summary>
        internal Texture2D Cursor { get; set; }

        /// <summary>
        ///     Sound Effect elements in skin
        /// </summary>
        internal SoundEffect Hit { get; set; }
        internal SoundEffect ComboBreak { get; set; }
        internal SoundEffect Applause { get; set; }
        internal SoundEffect Screenshot { get; set; }
        internal SoundEffect Click { get; set; }
        internal SoundEffect Back { get; set; }

        /// <summary>
        ///  Animation Elements
        /// </summary>
        internal List<Texture2D> HitLighting { get; set; }

        // Contains the file names of all skin elements
        private readonly string[] skinElements = new[]
        {
                @"column-bgmask",
                @"column-hitlighting",
                @"column-timingbar",

                // 4k HitObjects
                @"note-hitobject1",
                @"note-hitobject2",
                @"note-hitobject3",
                @"note-hitobject4",

                // 7k HitObjects
                @"note-hitobject7k1",
                @"note-hitobject7k2",
                @"note-hitobject7k3",
                @"note-hitobject7k4",
                @"note-hitobject7k5",
                @"note-hitobject7k6",
                @"note-hitobject7k7",

                // Grades
                @"grade-small-a",
                @"grade-small-b",
                @"grade-small-c",
                @"grade-small-d",
                @"grade-small-f",
                @"grade-small-s",
                @"grade-small-ss",
                @"grade-small-x",
                @"grade-small-xx",
                @"grade-small-xxx",

                // 4k Hit Object Hold Ends
                @"note-holdend1",
                @"note-holdend2",
                @"note-holdend3",
                @"note-holdend4",

                // 7k Hit Object Hold Ends
                @"note-holdend7k1",
                @"note-holdend7k2",
                @"note-holdend7k3",
                @"note-holdend7k4",
                @"note-holdend7k5",
                @"note-holdend7k6",
                @"note-holdend7k7",

                // 4k Hit Object Hold Bodies
                @"note-holdbody1",
                @"note-holdbody2",
                @"note-holdbody3",
                @"note-holdbody4",

                // 7k Hit Object Hold Bodies
                @"note-holdbody7k1",
                @"note-holdbody7k2",
                @"note-holdbody7k3",
                @"note-holdbody7k4",
                @"note-holdbody7k5",
                @"note-holdbody7k6",
                @"note-holdbody7k7",

                // 4k Note Receptors
                @"note-receptor1",
                @"note-receptor2",
                @"note-receptor3",
                @"note-receptor4",

                // 7k Note Receptors
                @"note-receptor7k1",
                @"note-receptor7k2",
                @"note-receptor7k3",
                @"note-receptor7k4",
                @"note-receptor7k5",
                @"note-receptor7k6",
                @"note-receptor7k7",

                // Judge
                @"judge-miss",
                @"judge-bad",
                @"judge-good",
                @"judge-great",
                @"judge-perfect",
                @"judge-marv",

                //  Cursor
                @"cursor",

                // Sound Effects
                @"hit",
                @"combobreak",
                @"applause",
                @"screenshot",
                @"click",
                @"back",

                // Animation Frames
                @"hitlighting"
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
                        NoteHitObjects[0] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-hitobject2":
                        NoteHitObjects[1] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-hitobject3":
                        NoteHitObjects[2] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-hitobject4":
                        NoteHitObjects[3] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-hitobject7k1":
                        NoteHitObjects7K[0] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-hitobject7k2":
                        NoteHitObjects7K[1] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-hitobject7k3":
                        NoteHitObjects7K[2] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-hitobject7k4":
                        NoteHitObjects7K[3] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-hitobject7k5":
                        NoteHitObjects7K[4] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-hitobject7k6":
                        NoteHitObjects7K[5] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-hitobject7k7":
                        NoteHitObjects7K[6] = LoadIndividualElement(element, skinElementPath);
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
                    case @"grade-small-f":
                        GradeSmallF = LoadIndividualElement(element, skinElementPath);
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
                    case @"note-holdend1":
                        NoteHoldEnds[0] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdend2":
                        NoteHoldEnds[1] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdend3":
                        NoteHoldEnds[2] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdend4":
                        NoteHoldEnds[3] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdend7k1":
                        NoteHoldEnds7K[0] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdend7k2":
                        NoteHoldEnds7K[1] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdend7k3":
                        NoteHoldEnds7K[2] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdend7k4":
                        NoteHoldEnds7K[3] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdend7k5":
                        NoteHoldEnds7K[4] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdend7k6":
                        NoteHoldEnds7K[5] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdend7k7":
                        NoteHoldEnds7K[6] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdbody1":
                        NoteHoldBodies[0] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdbody2":
                        NoteHoldBodies[1] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdbody3":
                        NoteHoldBodies[2] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdbody4":
                        NoteHoldBodies[3] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdbody7k1":
                        NoteHoldBodies7K[0] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdbody7k2":
                        NoteHoldBodies7K[1] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdbody7k3":
                        NoteHoldBodies7K[2] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdbody7k4":
                        NoteHoldBodies7K[3] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdbody7k5":
                        NoteHoldBodies7K[4] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdbody7k6":
                        NoteHoldBodies7K[5] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-holdbody7k7":
                        NoteHoldBodies7K[6] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-receptor1":
                        NoteReceptors[0] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-receptor2":
                        NoteReceptors[1] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-receptor3":
                        NoteReceptors[2] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-receptor4":
                        NoteReceptors[3] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-receptor7k1":
                        NoteReceptors7K[0] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-receptor7k2":
                        NoteReceptors7K[1] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-receptor7k3":
                        NoteReceptors7K[2] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-receptor7k4":
                        NoteReceptors7K[3] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-receptor7k5":
                        NoteReceptors7K[4] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-receptor7k6":
                        NoteReceptors7K[5] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"note-receptor7k7":
                        NoteReceptors7K[6] = LoadIndividualElement(element, skinElementPath);
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
                        Hit = LoadSoundEffectElement(element, skinElementPath);
                        break;
                    case @"combobreak":
                        ComboBreak = LoadSoundEffectElement(element, skinElementPath);
                        break;
                    case @"applause":
                        Applause = LoadSoundEffectElement(element, skinElementPath);
                        break;
                    case @"cursor":
                        Cursor = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"screenshot":
                        Screenshot = LoadSoundEffectElement(element, skinElementPath);
                        break;
                    case @"click":
                        Click = LoadSoundEffectElement(element, skinElementPath);
                        break;
                    case @"back":
                        Back = LoadSoundEffectElement(element, skinElementPath);
                        break;
                    case @"hitlighting":
                        HitLighting = LoadAnimationElements(skinDir, element, 5);
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
        ///     Loads a list of elements to be used in an animation.
        ///     Example:
        ///         - hitlighitng-0
        ///         - hitlighting-1
        ///         - hitlighting-2
        ///         //
        ///         - holdlighting-0
        ///         - holdlighting-1
        ///         - holdlighting-2
        /// </summary>
        /// <param name="skinDir"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        private List<Texture2D> LoadAnimationElements(string skinDir, string element, int defaultNum)
        {
            var animationList = new List<Texture2D>();

            // Run a loop and check if each file in the animation exists,
            for (var i = 0; File.Exists($"{skinDir}/{element}-{i}.png"); i++)
                animationList.Add(ImageLoader.Load($"{skinDir}/{element}-{i}.png"));

            // TODO: Run a check to see if the animation list has any in it.
            // If it does, then return it. If not, then we want to load the default skin's 
            // animations using the defaultNum specified.
            return animationList;
        }

        /// <summary>
        ///     Loads a sound effect element from a skin folder.
        /// </summary>
        private SoundEffect LoadSoundEffectElement(string element, string path)
        {
            //   Only load .wav files for skin elements
            path = path.Replace(".png", ".wav");

            // Load the actual file stream if it exists.
            if (File.Exists(path))
                return SoundEffect.FromStream(new FileStream(path, FileMode.Open));

            // Load the default if the path doesn't exist
            return GameBase.Content.Load<SoundEffect>(element);
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
            BgMaskPadding = ConfigHelper.ReadInt32(BgMaskPadding, data["Gameplay"]["BgMaskPadding"]);
            NotePadding = ConfigHelper.ReadInt32(NotePadding, data["Gameplay"]["NotePadding"]);
            TimingBarPixelSize = ConfigHelper.ReadInt32(TimingBarPixelSize, data["Gameplay"]["TimingBarPixelSize"]);
            HitLightingScale = ConfigHelper.ReadFloat(HitLightingScale, data["Gameplay"]["HitLightingScale"]);
            ColumnSize = ConfigHelper.ReadInt32(ColumnSize, data["Gameplay"]["ColumnSize"]);
            ColumnSize7K = ConfigHelper.ReadInt32(ColumnSize7K, data["Gameplay"]["ColumnSize7k"]);
            ReceptorYOffset = ConfigHelper.ReadInt32(ReceptorYOffset, data["Gameplay"]["ReceptorYOffset"]);
            ColumnAlignment = ConfigHelper.ReadPercentage(ColumnAlignment, data["Gameplay"]["ColumnAlignment"]);
            ColourObjectsBySnapDistance = ConfigHelper.ReadBool(ColourObjectsBySnapDistance, data["Gameplay"]["ColourObjectsBySnapDistance"]);
            LightFramesPerSecond = ConfigHelper.ReadByte(LightFramesPerSecond, data["Gameplay"]["LightsFramesPerSecond"]);
            ColourLight1 = ConfigHelper.ReadColor(ColourLight1, data["Colours"]["ColourLight1"]);
            ColourLight2 = ConfigHelper.ReadColor(ColourLight1, data["Colours"]["ColourLight2"]);
            ColourLight3 = ConfigHelper.ReadColor(ColourLight1, data["Colours"]["ColourLight3"]);
            ColourLight4 = ConfigHelper.ReadColor(ColourLight1, data["Colours"]["ColourLight4"]);
        }
    }
}
