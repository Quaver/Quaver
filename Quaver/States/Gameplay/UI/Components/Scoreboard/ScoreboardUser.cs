using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        ///     The current judgement we're on in the list of them.
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
            Size = new UDim2D(275, 50);

            // The alpha of the tect - determined by the scoreboard user type.
            float textAlpha;

            // Set props based on the type of scoreboard user this is.
            switch (Type)
            {
                case ScoreboardUserType.Self:
                    Tint = QuaverColors.MainAccent;
                    Alpha = 1f;
                    textAlpha = 1f;
                    break;
                case ScoreboardUserType.Other:
                    Tint = QuaverColors.MainAccentInactive;
                    Alpha = 0.75f;
                    textAlpha = 0.75f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (Screen.Map.Mode)
            {
                case GameMode.Keys4:
                case GameMode.Keys7:
                    if (Type == ScoreboardUserType.Other)
                        Processor = new ScoreProcessorKeys(Screen.Map, GameBase.CurrentMods);
                    else
                        Processor = Screen.Ruleset.ScoreProcessor;
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
                Alpha = textAlpha
            };

            // Set username position.
            var usernameTextSize = Username.Font.MeasureString(Username.Text);        
            Username.PosX = Avatar.SizeX + usernameTextSize.X / 2f + 10;
            Username.PosY = usernameTextSize.Y / 2f;
            
            // Create score text.
            Score = new SpriteText()
            {
                Parent = this,
                Font = QuaverFonts.AssistantRegular16,
                Alignment = Alignment.TopLeft,
                Text = Processor.Score.ToString("N0"),
                TextScale = 0.85f,
                Alpha = textAlpha
            };
            
            // Create score text.
            Combo = new SpriteText()
            {
                Parent = this,
                Font = QuaverFonts.AssistantRegular16,
                Alignment = Alignment.TopRight,
                Text = $"{Processor.Combo:N0}x",
                TextScale = 0.98f,
                Alpha = textAlpha
            };
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
            if (Type == ScoreboardUserType.Self)
                return;
            
            Processor.CalculateScore(UserJudgements[CurrentJudgement]);
            CurrentJudgement++;   
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
            Score.PosY = Username.PosY + scoreTextSize.Y * Score.TextScale / 2f + 10;

            // Combo
            Combo.Text = Processor.Combo.ToString("N0") + "x";

            var comboTextSize = Combo.Font.MeasureString(Combo.Text);
            Combo.PosX = -comboTextSize.X * Combo.TextScale / 2f - 10;
            Combo.PosY = Score.PosY - comboTextSize.Y * Combo.TextScale / 2f + 10;
        }
    }
}