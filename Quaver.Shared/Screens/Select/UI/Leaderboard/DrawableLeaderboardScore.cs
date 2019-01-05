/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning;
using Steamworks;
using TimeAgo;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace Quaver.Shared.Screens.Select.UI.Leaderboard
{
    public class DrawableLeaderboardScore : Button
    {
        /// <summary>
        ///     The score this drawable represents.
        /// </summary>
        public Score Score { get; }

        /// <summary>
        ///     The height of an inidvidual leaderboard score.
        /// </summary>
        public static int HEIGHT { get; } = 56;

        /// <summary>
        ///     The width of the score.
        /// </summary>
        public static int WIDTH { get; } = 620;

        /// <summary>
        ///     The rank of the score.
        /// </summary>
        private int Rank { get; }

        /// <summary>
        ///     The text that displays the rank of the score.
        /// </summary>
        private SpriteText TextRank { get; set; }

        /// <summary>
        ///     The score user's avatar.
        /// </summary>
        private Sprite Avatar { get; set; }

        /// <summary>
        ///     Displays an image of the grade achieved on the score.
        /// </summary>
        private Sprite Grade { get; set; }

        /// <summary>
        ///     Displays the username of the score user.
        /// </summary>
        private SpriteText Username { get; set; }

        /// <summary>
        ///     Displays the user's score and max combo.
        /// </summary>
        private SpriteText TextScore { get; set; }

        /// <summary>
        ///     Displays the modifiers used on the score.
        /// </summary>
        private SpriteText Mods { get; set; }

        /// <summary>
        ///     Displays how long ago the score took place.
        /// </summary>
        private SpriteText TimeAgo { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public DrawableLeaderboardScore(Score score = null, int rank = -1)
        {
            Score = score;
            Rank = rank;

            Size = new ScalableVector2(WIDTH, HEIGHT);
            Tint = Rank == -1 ? Colors.MainAccent : Color.Black;
            Alpha = 0.60f;

            // If there is no score, then we'll consider this to be a "No personal Best" score.
            if (score == null)
            {
                CreateNoPersonalBestScore();
                return;
            }

            CreateScore();

            Clicked += (sender, args) =>
            {
                var game = GameBase.Game as QuaverGame;
                var screen = game.CurrentScreen as SelectScreen;
                screen.ExitToResults(Score);
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Alpha = MathHelper.Lerp(Alpha, IsHovered ? 0.40f : 0.60f, (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 60, 1));
            base.Update(gameTime);
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
        ///
        /// </summary>
        private void CreateNoPersonalBestScore()
        {
            var nopbSet = new SpriteText(Fonts.Exo2Bold, $"No Personal Best {ConfigManager.LeaderboardSection.Value} Score", 13)
            {
                Parent = this,
                Alignment = Alignment.MidCenter
            };
        }

        /// <summary>
        ///     Creates the entire score drawable.
        /// </summary>
        private void CreateScore()
        {
            CreateTextRank();
            CreateAvatar();
            CreateGrade();
            CreateUsername();
            CreateTextScoreAndCombo();
            CreateModsUsed();
            CreateTimeAgo();
        }

        /// <summary>
        ///     Creates the text that displays the user's rank.
        /// </summary>
        private void CreateTextRank() => TextRank = new SpriteText(Fonts.Exo2Bold, $"{( Rank == -1 ? "PB" : $"{Rank}." )}", 13)
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            X = 18,
            Tint = Rank == -1 ? Colors.SecondaryAccent : Color.White
        };

        /// <summary>
        ///     Creates the avatar sprite for the user to use.
        /// </summary>
        private void CreateAvatar()
        {
            Avatar = new Sprite()
            {
                Parent = this,
                X = 60,
                Size = new ScalableVector2(HEIGHT, HEIGHT)
            };

            if (Score.Name == ConfigManager.Username.Value)
            {
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (SteamManager.UserAvatars.ContainsKey(SteamUser.GetSteamID().m_SteamID))
                    Avatar.Image = SteamManager.UserAvatars[SteamUser.GetSteamID().m_SteamID];
                else
                    Avatar.Image = UserInterface.YouAvatar;
            }
            else if (Score.IsOnline)
            {
                // Check to see if we have a Steam avatar for this user cached.
                if (SteamManager.UserAvatars.ContainsKey((ulong) Score.SteamId))
                {
                    Avatar.Image = SteamManager.UserAvatars[(ulong) Score.SteamId];
                    return;
                }

                Avatar.Alpha = 0;
                Avatar.Image = UserInterface.YouAvatar;

                // Otherwise we need to request for it.
                SteamManager.SteamUserAvatarLoaded += OnAvatarLoaded;
                SteamManager.SendAvatarRetrievalRequest((ulong) Score.SteamId);
            }
            else
            {
                Avatar.Image = UserInterface.UnknownAvatar;
            }
        }

        /// <summary>
        ///     Creates the sprite that displays the score user's grade.
        /// </summary>
        private void CreateGrade() => Grade = new Sprite()
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            X = Avatar.X + Avatar.Width + 10,
            Size = new ScalableVector2(HEIGHT * 0.65f, HEIGHT * 0.65f),
            Image = Score.Grade == API.Enums.Grade.F ? SkinManager.Skin.Grades[API.Enums.Grade.F]
                : SkinManager.Skin.Grades[GradeHelper.GetGradeFromAccuracy((float) Score.Accuracy)]
        };

        /// <summary>
        ///     The text that displays the user's username.
        /// </summary>
        private void CreateUsername() => Username = new SpriteText(Fonts.Exo2Bold, Score.Name, 13)
        {
            Parent = this,
            Alignment = Alignment.TopLeft,
            Position = new ScalableVector2(Grade.X + Grade.Width + 15, 3),
        };

        /// <summary>
        ///     Creates the text that displays the user's score and combo.
        /// </summary>
        private void CreateTextScoreAndCombo() => TextScore = new SpriteText(Fonts.Exo2Bold,
            $"{StringHelper.AccuracyToString((float) Score.PerformanceRating).Replace("%", "")} / {Score.TotalScore:n0} / {StringHelper.AccuracyToString((float) Score.Accuracy)} / {Score.MaxCombo:n0}x", 11, false)
        {
            Parent = this,
            Alignment = Alignment.TopLeft,
            Position = new ScalableVector2(Username.X, Username.Y + Username.Height + 3)
        };

        /// <summary>
        ///     Creates the text that displays the modifiers used on the score.
        /// </summary>
        private void CreateModsUsed() => Mods = new SpriteText(Fonts.Exo2Bold, ModHelper.GetModsString(Score.Mods), 11, false)
        {
            Parent = this,
            Alignment = Alignment.TopRight,
            X = -5,
            Y = Username.Y + 2
        };

        /// <summary>
        ///     Creates the text that displays how long ago the score was.
        /// </summary>
        private void CreateTimeAgo()
        {
            var time = DateTime.Parse(Score.DateTime);
            var timeDifference = DateTime.Now - time;

            string timeAgo;

            try
            {
                timeAgo = time.TimeAgo(CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                timeAgo = "1 minute ago";
            }

            TimeAgo = new SpriteText(Fonts.Exo2Bold, timeAgo, 11, false)
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                X = Mods.X,
                Y = TextScore.Y + 2,
                Tint = (long)timeDifference.TotalMilliseconds < 60 * 60 * 1000 ? Colors.MainAccent : Color.White
            };
        }

        /// <inheritdoc />
        /// <summary>
        ///     In this case, we only want buttons to be clickable if they're in the bounds of the scroll container.
        /// </summary>
        /// <returns></returns>
        protected override bool IsMouseInClickArea()
        {
            var newRect = Rectangle.Intersect(ScreenRectangle, Parent.ScreenRectangle);
            return GraphicsHelper.RectangleContains(newRect, MouseManager.CurrentState.Position);
        }

        /// <summary>
        ///     Called when a Steam avatar has loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnAvatarLoaded(object sender, SteamAvatarLoadedEventArgs e)
        {
            if (e.SteamId != (ulong) Score.SteamId)
                return;

            Avatar.Image = e.Texture;
            Avatar.ClearAnimations();
            Avatar.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Avatar.Alpha, 1, 600));
        }
    }
}
