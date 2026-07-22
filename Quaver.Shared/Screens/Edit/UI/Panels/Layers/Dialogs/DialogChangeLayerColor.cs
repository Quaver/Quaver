using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.Layers.Colors;
using Quaver.Shared.Screens.Edit.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.Panels.Layers.Dialogs
{
    public class DialogChangeLayerColor : ColorDialog
    {
        private EditorActionManager ActionManager { get; }

        private EditorLayerInfo Layer { get; }

        private Qua WorkingMap { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public DialogChangeLayerColor(EditorLayerInfo layer, EditorActionManager manager, Qua workingMap) : base(
            LocalizationManager.Get("Screen_Editor_ChangeLayerColor"),
            LocalizationManager.Get("Screen_Editor_ChangeLayerColorMessage"))
        {
            ActionManager = manager;
            Layer = layer;
            WorkingMap = workingMap;
            
            UpdateColor(ColorHelper.ToXnaColor(Layer.GetColor()));
        }

        protected override void OnColorChange(Color newColor)
        {
            ActionManager.Perform(new EditorActionChangeLayerColor(ActionManager, WorkingMap, Layer, newColor));
        }
    }
}
