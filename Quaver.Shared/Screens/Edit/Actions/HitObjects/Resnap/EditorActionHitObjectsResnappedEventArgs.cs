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

        /// <summary>
        /// </summary>
        public bool ShowNotif { get; }

        /// <summary>
        /// </summary>
        /// <param name="snaps"></param>
        /// <param name="hitObjectsToResnap"></param>
        /// <param name="showNotif"></param>
        public EditorActionHitObjectsResnappedEventArgs(List<int> snaps, List<HitObjectInfo> hitObjectsToResnap, bool showNotif)
        {
            Snaps = snaps;
            HitObjectsToResnap = hitObjectsToResnap;
            ShowNotif = showNotif;
        }
    }
}