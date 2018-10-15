using System;
using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.Assets;
using Quaver.Config;
using Quaver.Database.Scores;
using Quaver.Graphics;
using Quaver.Helpers;
using Quaver.Online;
using Quaver.Screens.Results;
using Quaver.Skinning;
using Steamworks;
using Wobble.Discord.RPC;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Screens;

namespace Quaver.Screens.Select.UI.MapInfo.Leaderboards.Scores
{
    public class LeaderboardScore : Button
    {
        /// <summary>
        ///     The parent leaderboard section.
        /// </summary>
        public LeaderboardSection Section { get; }

        /// <summary>
        ///     The score that's being represented.
        /// </summary>
        public LocalScore Score { get; }

        /// <summary>
        ///     The user's avatar.
        /// </summary>
        public Sprite Avatar { get; private set; }

        /// <summary>
        ///     The grade achieved on the score.
        /// </summary>
        public Sprite GradeAchieved { get; private set; }

        /// <summary>
        ///     The user's rank on the leaderboard.
        /// </summary>
        public SpriteText Rank { get; private set; }

        /// <summary>
        ///     The username of the score holder.
        /// </summary>
        public SpriteText Username { get; private set; }

        /// <summary>
        ///     Displays the score of the user.
        /// </summary>
        public SpriteText ScoreText { get; private set; }

        /// <summary>
        ///     Displays the accuracy of the user.
        /// </summary>
        public SpriteText Accuracy { get; private set; }

        /// <summary>
        ///     Displays the mods of the user.
        /// </summary>
        public SpriteText Mods { get; private set; }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="section"></param>
        ///  <param name="score"></param>
        /// <param name="rank"></param>
        public LeaderboardScore(LeaderboardSection section, LocalScore score, int rank)
        {
            Section = section;
            Score = score;

            Size = new ScalableVector2(section.ScrollContainer.Width, 54);
            Tint = Colors.DarkGray;
            Alpha = 0.65f;

            CreateRank(rank);
            CreateAvatar();
            CreateGradeAchieved();
            CreateUsername();
            CreateScoreText($"{score.Score:n0}");
            CreateAccuracyText();
            CreateModsText();

            section.ScrollContainer.AddContainedDrawable(this);
            Clicked += (sender, args) => QuaverScreenManager.ChangeScreen(new ResultsScreen(Score));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            PerformHoverAnimations(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        ///     Creates the user's avatar.
        /// </summary>
        private void CreateAvatar() => Avatar = new Sprite()
        {
            Parent = this,
            Alignment = Alignment.TopLeft,
            Size = new ScalableVector2(Height, Height),
            Image = ConfigManager.Username.Value == Score.Name ? SteamManager.UserAvatars[SteamUser.GetSteamID().m_SteamID] : UserInterface.UnknownAvatar,
            X = 50
        };

        /// <summary>
        ///     Creates the grade achieved sprite.
        /// </summary>
        private void CreateGradeAchieved() => GradeAchieved = new Sprite()
        {
            Parent = this,
            Alignment = Alignment.TopLeft,
            Size = new ScalableVector2(Height, Height),
            Image = SkinManager.Skin.Grades[Score.Grade],
            X = Avatar.X + Avatar.Width + 5
        };

        /// <summary>
        ///    Creates the rank text
        /// </summary>
        private void CreateRank(int rank)
        {
            Rank = new SpriteText(Fonts.Exo2Italic24, $"{rank}.")
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 18,
                TextScale = 0.60f
            };

            var rankSize = Rank.MeasureString() / 2f;
            Rank.X += rankSize.X;
        }

        /// <summary>
        ///     Creates the text for the username.
        /// </summary>
        private void CreateUsername()
        {
            Username = new SpriteText(Fonts.Exo2BoldItalic24, Score.Name)
            {
                Parent = this,
                TextScale = 0.50f,
                X = GradeAchieved.X + GradeAchieved.Width + 10,
                Y = 5
            };

            var size = Username.MeasureString() / 2f;
            Username.X += size.X;
            Username.Y += size.Y;
        }

        /// <summary>
        ///     Creates the text that displays
        /// </summary>
        /// <param name="value"></param>
        private void CreateScoreText(string value)
        {
            ScoreText = new SpriteText(Fonts.Exo2Regular24, $"{value} / {Score.MaxCombo}x")
            {
                Parent = this,
                TextScale = 0.45f,
                X = GradeAchieved.X + GradeAchieved.Width + 10,
                Y = Height - 5
            };

            var size = ScoreText.MeasureString() / 2f;

            ScoreText.X += size.X;
            ScoreText.Y -= size.Y;
        }

        /// <summary>
        ///    Creates the text that displays the accuracy.
        /// </summary>
        private void CreateAccuracyText()
        {
            Accuracy = new SpriteText(Fonts.Exo2Regular24, StringHelper.AccuracyToString((float) Score.Accuracy))
            {
                Parent = this,
                Alignment = Alignment.BotRight,
                TextScale = 0.45f,
                X = -10,
                Y = -5
            };

            var size = Accuracy.MeasureString() / 2f;
            Accuracy.X -= size.X;
            Accuracy.Y -= size.Y;
        }

        /// <summary>
        ///     Creates the text that displays the mods.
        /// </summary>
        private void CreateModsText()
        {
            Mods = new SpriteText(Fonts.Exo2Regular24, ModHelper.GetModsString(Score.Mods))
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                TextScale = 0.45f,
                X = -10,
                Y = 5
            };

            var size = Mods.MeasureString() / 2f;
            Mods.X -= size.X;
            Mods.Y += size.Y;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="gameTime"></param>
        private void PerformHoverAnimations(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
            FadeToColor(IsHovered ? Color.LightBlue : Colors.DarkGray, dt, 120);
        }

        /// <inheritdoc />
        /// <summary>
        ///     In this case, we only want buttons to be clickable if they're in the bounds of the scroll container.
        /// </summary>
        /// <returns></returns>
        protected override bool IsMouseInClickArea()
        {
            var newRect = Rectangle.Intersect(ScreenRectangle.ToRectangle(), Section.ScrollContainer.ScreenRectangle.ToRectangle());
            return GraphicsHelper.RectangleContains(newRect, MouseManager.CurrentState.Position);
        }
    }
}