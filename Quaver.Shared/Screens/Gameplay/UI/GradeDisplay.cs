/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

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
        ///     The current grade the player has.
        /// </summary>
        private Grade _grade;
        public Grade Grade
        {
            get => _grade;
            set
            {
                if (_grade == value)
                    return;

                _grade = value;
                Image = Scoring.Failed ? SkinManager.Skin.Grades[Grade.F] : SkinManager.Skin.Grades[value];
                UpdateWidth();
            }
        }
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
            Grade = GradeHelper.GetGradeFromAccuracy(Scoring.Accuracy);
        }

        /// <summary>
        ///     Updates the width so that the grade image's aspect ratio is preserved.
        /// </summary>
        public void UpdateWidth()
        {
            Width = Height * ((float)Image.Width / Image.Height);
        }
    }
}
