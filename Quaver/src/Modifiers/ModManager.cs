using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Logging;

namespace Quaver.Modifiers
{
    /// <summary>
    ///     Entire class that controls the addition and removal of game mods.
    /// </summary>
    internal class ModManager
    {
        /// <summary>
        ///     Adds a mod to our list, getting rid of any incompatible mods that are currently in there.
        ///     Also, specifying a speed, if need-be. That is only "required" if passing in ModIdentifier.Speed
        /// </summary>
        public static void AddMod(ModIdentifier modIdentifier, float speedRate = 1.0f)
        {
            IMod mod;

            // Set the newMod based on the ModType that is coming in
            switch (modIdentifier)
            {
                case ModIdentifier.Speed:
                    // Throw an exception if the speedRate isn't specified, but yet someone is trying to add the mod.
                    if (speedRate == 1.0f)
                        throw new ArgumentException("speedRate must be specified if you are adding ModIdentifier.Speed");
                    
                    mod = new Speed(speedRate);
                    break;
                case ModIdentifier.NoSliderVelocity:
                    mod = new NoSliderVelocities();
                    break;
                default:
                    return;
            }

            // Check if any incompatible mods are already in our current game modifiers, and remove them if that is the case.
            var incompatibleMods = GameBase.CurrentGameModifiers.FindAll(x => x.IncompatibleMods.Contains(mod.ModIdentifier));
            incompatibleMods.ForEach(x => RemoveMod(x.ModIdentifier));

            // Add The Mod
            GameBase.CurrentGameModifiers.Add(mod);

            // Initialize the mod and set its score multiplier.
            GameBase.ScoreMultiplier += mod.ScoreMultiplierAddition;
            mod.InitializeMod();  
            
            Console.WriteLine($"[MOD MANAGER] Added Mod: {mod.ModIdentifier} and removed all incompatible mods.");
            Console.WriteLine($"[MOD MANAGER] Current Mods: {String.Join(", ", GameBase.CurrentGameModifiers.Select(x => x.ToString()))}");
        }

        /// <summary>
        ///     Removes a mod from our GameBase
        /// </summary>
        public static void RemoveMod(ModIdentifier modIdentifier)
        {
            try
            {
                // Try to find the removed mod in the list
                var removedMod = GameBase.CurrentGameModifiers.Find(x => x.ModIdentifier == modIdentifier);

                // Remove The Mod's score multiplier
                GameBase.ScoreMultiplier -= removedMod.ScoreMultiplierAddition;

                // Remove the mod's speed modification
                if (modIdentifier == ModIdentifier.Speed) GameBase.GameClock = 1.0f;

                // Remove the Mod
                GameBase.CurrentGameModifiers.Remove(removedMod);
                Console.WriteLine($"[MOD MANAGER] Removed {modIdentifier} from the current game modifiers.");

            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Color.Red, 5.0f);
                Console.WriteLine("[MOD MANAGER] Error: Trying to remove mod that isn't activated. Moving On!");
            }
        }

        /// <summary>
        ///     Checks if a mod is currently activated.
        /// </summary>
        /// <param name="modIdentifier"></param>
        /// <returns></returns>
        public static bool Activated(ModIdentifier modIdentifier)
        {
            return GameBase.CurrentGameModifiers.Exists(x => x.ModIdentifier == modIdentifier);
        }

        /// <summary>
        ///     Removes all items from our list of mods
        /// </summary>
        public static void RemoveAllMods()
        {
            GameBase.CurrentGameModifiers.Clear();

            // Reset all GameBase variables to its defaults
            GameBase.ScoreMultiplier = 1.0f;
            GameBase.GameClock = 1.0f;
        }
    }
}
