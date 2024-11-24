using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.SF.ChangeMultiplierBatch
{
    [MoonSharpUserData]
    public class EditorActionChangeScrollSpeedFactorMultiplierBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ChangeScrollSpeedFactorMultiplierBatch;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private List<ScrollSpeedFactorInfo> ScrollSpeedFactors { get; }

        private List<float> OriginalMultipliers { get; } = new List<float>();

        private float Multiplier { get; }

        [MoonSharpVisible(false)]
        public EditorActionChangeScrollSpeedFactorMultiplierBatch(EditorActionManager manager, Qua workingMap, List<ScrollSpeedFactorInfo> SFs,
            float multiplier)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollSpeedFactors = SFs;
            Multiplier = multiplier;

            ScrollSpeedFactors.ForEach(x => OriginalMultipliers.Add(x.Multiplier));
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            ScrollSpeedFactors.ForEach(x => x.Multiplier = Multiplier);
            ActionManager.TriggerEvent(Type, new EditorChangedScrollSpeedFactorMultiplierBatchEventArgs(ScrollSpeedFactors, Multiplier));
        }

        [MoonSharpVisible(false)]
        public void Undo()
        {
            for (var i = 0; i < ScrollSpeedFactors.Count; i++)
                ScrollSpeedFactors[i].Multiplier = OriginalMultipliers[i];

            ActionManager.TriggerEvent(Type, new EditorChangedScrollSpeedFactorMultiplierBatchEventArgs(ScrollSpeedFactors, Multiplier));
        }
    }
}
