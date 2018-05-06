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
        private List<KeysInputButtonContainer> BindingStore { get; }

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
                    BindingStore = new List<KeysInputButtonContainer>
                    {
                        new KeysInputButtonContainer(ConfigManager.KeyMania4K1),
                        new KeysInputButtonContainer(ConfigManager.KeyMania4K2),
                        new KeysInputButtonContainer(ConfigManager.KeyMania4K3),
                        new KeysInputButtonContainer(ConfigManager.KeyMania4K4)
                    };
                    break;
                case GameMode.Keys7:
                    // Initialize 7K input button container.
                    BindingStore = new List<KeysInputButtonContainer>
                    {
                        new KeysInputButtonContainer(ConfigManager.KeyMania7K1),
                        new KeysInputButtonContainer(ConfigManager.KeyMania7K2),
                        new KeysInputButtonContainer(ConfigManager.KeyMania7K3),
                        new KeysInputButtonContainer(ConfigManager.KeyMania7K4),
                        new KeysInputButtonContainer(ConfigManager.KeyMania7K5),
                        new KeysInputButtonContainer(ConfigManager.KeyMania7K6),
                        new KeysInputButtonContainer(ConfigManager.KeyMania7K7)
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

                // Get the object pool itself.
                var objectPool = (KeysHitObjectPool) Ruleset.HitObjectPool;
                    
                // Find the object that is nearest in the lane that the user has pressed.
                var index = objectPool.GetIndexOfNearestLaneObject(i + 1, Ruleset.Screen.AudioTiming.CurrentTime);

                // Don't proceed if an object wasn't found.
                if (index == -1)
                    continue;
                
                Console.WriteLine($"{index}: " + (BindingStore[i].Pressed ? "Pressed" : "Released") + $" @{Ruleset.Screen.AudioTiming.CurrentTime}ms");
                
                // If the key was pressed, 
                if (BindingStore[i].Pressed)
                {
                    // Play the HitSounds for this object.
                    objectPool.PlayObjectHitSounds(index);
                }                   
            }
        }
    }
}