using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Qua.Scripts;

public class InstantiateNoteTest : MonoBehaviour {
    private QuaFile qFile;
    public Sprite[] Arrows;
    public Sprite sliderMiddle;
    public Sprite sliderEnd;
    public GameObject hitObjectTest;
    private int noteSize = 128;
    private int columnSize = 140;


    void Start () {
        qFile = QuaParser.Parse("E:\\GitHub\\Quaver\\TestFiles\\Qua\\planet_shaper.qua");
        if (qFile.Artist != null)
        {
            int i = 0;
            foreach (HitObject ho in qFile.HitObjects)
            {
                if (i < 200)
                {
                    //print(ho.KeyLane + " | " + ho.StartTime);
                    GameObject hoo = Instantiate(hitObjectTest, transform.Find("HitContainer"));
                    if (ho.KeyLane == 1 || ho.KeyLane == 2 || ho.KeyLane == 3 || ho.KeyLane == 4)
                    {
                        hoo.transform.Find("HitImage").GetComponent<SpriteRenderer>().sprite = Arrows[ho.KeyLane-1];
                    }
                    hoo.transform.localScale = Vector3.one * (100f / (float)noteSize) * (columnSize/100f);
                    hoo.transform.position = new Vector3(ho.KeyLane*((float)columnSize/100f)-((float)columnSize/100f *2), ho.StartTime/1000f*8f, 0);
                    //Instantiate(hito)
                    i++;
                }
                else
                {
                    break;
                }
            }
            //print(i+" | count: "+qFile.HitObjects.Count);
        }
    }
	

}
