using System;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.Remove
{
    public class EditorHitObjectRemovedEventArgs : EventArgs
    {
        public HitObjectInfo HitObject { get; }

        public EditorHitObjectRemovedEventArgs(HitObjectInfo h) => HitObject = h;
    }
}