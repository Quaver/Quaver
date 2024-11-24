using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.TimingGroups.MoveObjectsToTimingGroup
{
    [MoonSharpUserData]
    public class EditorActionMoveObjectsToTimingGroup : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.MoveObjectsToTimingGroup;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        public List<HitObjectInfo> HitObjects { get; }

        public List<string> OriginalTimingGroupIds { get; } = new();

        public string TimingGroupId { get; }

        [MoonSharpVisible(false)]
        public EditorActionMoveObjectsToTimingGroup(EditorActionManager manager, Qua workingMap, List<HitObjectInfo> hitObjectInfos,
            string timingGroupId)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            HitObjects = new List<HitObjectInfo>(hitObjectInfos);
            TimingGroupId = timingGroupId;

            HitObjects.ForEach(x => OriginalTimingGroupIds.Add(x.TimingGroup));
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            HitObjects.ForEach(x => x.TimingGroup = TimingGroupId);
            ActionManager.TriggerEvent(Type, new EditorMoveObjectsToTimingGroupEventArgs(HitObjects, TimingGroupId));
        }

        [MoonSharpVisible(false)]
        public void Undo()
        {
            for (var i = 0; i < HitObjects.Count; i++)
                HitObjects[i].TimingGroup = OriginalTimingGroupIds[i];

            ActionManager.TriggerEvent(Type, new EditorMoveObjectsToTimingGroupEventArgs(HitObjects, TimingGroupId));
        }
    }
}
