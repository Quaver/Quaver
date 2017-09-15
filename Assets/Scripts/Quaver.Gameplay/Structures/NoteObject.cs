// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Gameplay
{
    public struct NoteObject
    {
        public int StartTime;
        public int KeyLane;
        public int EndTime;
        public GameObject HitNote;
        public GameObject HitSet;
        public GameObject SliderMiddleObject;
        public GameObject SliderEndObject;
        public SpriteRenderer HitSprite;
        public SpriteRenderer SliderMiddleSprite;
        public SpriteRenderer SliderEndSprite;
    }
}