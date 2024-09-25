using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.Layers.Colors;
using Quaver.Shared.Screens.Edit.Dialogs;

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
            "CHANGE LAYER COLOR",
            "Enter a new RGB color for your layer...")
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