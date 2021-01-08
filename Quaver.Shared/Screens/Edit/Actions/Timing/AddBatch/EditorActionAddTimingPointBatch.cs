using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Timing.RemoveBatch;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.AddBatch
{
    [MoonSharpUserData]
    public class EditorActionAddTimingPointBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.AddTimingPointBatch;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private List<TimingPointInfo> TimingPoints { get; }

        [MoonSharpVisible(false)]
        public EditorActionAddTimingPointBatch(EditorActionManager manager, Qua workingMap, List<TimingPointInfo> tps)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            TimingPoints = tps;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            TimingPoints.ForEach(x => WorkingMap.TimingPoints.Add(x));
            WorkingMap.Sort();

            ActionManager.TriggerEvent(Type, new EditorTimingPointBatchAddedEventArgs(TimingPoints));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionRemoveTimingPointBatch(ActionManager, WorkingMap, TimingPoints).Perform();
    }
}
