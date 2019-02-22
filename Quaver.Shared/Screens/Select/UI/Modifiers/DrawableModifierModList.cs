/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Linq;
using Quaver.Shared.Modifiers;

namespace Quaver.Shared.Screens.Select.UI.Modifiers
{
    public sealed class DrawableModifierModList : DrawableModifier
    {
        /// <summary>
        ///     Modifiers in this option list.
        /// </summary>
        private IGameplayModifier[] Modifiers { get; }

        /// <summary>
        ///     Description for when the OFF button is selected.
        /// </summary>
        private string DefaultDescription { get; }

        public DrawableModifierModList(ModifiersDialog dialog, IGameplayModifier[] modifiers, string name, string defaultDescription)
            : base(dialog, name, defaultDescription, modifiers.All(x => x.Ranked))
        {
            Modifiers = modifiers;
            DefaultDescription = defaultDescription;
            UsePreviousSpriteBatchOptions = true;

            CreateModsDialogOptions();
            AlignOptions();
            ChangeSelectedOptionButton();
        }

        /// <summary>
        ///     Creates the option buttons.
        /// </summary>
        public void CreateModsDialogOptions()
        {
            Options.Add(new DrawableModifierOption(this, "OFF", (o, e) =>
            {
                foreach (var mod in Modifiers)
                {
                    if (ModManager.Mods.HasFlag(mod.ModIdentifier))
                        ModManager.RemoveMod(mod.ModIdentifier);
                }

                Description = DefaultDescription;
            }));

            foreach (var mod in Modifiers)
            {
                Options.Add(new DrawableModifierOption(this, mod.Name, (o, e) =>
                {
                    if (!ModManager.Mods.HasFlag(mod.ModIdentifier))
                        ModManager.AddMod(mod.ModIdentifier);

                    Description = mod.Description;
                }));
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void ChangeSelectedOptionButton()
        {
            foreach (var option in Options)
                option.Deselect();

            for (var i = 0; i < Modifiers.Length; i++)
            {
                if (ModManager.Mods.HasFlag(Modifiers[i].ModIdentifier))
                {
                    Options[i + 1].Select();
                    return;
                }
            }

            Options[0].Select();
        }
    }
}
