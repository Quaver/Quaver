using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.Colors
{
    public class EditorActionChangeLayerColor : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ColorLayer;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private EditorLayerInfo Layer { get; }

        private Color NewColor { get; }

        private Color OriginalColor { get; }

        public EditorActionChangeLayerColor(EditorActionManager manager, Qua workingMap, EditorLayerInfo layer, Color color)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            Layer = layer;

            NewColor = color;
            OriginalColor = ColorHelper.ToXnaColor(Layer.GetColor());
        }

        public void Perform()
        {
            Layer.ColorRgb = $"{NewColor.R},{NewColor.G},{NewColor.B}";
            ActionManager.TriggerEvent(Type, new  EditorLayerColorChangedEventArgs(Layer, OriginalColor, NewColor));
        }

        public void Undo() => new EditorActionChangeLayerColor(ActionManager, WorkingMap, Layer, OriginalColor).Perform();
    }
}