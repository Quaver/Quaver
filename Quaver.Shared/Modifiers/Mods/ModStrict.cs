/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using Quaver.API.Enums;

namespace Quaver.Shared.Modifiers.Mods
{
    /// <summary>
    ///     Strict Mod. Makes the hit timing windows harder during gameplay.
    /// </summary>
    internal class ModStrict : IGameplayModifier
    {
        /// <inheritdoc />
        /// <summary>
        ///     Name
        /// </summary>
        public string Name { get; set; } = "Strict";

        /// <inheritdoc />
        /// <summary>
        ///     ID
        /// </summary>
        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Strict;

        /// <inheritdoc />
        /// <summary>
        ///     ResultsScreenType
        /// </summary>
        public ModType Type { get; set; } = ModType.DifficultyIncrease;

        /// <inheritdoc />
        /// <summary>
        ///     Desc
        /// </summary>
        public string Description { get; set; } = "You'll need to be super accurate.";

        /// <inheritdoc />
        /// <summary>
        ///     Ranked?
        /// </summary>
        public bool Ranked { get; set; } = true;


        /// <inheritdoc />
        /// <summary>
        ///     Incompatible Mods
        /// </summary>
        public ModIdentifier[] IncompatibleMods { get; set; } = { ModIdentifier.Chill };

        /// <summary>
        ///     Initialize
        /// </summary>
        public void InitializeMod()
        {
        }
    }
}
