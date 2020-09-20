using System;
using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.ChangeBpmBatch
{
    [MoonSharpUserData]
    public class EditorActionChangeTimingPointBpmBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ChangeTimingPointBpmBatch;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private List<TimingPointInfo> TimingPoints { get; }

        private List<float> OriginalBpms { get; } = new List<float>();

        private float NewBpm { get; }

        [MoonSharpVisible(false)]
        public EditorActionChangeTimingPointBpmBatch(EditorActionManager manager, Qua workingMap, List<TimingPointInfo> tps,
            float newBpm)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            TimingPoints = tps;

            TimingPoints.ForEach(x => OriginalBpms.Add(x.Bpm));

            NewBpm = newBpm;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            TimingPoints.ForEach(x => x.Bpm = NewBpm);
            ActionManager.TriggerEvent(EditorActionType.ChangeTimingPointBpmBatch, new EditorChangedTimingPointBpmBatchEventArgs(TimingPoints));
        }

        [MoonSharpVisible(false)]
        public void Undo()
        {
            for (var i = 0; i < TimingPoints.Count; i++)
                TimingPoints[i].Bpm = OriginalBpms[i];

            ActionManager.TriggerEvent(EditorActionType.ChangeTimingPointBpmBatch, new EditorChangedTimingPointBpmBatchEventArgs(TimingPoints));
        }
    }
}
