using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.Resnap
{
    public class EditorActionHitObjectsResnappedEventArgs : EventArgs
    {
        /// <summary>
        /// </summary>
        public List<int> Snaps { get; }

        /// <summary>
        /// </summary>
        public List<HitObjectInfo> HitObjectsToResnap { get; }

        public EditorActionHitObjectsResnappedEventArgs(List<int> snaps, List<HitObjectInfo> hitObjectsToResnap)
        {
            Snaps = snaps;
            HitObjectsToResnap = hitObjectsToResnap;
        }
    }
}