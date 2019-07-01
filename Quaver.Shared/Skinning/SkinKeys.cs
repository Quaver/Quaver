/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using IniFileParser;
using IniFileParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield.Health;
using Quaver.Shared.Screens.Gameplay.UI.Health;
using Wobble;

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

        internal SortedDictionary<Judgement, Color> JudgeColors { get; private set; } = new SortedDictionary<Judgement, Color>()
        {
            {Judgement.Marv, new Color(255, 255, 200)},
            {Judgement.Perf, new Color(255, 255, 0)},
            {Judgement.Great, new Color(0, 255, 0)},
            {Judgement.Good, new Color(0, 168, 255)},
            {Judgement.Okay, new Color(255, 0, 255)},
            {Judgement.Miss, new Color(255, 0, 0)}
        };

        internal List<Color> ColumnColors { get; private set; } = new List<Color>()
        {
            Color.Transparent,
            Color.Transparent,
            Color.Transparent,
            Color.Transparent,
            Color.Transparent,
            Color.Transparent,
            Color.Transparent,
        };

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

        internal int ScoreDisplayScale { get; private set; }

        internal int RatingDisplayPosX { get; private set; }

        internal int RatingDisplayPosY { get; private set; }

        internal int RatingDisplayScale { get; private set; }

        internal int AccuracyDisplayPosX { get; private set; }

        internal int AccuracyDisplayPosY { get; private set; }

        internal int AccuracyDisplayScale { get; private set; }

        internal int KpsDisplayPosX { get; private set; }

        internal int KpsDisplayPosY { get; private set; }

        internal int KpsDisplayScale { get; private set; }

        internal int ComboPosX { get; private set; }

        internal int ComboPosY { get; private set; }

        internal int ComboDisplayScale { get; private set; }

        internal int JudgementBurstPosY { get; private set; }

        internal int HitErrorPosX { get; private set; }

        internal int HitErrorPosY { get; private set; }

        internal int HitErrorHeight { get; private set; }

        internal int HitErrorChevronSize { get; private set; }

        internal HealthBarType HealthBarType { get; private set; }

        internal HealthBarKeysAlignment HealthBarKeysAlignment { get; private set; }

        internal Color TimingLineColor { get; private set; }

        internal Color SongTimeProgressInactiveColor { get; private set; }

        internal Color SongTimeProgressActiveColor { get; private set; }

        internal int SongTimeProgressScale { get; private set; }

        internal float JudgementCounterAlpha { get; private set; }

        internal Color JudgementCounterFontColor { get; private set; }

        internal int JudgementCounterSize { get; private set; }

        internal bool DrawLongNoteEnd { get; private set; }

        internal Color DeadNoteColor { get; private set; }

        internal int BattleRoyaleAlertPosX { get; private set; }

        internal int BattleRoyaleAlertPosY { get; private set; }

        internal int BattleRoyaleAlertScale { get; private set; }

        internal int BattleRoyaleEliminatedPosX { get; private set; }

        internal int BattleRoyaleEliminatedPosY { get; private set; }

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

        /// <summary>
        /// </summary>
        internal List<Texture2D> EditorLayerNoteHitObjects { get; } = new List<Texture2D>();

        /// <summary>
        /// </summary>
        internal List<Texture2D> EditorLayerNoteHoldBodies { get; } = new List<Texture2D>();

        /// <summary>
        /// </summary>
        internal List<Texture2D> EditorLayerNoteHoldEnds { get; } = new List<Texture2D>();

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

        // ----- Lane Covers ----- //

        /// <summary>
        ///     Top lane cover texture.
        /// </summary>
        internal Texture2D LaneCoverTop { get; private set; }

        /// <summary>
        ///     Bottom lane cover texture.
        /// </summary>
        internal Texture2D LaneCoverBottom { get; private set; }

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

            // Set the generic config variables, and THEN try to read from
            // skin.ini.
            ReadConfig(true);
            ReadConfig(false);
            LoadTextures();
        }

        /// <summary>
        ///     Reads config file for skin.ini elements.
        ///
        ///     REMEMBER TO SET YOUR DEFAULTS FOR BOTH 4K AND 7K
        ///     AND ALL DEFAULT SKINS (BARS/ARROWS)
        /// </summary>
        private void ReadConfig(bool loadFromResources)
        {
            IniData config;

            if (loadFromResources)
            {
                using (var stream = new StreamReader(GameBase.Game.Resources.GetStream($"Quaver.Resources/Textures/Skins/{ConfigManager.DefaultSkin.Value}/skin.ini")))
                    config = new IniFileParser.IniFileParser(new ConcatenateDuplicatedKeysIniDataParser()).ReadData(stream);
            }
            else
            {
                if (Store.Config == null)
                    return;

                config = Store.Config;
            }

            var ini = config[ModeHelper.ToShortHand(Mode).ToUpper()];

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
            FlipNoteEndImagesOnUpscroll = ConfigHelper.ReadBool(FlipNoteEndImagesOnUpscroll, ini["FlipNoteEndImagesOnUpscroll"]);
            HitLightingY = ConfigHelper.ReadInt32(HitLightingY, ini["HitLightingY"]);
            HitLightingX = ConfigHelper.ReadInt32(HitLightingX, ini["HitLightingX"]);
            HitLightingFps = ConfigHelper.ReadInt32(HitLightingFps, ini["HitLightingFps"]);
            HoldLightingFps = ConfigHelper.ReadInt32(HoldLightingFps, ini["HoldLightingFps"]);
            HitLightingWidth = ConfigHelper.ReadInt32(HitLightingWidth, ini["HitLightingWidth"]);
            HitLightingHeight = ConfigHelper.ReadInt32(HitLightingHeight, ini["HitLightingHeight"]);
            ScoreDisplayPosX = ConfigHelper.ReadInt32(ScoreDisplayPosX, ini["ScoreDisplayPosX"]);
            ScoreDisplayPosY = ConfigHelper.ReadInt32(ScoreDisplayPosY, ini["ScoreDisplayPosY"]);
            RatingDisplayPosX = ConfigHelper.ReadInt32(RatingDisplayPosX, ini["RatingDisplayPosX"]);
            RatingDisplayPosY = ConfigHelper.ReadInt32(RatingDisplayPosY, ini["RatingDisplayPosY"]);
            AccuracyDisplayPosX = ConfigHelper.ReadInt32(AccuracyDisplayPosX, ini["AccuracyDisplayPosX"]);
            AccuracyDisplayPosY = ConfigHelper.ReadInt32(AccuracyDisplayPosY, ini["AccuracyDisplayPosY"]);
            KpsDisplayPosX = ConfigHelper.ReadInt32(KpsDisplayPosX, ini["KpsDisplayPosX"]);
            KpsDisplayPosY = ConfigHelper.ReadInt32(KpsDisplayPosY, ini["KpsDisplayPosY"]);
            ComboPosX = ConfigHelper.ReadInt32(ComboPosX, ini["ComboPosX"]);
            ComboPosY = ConfigHelper.ReadInt32(ComboPosY, ini["ComboPosY"]);
            JudgementBurstPosY = ConfigHelper.ReadInt32(JudgementBurstPosY, ini["JudgementBurstPosY"]);
            HealthBarType = ConfigHelper.ReadEnum(HealthBarType, ini["HealthBarType"]);
            HealthBarKeysAlignment = ConfigHelper.ReadEnum(HealthBarKeysAlignment, ini["HealthBarKeysAlignment"]);
            HitErrorPosX = ConfigHelper.ReadInt32(HitErrorPosX, ini["HitErrorPosX"]);
            HitErrorPosY = ConfigHelper.ReadInt32(HitErrorPosY, ini["HitErrorPosY"]);
            HitErrorHeight = ConfigHelper.ReadInt32(HitErrorHeight, ini["HitErrorHeight"]);
            HitErrorChevronSize = ConfigHelper.ReadInt32(HitErrorChevronSize, ini["HitErrorChevronSize"]);
            TimingLineColor = ConfigHelper.ReadColor(TimingLineColor, ini["TimingLineColor"]);
            SongTimeProgressInactiveColor = ConfigHelper.ReadColor(SongTimeProgressInactiveColor, ini["SongTimeProgressInactiveColor"]);
            SongTimeProgressActiveColor = ConfigHelper.ReadColor(SongTimeProgressActiveColor, ini["SongTimeProgressActiveColor"]);
            JudgementCounterAlpha = ConfigHelper.ReadFloat(JudgementCounterAlpha, ini["JudgementCounterAlpha"]);
            JudgementCounterFontColor = ConfigHelper.ReadColor(JudgementCounterFontColor, ini["JudgementCounterFontColor"]);
            JudgementCounterSize = ConfigHelper.ReadInt32(JudgementCounterSize, ini["JudgementCounterSize"]);
            DrawLongNoteEnd = ConfigHelper.ReadBool(DrawLongNoteEnd, ini["DrawLongNoteEnd"]);
            ScoreDisplayScale = ConfigHelper.ReadInt32(ScoreDisplayScale, ini["ScoreDisplayScale"]);
            RatingDisplayScale = ConfigHelper.ReadInt32(RatingDisplayScale, ini["RatingDisplayScale"]);
            AccuracyDisplayScale = ConfigHelper.ReadInt32(AccuracyDisplayScale, ini["AccuracyDisplayScale"]);
            ComboDisplayScale = ConfigHelper.ReadInt32(ComboDisplayScale, ini["ComboDisplayScale"]);
            KpsDisplayScale = ConfigHelper.ReadInt32(KpsDisplayScale, ini["KpsDisplayScale"]);
            SongTimeProgressScale = ConfigHelper.ReadInt32(SongTimeProgressScale, ini["SongTimeProgressScale"]);
            DeadNoteColor = ConfigHelper.ReadColor(DeadNoteColor, ini["DeadNoteColor"]);
            BattleRoyaleAlertPosX = ConfigHelper.ReadInt32(BattleRoyaleAlertPosX, ini["BattleRoyaleAlertPosX"]);
            BattleRoyaleAlertPosY = ConfigHelper.ReadInt32(BattleRoyaleAlertPosY, ini["BattleRoyaleAlertPosY"]);
            BattleRoyaleAlertScale = ConfigHelper.ReadInt32(BattleRoyaleAlertScale, ini["BattleRoyaleAlertScale"]);
            BattleRoyaleEliminatedPosX = ConfigHelper.ReadInt32(BattleRoyaleEliminatedPosX, ini["BattleRoyaleEliminatedPosX"]);
            BattleRoyaleEliminatedPosY = ConfigHelper.ReadInt32(BattleRoyaleEliminatedPosY, ini["BattleRoyaleEliminatedPosY"]);
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

            #region LANECOVER
            LaneCoverTop = LoadTexture(SkinKeysFolder.LaneCover, "cover-top", false);
            LaneCoverBottom = LoadTexture(SkinKeysFolder.LaneCover, "cover-bottom", false);
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
                resource = $"Quaver.Resources/Textures/Skins/{ConfigManager.DefaultSkin.Value.ToString()}/{Mode.ToString()}/{folder.ToString()}" +
                               $"/{GetResourcePath(element)}.png";
            }

            var folderName = shared ? folder.ToString() : $"/{ModeHelper.ToShortHand(Mode).ToLower()}/{folder.ToString()}";
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
                resource = $"Quaver.Resources/Textures/Skins/{ConfigManager.DefaultSkin.Value.ToString()}/{Mode.ToString()}/{folder.ToString()}" +
                           $"/{GetResourcePath(element)}";
            }

            var folderName = shared ? folder.ToString() : $"/{ModeHelper.ToShortHand(Mode).ToLower()}/{folder.ToString()}/";
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
        ///     Gets a file name in our resource store.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private string GetResourcePath(string element) => $"{element}";

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
                    ColumnColors[i] = ConfigHelper.ReadColor(ColumnColors[i], Store.Config[ModeHelper.ToShortHand(Mode).ToUpper()][$"ColumnColor{i + 1}"]);

                // HitObjects
                LoadHitObjects(NoteHitObjects, $"note-hitobject-{i + 1}", i);
                LoadHitObjects(NoteHoldHitObjects, $"note-holdhitobject-{i + 1}", i);

                // LNS
                NoteHoldBodies.Add(LoadSpritesheet(SkinKeysFolder.HitObjects, $"note-holdbody-{i + 1}", false, 0, 0));
                NoteHoldEnds.Add(LoadTexture(SkinKeysFolder.HitObjects, $"note-holdend-{i + 1}", false));

                // Receptors
                NoteReceptorsUp.Add(LoadTexture(SkinKeysFolder.Receptors, $"receptor-up-{i + 1}", false));
                NoteReceptorsDown.Add(LoadTexture(SkinKeysFolder.Receptors, $"receptor-down-{i + 1}", false));

                // Editor
                EditorLayerNoteHitObjects.Add(LoadTexture(SkinKeysFolder.Editor, $"note-hitobject-{i + 1}", false));
                EditorLayerNoteHoldBodies.Add(LoadTexture(SkinKeysFolder.Editor, $"note-holdbody-{i + 1}", false));
                EditorLayerNoteHoldEnds.Add(LoadTexture(SkinKeysFolder.Editor, $"note-holdend-{i + 1}", false));
            }
        }
    }
}
