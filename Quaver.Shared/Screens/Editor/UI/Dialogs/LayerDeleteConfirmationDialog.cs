using System;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Graphics.Dialogs;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs
{
    public class LayerDeleteConfirmationDialog : ConfirmCancelDialog
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="ruleset"></param>
        /// <param name="layer"></param>
        public LayerDeleteConfirmationDialog(EditorRulesetKeys ruleset, EditorLayerInfo layer)
            : base("Deleting this layer will remove all objects. Confirm?", (o, e) => OnConfirm(ruleset, layer))
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="ruleset"></param>
        /// <param name="layer"></param>
        private static void OnConfirm(EditorRulesetKeys ruleset, EditorLayerInfo layer)
        {
            Console.WriteLine("Should remove all objects in the layer.");
        }
    }
}