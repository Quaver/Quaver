using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.TimingGroups.MoveObjectsToTimingGroup
{
    public class EditorMoveObjectsToTimingGroupEventArgs : EventArgs
    {
        public List<HitObjectInfo> HitObjects { get; }

        public string TimingGroupId { get; }

        public EditorMoveObjectsToTimingGroupEventArgs(List<HitObjectInfo> hitObjectInfos, string timingGroupId)
        {
            HitObjects = hitObjectInfos;
            TimingGroupId = timingGroupId;
        }
    }
}