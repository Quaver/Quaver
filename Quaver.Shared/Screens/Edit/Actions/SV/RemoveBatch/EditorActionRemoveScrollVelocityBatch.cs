using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.SV.AddBatch;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.SV.RemoveBatch
{
    [MoonSharpUserData]
    public class EditorActionRemoveScrollVelocityBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.RemoveScrollVelocityBatch;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        public List<SliderVelocityInfo> ScrollVelocities { get; }

        public ScrollGroup ScrollGroup { get; }

        [MoonSharpVisible(false)]
        public EditorActionRemoveScrollVelocityBatch(EditorActionManager manager, Qua workingMap, List<SliderVelocityInfo> svs, ScrollGroup scrollGroup)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollVelocities = svs;
            ScrollGroup = scrollGroup;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            var svs = ScrollGroup?.ScrollVelocities ?? WorkingMap.SliderVelocities;

            foreach (var sv in ScrollVelocities)
                svs.Remove(sv);

            ActionManager.TriggerEvent(Type, new EditorScrollVelocityBatchRemovedEventArgs(ScrollVelocities, ScrollGroup));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionAddScrollVelocityBatch(ActionManager, WorkingMap, ScrollVelocities, ScrollGroup).Perform();
    }
}
