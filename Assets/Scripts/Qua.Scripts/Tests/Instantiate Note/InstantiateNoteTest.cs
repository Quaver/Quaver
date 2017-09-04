using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Qua.Scripts;

public class InstantiateNoteTest : MonoBehaviour {
    private QuaFile qFile;
    void Start () {
        qFile = QuaParser.Parse("E:\\GitHub\\Quaver\\TestFiles\\Qua\\format_example.qua");
        if (qFile.Artist != null)
        {
            int i = 0;
            foreach (HitObject ho in qFile.HitObjects)
            {
                print(ho.KeyLane + " | " + ho.StartTime);
                i++;
            }
            print(i+" | count: "+qFile.HitObjects.Count);
        }
    }
	
	void Update()
    {
        /*if (qFile.Artist != null)
        {
            int i = 0;
            foreach (HitObject ho in qFile.HitObjects)
            {
                print(ho.KeyLane + " | " + ho.StartTime);
                i++;
            }
            print(i);
        }*/ 
    }

}
