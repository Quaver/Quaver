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
using Quaver.API.Maps.Processors.Rating;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
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
        ///     Handles calculating rating for this individual user.
        /// </summary>
        internal RatingProcessorKeys RatingProcessor { get; }

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
        internal SpriteTextBitmap Score { get; }

        /// <summary>
        ///     Text that displays the user's current combo.
        /// </summary>
        internal SpriteTextBitmap Combo { get; }

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

        /// <summary>
        ///     Reference to the score
        /// </summary>
        private Score LocalScore { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="type"></param>
        /// <param name="username"></param>
        /// <param name="judgements"></param>
        /// <param name="avatar"></param>
        /// <param name="mods"></param>
        /// <param name="score"></param>
        /// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException"></exception>
        internal ScoreboardUser(GameplayScreen screen, ScoreboardUserType type, string username, List<Judgement> judgements, Texture2D avatar, ModIdentifier mods, Score score = null)
        {
            Screen = screen;
            LocalScore = score;
            Judgements = judgements;
            UsernameRaw = username;
            RatingProcessor = new RatingProcessorKeys(MapManager.Selected.Value.DifficultyFromMods(mods));
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
                    textAlpha = 0.65f;
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

            if (Type != ScoreboardUserType.Self)
            {
                if (LocalScore != null && LocalScore.IsOnline)
                {
                    // Check to see if we have a Steam avatar for this user cached.
                    if (SteamManager.UserAvatars.ContainsKey((ulong)LocalScore.SteamId))
                        Avatar.Image = SteamManager.UserAvatars[(ulong)LocalScore.SteamId];
                    else
                    {

                        Avatar.Alpha = 0;
                        Avatar.Image = UserInterface.UnknownAvatar;

                        // Otherwise we need to request for it.
                        SteamManager.SteamUserAvatarLoaded += OnAvatarLoaded;
                        SteamManager.SendAvatarRetrievalRequest((ulong)LocalScore.SteamId);
                    }
                }
                else
                {
                    Avatar.Image = UserInterface.UnknownAvatar;
                }
            }

            // Create username text.
            Username = new SpriteText(Fonts.Exo2Bold, GetUsernameFormatted(), 13)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Alpha = textAlpha,
                X = Avatar.Width + 10,
            };

            // Create score text.
            Score = new SpriteTextBitmap(FontsBitmap.AllerRegular, "0.00")
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Alpha = textAlpha,
                Y = Username.Y + Username.Height + 2,
                X = Username.X,
                FontSize = 18
            };

            // Create score text.
            Combo = new SpriteTextBitmap(FontsBitmap.AllerRegular, $"{Processor.Combo:N0}x")
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Alpha = textAlpha,
                FontSize = 18,
                X = -5
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            SteamManager.SteamUserAvatarLoaded -= OnAvatarLoaded;

            base.Destroy();
        }

        /// <summary>
        ///     Calculates score for a given object. Essentially it just calcs for the next Hitstat.
        /// </summary>
        internal void CalculateScoreForNextObject()
        {
            if (Type == ScoreboardUserType.Self)
            {
                Score.Text = $"{RatingProcessor.CalculateRating(Processor.Accuracy):0.00} / {StringHelper.AccuracyToString(Processor.Accuracy)}";
                Combo.Text = Processor.Combo.ToString("N0") + "x";

                SetTintBasedOnHealth();
                return;
            }

            // If the user doesn't have any more judgements then don't update them.
            if (Judgements.Count - 1 < CurrentJudgement)
                return;

            var processor = (ScoreProcessorKeys) Processor;
            processor.CalculateScore(Judgements[CurrentJudgement]);

            SetTintBasedOnHealth();

            Score.Text = $"{RatingProcessor.CalculateRating(Processor.Accuracy):0.00} / {StringHelper.AccuracyToString(Processor.Accuracy)}";
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

        /// <summary>
        ///     Called when a Steam avatar has loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnAvatarLoaded(object sender, SteamAvatarLoadedEventArgs e)
        {
            if (e.SteamId != (ulong)LocalScore.SteamId)
                return;

            Avatar.Image = e.Texture;
            Avatar.ClearAnimations();
            Avatar.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Avatar.Alpha, 1, 600));
        }
    }
}
