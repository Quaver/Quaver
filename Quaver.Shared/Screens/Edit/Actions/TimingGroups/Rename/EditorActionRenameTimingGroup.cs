using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.MoveObjectsToTimingGroup;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.Remove;

namespace Quaver.Shared.Screens.Edit.Actions.TimingGroups.Rename
{
    [MoonSharpUserData]
    public class EditorActionRenameTimingGroup : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.RenameTimingGroup;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        public string NewId { get; }
        public string OldId { get; }

        public TimingGroup TimingGroup { get; private set; }

        public List<HitObjectInfo> ChildHitObjects { get; private set; }

        [MoonSharpVisible(false)]
        public EditorActionRenameTimingGroup(EditorActionManager manager, Qua workingMap, string oldId, string newId,
            List<HitObjectInfo> childHitObjects)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            OldId = oldId;
            NewId = newId;
            ChildHitObjects = childHitObjects;
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            TimingGroup = WorkingMap.TimingGroups[OldId];
            WorkingMap.TimingGroups.Remove(OldId);
            WorkingMap.TimingGroups.Add(NewId, TimingGroup);

            if (ChildHitObjects == null)
            {
                ChildHitObjects = new List<HitObjectInfo>();
                foreach (var hitObject in WorkingMap.HitObjects)
                {
                    if (hitObject.TimingGroup != OldId)
                        continue;

                    ChildHitObjects.Add(hitObject);
                }
            }

            new EditorActionMoveObjectsToTimingGroup(ActionManager, WorkingMap, ChildHitObjects, NewId).Perform();
            ActionManager.TriggerEvent(Type, new EditorTimingGroupRenamedEventArgs(OldId, NewId, ChildHitObjects));
        }

        [MoonSharpVisible(false)]
        public void Undo() =>
            new EditorActionRenameTimingGroup(ActionManager, WorkingMap, NewId, OldId, ChildHitObjects).Perform();
    }
}