/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace Quaver.Shared.Screens.Gameplay.UI
{
    public class SongInformation : Sprite
    {
        /// <summary>
        /// Reference to the actual gameplay screen.
        /// </summary>
        private GameplayScreen Screen { get; }

        /// <summary>
        ///     Text that says "watching" when watching a replay.
        /// </summary>
        private SpriteTextPlus Watching { get; }

        /// <summary>
        ///     The title of the song
        /// </summary>
        private SpriteTextPlus Title { get; }

        /// <summary>
        ///     The difficulty name of the song
        /// </summary>
        private SpriteTextPlus Difficulty { get; }

        /// <summary>
        ///     The creator of the map
        /// </summary>
        private SpriteTextPlus Creator { get; }

        /// <summary>
        ///     The map's difficulty rating
        /// </summary>
        private SpriteTextPlus Rating { get; }

        /// <summary>
        ///     The activated mods.
        /// </summary>
        private SpriteTextPlus Mods { get; }

        /// <summary>
        ///     The amount of spacing between each piece of text.
        /// </summary>
        private int TextYSpacing { get; } = 16;

        /// <summary>
        ///     The scale used for fade animations.
        /// </summary>
        private static float AnimationScale => 120f / AudioEngine.Track.Rate;

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="screen"></param>
        internal SongInformation(GameplayScreen screen)
        {
            Screen = screen;

            Size = new ScalableVector2(750, 150);
            Tint = Colors.MainAccentInactive;
            Alpha = 0;

            // Create watching text outside of replay mode because other text relies on it.
            Watching = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                $"Watching {(screen.InReplayMode ? Screen.LoadedReplay.PlayerName : "")}", 20)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = 25,
                Alpha = 0
            };

            Title = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                $"{Screen.Map.Artist} - {Screen.Map.Title}", 20)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = Watching.Y + TextYSpacing + TextYSpacing,
                Alpha = 0,
            };

            Difficulty = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                $"[{Screen.Map.DifficultyName}]", 20)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = Title.Y + TextYSpacing + TextYSpacing * 0.85f,
                Alpha = 0
            };

            Creator = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                $"Mapped By: \"{Screen.Map.Creator}\"", 20)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = Difficulty.Y + TextYSpacing + TextYSpacing * 0.80f,
                Alpha = 0
            };

            var difficulty = Screen.Map.SolveDifficulty(ModManager.Mods, true).OverallDifficulty;

            Rating = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                $"Difficulty: {StringHelper.AccuracyToString(difficulty).Replace("%", "")}",
                20)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = Creator.Y + TextYSpacing + TextYSpacing * 0.75f,
                Alpha = 0,
                Tint = ColorHelper.DifficultyToColor(difficulty)
            };

            // Get a formatted string of the activated mods.
            var modsString = "Mods: " + (ModManager.CurrentModifiersList.Count > 0 ? $"{ModHelper.GetModsString(ModManager.Mods)}" : "None");

            modsString += $" ({JudgementWindowsDatabaseCache.Selected.Value.Name})";

            Mods = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), modsString, 20)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = Rating.Y + TextYSpacing + TextYSpacing * 0.7f,
                Alpha = 0
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Fade in on map start
            if (Screen.Timing.Time < -500)
            {
                var alpha = MathHelper.Lerp(Title.Alpha, 1, (float) Math.Min(dt / AnimationScale, 1));

                Title.Alpha = alpha;
                Difficulty.Alpha = alpha;
                Creator.Alpha = alpha;
                Rating.Alpha = alpha;
                Mods.Alpha = alpha;

                if (Screen.InReplayMode)
                    Watching.Alpha = 0;
            }
            else
            {
                var alpha = MathHelper.Lerp(Title.Alpha, 0, (float)Math.Min(dt / AnimationScale, 1));

                Title.Alpha = alpha;
                Difficulty.Alpha = alpha;
                Creator.Alpha = alpha;
                Rating.Alpha = alpha;
                Mods.Alpha = alpha;
            }

            base.Update(gameTime);
        }
    }
}
