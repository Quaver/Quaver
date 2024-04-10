using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.Swap
{
    public class EditorLanesSwappedEventArgs : EventArgs
    {
        public List<HitObjectInfo> HitObjects { get; }

        private int SwapLane1 { get; }
        private int SwapLane2 { get; }

        public EditorLanesSwappedEventArgs(List<HitObjectInfo> hitObjects, int swapLane1, int swapLane2)
        {
            HitObjects = hitObjects;
            SwapLane1 = swapLane1;
            SwapLane2 = swapLane2;
        }
    }
}