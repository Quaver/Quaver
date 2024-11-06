using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.TimingGroups.Rename
{
    public class EditorTimingGroupRenamedEventArgs : EventArgs
    {
        public EditorTimingGroupRenamedEventArgs(string oldId, string newId, List<HitObjectInfo> childHitObjects)
        {
            OldId = oldId;
            NewId = newId;
            ChildHitObjects = childHitObjects;
        }

        public string OldId { get; }
        public string NewId { get; }

        public List<HitObjectInfo> ChildHitObjects { get; }
    }
}