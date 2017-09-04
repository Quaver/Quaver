using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Qua.Scripts;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		QuaFile qFile = QuaParser.Parse(@"C:\Users\swan\Desktop\Stuff\Git\Quaver\TestFiles\Qua\planet_shaper.qua");
		Debug.Log(qFile.HitObjects.Count);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
