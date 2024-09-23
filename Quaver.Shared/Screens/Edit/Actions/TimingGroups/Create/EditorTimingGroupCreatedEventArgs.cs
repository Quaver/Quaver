using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.TimingGroups.Create
{
    public class EditorTimingGroupCreatedEventArgs : EventArgs
    {
        public string Id { get; }
        public TimingGroup TimingGroup { get; }
        public List<HitObjectInfo> ChildHitObjects { get; }

        public EditorTimingGroupCreatedEventArgs(string id, TimingGroup timingGroup, List<HitObjectInfo> childHitObjects)
        {
            Id = id;
            TimingGroup = timingGroup;
            ChildHitObjects = childHitObjects;
        }
    }
}