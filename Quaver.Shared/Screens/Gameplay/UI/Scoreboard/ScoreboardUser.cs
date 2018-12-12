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
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.UI.Scoreboard
{
    public class ScoreboardUser : Sprite
    {
        /// <summary>
        ///     Parent scoreboard
        /// </summary>
        public Scoreboard Scoreboard { get; set; }

        /// <summary>
        ///     Reference to the gameplay screen.
        /// </summary>
        private GameplayScreen Screen { get; }

        /// <summary>
        ///     The type of user this is. Either self or another.
        /// </summary>
        internal ScoreboardUserType Type { get; }

        /// <summary>
        ///    New score processor for this current user's scoreboard.
        ///    So we can calculate score on the fly.
        /// </summary>
        internal ScoreProcessor Processor { get; }

        /// <summary>
        ///    The user's target Y position based on their current rank.
        /// </summary>
        internal float TargetYPosition { get; set; }

        /// <summary>
        ///     The list of order judgements the user has gotten, so we can calculate their score
        ///     as the user plays the map.
        /// </summary>
        public List<Judgement> Judgements { get; }

        /// <summary>
        ///     The avatar for the user.
        /// </summary>
        private Sprite Avatar { get; }

        /// <summary>
        ///     Text that displays the username of the player.
        /// </summary>
        internal SpriteText Username { get; }

        /// <summary>
        ///     Text that displays the current score of the user.
        /// </summary>
        private SpriteText Score { get; }

        /// <summary>
        ///     Text that displays the user's current combo.
        /// </summary>
        private SpriteText Combo { get; }

        /// <summary>
        ///     The hit burst, whenever score is calculated again.
        /// </summary>
        private JudgementHitBurst HitBurst { get; }

        /// <summary>
        ///     The current judgement we're on in the list of them to calculate their score.
        /// </summary>
        private int CurrentJudgement { get; set; }

        /// <summary>
        ///     The user's raw username.
        /// </summary>
        internal string UsernameRaw { get; }

        /// <summary>
        ///     The rank of the user on the scoreboard.
        /// </summary>
        internal int Rank { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="type"></param>
        /// <param name="username"></param>
        /// <param name="judgements"></param>
        /// <param name="avatar"></param>
        /// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException"></exception>
        internal ScoreboardUser(GameplayScreen screen, ScoreboardUserType type, string username, List<Judgement> judgements, Texture2D avatar, ModIdentifier mods)
        {
            Screen = screen;
            Judgements = judgements;
            UsernameRaw = username;
            Type = type;
            Size = new ScalableVector2(260, 50);

            // Set position initially to offscreen
            X = -Width - 10;

            // The alpha of the tect - determined by the scoreboard user type.
            float textAlpha;

            // Set props based on the type of scoreboard user this is.
            switch (Type)
            {
                case ScoreboardUserType.Self:
                    Image = SkinManager.Skin.Scoreboard;
                    Alpha = 1f;
                    textAlpha = 1f;
                    break;
                case ScoreboardUserType.Other:
                    Image = SkinManager.Skin.ScoreboardOther;
                    Alpha = 0.75f;
                    textAlpha = 0.45f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (Screen.Map.Mode)
            {
                case GameMode.Keys4:
                case GameMode.Keys7:
                    Processor = Type == ScoreboardUserType.Other ? new ScoreProcessorKeys(Screen.Map, mods) : Screen.Ruleset.ScoreProcessor;
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }

            // Create avatar
            Avatar = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(Height, Height),
                Alignment = Alignment.MidLeft,
                Image = avatar,
            };

            // Create username text.
            Username = new SpriteText(BitmapFonts.Exo2Bold, GetUsernameFormatted(), 13)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Alpha = textAlpha,
                X = Avatar.Width + 10
            };

            // Create score text.
            Score = new SpriteText(BitmapFonts.Exo2Medium, Processor.Score.ToString("N0"), 12)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Alpha = textAlpha,
                Y = Username.Y + Username.Height + 2,
                X = Username.X
            };

            // Create score text.
            Combo = new SpriteText(BitmapFonts.Exo2Medium, $"{Processor.Combo:N0}x", 13)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Alpha = textAlpha
            };

            // Create hit burst
            HitBurst = new JudgementHitBurst(SkinManager.Skin.Judgements[Judgement.Miss], new Vector2(50, 50), 0)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Alpha = textAlpha
            };

            HitBurst.X = HitBurst.Frames[0].Width / 2f - 20;
        }

        /// <summary>
        ///     Calculates score for a given object. Essentially it just calcs for the next Hitstat.
        /// </summary>
        internal void CalculateScoreForNextObject()
        {
            if (Type == ScoreboardUserType.Self)
            {
                Score.Text = $"{Scoreboard.RatingCalculator.CalculateRating(Processor.Accuracy):0.##} ({StringHelper.AccuracyToString(Processor.Accuracy)})";
                Combo.Text = Processor.Combo.ToString("N0") + "x";

                // We don't actually store miss data in stats, so we'll just go by if the user's combo is now 0.
                HitBurst.PerformJudgementAnimation(Processor.Combo == 0 ? Judgement.Miss : Processor.Stats.Last().Judgement);
                SetTintBasedOnHealth();
                return;
            }

            // If the user doesn't have any more judgements then don't update them.
            if (Judgements.Count - 1 < CurrentJudgement)
                return;

            var processor = (ScoreProcessorKeys) Processor;
            processor.CalculateScore(Judgements[CurrentJudgement]);

            HitBurst.PerformJudgementAnimation(Judgements[CurrentJudgement]);

            SetTintBasedOnHealth();

            Score.Text = $"{Scoreboard.RatingCalculator.CalculateRating(Processor.Accuracy):0.##} ({StringHelper.AccuracyToString(Processor.Accuracy)})";
            Combo.Text = Processor.Combo.ToString("N0") + "x";

            CurrentJudgement++;
        }

        /// <summary>
        ///     Formatted username with rank
        /// </summary>
        /// <param name="???"></param>
        /// <returns></returns>
        internal string GetUsernameFormatted() => UsernameRaw == "" ? "  " : UsernameRaw;

        /// <summary>
        ///     Sets the text's color based on how much health the user has.
        /// </summary>
        private void SetTintBasedOnHealth()
        {
            if (Processor.Health >= 60)
                Username.Tint = Color.White;
            else if (Processor.Health >= 40)
                Username.Tint = Color.Yellow;
            else if (Processor.Health >= 1)
                Username.Tint = Color.Orange;
            else
                Username.Tint = Color.Red;
        }
    }
}
