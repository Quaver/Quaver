/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.API.Replays;
using Quaver.API.Replays.Virtual;
using Quaver.Shared.Config;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Input;
using Quaver.Shared.Screens.Gameplay.UI.Scoreboard;
using WebSocketSharp;
using Wobble.Logging;
using Logger = Wobble.Logging.Logger;

namespace Quaver.Shared.Screens.Gameplay.Rulesets
{
    public abstract class GameplayRuleset : IGameScreenComponent
    {
        /// <summary>
        ///     Reference to the parent gameplay screen
        /// </summary>
        public GameplayScreen Screen { get; }

        /// <summary>
        ///     The map being played
        /// </summary>
        public Qua Map { get; }

        /// <summary>
        ///     The game mode of the map being played.
        /// </summary>
        public GameMode Mode => Map.Mode;

        /// <summary>
        ///     The playfield for this ruleset.
        /// </summary>
        public IGameplayPlayfield Playfield { get; protected set; }

        /// <summary>
        ///     The input manager for this ruleset.
        /// </summary>
        public IGameplayInputManager InputManager { get; protected set; }

        /// <summary>
        ///     Manages all of the HitObjects for the screen.
        /// </summary>
        public HitObjectManager HitObjectManager { get; private set; }

        /// <summary>
        ///     Manages all the scoring for this play session and ruleset.
        /// </summary>
        public ScoreProcessor ScoreProcessor { get; set; }

        /// <summary>
        ///    Handles the *real* scoring values with standardized judgements.
        ///     <see cref="ScoreProcessor"/> can have custom windows. This is used
        ///     to calculate the player's real score
        /// </summary>
        public VirtualReplayPlayer StandardizedReplayPlayer { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="map"></param>
        protected GameplayRuleset(GameplayScreen screen, Qua map)
        {
            Screen = screen;
            Map = map;
            ScoreProcessor = CreateScoreProcessor(Map);
            InputManager = CreateInputManager();
            CreatePlayfield();
            HitObjectManager = CreateHitObjectManager();

            StandardizedReplayPlayer = new VirtualReplayPlayer(new Replay(Map.Mode,
                ConfigManager.Username.Value, ModManager.Mods, Screen.MapHash), map, null, true);

            // If in replay mode, pass all the existing replay frames to the standardized player.
            // there's no need to manually dump frames as the play goes on.
            if (Screen.InReplayMode && Screen.SpectatorClient == null)
            {
                var inputManager = (KeysInputManager) InputManager;

                StandardizedReplayPlayer.Replay.Frames = inputManager.ReplayInputManager.Replay.Frames;
                StandardizedReplayPlayer.PlayAllFrames();
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Updates the game ruleset.
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            if (!Screen.IsPaused)
                HitObjectManager.Update(gameTime);

            Playfield.Update(gameTime);
            UpdateStandardizedScoreProcessor();
        }

        /// <summary>
        ///     Handles the input of the game mode ruleset.
        /// </summary>
        /// <param name="gameTime"></param>
        public void HandleInput(GameTime gameTime) => InputManager.HandleInput(gameTime.ElapsedGameTime.TotalMilliseconds);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void Draw(GameTime gameTime) => Playfield.Draw(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Destroy()
        {
            Playfield.Destroy();
            HitObjectManager.Destroy();
        }

        /// <summary>
        ///     Polls <see cref="ReplayCapturer"/> and updates <see cref="StandardizedReplayPlayer"/>
        ///     with the standardized scoring values
        /// </summary>
        public void UpdateStandardizedScoreProcessor(bool force = false)
        {
            // No need to update the processor in replay mode
            if (Screen.InReplayMode && Screen.SpectatorClient == null)
                return;

            if (StandardizedReplayPlayer.CurrentFrame >= Screen.ReplayCapturer.Replay.Frames.Count - 1 && !force)
                return;

            for (var i = StandardizedReplayPlayer.Replay.Frames.Count; i < Screen.ReplayCapturer.Replay.Frames.Count; i++)
            {
                var frame = Screen.ReplayCapturer.Replay.Frames[i];
                StandardizedReplayPlayer.Replay.AddFrame(frame.Time, frame.Keys);
                StandardizedReplayPlayer.PlayNextFrame();

                var view = (GameplayScreenView) Screen.View;
                view.UpdateScoreAndAccuracyDisplays();

                if (view.ScoreboardLeft?.Users.Count != 0 && view.ScoreboardLeft?.Users.First().Type == ScoreboardUserType.Self)
                    view.ScoreboardLeft.Users.First().CalculateScoreForNextObject();

                if (view.ScoreboardRight?.Users.Count != 0 && view.ScoreboardRight?.Users.First().Type == ScoreboardUserType.Self)
                    view.ScoreboardRight.Users.First().CalculateScoreForNextObject();
            }
        }

        /// <summary>
        ///     Creates the score processor for this ruleset.
        /// </summary>
        /// <returns></returns>
        protected abstract ScoreProcessor CreateScoreProcessor(Qua map);

        /// <summary>
        ///     Creates the playfield for the ruleset.
        /// </summary>
        protected abstract void CreatePlayfield();

        /// <summary>
        ///     Creates a custom HitObjectManager for this ruleset.
        /// </summary>
        protected abstract HitObjectManager CreateHitObjectManager();

        /// <summary>
        ///     Creates the input manager for the ruleset.
        /// </summary>
        /// <returns></returns>
        protected abstract IGameplayInputManager CreateInputManager();
    }
}
