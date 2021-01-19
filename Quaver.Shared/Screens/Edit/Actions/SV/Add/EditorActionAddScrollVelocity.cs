using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.SV.Remove;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.SV.Add
{
    [MoonSharpUserData]
    public class EditorActionAddScrollVelocity : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.AddScrollVelocity;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private SliderVelocityInfo ScrollVelocity { get; }

        [MoonSharpVisible(false)]
        public EditorActionAddScrollVelocity(EditorActionManager manager, Qua workingMap, SliderVelocityInfo sv)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollVelocity = sv;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            WorkingMap.SliderVelocities.Add(ScrollVelocity);
            WorkingMap.Sort();

            ActionManager.TriggerEvent(Type, new EditorScrollVelocityAddedEventArgs(ScrollVelocity));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionRemoveScrollVelocity(ActionManager, WorkingMap, ScrollVelocity).Perform();
    }
}
