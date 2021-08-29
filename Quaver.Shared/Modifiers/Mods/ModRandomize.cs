/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Modifiers.Mods
{
    /// <summary>
    ///     Randomize gameplayModifier. Randomizes the lanes.
    /// </summary>
    internal class ModRandomize : IGameplayModifier
    {
        /// <summary>
        ///     Integer based seed for use with the Shuffle(Random) function.
        ///
        ///     Used in shuffling the order of the playfields lanes.
        /// </summary>
        public int Seed;

        public string Name { get; set; } = "Randomize";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Randomize;

        public ModType Type { get; set; } = ModType.Special;

        public string Description { get; set; } = "Swap up the lanes.";

        public bool Ranked() => false;

        public bool AllowedInMultiplayer { get; set; } = false;

        public bool OnlyMultiplayerHostCanCanChange { get; set; }

        public bool ChangesMapObjects { get; set; }

        public ModIdentifier[] IncompatibleMods { get; set; } = { };

        public Color ModColor { get; } = ColorHelper.HexToColor("#27AE60");

        public void InitializeMod()
        {
        }

        public void GenerateSeed() => Seed = new Random().Next(int.MaxValue);
    }
}
