using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.SV.Add;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.SV.Remove
{
    [MoonSharpUserData]
    public class EditorActionRemoveScrollVelocity : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.RemoveScrollVelocity;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private SliderVelocityInfo ScrollVelocity { get; }

        [MoonSharpVisible(false)]
        public EditorActionRemoveScrollVelocity(EditorActionManager manager, Qua workingMap, SliderVelocityInfo sv)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollVelocity = sv;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            WorkingMap.SliderVelocities.Remove(ScrollVelocity);
            ActionManager.TriggerEvent(Type, new EditorScrollVelocityRemovedEventArgs(ScrollVelocity));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionAddScrollVelocity(ActionManager, WorkingMap, ScrollVelocity).Perform();
    }
}
