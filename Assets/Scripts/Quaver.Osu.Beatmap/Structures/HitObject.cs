// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Osu.Beatmap
{
    public struct HitObject
    {
        public int X;
        public int Y;
        public int StartTime;
        public int Type;
        public int HitSound;
        public int EndTime;
        public string Additions;
        public bool Key1;
        public bool Key2;
        public bool Key3;
        public bool Key4;
    }
}

