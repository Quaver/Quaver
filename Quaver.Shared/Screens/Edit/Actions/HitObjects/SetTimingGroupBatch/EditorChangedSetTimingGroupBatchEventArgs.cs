using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.SetTimingGroupBatch
{
    public class EditorChangedSetTimingGroupBatchEventArgs : EventArgs
    {
        public List<HitObjectInfo> HitObjects { get; }

        public string TimingGroupId { get; }

        public EditorChangedSetTimingGroupBatchEventArgs(List<HitObjectInfo> hitObjectInfos, string timingGroupId)
        {
            HitObjects = hitObjectInfos;
            TimingGroupId = timingGroupId;
        }
    }
}