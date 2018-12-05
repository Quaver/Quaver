/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using Quaver.API.Enums;

namespace Quaver.Shared.Modifiers.Mods.Gameplay
{
    /// <summary>
    ///     ManiaModChill gameplayModifier. Makes the hit timing windows
    /// </summary>
    internal class ManiaModChill : IGameplayModifier
    {
        /// <inheritdoc />
        /// <summary>
        ///     Name
        /// </summary>
        public string Name { get; set; } = "ModChill";

        /// <inheritdoc />
        /// <summary>
        ///     Identifier
        /// </summary>
        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Chill;

        /// <inheritdoc />
        /// <summary>
        ///     ResultsScreenType
        /// </summary>
        public ModType Type { get; set; } = ModType.DifficultyDecrease;

        /// <inheritdoc />
        /// <summary>
        ///     Desc
        /// </summary>
        public string Description { get; set; } = "Make it easier on yourself.";

        /// <inheritdoc />
        /// <summary>
        ///     If gameplayModifier is ranked
        /// </summary>
        public bool Ranked { get; set; } = true;

        /// <inheritdoc />
        /// <summary>
        ///     Score x
        /// </summary>
        public float ScoreMultiplierAddition { get; set; } = -0.5f;

        /// <inheritdoc />
        /// <summary>
        ///     Incompatible Mods
        /// </summary>
        public ModIdentifier[] IncompatibleMods { get; set; } = { ModIdentifier.Strict };

        /// <inheritdoc />
        /// <summary>
        ///     Initialize
        /// </summary>
        public void InitializeMod()
        {
        }
    }
}
