
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