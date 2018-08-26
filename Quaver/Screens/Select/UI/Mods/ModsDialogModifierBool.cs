using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.Modifiers;

namespace Quaver.Screens.Select.UI.Mods
{
    public class ModsDialogModifierBool : ModsDialogModifier
    {
        public ModsDialogModifierBool(ModsDialog dialog, IGameplayModifier modifier)
            : base(dialog, modifier)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override List<ModsDialogOption> CreateModsDialogOptions()
        {
            var options = new List<ModsDialogOption>
            {
                new ModsDialogOption(this, "OFF", (o, e) => ModManager.RemoveMod(Modifier.ModIdentifier)),
                new ModsDialogOption(this, "ON", (o, e) => ModManager.AddMod(Modifier.ModIdentifier))
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