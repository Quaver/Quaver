// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Osu.Beatmap
{
    public struct TimingPoint
    {
        public float Offset;
        public float MillisecondsPerBeat;
        public int Meter;
        public int SampleType;
        public int SampleSet;
        public int Volume;
        public int Inherited;
        public int KiaiMode;
    }
}
