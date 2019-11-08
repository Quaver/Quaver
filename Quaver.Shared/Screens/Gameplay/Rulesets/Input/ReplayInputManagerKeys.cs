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
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;

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
        ///     Ctor -
        /// </summary>
        /// <param name="screen"></param>
        internal ReplayInputManagerKeys(GameplayScreen screen)
        {
            Screen = screen;
            Replay = Screen.LoadedReplay;

            var windows = Screen.SpectatorClient != null
                ? JudgementWindowsDatabaseCache.Standard
                : JudgementWindowsDatabaseCache.Selected.Value;

            VirtualPlayer = new VirtualReplayPlayer(Replay, Screen.Map, windows, Screen.SpectatorClient != null);

            VirtualPlayer.PlayAllFrames();

            // Populate unique key presses/releases.
            for (var i = 0; i < screen.Map.GetKeyCount(); i++)
            {
                UniquePresses.Add(false);
                UniqueReleases.Add(false);
            }
        }

        /// <summary>
        ///     Determines which frame we are on in the replay and sets if it has unique key presses/releases.
        /// </summary>
        internal void HandleInput()
        {
            if (Screen.SpectatorClient != null)
                VirtualPlayer.PlayAllFrames();

            HandleScoring();

            if (CurrentFrame >= Replay.Frames.Count || !(Manager.CurrentAudioPosition >= Replay.Frames[CurrentFrame].Time) || !Screen.InReplayMode)
                return;

            var previousActive = Replay.KeyPressStateToLanes(Replay.Frames[CurrentFrame - 1].Keys);
            var currentActive = Replay.KeyPressStateToLanes(Replay.Frames[CurrentFrame].Keys);

            foreach (var activeLane in currentActive)
            {
                try
                {
                    if (!previousActive.Contains(activeLane))
                        UniquePresses[activeLane] = true;
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            foreach (var activeLane in previousActive)
            {
                try
                {
                    if (!currentActive.Contains(activeLane))
                        UniqueReleases[activeLane] = true;
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            CurrentFrame++;
        }

        /// <summary>
        ///     Handles spectating a user if applicable
        /// </summary>
        public void HandleSpectating()
        {
            if (Screen.SpectatorClient == null)
                return;

            if (CurrentFrame >= Replay.Frames.Count)
            {
                if (AudioEngine.Track.IsPlaying)
                    AudioEngine.Track.Pause();

                if (!Screen.IsPaused)
                    Screen.IsPaused = true;
                return;
            }

            VirtualPlayer.PlayAllFrames();

            if (Screen.IsPaused)
                Screen.IsPaused = false;

            if (AudioEngine.Track.IsPaused)
                AudioEngine.Track.Play();
        }

        private void HandleScoring()
        {
            if (VirtualPlayer.CurrentFrame < VirtualPlayer.Replay.Frames.Count)
                VirtualPlayer.PlayAllFrames();

            for (var i = CurrentVirtualReplayStat + 1; i < VirtualPlayer.ScoreProcessor.Stats.Count; i++)
            {
                var hom = Screen.Ruleset.HitObjectManager as HitObjectManagerKeys;

                if (hom?.CurrentAudioPosition >= VirtualPlayer.ScoreProcessor.Stats[i].SongPosition)
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

                    playfield.Stage.JudgementHitBurst.PerformJudgementAnimation(judgement);

                    CurrentVirtualReplayStat++;
                }
                else
                    break;
            }
        }

        internal void HandleSkip()
        {
            var frame = Replay.Frames.FindLastIndex(x => x.Time <= Manager.CurrentAudioPosition);

            if (frame == -1)
                return;

            CurrentFrame = ModManager.IsActivated(ModIdentifier.Autoplay) ? frame + 1 : frame;
        }
    }
}
