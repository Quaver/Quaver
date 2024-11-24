/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.API.Replays;
using Quaver.API.Replays.Virtual;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Quaver.Shared.Screens.Tournament.Gameplay;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Input
{
    public class ReplayInputManagerKeys
    {
        /// <summary>
        ///     Reference to the actual gameplay screen.
        /// </summary>
        private GameplayScreen Screen { get; }

        /// <summary>
        ///     Reference to the hitobject manager.
        /// </summary>
        private HitObjectManagerKeys Manager => Screen.Ruleset.HitObjectManager as HitObjectManagerKeys;

        /// <summary>
        ///     The replay that is currently loaded.
        /// </summary>
        internal Replay Replay { get; }

        /// <summary>
        ///     The frame that we are currently on in the replay.
        /// </summary>
        internal int CurrentFrame { get; set; } = 1;

        /// <summary>
        ///     If there are unique key presses in the current frame, per lane.
        /// </summary>
        internal List<bool> UniquePresses { get; } = new List<bool>();

        /// <summary>
        ///     If there are unique key releases in the current frame, per lane.
        /// </summary>
        internal List<bool> UniqueReleases { get; } = new List<bool>();

        /// <summary>
        ///     Virtually plays replay frames
        /// </summary>
        public VirtualReplayPlayer VirtualPlayer { get; }

        /// <summary>
        ///     The current frame being played in the virtual replay player
        /// </summary>
        private int CurrentVirtualReplayStat { get; set; } = -1;

        /// <summary>
        /// </summary>
        private JudgementWindows Windows { get; set; }

        /// <summary>
        ///     The amount of keys in the map for the replay
        /// </summary>
        private int KeyCount { get; }

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="screen"></param>
        internal ReplayInputManagerKeys(GameplayScreen screen)
        {
            Screen = screen;
            Replay = Screen.LoadedReplay;

            Windows = Screen.SpectatorClient != null ? JudgementWindowsDatabaseCache.Standard : JudgementWindowsDatabaseCache.Selected.Value;
            VirtualPlayer = new VirtualReplayPlayer(Replay, Screen.Map, Windows, Screen.SpectatorClient != null);

            try
            {
                VirtualPlayer.PlayAllFrames();
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }

            // Populate unique key presses/releases.
            KeyCount = screen.Map.GetKeyCount();

            // Allows two bindings to the scratch key (K9)
            if (screen.Map.HasScratchKey)
                KeyCount++;

            for (var i = 0; i < KeyCount; i++)
            {
                UniquePresses.Add(false);
                UniqueReleases.Add(false);
            }
        }

        /// <summary>
        ///     Determines which frame we are on in the replay and sets if it has unique key presses/releases.
        /// </summary>
        internal void HandleInput(bool forceInput = false)
        {
            if (Screen.SpectatorClient != null && !Screen.IsSongSelectPreview)
                VirtualPlayer.PlayAllFrames();

            HandleScoring();

            if (!forceInput && CurrentFrame >= Replay.Frames.Count || !(Manager.CurrentAudioOffset >= Replay.Frames[CurrentFrame].Time) || !Screen.InReplayMode)
                return;

            if (Math.Abs(Manager.CurrentAudioOffset - Replay.Frames[CurrentFrame].Time) >= 200)
            {
                CurrentFrame = Replay.Frames.FindLastIndex(x => x.Time < AudioEngine.Track.Time);
                Logger.Important($"Skipped to replay frame: {CurrentFrame}", LogType.Runtime, false);
            }

            try
            {
                List<int> previousActive;

                previousActive = CurrentFrame - 1 >= 0
                    ? Replay.KeyPressStateToLanes(Replay.Frames[CurrentFrame - 1].Keys)
                    : new List<int>();

                var currentActive = Replay.KeyPressStateToLanes(Replay.Frames[CurrentFrame].Keys);

                foreach (var lane in currentActive)
                    UniquePresses[lane] = !previousActive.Contains(lane);

                foreach (var lane in previousActive)
                    UniqueReleases[lane] = !currentActive.Contains(lane);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
            finally
            {
                CurrentFrame++;
            }
        }

        /// <summary>
        ///     Handles spectating a user if applicable
        /// </summary>
        public void HandleSpectating()
        {
            if (Screen.SpectatorClient == null || Screen.IsSongSelectPreview)
                return;

            var isTournament = Screen is TournamentGameplayScreen;

            VirtualPlayer.PlayAllFrames();

            if (CurrentFrame >= Replay.Frames.Count && !isTournament && VirtualPlayer.CurrentFrame >= VirtualPlayer.Replay.Frames.Count)
            {
                if (AudioEngine.Track.IsPlaying)
                    AudioEngine.Track.Pause();

                if (!Screen.IsPaused)
                    Screen.IsPaused = true;

                return;
            }

            if (Screen.IsPaused && !isTournament)
                Screen.IsPaused = false;

            if (AudioEngine.Track.IsPaused && !isTournament)
                AudioEngine.Track.Play();
        }

        private void HandleScoring()
        {
            if (VirtualPlayer.CurrentFrame < VirtualPlayer.Replay.Frames.Count)
                VirtualPlayer.PlayAllFrames();

            for (var i = CurrentVirtualReplayStat + 1; i < VirtualPlayer.ScoreProcessor.Stats.Count; i++)
            {
                var hom = Screen.Ruleset.HitObjectManager as HitObjectManagerKeys;

                if (hom?.CurrentAudioOffset >= VirtualPlayer.ScoreProcessor.Stats[i].SongPosition)
                {
                    var judgement = VirtualPlayer.ScoreProcessor.Stats[i].Judgement;

                    ((ScoreProcessorKeys)Screen.Ruleset.ScoreProcessor).CalculateScore(judgement);

                    // Update Scoreboard
                    var view = (GameplayScreenView) Screen.View;
                    view.UpdateScoreAndAccuracyDisplays();

                    var playfield = (GameplayPlayfieldKeys)Screen.Ruleset.Playfield;
                    playfield.Stage.ComboDisplay.MakeVisible();

                    if (judgement != Judgement.Miss)
                        playfield.Stage.HitError.AddJudgement(judgement, VirtualPlayer.ScoreProcessor.Stats[i].HitDifference);

                    var lane = Math.Clamp(VirtualPlayer.ScoreProcessor.Stats[i].HitObject.Lane - 1, 0, playfield.Stage.JudgementHitBursts.Count - 1);
                    playfield.Stage.HitBubbles.AddJudgement(judgement);
                    playfield.Stage.JudgementHitBursts[lane].PerformJudgementAnimation(judgement);

                    CurrentVirtualReplayStat++;
                }
                else
                    break;
            }
        }

        internal void HandleSkip()
        {
            var time = AudioEngine.Track.Time;

            var frame = Replay.Frames.FindLastIndex(x => x.Time < time);

            if (frame == -1)
                return;

            CurrentFrame = frame - 1;

            if (CurrentFrame < 0)
                CurrentFrame = 0;

            // Reset the replay input state to one frame prior
            UniquePresses.Clear();
            UniqueReleases.Clear();

            for (var i = 0; i < KeyCount; i++)
            {
                UniquePresses.Add(false);
                UniqueReleases.Add(false);
            }

            var im = Screen.Ruleset.InputManager as KeysInputManager;
            im?.BindingStore.ForEach(x => x.Pressed = false);
            Screen.Ruleset.InputManager.HandleInput(0);

            CurrentVirtualReplayStat = -1;

            // Create a fresh scrore processor, so the score can be recalculated
            Screen.Ruleset.ScoreProcessor = new ScoreProcessorKeys(Screen.Map, Screen.Ruleset.ScoreProcessor.Mods, Windows);

            // Update the processor for the health bar, so it doesn't get stuck
            if (Screen.Ruleset is GameplayRulesetKeys ruleset)
            {
                var playfield = (GameplayPlayfieldKeys) ruleset.Playfield;
                playfield.Stage.HealthBar.UpdateProcessor(Screen.Ruleset.ScoreProcessor);
            }
        }
    }
}
