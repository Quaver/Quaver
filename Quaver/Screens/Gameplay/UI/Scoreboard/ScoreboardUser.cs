using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.Assets;
using Quaver.Modifiers;
using Quaver.Skinning;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Screens.Gameplay.UI.Scoreboard
{
    public class ScoreboardUser : Sprite
    {
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
        public List<HitStat> HitStats { get; }

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
        ///     The current HitStat we're on in the list of them to calculate their score.
        /// </summary>
        private int CurrentHitStat { get; set; }

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
        /// <param name="stats"></param>
        /// <param name="avatar"></param>
        /// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException"></exception>
        internal ScoreboardUser(GameplayScreen screen, ScoreboardUserType type, string username, List<HitStat> stats, Texture2D avatar)
        {
            Screen = screen;
            HitStats = stats;
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
                    Processor = Type == ScoreboardUserType.Other ? new ScoreProcessorKeys(Screen.Map, ModManager.Mods) : Screen.Ruleset.ScoreProcessor;
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
            Username = new SpriteText(Fonts.AllerRegular16, GetUsernameFormatted())
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Alpha = textAlpha,
                TextScale = 0.85f
            };

            SetUsernamePosition();

            // Create score text.
            Score = new SpriteText(Fonts.AllerRegular16, Processor.Score.ToString("N0"))
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                TextScale = 0.70f,
                Alpha = textAlpha
            };

            // Create score text.
            Combo = new SpriteText(Fonts.AllerRegular16, $"{Processor.Combo:N0}x")
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                TextScale = 0.78f,
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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            UpdateScoreTextAndPosition();
            base.Update(gameTime);
        }

        /// <summary>
        ///     Calculates score for a given object. Essentially it just calcs for the next Hitstat.
        /// </summary>
        internal void CalculateScoreForNextObject()
        {
            // Don't bother calculating if the type is self, because we already have it calculated.
            // but perform the judgement animation however.
            if (Type == ScoreboardUserType.Self)
            {
                // We don't actually store miss data in stats, so we'll just go by if the user's combo is now 0.
                HitBurst.PerformJudgementAnimation(Processor.Combo == 0 ? Judgement.Miss : Processor.Stats.Last().Judgement);
                SetTextColorBasedOnHealth();
                return;
            }

            // If the user doesn't have any more judgements then don't update them.
            if (HitStats.Count - 1 < CurrentHitStat)
                return;

            // Check how many judgements
            var processor = (ScoreProcessorKeys) Processor;

            if (HitStats[CurrentHitStat].HitDifference == int.MinValue)
                processor.CalculateScore(Judgement.Miss);
            else
            {
                var judgement = processor.CalculateScore(HitStats[CurrentHitStat].HitDifference, HitStats[CurrentHitStat].KeyPressType);
                HitBurst.PerformJudgementAnimation(judgement);
            }

            SetTextColorBasedOnHealth();

            CurrentHitStat++;
        }

        /// <summary>
        ///     Sets the correct username position.
        /// </summary>
        private void SetUsernamePosition()
        {
            // Set username position.
            var usernameTextSize = Username.Font.MeasureString(Username.Text);
            Username.X = Avatar.Width + usernameTextSize.X * Username.TextScale / 2f + 10;
            Username.Y = usernameTextSize.Y * Username.TextScale / 2f + 2;
        }

        /// <summary>
        ///     Updates the text & position to stay aligned.
        /// </summary>
        private void UpdateScoreTextAndPosition()
        {
            // Username
            SetUsernamePosition();

            // Score
            Score.Text = Processor.Score.ToString("N0");

            var scoreTextSize = Score.Font.MeasureString(Score.Text);
            Score.X = Avatar.Width + scoreTextSize.X * Score.TextScale / 2f + 12;
            Score.Y = Username.Y + scoreTextSize.Y * Score.TextScale / 2f + 12;

            // Combo
            Combo.Text = Processor.Combo.ToString("N0") + "x";

            var comboTextSize = Combo.Font.MeasureString(Combo.Text);
            Combo.X = -comboTextSize.X * Combo.TextScale / 2f - 8;
            Combo.Y = 0;
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
        private void SetTextColorBasedOnHealth()
        {
            if (Processor.Health >= 60)
                Username.TextColor = Color.White;
            else if (Processor.Health >= 40)
                Username.TextColor = Color.Yellow;
            else if (Processor.Health >= 1)
                Username.TextColor = Color.Orange;
            else
                Username.TextColor = Color.Red;
        }
    }
}
