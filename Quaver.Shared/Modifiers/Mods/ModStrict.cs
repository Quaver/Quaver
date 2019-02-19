/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Quaver.API.Enums;

namespace Quaver.Shared.Modifiers.Mods
{
    /// <summary>
    ///     Strict Mod. Makes the hit timing windows harder during gameplay.
    /// </summary>
    internal class ModStrict : IGameplayModifier
    {
        public string Name { get; set; } = "Strict";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Strict;

        public ModType Type { get; set; } = ModType.DifficultyIncrease;

        public string Description { get; set; } = "You'll need to be super accurate.";

        public bool Ranked { get; set; } = true;

        public ModIdentifier[] IncompatibleMods { get; set; } = { ModIdentifier.Chill };

        public void InitializeMod() => throw new NotImplementedException();
    }
}
