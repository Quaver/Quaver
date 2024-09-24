using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.MoveObjectsToTimingGroup;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.Remove;

namespace Quaver.Shared.Screens.Edit.Actions.TimingGroups.Create
{
    [MoonSharpUserData]
    public class EditorActionCreateTimingGroup : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.CreateTimingGroup;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        public string Id { get; }

        public TimingGroup TimingGroup { get; }

        public List<HitObjectInfo> ChildHitObjects { get; }

        private EditorActionMoveObjectsToTimingGroup MoveObjectsToTimingGroup { get; set; }

        [MoonSharpVisible(false)]
        public EditorActionCreateTimingGroup(EditorActionManager manager, Qua workingMap, string id,
            TimingGroup timingGroup, List<HitObjectInfo> childHitObjects)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            TimingGroup = timingGroup;
            ChildHitObjects = childHitObjects;
            Id = id;
            MoveObjectsToTimingGroup = new EditorActionMoveObjectsToTimingGroup(ActionManager, WorkingMap, ChildHitObjects, Id);
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            WorkingMap.TimingGroups.Add(Id, TimingGroup);
            ActionManager.TriggerEvent(Type, new EditorTimingGroupCreatedEventArgs(Id, TimingGroup, ChildHitObjects));
            MoveObjectsToTimingGroup.Perform();
        }

        [MoonSharpVisible(false)]
        public void Undo()
        {
            new EditorActionRemoveTimingGroup(ActionManager, WorkingMap, Id, TimingGroup, ChildHitObjects).Perform();
            MoveObjectsToTimingGroup.Undo();
        }
    }
}