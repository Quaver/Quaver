using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.SV.Remove;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Helpers;

namespace Quaver.Shared.Screens.Edit.Actions.SV.Add
{
    [MoonSharpUserData]
    public class EditorActionAddScrollVelocity : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.AddScrollVelocity;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        public ScrollGroup ScrollGroup { get; }

        public SliderVelocityInfo ScrollVelocity { get; }

        [MoonSharpVisible(false)]
        public EditorActionAddScrollVelocity(EditorActionManager manager, Qua workingMap, SliderVelocityInfo sv, ScrollGroup scrollGroup)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollVelocity = sv;
            ScrollGroup = scrollGroup ?? manager.EditScreen.SelectedScrollGroup;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            ScrollGroup.ScrollVelocities.InsertSorted(ScrollVelocity);
            ActionManager.TriggerEvent(Type, new EditorScrollVelocityAddedEventArgs(ScrollVelocity, ScrollGroup));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionRemoveScrollVelocity(ActionManager, WorkingMap, ScrollVelocity, ScrollGroup).Perform();
    }
}
