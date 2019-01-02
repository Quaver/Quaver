/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Input;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield.Lines;
using Quaver.Shared.Skinning;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys
{
    public class GameplayRulesetKeys : GameplayRuleset
    {
        /// <summary>
        ///     Reference to the timing line manager.
        ///
        ///     It gets initialized in GameplayRulesetKeys because it relies on both
        ///     the playfield and the HitObjectManager.
        ///
        ///     We can't intiialize it in Playfield as that gets created first.
        ///
        ///     This is a list because multiple scroll directions require multiple Timing Line Managers.
        /// 
        /// </summary>
        public List<TimingLineManager> TimingLineManager { get; } = new List<TimingLineManager>();

        /// <summary>
        ///     Dictates if we are currently using downscroll or not.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static ScrollDirection ScrollDirection
        {
            get
            {
                switch (MapManager.Selected.Value.Qua.Mode)
                {
                    case GameMode.Keys4:
                        return ConfigManager.ScrollDirection4K.Value;
                    case GameMode.Keys7:
                        return ConfigManager.ScrollDirection7K.Value;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="map"></param>
        public GameplayRulesetKeys(GameplayScreen screen, Qua map) : base(screen, map) => InitializeTimingLines();

        /// <summary>
        ///     Initialiize Timing Line Manager(s) depending on Scroll Direction.
        /// </summary>
        private void InitializeTimingLines()
        {
            // Create appropriate Timing Line Managers.
            // - Multiple Timing Line Managers for multiple Scroll Directions
            if (ConfigManager.DisplayTimingLines.Value)
            {
                var playfield = (GameplayPlayfieldKeys)Playfield;
                switch (MapManager.Selected.Value.Qua.Mode)
                {
                    case GameMode.Keys4:
                        CreateTimingLineManagers(4, ConfigManager.ScrollDirection4K.Value);
                        break;
                    case GameMode.Keys7:
                        CreateTimingLineManagers(7, ConfigManager.ScrollDirection7K.Value);
                        break;
                    default:
                        throw new Exception("Game Mode does not exist");
                }
            }
        }

        /// <summary>
        ///     Generate Timing Line Managers for scroll direction. Will create multiple managers if  multiple scroll directions exist.
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="direction"></param>
        private void CreateTimingLineManagers(int keys, ScrollDirection direction)
        {
            var playfield = (GameplayPlayfieldKeys)Playfield;
            if (direction.Equals(ScrollDirection.Split))
            {
                var halfIndex = keys / 2 - 1;
                var halfPos = playfield.Stage.Receptors[halfIndex].X + playfield.Stage.Receptors[halfIndex].Width;
                TimingLineManager.Add(new TimingLineManager(this, ScrollDirection.Down, playfield.HitPositionOffsets[0], halfPos, 0));
                TimingLineManager.Add(new Keys.Playfield.Lines.TimingLineManager(this, ScrollDirection.Up, playfield.ColumnLightingPositionY[halfIndex], playfield.Width - halfPos, halfPos));
                return;
            }
            TimingLineManager.Add(new Keys.Playfield.Lines.TimingLineManager(this, direction, playfield.HitPositionOffsets[0], playfield.Width, 0));
        }

        /// <inheritdoc />
        /// <summary>
        ///
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (!Screen.Failed && !Screen.IsPaused && TimingLineManager != null)
                foreach (var manager in TimingLineManager) manager.UpdateObjectPool();

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        protected override ScoreProcessor CreateScoreProcessor(Qua map) => new ScoreProcessorKeys(map, ModManager.Mods);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void CreatePlayfield() => Playfield = new GameplayPlayfieldKeys(Screen, this);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected override HitObjectManager CreateHitObjectManager() => new HitObjectManagerKeys(this, Map);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected override IGameplayInputManager CreateInputManager() => new KeysInputManager(this, Map.Mode);
    }
}
