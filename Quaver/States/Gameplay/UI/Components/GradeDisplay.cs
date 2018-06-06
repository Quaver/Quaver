using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Graphics.Sprites;
using Quaver.Main;

namespace Quaver.States.Gameplay.UI.Components
{
    internal class GradeDisplay : Sprite
    {
        /// <summary>
        /// 
        /// </summary>
        private ScoreProcessor Scoring { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processor"></param>
        internal GradeDisplay(ScoreProcessor processor) => Scoring = processor;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            ChangeGradeImage();
            
            base.Update(dt);
        }

        /// <summary>
        ///     Changes the image of the sprite based on the user's current grade.
        /// </summary>
        private void ChangeGradeImage()
        {
            Visible = Scoring.Score > 0;
            
            var grade = GradeHelper.GetGradeFromAccuracy(Scoring.Accuracy, Scoring.Accuracy >= 100f && Scoring.CurrentJudgements[Judgement.Perfect] == 0);
            Image = Scoring.Failed ? GameBase.LoadedSkin.GradeSmallF : GameBase.LoadedSkin.ConvertGradeToSkinElement(grade);  
        }
    }
}