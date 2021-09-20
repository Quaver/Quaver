/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Quaver.API.Enums;

namespace Quaver.Shared.Modifiers.Mods
{
    public class ModPaused : IGameplayModifier
    {
        public string Name { get; set; } = "Paused";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Paused;

        public ModType Type { get; set; } = ModType.Special;

        public string Description { get; set; } = "Player paused in gameplay";

        public bool Ranked() => false;

        public bool AllowedInMultiplayer { get; set; } = true;

        public bool OnlyMultiplayerHostCanCanChange { get; set; }

        public bool ChangesMapObjects { get; set; }

        public ModIdentifier[] IncompatibleMods { get; set; } = {};

        public Color ModColor { get; }

        public void InitializeMod() {}
    }
}
