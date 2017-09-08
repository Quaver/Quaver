using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Qua {

	public struct HitObject
	{
		public int StartTime;
		public int KeyLane;
		public int EndTime;
        //I'll make a new class for notes later. I'm trying to optimize frame rate :p
    	public GameObject note;
        public GameObject hitObject;
        public GameObject sliderMiddleObject;
        public GameObject sliderEndObject;
        public SpriteRenderer hitSprite;
        public SpriteRenderer sliderMiddleSprite;
        public SpriteRenderer sliderEndSprite;
    }

}
