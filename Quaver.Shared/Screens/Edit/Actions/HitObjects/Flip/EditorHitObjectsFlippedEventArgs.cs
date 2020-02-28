using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.Flip
{
    public class EditorHitObjectsFlippedEventArgs : EventArgs
    {
        public List<HitObjectInfo> HitObjects { get; }

        public EditorHitObjectsFlippedEventArgs(List<HitObjectInfo> hitObjects) => HitObjects = hitObjects;
    }
}