using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Main;

namespace Quaver.Modifiers
{
    /// <summary>
    ///     Entire class that controls the addition and removal of game mods.
    /// </summary>
    internal class ModManager
    {
        /// <summary>
        ///     Adds a mod to our list, getting rid of any incompatible mods that are currently in there.
        /// </summary>
        public static void AddMod(ModIdentifier modIdentifier)
        {
            IMod mod;

            // Set the newMod based on the ModType that is coming in
            switch (modIdentifier)
            {
                case ModIdentifier.Speed15X:
                    mod = new Speed15X();
                    break;
                case ModIdentifier.Speed75X:
                    mod = new Speed75X();
                    break;
                case ModIdentifier.NoSliderVelocity:
                    mod = new NoSliderVelocities();
                    break;
                default:
                    return;
            }

            // First check to see is already activated there.
            if (GameBase.CurrentGameModifiers.Exists(x => x.ModIdentifier == mod.ModIdentifier))
            {
                Console.WriteLine($"[MOD MANAGER] Error: Game Modifier {mod.ModIdentifier} has already been activated.");
                return;
            }

            // Check if any incompatible mods are already in our current game modifiers, and remove them if that is the case.
            GameBase.CurrentGameModifiers.RemoveAll(x => x.IncompatibleMods.Contains(mod.ModIdentifier));

            // Add The Mod
            GameBase.CurrentGameModifiers.Add(mod);

            // Initialize the mod and set its score multiplier.
            GameBase.ScoreMultiplier += mod.ScoreMultiplierAddition;
            mod.InitializeMod();  
            
            Console.WriteLine($"[MOD MANAGER] Added Mod: {mod.ModIdentifier} and removed all incompatible mods.");
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
                if (removedMod.SpeedAlterationRate != 1.0f) GameBase.GameClock = 1.0f;

                // Remove the Mod
                GameBase.CurrentGameModifiers.Remove(removedMod);
                Console.WriteLine($"[MOD MANAGER] Removed {modIdentifier} from the current game modifiers.");

            }
            catch (Exception e)
            {
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
    }
}
