using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Timing.ChangeSignature
{
    [MoonSharpUserData]
    public class EditorActionChangeTimingPointSignature : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ChangeTimingPointSignature;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        private TimingPointInfo TimingPoint { get; }

        private int OriginalSignature { get; }

        private int NewSignature { get; }

        [MoonSharpVisible(false)]
        public EditorActionChangeTimingPointSignature(EditorActionManager manager, Qua workingMap, TimingPointInfo tp, int newSignature)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            TimingPoint = tp;

            OriginalSignature = (int)tp.Signature;
            NewSignature = newSignature;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            TimingPoint.Signature = (TimeSignature)NewSignature;
            ActionManager.TriggerEvent(Type, new EditorTimingPointSignatureChangedEventArgs(OriginalSignature, NewSignature));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionChangeTimingPointSignature(ActionManager, WorkingMap, TimingPoint, OriginalSignature).Perform();
    }
}