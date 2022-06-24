/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Modifiers.Mods
{
    public class ModAutoplay : IGameplayModifier
    {
        public string Name { get; set; } = "Autoplay";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Autoplay;

        public ModType Type { get; set; } = ModType.Special;

        public string Description { get; set; } = "Take a break, and watch something magical.";

        public bool Ranked() => false;

        public bool AllowedInMultiplayer { get; set; } = false;

        public bool OnlyMultiplayerHostCanCanChange { get; set; }

        public bool ChangesMapObjects { get; set; }

        public ModIdentifier[] IncompatibleMods { get; set; } =
        {
            ModIdentifier.NoFail,
            ModIdentifier.NoMiss
        };

        public Color ModColor { get; } = ColorHelper.HexToColor("#2D9CDB");

        public void InitializeMod()
        {
        }
    }
}
