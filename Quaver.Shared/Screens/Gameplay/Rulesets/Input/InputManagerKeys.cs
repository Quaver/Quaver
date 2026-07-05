/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Quaver.Shared.Screens.Tournament.Gameplay;
using Wobble;
using Wobble.Bindables;
using Wobble.Input;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Input
{
    internal class KeysInputManager : IGameplayInputManager
    {
        /// <summary>
        ///     The list of button containers for these keys.
        /// </summary>
        internal List<InputBindingKeys> BindingStore { get; private set; }

        /// <summary>
        ///     Reference to the ruleset
        /// </summary>
        private GameplayRulesetKeys Ruleset { get; }

        /// <summary>
        ///     The replay input manager.
        /// </summary>
        internal ReplayInputManagerKeys ReplayInputManager { get; set; }

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="ruleset"></param>
        /// <param name="mode"></param>
        internal KeysInputManager(GameplayRulesetKeys ruleset, GameMode mode)
        {
            Ruleset = ruleset;

            SetInputKeybinds(mode);

            // Init replay
            if (Ruleset.Screen != null && Ruleset.Screen.InReplayMode)
                ReplayInputManager = new ReplayInputManagerKeys(Ruleset.Screen);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>Sta
        public void HandleInput(double dt)
        {
            // Handle scroll speed changes if necessary.
            ChangeScrollSpeed();

            // Handle Replay Input Manager if necessary.
            // - Grab the previous replay frame that we're on and update the replay's input manager to see if we have any updated frames.
            // - If the current and previous frames are the same, we don't have to do anything.
            if (Ruleset.Screen.InReplayMode)
            {
                var previousReplayFrame = ReplayInputManager.CurrentFrame;
                ReplayInputManager?.HandleInput();

                if (previousReplayFrame == ReplayInputManager.CurrentFrame)
                    return;
            }

            // Handle Key States
            for (var lane = 0; lane < BindingStore.Count; lane++)
            {
                // Is determined by whether a key is uniquely released or pressed.
                // Will not bother handling key presses/releases if this value is false.
                var needsUpdating = false;
                var inputLane = lane;

                // Allow multiple keybinds for scratch lane
                if (Ruleset.Map.HasScratchKey && lane == BindingStore.Count - 1)
                    inputLane--;

                // A key was uniquely pressed.
                if (!BindingStore[lane].Pressed && (GenericKeyManager.IsUniquePress(BindingStore[lane].Key.Value) &&
                                                    !Ruleset.Screen.InReplayMode || Ruleset.Screen.InReplayMode && ReplayInputManager.UniquePresses[lane]))
                {
                    // Update Replay Manager. Reset UniquePresses value for this lane.
                    if (Ruleset.Screen.InReplayMode)
                        ReplayInputManager.UniquePresses[inputLane] = false;

                    // Toggle this key on from BindingStore and enable needsUpdating value to handle key press
                    BindingStore[lane].Pressed = true;
                    needsUpdating = true;

                    var screenView = (GameplayScreenView)Ruleset.Screen.View;
                    screenView.KpsDisplay.AddClick();
                }
                // A key was uniquely released.
                else if (BindingStore[lane].Pressed && (GenericKeyManager.IsUniqueRelease(BindingStore[lane].Key.Value) &&
                                                        !Ruleset.Screen.InReplayMode
                                                    || Ruleset.Screen.InReplayMode && ReplayInputManager.UniqueReleases[lane]))
                {
                    // Update Replay Manager. Reset UniquePresses value for this lane.
                    if (Ruleset.Screen.InReplayMode)
                        ReplayInputManager.UniqueReleases[lane] = false;

                    // Toggle this key on from BindingStore and enable needsUpdating value to handle key release
                    BindingStore[lane].Pressed = false;
                    needsUpdating = true;
                }

                // Handle Key Pressing/Releasing for this specific frame
                var manager = (HitObjectManagerKeys)Ruleset.HitObjectManager;

                if (BindingStore[lane].Pressed && !Ruleset.Screen.InReplayMode 
                    || Ruleset.Screen.InReplayMode && ReplayInputManager.Presses[lane])
                {
                    HandleMinePresses(manager, inputLane);
                }

                // Don't bother updating the game any further if this event isn't important.
                if (!needsUpdating)
                    continue;

                // Update Playfield
                ((GameplayPlayfieldKeys)Ruleset.Playfield).Stage.SetReceptorAndLightingActivity(inputLane, BindingStore[lane].Pressed || BindingStore[inputLane].Pressed);

                if (BindingStore[lane].Pressed)
                {
                    var hitObject = manager.GetClosestTap(inputLane);

                    if (hitObject != null)
                        HandleKeyPress(manager, hitObject);
                }
                else
                {
                    var hitObject = manager.GetClosestRelease(inputLane);

                    if (hitObject != null)
                        HandleKeyRelease(manager, hitObject);
                }
            }
        }

        /// <summary>
        ///     Handles the following mines during press of a lane
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="inputLane"></param>
        private void HandleMinePresses(HitObjectManagerKeys manager, int inputLane)
        {
            foreach (var info in manager.MineLanes[inputLane])
            {
                if (info.State is HitObjectState.Dead or HitObjectState.Removed)
                    continue;

                // Get Judgement and references
                var time = (int)manager.CurrentAudioOffset;
                var endTime = info.IsLongNote ? info.EndTime : info.StartTime;

                if (time < info.StartTime - Ruleset.ScoreProcessor.JudgementWindow[Judgement.Marv])
                    break;
                if (time > endTime + Ruleset.ScoreProcessor.JudgementWindow[Judgement.Marv])
                    continue;

                var hitDifference = info.StartTime - time;
                var stat = new HitStat(HitStatType.Miss, KeyPressType.Press, info.HitObjectInfo, time, Judgement.Miss,
                    hitDifference, Ruleset.ScoreProcessor.Accuracy, Ruleset.ScoreProcessor.Health);

                ((ScoreProcessorKeys)Ruleset.ScoreProcessor).CalculateScore(stat);
                var lane = info.Lane - 1;

                // Play the HitSounds of closest hit object.
                var game = GameBase.Game as QuaverGame;

                if (game?.CurrentScreen?.Type != QuaverScreenType.Editor)
                {
                    if (ConfigManager.EnableHitsounds.Value) HitObjectManager.PlayObjectHitSounds(info.HitObjectInfo);
                    if (ConfigManager.EnableKeysounds.Value) HitObjectManager.PlayObjectKeySounds(info.HitObjectInfo);
                }

                // Update stats
                Ruleset.ScoreProcessor.Stats.Add(stat);

                // Update Scoreboard
                var view = (GameplayScreenView)Ruleset.Screen.View;
                view.UpdateScoreboardUsers();
                view.UpdateScoreAndAccuracyDisplays();

                // Update Playfield
                var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;

                // Get hit burst lane
                var judgementHitBurstLane = Math.Clamp(lane, 0, playfield.Stage.JudgementHitBursts.Count - 1);

                if (ReplayInputManager == null)
                {
                    playfield.Stage.ComboDisplay.MakeVisible();
                    playfield.Stage.HitError.AddJudgement(Judgement.Miss, info.StartTime - time);
                    playfield.Stage.HitBubbles.AddJudgement(Judgement.Miss);
                    playfield.Stage.JudgementHitBursts[judgementHitBurstLane].PerformJudgementAnimation(Judgement.Miss);
                }

                // Update Object Pooling
                info.State = HitObjectState.Removed;
            }
        }

        /// <summary>
        ///     Handles an individual key press during gameplay.
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="gameplayHitObject"></param>
        /// <param name="objectIndex"></param>
        private void HandleKeyPress(HitObjectManagerKeys manager, NoteControllerKeys info)
        {
            // Play the HitSounds of closest hit object.
            var game = GameBase.Game as QuaverGame;

            if (game?.CurrentScreen?.Type != QuaverScreenType.Editor)
            {
                if (ConfigManager.EnableHitsounds.Value)
                    HitObjectManager.PlayObjectHitSounds(info.HitObjectInfo);
                if (ConfigManager.EnableKeysounds.Value)
                    HitObjectManager.PlayObjectKeySounds(info.HitObjectInfo);
            }

            // Get Judgement and references
            var time = (int)manager.CurrentAudioOffset;
            var hitDifference = info.StartTime - time;
            var judgement = ((ScoreProcessorKeys)Ruleset.ScoreProcessor).CalculateScore(hitDifference,
                KeyPressType.Press, ReplayInputManager == null);
            var lane = info.Lane - 1;

            // Ignore Ghost Taps
            if (judgement == Judgement.Ghost)
                return;

            // Remove HitObject from Object Pool. Will be recycled/killed as necessary.
            // gameplayHitObject = manager.ActiveNoteLanes[lane].Dequeue();
            manager.HitObjectQueueLanes[lane].Dequeue();

            // Update stats
            Ruleset.ScoreProcessor.Stats.Add(
                new HitStat(
                    HitStatType.Hit,
                    KeyPressType.Press,
                    info.HitObjectInfo, time,
                    judgement,
                    hitDifference,
                    Ruleset.ScoreProcessor.Accuracy,
                    Ruleset.ScoreProcessor.Health
            ));

            // Update Scoreboard
            var view = (GameplayScreenView)Ruleset.Screen.View;
            view.UpdateScoreboardUsers();
            view.UpdateScoreAndAccuracyDisplays();

            // Update Playfield
            var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;

            // Get hit burst lane
            var judgementHitBurstLane = Math.Clamp(lane, 0, playfield.Stage.JudgementHitBursts.Count - 1);

            if (ReplayInputManager == null)
            {
                playfield.Stage.ComboDisplay.MakeVisible();
                playfield.Stage.HitError.AddJudgement(judgement, info.StartTime - time);
                playfield.Stage.HitBubbles.AddJudgement(judgement);
                playfield.Stage.JudgementHitBursts[judgementHitBurstLane].PerformJudgementAnimation(judgement);
            }

            // Update Object Pooling
            switch (judgement)
            {
                // Handle early miss cases here.
                case Judgement.Miss when info.IsLongNote:
                    // Add another miss when hit missing LNS
                    if (ReplayInputManager == null)
                    {
                        ((ScoreProcessorKeys)Ruleset.ScoreProcessor).CalculateScore(Judgement.Miss, true, false);

                        Ruleset.ScoreProcessor.Stats.Add(
                            new HitStat(
                                HitStatType.Miss,
                                KeyPressType.Press,
                                info.HitObjectInfo, time,
                                Judgement.Miss,
                                int.MinValue,
                                Ruleset.ScoreProcessor.Accuracy,
                                Ruleset.ScoreProcessor.Health
                            ));

                        view.UpdateScoreboardUsers();
                        view.UpdateScoreAndAccuracyDisplays();
                        playfield.Stage.HitBubbles.AddJudgement(Judgement.Miss);
                        playfield.Stage.JudgementHitBursts[judgementHitBurstLane].PerformJudgementAnimation(Judgement.Miss);
                    }

                    info.State = HitObjectState.Dead;
                    break;
                // Handle miss cases.
                case Judgement.Miss:
                    info.State = HitObjectState.Removed;
                    break;
                // Handle non-miss cases. Perform Hit Lighting Animation and Handle Object pooling.
                default:
                    playfield.Stage.HitLightingObjects[lane].PerformHitAnimation(info.IsLongNote, judgement);
                    if (info.IsLongNote)
                    {
                        manager.ChangeHitObjectToHeld(info);
                        info.HitObject?.StartLongNoteAnimation();
                    }
                    else
                        info.State = HitObjectState.Removed;
                    break;
            }
        }

        /// <summary>
        ///     Handles an individual key release during gameplay.
        /// </summary>
        private void HandleKeyRelease(HitObjectManagerKeys manager, NoteControllerKeys info)
        {
            // Get judgement and references
            var lane = info.Lane - 1;
            var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;
            var time = (int)manager.CurrentAudioOffset;
            var hitDifference = info.EndTime - time;

            var judgement = ((ScoreProcessorKeys)Ruleset.ScoreProcessor).CalculateScore(hitDifference,
                KeyPressType.Release, ReplayInputManager == null);

            // Update animations
            playfield.Stage.HitLightingObjects[lane].StopHolding();
            info.HitObject?.StopLongNoteAnimation();

            // Dequeue from pool
            manager.HeldLongNoteLanes[lane].Dequeue();

            var view = (GameplayScreenView)Ruleset.Screen.View;

            // Get hit burst lane
            var judgementHitBurstLane = Math.Clamp(lane, 0, playfield.Stage.JudgementHitBursts.Count - 1);

            // If LN has been released during a window
            if (judgement != Judgement.Ghost)
            {
                var game = GameBase.Game as QuaverGame;
                if (game?.CurrentScreen?.Type != QuaverScreenType.Editor)
                {
                    if (ConfigManager.EnableHitsounds.Value && ConfigManager.EnableLongNoteReleaseHitsounds.Value)
                        HitObjectManager.PlayObjectHitSounds(info.HitObjectInfo);
                }

                // Update stats
                Ruleset.ScoreProcessor.Stats.Add(
                    new HitStat(
                        HitStatType.Hit,
                        KeyPressType.Release,
                        info.HitObjectInfo,
                        time,
                        judgement,
                        hitDifference,
                        Ruleset.ScoreProcessor.Accuracy,
                        Ruleset.ScoreProcessor.Health
                ));

                // Update scoreboard
                view.UpdateScoreboardUsers();
                view.UpdateScoreAndAccuracyDisplays();

                // Update Playfield
                if (ReplayInputManager == null)
                {
                    playfield.Stage.ComboDisplay.MakeVisible();
                    playfield.Stage.HitError.AddJudgement(judgement, info.EndTime - time);
                    playfield.Stage.HitBubbles.AddJudgement(judgement);
                    playfield.Stage.JudgementHitBursts[judgementHitBurstLane].PerformJudgementAnimation(judgement);
                }

                // play hitlighting animation on release
                if (info.IsLongNote)
                    playfield.Stage.HitLightingObjects[lane].PerformHitAnimation(false, judgement);

                // If the player recieved an early miss or "okay",
                // show the player that they were inaccurate by killing the object instead of recycling it
                if (judgement == Judgement.Miss || judgement == Judgement.Okay)
                    info.State = HitObjectState.Dead;
                else
                    info.State = HitObjectState.Removed;

                return;
            }

            // If LN has been released early

            // Add new hit stat data and update score
            Ruleset.ScoreProcessor.Stats.Add(
                new HitStat(
                    HitStatType.Hit,
                    KeyPressType.Release,
                    info.HitObjectInfo,
                    time,
                    Judgement.Miss,
                    hitDifference,
                    Ruleset.ScoreProcessor.Accuracy,
                    Ruleset.ScoreProcessor.Health
            ));

            if (ReplayInputManager == null)
                Ruleset.ScoreProcessor.CalculateScore(Judgement.Miss, true, false);

            // Update scoreboard
            view.UpdateScoreboardUsers();
            view.UpdateScoreAndAccuracyDisplays();

            // Perform hit burst animation
            if (ReplayInputManager == null)
            {
                playfield.Stage.HitBubbles.AddJudgement(Judgement.Miss);
                playfield.Stage.JudgementHitBursts[judgementHitBurstLane].PerformJudgementAnimation(Judgement.Miss);
            }

            // Update Object Pool
            info.State = HitObjectState.Dead;
        }

        /// <summary>
        ///     Handles scroll speed changes.
        /// </summary>
        private void ChangeScrollSpeed()
        {
            // Only allow scroll speed changes if the map hasn't started or if we're on a break
            if (Ruleset.Screen.IsSongSelectPreview || Ruleset.Screen.Timing.Time >= 5000 && !Ruleset.Screen.EligibleToSkip && !(Ruleset.Screen is TournamentGameplayScreen) && !Ruleset.Screen.InReplayMode)
                return;

            if (!KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyIncreaseScrollSpeed.Value) &&
                !KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyDecreaseScrollSpeed.Value))
                return;

            var speedIncrease = KeyboardManager.IsCtrlDown() ? 1 : 10;

            var scrollSpeed = ConfigManager.ScrollSpeeds[Ruleset.Screen.Map.Mode];

            if (KeyboardManager.IsShiftDown())
            {
                // Handle local scroll speed changes with <shift> key held.
                var targetScrollSpeed = MapManager.CustomScrollSpeed ?? scrollSpeed.Value;
                if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyIncreaseScrollSpeed.Value))
                {
                    targetScrollSpeed += speedIncrease;
                }
                else if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyDecreaseScrollSpeed.Value))
                {
                    targetScrollSpeed -= speedIncrease;
                }

                if (targetScrollSpeed == scrollSpeed.Value)
                {
                    // Reset to global if the target speed is the same as global
                    MapManager.CustomScrollSpeed = null;

                    NotificationManager.ShowOrUpdate("gameplay-scroll-speed", NotificationLevel.Info,
                        $"Scroll speed (local) has been reset to global: {scrollSpeed.Value / 10f:0.0}",
                        null, true);
                }
                else
                {
                    // Set custom local scroll speed
                    MapManager.CustomScrollSpeed = targetScrollSpeed;

                    NotificationManager.ShowOrUpdate("gameplay-scroll-speed", NotificationLevel.Info,
                        $"Scroll speed (local) has been changed to: {targetScrollSpeed / 10f:0.0}",
                        null, true);
                }
            }
            else
            {
                // Update the global scroll speed.
                // If there is a custom local scroll speed set, this would not have any
                // visual effect right away.

                if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyIncreaseScrollSpeed.Value))
                    scrollSpeed.Value += speedIncrease;
                else if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyDecreaseScrollSpeed.Value))
                    scrollSpeed.Value -= speedIncrease;

                NotificationManager.ShowOrUpdate("gameplay-scroll-speed", NotificationLevel.Info,
                    $"Scroll speed (global) has been changed to: {scrollSpeed.Value / 10f:0.0}",
                    null, true);
            }
        }

        /// <summary>
        ///     Sets input keybinds based on which player is playing
        /// </summary>
        /// <param name="mode"></param>
        private void SetInputKeybinds(GameMode mode)
        {
            if (Ruleset.Screen.TournamentOptions == null || Ruleset.Screen.TournamentOptions?.Index == 0)
                SetPlayer1Keybinds(mode);
            else if (Ruleset.Screen.TournamentOptions != null)
                SetPlayer2Keybinds(mode);
        }

        /// <summary>
        ///     Sets the keybinds for player 1
        /// </summary>
        /// <param name="mode"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void SetPlayer1Keybinds(GameMode mode)
        {
            BindingStore = ConfigManager.KeyLayouts[mode].Select(x => new InputBindingKeys(x)).ToList();
            if (Ruleset.Map.HasScratchKey)
            {
                BindingStore.AddRange(ConfigManager.ScratchKeyLayouts[mode].Select(x => new InputBindingKeys(x)));
            }
        }

        /// <summary>
        ///     Sets the keybinds for player 1
        /// </summary>
        /// <param name="mode"></param>
        private void SetPlayer2Keybinds(GameMode mode)
        {
            BindingStore = ConfigManager.CoopKeyLayouts[mode].Select(x => new InputBindingKeys(x)).ToList();
        }
    }
}
