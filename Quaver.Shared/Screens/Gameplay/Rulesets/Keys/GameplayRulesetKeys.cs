/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Processors.Scoring.Multiplayer;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Input;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield.Lines;
using Quaver.Shared.Skinning;
using Steamworks;
using Wobble.Logging;

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
        public List<TimingLineManager> TimingLineManagers { get; } = new List<TimingLineManager>();

        /// <summary>
        ///     Dictates if we are currently using downscroll or not.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static ScrollDirection ScrollDirection
        {
            get
            {
                if (MapManager.Selected.Value.Qua != null)
                    return ConfigManager.ScrollDirections[MapManager.Selected.Value.Qua.Mode].Value;

                return ConfigManager.ScrollDirections[GameMode.Keys4].Value;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="map"></param>
        public GameplayRulesetKeys(GameplayScreen screen, Qua map) : base(screen, map) => InitializeTimingLines();

        /// <summary>
        ///     Generate Timing Line Managers for scroll direction. Will create multiple managers if  multiple scroll directions exist.
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="direction"></param>
        private void InitializeTimingLines()
        {
            // Do not create timing lines if DisplayTimingLines config is turned off.
            if (!ConfigManager.DisplayTimingLines.Value)
                return;

            var playfield = (GameplayPlayfieldKeys)Playfield;
            var keys = MapManager.Selected.Value.Qua?.GetKeyCount() ?? 4;

            var lastDir = playfield.ScrollDirections[0];
            var lastWidth = playfield.Stage.Receptors[0].Width;
            var lastPosX = playfield.Stage.Receptors[0].X;
            var lastPosY = playfield.TimingLinePositionY[0];
            for (var i = 1; i <= keys; i++)
            {
                if (i == keys || playfield.ScrollDirections[i] != lastDir || playfield.TimingLinePositionY[i] != lastPosY)
                {
                    TimingLineManagers.Add(new TimingLineManager(this, lastDir, lastPosY, lastWidth, lastPosX));

                    if (i == keys)
                        break;

                    lastWidth = 0;
                    lastDir = playfield.ScrollDirections[i];
                    lastPosX = playfield.Stage.Receptors[i].X;
                    lastPosY = playfield.TimingLinePositionY[i];
                }

                lastWidth += playfield.Stage.Receptors[i].Width;
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // This should be _after_ base.Update, since this uses HitObjectManager.CurrentTrackPosition,
            // which is updated in base.Update.
            if (TimingLineManagers != null)
                foreach (var manager in TimingLineManagers) manager.UpdateTimingLines();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        protected override ScoreProcessor CreateScoreProcessor(Qua map)
        {
            var windows = JudgementWindowsDatabaseCache.Selected.Value;
            ScoreProcessor processor;

            if (Screen.IsMultiplayerGame)
            {
                processor = new ScoreProcessorKeys(map, ModManager.Mods,
                    new ScoreProcessorMultiplayer((MultiplayerHealthType)OnlineManager.CurrentGame.HealthType, OnlineManager.CurrentGame.Lives), windows);
            }
            else
            {
                processor = new ScoreProcessorKeys(map, ModManager.Mods, windows);
            }

            processor.PlayerName = ConfigManager.Username.Value;
            processor.SteamId = SteamUser.GetSteamID().m_SteamID;
            processor.UserId = OnlineManager.Self?.OnlineUser?.Id ?? 0;

            Logger.Debug("---- Health Weighting ----", LogType.Runtime);

            foreach (var weight in processor.JudgementHealthWeighting)
                Logger.Debug($"{weight.Key}: {weight.Value}", LogType.Runtime);

            return processor;
        }

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
