using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.RemoveBatch
{
    public class EditorHitObjectBatchRemovedEventArgs : EventArgs
    {
        /// <summary>
        /// </summary>
        public List<HitObjectInfo> HitObjects { get; }

        public EditorHitObjectBatchRemovedEventArgs(List<HitObjectInfo> hitObjects) => HitObjects = hitObjects;
    }
}