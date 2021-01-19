using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Helpers;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.Colors
{
    [MoonSharpUserData]
    public class EditorActionChangeLayerColor : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ColorLayer;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private EditorLayerInfo Layer { get; }

        private Color NewColor { get; }

        private Color OriginalColor { get; }

        [MoonSharpVisible(false)]
        public EditorActionChangeLayerColor(EditorActionManager manager, Qua workingMap, EditorLayerInfo layer, Color color)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            Layer = layer;

            NewColor = color;
            OriginalColor = ColorHelper.ToXnaColor(Layer.GetColor());
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            Layer.ColorRgb = $"{NewColor.R},{NewColor.G},{NewColor.B}";
            ActionManager.TriggerEvent(Type, new EditorLayerColorChangedEventArgs(Layer, OriginalColor, NewColor));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionChangeLayerColor(ActionManager, WorkingMap, Layer, OriginalColor).Perform();
    }
}
