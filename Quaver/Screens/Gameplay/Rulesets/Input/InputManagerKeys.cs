using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.Config;
using Quaver.Graphics.Notifications;
using Quaver.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Screens.Gameplay.Rulesets.Keys;
using Quaver.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Quaver.Screens.Gameplay.Rulesets.Keys.Playfield;
using Wobble.Input;

namespace Quaver.Screens.Gameplay.Rulesets.Input
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
        internal ReplayInputManagerKeys ReplayInputManager { get; }

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
            if (ReplayInputManager != null)
            {
                // Grab the previous replay frame that we're on.
                var previousReplayFrame = ReplayInputManager.CurrentFrame;

                // Update the replay's input manager to see if we have any updated frames.
                ReplayInputManager?.HandleInput();

                // Grab the current replay frame.
                // - If the two frames are the same, we don't have to update Key Press state.
                var currentReplayFrame = ReplayInputManager.CurrentFrame;
                if (previousReplayFrame == currentReplayFrame)
                    return;
            }

            // Handle Key States
            for (var lane = 0; lane < BindingStore.Count; lane++)
            {
                // Is determined by whether a key is uniquely released or pressed.
                //  - If this value is false, it will not bother handling key press/releases. 
                var needsUpdating = false;

                // A key was uniquely pressed.
                if (!BindingStore[lane].Pressed && (KeyboardManager.IsUniqueKeyPress(BindingStore[lane].Key.Value) && ReplayInputManager == null
                                                || ReplayInputManager != null && ReplayInputManager.UniquePresses[lane]))
                {
                    // Update Replay Manager. Reset UniquePresses value for this lane.
                    if (ReplayInputManager != null)
                        ReplayInputManager.UniquePresses[lane] = false;

                    // Toggle this key on from BindingStore and enable needsUpdating value to handle key press
                    BindingStore[lane].Pressed = true;
                    needsUpdating = true;

                    var screenView = (GameplayScreenView) Ruleset.Screen.View;
                    screenView.KpsDisplay.AddClick();
                }
                // A key was uniquely released.
                else if (BindingStore[lane].Pressed && (KeyboardManager.IsUniqueKeyRelease(BindingStore[lane].Key.Value) && ReplayInputManager == null
                                                    || ReplayInputManager != null && ReplayInputManager.UniqueReleases[lane]))
                {
                    // Update Replay Manager. Reset UniquePresses value for this lane.
                    if (ReplayInputManager != null)
                        ReplayInputManager.UniqueReleases[lane] = false;

                    // Toggle this key on from BindingStore and enable needsUpdating value to handle key release
                    BindingStore[lane].Pressed = false;
                    needsUpdating = true;
                }

                // Don't bother updating the game any further if this event isn't important.
                if (!needsUpdating)
                    continue;

                // Update Playfield
                var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;
                playfield.Stage.SetReceptorAndLightingActivity(lane, BindingStore[lane].Pressed);

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
        /// <param name="hitObject"></param>
        /// <param name="objectIndex"></param>
        private void HandleKeyPress(HitObjectManagerKeys manager, GameplayHitObjectKeys hitObject)
        {
            // Play the HitSounds of closest hit object.
            HitObjectManager.PlayObjectHitSounds(hitObject.Info);

            // Get Judgement and references
            var time = (int) Ruleset.Screen.Timing.Time;
            var hitDifference = hitObject.Info.StartTime - time;
            var processor = (ScoreProcessorKeys)Ruleset.ScoreProcessor;
            var judgement = processor.CalculateScore(hitDifference, KeyPressType.Press);
            var lane = hitObject.Info.Lane - 1;

            // Ignore Ghost Taps
            if (judgement == Judgement.Ghost)
                return;

            // Remove HitObject from Object Pool. Will be recycled/killed as necessary.
            hitObject = manager.ObjectPool[lane].Dequeue();

            // Update stats
            Ruleset.ScoreProcessor.Stats.Add(
                new HitStat(
                    HitStatType.Hit,
                    KeyPressType.Press,
                    hitObject.Info, time,
                    judgement,
                    hitDifference,
                    Ruleset.ScoreProcessor.Accuracy,
                    Ruleset.ScoreProcessor.Health
            ));

            // Update Scoreboard
            var screenView = (GameplayScreenView)Ruleset.Screen.View;
            screenView.UpdateScoreboardUsers();

            // Update Playfield
            var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;
            playfield.Stage.ComboDisplay.MakeVisible();
            playfield.Stage.HitError.AddJudgement(judgement, hitObject.Info.StartTime - Ruleset.Screen.Timing.Time);
            playfield.Stage.JudgementHitBurst.PerformJudgementAnimation(judgement);

            // Update Object Pooling
            switch (judgement)
            {
                // Handle early miss cases here.
                case Judgement.Miss when hitObject.IsLongNote:
                    manager.KillPoolObject(hitObject);
                    break;
                // Handle miss cases.
                case Judgement.Miss:
                    manager.RecyclePoolObject(hitObject);
                    break;
                // Handle non-miss cases. Perform Hit Lighting Animation and Handle Object pooling.
                default:
                    playfield.Stage.HitLightingObjects[lane].PerformHitAnimation(hitObject.IsLongNote);
                    if (hitObject.IsLongNote)
                        manager.ChangePoolObjectStatusToHeld(hitObject);
                    else
                        manager.RecyclePoolObject(hitObject);
                    break;
            }
        }

        /// <summary>
        ///     Handles an individual key release during gameplay.
        /// </summary>
        private void HandleKeyRelease(HitObjectManagerKeys manager, GameplayHitObjectKeys hitObject)
        {
            // Get judgement and references
            var lane = hitObject.Info.Lane - 1;
            var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;
            var hitDifference = manager.HeldLongNotes[lane].Peek().Info.EndTime - (int) Ruleset.Screen.Timing.Time;
            var processor = (ScoreProcessorKeys)Ruleset.ScoreProcessor;
            var judgement = processor.CalculateScore(hitDifference, KeyPressType.Release);
            GameplayScreenView screenView;

            // If LN has been released during a window
            if (judgement != Judgement.Ghost)
            {
                // Dequeue from pool
                hitObject = manager.HeldLongNotes[lane].Dequeue();

                // Update stats
                Ruleset.ScoreProcessor.Stats.Add(
                    new HitStat(
                        HitStatType.Hit,
                        KeyPressType.Release,
                        hitObject.Info,
                        (int)Ruleset.Screen.Timing.Time,
                        judgement,
                        hitDifference,
                        Ruleset.ScoreProcessor.Accuracy,
                        Ruleset.ScoreProcessor.Health
                ));

                // Update scoreboard
                screenView = (GameplayScreenView)Ruleset.Screen.View;
                screenView.UpdateScoreboardUsers();

                // Update Playfield
                playfield.Stage.ComboDisplay.MakeVisible();
                playfield.Stage.HitError.AddJudgement(judgement, hitObject.Info.EndTime - (int) Ruleset.Screen.Timing.Time);
                playfield.Stage.JudgementHitBurst.PerformJudgementAnimation(judgement);
                playfield.Stage.HitLightingObjects[lane].StopHolding();

                // If the player recieved an early miss or "okay",
                // show the player that they were inaccurate by killing the object instead of recycling it
                if (judgement == Judgement.Miss || judgement == Judgement.Okay)
                    manager.KillHoldPoolObject(hitObject);
                else
                    manager.RecyclePoolObject(hitObject);

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
                    hitObject.Info,
                    (int)Ruleset.Screen.Timing.Time,
                    Judgement.Miss,
                    hitDifference,
                    Ruleset.ScoreProcessor.Accuracy,
                    Ruleset.ScoreProcessor.Health
            ));
            Ruleset.ScoreProcessor.CalculateScore(missedJudgement);

            // Update scoreboard
            screenView = (GameplayScreenView)Ruleset.Screen.View;
            screenView.UpdateScoreboardUsers();

            // Perform hit burst animation
            playfield.Stage.JudgementHitBurst.PerformJudgementAnimation(Judgement.Miss);

            // Update Object Pool
            manager.KillHoldPoolObject(hitObject);
        }

        /// <summary>
        ///     Handles scroll speed changes.
        /// </summary>
        private void ChangeScrollSpeed()
        {
            // Only allow scroll speed changes if the map hasn't started or if we're on a break
            if (Ruleset.Screen.Timing.Time >= 5000 && !Ruleset.Screen.OnBreak)
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
            }
        }
    }
}
