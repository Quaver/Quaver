/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Edit.Rulesets.Keys.Playfield
{
    public class EditorPlayfieldKeys : IEditorPlayfield
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorScreen Screen { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public GameMode Mode { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public Container Container { get; private set; }

        /// <summary>
        ///     The background of the playfield.
        /// </summary>
        public Container BackgroundContainer { get; private set; }

        /// <summary>
        ///     The scroll container, which contains HitObjects + snap lines.
        /// </summary>
        public EditorScrollContainerKeys ScrollContainer { get; private set; }

        /// <summary>
        ///     The size of each column.
        /// </summary>
        public float ColumnSize { get; } = 75;

        /// <summary>
        ///     The scroll speed of the objects in the editor.
        /// </summary>
        private float _scrollSpeed = 22;
        public float ScrollSpeed
        {
            get => _scrollSpeed / (20 * AudioEngine.Track.Rate);
            set => _scrollSpeed = value;
        }

        /// <summary>
        ///     The width of the playfield.
        /// </summary>
        public float Width => ColumnSize * Screen.Map.GetKeyCount();

        #region STAGE_SPRITES
        /// <summary>
        ///     The background sprite of the stage
        /// </summary>
        private Sprite StageBackground { get; set; }

        /// <summary>
        ///     The width of the StageLeft/StageRight
        /// </summary>
        private int StageBorderWidth { get; } = 1;

        /// <summary>
        ///     The left side of the stage.
        /// </summary>
        private Sprite StageLeft { get; set; }

        /// <summary>
        ///     The right side of the stage.
        /// </summary>
        private Sprite StageRight { get; set; }

        /// <summary>
        ///     The y position of where the hitposition is located.
        /// </summary>
        public float HitPositionY => StageBackground.Height - 200;

        /// <summary>
        ///     Sprite that displays where the hit position is.
        /// </summary>
        private Sprite StageHitPosition { get; set; }
        #endregion

        /// <summary>
        ///     The snap lines
        /// </summary>
        private EditorSnapLinesKeys SnapLines { get; set; }

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="mode"></param>
        public EditorPlayfieldKeys(EditorScreen screen, GameMode mode)
        {
            Screen = screen;
            Mode = mode;

            // Create parent container.
            Container = new Container();

            // Create container for background elements
            BackgroundContainer = new Container
            {
                Parent = Container,
                Size = new ScalableVector2(Width, WindowManager.Height),
                Alignment = Alignment.TopCenter
            };

            ScrollContainer = new EditorScrollContainerKeys(this);
            CreateStage();
            InitializeSnapLines();
        }

        /// <summary>
        ///     Creates a mask for the background of where the editor will live.
        /// </summary>
        private void CreateStage()
        {
            var columnRatio = Width / BackgroundContainer.Height;
            var bgMaskSize = Math.Max(WindowManager.Height * columnRatio, WindowManager.Height);

            StageBackground = new Sprite()
            {
                Image = UserInterface.BlankBox,
                Tint = Color.Black,
                Alpha = 0.5f,
                Size = new ScalableVector2(Width, bgMaskSize),
                Alignment = Alignment.MidCenter,
                Parent = BackgroundContainer
            };

            StageLeft = new Sprite()
            {
                Parent = BackgroundContainer,
                Tint = Color.Gray,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(StageBorderWidth, StageBackground.Height),
                Alpha = 0.5f
            };

            StageRight = new Sprite()
            {
                Parent = BackgroundContainer,
                Tint = Color.Gray,
                Alignment = Alignment.TopRight,
                Size = new ScalableVector2(StageBorderWidth, StageBackground.Height),
                Alpha = 0.5f,
                X = StageBorderWidth
            };

            StageHitPosition = new Sprite()
            {
                Parent = BackgroundContainer,
                Tint = Color.Green,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(Width - StageBorderWidth, 5),
                Y = HitPositionY
            };
        }

        /// <summary>
        ///     Initialzies the snap lines container.
        /// </summary>
        private void InitializeSnapLines() => SnapLines = new EditorSnapLinesKeys(Screen, this);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime) => Container?.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void Draw(GameTime gameTime) => Container?.Draw(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Destroy() => Container?.Destroy();
    }
}
