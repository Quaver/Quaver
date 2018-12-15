/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Quaver.Shared.Skinning;
using System;
using Wobble.Graphics;
using Wobble.Window;

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
        /// </summary>
        public Container Container { get; set; }

        /// <summary>
        ///     The background of the playfield.
        /// </summary>
        public Container BackgroundContainer { get; private set; }

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
        public float Padding => SkinManager.Skin.Keys[Screen.Map.Mode].BgMaskPadding;

        /// <summary>
        ///     The size of the each ane.
        /// </summary>
        public float LaneSize => SkinManager.Skin.Keys[Screen.Map.Mode].ColumnSize;

        /// <summary>
        ///     Padding of the receptor.
        /// </summary>
        internal float ReceptorPadding => SkinManager.Skin.Keys[Screen.Map.Mode].NotePadding;

        /// <summary>
        ///     Position for each Receptor in each lane
        /// </summary>
        internal float[] ReceptorPositionY { get; set; }

        /// <summary>
        ///     Position for each Column Lighting
        /// </summary>
        internal float[] ColumnLightingPositionY { get; set; }

        /// <summary>
        ///     Determines the scroll direction of each lane
        /// </summary>
        public ScrollDirection[] ScrollDirections { get; private set; }

        /// <summary>
        ///     The offset from the edge of the screen of the hit position.
        /// </summary>
        public float[] HitPositionOffsets { get; private set; }

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
            DetermineScrollDirections();
            SetReceptorPositions();
            ApplyHitPositionsOffset();
            CreateElementContainers();
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
                Alignment = Alignment.TopCenter
            };

            // Create the foreground container.
            ForegroundContainer = new Container
            {
                Parent = Container,
                Size = new ScalableVector2(Width, WindowManager.Height),
                Alignment = Alignment.TopCenter
            };

            Stage = new GameplayPlayfieldKeysStage(Screen, this);
        }

        /// <summary>
        ///     Determine the Scroll Directions for each Lane.
        /// </summary>
        private void DetermineScrollDirections()
        {
            switch (Ruleset.Map.Mode)
            {
                case GameMode.Keys4:
                    switch (ConfigManager.ScrollDirection4K.Value)
                    {
                        case ScrollDirection.Down:
                            ScrollDirections = new ScrollDirection[4]{
                                ScrollDirection.Down,
                                ScrollDirection.Down,
                                ScrollDirection.Down,
                                ScrollDirection.Down
                            };
                            break;
                        case ScrollDirection.Up:
                            ScrollDirections = new ScrollDirection[4]{
                                ScrollDirection.Up,
                                ScrollDirection.Up,
                                ScrollDirection.Up,
                                ScrollDirection.Up
                            };
                            break;
                        case ScrollDirection.Split:
                            ScrollDirections = new ScrollDirection[4]{
                                ScrollDirection.Down,
                                ScrollDirection.Down,
                                ScrollDirection.Up,
                                ScrollDirection.Up
                            };
                            break;
                        default:
                            throw new Exception("Scroll Direction Config Value does not exist");
                    }
                    break;
                case GameMode.Keys7:
                    switch (ConfigManager.ScrollDirection7K.Value)
                    {
                        case ScrollDirection.Down:
                            ScrollDirections = new ScrollDirection[7]{
                                ScrollDirection.Down,
                                ScrollDirection.Down,
                                ScrollDirection.Down,
                                ScrollDirection.Down,
                                ScrollDirection.Down,
                                ScrollDirection.Down,
                                ScrollDirection.Down
                            };
                            break;
                        case ScrollDirection.Up:
                            ScrollDirections = new ScrollDirection[7]{
                                ScrollDirection.Up,
                                ScrollDirection.Up,
                                ScrollDirection.Up,
                                ScrollDirection.Up,
                                ScrollDirection.Up,
                                ScrollDirection.Up,
                                ScrollDirection.Up
                            };
                            break;
                        case ScrollDirection.Split:
                            ScrollDirections = new ScrollDirection[7]{
                                ScrollDirection.Down,
                                ScrollDirection.Down,
                                ScrollDirection.Down,
                                ScrollDirection.Down,
                                ScrollDirection.Up,
                                ScrollDirection.Up,
                                ScrollDirection.Up
                            };
                            break;
                        default:
                            throw new Exception("Scroll Direction Config Value does not exist");
                    }
                    break;
                default:
                    throw new Exception("Map Mode does not exist.");
            }
        }

        /// <summary>
        ///     Set Positions for Receptor and Hit Lighting
        /// </summary>
        private void SetReceptorPositions()
        {
            var skin = SkinManager.Skin.Keys[Screen.Map.Mode];
            ReceptorPositionY = new float[ScrollDirections.Length];
            ColumnLightingPositionY = new float[ScrollDirections.Length];

            for (var i = 0; i < ScrollDirections.Length; i++)
            {
                switch (ScrollDirections[i])
                {
                    case ScrollDirection.Down:
                        ReceptorPositionY[i] = WindowManager.Height - (skin.ReceptorPosOffsetY + LaneSize * skin.NoteReceptorsUp[i].Height / skin.NoteReceptorsUp[i].Width);
                        ColumnLightingPositionY[i] = ReceptorPositionY[i];
                        break;
                    case ScrollDirection.Up:
                        // todo: reference current lane?
                        var receptor = skin.NoteReceptorsUp[i];
                        var hitObject = skin.NoteHitObjects[i][0];
                        ReceptorPositionY[i] = skin.ReceptorPosOffsetY;
                        ColumnLightingPositionY[i] = ReceptorPositionY[i] + skin.ColumnSize * (float)((double)receptor.Height / receptor.Width - (double)hitObject.Height / hitObject.Width);
                        break;
                    default:
                        throw new Exception($"Scroll Direction in current lane index {i} does not exist.");
                }
            }
        }

        /// <summary>
        ///     Set Hit Position Variables to every lane that each Hit Object will reference. 
        /// </summary>
        private void ApplyHitPositionsOffset()
        {
            var skin = SkinManager.Skin.Keys[Ruleset.Mode];
            HitPositionOffsets = new float[ScrollDirections.Length];

            for (var i = 0; i < ScrollDirections.Length; i++)
            {
                switch (ScrollDirections[i])
                {
                    case ScrollDirection.Down:
                        HitPositionOffsets[i] = ReceptorPositionY[i] + skin.HitPosOffsetY;
                        break;
                    case ScrollDirection.Up:
                        HitPositionOffsets[i] = ReceptorPositionY[i] - skin.HitPosOffsetY;
                        break;
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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void Draw(GameTime gameTime) => Container.Draw(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Destroy() => Container?.Destroy();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void HandleFailure(GameTime gameTime)
        {
        }
    }
}
