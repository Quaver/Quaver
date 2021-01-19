using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.SV.ChangeMultiplierBatch
{
    [MoonSharpUserData]
    public class EditorActionChangeScrollVelocityMultiplierBatch : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ChangeScrollVelocityMultiplierBatch;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private List<SliderVelocityInfo> ScrollVelocities { get; }

        private List<float> OriginalMultipliers { get; } = new List<float>();

        private float Multiplier { get; }

        [MoonSharpVisible(false)]
        public EditorActionChangeScrollVelocityMultiplierBatch(EditorActionManager manager, Qua workingMap, List<SliderVelocityInfo> svs,
            float multiplier)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            ScrollVelocities = svs;
            Multiplier = multiplier;

            ScrollVelocities.ForEach(x => OriginalMultipliers.Add(x.Multiplier));
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            ScrollVelocities.ForEach(x => x.Multiplier = Multiplier);
            ActionManager.TriggerEvent(Type, new EditorChangedScrollVelocityMultiplierBatchEventArgs(ScrollVelocities, Multiplier));
        }

        [MoonSharpVisible(false)]
        public void Undo()
        {
            for (var i = 0; i < ScrollVelocities.Count; i++)
                ScrollVelocities[i].Multiplier = OriginalMultipliers[i];

            ActionManager.TriggerEvent(Type, new EditorChangedScrollVelocityMultiplierBatchEventArgs(ScrollVelocities, Multiplier));
        }
    }
}
