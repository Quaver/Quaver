// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

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