/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using Quaver.API.Enums;

namespace Quaver.Shared.Modifiers.Mods
{
    internal class ModNoSliderVelocities: IGameplayModifier
    {
        /// <inheritdoc />
        /// <summary>
        ///     The name of the gameplayModifier.
        /// </summary>
        public string Name { get; set; } = "No Slider Velocities";

        /// <inheritdoc />
        /// <summary>
        ///     The identifier of the mod.
        /// </summary>
        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.NoSliderVelocity;

        /// <inheritdoc />
        /// <summary>
        ///     The type of gameplayModifier as defined in the enum
        /// </summary>
        public ModType Type { get; set; } = ModType.Special;

        /// <inheritdoc />
        /// <summary>
        ///     The description of the Mod
        /// </summary>
        public string Description { get; set; } = "Hate scroll speed changes? Say no more.";

        /// <inheritdoc />
        /// <summary>
        ///     Is the gameplayModifier ranked?
        /// </summary>
        public bool Ranked { get; set; } = false;

        /// <inheritdoc />
        /// <summary>
        ///     The identifier of mods that are incompatible with this one.
        /// </summary>
        public ModIdentifier[] IncompatibleMods { get; set; } = { };

        /// <inheritdoc />
        /// <summary>
        ///     All the gameplayModifier logic should go here, setting unique variables. NEVER call this directly. Always use
        ///     ModManager.AddMod();
        /// </summary>
        public void InitializeMod() {}
    }
}
