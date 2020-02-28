using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.PlaceBatch
{
    public class EditorHitObjectBatchPlacedEventArgs : EventArgs
    {
        public List<HitObjectInfo> HitObjects { get; }

        public EditorHitObjectBatchPlacedEventArgs(List<HitObjectInfo> hitObjects) => HitObjects = hitObjects;
    }
}