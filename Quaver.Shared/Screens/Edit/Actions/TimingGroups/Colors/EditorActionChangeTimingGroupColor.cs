using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Screens.Edit.Actions.TimingGroups.Colors
{
    [MoonSharpUserData]
    public class EditorActionChangeTimingGroupColor : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.ColorTimingGroup;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        public TimingGroup TimingGroup { get; }

        public Color NewColor { get; }

        public Color OriginalColor { get; }

        [MoonSharpVisible(false)]
        public EditorActionChangeTimingGroupColor(EditorActionManager manager, Qua workingMap, TimingGroup timingGroup, Color color)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            TimingGroup = timingGroup;

            NewColor = color;
            OriginalColor = ColorHelper.ToXnaColor(TimingGroup.GetColor());
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            TimingGroup.ColorRgb = $"{NewColor.R},{NewColor.G},{NewColor.B}";
            ActionManager.TriggerEvent(Type, new EditorTimingGroupColorChangedEventArgs(TimingGroup, OriginalColor, NewColor));
        }

        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionChangeTimingGroupColor(ActionManager, WorkingMap, TimingGroup, OriginalColor).Perform();
    }
}
