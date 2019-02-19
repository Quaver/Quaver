/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Quaver.Shared.Modifiers;

namespace Quaver.Shared.Screens.Select.UI.Modifiers
{
    public sealed class DrawableModifierBool : DrawableModifier
    {
        private IGameplayModifier Modifier { get; }

        public DrawableModifierBool(ModifiersDialog dialog, IGameplayModifier modifier)
            : base(dialog, modifier)
        {
            Modifier = modifier;

            CreateModsDialogOptions();
            AlignOptions();
            ChangeSelectedOptionButton();
        }

        /// <summary>
        ///     Creates the option buttons.
        /// </summary>
        private void CreateModsDialogOptions()
        {
            Options.Add(new DrawableModifierOption(this, "OFF", (o, e) =>
            {
                if (ModManager.Mods.HasFlag(Modifier.ModIdentifier))
                    ModManager.RemoveMod(Modifier.ModIdentifier);
            })
            {
                Width = 60,
            });
            Options.Add(new DrawableModifierOption(this, "ON", (o, e) =>
            {
                if (!ModManager.Mods.HasFlag(Modifier.ModIdentifier))
                    ModManager.AddMod(Modifier.ModIdentifier);
            })
            {
                Width = 60,
            });
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
