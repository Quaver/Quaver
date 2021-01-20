using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Timing.AddBatch;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.RemoveBatch
{
    [MoonSharpUserData]
    public class EditorActionRemoveTimingPointBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.RemoveTimingPointBatch;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private List<TimingPointInfo> TimingPoints { get; }

        [MoonSharpVisible(false)]
        public EditorActionRemoveTimingPointBatch(EditorActionManager manager, Qua workingMap, List<TimingPointInfo> tps)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            TimingPoints = tps;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            foreach (var tp in TimingPoints)
                WorkingMap.TimingPoints.Remove(tp);

            ActionManager.TriggerEvent(Type, new EditorTimingPointBatchRemovedEventArgs(TimingPoints));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionAddTimingPointBatch(ActionManager, WorkingMap, TimingPoints).Perform();
    }
}
