using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Qua.Scripts;

public class InstantiateNoteTest : MonoBehaviour {
    private QuaFile qFile;
    public GameObject hitObjectTest;
    private const int noteSize = 128; //temp
    private const int columnSize = 200; //temp
    private const int scrollSpeed = 15; //temp

    private bool upScroll = false; //true = upscroll, false = downscroll
    private float uScrollFloat = 1f;

    void Start () {
        if (upScroll) uScrollFloat = -1f;

        //remove once tempBar is removed. For hit receptors.
        transform.Find("tempBar").transform.localPosition = new Vector3(0, -uScrollFloat * 5f, 0); //remove once tempBar is removed. For hit receptors.

        //Changes the transparency mode of the camera so images dont clip
        GameObject.Find("Main Camera").GetComponent<Camera>().transparencySortMode = TransparencySortMode.Orthographic;

        qFile = QuaParser.Parse("E:\\GitHub\\Quaver\\TestFiles\\Qua\\planet_shaper.qua");
        if (qFile.IsValidQua == true)
        {
            int i = 0;
            //Creates note + rotates it
            //ho.KeyLane == 2: no rotation
            foreach (HitObject ho in qFile.HitObjects)
            {
                if (i < 500) //Only 500 for testing
                {
                    GameObject hoo = Instantiate(hitObjectTest, transform.Find("HitContainer"));
                    hoo.transform.Find("HitImage").transform.localScale = Vector3.one * (128f / (float)noteSize * (columnSize/ 128f));
                    hoo.transform.position = new Vector3(ho.KeyLane*((float)columnSize/ 128f) -((float)columnSize/ 128f * 2.5f), uScrollFloat * ho.StartTime/ 1000f * (float)scrollSpeed, 0);
                    if (ho.KeyLane == 1)
                    {
                        hoo.transform.Find("HitImage").transform.eulerAngles = new Vector3(0, 0, -90f);
                    }
                    else if (ho.KeyLane == 3)
                    {
                        hoo.transform.Find("HitImage").transform.eulerAngles = new Vector3(0, 0, 180f);
                    }
                    else if (ho.KeyLane == 4)
                    {
                        hoo.transform.Find("HitImage").transform.eulerAngles = new Vector3(0, 0, 90f);
                    }

                    if (ho.EndTime > 0 && ho.EndTime > ho.StartTime)
                    {
                        float lnSize = ho.EndTime - ho.StartTime;
                        
                        hoo.transform.Find("SliderMiddle").transform.localScale = Vector3.one * (128f / (float)noteSize * (columnSize / 128f));
                        hoo.transform.Find("SliderEnd").transform.localScale = Vector3.one * (128f / (float)noteSize * (columnSize / 128f));

                        hoo.transform.Find("SliderMiddle").GetComponent<SpriteRenderer>().size = new Vector2(1f, -uScrollFloat * lnSize / 1000f * (float)scrollSpeed * (128f / columnSize));
                        if (uScrollFloat >= 1f) hoo.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().flipY = true;
                        hoo.transform.Find("SliderEnd").transform.localPosition = new Vector3(0f, uScrollFloat * lnSize / 1000f * (float)scrollSpeed, 0.1f);
                    }
                    else
                    {
                        Destroy(hoo.transform.Find("SliderMiddle").gameObject);
                        Destroy(hoo.transform.Find("SliderEnd").gameObject);
                    }

                    i++;
                }
                else
                {
                    break;
                }
            }
        }
    }
    
    void Update()
    {
        float curSongTime = transform.GetComponent<AudioSource>().time;

        //HitBar offset is 5 units. Osu parses notes 0.07seconds late so 0.07seconds is subtracted to counteract.
        transform.Find("HitContainer").transform.localPosition = new Vector3(0, -uScrollFloat * (curSongTime-0.07f) * (float)scrollSpeed  - uScrollFloat*5f, 0);

        
    }
	

}
