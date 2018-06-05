using System;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Main;
using Quaver.States.Gameplay.UI.Components.Health;

namespace Quaver.States.Gameplay.GameModes.Keys.Playfield.Health
{
    internal class HealthBarKeys : HealthBar
    {
        /// <summary>
        ///     Reference to the mania playfield.
        /// </summary>
        private KeysPlayfield Playfield { get; }
        
        /// <summary>
        ///     Where the health bar is aligned relative to the playfield.
        /// </summary>
        private HealthBarKeysAlignment BarAlignment { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="playfield"></param>
        /// <param name="type"></param>
        /// <param name="alignment"></param>
        /// <param name="processor"></param>
        internal HealthBarKeys(KeysPlayfield playfield, HealthBarType type, HealthBarKeysAlignment alignment, ScoreProcessor processor) 
                                : base(type, processor)
        {
            Playfield = playfield;
            BarAlignment = alignment;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Set position based on stage.
        /// </summary>
        /// <param name="state"></param>
        public override void Initialize(IGameState state)
        {
            base.Initialize(state);

            switch (BarAlignment)
            {
                case HealthBarKeysAlignment.TopLeft:
                    // No need to handle. It's defaulted at the top left.
                    break;
                case HealthBarKeysAlignment.LeftStage:
                    // Align the two bars at the center where the playfield is.
                    BackgroundBar.Alignment = Alignment.BotCenter;
                    ForegroundBar.Alignment = Alignment.BotCenter;

                    var newLeftPosX = -Playfield.Width / 2f;
                    BackgroundBar.PosX = newLeftPosX;
                    ForegroundBar.PosX = newLeftPosX;
                    break;
                case HealthBarKeysAlignment.RightStage:
                    // Align the two bars at the center where the playfield is.
                    BackgroundBar.Alignment = Alignment.BotCenter;
                    ForegroundBar.Alignment = Alignment.BotCenter;
                    
                    var newRightPosX = Playfield.Width / 2f;
                    BackgroundBar.PosX = newRightPosX;
                    ForegroundBar.PosX = newRightPosX;
                    break;
            }
        }
    }
}