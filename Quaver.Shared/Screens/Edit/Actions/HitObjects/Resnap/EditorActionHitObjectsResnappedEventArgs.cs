using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.RemoveBatch
{
    public class EditorActionHitObjectsResnappedEventArgs : EventArgs
    {
        /// <summary>
        /// </summary>
        public List<int> Snaps { get; }

        public EditorActionHitObjectsResnappedEventArgs(List<int> snaps) => Snaps = snaps;
    }
}
