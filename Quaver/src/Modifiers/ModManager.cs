using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Audio;
using Quaver.Logging;
using Quaver.Modifiers.Mods;

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
        public static void AddMod(ModIdentifier modIdentifier)
        {
            IMod mod;

            // Set the newMod based on the ModType that is coming in
            switch (modIdentifier)
            {
                case ModIdentifier.Speed05X:                  
                case ModIdentifier.Speed06X:
                case ModIdentifier.Speed07X:
                case ModIdentifier.Speed08X:
                case ModIdentifier.Speed09X:
                case ModIdentifier.Speed11X:
                case ModIdentifier.Speed12X:
                case ModIdentifier.Speed13X:
                case ModIdentifier.Speed14X:
                case ModIdentifier.Speed15X:
                case ModIdentifier.Speed16X:
                case ModIdentifier.Speed17X:
                case ModIdentifier.Speed18X:
                case ModIdentifier.Speed19X:
                case ModIdentifier.Speed20X:
                    mod = new Speed(modIdentifier);
                    break;
                case ModIdentifier.NoSliderVelocity:
                    mod = new NoSliderVelocities();
                    break;
                case ModIdentifier.Strict:
                    mod = new Strict();
                    break;
                case ModIdentifier.Chill:
                    mod = new Chill();
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
            
            Logger.Log($"Added Mod: {mod.ModIdentifier} and removed all incompatible mods.", Color.Cyan);
            Logger.Log($"Current Mods: {String.Join(", ", GameBase.CurrentGameModifiers.Select(x => x.ToString()))}", Color.Cyan);
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
   
                // Remove the Mod
                GameBase.CurrentGameModifiers.Remove(removedMod);
                Logger.Log($"Removed {modIdentifier} from the current game modifiers.", Color.Cyan);

            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Color.Red, 5.0f);
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

        /// <summary>
        ///     Removes any speed mods from the game and resets the clock
        /// </summary>
        public static void RemoveSpeedMods()
        {
            try
            {
                GameBase.CurrentGameModifiers.RemoveAll(x => x.Type == ModType.Speed);
                GameBase.GameClock = 1.0f;
                SongManager.ChangeSongSpeed();
                Logger.Log($"Removed Speed Mods from the current game modifiers.", Color.Cyan);
                Logger.Log($"Current Mods: {String.Join(", ", GameBase.CurrentGameModifiers.Select(x => x.ToString()))}", Color.Cyan);
            }
            catch (Exception e)
            {
            }
        }
    }
}
