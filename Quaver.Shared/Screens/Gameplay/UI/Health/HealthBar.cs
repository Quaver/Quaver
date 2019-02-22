/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.UI.Health
{
    public class HealthBar : AnimatableSprite
    {
        /// <summary>
        ///     The type of health bar this is. Whether horizontal or vertical.
        /// </summary>
        internal HealthBarType Type { get; }

        /// <summary>
        ///     The bar displayed in the foreground. This one dictates the amount
        ///     of health the user currently has.
        /// </summary>
        protected AnimatableSprite ForegroundBar { get; set; }

        /// <summary>
        ///     Reference to the current score processor.
        /// </summary>
        private ScoreProcessor Processor { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="processor"></param>
        internal HealthBar(HealthBarType type, ScoreProcessor processor) : base(SkinManager.Skin.HealthBarBackground)
        {
            Type = type;
            Processor = processor;

            Size = new ScalableVector2(Frames.First().Width, Frames.First().Height);

            // Start animation
            StartLoop(Direction.Forward, 60);

            // Create the foreground bar (the one that'll serve as the gauge progress).
            ForegroundBar = new AnimatableSprite(SkinManager.Skin.HealthBarForeground)
            {
                Parent = this,
                SpriteBatchOptions = new SpriteBatchOptions()
                {
                    SortMode = SpriteSortMode.Deferred,
                    BlendState = BlendState.NonPremultiplied,
                    SamplerState = SamplerState.PointClamp,
                    DepthStencilState = DepthStencilState.Default,
                    RasterizerState = RasterizerState.CullNone,
                    Shader = new Shader(GameBase.Game.Resources.Get("Quaver.Resources/Shaders/semi-transparent.mgfxo"), new Dictionary<string, object>()
                    {
                        {"p_position", new Vector2()},
                        {"p_rectangle", new Vector2()},
                        {"p_dimensions", new Vector2()},
                        {"p_alpha", 0f}
                    })
                }
            };

            ForegroundBar.Size = new ScalableVector2(ForegroundBar.Frames.First().Width, ForegroundBar.Frames.First().Height);

            // Start animation.
            ForegroundBar.StartLoop(Direction.Forward, 60);

            switch (Type)
            {
                case HealthBarType.Horizontal:
                    Alignment = Alignment.TopLeft;
                    ForegroundBar.Alignment = Alignment.TopLeft;

                    ForegroundBar.SpriteBatchOptions.Shader.SetParameter("p_position", new Vector2(ForegroundBar.Width, 0f), true);
                    break;
                case HealthBarType.Vertical:
                    Alignment = Alignment.BotLeft;
                    ForegroundBar.Alignment = Alignment.TopLeft;

                    ForegroundBar.SpriteBatchOptions.Shader.SetParameter("p_position", new Vector2(0, 0), true); 
                    break;
                default:
                    throw new NotImplementedException();
            }

            // Set default shader params.
            ForegroundBar.SpriteBatchOptions.Shader.SetParameter("p_rectangle", new Vector2(Width, Height), true);
            ForegroundBar.SpriteBatchOptions.Shader.SetParameter("p_dimensions", new Vector2(Width, Height), true);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Update Sprites
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            SetHealthBarProgress(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        ///        Moves the shader rect's position/size back and forth based on the user's health.
        ///        This creates that health gauge effect.
        /// </summary>
        /// <param name="gameTime"></param>
        private void SetHealthBarProgress(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            switch (Type)
            {
                // We handle horizontal bar types with the position in this case, so we can alpha mask it
                // with a full bar from right to left.
                case HealthBarType.Horizontal:
                    // Target position based on the user's current health.
                    var targetPosX = Processor.Health / 100 * ForegroundBar.Width;

                    var shaderPosition = (Vector2)ForegroundBar.SpriteBatchOptions.Shader.Parameters["p_position"];

                    var newPosX = MathHelper.Lerp(shaderPosition.X, targetPosX, (float) Math.Min(dt / 30, 1));
                    ForegroundBar.SpriteBatchOptions.Shader.SetParameter("p_position", new Vector2(newPosX, 0f), true);
                    break;
                // We handle vertical bar types with the size of the bar instead of position, since we're
                // going from top to bottom.
                case HealthBarType.Vertical:
                    // Get new size of the bar based on the user's current health.
                    var targetSizeY = ForegroundBar.Height - Processor.Health / 100 * ForegroundBar.Height;

                    var shaderRectangle = (Vector2)ForegroundBar.SpriteBatchOptions.Shader.Parameters["p_rectangle"];

                    var newSizeY = MathHelper.Lerp(shaderRectangle.Y, targetSizeY, (float) Math.Min(dt / 30, 1));
                    ForegroundBar.SpriteBatchOptions.Shader.SetParameter("p_rectangle", new Vector2(ForegroundBar.Width, newSizeY), true);
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }
        }
    }
}
