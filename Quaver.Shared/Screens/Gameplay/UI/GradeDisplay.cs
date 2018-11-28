using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Skinning;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.UI
{
    public class GradeDisplay : Sprite
    {
        /// <summary>
        ///
        /// </summary>
        private ScoreProcessor Scoring { get; }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="processor"></param>
        public GradeDisplay(ScoreProcessor processor) => Scoring = processor;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            ChangeGradeImage();

            base.Update(gameTime);
        }

        /// <summary>
        ///     Changes the image of the sprite based on the user's current grade.
        /// </summary>
        private void ChangeGradeImage()
        {
            Visible = Scoring.Score > 0;

            var grade = GradeHelper.GetGradeFromAccuracy(Scoring.Accuracy, Scoring.Accuracy >= 100f && Scoring.CurrentJudgements[Judgement.Perf] == 0);
            Image = Scoring.Failed ? SkinManager.Skin.Grades[Grade.F] : SkinManager.Skin.Grades[grade];
        }
    }
}
