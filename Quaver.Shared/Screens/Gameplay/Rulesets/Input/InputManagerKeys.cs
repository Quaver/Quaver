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
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Wobble.Input;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Input
{
    internal class KeysInputManager : IGameplayInputManager
    {
        /// <summary>
        ///     The list of button containers for these keys.
        /// </summary>
        internal List<InputBindingKeys> BindingStore { get; }

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
            switch (mode)
            {
                case GameMode.Keys4:
                    // Initialize 4K Input button container.
                    BindingStore = new List<InputBindingKeys>
                    {
                        new InputBindingKeys(ConfigManager.KeyMania4K1),
                        new InputBindingKeys(ConfigManager.KeyMania4K2),
                        new InputBindingKeys(ConfigManager.KeyMania4K3),
                        new InputBindingKeys(ConfigManager.KeyMania4K4)
                    };
                    break;
                case GameMode.Keys7:
                    // Initialize 7K input button container.
                    BindingStore = new List<InputBindingKeys>
                    {
                        new InputBindingKeys(ConfigManager.KeyMania7K1),
                        new InputBindingKeys(ConfigManager.KeyMania7K2),
                        new InputBindingKeys(ConfigManager.KeyMania7K3),
                        new InputBindingKeys(ConfigManager.KeyMania7K4),
                        new InputBindingKeys(ConfigManager.KeyMania7K5),
                        new InputBindingKeys(ConfigManager.KeyMania7K6),
                        new InputBindingKeys(ConfigManager.KeyMania7K7)
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            Ruleset = ruleset;

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

                // A key was uniquely pressed.
                if (!BindingStore[lane].Pressed && (KeyboardManager.IsUniqueKeyPress(BindingStore[lane].Key.Value) &&
                                                    !Ruleset.Screen.InReplayMode || Ruleset.Screen.InReplayMode && ReplayInputManager.UniquePresses[lane]))
                {
                    // Update Replay Manager. Reset UniquePresses value for this lane.
                    if (Ruleset.Screen.InReplayMode)
                        ReplayInputManager.UniquePresses[lane] = false;

                    // Toggle this key on from BindingStore and enable needsUpdating value to handle key press
                    BindingStore[lane].Pressed = true;
                    needsUpdating = true;

                    var screenView = (GameplayScreenView) Ruleset.Screen.View;
                    screenView.KpsDisplay.AddClick();
                }
                // A key was uniquely released.
                else if (BindingStore[lane].Pressed && (KeyboardManager.IsUniqueKeyRelease(BindingStore[lane].Key.Value) &&
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

                // Don't bother updating the game any further if this event isn't important.
                if (!needsUpdating)
                    continue;

                // Update Playfield
                ((GameplayPlayfieldKeys)Ruleset.Playfield).Stage.SetReceptorAndLightingActivity(lane, BindingStore[lane].Pressed);

                // Handle Key Pressing/Releasing for this specific frame
                var manager = (HitObjectManagerKeys)Ruleset.HitObjectManager;
                if (BindingStore[lane].Pressed)
                {
                    var hitObject = manager.GetClosestTap(lane);
                    if (hitObject != null)
                        HandleKeyPress(manager, hitObject);
                }
                else
                {
                    var hitObject = manager.GetClosestRelease(lane);
                    if (hitObject != null)
                        HandleKeyRelease(manager, hitObject);
                }
            }
        }

        /// <summary>
        ///     Handles an individual key press during gameplay.
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="gameplayHitObject"></param>
        /// <param name="objectIndex"></param>
        private void HandleKeyPress(HitObjectManagerKeys manager, GameplayHitObjectKeys gameplayHitObject)
        {
            // Play the HitSounds of closest hit object.
            if (ConfigManager.EnableHitsounds.Value)
                HitObjectManager.PlayObjectHitSounds(gameplayHitObject.Info);
            if (ConfigManager.EnableKeysounds.Value)
                HitObjectManager.PlayObjectKeySounds(gameplayHitObject.Info);

            // Get Judgement and references
            var time = (int)manager.CurrentAudioPosition;
            var hitDifference = gameplayHitObject.Info.StartTime - time;
            var judgement = ((ScoreProcessorKeys)Ruleset.ScoreProcessor).CalculateScore(hitDifference, KeyPressType.Press, ReplayInputManager == null);
            var lane = gameplayHitObject.Info.Lane - 1;

            // Ignore Ghost Taps
            if (judgement == Judgement.Ghost)
                return;

            // Remove HitObject from Object Pool. Will be recycled/killed as necessary.
            gameplayHitObject = manager.ActiveNoteLanes[lane].Dequeue();

            // Update stats
            Ruleset.ScoreProcessor.Stats.Add(
                new HitStat(
                    HitStatType.Hit,
                    KeyPressType.Press,
                    gameplayHitObject.Info, time,
                    judgement,
                    hitDifference,
                    Ruleset.ScoreProcessor.Accuracy,
                    Ruleset.ScoreProcessor.Health
            ));

            // Update Scoreboard
            var view = (GameplayScreenView) Ruleset.Screen.View;
            view.UpdateScoreboardUsers();
            view.UpdateScoreAndAccuracyDisplays();

            // Update Playfield
            var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;

            if (ReplayInputManager == null)
            {
                playfield.Stage.ComboDisplay.MakeVisible();
                playfield.Stage.HitError.AddJudgement(judgement, gameplayHitObject.Info.StartTime - manager.CurrentAudioPosition);
                playfield.Stage.JudgementHitBurst.PerformJudgementAnimation(judgement);
            }

            // Update Object Pooling
            switch (judgement)
            {
                // Handle early miss cases here.
                case Judgement.Miss when gameplayHitObject.Info.IsLongNote:
                    // Add another miss when hit missing LNS
                    ((ScoreProcessorKeys)Ruleset.ScoreProcessor).CalculateScore(Judgement.Miss);
                    Ruleset.ScoreProcessor.Stats.Add(
                        new HitStat(
                            HitStatType.Miss,
                            KeyPressType.Press,
                            gameplayHitObject.Info, time,
                            Judgement.Miss,
                            time,
                            Ruleset.ScoreProcessor.Accuracy,
                            Ruleset.ScoreProcessor.Health
                        ));


                    view.UpdateScoreboardUsers();
                    view.UpdateScoreAndAccuracyDisplays();
                    playfield.Stage.JudgementHitBurst.PerformJudgementAnimation(Judgement.Miss);

                    manager.KillPoolObject(gameplayHitObject);
                    break;
                // Handle miss cases.
                case Judgement.Miss:
                    manager.RecyclePoolObject(gameplayHitObject);
                    break;
                // Handle non-miss cases. Perform Hit Lighting Animation and Handle Object pooling.
                default:
                    playfield.Stage.HitLightingObjects[lane].PerformHitAnimation(gameplayHitObject.Info.IsLongNote);
                    if (gameplayHitObject.Info.IsLongNote)
                    {
                        manager.ChangePoolObjectStatusToHeld(gameplayHitObject);
                        gameplayHitObject.StartLongNoteAnimation();
                    }
                    else
                        manager.RecyclePoolObject(gameplayHitObject);
                    break;
            }
        }

        /// <summary>
        ///     Handles an individual key release during gameplay.
        /// </summary>
        private void HandleKeyRelease(HitObjectManagerKeys manager, GameplayHitObjectKeys gameplayHitObject)
        {
            // Get judgement and references
            var lane = gameplayHitObject.Info.Lane - 1;
            var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;
            var hitDifference = (int)(manager.HeldLongNoteLanes[lane].Peek().Info.EndTime - manager.CurrentAudioPosition);
            var judgement = ((ScoreProcessorKeys)Ruleset.ScoreProcessor).CalculateScore(hitDifference, KeyPressType.Release, ReplayInputManager == null);

            // Update animations
            playfield.Stage.HitLightingObjects[lane].StopHolding();
            gameplayHitObject.StopLongNoteAnimation();

            // Dequeue from pool
            gameplayHitObject = manager.HeldLongNoteLanes[lane].Dequeue();

            var view = (GameplayScreenView) Ruleset.Screen.View;

            // If LN has been released during a window
            if (judgement != Judgement.Ghost)
            {
                // Update stats
                Ruleset.ScoreProcessor.Stats.Add(
                    new HitStat(
                        HitStatType.Hit,
                        KeyPressType.Release,
                        gameplayHitObject.Info,
                        (int)manager.CurrentAudioPosition,
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
                    playfield.Stage.HitError.AddJudgement(judgement, (int)(gameplayHitObject.Info.EndTime - manager.CurrentAudioPosition));
                    playfield.Stage.JudgementHitBurst.PerformJudgementAnimation(judgement);
                }

                // If the player recieved an early miss or "okay",
                // show the player that they were inaccurate by killing the object instead of recycling it
                if (judgement == Judgement.Miss || judgement == Judgement.Okay)
                    manager.KillHoldPoolObject(gameplayHitObject);
                else
                    manager.RecyclePoolObject(gameplayHitObject);

                return;
            }

            // If LN has been released early
            // Judgement for when the player releases too early
            const Judgement missedJudgement = Judgement.Miss;

            // Add new hit stat data and update score
            Ruleset.ScoreProcessor.Stats.Add(
                new HitStat(
                    HitStatType.Hit,
                    KeyPressType.Release,
                    gameplayHitObject.Info,
                    (int)manager.CurrentAudioPosition,
                    Judgement.Miss,
                    hitDifference,
                    Ruleset.ScoreProcessor.Accuracy,
                    Ruleset.ScoreProcessor.Health
            ));

            if (ReplayInputManager == null)
                Ruleset.ScoreProcessor.CalculateScore(missedJudgement);

            // Update scoreboard
            view.UpdateScoreboardUsers();
            view.UpdateScoreAndAccuracyDisplays();

            // Perform hit burst animation
            if (ReplayInputManager == null)
                playfield.Stage.JudgementHitBurst.PerformJudgementAnimation(Judgement.Miss);

            // Update Object Pool
            manager.KillHoldPoolObject(gameplayHitObject);
        }

        /// <summary>
        ///     Handles scroll speed changes.
        /// </summary>
        private void ChangeScrollSpeed()
        {
            // Only allow scroll speed changes if the map hasn't started or if we're on a break
            if (Ruleset.Screen.Timing.Time >= 5000 && !Ruleset.Screen.EligibleToSkip)
                return;

            // Decrease
            if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyDecreaseScrollSpeed.Value))
            {
                switch (Ruleset.Screen.Map.Mode)
                {
                    case GameMode.Keys4:
                        ConfigManager.ScrollSpeed4K.Value--;
                        NotificationManager.Show(NotificationLevel.Success, $"4K Scroll speed set to: {ConfigManager.ScrollSpeed4K.Value}");
                        break;
                    case GameMode.Keys7:
                        ConfigManager.ScrollSpeed7K.Value--;
                        NotificationManager.Show(NotificationLevel.Success, $"7K Scroll speed set to: {ConfigManager.ScrollSpeed7K.Value}");
                        break;
                }

                ((HitObjectManagerKeys)Ruleset.HitObjectManager).ForceUpdateLNSize();
            }
            // Increase
            else if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyIncreaseScrollSpeed.Value))
            {
                switch (Ruleset.Screen.Map.Mode)
                {
                    case GameMode.Keys4:
                        ConfigManager.ScrollSpeed4K.Value++;
                        NotificationManager.Show(NotificationLevel.Success, $"4K Scroll speed set to: {ConfigManager.ScrollSpeed4K.Value}");
                        break;
                    case GameMode.Keys7:
                        ConfigManager.ScrollSpeed7K.Value++;
                        NotificationManager.Show(NotificationLevel.Success, $"7K Scroll speed set to: {ConfigManager.ScrollSpeed7K.Value}");
                        break;
                }
                ((HitObjectManagerKeys)Ruleset.HitObjectManager).ForceUpdateLNSize();
            }
        }
    }
}
