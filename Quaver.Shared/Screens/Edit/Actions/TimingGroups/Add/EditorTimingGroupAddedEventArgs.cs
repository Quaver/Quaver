using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.TimingGroups.Add
{
    public class EditorTimingGroupAddedEventArgs : EventArgs
    {
        public string Id { get; }
        public TimingGroup TimingGroup { get; }
        public List<HitObjectInfo> ChildHitObjects { get; }

        public EditorTimingGroupAddedEventArgs(string id, TimingGroup timingGroup, List<HitObjectInfo> childHitObjects)
        {
            Id = id;
            TimingGroup = timingGroup;
            ChildHitObjects = childHitObjects;
        }
    }
}