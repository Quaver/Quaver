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
using System.Reflection;
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

        /// <summary>
        /// </summary>
        private string DefaultSkin { get; set; }

        #region SKIN.INI VALUES

        [FixedScale]
        internal float StageReceptorPadding { get; private set; }

        [FixedScale]
        internal float HitPosOffsetY { get; set; }

        [FixedScale]
        internal float NotePadding { get; private set; }

        // Not FixedScale because it's used as a factor in an expression with another, already scaled, attribute.
        internal float ColumnLightingScale { get; private set; }

        [FixedScale]
        internal float ColumnLightingOffsetY { get; private set; }

        [FixedScale]
        internal float ColumnSize { get; private set; }

        [FixedScale]
        internal float ReceptorPosOffsetY { get; private set; }

        [FixedScale]
        internal float ColumnAlignment { get; private set; }

        internal bool ColorObjectsBySnapDistance { get; private set; }

        [FixedScale]
        internal float JudgementHitBurstScale { get; private set; }

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
            Color.Transparent,
        };

        internal float BgMaskAlpha { get; private set;  }

        internal bool FlipNoteImagesOnUpscroll { get; private set; }

        internal bool FlipNoteEndImagesOnUpscroll { get; private set; }

        [FixedScale]
        internal float HitLightingX { get; private set; }

        [FixedScale]
        internal float HitLightingY { get; private set; }

        internal int HitLightingFps { get; private set; }

        internal int HoldLightingFps { get; private set; }

        internal int HitLightingScale { get; private set; } = 100;

        internal int HoldLightingScale { get; private set; } = 100;

        internal bool HitLightingColumnRotation { get; private set; }

        internal bool HoldLightingColumnRotation { get; private set; }

        [FixedScale]
        internal float ScoreDisplayPosX { get; private set; }

        [FixedScale]
        internal float ScoreDisplayPosY { get; private set; }

        [FixedScale]
        internal float ScoreDisplayScale { get; private set; }

        [FixedScale]
        internal float RatingDisplayPosX { get; private set; }

        [FixedScale]
        internal float RatingDisplayPosY { get; private set; }

        [FixedScale]
        internal float RatingDisplayScale { get; private set; }

        [FixedScale]
        internal float AccuracyDisplayPosX { get; private set; }

        [FixedScale]
        internal float AccuracyDisplayPosY { get; private set; }

        [FixedScale]
        internal float AccuracyDisplayScale { get; private set; }

        [FixedScale]
        internal float KpsDisplayPosX { get; private set; }

        [FixedScale]
        internal float KpsDisplayPosY { get; private set; }

        [FixedScale]
        internal float KpsDisplayScale { get; private set; }

        [FixedScale]
        internal float ComboPosX { get; private set; }

        [FixedScale]
        internal float ComboPosY { get; private set; }

        [FixedScale]
        internal float ComboDisplayScale { get; private set; }

        [FixedScale]
        internal float JudgementBurstPosY { get; private set; }

        internal bool DisplayJudgementsInEachColumn { get; private set; }

        internal bool RotateJudgements { get; private set; }

        [FixedScale]
        internal float HitErrorPosX { get; private set; }

        [FixedScale]
        internal float HitErrorPosY { get; private set; }

        [FixedScale]
        internal float HitErrorHeight { get; private set; }

        [FixedScale]
        internal float HitErrorChevronSize { get; private set; }

        internal HealthBarType HealthBarType { get; private set; }

        internal HealthBarKeysAlignment HealthBarKeysAlignment { get; private set; }

        [FixedScale]
        internal float HealthBarScale{ get; private set; }

        internal Color TimingLineColor { get; private set; }

        internal Color SongTimeProgressInactiveColor { get; private set; }

        internal Color SongTimeProgressActiveColor { get; private set; }

        [FixedScale]
        internal float SongTimeProgressScale { get; private set; }

        internal bool SongTimeProgressPositionAtTop { get; private set; }

        internal float JudgementCounterAlpha { get; private set; }

        internal Color JudgementCounterFontColor { get; private set; }

        internal bool UseJudgementColorForNumbers { get; private set; }

        [FixedScale]
        internal float JudgementCounterSize { get; private set; }

        [FixedScale]
        internal float JudgementCounterPosX { get; private set; }

        [FixedScale]
        internal float JudgementCounterPosY { get; private set; }

        internal float JudgementCounterPadding { get; private set; }

        internal bool JudgementCounterHorizontal { get; private set; }

        internal bool JudgementCounterFadeToAlpha { get; private set; }

        internal bool DrawLongNoteEnd { get; private set; }

        internal Color DeadNoteColor { get; private set; }

        [FixedScale]
        internal float BattleRoyaleAlertPosX { get; private set; }

        [FixedScale]
        internal float BattleRoyaleAlertPosY { get; private set; }

        [FixedScale]
        internal float BattleRoyaleAlertScale { get; private set; }

        [FixedScale]
        internal float BattleRoyaleEliminatedPosX { get; private set; }

        [FixedScale]
        internal float BattleRoyaleEliminatedPosY { get; private set; }

        [FixedScale]
        internal float HealthBarPosOffsetX { get; private set; }

        [FixedScale]
        internal float HealthBarPosOffsetY { get; private set; }

        internal bool UseHitObjectSheet { get; private set; }

        [FixedScale]
        internal float ScratchLaneSize { get; private set; }

        internal bool RotateHitObjectsByColumn { get; private set; }

        internal int JudgementHitBurstFps { get; private set; }

        [FixedScale]
        internal int WidthForNoteHeightScale { get; private set; }

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
            DefaultSkin = ConfigManager.DefaultSkin?.Value.ToString() ?? DefaultSkins.Bar.ToString();

            // Set the generic config variables, and THEN try to read from
            // skin.ini.
            ReadConfig(false);
            ReadConfig(true);
            ReadConfig(false);

            FixScale();
            FixValues();
            LoadTextures();
        }

        /// <summary>
        ///     Scale all necessary skin keys to match the UI redesign coordinate system.
        /// </summary>
        private void FixScale()
        {
            var properties = GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(info => Attribute.IsDefined(info, typeof(FixedScale)));
            foreach (var property in properties)
            {
                var value = property.GetValue(this);
                if (value is float f)
                {
                    property.SetValue(this, f * QuaverGame.SkinScalingFactor);
                }
            }
        }

        /// <summary>
        ///     Fixes any invalid values
        /// </summary>
        private void FixValues()
        {
            if (ScratchLaneSize <= 0)
                ScratchLaneSize = ColumnSize;
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
                using (var stream = new StreamReader(GameBase.Game.Resources.GetStream($"Quaver.Resources/Textures/Skins/{DefaultSkin}/skin.ini")))
                    config = new IniFileParser.IniFileParser(new ConcatenateDuplicatedKeysIniDataParser()).ReadData(stream);
            }
            else
            {
                if (Store.Config == null)
                    return;

                config = Store.Config;
            }

            var ini = config[ModeHelper.ToShortHand(Mode).ToUpper()];

            StageReceptorPadding = ConfigHelper.ReadInt32((int) StageReceptorPadding, ini["StageReceptorPadding"]);
            HitPosOffsetY = ConfigHelper.ReadInt32((int) HitPosOffsetY, ini["HitPosOffsetY"]);
            NotePadding = ConfigHelper.ReadInt32((int) NotePadding, ini["NotePadding"]);
            ColumnLightingScale = ConfigHelper.ReadFloat(ColumnLightingScale, ini["ColumnLightingScale"]);
            ColumnLightingOffsetY = ConfigHelper.ReadInt32((int) ColumnLightingOffsetY, ini["ColumnLightingOffsetY"]);
            ColumnSize = ConfigHelper.ReadInt32((int) ColumnSize, ini["ColumnSize"]);
            ReceptorPosOffsetY = ConfigHelper.ReadInt32((int) ReceptorPosOffsetY, ini["ReceptorPosOffsetY"]);
            ColumnAlignment = ConfigHelper.ReadInt32((int) ColumnAlignment, ini["ColumnAlignment"]);
            ColorObjectsBySnapDistance = ConfigHelper.ReadBool(ColorObjectsBySnapDistance, ini["ColorObjectsBySnapDistance"]);
            JudgementHitBurstScale = ConfigHelper.ReadByte((byte) JudgementHitBurstScale, ini["JudgementHitBurstScale"]);
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
            HitLightingY = ConfigHelper.ReadInt32((int) HitLightingY, ini["HitLightingY"]);
            HitLightingX = ConfigHelper.ReadInt32((int) HitLightingX, ini["HitLightingX"]);
            HitLightingFps = ConfigHelper.ReadInt32(HitLightingFps, ini["HitLightingFps"]);
            HoldLightingFps = ConfigHelper.ReadInt32(HoldLightingFps, ini["HoldLightingFps"]);
            HitLightingScale = ConfigHelper.ReadInt32(HitLightingScale, ini["HitLightingScale"]);
            HoldLightingScale = ConfigHelper.ReadInt32(HitLightingScale, ini["HoldLightingScale"]);
            HitLightingColumnRotation = ConfigHelper.ReadBool(HitLightingColumnRotation, ini["HitLightingColumnRotation"]);
            HoldLightingColumnRotation = ConfigHelper.ReadBool(HoldLightingColumnRotation, ini["HoldLightingColumnRotation"]);
            ScoreDisplayPosX = ConfigHelper.ReadInt32((int) ScoreDisplayPosX, ini["ScoreDisplayPosX"]);
            ScoreDisplayPosY = ConfigHelper.ReadInt32((int) ScoreDisplayPosY, ini["ScoreDisplayPosY"]);
            RatingDisplayPosX = ConfigHelper.ReadInt32((int) RatingDisplayPosX, ini["RatingDisplayPosX"]);
            RatingDisplayPosY = ConfigHelper.ReadInt32((int) RatingDisplayPosY, ini["RatingDisplayPosY"]);
            AccuracyDisplayPosX = ConfigHelper.ReadInt32((int) AccuracyDisplayPosX, ini["AccuracyDisplayPosX"]);
            AccuracyDisplayPosY = ConfigHelper.ReadInt32((int) AccuracyDisplayPosY, ini["AccuracyDisplayPosY"]);
            KpsDisplayPosX = ConfigHelper.ReadInt32((int) KpsDisplayPosX, ini["KpsDisplayPosX"]);
            KpsDisplayPosY = ConfigHelper.ReadInt32((int) KpsDisplayPosY, ini["KpsDisplayPosY"]);
            ComboPosX = ConfigHelper.ReadInt32((int) ComboPosX, ini["ComboPosX"]);
            ComboPosY = ConfigHelper.ReadInt32((int) ComboPosY, ini["ComboPosY"]);
            JudgementBurstPosY = ConfigHelper.ReadInt32((int) JudgementBurstPosY, ini["JudgementBurstPosY"]);
            DisplayJudgementsInEachColumn = ConfigHelper.ReadBool(DisplayJudgementsInEachColumn, ini["DisplayJudgementsInEachColumn"]);
            RotateJudgements = ConfigHelper.ReadBool(RotateJudgements, ini["RotateJudgements"]);
            HealthBarType = ConfigHelper.ReadEnum(HealthBarType, ini["HealthBarType"]);
            HealthBarKeysAlignment = ConfigHelper.ReadEnum(HealthBarKeysAlignment, ini["HealthBarKeysAlignment"]);
            HealthBarScale = ConfigHelper.ReadInt32((int) HealthBarScale, ini["HealthBarScale"]);
            HitErrorPosX = ConfigHelper.ReadInt32((int) HitErrorPosX, ini["HitErrorPosX"]);
            HitErrorPosY = ConfigHelper.ReadInt32((int) HitErrorPosY, ini["HitErrorPosY"]);
            HitErrorHeight = ConfigHelper.ReadInt32((int) HitErrorHeight, ini["HitErrorHeight"]);
            HitErrorChevronSize = ConfigHelper.ReadInt32((int) HitErrorChevronSize, ini["HitErrorChevronSize"]);
            TimingLineColor = ConfigHelper.ReadColor(TimingLineColor, ini["TimingLineColor"]);
            SongTimeProgressInactiveColor = ConfigHelper.ReadColor(SongTimeProgressInactiveColor, ini["SongTimeProgressInactiveColor"]);
            SongTimeProgressActiveColor = ConfigHelper.ReadColor(SongTimeProgressActiveColor, ini["SongTimeProgressActiveColor"]);
            JudgementCounterAlpha = ConfigHelper.ReadFloat(JudgementCounterAlpha, ini["JudgementCounterAlpha"]);
            JudgementCounterFontColor = ConfigHelper.ReadColor(JudgementCounterFontColor, ini["JudgementCounterFontColor"]);
            UseJudgementColorForNumbers = ConfigHelper.ReadBool(UseJudgementColorForNumbers, ini["UseJudgementColorForNumbers"]);
            JudgementCounterSize = ConfigHelper.ReadInt32((int) JudgementCounterSize, ini["JudgementCounterSize"]);
            JudgementCounterPosX = ConfigHelper.ReadInt32((int) JudgementCounterPosX, ini["JudgementCounterPosX"]);
            JudgementCounterPosY = ConfigHelper.ReadInt32((int) JudgementCounterPosY, ini["JudgementCounterPosY"]);
            JudgementCounterPadding = ConfigHelper.ReadInt32((int) JudgementCounterPadding, ini["JudgementCounterPadding"]);
            JudgementCounterHorizontal = ConfigHelper.ReadBool(JudgementCounterHorizontal, ini["JudgementCounterHorizontal"]);
            JudgementCounterFadeToAlpha = ConfigHelper.ReadBool(JudgementCounterFadeToAlpha, ini["JudgementCounterFadeToAlpha"]);
            DrawLongNoteEnd = ConfigHelper.ReadBool(DrawLongNoteEnd, ini["DrawLongNoteEnd"]);
            ScoreDisplayScale = ConfigHelper.ReadInt32((int) ScoreDisplayScale, ini["ScoreDisplayScale"]);
            RatingDisplayScale = ConfigHelper.ReadInt32((int) RatingDisplayScale, ini["RatingDisplayScale"]);
            AccuracyDisplayScale = ConfigHelper.ReadInt32((int) AccuracyDisplayScale, ini["AccuracyDisplayScale"]);
            ComboDisplayScale = ConfigHelper.ReadInt32((int) ComboDisplayScale, ini["ComboDisplayScale"]);
            KpsDisplayScale = ConfigHelper.ReadInt32((int) KpsDisplayScale, ini["KpsDisplayScale"]);
            SongTimeProgressScale = ConfigHelper.ReadInt32((int) SongTimeProgressScale, ini["SongTimeProgressScale"]);
            SongTimeProgressPositionAtTop = ConfigHelper.ReadBool(SongTimeProgressPositionAtTop, ini["SongTimeProgressPositionAtTop"]);
            DeadNoteColor = ConfigHelper.ReadColor(DeadNoteColor, ini["DeadNoteColor"]);
            BattleRoyaleAlertPosX = ConfigHelper.ReadInt32((int) BattleRoyaleAlertPosX, ini["BattleRoyaleAlertPosX"]);
            BattleRoyaleAlertPosY = ConfigHelper.ReadInt32((int) BattleRoyaleAlertPosY, ini["BattleRoyaleAlertPosY"]);
            BattleRoyaleAlertScale = ConfigHelper.ReadInt32((int) BattleRoyaleAlertScale, ini["BattleRoyaleAlertScale"]);
            BattleRoyaleEliminatedPosX = ConfigHelper.ReadInt32((int) BattleRoyaleEliminatedPosX, ini["BattleRoyaleEliminatedPosX"]);
            BattleRoyaleEliminatedPosY = ConfigHelper.ReadInt32((int) BattleRoyaleEliminatedPosY, ini["BattleRoyaleEliminatedPosY"]);
            HealthBarPosOffsetX = ConfigHelper.ReadInt32((int) HealthBarPosOffsetX, ini["HealthBarPosOffsetX"]);
            HealthBarPosOffsetY = ConfigHelper.ReadInt32((int) HealthBarPosOffsetY, ini["HealthBarPosOffsetY"]);
            UseHitObjectSheet = ConfigHelper.ReadBool(UseHitObjectSheet, ini["UseHitObjectSheet"]);
            ScratchLaneSize = ConfigHelper.ReadFloat(ScratchLaneSize, ini["ScratchLaneSize"]);
            RotateHitObjectsByColumn = ConfigHelper.ReadBool(RotateHitObjectsByColumn, ini["RotateHitObjectsByColumn"]);
            JudgementHitBurstFps = ConfigHelper.ReadInt32(JudgementHitBurstFps, ini["JudgementHitBurstFps"]);
            WidthForNoteHeightScale = ConfigHelper.ReadInt32(WidthForNoteHeightScale, ini["WidthForNoteHeightScale"]);

            var defaultSkin = ini["DefaultSkin"];

            if (!string.IsNullOrEmpty(defaultSkin) && Enum.IsDefined(typeof(DefaultSkins), defaultSkin))
                DefaultSkin = defaultSkin;
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
                resource = $"Quaver.Resources/Textures/Skins/{DefaultSkin}/{Mode.ToString()}/{folder.ToString()}" +
                               $"/{GetResourcePath(element)}.png";
            }

            var folderName = shared ? folder.ToString() : $"/{ModeHelper.ToShortHand(Mode).ToLower()}/{folder.ToString()}";
            return SkinStore.LoadSingleTexture($"{Store.Dir}/{folderName}/{element}", resource);
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
                resource = $"Quaver.Resources/Textures/Skins/{DefaultSkin}/{Mode.ToString()}/{folder.ToString()}" +
                           $"/{GetResourcePath(element)}";
            }

            var folderName = shared ? folder.ToString() : $"/{ModeHelper.ToShortHand(Mode).ToLower()}/{folder.ToString()}/";
            return Store.LoadSpritesheet(folderName, element, resource, rows, columns, extension);
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
            for (var i = 0; i < 8; i++)
            {
                if (i == 5 && Mode == GameMode.Keys4)
                    break;

                // Column Colors
                if (Store.Config != null)
                    ColumnColors[i] = ConfigHelper.ReadColor(ColumnColors[i], Store.Config[ModeHelper.ToShortHand(Mode).ToUpper()][$"ColumnColor{i + 1}"]);

                // HitObjects
                if (!UseHitObjectSheet)
                {
                    LoadHitObjects(NoteHitObjects, $"note-hitobject-{i + 1}", i);
                    LoadHitObjects(NoteHoldHitObjects, $"note-holdhitobject-{i + 1}", i);
                }
                else
                {
                    const int snapCount = 9;

                    var objects = LoadSpritesheet(SkinKeysFolder.HitObjects, "note-hitobject-sheet", false, snapCount, 1);

                    NoteHitObjects.Add(objects);
                    NoteHoldHitObjects.Add(objects);


                    for (var j = 0; j < snapCount - NoteHitObjects[i].Count; j++)
                        NoteHitObjects[i].Add(NoteHitObjects[i].Last());

                    for (var j = 0; j < snapCount - NoteHoldHitObjects[i].Count; j++)
                        NoteHoldHitObjects[i].Add(NoteHoldHitObjects[i].Last());
                }

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
