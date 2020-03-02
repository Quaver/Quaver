using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.Hitsounds.Add
{
    public class EditorHitsoundAddedEventArgs : EventArgs
    {
        public List<HitObjectInfo> HitObjects { get; }

        public HitSounds Sound { get; }

        public EditorHitsoundAddedEventArgs(List<HitObjectInfo> hitObjects, HitSounds sound)
        {
            HitObjects = hitObjects;
            Sound = sound;
        }
    }
}