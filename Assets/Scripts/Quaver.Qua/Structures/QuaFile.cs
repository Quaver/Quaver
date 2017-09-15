// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Qua
{
    public struct QuaFile
    {
        // # General
        public bool IsValidQua;
        public string AudioFile;
        public int AudioLeadIn;
        public int SongPreviewTime;
        public string BackgroundFile;

        // # Metadata
        public string Title;
        public string TitleUnicode;
        public string Artist;
        public string ArtistUnicode;
        public string Source;
        public string Tags;
        public string Creator;
        public string DifficultyName;
        public int MapID;
        public int MapSetID;
        public string Description;

        // Difficulty
        public float HPDrain;
        public float AccuracyStrain;

        // Timing
        public List<TimingPoint> TimingPoints;

        // SV
        public List<SliderVelocity> SliderVelocities;

        // HitObjects
        public List<HitObject> HitObjects;
    }
}

