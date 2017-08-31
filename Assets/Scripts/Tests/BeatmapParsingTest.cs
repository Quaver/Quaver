using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BeatmapParsingTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Beatmap beatmap = BeatmapParser.Parse(@"C:\Users\swan\Desktop\Stuff\Git\Quaver\Assets\Songs\w1sh\w1sh.sm");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
