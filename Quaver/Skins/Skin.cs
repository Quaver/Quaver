using System;
using System.Collections.Generic;
using IniParser;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting.Channels;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Quaver.API.Enums;
using Quaver.Audio;
using Quaver.Graphics.UserInterface;
using Quaver.Helpers;
using Quaver.Logging;

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
        internal string Name { get; set; }

        /// <summary>
        /// Author of the skin
        /// </summary>
        internal string Author { get; set; }

        /// <summary>
        /// Version number of the skin
        /// </summary>
        internal string Version { get; set; }

        /// <summary>
        ///     Determines the transparency of the bg masks
        /// </summary>
        internal float BgMaskAlpha { get; set; } = 0.95f;

        /// <summary>
        ///     The padding (Positional offset) of the notes relative from the bg mask.
        /// </summary>
        internal int BgMaskPadding4K { get; set; }
        internal int BgMaskPadding7K { get; set; }

        /// <summary>
        ///     The padding (Positional offset) of the notes relative from eachother.
        /// </summary>
        internal int NotePadding4K { get; set; }
        internal int NotePadding7K { get; set; }

        /// <summary>
        ///     Size of each lane in pixels.
        /// </summary>
        internal int ColumnSize4K { get; set; }
        internal int ColumnSize7K { get; set; }

        /// <summary>
        ///     Hit Position for hit objects.
        /// </summary>
        internal int HitPositionOffset4K { get; set; }
        internal int HitPositionOffset7K { get; set; }

        /// <summary>
        ///     The offset of the hit receptor
        /// </summary>
        internal int ReceptorPositionOffset4K { get; set; }
        internal int ReceptorPositionOffset7K { get; set; }

        /// <summary>
        ///     The size of the timing pars (in pixels).
        /// </summary>
        internal int TimingBarPixelSize { get; set; }

        /// <summary>
        ///     The scale of the column lighting objects.
        /// </summary>
        internal float ColumnLightingScale { get; set; }

        /// <summary>
        /// The alignment of the playfield as a percentage. 
        /// </summary>
        internal byte ColumnAlignment { get; set; }

        /// <summary>
        ///     Determines whether or not to color the HitObjects by their snap distance. 4K ONLY.
        /// </summary>
        internal bool ColourObjectsBySnapDistance { get; set; }

        /// <summary>
        ///     Determines whether or not receptors are over hit objects
        /// </summary>
        internal bool ReceptorsOverHitObjects4K { get; set; }
        internal bool ReceptorsOverHitObjects7K { get; set; }

        /// <summary>
        ///     Determines whether notes will be flipped or not when using up scroll for this skin.
        /// </summary>
        internal bool FlipNoteImagesOnUpScroll4K { get; set; }
        internal bool FlipNoteImagesOnUpScroll7K { get; set; }

        /// <summary>
        ///     Column Colors. Used for hit lighting and other stuff that depends on column colors.
        /// </summary>
        internal Color[] ColumnColors4K { get; set; } = new Color[4];
        internal Color[] ColumnColors7K { get; set; } = new Color[7];

        /// <summary>
        ///     Determines the FPS of the animations
        ///     Max - 255fps.
        /// </summary>
        internal byte LightFramesPerSecond { get; set; }

        /// <summary>
        ///     The colour that is used for the column's lighting.
        ///     [0] Marv
        ///     [1] Perf
        ///     [2] Great
        ///     [3] Good
        ///     [4] Okay
        ///     [5] Miss
        /// 
        ///     You can access an individual one by calling
        ///     Skin.GetJudgeColor(Judge color);
        ///     See: The Judge enum below.
        /// </summary>
        internal List<Color> JudgeColors { get; set; } = new List<Color>();

        /// <summary>
        ///     All of the textures for the loaded skin elements. 
        ///     We first attempt to load the selected skin's elements, however if we can't,
        ///     it'll result it to the default.
        /// </summary>
        internal Texture2D StageBgMask4K { get; set; }
        internal Texture2D StageBgMask7K { get; set; }

        /// <summary>
        ///     Lane Lighting Sprites
        /// </summary>
        internal Texture2D ColumnLighting4K { get; set; }
        internal Texture2D ColumnLighting7K { get; set; }

        /// <summary>
        ///     Timing bar QuaverSprite
        /// </summary>
        internal Texture2D StageTimingBar { get; set; }

        /// <summary>
        ///     Stage QuaverSprite. Is displayed on the left side of the stage.
        /// </summary>
        internal Texture2D StageLeftBorder { get; set; }
        internal Texture2D StageRightBorder { get; set; }
        internal Texture2D StageHitPositionOverlay { get; set; }
        internal Texture2D StageDistantOverlay { get; set; }

        // 4k - HitObjects, HoldBodies, HoldEndies, & NoteReceptors
        // defined for each key lane.
        internal List<List<Texture2D>> NoteHitObjects4K { get; set; } = new List<List<Texture2D>>();
        internal List<List<Texture2D>> NoteHoldHitObjects4K { get; set; } = new List<List<Texture2D>>();
        internal Texture2D[] NoteHoldBodies4K { get; set; } = new Texture2D[4];
        internal Texture2D[] NoteHoldEnds4K { get; set; } = new Texture2D[4];
        internal Texture2D[] NoteReceptorsUp4K { get; set; } = new Texture2D[4];
        internal Texture2D[] NoteReceptorsDown4K { get; set; } = new Texture2D[4];
        internal Texture2D[] NoteHitEffects4K { get; set; } = new Texture2D[4];

        // 7k - HitObjects, HoldBodies, HoldEndies, & NoteReceptors
        // defined for each key lane.
        internal Texture2D[] NoteHitObjects7K { get; set; } = new Texture2D[7];
        internal Texture2D[] NoteHoldBodies7K { get; set; } = new Texture2D[7];
        internal Texture2D[] NoteHoldEnds7K { get; set; } = new Texture2D[7];
        internal Texture2D[] NoteReceptorsUp7K { get; set; } = new Texture2D[7];
        internal Texture2D[] NoteReceptorsDown7K { get; set; } = new Texture2D[7];
        internal Texture2D[] NoteHitEffects7K { get; set; } = new Texture2D[7];

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

        /// <summary>
        ///     Judge
        /// </summary>
        internal Texture2D JudgeMiss { get; set; }
        internal Texture2D JudgeOkay { get; set; }
        internal Texture2D JudgeGood { get; set; }
        internal Texture2D JudgeGreat { get; set; }
        internal Texture2D JudgePerf { get; set; }
        internal Texture2D JudgeMarv { get; set; }

        /// <summary>
        ///     QuaverCursor
        /// </summary>
        internal Texture2D Cursor { get; set; }

        /// <summary>
        ///     Sound Effect elements in skin7k-note-hitobject-
        /// </summary>
        internal SoundEffect SoundHit { get; set; }
        internal SoundEffect SoundHitClap { get; set; }
        internal SoundEffect SoundHitWhistle { get; set; }
        internal SoundEffect SoundHitFinish { get; set; }
        internal SoundEffect SoundComboBreak { get; set; }
        internal SoundEffect SoundApplause { get; set; }
        internal SoundEffect SoundScreenshot { get; set; }
        internal SoundEffect SoundClick { get; set; }
        internal SoundEffect SoundBack { get; set; }

        /// <summary>
        ///     The number of files that will be loaded in the default skin
        ///     for hit burst animations.
        /// </summary>
        private int HitBurstAnimDefault { get; } = 5;

        // Contains the file names of all skin elements
        private readonly string[] skinElements = new[]
        {
                // Stage
                @"4k-stage-bgmask",
                @"7k-stage-bgmask",
                @"stage-left-border",
                @"stage-right-border",
                @"stage-hitposition-overlay",
                @"stage-distant-overlay",
                @"stage-timingbar",
                @"4k-column-lighting",
                @"7k-column-lighting",

                // 4k Hit effect
                @"4k-note-hiteffect-1",
                @"4k-note-hiteffect-2",
                @"4k-note-hiteffect-3",
                @"4k-note-hiteffect-4",

                // 7k Hit Effect
                @"7k-note-hiteffect-1",
                @"7k-note-hiteffect-2",
                @"7k-note-hiteffect-3",
                @"7k-note-hiteffect-4",
                @"7k-note-hiteffect-5",
                @"7k-note-hiteffect-6",
                @"7k-note-hiteffect-7",

                // 4k HitObjects
                @"4k-note-hitobject-1",
                @"4k-note-hitobject-2",
                @"4k-note-hitobject-3",
                @"4k-note-hitobject-4",

                // 4k LN Hit Objects
                @"4k-note-holdhitobject-1",
                @"4k-note-holdhitobject-2",
                @"4k-note-holdhitobject-3",
                @"4k-note-holdhitobject-4",

                // 7k HitObjects
                @"7k-note-hitobject-1",
                @"7k-note-hitobject-2",
                @"7k-note-hitobject-3",
                @"7k-note-hitobject-4",
                @"7k-note-hitobject-5",
                @"7k-note-hitobject-6",
                @"7k-note-hitobject-7",

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

                // 4k Hit Object Hold Ends
                @"4k-note-holdend-1",
                @"4k-note-holdend-2",
                @"4k-note-holdend-3",
                @"4k-note-holdend-4",

                // 7k Hit Object Hold Ends
                @"7k-note-holdend-1",
                @"7k-note-holdend-2",
                @"7k-note-holdend-3",
                @"7k-note-holdend-4",
                @"7k-note-holdend-5",
                @"7k-note-holdend-6",
                @"7k-note-holdend-7",

                // 4k Hit Object Hold Bodies
                @"4k-note-holdbody-1",
                @"4k-note-holdbody-2",
                @"4k-note-holdbody-3",
                @"4k-note-holdbody-4",

                // 7k Hit Object Hold Bodies
                @"7k-note-holdbody-1",
                @"7k-note-holdbody-2",
                @"7k-note-holdbody-3",
                @"7k-note-holdbody-4",
                @"7k-note-holdbody-5",
                @"7k-note-holdbody-6",
                @"7k-note-holdbody-7",

                // 4k Note Receptors
                @"4k-receptor-up-1",
                @"4k-receptor-up-2",
                @"4k-receptor-up-3",
                @"4k-receptor-up-4",

                // 4k Note Receptors Down
                @"4k-receptor-down-1",
                @"4k-receptor-down-2",
                @"4k-receptor-down-3",
                @"4k-receptor-down-4",

                // 7k Note Receptors
                @"7k-receptor-up-1",
                @"7k-receptor-up-2",
                @"7k-receptor-up-3",
                @"7k-receptor-up-4",
                @"7k-receptor-up-5",
                @"7k-receptor-up-6",
                @"7k-receptor-up-7",

                // 7k Note Receptors Down
                @"7k-receptor-down-1",
                @"7k-receptor-down-2",
                @"7k-receptor-down-3",
                @"7k-receptor-down-4",
                @"7k-receptor-down-5",
                @"7k-receptor-down-6",
                @"7k-receptor-down-7",

                // Judge
                @"judge-miss",
                @"judge-okay",
                @"judge-good",
                @"judge-great",
                @"judge-perf",
                @"judge-marv",

                //  QuaverCursor
                @"main-cursor",

                // Sound Effects
                @"sound-hit",
                @"sound-hitclap",
                @"sound-hitwhistle",
                @"sound-hitfinish",

                @"sound-combobreak",
                @"sound-applause",
                @"sound-screenshot",
                @"sound-click",
                @"sound-back",
        };

        /// <summary>
        ///     Constructor, 
        /// </summary>
        /// <param name="directory"></param>
        internal Skin(string directory)
        {
            // The skin dir
            var skinDirectory = ConfigManager.SkinDirectory + "/" + directory;

            // Read Skin.ini
            ReadSkinConfig(skinDirectory);

            // Load all skin elements
            LoadSkinElements(skinDirectory);
        }

        /// <summary>
        ///     Loads the skin defined in the config file. 
        /// </summary>
        public static void LoadSkin()
        {
            GameBase.LoadedSkin = new Skin(ConfigManager.Skin);
            GameBase.QuaverCursor = new QuaverCursor();
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
                    case @"4k-stage-bgmask":
                        StageBgMask4K = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-stage-bgmask":
                        StageBgMask7K = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"stage-timingbar":
                        StageTimingBar = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"stage-left-border":
                        StageLeftBorder = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"stage-right-border":
                        StageRightBorder = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"stage-hitposition-overlay":
                        StageHitPositionOverlay = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"stage-distant-overlay":
                        StageDistantOverlay = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-column-lighting":
                        ColumnLighting4K = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-column-lighting":
                        ColumnLighting7K = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-note-hiteffect-1":
                        NoteHitEffects4K[0] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-note-hiteffect-2":
                        NoteHitEffects4K[1] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-note-hiteffect-3":
                        NoteHitEffects4K[2] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-note-hiteffect-4":
                        NoteHitEffects4K[3] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-hiteffect-1":
                        NoteHitEffects7K[0] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-hiteffect-2":
                        NoteHitEffects7K[1] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-hiteffect-3":
                        NoteHitEffects7K[2] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-hiteffect-4":
                        NoteHitEffects7K[3] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-hiteffect-5":
                        NoteHitEffects7K[4] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-hiteffect-6":
                        NoteHitEffects7K[5] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-hiteffect-7":
                        NoteHitEffects7K[6] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-note-hitobject-1":
                        LoadHitObjects(NoteHitObjects4K, skinDir, element, 0);
                        break;
                    case @"4k-note-hitobject-2":
                        LoadHitObjects(NoteHitObjects4K, skinDir, element, 1);
                        break;
                    case @"4k-note-hitobject-3":
                        LoadHitObjects(NoteHitObjects4K, skinDir, element, 2);
                        break;
                    case @"4k-note-hitobject-4":
                        LoadHitObjects(NoteHitObjects4K, skinDir, element, 3);
                        break;
                    case @"4k-note-holdhitobject-1":
                        LoadHitObjects(NoteHoldHitObjects4K, skinDir, element, 0);
                        break;
                    case @"4k-note-holdhitobject-2":
                        LoadHitObjects(NoteHoldHitObjects4K, skinDir, element, 1);
                        break;
                    case @"4k-note-holdhitobject-3":
                        LoadHitObjects(NoteHoldHitObjects4K, skinDir, element, 2);
                        break;
                    case @"4k-note-holdhitobject-4":
                        LoadHitObjects(NoteHoldHitObjects4K, skinDir, element, 3);
                        break;
                    case @"7k-note-hitobject-1":
                        NoteHitObjects7K[0] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-hitobject-2":
                        NoteHitObjects7K[1] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-hitobject-3":
                        NoteHitObjects7K[2] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-hitobject-4":
                        NoteHitObjects7K[3] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-hitobject-5":
                        NoteHitObjects7K[4] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-hitobject-6":
                        NoteHitObjects7K[5] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-hitobject-7":
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
                    case @"4k-note-holdend-1":
                        NoteHoldEnds4K[0] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-note-holdend-2":
                        NoteHoldEnds4K[1] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-note-holdend-3":
                        NoteHoldEnds4K[2] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-note-holdend-4":
                        NoteHoldEnds4K[3] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-holdend-1":
                        NoteHoldEnds7K[0] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-holdend-2":
                        NoteHoldEnds7K[1] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-holdend-3":
                        NoteHoldEnds7K[2] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-holdend-4":
                        NoteHoldEnds7K[3] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-holdend-5":
                        NoteHoldEnds7K[4] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-holdend-6":
                        NoteHoldEnds7K[5] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-holdend-7":
                        NoteHoldEnds7K[6] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-note-holdbody-1":
                        NoteHoldBodies4K[0] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-note-holdbody-2":
                        NoteHoldBodies4K[1] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-note-holdbody-3":
                        NoteHoldBodies4K[2] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-note-holdbody-4":
                        NoteHoldBodies4K[3] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-holdbody-1":
                        NoteHoldBodies7K[0] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-holdbody-2":
                        NoteHoldBodies7K[1] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-holdbody-3":
                        NoteHoldBodies7K[2] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-holdbody-4":
                        NoteHoldBodies7K[3] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-holdbody-5":
                        NoteHoldBodies7K[4] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-holdbody-6":
                        NoteHoldBodies7K[5] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-note-holdbody-7":
                        NoteHoldBodies7K[6] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-receptor-up-1":
                        NoteReceptorsUp4K[0] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-receptor-up-2":
                        NoteReceptorsUp4K[1] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-receptor-up-3":
                        NoteReceptorsUp4K[2] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-receptor-up-4":
                        NoteReceptorsUp4K[3] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-receptor-down-1":
                        NoteReceptorsDown4K[0] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-receptor-down-2":
                        NoteReceptorsDown4K[1] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-receptor-down-3":
                        NoteReceptorsDown4K[2] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"4k-receptor-down-4":
                        NoteReceptorsDown4K[3] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-receptor-up-1":
                        NoteReceptorsUp7K[0] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-receptor-up-2":
                        NoteReceptorsUp7K[1] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-receptor-up-3":
                        NoteReceptorsUp7K[2] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-receptor-up-4":
                        NoteReceptorsUp7K[3] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-receptor-up-5":
                        NoteReceptorsUp7K[4] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-receptor-up-6":
                        NoteReceptorsUp7K[5] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-receptor-up-7":
                        NoteReceptorsUp7K[6] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-receptor-down-1":
                        NoteReceptorsDown7K[0] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-receptor-down-2":
                        NoteReceptorsDown7K[1] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-receptor-down-3":
                        NoteReceptorsDown7K[2] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-receptor-down-4":
                        NoteReceptorsDown7K[3] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-receptor-down-5":
                        NoteReceptorsDown7K[4] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-receptor-down-6":
                        NoteReceptorsDown7K[5] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"7k-receptor-down-7":
                        NoteReceptorsDown7K[6] = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"judge-miss":
                        JudgeMiss = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"judge-okay":
                        JudgeOkay = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"judge-good":
                        JudgeGood = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"judge-great":
                        JudgeGreat = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"judge-perf":
                        JudgePerf = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"judge-marv":
                        JudgeMarv = LoadIndividualElement(element, skinElementPath);
                        break;
                    case @"sound-hit":
                        SoundHit = LoadSoundEffectElement(element, skinElementPath);
                        break;
                    case @"sound-hitclap":
                        SoundHitClap = LoadSoundEffectElement(element, skinElementPath);
                        break;
                    case @"sound-hitwhistle":
                        SoundHitWhistle = LoadSoundEffectElement(element, skinElementPath);
                        break;
                    case @"sound-hitfinish":
                        SoundHitFinish = LoadSoundEffectElement(element, skinElementPath);
                        break;
                    case @"sound-combobreak":
                        SoundComboBreak = LoadSoundEffectElement(element, skinElementPath);
                        break;
                    case @"sound-applause":
                        SoundApplause = LoadSoundEffectElement(element, skinElementPath);
                        break;
                    case @"sound-screenshot":
                        SoundScreenshot = LoadSoundEffectElement(element, skinElementPath);
                        break;
                    case @"sound-click":
                        SoundClick = LoadSoundEffectElement(element, skinElementPath);
                        break;
                    case @"sound-back":
                        SoundBack = LoadSoundEffectElement(element, skinElementPath);
                        break;
                    case @"main-cursor":
                        Cursor = LoadIndividualElement(element, skinElementPath);
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
                return GraphicsHelper.LoadTexture2DFromFile(path);

            // Otherwise, we'll have to change the path to that of the default element and load that instead.
            path = element;

            // Load default skin element    
            try
            {
                // Load based on which default skin is loaded
                // prepend with 'arrow' for the file name if the arrow skin is selected.
                switch (ConfigManager.DefaultSkin)
                {
                    case DefaultSkins.Arrow:
                        path = "arrow-" + path;
                        break;
                    case DefaultSkins.Bar:
                        path = "bar-" + path;
                        break;
                }

                return GameBase.Content.Load<Texture2D>(path);
            }
            catch
            {
                Logger.LogError($"Default skin element not found {path}", LogType.Runtime);
                return GameBase.Content.Load<Texture2D>("main-blank-box");
            }    
        }

        /// <summary>
        ///     Loads the HitObjects w/ note snapping
        ///     Each hitobject lane, gets to have more images for each snap distance.
        /// 
        ///     Example:
        ///         In "note-hitobjectx-y", (x is denoted as the lane, and y is the snap)
        ///         That being said, note-hitobject3-16th, would be the object in lane 3, with 16th snap. 
        /// 
        ///         NOTE: For 1/1, objects, there is no concept of y. So the HitObject in lane 4, with 1/1 snap
        ///         would have a file name of note-hitobject4. This is so that we don't require filename changes
        ///         even though the user may not use snapping.    
        /// 
        ///         - note-hitobject1 (Lane 1 Default which is also 1/1 snap.)
        ///         - note-hitobject1-2nd (Lane 1, 1/2 Snap)
        ///         - note-hitobject1-3rd (Lane 1, 1/3 Snap)
        ///         - note-hitobject1-4th (Lane 1, 1/4 Snap)
        ///         //
        ///         - note-hitobject2 (Lane 2 Default which is also 1/1 snap.)
        ///         - note-hitobject2-2nd (Lane 2, 1/2 Snap)
        /// </summary>
        /// <param name="skinDir"></param>
        /// <param name="element"></param>
        /// <param name="defaultNum"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private void LoadHitObjects(List<List<Texture2D>> HitObjects, string skinDir, string element, int index)
        {
            var objectsList = new List<Texture2D>();

            // First load the beginning HitObject element, that doesn't require snapping.
            objectsList.Add(LoadIndividualElement(element, skinDir + $"/{element}.png"));

            // Don't bother looking for snap objects if the skin config doesn't permit it.
            if (!ColourObjectsBySnapDistance)
            {
                HitObjects.Insert(index, objectsList);
                return;
            }

            // For each snap we load the separate image for it. 
            // It HAS to be loaded in an incremental fashion. 
            // So you can't have 1/48, but not have 1/3, etc.
            var snaps = new [] { "2nd", "3rd", "4th", "6th", "8th", "12th", "16th", "48th" };

            // If it can find the appropriate files, load them.
            for (var i = 0; i < snaps.Length; i++)
                objectsList.Add(LoadIndividualElement($"{element}-{snaps[i]}", skinDir + $"/{element}-{snaps[i]}.png"));

            HitObjects.Insert(index, objectsList);
        }

        /// <summary>
        ///     Loads a list of elements to be used in an animation.
        ///     Example:
        ///         - 4k-note-hiteffect-0@0
        ///         - 4k-note-hiteffect-1@1
        ///         - 4k-note-hiteffect-2@2
        ///         //
        ///         - 4k-note-holdbody-active-1@0
        ///         - 4k-note-holdbody-active-1@1
        ///         - 4k-note-holdbody-active-1@2
        /// </summary>
        /// <param name="skinDir"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        private List<Texture2D> LoadAnimationElements(string skinDir, string element, int defaultNum)
        {
            var animationList = new List<Texture2D>();

            // Run a loop and check if each file in the animation exists,
            for (var i = 0; File.Exists($"{skinDir}/{element}@{i}.png"); i++)
                animationList.Add(GraphicsHelper.LoadTexture2DFromFile($"{skinDir}/{element}@{i}.png"));

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
            try
            {
                if (File.Exists(path))
                    return SoundEffect.FromStream(new FileStream(path, FileMode.Open));
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }

            // Load the default if the path doesn't exist
            switch (ConfigManager.DefaultSkin)
            {
                case DefaultSkins.Arrow:
                    element = "arrow-" + element;
                    break;
                case DefaultSkins.Bar:
                    element = "bar-" + element;
                    break;
            }

            return GameBase.Content.Load<SoundEffect>(element);
        }

        /// <summary>
        ///     Reads a skin.ini file/sets skin config
        /// </summary>
        /// <param name="skinDir"></param>
        private void ReadSkinConfig(string skinDir)
        {
            // Before trying to read the skin.ini file, set the defaults
            // based on the default skin loaded
            switch (ConfigManager.DefaultSkin)
            {
                case DefaultSkins.Bar:
                    Name = "Default Bar Skin";
                    Author = "Quaver Team";
                    Version = "1.0";
                    BgMaskPadding4K = 0;
                    BgMaskPadding7K = 0;
                    HitPositionOffset4K = -28;
                    HitPositionOffset7K = -28;
                    NotePadding4K = 0;
                    NotePadding7K = 0;
                    TimingBarPixelSize = 2;
                    ColumnLightingScale = 1.5f;
                    ColumnSize4K = 100;
                    ColumnSize7K = 75;
                    ReceptorPositionOffset4K = -110;
                    ReceptorPositionOffset7K = -110;
                    ColumnAlignment = 50;
                    ColourObjectsBySnapDistance = false;
                    LightFramesPerSecond = 240;
                    ReceptorsOverHitObjects4K = true;
                    ReceptorsOverHitObjects7K = true;
                    JudgeColors.Insert(0, new Color(255, 255, 200));
                    JudgeColors.Insert(1, new Color(255, 255, 0));
                    JudgeColors.Insert(2, new Color(0, 255, 0));
                    JudgeColors.Insert(3, new Color(0, 168, 255));
                    JudgeColors.Insert(4, new Color(255, 0, 255));
                    JudgeColors.Insert(5, new Color(255, 0, 0));
                    ColumnColors4K[0] = new Color(255, 138, 234);
                    ColumnColors4K[1] = new Color(126, 233, 129);
                    ColumnColors4K[2] = new Color(126, 233, 129);
                    ColumnColors4K[3] = new Color(255, 138, 234);
                    ColumnColors7K[0] = new Color(255, 138, 234);
                    ColumnColors7K[1] = new Color(126, 233, 129);
                    ColumnColors7K[2] = new Color(255, 138, 234);
                    ColumnColors7K[3] = new Color(255, 251, 138);
                    ColumnColors7K[4] = new Color(255, 138, 234);
                    ColumnColors7K[5] = new Color(126, 233, 129);
                    ColumnColors7K[6] = new Color(255, 138, 234);
                    BgMaskAlpha = 1f;
                    FlipNoteImagesOnUpScroll4K = true;
                    FlipNoteImagesOnUpScroll7K = true;
                    break;
                case DefaultSkins.Arrow:
                    Name = "Default Arrow Skin";
                    Author = "Quaver Team";
                    Version = "1.0";
                    BgMaskPadding4K = 10;
                    BgMaskPadding7K = 10;
                    HitPositionOffset4K = 0;
                    HitPositionOffset7K = 0;
                    NotePadding4K = 4;
                    NotePadding7K = 4;
                    TimingBarPixelSize = 2;
                    ColumnLightingScale = 1.0f;
                    ColumnSize4K = 95;
                    ColumnSize7K = 65;
                    ReceptorPositionOffset4K = -75;
                    ReceptorPositionOffset7K = -75;
                    ColumnAlignment = 50;
                    ColourObjectsBySnapDistance = true;
                    LightFramesPerSecond = 240;
                    ReceptorsOverHitObjects4K = false;
                    ReceptorsOverHitObjects7K = false;
                    JudgeColors.Insert(0, new Color(255, 255, 200));
                    JudgeColors.Insert(1, new Color(255, 255, 0));
                    JudgeColors.Insert(2, new Color(0, 255, 0));
                    JudgeColors.Insert(3, new Color(0, 168, 255));
                    JudgeColors.Insert(4, new Color(255, 0, 255));
                    JudgeColors.Insert(5, new Color(255, 0, 0));
                    ColumnColors4K[0] = new Color(255, 255, 255);
                    ColumnColors4K[1] = new Color(255, 255, 255);
                    ColumnColors4K[2] = new Color(255, 255, 255);
                    ColumnColors4K[3] = new Color(255, 255, 255);
                    ColumnColors7K[0] = new Color(255, 255, 255);
                    ColumnColors7K[1] = new Color(255, 255, 255);
                    ColumnColors7K[2] = new Color(255, 255, 255);
                    ColumnColors7K[3] = new Color(255, 255, 255);
                    ColumnColors7K[4] = new Color(255, 255, 255);
                    ColumnColors7K[5] = new Color(255, 255, 255);
                    ColumnColors7K[6] = new Color(255, 255, 255);
                    BgMaskAlpha = 0.9f;
                    FlipNoteImagesOnUpScroll4K = true;
                    FlipNoteImagesOnUpScroll7K = true;
                    break;
            }

            // Check if skin.ini file exists.
            if (!File.Exists(skinDir + "/skin.ini"))
                return;

            // Begin Parsing skin.ini if it does.
            var data = new FileIniDataParser().ReadFile(skinDir + "/skin.ini");
            Name = ConfigHelper.ReadString(Name, data["General"]["Name"]);
            Author = ConfigHelper.ReadString(Author, data["General"]["Author"]);
            Version = ConfigHelper.ReadString(Version, data["General"]["Version"]);
            BgMaskPadding4K = ConfigHelper.ReadInt32(BgMaskPadding4K, data["Gameplay"]["BgMaskPadding4K"]);
            BgMaskPadding7K = ConfigHelper.ReadInt32(BgMaskPadding7K, data["Gameplay"]["BgMaskPadding7K"]);
            HitPositionOffset4K = ConfigHelper.ReadInt32(HitPositionOffset4K, data["Gameplay"]["HitPositionOffset4K"]);
            HitPositionOffset7K = ConfigHelper.ReadInt32(HitPositionOffset7K, data["Gameplay"]["HitPositionOffset7K"]);
            NotePadding4K = ConfigHelper.ReadInt32(NotePadding4K, data["Gameplay"]["NotePadding4K"]);
            NotePadding7K = ConfigHelper.ReadInt32(NotePadding7K, data["Gameplay"]["NotePadding7K"]);
            TimingBarPixelSize = ConfigHelper.ReadInt32(TimingBarPixelSize, data["Gameplay"]["TimingBarPixelSize"]);
            ColumnLightingScale = ConfigHelper.ReadFloat(ColumnLightingScale, data["Gameplay"]["ColumnLightingScale"]);
            ColumnSize4K = ConfigHelper.ReadInt32(ColumnSize4K, data["Gameplay"]["ColumnSize4K"]);
            ColumnSize7K = ConfigHelper.ReadInt32(ColumnSize7K, data["Gameplay"]["ColumnSize7K"]);
            ReceptorPositionOffset4K = ConfigHelper.ReadInt32(ReceptorPositionOffset4K, data["Gameplay"]["ReceptorPositionOffset4K"]);
            ReceptorPositionOffset7K = ConfigHelper.ReadInt32(ReceptorPositionOffset7K, data["Gameplay"]["ReceptorPositionOffset7K"]);
            ColumnAlignment = ConfigHelper.ReadPercentage(ColumnAlignment, data["Gameplay"]["ColumnAlignment"]);
            ColourObjectsBySnapDistance = ConfigHelper.ReadBool(ColourObjectsBySnapDistance, data["Gameplay"]["ColourObjectsBySnapDistance"]);
            LightFramesPerSecond = ConfigHelper.ReadByte(LightFramesPerSecond, data["Gameplay"]["LightsFramesPerSecond"]);
            ReceptorsOverHitObjects4K = ConfigHelper.ReadBool(ReceptorsOverHitObjects4K, data["Gameplay"]["ReceptorsOverHitObjects4K"]);
            ReceptorsOverHitObjects7K = ConfigHelper.ReadBool(ReceptorsOverHitObjects7K, data["Gameplay"]["ReceptorsOverHitObjects7K"]);
            JudgeColors[0] = ConfigHelper.ReadColor(JudgeColors[0], data["Gameplay"]["JudgeColorMarv"]);
            JudgeColors[1] = ConfigHelper.ReadColor(JudgeColors[1], data["Gameplay"]["JudgeColorPerf"]);
            JudgeColors[2] = ConfigHelper.ReadColor(JudgeColors[2], data["Gameplay"]["JudgeColorGreat"]);
            JudgeColors[3] = ConfigHelper.ReadColor(JudgeColors[3], data["Gameplay"]["JudgeColorGood"]);
            JudgeColors[4] = ConfigHelper.ReadColor(JudgeColors[4], data["Gameplay"]["JudgeColorOkay"]);
            JudgeColors[5] = ConfigHelper.ReadColor(JudgeColors[5], data["Gameplay"]["JudgeColorMiss"]);
            ColumnColors4K[0] = ConfigHelper.ReadColor(ColumnColors4K[0], data["Gameplay"]["ColumnColor4K1"]);
            ColumnColors4K[1] = ConfigHelper.ReadColor(ColumnColors4K[1], data["Gameplay"]["ColumnColor4K2"]);
            ColumnColors4K[2] = ConfigHelper.ReadColor(ColumnColors4K[2], data["Gameplay"]["ColumnColor4K3"]);
            ColumnColors4K[3] = ConfigHelper.ReadColor(ColumnColors4K[3], data["Gameplay"]["ColumnColor4K4"]);
            ColumnColors7K[0] = ConfigHelper.ReadColor(ColumnColors7K[0], data["Gameplay"]["ColumnColor7K1"]);
            ColumnColors7K[1] = ConfigHelper.ReadColor(ColumnColors7K[1], data["Gameplay"]["ColumnColor7K2"]);
            ColumnColors7K[2] = ConfigHelper.ReadColor(ColumnColors7K[2], data["Gameplay"]["ColumnColor7K3"]);
            ColumnColors7K[3] = ConfigHelper.ReadColor(ColumnColors7K[3], data["Gameplay"]["ColumnColor7K4"]);
            ColumnColors7K[4] = ConfigHelper.ReadColor(ColumnColors7K[4], data["Gameplay"]["ColumnColor7K5"]);
            ColumnColors7K[5] = ConfigHelper.ReadColor(ColumnColors7K[5], data["Gameplay"]["ColumnColor7K6"]);
            ColumnColors7K[6] = ConfigHelper.ReadColor(ColumnColors7K[6], data["Gameplay"]["ColumnColor7K7"]);
            BgMaskAlpha = ConfigHelper.ReadFloat(BgMaskAlpha, data["Gameplay"]["BgMaskAlpha"]);
            FlipNoteImagesOnUpScroll4K = ConfigHelper.ReadBool(FlipNoteImagesOnUpScroll4K, data["Gameplay"]["FlipNoteImagesOnUpScroll4K"]);
            FlipNoteImagesOnUpScroll7K = ConfigHelper.ReadBool(FlipNoteImagesOnUpScroll7K, data["Gameplay"]["FlipNoteImagesOnUpScroll7K"]);

            Logger.LogSuccess($"skin.ini file has successfully been read.", LogType.Runtime);
        }

        /// <summary>
        ///     Gets an individual judge color from the list of Judge colors
        ///     Returns black if its unable to be found.
        /// </summary>
        /// <param name="judge"></param>
        /// <returns></returns>
        public Color GetJudgeColor(Judge judge)
        {
            return JudgeColors.Count == 0 ? new Color(0, 0, 0) : JudgeColors[(int) judge];
        }
    }
}
