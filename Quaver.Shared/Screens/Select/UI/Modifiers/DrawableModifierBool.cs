/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Quaver.Shared.Modifiers;

namespace Quaver.Shared.Screens.Select.UI.Modifiers
{
    public class DrawableModifierBool : DrawableModifier
    {
        public DrawableModifierBool(ModifiersDialog dialog, IGameplayModifier modifier)
            : base(dialog, modifier) => UsePreviousSpriteBatchOptions = true;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override List<DrawableModifierOption> CreateModsDialogOptions()
        {
            var options = new List<DrawableModifierOption>
            {
                new DrawableModifierOption(this, "OFF", (o, e) =>
                {
                    if (ModManager.Mods.HasFlag(Modifier.ModIdentifier))
                        ModManager.RemoveMod(Modifier.ModIdentifier);
                }),
                new DrawableModifierOption(this, "ON", (o, e) =>
                {
                    if (!ModManager.Mods.HasFlag(Modifier.ModIdentifier))
                        ModManager.AddMod(Modifier.ModIdentifier);
                })
            };
            return options;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void ChangeSelectedOptionButton()
        {
            if (ModManager.IsActivated(Modifier.ModIdentifier))
            {
                Options[0].Deselect();
                Options[1].Select();
            }
            else
            {
                Options[0].Select();
                Options[1].Deselect();
            }
        }
    }
}
