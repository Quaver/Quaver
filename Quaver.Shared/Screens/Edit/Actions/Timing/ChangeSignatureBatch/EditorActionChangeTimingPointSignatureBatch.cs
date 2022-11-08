using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Enums;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.ChangeSignatureBatch
{
    [MoonSharpUserData]
    public class EditorActionChangeTimingPointSignatureBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ChangeTimingPointSignatureBatch;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private List<TimingPointInfo> TimingPoints { get; }

        private List<int> OriginalSignatures { get; } = new List<int>();

        private int NewSignature { get; }

        [MoonSharpVisible(false)]
        public EditorActionChangeTimingPointSignatureBatch(EditorActionManager manager, Qua workingMap, List<TimingPointInfo> tps,
            int newSignature)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            TimingPoints = tps;

            TimingPoints.ForEach(x => OriginalSignatures.Add((int)x.Signature));

            NewSignature = newSignature;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            TimingPoints.ForEach(x => x.Signature = (TimeSignature)NewSignature);
            ActionManager.TriggerEvent(EditorActionType.ChangeTimingPointSignatureBatch, new EditorChangedTimingPointSignatureBatchEventArgs(TimingPoints));
        }

        [MoonSharpVisible(false)]
        public void Undo()
        {
            for (var i = 0; i < TimingPoints.Count; i++)
                TimingPoints[i].Signature = (TimeSignature)OriginalSignatures[i];

            ActionManager.TriggerEvent(EditorActionType.ChangeTimingPointSignatureBatch, new EditorChangedTimingPointSignatureBatchEventArgs(TimingPoints));
        }
    }
}