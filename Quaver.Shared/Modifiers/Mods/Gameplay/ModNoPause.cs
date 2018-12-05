/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using Quaver.API.Enums;

namespace Quaver.Shared.Modifiers.Mods.Gameplay
{
    internal class ManiaModNoPause : IGameplayModifier
    {
        public string Name { get; set; } = "No Pause";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.NoPause;

        public ModType Type { get; set; } = ModType.Special;

        public string Description { get; set; } = "A wise man once said - Pausing is Cheating™";

        public bool Ranked { get; set; } = true;

        public float ScoreMultiplierAddition { get; set; } = 1.0f;

        public ModIdentifier[] IncompatibleMods { get; set; } =
        {
            ModIdentifier.Autoplay,
            ModIdentifier.Paused
        };

        public void InitializeMod()
        {
        }
    }
}
