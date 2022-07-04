/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Rating;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.API.Maps.Processors.Scoring.Multiplayer;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning;
using Steamworks;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Logging;
using Wobble.Managers;
using Wobble.Window;

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
        internal ScoreProcessor Processor { get; set; }

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
        public List<Judgement> Judgements { get; set; }

        /// <summary>
        ///     The avatar for the user.
        /// </summary>
        internal Sprite Avatar { get; }

        /// <summary>
        ///     Text that displays the username of the player.
        /// </summary>
        internal SpriteTextPlus Username { get; }

        /// <summary>
        ///     Text that displays the current score of the user.
        /// </summary>
        internal SpriteTextPlus Score { get; }

        /// <summary>
        ///     Text that displays the user's current combo.
        /// </summary>
        internal SpriteTextPlus Combo { get; }

        /// <summary>
        ///     Text that displays the rank for each user
        /// </summary>
        internal SpriteTextPlus RankText { get; }

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
        public Score LocalScore { get; }

        /// <summary>
        ///     If the particular user has quit the game
        /// </summary>
        public bool HasQuit { get; private set; }

        public bool IsOneVersusOne => OnlineManager.CurrentGame?.Ruleset == MultiplayerGameRuleset.Free_For_All
                                      && Screen.LocalScores.Count == 1;

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
        /// <param name="processor"></param>
        /// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException"></exception>
        internal ScoreboardUser(GameplayScreen screen, ScoreboardUserType type, string username, List<Judgement> judgements, Texture2D avatar,
            ModIdentifier mods, Score score = null, RatingProcessorKeys processor = null)
        {
            Screen = screen;
            LocalScore = score;
            Judgements = judgements;
            UsernameRaw = username;
            RatingProcessor = (RatingProcessorKeys) score?.RatingProcessor ?? processor ?? new RatingProcessorKeys(MapManager.Selected.Value.DifficultyFromMods(mods));
            Type = type;
            Size = new ScalableVector2(299, 58);

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
                    textAlpha = 0.65f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (Screen.Map.Mode)
            {
                case GameMode.Keys4:
                case GameMode.Keys7:
                    if (screen.IsMultiplayerGame && Type == ScoreboardUserType.Other)
                    {
                        var mp = new ScoreProcessorMultiplayer((MultiplayerHealthType) OnlineManager.CurrentGame.HealthType, OnlineManager.CurrentGame.Lives);
                        Processor = new ScoreProcessorKeys(Screen.Map, mods, mp);
                    }
                    else
                        Processor = Type == ScoreboardUserType.Other ? new ScoreProcessorKeys(Screen.Map, mods): Screen.Ruleset.ScoreProcessor;
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

            RankText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "?.", 20, false)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Avatar.X + Avatar.Width + 14,
                Alpha = textAlpha
            };

            if (Type != ScoreboardUserType.Self)
            {
                if (LocalScore != null && (LocalScore.IsOnline || LocalScore.IsMultiplayer))
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
                    Avatar.Image = ConfigManager.Username?.Value == UsernameRaw
                        ? SteamManager.UserAvatars[SteamUser.GetSteamID().m_SteamID]
                        : UserInterface.UnknownAvatar;
                }
            }

            // Create username text.
            Username = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), GetUsernameFormatted(), 21)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Alpha = textAlpha,
                X = RankText.X + RankText.Width + 20,
                Y = 8,
            };

            // Create score text.
            Score = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "0.00 / 0.00%", 19, false)
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Alpha = textAlpha,
                Y = -Username.Y,
                X = Username.X,
            };

            // Create score text.
            Combo = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), $"{Processor.Combo:N0}x", 18, false)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Alpha = textAlpha,
                X = -5
            };
        }

        public override void DrawToSpriteBatch()
        {
            if (RectangleF.Intersects(ScreenRectangle, WindowManager.Rectangle))
                base.DrawToSpriteBatch();
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
        internal void CalculateScoreForNextObject(bool setScoreboardValues = true)
        {
            if (setScoreboardValues && Type == ScoreboardUserType.Self)
            {
                var oldProcessor = Processor;
                Processor = Screen.Ruleset.StandardizedReplayPlayer.ScoreProcessor;
                var rating = CalculateRating();

                if (Screen.InReplayMode)
                    Processor = Screen.Ruleset.ScoreProcessor;
                else
                    Processor = oldProcessor;

                Score.Text = $"{StringHelper.RatingToString(rating)} / {StringHelper.AccuracyToString(Processor.Accuracy)}";
                Combo.Text = Processor.Combo.ToString("N0") + "x";

                SetTintBasedOnHealth();
                Scoreboard.TeamBanner?.UpdateAverageRating();
                return;
            }

            // If the user doesn't have any more judgements then don't update them.
            if (Judgements.Count - 1 < CurrentJudgement)
                return;

            var processor = (ScoreProcessorKeys) Processor;
            processor.CalculateScore(Judgements[CurrentJudgement]);

            if (setScoreboardValues)
            {
                SetTintBasedOnHealth();

                var rating = CalculateRating();

                Score.Text = $"{rating:0.00} / {StringHelper.AccuracyToString(Processor.Accuracy)}";
                Combo.Text = Processor.Combo.ToString("N0") + "x";
                Scoreboard.TeamBanner?.UpdateAverageRating();
            }

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
        public void SetTintBasedOnHealth()
        {
            try
            {
                if (Processor == null || Username == null)
                    return;

                if (Processor.MultiplayerProcessor != null)
                {
                    if (Processor.MultiplayerProcessor.IsEliminated || Processor.MultiplayerProcessor.IsRegeneratingHealth)
                    {
                        Username.Tint = Color.Red;
                        return;
                    }
                }

                if (Processor.Health >= 60)
                    Username.Tint = Color.White;
                else if (Processor.Health >= 40)
                    Username.Tint = Color.Yellow;
                else if (Processor.Health >= 1)
                    Username.Tint = Color.Orange;
                else
                    Username.Tint = Color.Red;
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
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

        /// <summary>
        /// </summary>
        public void QuitGame()
        {
            HasQuit = true;
            Username.Text = $"[Quit] {UsernameRaw}";
            Username.Tint = Color.Crimson;
        }

        // Sets the appropriate image for the scoreboard user
        public void SetImage()
        {
            switch (Type)
            {
                case ScoreboardUserType.Self:
                    Image = SkinManager.Skin.Scoreboard;

                    if (OnlineManager.CurrentGame != null &&
                        OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Team)
                    {
                        switch (Scoreboard.Team)
                        {
                            case MultiplayerTeam.Red:
                                Image = SkinManager.Skin.ScoreboardRedTeam;
                                break;
                            case MultiplayerTeam.Blue:
                                Image = SkinManager.Skin.ScoreboardBlueTeam;
                                break;
                        }
                    }
                    else if (IsOneVersusOne)
                        Image = SkinManager.Skin.ScoreboardRedTeam;
                    break;
                case ScoreboardUserType.Other:
                    Image = SkinManager.Skin.ScoreboardOther;

                    if (OnlineManager.CurrentGame != null &&
                        OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Team)
                    {
                        switch (Scoreboard.Team)
                        {
                            case MultiplayerTeam.Red:
                                Image = SkinManager.Skin.ScoreboardRedTeamOther;
                                break;
                            case MultiplayerTeam.Blue:
                                Image = SkinManager.Skin.ScoreboardBlueTeamOther;
                                break;
                        }
                    }
                    else if (IsOneVersusOne)
                        Image = UserInterface.ScoreboardBlueMirrored;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public double CalculateRating()
        {
            var oldProcessor = Processor;

            if (Type == ScoreboardUserType.Self)
                Processor = Screen.Ruleset.StandardizedReplayPlayer.ScoreProcessor;

            var rating = RatingProcessor.CalculateRating(Processor.Accuracy);

            Processor = oldProcessor;

            if (Processor.MultiplayerProcessor != null)
            {
                if (OnlineManager.CurrentGame != null && OnlineManager.CurrentGame.Ruleset != MultiplayerGameRuleset.Battle_Royale &&
                    (Processor.MultiplayerProcessor.IsRegeneratingHealth || Processor.MultiplayerProcessor.IsEliminated))
                    rating = 0;
            }

            return rating;
        }
    }
}
