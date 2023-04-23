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
                switch (MapManager.Selected.Value?.Qua?.Mode)
                {
                    case GameMode.Keys4:
                        return ConfigManager.ScrollDirection4K.Value;
                    case GameMode.Keys7:
                        return ConfigManager.ScrollDirection7K.Value;
                    default:
                        return ConfigManager.ScrollDirection4K.Value;
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
        ///     Generate Timing Line Managers for scroll direction. Will create multiple managers if  multiple scroll directions exist.
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="direction"></param>
        private void InitializeTimingLines()
        {
            // Do not create timing lines if DisplayTimingLines config is turned off.
            if (!ConfigManager.DisplayTimingLines.Value)
                return;

            var direction = ScrollDirection;
            var playfield = (GameplayPlayfieldKeys)Playfield;
            var keys = MapManager.Selected.Value.Qua?.GetKeyCount() ?? 4;

            if (direction.Equals(ScrollDirection.Split))
            {
                var halfIndex = (int)Math.Ceiling(keys / 2.0) - 1;
                var halfPos = playfield.Stage.Receptors[halfIndex].X + playfield.Stage.Receptors[halfIndex].Width;
                TimingLineManagers.Add(new TimingLineManager(this, ScrollDirection.Down, playfield.TimingLinePositionY[0], halfPos, 0));
                TimingLineManagers.Add(new TimingLineManager(this, ScrollDirection.Up, playfield.TimingLinePositionY[halfIndex + 1], playfield.Width - halfPos, halfPos));
                return;
            }
            TimingLineManagers.Add(new TimingLineManager(this, direction, playfield.TimingLinePositionY[0], playfield.Width, 0));
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
                    new ScoreProcessorMultiplayer((MultiplayerHealthType) OnlineManager.CurrentGame.HealthType, OnlineManager.CurrentGame.Lives), windows);
            }
            else
            {
                processor = new ScoreProcessorKeys(map, ModManager.Mods, windows);
            }

            processor.PlayerName = ConfigManager.Username.Value;
            processor.SteamId = SteamUser.GetSteamID().m_SteamID;
            processor.UserId = OnlineManager.Self?.OnlineUser?.Id ?? 0;

            Logger.Important($"---- Health Weighting ----", LogType.Runtime);

            foreach (var weight in processor.JudgementHealthWeighting)
                Logger.Important($"{weight.Key}: {weight.Value}", LogType.Runtime);

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
