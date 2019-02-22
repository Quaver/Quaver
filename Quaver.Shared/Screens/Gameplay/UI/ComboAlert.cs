/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2019 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.UI
{
    public class ComboAlert : Container
    {
        /// <summary>
        /// </summary>
        private ScoreProcessor Processor { get; }

        /// <summary>
        /// </summary>
        private int ComboLastFrame { get; set; }

        /// <summary>
        /// </summary>
        private bool PlayedForThisCombo { get; set; }

        /// <summary>
        /// </summary>
        private Sprite AlertSprite { get; }

        /// <summary>
        /// </summary>
        private int TextureIndex { get; set; } = -1;

        /// <summary>
        /// </summary>
        private int SoundEffectIndex { get; set; } = -1;

        /// <summary>
        /// </summary>
        /// <param name="processor"></param>
        public ComboAlert(ScoreProcessor processor)
        {
            Processor = processor;

            AlertSprite = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidRight,
            };

            if (SkinManager.Skin.ComboAlerts.Count != 0)
            {
                AlertSprite.Image = SkinManager.Skin.ComboAlerts.First();
                AlertSprite.Size = new ScalableVector2(AlertSprite.Image.Width, AlertSprite.Image.Height);
                AlertSprite.X = AlertSprite.Width;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (Processor.Combo != ComboLastFrame)
                PlayedForThisCombo = false;

            if (Processor.Combo > ComboLastFrame && Processor.Combo % 100 == 0 && !PlayedForThisCombo)
            {
                if (SkinManager.Skin.ComboAlerts.Count != 0)
                {
                    AlertSprite.ClearAnimations();
                    AlertSprite.Alpha = 0;

                    if (TextureIndex + 1 < SkinManager.Skin.ComboAlerts.Count)
                        TextureIndex++;
                    else
                        TextureIndex = 0;

                    AlertSprite.Image = SkinManager.Skin.ComboAlerts[TextureIndex];
                    AlertSprite.Size = new ScalableVector2(AlertSprite.Image.Width, AlertSprite.Image.Height);
                    AlertSprite.X = AlertSprite.Width + 10;

                    AlertSprite
                        .FadeTo(1, Easing.Linear, 300)
                        .MoveToX(-80, Easing.OutQuint, 1000)
                        .Wait(300);

                    AlertSprite
                        .FadeTo(0, Easing.Linear, 200);
                }

                if (SkinManager.Skin.SoundComboAlerts.Count != 0)
                {
                    if (SoundEffectIndex + 1 < SkinManager.Skin.SoundComboAlerts.Count)
                        SoundEffectIndex++;
                    else
                        SoundEffectIndex = 0;

                    SkinManager.Skin.SoundComboAlerts[SoundEffectIndex].CreateChannel().Play();
                }

                PlayedForThisCombo = true;
            }

            ComboLastFrame = Processor.Combo;
            base.Update(gameTime);
        }
    }
}