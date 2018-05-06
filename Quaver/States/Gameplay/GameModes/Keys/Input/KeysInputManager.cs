using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.Config;
using Quaver.Helpers;
using Quaver.Input;
using Quaver.Main;

namespace Quaver.States.Gameplay.GameModes.Keys.Input
{
    internal class KeysInputManager : IGameplayInputManager
    {
        /// <summary>
        ///     The list of button containers for these keys.
        /// </summary>
        private List<KeysInputButtonContainer> BindingStore { get; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="mode"></param>
        internal KeysInputManager(GameMode mode)
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
        }

        /// <summary>
        ///     Handle the input 
        /// </summary>
        public void HandleInput(double dt)
        {
            foreach (var binding in BindingStore)
            {
                if (InputHelper.IsUniqueKeyPress(binding.Key.Value) && !binding.Pressed)
                {
                    binding.Pressed = true;
                    
                    // Handle Key Press
                    Console.WriteLine($"Lane {BindingStore.IndexOf(binding) + 1} Key Pressed");
                }
                else if (GameBase.KeyboardState.IsKeyUp(binding.Key.Value) && binding.Pressed)
                {
                    binding.Pressed = false;
                    
                    // Handle Key Release
                    Console.WriteLine($"Lane {BindingStore.IndexOf(binding) + 1} Key Released");
                }
            }
        }
    }
}