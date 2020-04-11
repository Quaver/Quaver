/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Modifiers.Mods;
using Quaver.Shared.Online;
using Wobble.Logging;
using Wobble.Managers;
//using Quaver.Shared.Graphics.Notifications;

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
                var mods = 0L;

                foreach (var mod in CurrentModifiersList)
                    mods += (long) mod.ModIdentifier;

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
        public static void AddMod(ModIdentifier modIdentifier, bool updateMultiplayerMods = false)
        {
            IGameplayModifier gameplayModifier;

            // Set the newMod based on the ModType that is coming in
            switch (modIdentifier)
            {
                case ModIdentifier.Speed05X:
                case ModIdentifier.Speed055X:
                case ModIdentifier.Speed06X:
                case ModIdentifier.Speed065X:
                case ModIdentifier.Speed07X:
                case ModIdentifier.Speed075X:
                case ModIdentifier.Speed08X:
                case ModIdentifier.Speed085X:
                case ModIdentifier.Speed09X:
                case ModIdentifier.Speed095X:
                case ModIdentifier.Speed105X:
                case ModIdentifier.Speed11X:
                case ModIdentifier.Speed115X:
                case ModIdentifier.Speed12X:
                case ModIdentifier.Speed125X:
                case ModIdentifier.Speed13X:
                case ModIdentifier.Speed135X:
                case ModIdentifier.Speed14X:
                case ModIdentifier.Speed145X:
                case ModIdentifier.Speed15X:
                case ModIdentifier.Speed155X:
                case ModIdentifier.Speed16X:
                case ModIdentifier.Speed165X:
                case ModIdentifier.Speed17X:
                case ModIdentifier.Speed175X:
                case ModIdentifier.Speed18X:
                case ModIdentifier.Speed185X:
                case ModIdentifier.Speed19X:
                case ModIdentifier.Speed195X:
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
                case ModIdentifier.Inverse:
                    gameplayModifier = new ModInverse();
                    break;
                case ModIdentifier.FullLN:
                    gameplayModifier = new ModFullLN();
                    break;
                case ModIdentifier.Mirror:
                    gameplayModifier = new ModMirror();
                    break;
                case ModIdentifier.Coop:
                    gameplayModifier = new ModCoop();
                    break;
                case ModIdentifier.HeatlthAdjust:
                    gameplayModifier = new ModLongNoteAdjust();
                    break;
                default:
                    return;
            }

            // Remove incompatible mods.
            var incompatibleMods = CurrentModifiersList.FindAll(x => x.IncompatibleMods.Contains(gameplayModifier.ModIdentifier));
            incompatibleMods.ForEach(x => RemoveMod(x.ModIdentifier, false));

            // Remove the mod if it's already on.
            var alreadyOnMod = CurrentModifiersList.Find(x => x.ModIdentifier == gameplayModifier.ModIdentifier);

            if (alreadyOnMod != null)
                CurrentModifiersList.Remove(alreadyOnMod);

            // Add The Mod
            CurrentModifiersList.Add(gameplayModifier);
            gameplayModifier.InitializeMod();

            if (updateMultiplayerMods)
                UpdateMultiplayerMods();

            ModsChanged?.Invoke(typeof(ModManager), new ModsChangedEventArgs(ModChangeType.Add, Mods, modIdentifier));

            Logger.Debug($"Added mod: {gameplayModifier.ModIdentifier}.", LogType.Runtime, false);
        }

         /// <summary>
        ///     Removes a gameplayModifier from our GameBase
        /// </summary>
        public static void RemoveMod(ModIdentifier modIdentifier, bool updateMultiplayerMods = false)
        {
            try
            {
                // Try to find the removed gameplayModifier in the list
                var removedMod = CurrentModifiersList.Find(x => x.ModIdentifier == modIdentifier);

                // Remove the Mod
                CurrentModifiersList.Remove(removedMod);

                if (updateMultiplayerMods)
                    UpdateMultiplayerMods();

                ModsChanged?.Invoke(typeof(ModManager), new ModsChangedEventArgs(ModChangeType.Removal, Mods, modIdentifier));

                

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
        public static void RemoveAllMods(bool invokeEvent = true)
        {
            CurrentModifiersList.Clear();
            CheckModInconsistencies();
            UpdateMultiplayerMods();

            ModsChanged?.Invoke(typeof(ModManager), new ModsChangedEventArgs(ModChangeType.RemoveAll, Mods, ModIdentifier.None));

            Logger.Debug("Removed all modifiers", LogType.Runtime, false);
        }

        /// <summary>
        ///     Adds speed mods from a given rate.
        /// </summary>
        /// <param name="rate"></param>
        public static void AddSpeedMods(float rate)
        {
            Console.WriteLine("AddSpeedMods called");
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (rate == 1.0f)
            {
                RemoveSpeedMods(true);
                return;
            }

            var speedMod = ModHelper.GetModsFromRate(rate);

            if (speedMod == ModIdentifier.None)
                return;

            AddMod(speedMod);
            UpdateMultiplayerMods();
        }

        /// <summary>
        ///     Removes any speed mods from the game and resets the clock
        /// </summary>
        public static void RemoveSpeedMods(bool updateMultiplayerMods = false)
        {
            try
            {
                CurrentModifiersList.RemoveAll(x => x.Type == ModType.Speed);

                if (AudioEngine.Track != null && !AudioEngine.Track.IsDisposed)
                    AudioEngine.Track.Rate = ModHelper.GetRateFromMods(Mods);

                CheckModInconsistencies();

                if (updateMultiplayerMods)
                    UpdateMultiplayerMods();

                ModsChanged?.Invoke(typeof(ModManager), new ModsChangedEventArgs(ModChangeType.RemoveSpeed, Mods,
                    ModIdentifier.None));

                

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
        public static void CheckModInconsistencies()
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

        /// <summary>
        ///     Converts a mod combination to
        /// </summary>
        /// <param name="mods"></param>
        /// <returns></returns>
        public static List<ModIdentifier> GetModsList(ModIdentifier mods)
        {
            var list = new List<ModIdentifier>();

            for (var i = 0; i <= Math.Log(Math.Abs((long)mods), 2); i++)
            {
                var mod = (ModIdentifier)((long)Math.Pow(2, i));

                if (!mods.HasFlag(mod))
                    continue;

                try
                {
                    list.Add(mod);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }

            return list;
        }

        /// <summary>
        ///     Updates the activated mods for multiplayer if activated
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private static void UpdateMultiplayerMods()
        {
            var game = OnlineManager.CurrentGame;

            if (game == null)
                return;

            var isHost = game.HostId == OnlineManager.Self.OnlineUser.Id;

            // Check if the user is allowed to update mods based on the current match settings
            if (!isHost && game.FreeModType == MultiplayerFreeModType.None)
                return;

            var rate = ModHelper.GetModsFromRate(ModHelper.GetRateFromMods(Mods));
            var hostChangeableMods = CurrentModifiersList.FindAll(x => x.OnlyMultiplayerHostCanCanChange);
            var ourMods = game.PlayerMods.Find(x => x.UserId == OnlineManager.Self.OnlineUser.Id);
            var otherMods = CurrentModifiersList.FindAll(x => !x.OnlyMultiplayerHostCanCanChange
                                                              && x.Type != ModType.Speed
                                                              && !hostChangeableMods.Contains(x));

            // Free Mod isn't enabled, so if the user is host, then activate those mods globally.
            if (game.FreeModType == MultiplayerFreeModType.None)
            {
                if (!isHost)
                    return;

                var difficulty = MapManager.Selected.Value.DifficultyFromMods(Mods);
                OnlineManager.Client?.MultiplayerChangeGameModifiers((long) Mods, difficulty);
            }
            // Only free mod is enabled, so if the user is host, they have to enable the rate and host changeable
            // modifiers globally.
            else if (game.FreeModType == MultiplayerFreeModType.Regular)
            {
                var customMods = otherMods.Sum(x => (long) x.ModIdentifier);

                if (long.Parse(ourMods.Modifiers) != customMods)
                    OnlineManager.Client?.MultiplayerChangePlayerModifiers(customMods);

                if (isHost)
                {
                    ModIdentifier globalMods = 0;

                    if (rate != ModIdentifier.None)
                        globalMods |= rate;

                    globalMods |= (ModIdentifier) hostChangeableMods.Sum(x => (long) x.ModIdentifier);

                    var difficulty = MapManager.Selected.Value.DifficultyFromMods(globalMods);
                    OnlineManager.Client?.MultiplayerChangeGameModifiers((long) globalMods, difficulty);
                }
            }
            // Only free rate is enabled. If the user is host, enable host-only and custom modifiers globally,
            // while the rate is activated as a player-specific modifier.
            else if (game.FreeModType == MultiplayerFreeModType.Rate)
            {
                if (long.Parse(ourMods.Modifiers) != (long) rate)
                    OnlineManager.Client?.MultiplayerChangePlayerModifiers((long) rate);

                if (isHost)
                {
                    ModIdentifier globalMods = 0;

                    globalMods |= (ModIdentifier) hostChangeableMods.Sum(x => (long) x.ModIdentifier);
                    globalMods |= (ModIdentifier) otherMods.Sum(x => (long) x.ModIdentifier);

                    var difficulty = MapManager.Selected.Value.DifficultyFromMods(globalMods);
                    OnlineManager.Client?.MultiplayerChangeGameModifiers((long) globalMods, difficulty);
                }
            }
            // Both free mod and free rate are activated. If host, activate host mods globally.
            // otherwise activate all other mods customly.
            else if (game.FreeModType.HasFlag(MultiplayerFreeModType.Regular) &&
                     game.FreeModType.HasFlag(MultiplayerFreeModType.Rate))
            {
                ModIdentifier customMods = 0;

                if (rate > 0)
                    customMods |= rate;

                customMods |= (ModIdentifier) otherMods.Sum(x => (long) x.ModIdentifier);

                if (long.Parse(ourMods.Modifiers) != (long) customMods)
                    OnlineManager.Client?.MultiplayerChangePlayerModifiers((long) customMods);

                if (isHost)
                {
                    var globalMods = (ModIdentifier) hostChangeableMods.Sum(x => (long) x.ModIdentifier);

                    var difficulty = MapManager.Selected.Value.DifficultyFromMods(globalMods);
                    OnlineManager.Client?.MultiplayerChangeGameModifiers((long) globalMods, difficulty);
                }
            }

            Logger.Important($"Updating multiplayer mods: {Mods}", LogType.Runtime);
        }

        /// <summary>
        ///     Invokes <see cref="ModsChanged"/>
        /// </summary>
        public static void FireModsChangedEvent() => ModsChanged?.Invoke(typeof(ModManager), new ModsChangedEventArgs(ModChangeType.Add, Mods, Mods));

        /// <summary>
        ///     Gets a texture for an individual mod
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="inactive"></param>
        /// <returns></returns>
        public static Texture2D GetTexture(ModIdentifier mod, bool inactive = false)
        {
            if (mod <= 0)
                return TextureManager.Load($@"Quaver.Resources/Textures/UI/Mods/None.png");

            if (inactive)
                return TextureManager.Load($@"Quaver.Resources/Textures/UI/Mods/N-{ModHelper.GetModsString(mod)}.png");

            return TextureManager.Load($@"Quaver.Resources/Textures/UI/Mods/{ModHelper.GetModsString(mod)}.png");
        }
    }
}
