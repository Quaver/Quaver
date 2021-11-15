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
    public class ModNoLongNotes : IGameplayModifier
    {
        public string Name { get; set; } = "No Long Notes";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.NoLongNotes;

        public ModType Type { get; set; } = ModType.Special;

        public string Description { get; set; } = "I have a variety of taste preferences, but noodles aren't included.";

        public bool Ranked() => true;

        public bool AllowedInMultiplayer { get; set; } = true;

        public bool OnlyMultiplayerHostCanCanChange { get; set; } = true;

        public bool ChangesMapObjects { get; set; } = true;

        public ModIdentifier[] IncompatibleMods { get; set; } = { ModIdentifier.Inverse, ModIdentifier.FullLN };

        public Color ModColor { get; } = ColorHelper.HexToColor("#F2994A");

        public void InitializeMod() {}
    }
}
