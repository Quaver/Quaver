/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield.Health;
using Quaver.Shared.Screens.Gameplay.UI.Health;

namespace Quaver.Shared.Skinning
{
    public class SkinKeys
    {
                /// <summary>
        ///     Reference to the
        /// </summary>
        private SkinStore Store { get; }

        /// <summary>
        ///     The game mode this skin is for.
        /// </summary>
        private GameMode Mode { get; }

        /// <summary>
        ///    The string that's prepended before each file name.
        /// </summary>
        private string ShortName { get; }

        /// <summary>
        ///     Value thats prepended before file names in the resource store.
        /// </summary>
        private string ResourceFilePrepender { get; set; }

#region SKIN.INI VALUES

        internal int StageReceptorPadding { get; private set; }

        internal int HitPosOffsetY { get; private set; }

        internal int NotePadding { get; private set; }

        internal int TimingBarPixelSize { get; private set; }

        internal float ColumnLightingScale { get; private set; }

        internal int ColumnLightingOffsetY { get; private set; }

        internal int ColumnSize { get; private set; }

        internal int ReceptorPosOffsetY { get; private set; }

        internal int ColumnAlignment { get; private set; }

        internal bool ColorObjectsBySnapDistance { get; private set; }

        internal byte JudgementHitBurstScale { get; private set; }

        internal bool ReceptorsOverHitObjects { get; private set; }

        internal SortedDictionary<Judgement, Color> JudgeColors { get; private set; }

        internal List<Color> ColumnColors { get; private set; } = new List<Color>();

        internal float BgMaskAlpha { get; private set;  }

        internal bool FlipNoteImagesOnUpscroll { get; private set; }

        internal bool FlipNoteEndImagesOnUpscroll { get; private set; }

        internal int HitLightingX { get; private set; }

        internal int HitLightingY { get; private set; }

        internal int HitLightingFps { get; private set; }

        internal int HoldLightingFps { get; private set; }

        internal int HitLightingWidth { get; private set; }

        internal int HitLightingHeight { get; private set; }

        internal int ScoreDisplayPosX { get; private set; }

        internal int ScoreDisplayPosY { get; private set; }

        internal int AccuracyDisplayPosX { get; private set; }

        internal int AccuracyDisplayPosY { get; private set; }

        internal int KpsDisplayPosX { get; private set; }

        internal int KpsDisplayPosY { get; private set; }

        internal int ComboPosY { get; private set; }

        internal int JudgementBurstPosY { get; private set; }

        internal int HitErrorPosX { get; private set; }

        internal int HitErrorPosY { get; private set; }

        internal int HitErrorHeight { get; private set; }

        internal int HitErrorChevronSize { get; private set; }

        internal HealthBarType HealthBarType { get; private set; }

        internal HealthBarKeysAlignment HealthBarKeysAlignment { get; private set; }

#endregion

#region TEXTURES

        // ----- Column ----- //
        /// <summary>
        ///
        /// </summary>
        internal Texture2D ColumnLighting { get; private set; }

        // ----- Stage ----- //

        /// <summary>
        ///
        /// </summary>
        internal Texture2D StageBgMask { get; private set; }

        /// <summary>
        ///
        /// </summary>
        internal Texture2D StageTimingBar { get; private set; }

        /// <summary>
        ///
        /// </summary>
        internal Texture2D StageLeftBorder { get; private set; }

        /// <summary>
        ///
        /// </summary>
        internal Texture2D StageRightBorder { get; private set; }

        /// <summary>
        ///
        /// </summary>
        internal Texture2D StageHitPositionOverlay { get; private set; }

        /// <summary>
        ///
        /// </summary>
        internal Texture2D StageDistantOverlay { get; private set; }

        // ----- HitObjects ----- //

        /// <summary>
        ///
        /// </summary>
        internal List<List<Texture2D>> NoteHitObjects { get; } = new List<List<Texture2D>>();

        /// <summary>
        ///
        /// </summary>
        internal List<List<Texture2D>> NoteHoldHitObjects { get; } = new List<List<Texture2D>>();

        /// <summary>
        ///
        /// </summary>
        internal List<List<Texture2D>> NoteHoldBodies { get;} = new List<List<Texture2D>>();

        /// <summary>
        ///
        /// </summary>
        internal List<Texture2D> NoteHoldEnds { get; } = new List<Texture2D>();

        // ----- Receptors ----- //

        /// <summary>
        ///
        /// </summary>
        internal List<Texture2D> NoteReceptorsUp { get; } = new List<Texture2D>();

        /// <summary>
        ///
        /// </summary>
        internal List<Texture2D> NoteReceptorsDown { get; } = new List<Texture2D>();

        // ----- Hitlighting ----- //

        /// <summary>
        ///
        /// </summary>
        internal List<Texture2D> HitLighting { get; private set; } = new List<Texture2D>();

        /// <summary>
        ///
        /// </summary>
        internal List<Texture2D> HoldLighting { get; private set; } = new List<Texture2D>();

#endregion

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="store"></param>
        /// <param name="mode"></param>
        internal SkinKeys(SkinStore store, GameMode mode)
        {
            Store = store;
            Mode = mode;

            switch (Mode)
            {
                case GameMode.Keys4:
                    ShortName = "4k";
                    SetDefault4KConfig();
                    break;
                case GameMode.Keys7:
                    ShortName = "7k";
                    SetDefault7KConfig();
                    break;
                default:
                    throw new InvalidEnumArgumentException($"SkinKeys can only be instantiated with: {GameMode.Keys4} or {GameMode.Keys7}. Got {Mode}");
            }

            // Set the generic config variables, and THEN try to read from
            // skin.ini.
            SetGenericConfig();
            ReadConfig();
            LoadTextures();
        }

        /// <summary>
        ///     Sets config values based on the selected default skin.
        /// </summary>
        private void SetGenericConfig()
        {
            switch (ConfigManager.DefaultSkin.Value)
            {
                case DefaultSkins.Arrow:
                    ResourceFilePrepender = "arrow";
                    break;
                case DefaultSkins.Bar:
                    ResourceFilePrepender = "bar";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            JudgeColors = new SortedDictionary<Judgement, Color>
            {
                {Judgement.Marv, new Color(255, 255, 200)},
                {Judgement.Perf, new Color(255, 255, 0)},
                {Judgement.Great, new Color(0, 255, 0)},
                {Judgement.Good, new Color(0, 168, 255)},
                {Judgement.Okay, new Color(255, 0, 255)},
                {Judgement.Miss, new Color(255, 0, 0)}
            };
        }

        /// <summary>
        ///     Sets all of the default values for 4K.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void SetDefault4KConfig()
        {
            switch (ConfigManager.DefaultSkin.Value)
            {
                case DefaultSkins.Bar:
                    StageReceptorPadding = 0;
                    HitPosOffsetY = 15;
                    NotePadding = 0;
                    TimingBarPixelSize = 2;
                    ColumnLightingScale = 1f;
                    ColumnLightingOffsetY = 0;
                    ColumnSize = 110;
                    ReceptorPosOffsetY = 0;
                    ColumnAlignment = 0;
                    ColorObjectsBySnapDistance = false;
                    JudgementHitBurstScale = 150;
                    ReceptorsOverHitObjects = true;
                    ColumnColors = new List<Color>()
                    {
                        Color.DarkGray,
                        Colors.MainAccentInactive,
                        Colors.MainAccentInactive,
                        Color.DarkGray
                    };
                    BgMaskAlpha = 1f;
                    FlipNoteImagesOnUpscroll = true;
                    FlipNoteEndImagesOnUpscroll = true;
                    HitLightingY = 0;
                    HitLightingX = 0;
                    HitLightingFps = 60;
                    HoldLightingFps = 60;
                    HitLightingWidth = 0;
                    HitLightingHeight = 0;
                    ScoreDisplayPosX = 10;
                    ScoreDisplayPosY = 5;
                    AccuracyDisplayPosX = -10;
                    AccuracyDisplayPosY = 5;
                    KpsDisplayPosX = -10;
                    KpsDisplayPosY = 10;
                    ComboPosY = -40;
                    JudgementBurstPosY = 108;
                    HealthBarType = HealthBarType.Vertical;
                    HealthBarKeysAlignment = HealthBarKeysAlignment.RightStage;
                    HitErrorPosX = 0;
                    HitErrorPosY = 55;
                    HitErrorHeight = 10;
                    HitErrorChevronSize = 8;
                    break;
                case DefaultSkins.Arrow:
                    StageReceptorPadding = 10;
                    HitPosOffsetY = 105;
                    NotePadding = 8;
                    TimingBarPixelSize = 2;
                    ColumnLightingScale = 1.0f;
                    ColumnLightingOffsetY = 0;
                    ColumnSize = 105;
                    ReceptorPosOffsetY = 10;
                    ColumnAlignment = 0;
                    ColorObjectsBySnapDistance = true;
                    JudgementHitBurstScale = 150;
                    ReceptorsOverHitObjects = false;
                    ColumnColors = new List<Color>
                    {
                        new Color(255, 255, 255),
                        new Color(255, 255, 255),
                        new Color(255, 255, 255),
                        new Color(255, 255, 255)
                    };
                    BgMaskAlpha = 0.9f;
                    FlipNoteImagesOnUpscroll = false;
                    FlipNoteEndImagesOnUpscroll = true;
                    HitLightingY = 0;
                    HitLightingX = 0;
                    HitLightingFps = 60;
                    HoldLightingFps = 60;
                    HitLightingWidth = 0;
                    HitLightingHeight = 0;
                    ScoreDisplayPosX = 10;
                    ScoreDisplayPosY = 5;
                    AccuracyDisplayPosX = -10;
                    AccuracyDisplayPosY = 5;
                    KpsDisplayPosX = -10;
                    KpsDisplayPosY = 10;
                    ComboPosY = -40;
                    JudgementBurstPosY = 108;
                    HealthBarType = HealthBarType.Vertical;
                    HealthBarKeysAlignment = HealthBarKeysAlignment.RightStage;
                    HitErrorPosX = 0;
                    HitErrorPosY = 55;
                    HitErrorHeight = 10;
                    HitErrorChevronSize = 8;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Sets all of the default values for 7K
        /// </summary>
        private void SetDefault7KConfig()
        {
            switch (ConfigManager.DefaultSkin.Value)
            {
                case DefaultSkins.Bar:
                    StageReceptorPadding = 0;
                    HitPosOffsetY = 15;
                    NotePadding = 0;
                    TimingBarPixelSize = 2;
                    ColumnLightingScale = 1f;
                    ColumnLightingOffsetY = 0;
                    ColumnSize = 85;
                    ReceptorPosOffsetY = 0;
                    ColumnAlignment = 0;
                    ColorObjectsBySnapDistance = false;
                    JudgementHitBurstScale = 150;
                    ReceptorsOverHitObjects = true;
                    ColumnColors = new List<Color>
                    {
                        Color.DarkGray,
                        Colors.MainAccentInactive,
                        Color.DarkGray,
                        Colors.SecondaryAccentInactive,
                        Color.DarkGray,
                        Colors.MainAccentInactive,
                        Color.DarkGray,
                    };
                    BgMaskAlpha = 1f;
                    FlipNoteImagesOnUpscroll = true;
                    FlipNoteEndImagesOnUpscroll = true;
                    HitLightingY = 0;
                    HitLightingX = 0;
                    HitLightingFps = 60;
                    HoldLightingFps = 60;
                    HitLightingWidth = 0;
                    HitLightingHeight = 0;
                    ScoreDisplayPosX = 10;
                    ScoreDisplayPosY = 5;
                    AccuracyDisplayPosX = -10;
                    AccuracyDisplayPosY = 5;
                    KpsDisplayPosX = -10;
                    KpsDisplayPosY = 10;
                    ComboPosY = -40;
                    JudgementBurstPosY = 108;
                    HealthBarType = HealthBarType.Vertical;
                    HealthBarKeysAlignment = HealthBarKeysAlignment.RightStage;
                    HitErrorPosX = 0;
                    HitErrorPosY = 55;
                    HitErrorHeight = 10;
                    HitErrorChevronSize = 8;
                    break;
                case DefaultSkins.Arrow:
                    StageReceptorPadding = 10;
                    HitPosOffsetY = 86;
                    NotePadding = 8;
                    TimingBarPixelSize = 2;
                    ColumnLightingScale = 1.0f;
                    ColumnLightingOffsetY = 0;
                    ColumnSize = 85;
                    ReceptorPosOffsetY = 10;
                    ColumnAlignment = 0;
                    ColorObjectsBySnapDistance = true;
                    JudgementHitBurstScale = 150;
                    ReceptorsOverHitObjects = false;
                    ColumnColors = new List<Color>
                    {
                        new Color(255, 255, 255),
                        new Color(255, 255, 255),
                        new Color(255, 255, 255),
                        new Color(255, 255, 255),
                        new Color(255, 255, 255),
                        new Color(255, 255, 255),
                        new Color(255, 255, 255),
                        new Color(255, 255, 255)
                    };
                    BgMaskAlpha = 0.9f;
                    FlipNoteImagesOnUpscroll = false;
                    FlipNoteEndImagesOnUpscroll = true;
                    HitLightingY = 0;
                    HitLightingX = 0;
                    HitLightingFps = 60;
                    HoldLightingFps = 60;
                    HitLightingWidth = 0;
                    HitLightingHeight = 0;
                    ScoreDisplayPosX = 10;
                    ScoreDisplayPosY = 5;
                    AccuracyDisplayPosX = -10;
                    AccuracyDisplayPosY = 5;
                    KpsDisplayPosX = -10;
                    KpsDisplayPosY = 10;
                    ComboPosY = -40;
                    JudgementBurstPosY = 108;
                    HealthBarType = HealthBarType.Vertical;
                    HealthBarKeysAlignment = HealthBarKeysAlignment.RightStage;
                    HitErrorPosX = 0;
                    HitErrorPosY = 55;
                    HitErrorHeight = 10;
                    HitErrorChevronSize = 8;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Reads config file for skin.ini elements.
        ///
        ///     REMEMBER TO SET YOUR DEFAULTS FOR BOTH 4K AND 7K
        ///     AND ALL DEFAULT SKINS (BARS/ARROWS)
        /// </summary>
        private void ReadConfig()
        {
            if (Store.Config == null)
                return;

            var ini = Store.Config[ShortName.ToUpper()];

            StageReceptorPadding = ConfigHelper.ReadInt32(StageReceptorPadding, ini["StageReceptorPadding"]);
            HitPosOffsetY = ConfigHelper.ReadInt32(HitPosOffsetY, ini["HitPosOffsetY"]);
            NotePadding = ConfigHelper.ReadInt32(NotePadding, ini["NotePadding"]);
            TimingBarPixelSize = ConfigHelper.ReadInt32(TimingBarPixelSize, ini["TimingBarPixelSize"]);
            ColumnLightingScale = ConfigHelper.ReadFloat(ColumnLightingScale, ini["ColumnLightingScale"]);
            ColumnLightingOffsetY = ConfigHelper.ReadInt32(ColumnLightingOffsetY, ini["ColumnLightingOffsetY"]);
            ColumnSize = ConfigHelper.ReadInt32(ColumnSize, ini["ColumnSize"]);
            ReceptorPosOffsetY = ConfigHelper.ReadInt32(ReceptorPosOffsetY, ini["ReceptorPosOffsetY"]);
            ColumnAlignment = ConfigHelper.ReadInt32(ColumnAlignment, ini["ColumnAlignment"]);
            ColorObjectsBySnapDistance = ConfigHelper.ReadBool(ColorObjectsBySnapDistance, ini["ColorObjectsBySnapDistance"]);
            JudgementHitBurstScale = ConfigHelper.ReadByte(JudgementHitBurstScale, ini["JudgementHitBurstScale"]);
            ReceptorsOverHitObjects = ConfigHelper.ReadBool(ReceptorsOverHitObjects, ini["ReceptorsOverHitObjects"]);
            JudgeColors[Judgement.Marv] = ConfigHelper.ReadColor(JudgeColors[Judgement.Marv], ini["JudgeColorMarv"]);
            JudgeColors[Judgement.Perf] = ConfigHelper.ReadColor(JudgeColors[Judgement.Perf], ini["JudgeColorPerf"]);
            JudgeColors[Judgement.Great] = ConfigHelper.ReadColor(JudgeColors[Judgement.Great], ini["JudgeColorGreat"]);
            JudgeColors[Judgement.Good] = ConfigHelper.ReadColor(JudgeColors[Judgement.Good], ini["JudgeColorGood"]);
            JudgeColors[Judgement.Okay] = ConfigHelper.ReadColor(JudgeColors[Judgement.Okay], ini["JudgeColorOkay"]);
            JudgeColors[Judgement.Miss] = ConfigHelper.ReadColor(JudgeColors[Judgement.Miss], ini["JudgeColorMiss"]);
            BgMaskAlpha = ConfigHelper.ReadFloat(BgMaskAlpha, ini["BgMaskAlpha"]);
            FlipNoteImagesOnUpscroll = ConfigHelper.ReadBool(FlipNoteImagesOnUpscroll, ini["FlipNoteImagesOnUpscroll"]);
            FlipNoteEndImagesOnUpscroll = ConfigHelper.ReadBool(FlipNoteEndImagesOnUpscroll, ini["FlipNoteEndImageOnUpscroll"]);
            HitLightingY = ConfigHelper.ReadInt32(HitLightingY, ini["HitLightingY"]);
            HitLightingX = ConfigHelper.ReadInt32(HitLightingX, ini["HitLightingX"]);
            HitLightingFps = ConfigHelper.ReadInt32(HitLightingFps, ini["HitLightingFps"]);
            HoldLightingFps = ConfigHelper.ReadInt32(HoldLightingFps, ini["HoldLightingFps"]);
            HitLightingWidth = ConfigHelper.ReadInt32(HitLightingWidth, ini["HitLightingWidth"]);
            HitLightingHeight = ConfigHelper.ReadInt32(HitLightingHeight, ini["HitLightingHeight"]);
            ScoreDisplayPosX = ConfigHelper.ReadInt32(ScoreDisplayPosX, ini["ScoreDisplayPosX"]);
            ScoreDisplayPosY = ConfigHelper.ReadInt32(ScoreDisplayPosY, ini["ScoreDisplayPosY"]);
            AccuracyDisplayPosX = ConfigHelper.ReadInt32(AccuracyDisplayPosX, ini["AccuracyDisplayPosX"]);
            AccuracyDisplayPosY = ConfigHelper.ReadInt32(AccuracyDisplayPosY, ini["AccuracyDisplayPosY"]);
            KpsDisplayPosX = ConfigHelper.ReadInt32(KpsDisplayPosX, ini["KpsDisplayPosX"]);
            KpsDisplayPosY = ConfigHelper.ReadInt32(KpsDisplayPosY, ini["KpsDisplayPosY"]);
            ComboPosY = ConfigHelper.ReadInt32(ComboPosY, ini["ComboPosY"]);
            JudgementBurstPosY = ConfigHelper.ReadInt32(JudgementBurstPosY, ini["JudgementBurstPosY"]);
            HealthBarType = ConfigHelper.ReadHealthBarType(HealthBarType, ini["HealthBarType"]);
            HealthBarKeysAlignment = ConfigHelper.ReadHealthBarKeysAlignment(HealthBarKeysAlignment, ini["HealthBarKeysAlignment"]);
            HitErrorPosX = ConfigHelper.ReadInt32(HitErrorPosX, ini["HitErrorPosX"]);
            HitErrorPosY = ConfigHelper.ReadInt32(HitErrorPosY, ini["HitErrorPosY"]);
            HitErrorHeight = ConfigHelper.ReadInt32(HitErrorHeight, ini["HitErrorHeight"]);
            HitErrorChevronSize = ConfigHelper.ReadInt32(HitErrorChevronSize, ini["HitErrorChevronSize"]);
        }

        /// <summary>
        ///     Loads skin element textures.
        /// </summary>
        private void LoadTextures()
        {
            #region LIGHTING
            ColumnLighting = LoadTexture(SkinKeysFolder.Lighting, "column-lighting", false);
            HitLighting = LoadSpritesheet(SkinKeysFolder.Lighting, "hitlighting", false, 0, 0);
            HoldLighting = LoadSpritesheet(SkinKeysFolder.Lighting, "holdlighting", false, 0, 0);
            #endregion

            #region STAGE
            StageBgMask = LoadTexture(SkinKeysFolder.Stage, "stage-bgmask", false);
            StageTimingBar = LoadTexture(SkinKeysFolder.Stage, "stage-timingbar", false);
            StageLeftBorder = LoadTexture(SkinKeysFolder.Stage, "stage-left-border", false);
            StageRightBorder = LoadTexture(SkinKeysFolder.Stage, "stage-right-border", false);
            StageHitPositionOverlay = LoadTexture(SkinKeysFolder.Stage, "stage-hitposition-overlay", false);
            StageDistantOverlay = LoadTexture(SkinKeysFolder.Stage, "stage-distant-overlay", false);
            #endregion

            #region MISC
            LoadLaneSpecificElements();
            #endregion
        }

        /// <summary>
        ///     Loads an individual skin element.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="element"></param>
        /// <param name="shared">If the resource is shared between key modes.</param>
        /// <param name="extension"></param>
        /// <returns></returns>
        private Texture2D LoadTexture(SkinKeysFolder folder, string element, bool shared, string extension = ".png")
        {
            string resource;
            if (shared)
            {
                resource = $"Quaver.Resources/Textures/Skins/Shared/{folder.ToString()}/{element}.png";
            }
            else
            {
                resource = $"Quaver.Resources/Textures/Skins/{ConfigManager.DefaultSkin.Value.ToString()}/{folder.ToString()}" +
                               $"/{Mode.ToString()}/{GetResourcePath(element)}.png";
            }

            var folderName = shared ? folder.ToString() : $"/{ShortName}/{folder.ToString()}";
            return SkinStore.LoadSingleTexture($"{SkinStore.Dir}/{folderName}/{element}", resource);
        }

        /// <summary>
        ///     Loads a spritesheet
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="element"></param>
        /// <param name="shared">If the resource is shared between key modes.</param>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        private List<Texture2D> LoadSpritesheet(SkinKeysFolder folder, string element, bool shared, int rows, int columns, string extension = ".png")
        {
            string resource;
            if (shared)
            {
                resource = $"Quaver.Resources/Textures/Skins/Shared/{folder.ToString()}/{element}";
            }
            else
            {
                resource = $"Quaver.Resources/Textures/Skins/{ConfigManager.DefaultSkin.Value.ToString()}/{folder.ToString()}" +
                           $"/{Mode.ToString()}/{GetResourcePath(element)}";
            }

            var folderName = shared ? folder.ToString() : $"/{ShortName}/{folder.ToString()}/";
            return SkinStore.LoadSpritesheet(folderName, element, resource, rows, columns, extension);
        }

        /// <summary>
        ///     Loads the HitObjects w/ note snapping
        ///     Each hitobject lane, gets to have more images for each snap distance.
        ///
        ///     Example:
        ///         In "note-hitobjectx-y", (x is denoted as the lane, and y is the snap)
        ///         That being said, note-hitobject3-16th, would be the object in lane 3, with 16th snap.
        ///
        ///         NOTE: For 1/1, objects, there is no concept of y. So the ManiaHitObject in lane 4, with 1/1 snap
        ///         would have a file name of note-hitobject4. This is so that we don't require filename changes
        ///         even though the user may not use snapping.
        ///
        ///         - note-hitobject-1 (Lane 1 Default which is also 1/1 snap.)
        ///         - note-hitobject-1-2nd (Lane 1, 1/2 Snap)
        ///         - note-hitobject-1-3rd (Lane 1, 1/3 Snap)
        ///         - note-hitobject-1-4th (Lane 1, 1/4 Snap)
        ///         //
        ///         - note-hitobject-2 (Lane 2 Default which is also 1/1 snap.)
        ///         - note-hitobject-2-2nd (Lane 2, 1/2 Snap)
        /// </summary>
        /// <param name="hitObjects"></param>
        /// <param name="element"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private void LoadHitObjects(IList<List<Texture2D>> hitObjects, string element, int index)
        {
            // First load the beginning HitObject element that doesn't require snapping.
            var objectsList = new List<Texture2D> {LoadTexture(SkinKeysFolder.HitObjects, element, false)};

            // Don't bother looking for snap objects if the skin config doesn't permit it.
            if (!ColorObjectsBySnapDistance)
            {
                hitObjects.Insert(index, objectsList);
                return;
            }

            // For each snap we load the separate image for it.
            // It HAS to be loaded in an incremental fashion.
            // So you can't have 1/48, but not have 1/3, etc.
            var snaps = new [] { "2nd", "3rd", "4th", "6th", "8th", "12th", "16th", "48th" };


            // If it can find the appropriate files, load them.
            objectsList.AddRange(snaps.Select(snap => LoadTexture(SkinKeysFolder.HitObjects, $"{element}-{snap}", false)));

            hitObjects.Insert(index, objectsList);
        }

        /// <summary>
        ///     Gets a skin element's path.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="element"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        private string GetElementPath(SkinKeysFolder folder, string element, string ext) => $"{SkinStore.Dir}/{ShortName}/{folder}/{element}{ext}";

        /// <summary>
        ///     Gets a file name in our resource store that is shared between all keys modes.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private string GetModeSharedResourcePath(string element) => $"{ResourceFilePrepender}-{element}";

        /// <summary>
        ///     Gets a file name in our resource store.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private string GetResourcePath(string element) => $"{ResourceFilePrepender}-{ShortName}-{element}";

        /// <summary>
        ///     Loads elements that rely on the lane.
        /// </summary>
        private void LoadLaneSpecificElements()
        {
            for (var i = 0; i < 7; i++)
            {
                if (i == 4 && Mode == GameMode.Keys4)
                    break;

                // Column Colors
                if (Store.Config != null)
                    ColumnColors[i] = ConfigHelper.ReadColor(ColumnColors[i], Store.Config[ShortName.ToUpper()][$"ColumnColor{i + 1}"]);

                // HitObjects
                LoadHitObjects(NoteHitObjects, $"note-hitobject-{i + 1}", i);
                LoadHitObjects(NoteHoldHitObjects, $"note-holdhitobject-{i + 1}", i);

                // LNS
                NoteHoldBodies.Add(LoadSpritesheet(SkinKeysFolder.HitObjects, $"note-holdbody-{i + 1}", false, 0, 0));
                NoteHoldEnds.Add(LoadTexture(SkinKeysFolder.HitObjects, $"note-holdend-{i + 1}", false));

                // Receptors
                NoteReceptorsUp.Add(LoadTexture(SkinKeysFolder.Receptors, $"receptor-up-{i + 1}", false));
                NoteReceptorsDown.Add(LoadTexture(SkinKeysFolder.Receptors, $"receptor-down-{i + 1}", false));
            }
        }
    }
}
