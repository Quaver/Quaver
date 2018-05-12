using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Quaver.API.Enums;
using Quaver.Config;
using Quaver.Helpers;
using Quaver.Input;
using Quaver.Main;
using Quaver.States.Gameplay.GameModes.Keys.Playfield;

namespace Quaver.States.Gameplay.GameModes.Keys.Input
{
    internal class KeysInputManager : IGameplayInputManager
    {
        /// <summary>
        ///     The list of button containers for these keys.
        /// </summary>
        private List<KeysInputBinding> BindingStore { get; }

        /// <summary>
        ///     Reference to the ruleset
        /// </summary>
        private GameModeKeys Ruleset { get;}

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="ruleset"></param>
        /// <param name="mode"></param>
        internal KeysInputManager(GameModeKeys ruleset, GameMode mode)
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
        }

         /// <inheritdoc />
         /// <summary>
         /// </summary>
        public void HandleInput(double dt)
        {
            for (var i = 0; i < BindingStore.Count; i++)
            {
                // Keeps track of if this key input is is important enough for us to want to 
                // update more things like animations, score, etc.
                var needsUpdating = false;
                
                // Key Pressed Uniquely
                if (InputHelper.IsUniqueKeyPress(BindingStore[i].Key.Value) && !BindingStore[i].Pressed)
                {
                    BindingStore[i].Pressed = true;
                    needsUpdating = true;
                }
                // Key Released Uniquely.
                else if (GameBase.KeyboardState.IsKeyUp(BindingStore[i].Key.Value) && BindingStore[i].Pressed)
                {
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
                var objectIndex = manager.GetIndexOfNearestLaneObject(i + 1, Ruleset.Screen.AudioTiming.CurrentTime);
                var hitObject = (KeysHitObject) manager.ObjectPool[objectIndex];

                // Don't proceed if an object wasn't found.
                if (objectIndex == -1)
                    continue;
                                
                // If the key was pressed, 
                if (BindingStore[i].Pressed)
                {
                    // Play the HitSounds for this object.
                    manager.PlayObjectHitSounds(objectIndex);

                    // Check which hit window this object's timing is in
                    for (var j = 0; j < Ruleset.ScoreProcessor.JudgementWindow.Count; j++)
                    {
                        // Check if the user actually hit the object.
                        if (!(Math.Abs(hitObject.Info.StartTime - Ruleset.Screen.AudioTiming.CurrentTime) <= Ruleset.ScoreProcessor.JudgementWindow[(Judgement) j])) 
                            continue;
                        
                        var judgement = (Judgement) j;
                            
                        // Update the user's score
                        Ruleset.ScoreProcessor.CalculateScore(judgement);

                        // If the user is spamming.
                        if (judgement >= Judgement.Good)
                        {
                            manager.KillPoolObject(objectIndex);
                        }
                        else
                        {
                            // If the object is an LN, change the status to held.
                            if (hitObject.IsLongNote)
                                manager.ChangePoolObjectStatusToHeld(objectIndex);
                            // Otherwise, just recycle the object.
                            else
                                manager.RecyclePoolObject(objectIndex);
                        }

                        break;
                    }
                }
                // If the key was released.
                else
                {                                   
                    // Look for the nearest long note in the current lane.
                    var longNoteIndex = -1;
                    for (i = 0; i < manager.HeldLongNotes.Count; i++)
                    {
                        if (manager.HeldLongNotes[i].Info.Lane != i + 1) 
                            continue;
                        
                        longNoteIndex = i;
                        break;
                    }

                    // Return if we can't find a long note.
                    if (longNoteIndex == -1)
                        return;
                    
                    // Keeps track of if we've released in a judgement window.
                    var judgementIndex = -1;
                    
                    for (var j = 0; j < Ruleset.ScoreProcessor.JudgementWindow.Count; j++)
                    {
                        var judgement = (Judgement) j;

                        // The window for releasing a long note. (Window * Multiplier.)
                        var releaseWindow = Ruleset.ScoreProcessor.JudgementWindow[judgement] * Ruleset.ScoreProcessor.WindowReleaseMultiplier[judgement];
                        
                        // Find if we've released in a judgement window.
                        if (!(Math.Abs(hitObject.Info.EndTime - Ruleset.Screen.AudioTiming.CurrentTime) < releaseWindow))
                            continue;
                        
                        judgementIndex = i;
                        break;
                    }
                    
                    // If LN has been released during a HitWindow
                    if (judgementIndex > -1)
                    {
                        // Update the user's score with that specific judgement.
                        Ruleset.ScoreProcessor.CalculateScore((Judgement) judgementIndex);
                    }
                    // Otherwise if it has been released too early, count is as a miss.
                    else
                    {
                        Ruleset.ScoreProcessor.CalculateScore(Judgement.Miss);
                        
                        // Stop holding the note.
                        manager.KillHoldPoolObject(longNoteIndex);
                    }
                }
            }
        }
    }
}