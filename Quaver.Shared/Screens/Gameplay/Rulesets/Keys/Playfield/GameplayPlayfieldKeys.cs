/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Quaver.Shared.Skinning;
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
        ///     The Y position of the receptors.
        /// </summary>
        internal float ReceptorPositionY
        {
            get
            {
                var skin = SkinManager.Skin.Keys[Screen.Map.Mode];

                if (GameplayRulesetKeys.IsDownscroll)
                    return WindowManager.Height - (skin.ReceptorPosOffsetY + LaneSize * skin.NoteReceptorsUp[0].Height / skin.NoteReceptorsUp[0].Width);

                return skin.ReceptorPosOffsetY;
            }
        }

        /// <summary>
        ///     The Y position of the column lighting
        /// </summary>
        internal float ColumnLightingPositionY
        {
            get
            {
                if (GameplayRulesetKeys.IsDownscroll)
                    return ReceptorPositionY;

                var skin = SkinManager.Skin.Keys[Screen.Map.Mode];

                var receptor = skin.NoteReceptorsUp[0];
                var hitObject = skin.NoteHitObjects[0][0];
                return ReceptorPositionY + skin.ColumnSize * (float)((double)receptor.Height / receptor.Width - (double)hitObject.Height / hitObject.Width);
            }
        }

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

            // Create background container
            BackgroundContainer = new Container
            {
                Parent = Container,
                Size = new ScalableVector2(Width, WindowManager.Height),
                Alignment = Alignment.TopCenter,
                X = SkinManager.Skin.Keys[screen.Map.Mode].ColumnAlignment
            };

            // Create the foreground container.
            ForegroundContainer = new Container
            {
                Parent = Container,
                Size = new ScalableVector2(Width, WindowManager.Height),
                Alignment = Alignment.TopCenter,
                X = BackgroundContainer.X
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
    }
}
