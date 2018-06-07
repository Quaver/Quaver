using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Config;
using Quaver.States.Gameplay.GameModes.Keys.Playfield.Health;
using Quaver.States.Gameplay.UI.Components.Health;

namespace Quaver.Skinning
{
    internal class SkinKeys
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

#region SKIN.INI VALUES

        /// <summary>
        /// 
        /// </summary>
        internal int BgMaskPadding { get; private set; }

        /// <summary>
        ///     
        /// </summary>
        internal int HitPositionOffsetY { get; private set; }

        /// <summary>
        ///     
        /// </summary>
        internal int NotePadding { get; private set; }

        /// <summary>
        ///     
        /// </summary>
        internal int TimingBarPixelSize { get; private set; }

        /// <summary>
        ///     
        /// </summary>
        internal float ColumnLightingScale { get; private set; }

        /// <summary>
        ///     
        /// </summary>
        internal int ColumnSize { get; private set; }

        /// <summary>
        ///     
        /// </summary>
        internal int ReceptorPositionOffsetY { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        internal byte ColumnAlignment { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        internal bool ColorObjectsBySnapDistance { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        internal byte JudgementHitBurstScale { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        internal bool ReceptorsOverHitObjects { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        internal SortedDictionary<Judgement, Color> JudgeColors { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        internal List<Color> ColumnColors { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        internal float BgMaskAlpha { get; private set;  }

        /// <summary>
        /// 
        /// </summary>
        internal bool FlipNoteImagesOnUpscroll { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        internal int HitLightingY { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        internal int HitLightingWidth { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        internal int HitLightingHeight { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        internal int ScoreDisplayPosX { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        internal int ScoreDisplayPosY { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        internal int AccuracyDisplayPosX { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        internal int AccuracyDisplayPosY { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        internal int KpsDisplayPosX { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        internal int KpsDisplayPosY { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        internal int ComboPosY { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        internal int JudgementBurstPosY { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        internal HealthBarType HealthBarType { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        internal HealthBarKeysAlignment HealthBarKeysAlignment { get; private set; }
        
        //
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
        }

        /// <summary>
        ///     Sets config values based on the selected default skin.
        /// </summary>
        private void SetGenericConfig()
        {
            Store.Author = "Quaver Team";
            Store.Version = "v0.1";
            
            switch (ConfigManager.DefaultSkin.Value)
            {
                case DefaultSkins.Arrow:
                    Store.Name = "Default Arrow Skin";
                    break;
                case DefaultSkins.Bar:
                    Store.Name = "Default Bar Skin";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            JudgeColors = new SortedDictionary<Judgement, Color>
            {
                {Judgement.Marvelous, new Color(255, 255, 200)},
                {Judgement.Perfect, new Color(255, 255, 0)},
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
                    BgMaskPadding = 0;
                    HitPositionOffsetY= 0;
                    NotePadding = 0;
                    TimingBarPixelSize = 2;
                    ColumnLightingScale = 1.5f;
                    ColumnSize = 100;
                    ReceptorPositionOffsetY = -110;
                    ColumnAlignment = 50;
                    ColorObjectsBySnapDistance = false;
                    JudgementHitBurstScale = 150;
                    ReceptorsOverHitObjects = true;
                    ColumnColors = new List<Color>()
                    {
                        new Color(255, 138, 234), 
                        new Color(126, 233, 129),
                        new Color(126, 233, 129),
                        new Color(255, 138, 234)
                    };
                    BgMaskAlpha = 1f;
                    FlipNoteImagesOnUpscroll = true;
                    HitLightingY = 0;
                    HitLightingWidth = 0;
                    HitLightingHeight = 0;
                    ScoreDisplayPosX = 10;
                    ScoreDisplayPosY = 5;
                    AccuracyDisplayPosX = -10;
                    AccuracyDisplayPosY = 5;
                    KpsDisplayPosX = -10;
                    KpsDisplayPosY = 10;
                    ComboPosY = 0;
                    JudgementBurstPosY = 105;
                    HealthBarType = HealthBarType.Vertical;
                    HealthBarKeysAlignment = HealthBarKeysAlignment.RightStage;
                    break;
                case DefaultSkins.Arrow:
                    BgMaskPadding = 10;
                    HitPositionOffsetY = 0;
                    NotePadding = 4;
                    TimingBarPixelSize = 2;
                    ColumnLightingScale = 1.0f;
                    ColumnSize = 95;
                    ReceptorPositionOffsetY = 10;
                    ColumnAlignment = 50;
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
                    FlipNoteImagesOnUpscroll = true;
                    HitLightingY = 0;
                    HitLightingWidth = 0;
                    HitLightingHeight = 0;
                    ScoreDisplayPosX = 10;
                    ScoreDisplayPosY = 5;
                    AccuracyDisplayPosX = -10;
                    AccuracyDisplayPosY = 5;
                    KpsDisplayPosX = -10;
                    KpsDisplayPosY = 10;
                    ComboPosY = 0;
                    JudgementBurstPosY = 105;
                    HealthBarType = HealthBarType.Vertical;
                    HealthBarKeysAlignment = HealthBarKeysAlignment.RightStage;
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
                    BgMaskPadding = 0;
                    HitPositionOffsetY = 0;
                    NotePadding = 0;
                    TimingBarPixelSize = 2;
                    ColumnLightingScale = 1.5f;
                    ColumnSize = 75;
                    ReceptorPositionOffsetY = -110;
                    ColumnAlignment = 50;
                    ColorObjectsBySnapDistance = false;
                    JudgementHitBurstScale = 150;
                    ReceptorsOverHitObjects = true;
                    ColumnColors = new List<Color>
                    {
                        new Color(255, 138, 234),
                        new Color(126, 233, 129),
                        new Color(255, 138, 234),
                        new Color(255, 251, 138),
                        new Color(255, 138, 234),
                        new Color(126, 233, 129),
                        new Color(255, 138, 234)
                    };
                    BgMaskAlpha = 1f;
                    FlipNoteImagesOnUpscroll = true;
                    HitLightingY = 0;
                    HitLightingWidth = 0;
                    HitLightingHeight = 0;
                    ScoreDisplayPosX = 10;
                    ScoreDisplayPosY = 5;
                    AccuracyDisplayPosX = -10;
                    AccuracyDisplayPosY = 5;
                    KpsDisplayPosX = -10;
                    KpsDisplayPosY = 10;
                    ComboPosY = 0;
                    JudgementBurstPosY = 105;
                    HealthBarType = HealthBarType.Vertical;
                    HealthBarKeysAlignment = HealthBarKeysAlignment.RightStage;
                    break;
                case DefaultSkins.Arrow:
                    BgMaskPadding = 10;
                    HitPositionOffsetY = 0;
                    NotePadding = 4;
                    TimingBarPixelSize = 2;
                    ColumnLightingScale = 1.0f;
                    ColumnSize = 65;
                    ReceptorPositionOffsetY = 0;
                    ColumnAlignment = 50;
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
                        new Color(255, 255, 255)
                    };
                    BgMaskAlpha = 0.9f;
                    FlipNoteImagesOnUpscroll = true;
                    HitLightingY = 0;
                    HitLightingWidth = 0;
                    HitLightingHeight = 0;
                    ScoreDisplayPosX = 10;
                    ScoreDisplayPosY = 5;
                    AccuracyDisplayPosX = -10;
                    AccuracyDisplayPosY = 5;
                    KpsDisplayPosX = -10;
                    KpsDisplayPosY = 10;
                    ComboPosY = 0;
                    JudgementBurstPosY = 105;
                    HealthBarType = HealthBarType.Vertical;
                    HealthBarKeysAlignment = HealthBarKeysAlignment.RightStage;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Reads config file for skin.ini elements.
        /// </summary>
        private void ReadConfig()
        {
            if (Store.Config == null)
                return;
            
            
            var ini = Store.Config[ShortName.ToUpper()];
            
            BgMaskPadding = ConfigHelper.ReadInt32(BgMaskPadding, ini["BgMaskPadding"]);
            HitPositionOffsetY = ConfigHelper.ReadInt32(HitPositionOffsetY, ini["HitPositionOffsetY"]);
            NotePadding = ConfigHelper.ReadInt32(NotePadding, ini["NotePadding"]);
            TimingBarPixelSize = ConfigHelper.ReadInt32(TimingBarPixelSize, ini["TimingBarPixelSize"]);
            ColumnLightingScale = ConfigHelper.ReadFloat(ColumnLightingScale, ini["ColumnLightingScale"]);
            ColumnSize = ConfigHelper.ReadInt32(ColumnSize, ini["ColumnSize"]);
            ReceptorPositionOffsetY = ConfigHelper.ReadInt32(ReceptorPositionOffsetY, ini["ReceptorPositionOffsetY"]);
            ColumnAlignment = ConfigHelper.ReadPercentage(ColumnAlignment, ini["ColumnAlignment"]);
            ColorObjectsBySnapDistance = ConfigHelper.ReadBool(ColorObjectsBySnapDistance, ini["ColorObjectsBySnapDistance"]);
            JudgementHitBurstScale = ConfigHelper.ReadByte(JudgementHitBurstScale, ini["JudgementHitBurstScale"]);
            ReceptorsOverHitObjects = ConfigHelper.ReadBool(ReceptorsOverHitObjects, ini["ReceptorsOverHitObjects"]);
            JudgeColors[Judgement.Marvelous] = ConfigHelper.ReadColor(JudgeColors[Judgement.Marvelous], ini["JudgeColorMarv"]);
            JudgeColors[Judgement.Perfect] = ConfigHelper.ReadColor(JudgeColors[Judgement.Perfect], ini["JudgeColorPerf"]);
            JudgeColors[Judgement.Great] = ConfigHelper.ReadColor(JudgeColors[Judgement.Great], ini["JudgeColorGreat"]);
            JudgeColors[Judgement.Good] = ConfigHelper.ReadColor(JudgeColors[Judgement.Good], ini["JudgeColorGood"]);
            JudgeColors[Judgement.Okay] = ConfigHelper.ReadColor(JudgeColors[Judgement.Okay], ini["JudgeColorOkay"]);
            JudgeColors[Judgement.Miss] = ConfigHelper.ReadColor(JudgeColors[Judgement.Miss], ini["JudgeColorMiss"]);
            ColumnColors[0] = ConfigHelper.ReadColor(ColumnColors[0], ini["ColumnColor1"]);
            ColumnColors[1] = ConfigHelper.ReadColor(ColumnColors[1], ini["ColumnColor2"]);
            ColumnColors[2] = ConfigHelper.ReadColor(ColumnColors[2], ini["ColumnColor3"]);
            ColumnColors[3] = ConfigHelper.ReadColor(ColumnColors[3], ini["ColumnColor4"]);
            if (Mode == GameMode.Keys7)
            {
                ColumnColors[4] = ConfigHelper.ReadColor(ColumnColors[4], ini["ColumnColor5"]);
                ColumnColors[5] = ConfigHelper.ReadColor(ColumnColors[5], ini["ColumnColor6"]);
                ColumnColors[6] = ConfigHelper.ReadColor(ColumnColors[6], ini["ColumnColor7"]);
            }
            BgMaskAlpha = ConfigHelper.ReadFloat(BgMaskAlpha, ini["BgMaskAlpha"]);
            FlipNoteImagesOnUpscroll = ConfigHelper.ReadBool(FlipNoteImagesOnUpscroll, ini["FlipNoteImagesOnUpscroll"]);
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
        }
    }
}