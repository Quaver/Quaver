/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace Quaver.Shared.Screens.Gameplay.UI
{
    public class HitErrorBar : Container
    {
        /// <summary>
        ///     The middle 0ms line for the hit error bar.
        /// </summary>
        public Sprite MiddleLine { get; }

        /// <summary>
        ///     The size of the hit error object pool.
        /// </summary>
        private int PoolSize { get; } = 64;

        /// <summary>
        ///     The list of lines that are currently in the hit error.
        /// </summary>
        private HitErrorLine[] LinePool { get; }

        /// <summary>
        ///     The current index we're in within the object pool.
        ///     Initialized to -1 because we add to it each time we add a judgement.
        /// </summary>
        private int CurrentLinePoolIndex { get; set; } = -1;

        /// <summary>
        ///     the last hit chevron.
        /// </summary>
        public Sprite LastHitCheveron { get; }

        /// <summary>
        ///    The initial alpha of the hit error lines.
        /// </summary>
        public float Alpha { get; set; } = 0.5f;

        /// <inheritdoc />
        /// <summary>
        ///   Ctor -
        /// </summary>
        /// <param name="type"></param>
        /// <param name="size"></param>
        public HitErrorBar(ScalableVector2 size)
        {
            Size = size;

            MiddleLine = new Sprite()
            {
                Size = new ScalableVector2(2, 0, 0, 1),
                Alignment = Alignment.MidCenter,
                Parent = this
            };

            // Create the object pool and initialize all of the lines.
            LinePool = new HitErrorLine[PoolSize];

            _ = new HitErrorLineBatch(LinePool, MiddleLine)
            {
                Parent = this,
                Size = new ScalableVector2(0, 0, 1, 1),
                Alignment = Alignment.MidCenter
            };

            for (var i = 0; i < PoolSize; i++)
                LinePool[i].Alpha = 0;

            // Create the hit chevron.
            var chevronSize = SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].HitErrorChevronSize;

            LastHitCheveron = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Alpha = 1,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_caret_down),
                Y = -Height - 3,
                Size = new ScalableVector2(chevronSize, chevronSize),
                Tint = SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].HitErrorNeutralColor
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Gradually fade out the line.
            for (var i = 0; i < LinePool.Length; i++)
            {
                if (LinePool[i].Alpha <= 0)
                    continue;

                var alpha = MathHelper.Lerp(LinePool[i].Alpha, 0, (float)Math.Min(dt / ConfigManager.HitErrorFadeTime.Value, 1));
                LinePool[i].Alpha = alpha <= 0.001f ? 0 : alpha;
            }

            // Tween the chevron to the last hit
            if (CurrentLinePoolIndex != -1)
                LastHitCheveron.X = MathHelper.Lerp(LastHitCheveron.X, LinePool[CurrentLinePoolIndex].X, (float)Math.Min(dt / 360, 1));

            base.Update(gameTime);
        }

        /// <summary>
        ///     Adds a judgement to the hit error at a given hit time.
        /// </summary>
        public void AddJudgement(Judgement j, double hitTime)
        {
            CurrentLinePoolIndex++;

            if (CurrentLinePoolIndex >= PoolSize)
                CurrentLinePoolIndex = 0;

            LinePool[CurrentLinePoolIndex].Tint = SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].JudgeColors[j];

            LinePool[CurrentLinePoolIndex].X = -(float)hitTime / ModHelper.GetRateFromMods(ModManager.Mods) * SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].HitErrorWidthScale;
            LinePool[CurrentLinePoolIndex].Alpha = Alpha;

            if (!ConfigManager.ColorHitErrorByTiming.Value)
            {
                LastHitCheveron.Tint = Color.White;
            }
            else if (ConfigManager.HitErrorEarlyLateWindow.Value <= hitTime)
            {
                LastHitCheveron.Tint = SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].HitErrorEarlyColor;
            }
            else if (hitTime <= -ConfigManager.HitErrorEarlyLateWindow.Value)
            {
                LastHitCheveron.Tint = SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].HitErrorLateColor;
            }
            else
            {
                LastHitCheveron.Tint = SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].HitErrorNeutralColor;
            }
        }

        private struct HitErrorLine
        {
            public float X;

            public float Alpha;

            public Color Tint;
        }

        private class HitErrorLineBatch : Sprite
        {
            private const float LineWidth = 4;

            private readonly HitErrorLine[] lines;
            private readonly Sprite middleLine;

            public HitErrorLineBatch(HitErrorLine[] lines, Sprite middleLine)
            {
                this.lines = lines;
                this.middleLine = middleLine;
                Image = WobbleAssets.WhiteBox;
            }

            public override void DrawToSpriteBatch()
            {
                if (!Visible || Alpha <= 0.001f)
                    return;

                var batchAlpha = Alpha;
                var scaledLineWidth = LineWidth * Math.Abs(AbsoluteScale.X);
                var lineHeight = RenderRectangle.Height;
                var centerX = middleLine.RenderRectangle.X + middleLine.RenderRectangle.Width / 2f;
                var y = RenderRectangle.Y;

                for (var i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];

                    if (line.Alpha <= 0.001f)
                        continue;

                    var rect = new RectangleF(
                        centerX + line.X * AbsoluteScale.X - scaledLineWidth / 2f,
                        y,
                        scaledLineWidth,
                        lineHeight);

                    GameBase.Game.SpriteBatch.Draw(Image, rect, null, line.Tint * (line.Alpha * batchAlpha), SpriteOverallRotation,
                        Origin, SpriteEffect, 0f);
                }
            }
        }
    }
}
