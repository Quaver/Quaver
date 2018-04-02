using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Audio;
using Quaver.Logging;
using Quaver.Main;
using Quaver.Modifiers.Mods;
using Quaver.Modifiers.Mods.Mania;

namespace Quaver.Modifiers
{
    /// <summary>
    ///     Entire class that controls the addition and removal of game mods.
    /// </summary>
    internal class ModManager
    {
        /// <summary>
        ///     Adds a gameplayModifier to our list, getting rid of any incompatible mods that are currently in there.
        ///     Also, specifying a speed, if need-be. That is only "required" if passing in ModIdentifier.ManiaModSpeed
        /// </summary>
        public static void AddMod(ModIdentifier modIdentifier)
        {
            IGameplayModifier gameplayModifier;

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
                    gameplayModifier = new ManiaModSpeed(modIdentifier);
                    break;
                case ModIdentifier.NoSliderVelocity:
                    gameplayModifier = new ManiaModNoSliderVelocities();
                    break;
                case ModIdentifier.Strict:
                    gameplayModifier = new ManiaModStrict();
                    break;
                case ModIdentifier.Chill:
                    gameplayModifier = new ManiaModChill();
                    break;
                default:
                    return;
            }

            // Check if any incompatible mods are already in our current game modifiers, and remove them if that is the case.
            var incompatibleMods = GameBase.CurrentGameModifiers.FindAll(x => x.IncompatibleMods.Contains(gameplayModifier.ModIdentifier));
            incompatibleMods.ForEach(x => RemoveMod(x.ModIdentifier));

            // Add The Mod
            GameBase.CurrentGameModifiers.Add(gameplayModifier);

            // Initialize the gameplayModifier and set its score multiplier.
            GameBase.ScoreMultiplier += gameplayModifier.ScoreMultiplierAddition;
            gameplayModifier.InitializeMod();  
            
            Logger.LogSuccess($"Added Mod: {gameplayModifier.ModIdentifier} and removed all incompatible mods.", LogType.Runtime);
            Logger.LogInfo($"Current Mods: {string.Join(", ", GameBase.CurrentGameModifiers.Select(x => x.ToString()))}", LogType.Runtime);
        }

        /// <summary>
        ///     Removes a gameplayModifier from our GameBase
        /// </summary>
        public static void RemoveMod(ModIdentifier modIdentifier)
        {
            try
            {
                // Try to find the removed gameplayModifier in the list
                var removedMod = GameBase.CurrentGameModifiers.Find(x => x.ModIdentifier == modIdentifier);

                // Remove The Mod's score multiplier
                GameBase.ScoreMultiplier -= removedMod.ScoreMultiplierAddition;
   
                // Remove the Mod
                GameBase.CurrentGameModifiers.Remove(removedMod);
                Logger.LogSuccess($"Removed {modIdentifier} from the current game modifiers.", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Checks if a gameplayModifier is currently activated.
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
            GameBase.AudioEngine.PlaybackRate= 1.0f;
        }

        /// <summary>
        ///     Removes any speed mods from the game and resets the clock
        /// </summary>
        public static void RemoveSpeedMods()
        {
            try
            {
                GameBase.CurrentGameModifiers.RemoveAll(x => x.Type == ModType.Speed);
                GameBase.AudioEngine.SetPlaybackRate();

                Logger.LogSuccess($"Removed ManiaModSpeed Mods from the current game modifiers.", LogType.Runtime);
                Logger.LogInfo($"Current Mods: {string.Join(", ", GameBase.CurrentGameModifiers.Select(x => x.ToString()))}", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Makes sure that the speed gameplayModifier selected matches up with the game clock and sets the correct one.
        /// </summary>
        public static void CheckModInconsistencies()
        {
            var mod = GameBase.CurrentGameModifiers.Find(x => x.Type == ModType.Speed);

            // Re-intialize the correct gameplayModifier.
            var index = GameBase.CurrentGameModifiers.IndexOf(mod);

            if (index != -1)
                GameBase.CurrentGameModifiers[index] = new ManiaModSpeed(mod.ModIdentifier);
            else
                GameBase.AudioEngine.PlaybackRate = 1.0f;
        }
    }
}
