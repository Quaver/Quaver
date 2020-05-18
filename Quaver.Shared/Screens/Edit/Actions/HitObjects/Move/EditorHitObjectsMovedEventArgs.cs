using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.Move
{
    public class EditorHitObjectsMovedEventArgs : EventArgs
    {
        public List<HitObjectInfo> HitObjects { get; }

        public EditorHitObjectsMovedEventArgs(List<HitObjectInfo> objects) => HitObjects = objects;
    }
}