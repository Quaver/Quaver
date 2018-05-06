using System;
using System.Collections.Generic;
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
        ///     Reference to the playfield for this input manager.
        /// </summary>
        private KeysPlayfield Playfield { get; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="playfield"></param>
        /// <param name="mode"></param>
        internal KeysInputManager(KeysPlayfield playfield, GameMode mode)
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

            Playfield = playfield;
        }

        /// <summary>
        ///     Handle the input 
        /// </summary>
        public void HandleInput(double dt)
        {
            for (var i = 0; i < BindingStore.Count; i++)
            {
                // Key Pressed Uniquely
                if (InputHelper.IsUniqueKeyPress(BindingStore[i].Key.Value) && !BindingStore[i].Pressed)
                {
                    BindingStore[i].Pressed = true;
                                  
                    // Handle Key Press
                    Console.WriteLine($"Lane {BindingStore.IndexOf(BindingStore[i]) + 1} Key Pressed");
                    
                    Console.WriteLine(Playfield.Width);
                    Playfield.Stage.SetReceptorAndLightingActivity(i, BindingStore[i].Pressed);

                }
                // Key Released Uniquely.
                else if (GameBase.KeyboardState.IsKeyUp(BindingStore[i].Key.Value) && BindingStore[i].Pressed)
                {
                    BindingStore[i].Pressed = false;
                
                    Console.WriteLine($"Lane {BindingStore.IndexOf(BindingStore[i]) + 1} Key Released");
                    
                    // Handle Key Release
                    Playfield.Stage.SetReceptorAndLightingActivity(i, BindingStore[i].Pressed);
                }
            }
        }
    }
}