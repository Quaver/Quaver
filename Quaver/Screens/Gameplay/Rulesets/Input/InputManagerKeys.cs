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
            // Handle replay input if necessary.
            if (ReplayInputManager != null)
            {
                // Grab the previous replay frame that we're on.
                var previousReplayFrame = ReplayInputManager.CurrentFrame;

                // Update the replay's input manager to see if we have any updated frames.
                ReplayInputManager?.HandleInput();

                // Grab the current replay frame.
                var currentReplayFrame = ReplayInputManager.CurrentFrame;

                // If the two frames are the same, we don't have to update the key press state.
                if (previousReplayFrame == currentReplayFrame)
                    return;
            }

            for (var i = 0; i < BindingStore.Count; i++)
            {
                // Keeps track of if this key input is is important enough for us to want to
                // update more things like animations, score, etc.
                var needsUpdating = false;

                // A key was uniquely pressed.
                if (!BindingStore[i].Pressed && (KeyboardManager.IsUniqueKeyPress(BindingStore[i].Key.Value) && ReplayInputManager == null
                                                || ReplayInputManager != null && ReplayInputManager.UniquePresses[i]))
                {
                    // We've already handling the unique key press, so reset it.
                    if (ReplayInputManager != null)
                        ReplayInputManager.UniquePresses[i] = false;

                    BindingStore[i].Pressed = true;
                    needsUpdating = true;

                    var screenView = (GameplayScreenView) Ruleset.Screen.View;
                    screenView.KpsDisplay.AddClick();
                }
                // A key was uniquely released.
                else if (BindingStore[i].Pressed && (KeyboardManager.IsUniqueKeyRelease(BindingStore[i].Key.Value) && ReplayInputManager == null
                                                    || ReplayInputManager != null && ReplayInputManager.UniqueReleases[i]))
                {
                    // We're already handling the unique key release so reset.
                    if (ReplayInputManager != null)
                        ReplayInputManager.UniqueReleases[i] = false;

                    BindingStore[i].Pressed = false;
                    needsUpdating = true;
                }

                // Don't bother updating the game any further if this event isn't important.
                if (!needsUpdating)
                    continue;

                // Update the receptor of the playfield
                var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;
                playfield.Stage.SetReceptorAndLightingActivity(i, BindingStore[i].Pressed);

                // Get the object manager itself.
                var manager = (HitObjectManagerKeys)Ruleset.HitObjectManager;

                // Find the object that is nearest in the lane that the user has pressed.
                var objectIndex = manager.GetIndexOfNearestLaneObject(i + 1, Ruleset.Screen.Timing.Time);

                // Don't proceed if an object wasn't found.
                if (objectIndex == -1)
                    continue;

                // If the key was pressed during this frame.
                if (BindingStore[i].Pressed)
                {
                    HandleKeyPress(manager, (GameplayHitObjectKeys)manager.ObjectPool[objectIndex], objectIndex);
                }
                // If the key was released during this frame.
                else
                {
                    // Find the index of the actual closest LN and handle the key release
                    // if so.
                    for (var j = 0; j < manager.HeldLongNotes.Count; j++)
                    {
                        // Handle the release.
                        if (manager.HeldLongNotes[j].Info.Lane == i + 1)
                            HandleKeyRelease(manager, j);
                    }
                }
            }

            // Handle scroll speed changes.
            ChangeScrollSpeed();
        }

        /// <summary>
        ///     Handles an individual key press during gameplay.
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="hitObject"></param>
        /// <param name="objectIndex"></param>
        private void HandleKeyPress(HitObjectManagerKeys manager, GameplayHitObjectKeys hitObject, int objectIndex)
        {
            // Play the HitSounds for this object.
            HitObjectManager.PlayObjectHitSounds(manager.ObjectPool[objectIndex].Info);

            //NEW
            var time = (int) Ruleset.Screen.Timing.Time;
            var hitDifference = (int) (hitObject.TrueStartTime - time);
            var processor = (ScoreProcessorKeys)Ruleset.ScoreProcessor;
            var judgement = processor.CalculateScore(hitDifference, KeyPressType.Press);

            // Ignore Ghost Taps
            if (judgement == Judgement.Ghost)
                return;

            var stat = new HitStat(HitStatType.Hit, KeyPressType.Press, hitObject.Info, time, judgement, hitDifference,
                                        Ruleset.ScoreProcessor.Accuracy, Ruleset.ScoreProcessor.Health);
            Ruleset.ScoreProcessor.Stats.Add(stat);

            var screenView = (GameplayScreenView)Ruleset.Screen.View;
            screenView.UpdateScoreboardUsers();

            switch (judgement)
            {
                // Handle early miss cases here.
                case Judgement.Miss when hitObject.IsLongNote:
                    manager.KillPoolObject(objectIndex);
                    break;
                // Handle non-miss cases.
                case Judgement.Miss:
                    manager.RecyclePoolObject(objectIndex);
                    break;
                default:
                    if (hitObject.IsLongNote)
                        manager.ChangePoolObjectStatusToHeld(objectIndex);
                    // If the object is not an LN, recycle it.
                    else
                        manager.RecyclePoolObject(objectIndex);
                    break;
            }

            // Make the combo display visible since it is now changing.
            var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;
            playfield.Stage.ComboDisplay.MakeVisible();

            // Also add a judgement to the hit error.
            playfield.Stage.HitError.AddJudgement(judgement, hitObject.TrueStartTime - Ruleset.Screen.Timing.Time);

            // Perform hit burst animation
            playfield.Stage.JudgementHitBurst.PerformJudgementAnimation(judgement);

            // Don't execute any further if the user early missed, as these
            // are things pertaining to animations when the user actually hits the note.
            if (judgement == Judgement.Miss)
                return;

            // Perform hit lighting animation
            var laneIndex = hitObject.Info.Lane - 1;

            // If the object is a long note, let the hitlighting actually know about it.
            if (hitObject.IsLongNote)
                playfield.Stage.HitLightingObjects[laneIndex].IsHoldingLongNote = true;

            playfield.Stage.HitLightingObjects[laneIndex].PerformHitAnimation();

            #region Old HandleKeyPress
            //OLD
            /*
            // Check which hit window this object's timing is in
            for (var j = 0; j < Ruleset.ScoreProcessor.JudgementWindow.Count; j++)
            {
                var time = Ruleset.Screen.Timing.Time;
                var hitDifference = hitObject.TrueStartTime - time;

                // Check if the user actually hit the object.
                if (!(Math.Abs(hitDifference) <= Ruleset.ScoreProcessor.JudgementWindow[(Judgement)j]))
                    continue;

                var judgement = (Judgement)j;

                // Update the user's score
                Ruleset.ScoreProcessor.CalculateScore(hitDifference);

                // Add new hit stat data.
                var stat = new HitStat(HitStatType.Hit, hitObject.Info, time, judgement, hitDifference,
                                        Ruleset.ScoreProcessor.Accuracy, Ruleset.ScoreProcessor.Health);
                Ruleset.ScoreProcessor.Stats.Add(stat);

                var screenView = (GameplayScreenView) Ruleset.Screen.View;
                screenView.UpdateScoreboardUsers();

                switch (judgement)
                {
                    // Handle early miss cases here.
                    case Judgement.Miss when hitObject.IsLongNote:
                        manager.KillPoolObject(objectIndex);
                        break;
                    // Handle non-miss cases.
                    case Judgement.Miss:
                        manager.RecyclePoolObject(objectIndex);
                        break;
                    default:
                        if (hitObject.IsLongNote)
                            manager.ChangePoolObjectStatusToHeld(objectIndex);
                        // If the object is not an LN, recycle it.
                        else
                            manager.RecyclePoolObject(objectIndex);
                        break;
                }

                // Make the combo display visible since it is now changing.
                var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;
                playfield.Stage.ComboDisplay.MakeVisible();

                // Also add a judgement to the hit error.
                playfield.Stage.HitError.AddJudgement(judgement, hitObject.TrueStartTime - Ruleset.Screen.Timing.Time);

                // Perform hit burst animation
                playfield.Stage.JudgementHitBurst.PerformJudgementAnimation(judgement);

                // Don't execute any further if the user early missed, as these
                // are things pertaining to animations when the user actually hits the note.
                if (judgement == Judgement.Miss)
                    return;

                // Perform hit lighting animation
                var laneIndex = hitObject.Info.Lane - 1;

                // If the object is a long note, let the hitlighting actually know about it.
                if (hitObject.IsLongNote)
                    playfield.Stage.HitLightingObjects[laneIndex].IsHoldingLongNote = true;

                playfield.Stage.HitLightingObjects[laneIndex].PerformHitAnimation();
                break;
            }
            */
            #endregion
        }

        /// <summary>
        ///     Handles an individual key release during gameplay.
        /// </summary>
        private void HandleKeyRelease(HitObjectManagerKeys manager, int noteIndex)
        {
            // Don't bother executing if there aren't any long notes.
            if (manager.HeldLongNotes.Count == 0)
                return;

            var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;
            playfield.Stage.ComboDisplay.MakeVisible();

            // Stop looping hit lighting.
            playfield.Stage.HitLightingObjects[manager.HeldLongNotes[noteIndex].Info.Lane - 1].StopHolding();

            // Calculate Score + Get Judgement.
            var hitDifference = (int) (manager.HeldLongNotes[noteIndex].TrueEndTime - (int) Ruleset.Screen.Timing.Time);
            var processor = (ScoreProcessorKeys)Ruleset.ScoreProcessor;
            var judgement = processor.CalculateScore(hitDifference, KeyPressType.Release);

            // If LN has been released during a window
            if (judgement != Judgement.Ghost)
            {

                // Add new hit stat data.
                var stat = new HitStat(HitStatType.Hit, KeyPressType.Release, manager.HeldLongNotes[noteIndex].Info, (int) Ruleset.Screen.Timing.Time,
                                            judgement, hitDifference, Ruleset.ScoreProcessor.Accuracy, Ruleset.ScoreProcessor.Health);
                Ruleset.ScoreProcessor.Stats.Add(stat);

                var screenView = (GameplayScreenView)Ruleset.Screen.View;
                screenView.UpdateScoreboardUsers();

                // Also add a judgement to the hit error.
                playfield.Stage.HitError.AddJudgement(judgement, manager.HeldLongNotes[noteIndex].TrueEndTime - (int) Ruleset.Screen.Timing.Time);

                // Perform hit burst animation
                playfield.Stage.JudgementHitBurst.PerformJudgementAnimation(judgement);

                // Lastly kill the object.
                manager.KillHoldPoolObject(noteIndex, true);
            }
            // If LN has been released early
            else
            {
                // Judgement for when the player releases too early
                const Judgement missedJudgement = Judgement.Miss;

                // Count it as a miss if it was released early and kill the hold.
                Ruleset.ScoreProcessor.CalculateScore(missedJudgement);

                // Add new hit stat data.
                var stat = new HitStat(HitStatType.Hit, KeyPressType.Release, manager.HeldLongNotes[noteIndex].Info, (int) Ruleset.Screen.Timing.Time,
                                            Judgement.Miss, hitDifference, Ruleset.ScoreProcessor.Accuracy, Ruleset.ScoreProcessor.Health);
                Ruleset.ScoreProcessor.Stats.Add(stat);

                var screenView = (GameplayScreenView)Ruleset.Screen.View;
                screenView.UpdateScoreboardUsers();

                // Perform hit burst animation
                playfield.Stage.JudgementHitBurst.PerformJudgementAnimation(Judgement.Miss);

                manager.KillHoldPoolObject(noteIndex);
            }

            #region Old HandleKeyRelease
            // OLD
            /*

            // Don't bother executing if there aren't any long notes.
            if (manager.HeldLongNotes.Count == 0)
                return;

            // Check which window the object has
            var receivedJudgementIndex = -1;

            // Stores the hit time difference. Declared out of scope of the loop so we can use it
            // to store hit data.
            double timeDiff = 0;

            // JudgementWindow.Count -1 here because we don't count "misses" in this case, which is the last judgement.
            for (var j = 0; j < Ruleset.ScoreProcessor.JudgementWindow.Count - 1; j++)
            {
                // Get the release window of the current judgement.
                var releaseWindow = Ruleset.ScoreProcessor.JudgementWindow[(Judgement)j] * Ruleset.ScoreProcessor.WindowReleaseMultiplier[(Judgement)j];

                timeDiff = manager.HeldLongNotes[noteIndex].TrueEndTime - Ruleset.Screen.Timing.Time;
                if (!(Math.Abs(timeDiff) < releaseWindow))
                    continue;

                receivedJudgementIndex = j;
                break;
            }

            // Make the combo display visible since it is now changing.
            var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;
            playfield.Stage.ComboDisplay.MakeVisible();

            // Stop looping hit lighting.
            playfield.Stage.HitLightingObjects[manager.HeldLongNotes[noteIndex].Info.Lane - 1].StopHolding();

            // If LN has been released during a window
            if (receivedJudgementIndex != -1)
            {
                // Calc new score.
                var receivedJudgement = (Judgement)receivedJudgementIndex;
                Ruleset.ScoreProcessor.CalculateScore(receivedJudgement);

                // Add new hit stat data.
                var stat = new HitStat(HitStatType.Hit, manager.HeldLongNotes[noteIndex].Info, Ruleset.Screen.Timing.Time,
                                            receivedJudgement, timeDiff, Ruleset.ScoreProcessor.Accuracy, Ruleset.ScoreProcessor.Health);
                Ruleset.ScoreProcessor.Stats.Add(stat);

                var screenView = (GameplayScreenView)Ruleset.Screen.View;
                screenView.UpdateScoreboardUsers();

                // Also add a judgement to the hit error.
                playfield.Stage.HitError.AddJudgement((Judgement)receivedJudgementIndex, manager.HeldLongNotes[noteIndex].TrueEndTime - Ruleset.Screen.Timing.Time);

                // Perform hit burst animation
                playfield.Stage.JudgementHitBurst.PerformJudgementAnimation((Judgement)receivedJudgementIndex);

                // Lastly kill the object.
                manager.KillHoldPoolObject(noteIndex, true);
            }
            // If LN has been released early
            else
            {
                const Judgement receivedJudgement = Judgement.Miss;

                // Count it as an okay if it was released early and kill the hold.
                Ruleset.ScoreProcessor.CalculateScore(receivedJudgement);

                // Add new hit stat data.
                var stat = new HitStat(HitStatType.Hit, manager.HeldLongNotes[noteIndex].Info, Ruleset.Screen.Timing.Time,
                                            receivedJudgement, timeDiff, Ruleset.ScoreProcessor.Accuracy, Ruleset.ScoreProcessor.Health);
                Ruleset.ScoreProcessor.Stats.Add(stat);

                var screenView = (GameplayScreenView)Ruleset.Screen.View;
                screenView.UpdateScoreboardUsers();

                // Perform hit burst animation
                playfield.Stage.JudgementHitBurst.PerformJudgementAnimation(Judgement.Miss);

                manager.KillHoldPoolObject(noteIndex);
            }
            */
            #endregion
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
