using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.SV.RemoveBatch;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Helpers;

namespace Quaver.Shared.Screens.Edit.Actions.SV.AddBatch
{
    [MoonSharpUserData]
    public class EditorActionAddScrollVelocityBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.AddScrollVelocityBatch;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        public List<SliderVelocityInfo> ScrollVelocities { get; }

        public ScrollGroup ScrollGroup { get; }

        [MoonSharpVisible(false)]
        public EditorActionAddScrollVelocityBatch(EditorActionManager manager, Qua workingMap, List<SliderVelocityInfo> svs, ScrollGroup scrollGroup)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollVelocities = svs;
            ScrollGroup = scrollGroup ?? manager.EditScreen.SelectedScrollGroup;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            ScrollGroup.ScrollVelocities.InsertSorted(ScrollVelocities);
            ActionManager.TriggerEvent(Type, new EditorScrollVelocityBatchAddedEventArgs(ScrollVelocities, ScrollGroup));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionRemoveScrollVelocityBatch(ActionManager, WorkingMap, ScrollVelocities, ScrollGroup).Perform();
    }
}
