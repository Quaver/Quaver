// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

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

