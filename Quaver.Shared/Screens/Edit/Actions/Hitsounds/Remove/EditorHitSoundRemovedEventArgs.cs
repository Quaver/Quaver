using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Hitsounds.Add;

namespace Quaver.Shared.Screens.Edit.Actions.Hitsounds.Remove
{
    public class EditorHitSoundRemovedEventArgs : EditorHitsoundAddedEventArgs
    {
        public EditorHitSoundRemovedEventArgs(List<HitObjectInfo> hitObjects, HitSounds sound) : base(hitObjects, sound)
        {
        }
    }
}