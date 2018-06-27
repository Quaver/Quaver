using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.Config;
using Quaver.Graphics.Sprites;
using Quaver.Helpers;
using Quaver.Input;
using Quaver.Logging;
using Quaver.Main;
using Quaver.States.Gameplay.GameModes.Keys.Playfield;
using Quaver.States.Gameplay.HitObjects;
using Quaver.States.Gameplay.Replays;

namespace Quaver.States.Gameplay.GameModes.Keys.Input
{
    internal class KeysInputManager : IGameplayInputManager
    {
        /// <summary>
        ///     The list of button containers for these keys.
        /// </summary>
        internal List<KeysInputBinding> BindingStore { get; }

        /// <summary>
        ///     Reference to the ruleset
        /// </summary>
        private GameModeRulesetKeys Ruleset { get;}

        /// <summary>
        ///     The replay input manager.
        /// </summary>
        internal ReplayInputManagerKeys ReplayInputManager { get; }
        
        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="ruleset"></param>
        /// <param name="mode"></param>
        /// <param name="replay"></param>
        internal KeysInputManager(GameModeRulesetKeys ruleset, GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Keys4:
                    // Initialize 4K Input button container.
                    BindingStore = new List<KeysInputBinding>
                    {
                        new KeysInputBinding(ConfigManager.KeyMania4K1),
                        new KeysInputBinding(ConfigManager.KeyMania4K2),
                        new KeysInputBinding(ConfigManager.KeyMania4K3),
                        new KeysInputBinding(ConfigManager.KeyMania4K4)
                    };
                    break;
                case GameMode.Keys7:
                    // Initialize 7K input button container.
                    BindingStore = new List<KeysInputBinding>
                    {
                        new KeysInputBinding(ConfigManager.KeyMania7K1),
                        new KeysInputBinding(ConfigManager.KeyMania7K2),
                        new KeysInputBinding(ConfigManager.KeyMania7K3),
                        new KeysInputBinding(ConfigManager.KeyMania7K4),
                        new KeysInputBinding(ConfigManager.KeyMania7K5),
                        new KeysInputBinding(ConfigManager.KeyMania7K6),
                        new KeysInputBinding(ConfigManager.KeyMania7K7)
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
         /// </summary>
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
                if (!BindingStore[i].Pressed && 
                        (InputHelper.IsUniqueKeyPress(BindingStore[i].Key.Value) && ReplayInputManager == null
                        || ReplayInputManager != null && ReplayInputManager.UniquePresses[i]))
                {
                    // We've already handling the unique key press, so reset it.
                    if (ReplayInputManager != null)
                        ReplayInputManager.UniquePresses[i] = false;
                    
                    BindingStore[i].Pressed = true;
                    needsUpdating = true;
                    
                    // Add to keys per second
                    Ruleset.Screen.UI.KpsDisplay.AddClick();
                }
                // A key was uniquely released.
                else if (BindingStore[i].Pressed && 
                            (InputHelper.IsUniqueKeyRelease(BindingStore[i].Key.Value) && ReplayInputManager == null 
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
                var playfield = (KeysPlayfield) Ruleset.Playfield;             
                playfield.Stage.SetReceptorAndLightingActivity(i, BindingStore[i].Pressed);

                // Get the object manager itself.
                var manager = (KeysHitObjectManager) Ruleset.HitObjectManager;
                    
                // Find the object that is nearest in the lane that the user has pressed.
                var objectIndex = manager.GetIndexOfNearestLaneObject(i + 1, Ruleset.Screen.Timing.CurrentTime);

                // Don't proceed if an object wasn't found.
                if (objectIndex == -1)
                    continue;
                
                // If the key was pressed during this frame.
                if (BindingStore[i].Pressed)
                {
                    HandleKeyPress(manager, (KeysHitObject) manager.ObjectPool[objectIndex], objectIndex);
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
        private void HandleKeyPress(KeysHitObjectManager manager, KeysHitObject hitObject, int objectIndex)
        {
            // Play the HitSounds for this object.
            HitObjectManager.PlayObjectHitSounds(manager.ObjectPool[objectIndex].Info);

            // Check which hit window this object's timing is in
            for (var j = 0; j < Ruleset.ScoreProcessor.JudgementWindow.Count; j++)
            {
                var time = Ruleset.Screen.Timing.CurrentTime;
                var hitDifference = hitObject.TrueStartTime - time;
                
                // Check if the user actually hit the object.
                if (!(Math.Abs(hitDifference) <= Ruleset.ScoreProcessor.JudgementWindow[(Judgement) j])) 
                    continue;
                        
                var judgement = (Judgement) j;
                            
                // Update the user's score
                Ruleset.ScoreProcessor.CalculateScore(judgement);

                // Add new hit stat data.
                var stat = new HitStat(HitStatType.Hit, hitObject.Info, time, judgement, hitDifference, 
                                        Ruleset.ScoreProcessor.Accuracy, Ruleset.ScoreProcessor.Health);
                Ruleset.ScoreProcessor.Stats.Add(stat);
                
                // Update all the users on the scoreboard.
                Ruleset.Screen.UI.UpdateScoreboardUsers();
                
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
                var playfield = (KeysPlayfield) Ruleset.Playfield;
                playfield.Stage.ComboDisplay.MakeVisible();

                // Also add a judgement to the hit error.
                playfield.Stage.HitError.AddJudgement(judgement, hitObject.TrueStartTime - Ruleset.Screen.Timing.CurrentTime);
                               
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
                    playfield.Stage.HitLighting[laneIndex].IsHoldingLongNote = true;
                
                playfield.Stage.HitLighting[laneIndex].PerformHitAnimation();
                break;
            }
        }

        /// <summary>
        ///     Handles an individual key release during gameplay.
        /// </summary>
        private void HandleKeyRelease(KeysHitObjectManager manager, int noteIndex)
        {            
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
                var releaseWindow = Ruleset.ScoreProcessor.JudgementWindow[(Judgement) j] * Ruleset.ScoreProcessor.WindowReleaseMultiplier[(Judgement) j];

                timeDiff = manager.HeldLongNotes[noteIndex].TrueEndTime - Ruleset.Screen.Timing.CurrentTime;
                if (!(Math.Abs(timeDiff) < releaseWindow)) 
                    continue;
                        
                receivedJudgementIndex = j;
                break;
            }
    
            // Make the combo display visible since it is now changing.
            var playfield = (KeysPlayfield) Ruleset.Playfield;
            playfield.Stage.ComboDisplay.MakeVisible();
                                  
            // Stop looping hit lighting.
            playfield.Stage.HitLighting[manager.HeldLongNotes[noteIndex].Info.Lane - 1].StopHolding();
                
            // If LN has been released during a window
            if (receivedJudgementIndex != -1)
            {
                // Calc new score.
                var receivedJudgement = (Judgement) receivedJudgementIndex;
                Ruleset.ScoreProcessor.CalculateScore(receivedJudgement);
                
                // Add new hit stat data.
                var stat = new HitStat(HitStatType.Hit, manager.HeldLongNotes[noteIndex].Info, Ruleset.Screen.Timing.CurrentTime, 
                                            receivedJudgement, timeDiff, Ruleset.ScoreProcessor.Accuracy, Ruleset.ScoreProcessor.Health);
                Ruleset.ScoreProcessor.Stats.Add(stat);
                
                // Update all the users on the scoreboard.
                Ruleset.Screen.UI.UpdateScoreboardUsers();
                
                // Also add a judgement to the hit error.
                playfield.Stage.HitError.AddJudgement((Judgement)receivedJudgementIndex, manager.HeldLongNotes[noteIndex].TrueEndTime - Ruleset.Screen.Timing.CurrentTime);
                
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
                var stat = new HitStat(HitStatType.Hit, manager.HeldLongNotes[noteIndex].Info, Ruleset.Screen.Timing.CurrentTime, 
                                            receivedJudgement, timeDiff, Ruleset.ScoreProcessor.Accuracy, Ruleset.ScoreProcessor.Health);
                Ruleset.ScoreProcessor.Stats.Add(stat);
                
                // Update all the users on the scoreboard.
                Ruleset.Screen.UI.UpdateScoreboardUsers();
                
                // Perform hit burst animation
                playfield.Stage.JudgementHitBurst.PerformJudgementAnimation(Judgement.Miss);
                
                manager.KillHoldPoolObject(noteIndex);
            } 
        }

        /// <summary>
        ///     Handles scroll speed changes.
        /// </summary>
        private void ChangeScrollSpeed()
        {
            // Only allow scroll speed changes if the map hasn't started or if we're on a break
            if (Ruleset.Screen.Timing.CurrentTime >= 5000 && !Ruleset.Screen.OnBreak)
                return;
            
            if (InputHelper.IsUniqueKeyPress(ConfigManager.KeyDecreaseScrollSpeed.Value))
            {
                switch (Ruleset.Screen.Map.Mode)
                {
                    case GameMode.Keys4:
                        ConfigManager.ScrollSpeed4K.Value--;
                        Logger.LogImportant($"Scroll Speed Set To: {ConfigManager.ScrollSpeed4K.Value}", LogType.Runtime);
                        break;
                    case GameMode.Keys7:
                        ConfigManager.ScrollSpeed7K.Value--;
                        Logger.LogImportant($"Scroll Speed Set To: {ConfigManager.ScrollSpeed7K.Value}", LogType.Runtime);
                        break;
                }
            }
            else if (InputHelper.IsUniqueKeyPress(ConfigManager.KeyIncreaseScrollSpeed.Value))
            {
                switch (Ruleset.Screen.Map.Mode)
                {
                    case GameMode.Keys4:
                        ConfigManager.ScrollSpeed4K.Value++;
                        Logger.LogImportant($"Scroll Speed Set To: {ConfigManager.ScrollSpeed4K.Value}", LogType.Runtime);
                        break;
                    case GameMode.Keys7:
                        ConfigManager.ScrollSpeed7K.Value++;
                        Logger.LogImportant($"Scroll Speed Set To: {ConfigManager.ScrollSpeed7K.Value}", LogType.Runtime);
                        break;
                }
            }
        }
    }
}