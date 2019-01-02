/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using Quaver.API.Enums;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Modifiers.Mods
{
    public class ModNoFail : IGameplayModifier
    {
        public string Name { get; set; } = "No Fail";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.NoFail;

        public ModType Type { get; set; } = ModType.Special;

        public string Description { get; set; } = "Failure is not an option.";

        public bool Ranked { get; set; } = false;

        public Sprite UnrankedSprite { get; set; }

        public ModIdentifier[] IncompatibleMods { get; set; } =
        {
            ModIdentifier.Autoplay,
        };

        public void InitializeMod() {}
    }
}
