using System;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.Place
{
    public class EditorHitObjectPlacedEventArgs : EventArgs
    {
        /// <summary>
        ///     The hitobject that was placed by the user
        /// </summary>
        public HitObjectInfo HitObject { get; }

        /// <summary>
        /// </summary>
        /// <param name="h"></param>
        public EditorHitObjectPlacedEventArgs(HitObjectInfo h) => HitObject = h;
    }
}