/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Quaver.Shared.Skinning;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;
using  Microsoft.Xna.Framework;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield
{
    public class GameplayPlayfieldKeys : IGameplayPlayfield
    {
        /// <summary>
        ///     Reference to the current gameplay screen.
        /// </summary>
        public GameplayScreen Screen { get; }

        /// <summary>
        ///     Reference to the ruleset for the playfield.
        /// </summary>
        public GameplayRulesetKeys Ruleset { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>1
        public Container Container { get; set; }

        /// <summary>
        ///     The background of the playfield.
        /// </summary>
        public Container BackgroundContainer { get; private set; }

        /// <summary>
        ///     Where Hit Object and Timing Line elements are contained.
        /// </summary>
        public Container HitObjectContainer { get; private set; }

        /// <summary>
        ///     The foreground of the playfield.
        /// </summary>
        public Container ForegroundContainer { get; private set; }

        /// <summary>
        ///     The stage of the playfield.
        /// </summary>
        public GameplayPlayfieldKeysStage Stage { get; private set; }

        /// <summary>
        ///     The X size of the playfield.
        /// </summary>
        public float Width => (LaneSize + ReceptorPadding) * Screen.Map.GetKeyCount() + Padding * 2 - ReceptorPadding;

        /// <summary>
        ///     Padding of the playfield.
        /// </summary>
        public float Padding => SkinManager.Skin.Keys[Screen.Map.Mode].StageReceptorPadding;

        /// <summary>
        ///     The size of the each ane.
        /// </summary>
        public float LaneSize => SkinManager.Skin.Keys[Screen.Map.Mode].ColumnSize;

        /// <summary>
        ///     Padding of the receptor.
        /// </summary>
        internal float ReceptorPadding => SkinManager.Skin.Keys[Screen.Map.Mode].NotePadding;

        /// <summary>
        ///     Position for each Receptor in each lane from the top of the screen.
        /// </summary>
        internal float[] ReceptorPositionY { get; private set; }

        /// <summary>
        ///     Position for each Column Lighting relative from the top of the screen.
        /// </summary>
        internal float[] ColumnLightingPositionY { get; private set; }

        /// <summary>
        ///     HitObject Target Position from the relative top of the screen.
        /// </summary>
        internal float[] HitPositionY { get; private set; }

        /// <summary>
        ///     HeldHitObject Target Position relative from the top of the screen.
        /// </summary>
        internal float[] HoldHitPositionY { get; private set; }

        /// <summary>
        ///     Position for each Timing Line relative from the top of the screen.
        /// </summary>
        internal float[] TimingLinePositionY { get; private set; }

        /// <summary>
        ///     Size Adjustment for each LongNote in specific lane so the LN EndTime snaps with StartTime.
        /// </summary>
        internal float[] LongNoteSizeAdjustment { get; private set; }

        /// <summary>
        ///     Determines the scroll direction of each lane
        /// </summary>
        public ScrollDirection[] ScrollDirections { get; private set; }

        /// <summary>
        ///     This is used to render the Playfield itself only when Effects are enabled.
        /// </summary>
        private RenderTarget2D PlayfieldRenderer { get; set; }

        /// <summary>
        ///     This is used to render Playfield effects.
        /// </summary>
        private RenderTarget2D EffectRenderer { get; set; }

        /// <summary>
        ///     Everything rendered to the Playfield Renderer will be displayed in this sprite.
        /// </summary>
        private Sprite PlayfieldSprite { get; set; }

        /// <summary>
        ///     Used to draw the Perspective Playfield Plane Shape.
        /// </summary>
        private VertexPositionTexture[] PlayfieldVertices { get; set; }

        /// <summary>
        ///     Used to render the Perspective Playfield.
        /// </summary>
        private BasicEffect PerspectiveEffect { get; set; }

        /// <summary>
        ///     Total Subdivisions for the Persepective Playfield Plane (X).
        /// </summary>
        private const int PLANE_SUBDIVISIONS_COUNT_X = 64;

        /// <summary>
        ///     Total Subdivisions for the Persepective Playfield Plane (Y).
        /// </summary>
        private const int PLANE_SUBDIVISIONS_COUNT_Y = 64;

        /// <summary>
        ///     Max tilt value. 0.5f means that the texture will be 0 at one of the edges of the screen if the player uses the Max PlayfieldTilt value in their config.
        /// </summary>
        private const float TILT_MAX = 0.4f;

        /// <summary>
        ///     Determines how much stretching will be applied to the Playfield Plane. This value is used to give it a more "3D" feel rather than a distorted one.
        /// </summary>
        private const float TILT_EXP = 0.6f;

        /// <summary>
        ///
        /// </summary>
        private bool PerspectiveEnabled { get; set; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="ruleset"></param>
        public GameplayPlayfieldKeys(GameplayScreen screen, GameplayRulesetKeys ruleset)
        {
            Screen = screen;
            Ruleset = ruleset;
            Container = new Container();
            SetLaneScrollDirections();
            SetReferencePositions();
            CreateElementContainers();
            InitializePerspectiveElements();
        }

        /// <summary>
        ///     Create Foreground and Background Containers, as well as the Stage.
        /// </summary>
        private void CreateElementContainers()
        {
            // Create background container
            BackgroundContainer = new Container
            {
                Parent = Container,
                Size = new ScalableVector2(Width, WindowManager.Height),
                Alignment = Alignment.TopCenter,
                X = SkinManager.Skin.Keys[Screen.Map.Mode].ColumnAlignment
            };

            // Create Hit Object Container
            HitObjectContainer = new Container()
            {
                Parent = Container,
                Size = new ScalableVector2(Width, WindowManager.Height),
                Alignment = Alignment.TopCenter,
                X = SkinManager.Skin.Keys[Screen.Map.Mode].ColumnAlignment
            };

            // Create the foreground container.
            ForegroundContainer = new Container
            {
                Parent = Container,
                Size = new ScalableVector2(Width, WindowManager.Height),
                Alignment = Alignment.TopCenter,
                X = SkinManager.Skin.Keys[Screen.Map.Mode].ColumnAlignment
            };

            Stage = new GameplayPlayfieldKeysStage(Screen, this);
        }

        /// <summary>
        ///     Returns Array of Scroll Directions for each specific lane for a specific GameMode.
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private void SetLaneScrollDirections()
        {
            var keys = MapManager.Selected.Value.Qua.GetKeyCount();

            ScrollDirection direction;
            switch (Ruleset.Map.Mode)
            {
                case GameMode.Keys4:
                    direction = ConfigManager.ScrollDirection4K.Value;
                    break;
                case GameMode.Keys7:
                    direction = ConfigManager.ScrollDirection7K.Value;
                    break;
                default:
                    throw new Exception("Map Mode does not exist.");
            }

            // Case: Config = Split Scroll
            if (direction.Equals(ScrollDirection.Split))
            {
                var halfIndex = (int)Math.Ceiling(keys / 2.0);
                ScrollDirections = new ScrollDirection[keys];
                for (var i = 0; i < keys; i++)
                {
                    if (i >= halfIndex)
                        ScrollDirections[i] = ScrollDirection.Up;
                    else
                        ScrollDirections[i] = ScrollDirection.Down;
                }
                return;
            }

            // Case: Config = Down/Up Scroll
            ScrollDirections = Enumerable.Repeat(direction, keys).ToArray();
        }

        /// <summary>
        ///     Set Positions for Receptor, Column Lighting, Hit Position, and Timing Lines
        /// </summary>
        private void SetReferencePositions()
        {
            var skin = SkinManager.Skin.Keys[Screen.Map.Mode];
            ReceptorPositionY = new float[ScrollDirections.Length];
            ColumnLightingPositionY = new float[ScrollDirections.Length];
            HitPositionY = new float[ScrollDirections.Length];
            HoldHitPositionY = new float[ScrollDirections.Length];
            TimingLinePositionY = new float[ScrollDirections.Length];
            LongNoteSizeAdjustment = new float[ScrollDirections.Length];

            for (var i = 0; i < ScrollDirections.Length; i++)
            {
                var hitObOffset = LaneSize * skin.NoteHitObjects[i][0].Height / skin.NoteHitObjects[i][0].Width;
                var holdHitObOffset = LaneSize * skin.NoteHoldHitObjects[i][0].Height / skin.NoteHoldHitObjects[i][0].Width;
                var holdEndOffset = LaneSize * skin.NoteHoldEnds[i].Height / skin.NoteHoldEnds[i].Width;
                var receptorOffset = LaneSize * skin.NoteReceptorsUp[i].Height / skin.NoteReceptorsUp[i].Width;

                if (SkinManager.Skin.Keys[Screen.Map.Mode].DrawLongNoteEnd)
                    LongNoteSizeAdjustment[i] = (holdHitObOffset - holdEndOffset) / 2;
                else
                    LongNoteSizeAdjustment[i] = holdHitObOffset / 2;

                switch (ScrollDirections[i])
                {
                    case ScrollDirection.Down:
                        ReceptorPositionY[i] = WindowManager.Height - skin.ReceptorPosOffsetY - receptorOffset;
                        ColumnLightingPositionY[i] = ReceptorPositionY[i] - skin.ColumnLightingOffsetY - skin.ColumnLightingScale * LaneSize * skin.ColumnLighting.Height / skin.ColumnLighting.Width;
                        HitPositionY[i] = ReceptorPositionY[i] + skin.HitPosOffsetY - hitObOffset;
                        HoldHitPositionY[i] = ReceptorPositionY[i] + skin.HitPosOffsetY - holdHitObOffset;
                        TimingLinePositionY[i] = ReceptorPositionY[i] + skin.HitPosOffsetY;
                        break;
                    case ScrollDirection.Up:
                        ReceptorPositionY[i] = skin.ReceptorPosOffsetY;
                        HitPositionY[i] = ReceptorPositionY[i] - skin.HitPosOffsetY + receptorOffset;
                        HoldHitPositionY[i] = ReceptorPositionY[i] - skin.HitPosOffsetY + receptorOffset;
                        ColumnLightingPositionY[i] = ReceptorPositionY[i] + receptorOffset + skin.ColumnLightingOffsetY;
                        TimingLinePositionY[i] = HitPositionY[i];
                        break;
                    default:
                        throw new Exception($"Scroll Direction in current lane index {i} does not exist.");
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            Stage.Update(gameTime);
            Container?.Update(gameTime);
        }

        /// <summary>
        ///     Initializes Stage Perspective Elements
        /// </summary>
        private void InitializePerspectiveElements()
        {
            // Check if Playfield should be tilted.
            float tilt;
            switch (Ruleset.Map.Mode)
            {
                case GameMode.Keys4:
                    tilt = ConfigManager.PlayfieldTilt4K.Value;
                    break;
                case GameMode.Keys7:
                    tilt = ConfigManager.PlayfieldTilt7K.Value;
                    break;
                default:
                    throw new Exception("Map Mode does not exist.");

            }

            PerspectiveEnabled = tilt != 0;

            // Ignore this method if Playfield is not tilted.
            if (!PerspectiveEnabled)
                return;

            // Create Renderers and reference Window Size
            var width = (int)WindowManager.Width;
            var height = (int)WindowManager.Height;
            Console.WriteLine($"width: {width}, height: {height}");

            PlayfieldRenderer = new RenderTarget2D(GameBase.Game.GraphicsDevice, GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferWidth, GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferHeight, false,
                GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            EffectRenderer = new RenderTarget2D(GameBase.Game.GraphicsDevice, GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferWidth, GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferHeight, false,
                GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            // Compute for tilt.
            tilt = width * TILT_MAX * (tilt / 100f);
            tilt = ScrollDirections[0] == ScrollDirection.Down ? tilt : tilt * -1;

            // Compute for warp.
            var warp = 1f;
            //var warp = 1 / ((-tilt / (width * TILT_MAX)) * TILT_EXP + 1);

            // Create Playfield Plane Points
            var points = new List<Vector2>();
            for (var i = 0; i < PLANE_SUBDIVISIONS_COUNT_X; i++)
            {
                for (var j = 0; j < PLANE_SUBDIVISIONS_COUNT_Y; j++)
                {
                    // Tri 1
                    points.Add(new Vector2((float)i / PLANE_SUBDIVISIONS_COUNT_X, (float)j / PLANE_SUBDIVISIONS_COUNT_Y  ));
                    points.Add(new Vector2((float)(i + 1) / PLANE_SUBDIVISIONS_COUNT_X, (float)j / PLANE_SUBDIVISIONS_COUNT_Y  ));
                    points.Add(new Vector2((float)i / PLANE_SUBDIVISIONS_COUNT_X, (float)(j + 1) / PLANE_SUBDIVISIONS_COUNT_Y  ));

                    // Tri 2p
                    points.Add(new Vector2((float)i/ PLANE_SUBDIVISIONS_COUNT_X, (float)(j + 1) / PLANE_SUBDIVISIONS_COUNT_Y  ));
                    points.Add(new Vector2((float)(i + 1) / PLANE_SUBDIVISIONS_COUNT_X, (float)j / PLANE_SUBDIVISIONS_COUNT_Y  ));
                    points.Add(new Vector2((float)(i + 1) / PLANE_SUBDIVISIONS_COUNT_X, (float)(j + 1) / PLANE_SUBDIVISIONS_COUNT_Y  ));
                }
            }

            // Create Playfield Plane
            PlayfieldVertices = new VertexPositionTexture[PLANE_SUBDIVISIONS_COUNT_X * PLANE_SUBDIVISIONS_COUNT_Y * 6];
            for (var i = 0; i < points.Count; i++)
            {
                var posY = ScrollDirections[0] == ScrollDirection.Down ? (float)Math.Pow(points[i].Y, warp) : (float)Math.Pow(points[i].Y, warp);
                var posX = tilt * (2f * (points[i].Y - 0.5f));
                PlayfieldVertices[i].Position = new Vector3(points[i].X * (width + posX * 2) - posX, posY * height, 0);
                PlayfieldVertices[i].TextureCoordinate = points[i];
            }

            // Create Perspective Effect
            var projection = Matrix.CreateOrthographicOffCenter(0, width, height, 0, 0, (PLANE_SUBDIVISIONS_COUNT_X - 1) * (PLANE_SUBDIVISIONS_COUNT_Y - 1) );
            PerspectiveEffect = new BasicEffect(GameBase.Game.GraphicsDevice)
            {
                TextureEnabled = true,
                World = Matrix.Identity,
                View = Matrix.Identity,
                Projection = projection,
                Texture = PlayfieldRenderer
            };

            // Create Playfield Sprite. Image from Effect Renderer will be displayed on this.
            PlayfieldSprite = new Sprite()
            {
                Parent = Container,
                Size = new ScalableVector2(width, height),
                Image = EffectRenderer
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void PreDraw(GameTime gameTime)
        {
            // If Config for Perspective is not enabled, ignore this method.
            if (!PerspectiveEnabled)
                return;

            // Stop the sprite batch to render the Playfield
            try
            {
                GameBase.Game.SpriteBatch.End();
            }
            catch (Exception e)
            {
                // Ignored.
            }

            // Draw Playfield Texture
            GameBase.Game.GraphicsDevice.SetRenderTarget(PlayfieldRenderer);
            GameBase.Game.GraphicsDevice.Clear(Color.Transparent);
            GameBase.Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            BackgroundContainer.Draw(gameTime);
            HitObjectContainer.Draw(gameTime);
            GameBase.Game.SpriteBatch.End();

            // Draw Perspective
            GameBase.Game.GraphicsDevice.SetRenderTarget(EffectRenderer);
            foreach (var pass in PerspectiveEffect.CurrentTechnique.Passes)
                pass.Apply();

            GameBase.Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, PlayfieldVertices, 0, PLANE_SUBDIVISIONS_COUNT_X * PLANE_SUBDIVISIONS_COUNT_Y * 2,
                VertexPositionTexture.VertexDeclaration);

            // Reinitialize Sprite Batch for Draw()
            GameBase.Game.GraphicsDevice.SetRenderTarget(null);
            GameBase.DefaultSpriteBatchOptions.Begin();
            //GameBase.Game.SpriteBatch.Begin();
            //GameBase.Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void Draw(GameTime gameTime)
        {
            // Draw Regular Playfield if Perspective is disabled.
            if (!PerspectiveEnabled)
            {
                BackgroundContainer.Draw(gameTime);
                HitObjectContainer.Draw(gameTime);
                ForegroundContainer.Draw(gameTime);
                return;
            }

            // Draw Perspective Playfield if Config is enabled.
            PlayfieldSprite.Draw(gameTime);
            ForegroundContainer.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Destroy()
        {
            if (PerspectiveEnabled)
            {
                PlayfieldRenderer.Dispose();
                EffectRenderer.Dispose();
            }

            Container?.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void HandleFailure(GameTime gameTime)
        {
        }
    }
}
