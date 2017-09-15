// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
