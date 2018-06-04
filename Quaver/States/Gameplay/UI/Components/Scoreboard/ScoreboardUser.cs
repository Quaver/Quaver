using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Graphics;
using Quaver.Graphics.Colors;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Main;
using Quaver.States.Gameplay.UI.Components.Judgements;

namespace Quaver.States.Gameplay.UI.Components.Scoreboard
{
    internal class ScoreboardUser : Sprite
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
        private List<Judgement> UserJudgements { get; }

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
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="type"></param>
        /// <param name="username"></param>
        /// <param name="judgements"></param>
        /// <param name="avatar"></param>
        /// <exception cref="InvalidEnumArgumentException"></exception>
        internal ScoreboardUser(GameplayScreen screen, ScoreboardUserType type, string username, List<Judgement> judgements, Texture2D avatar)
        {
            Screen = screen;       
            UserJudgements = judgements;
            Type = type;
            Size = new UDim2D(250, 40);

            // The alpha of the tect - determined by the scoreboard user type.
            float textAlpha;

            // Set props based on the type of scoreboard user this is.
            switch (Type)
            {
                case ScoreboardUserType.Self:
                    Tint = Color.Black;
                    Alpha = 1f;
                    textAlpha = 1f;
                    break;
                case ScoreboardUserType.Other:
                    Tint = Color.Black;
                    Alpha = 0.75f;
                    textAlpha = 0.50f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (Screen.Map.Mode)
            {
                case GameMode.Keys4:
                case GameMode.Keys7:
                    Processor = Type == ScoreboardUserType.Other ? new ScoreProcessorKeys(Screen.Map, GameBase.CurrentMods) : Screen.Ruleset.ScoreProcessor;
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }

            // Create avatar
            Avatar = new Sprite()
            {
                Parent = this,
                Size = new UDim2D(SizeY, SizeY),
                Alignment = Alignment.MidLeft,
                Image = avatar
            };
            
            // Create username text.
            Username = new SpriteText()
            {
                Parent = this,
                Font = QuaverFonts.AssistantRegular16,
                Text = (username == "") ? "  " : username,
                Alignment = Alignment.TopLeft,
                Alpha = textAlpha,
                TextScale = 0.75f
            };

            SetUsernamePosition();
            
            // Create score text.
            Score = new SpriteText()
            {
                Parent = this,
                Font = QuaverFonts.AssistantRegular16,
                Alignment = Alignment.TopLeft,
                Text = Processor.Score.ToString("N0"),
                TextScale = 0.60f,
                Alpha = textAlpha
            };
            
            // Create score text.
            Combo = new SpriteText()
            {
                Parent = this,
                Font = QuaverFonts.AssistantRegular16,
                Alignment = Alignment.MidRight,
                Text = $"{Processor.Combo:N0}x",
                TextScale = 0.65f,
                Alpha = textAlpha
            };
            
            // Create hit burst
            HitBurst = new JudgementHitBurst(GameBase.LoadedSkin.JudgeMiss, new Vector2(50, 50), 0)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Alpha = 0.75f
            };
            HitBurst.PosX = HitBurst.Frames[0].Width / 2f - 20;
        }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            UpdateScoreTextAndPosition();
            base.Update(dt);
        }

        /// <summary>
        ///     Calculates score for a given object. Essentially it just calcs for the next judgement.
        /// </summary>
        internal void CalculateScoreForNextObject()
        {           
            // Don't bother calculating if the type is self, because we already have it calculated.
            // but perform the judgement animation however.
            if (Type == ScoreboardUserType.Self)
            {
                // We don't actually store miss data in stats, so we'll just go by if the user's combo is now 0.
                HitBurst.PerformJudgementAnimation(Processor.Combo == 0 ? Judgement.Miss : Processor.Stats.Last().Judgement);
                return;
            }
            
            Processor.CalculateScore(UserJudgements[CurrentJudgement]);
            HitBurst.PerformJudgementAnimation(UserJudgements[CurrentJudgement]);
            CurrentJudgement++;   
        }

        /// <summary>
        ///     Sets the correct username position.
        /// </summary>
        internal void SetUsernamePosition()
        {
            // Set username position.
            var usernameTextSize = Username.Font.MeasureString(Username.Text);        
            Username.PosX = Avatar.SizeX + usernameTextSize.X * Username.TextScale / 2f + 10;
            Username.PosY = usernameTextSize.Y * Username.TextScale / 2f - 2;
        }
        
        /// <summary>
        ///     Updates the text & position to stay aligned.
        /// </summary>
        private void UpdateScoreTextAndPosition()
        {
            // Score
            Score.Text = Processor.Score.ToString("N0");

            var scoreTextSize = Score.Font.MeasureString(Score.Text);
            Score.PosX = Avatar.SizeX + scoreTextSize.X * Score.TextScale / 2f + 12;
            Score.PosY = Username.PosY + scoreTextSize.Y * Score.TextScale / 2f + 8;

            // Combo
            Combo.Text = Processor.Combo.ToString("N0") + "x";

            var comboTextSize = Combo.Font.MeasureString(Combo.Text);
            Combo.PosX = -comboTextSize.X * Combo.TextScale / 2f - 8;
            Combo.PosY = 0;
        }
    }
}