/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Shared.Audio;
using Quaver.Shared.Modifiers.Mods;
using Wobble.Logging;

namespace Quaver.Shared.Modifiers
{
    /// <summary>
    ///     Entire class that controls the addition and removal of game mods.
    /// </summary>
    internal static class ModManager
    {
        /// <summary>
        ///     The list of currently activated game modifiers.
        /// </summary>
        public static List<IGameplayModifier> CurrentModifiersList { get; } = new List<IGameplayModifier>();

        /// <summary>
        ///     The current modifiers in ModId format
        /// </summary>
        public static ModIdentifier Mods
        {
            get
            {
                var mods = 0;

                foreach (var mod in CurrentModifiersList)
                    mods += (int) mod.ModIdentifier;

                return (ModIdentifier) mods;
            }
        }

        /// <summary>
        ///     Event emitted when mods have changed.
        /// </summary>
        public static event EventHandler<ModsChangedEventArgs> ModsChanged;

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
                    gameplayModifier = new ModSpeed(modIdentifier);
                    break;
                case ModIdentifier.NoSliderVelocity:
                    gameplayModifier = new ModNoSliderVelocities();
                    break;
                case ModIdentifier.Strict:
                    gameplayModifier = new ModStrict();
                    break;
                case ModIdentifier.Chill:
                    gameplayModifier = new ModChill();
                    break;
                case ModIdentifier.Autoplay:
                    gameplayModifier = new ModAutoplay();
                    break;
                case ModIdentifier.Paused:
                    gameplayModifier = new ModPaused();
                    break;
                case ModIdentifier.NoFail:
                    gameplayModifier = new ModNoFail();
                    break;
                case ModIdentifier.NoLongNotes:
                    gameplayModifier = new ModNoLongNotes();
                    break;
                case ModIdentifier.Randomize:
                    gameplayModifier = new ModRandomize();
                    break;
                default:
                    return;
            }

            // Remove incompatible mods.
            var incompatibleMods = CurrentModifiersList.FindAll(x => x.IncompatibleMods.Contains(gameplayModifier.ModIdentifier));
            incompatibleMods.ForEach(x => RemoveMod(x.ModIdentifier));

            // Remove the mod if it's already on.
            var alreadyOnMod = CurrentModifiersList.Find(x => x.ModIdentifier == gameplayModifier.ModIdentifier);
            if (alreadyOnMod != null)
                CurrentModifiersList.Remove(alreadyOnMod);

            // Add The Mod
            CurrentModifiersList.Add(gameplayModifier);
            gameplayModifier.InitializeMod();

            ModsChanged?.Invoke(typeof(ModManager), new ModsChangedEventArgs(Mods));
            Logger.Debug($"Added mod: {gameplayModifier.ModIdentifier}.", LogType.Runtime, false);
        }

        /// <summary>
        ///     Removes a gameplayModifier from our GameBase
        /// </summary>
        public static void RemoveMod(ModIdentifier modIdentifier)
        {
            try
            {
                // Try to find the removed gameplayModifier in the list
                var removedMod = CurrentModifiersList.Find(x => x.ModIdentifier == modIdentifier);

                // Remove the Mod
                CurrentModifiersList.Remove(removedMod);

                ModsChanged?.Invoke(typeof(ModManager), new ModsChangedEventArgs(Mods));
                Logger.Debug($"Removed mod: {removedMod.ModIdentifier}.", LogType.Runtime, false);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Checks if a gameplayModifier is currently activated.
        /// </summary>
        /// <param name="modIdentifier"></param>
        /// <returns></returns>
        public static bool IsActivated(ModIdentifier modIdentifier) => CurrentModifiersList.Exists(x => x.ModIdentifier == modIdentifier);

        /// <summary>
        ///     Removes all items from our list of mods
        /// </summary>
        public static void RemoveAllMods()
        {
            CurrentModifiersList.Clear();
            CheckModInconsistencies();

            ModsChanged?.Invoke(typeof(ModManager), new ModsChangedEventArgs(Mods));
            Logger.Debug("Removed all modifiers", LogType.Runtime, false);
        }

        /// <summary>
        ///     Adds speed mods from a given rate.
        /// </summary>
        /// <param name="rate"></param>
        public static void AddSpeedMods(float rate)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (rate == 1.0f)
            {
                RemoveSpeedMods();
                return;
            }

            var speedMod = ModHelper.GetModsFromRate(rate);

            if (speedMod == ModIdentifier.None)
                return;

            AddMod(speedMod);
        }

        /// <summary>
        ///     Removes any speed mods from the game and resets the clock
        /// </summary>
        public static void RemoveSpeedMods()
        {
            try
            {
                CurrentModifiersList.RemoveAll(x => x.Type == ModType.Speed);

                if (AudioEngine.Track != null)
                    AudioEngine.Track.Rate = ModHelper.GetRateFromMods(Mods);

                CheckModInconsistencies();

                ModsChanged?.Invoke(typeof(ModManager), new ModsChangedEventArgs(Mods));
                Logger.Debug("Removed all speed modifiers", LogType.Runtime, false);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Makes sure that the speed gameplayModifier selected matches up with the game clock and sets the correct one.
        /// </summary>
        private static void CheckModInconsistencies()
        {
            try
            {
                if (AudioEngine.Track != null && !AudioEngine.Track.IsDisposed)
                    AudioEngine.Track.Rate = ModHelper.GetRateFromMods(Mods);
            }
            catch (Exception e)
            {
                // ignored.
            }
        }
    }
}
