using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.SetTimingGroupBatch;
using Quaver.Shared.Screens.Edit.Actions.SV.Remove;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.Remove;

namespace Quaver.Shared.Screens.Edit.Actions.TimingGroups.Add
{
    [MoonSharpUserData]
    public class EditorActionAddTimingGroup : IEditorAction
    {
        public EditorActionType Type { get; } = EditorActionType.AddTimingGroup;

        private EditorActionManager ActionManager { get; }

        private Qua WorkingMap { get; }

        public string Id { get; }

        public TimingGroup TimingGroup { get; }

        public List<HitObjectInfo> ChildHitObjects { get; }

        private EditorActionSetTimingGroupBatch SetTimingGroupBatch { get; set; }

        [MoonSharpVisible(false)]
        public EditorActionAddTimingGroup(EditorActionManager manager, Qua workingMap, string id,
            TimingGroup timingGroup, List<HitObjectInfo> childHitObjects)
        {
            ActionManager = manager;
            WorkingMap = workingMap;
            TimingGroup = timingGroup;
            ChildHitObjects = childHitObjects;
            Id = id;
            SetTimingGroupBatch = new EditorActionSetTimingGroupBatch(ActionManager, WorkingMap, ChildHitObjects, Id);
        }

        [MoonSharpVisible(false)]
        public void Perform()
        {
            WorkingMap.TimingGroups.Add(Id, TimingGroup);
            ActionManager.TriggerEvent(Type, new EditorTimingGroupAddedEventArgs(Id, TimingGroup, ChildHitObjects));
            SetTimingGroupBatch.Perform();
        }

        [MoonSharpVisible(false)]
        public void Undo()
        {
            new EditorActionRemoveTimingGroup(ActionManager, WorkingMap, Id, TimingGroup, ChildHitObjects).Perform();
            SetTimingGroupBatch.Undo();
        }
    }
}