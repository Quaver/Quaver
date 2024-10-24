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

        public ScrollGroup ScrollGroup { get; }

        public SliderVelocityInfo ScrollVelocity { get; }

        [MoonSharpVisible(false)]
        public EditorActionRemoveScrollVelocity(EditorActionManager manager, Qua workingMap, SliderVelocityInfo sv, ScrollGroup scrollGroup = null)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollVelocity = sv;
            ScrollGroup = scrollGroup ?? manager.EditScreen.SelectedScrollGroup;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            ActionManager.TriggerEvent(Type, new EditorScrollVelocityRemovedEventArgs(ScrollVelocity, ScrollGroup));
        }

        [MoonSharpVisible(false)]
        public void Undo()
        {
            new EditorActionAddScrollVelocity(ActionManager, WorkingMap, ScrollVelocity, ScrollGroup).Perform();
        }
    }
}
