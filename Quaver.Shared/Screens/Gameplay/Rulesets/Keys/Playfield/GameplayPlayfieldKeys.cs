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
        public Container BackgroundContainer { get; }

        /// <summary>
        ///     The foreground of the playfield.
        /// </summary>
        public Container ForegroundContainer { get; }

        /// <summary>
        ///     The stage of the playfield.
        /// </summary>
        public GameplayPlayfieldKeysStage Stage { get; }

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

        /*
        /// <summary>
        ///     The Y position of the receptors.
        /// </summary>
        internal float ReceptorPositionY
        {
            get
            {
                var skin = SkinManager.Skin.Keys[Screen.Map.Mode];

                if (GameplayRulesetKeys.ScrollDirection.Equals(ScrollDirection.DownScroll))
                    return WindowManager.Height - (skin.ReceptorPosOffsetY + LaneSize * skin.NoteReceptorsUp[0].Height / skin.NoteReceptorsUp[0].Width);

                return skin.ReceptorPosOffsetY;
            }
        }*/

        /*
        /// <summary>
        ///     The Y position of the column lighting
        /// </summary>
        internal float ColumnLightingPositionY
        {
            get
            {
                if (GameplayRulesetKeys.ScrollDirection.Equals(ScrollDirection.DownScroll))
                    return ReceptorPositionY;

                var skin = SkinManager.Skin.Keys[Screen.Map.Mode];

                var receptor = skin.NoteReceptorsUp[0];
                var hitObject = skin.NoteHitObjects[0][0];
                return ReceptorPositionY + skin.ColumnSize * (float)((double)receptor.Height / receptor.Width - (double)hitObject.Height / hitObject.Width);
            }
        }*/

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

            var skin = SkinManager.Skin.Keys[Screen.Map.Mode];
            ReceptorPositionY = new float[ScrollDirections.Length];
            ColumnLightingPositionY = new float[ScrollDirections.Length];

            for (var i = 0; i < ScrollDirections.Length; i++)
            {
                switch (ScrollDirections[i])
                {
                    case ScrollDirection.DownScroll:
                        ReceptorPositionY[i] = WindowManager.Height - (skin.ReceptorPosOffsetY + LaneSize * skin.NoteReceptorsUp[0].Height / skin.NoteReceptorsUp[0].Width);
                        ColumnLightingPositionY[i] = ReceptorPositionY[i];
                        break;
                    case ScrollDirection.UpScroll:
                        // todo: reference current lane?
                        var receptor = skin.NoteReceptorsUp[0];
                        var hitObject = skin.NoteHitObjects[0][0];
                        ReceptorPositionY[i] = skin.ReceptorPosOffsetY;
                        ColumnLightingPositionY[i] = ReceptorPositionY[i] + skin.ColumnSize * (float)((double)receptor.Height / receptor.Width - (double)hitObject.Height / hitObject.Width);
                        break;
                    default:
                        throw new Exception($"Scroll Direction in current lane index {i} does not exist.");
                }
            }

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
                        case ScrollDirection.DownScroll:
                            ScrollDirections = new ScrollDirection[4]{
                                ScrollDirection.DownScroll,
                                ScrollDirection.DownScroll,
                                ScrollDirection.DownScroll,
                                ScrollDirection.DownScroll
                            };
                            break;
                        case ScrollDirection.UpScroll:
                            ScrollDirections = new ScrollDirection[4]{
                                ScrollDirection.UpScroll,
                                ScrollDirection.UpScroll,
                                ScrollDirection.UpScroll,
                                ScrollDirection.UpScroll
                            };
                            break;
                        default:
                            throw new Exception("Scroll Direction Config Value does not exist");
                    }
                    break;
                case GameMode.Keys7:
                    switch (ConfigManager.ScrollDirection7K.Value)
                    {
                        case ScrollDirection.DownScroll:
                            ScrollDirections = new ScrollDirection[7]{
                                ScrollDirection.DownScroll,
                                ScrollDirection.DownScroll,
                                ScrollDirection.DownScroll,
                                ScrollDirection.DownScroll,
                                ScrollDirection.DownScroll,
                                ScrollDirection.DownScroll,
                                ScrollDirection.DownScroll
                            };
                            break;
                        case ScrollDirection.UpScroll:
                            ScrollDirections = new ScrollDirection[7]{
                                ScrollDirection.UpScroll,
                                ScrollDirection.UpScroll,
                                ScrollDirection.UpScroll,
                                ScrollDirection.UpScroll,
                                ScrollDirection.UpScroll,
                                ScrollDirection.UpScroll,
                                ScrollDirection.UpScroll
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
    }
}
