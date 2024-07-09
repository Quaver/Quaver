using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.SF.ChangeLaneMaskBatch
{
    [MoonSharpUserData]
    public class EditorActionChangeScrollSpeedFactorLaneMaskBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ChangeScrollSpeedFactorLaneMaskBatch;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private List<ScrollSpeedFactorInfo> ScrollSpeedFactors { get; }

        private List<int> OriginalLaneMasks { get; } = new List<int>();

        private int ActiveLaneMask { get; }

        private int InactiveLaneMask { get; }

        [MoonSharpVisible(false)]
        public EditorActionChangeScrollSpeedFactorLaneMaskBatch(EditorActionManager manager, Qua workingMap,
            List<ScrollSpeedFactorInfo> SFs,
            int activeLaneMask, int inactiveLaneMask)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollSpeedFactors = SFs;
            ActiveLaneMask = activeLaneMask;
            InactiveLaneMask = inactiveLaneMask;

            ScrollSpeedFactors.ForEach(x => OriginalLaneMasks.Add(x.LaneMask));
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            ScrollSpeedFactors.ForEach(x => x.LaneMask = (x.LaneMask | ActiveLaneMask) & ~InactiveLaneMask);
            ActionManager.TriggerEvent(Type,
                new EditorChangedScrollSpeedFactorLaneMaskBatchEventArgs(ScrollSpeedFactors, ActiveLaneMask,
                    InactiveLaneMask));
            WorkingMap.Sort();
        }

        [MoonSharpVisible(false)]
        public void Undo()
        {
            for (var i = 0; i < ScrollSpeedFactors.Count; i++)
                ScrollSpeedFactors[i].LaneMask = OriginalLaneMasks[i];

            ActionManager.TriggerEvent(Type,
                new EditorChangedScrollSpeedFactorLaneMaskBatchEventArgs(ScrollSpeedFactors, ActiveLaneMask,
                    InactiveLaneMask));
        }
    }
}