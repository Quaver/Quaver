﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Qua.Scripts;

public class InstantiateNoteTest : MonoBehaviour {
    private QuaFile qFile;
    public GameObject hitObjectTest;
    public GameObject receptorBar;
    private GameObject[] receptors; 

    private const int noteSize = 128; //temp, size of noteskin in pixels
    private const int columnSize = 200; //temp
    private const int scrollSpeed = 15; //temp
    private const int receptorOffset = 625; //temp
    private float[] noteRot = new float[4] { -90f, 0f, 180f, 90f }; //Rotation of arrows if arrow skin is used

    private bool upScroll = true; //true = upscroll, false = downscroll
    private float uScrollFloat = 1f;

    void Start () {
        if (upScroll) uScrollFloat = -1f;

        //Declaring Receptor Values
        receptorBar.transform.localPosition = new Vector3(0, -uScrollFloat * receptorOffset/100f + uScrollFloat* (columnSize / 256f), 1f);
        receptors = new GameObject[4];
        receptors[0] = receptorBar.transform.Find("R1").gameObject;
        receptors[1] = receptorBar.transform.Find("R2").gameObject;
        receptors[2] = receptorBar.transform.Find("R3").gameObject;
        receptors[3] = receptorBar.transform.Find("R4").gameObject;

        int i = 0;
        foreach (GameObject r0 in receptors)
        {
            r0.transform.localScale = Vector3.one * (128f / noteSize * (columnSize / 128f));
            r0.transform.localPosition = new Vector3((i+1) * (columnSize / 128f) - (columnSize / 128f * 2.5f), 0, 0);
            r0.transform.transform.eulerAngles = new Vector3(0, 0, noteRot[i]); //Rotation
            i++;
        }

        //Changes the transparency mode of the camera so images dont clip
        GameObject.Find("Main Camera").GetComponent<Camera>().transparencySortMode = TransparencySortMode.Orthographic;

        qFile = QuaParser.Parse("E:\\GitHub\\Quaver\\TestFiles\\Qua\\planet_shaper.qua");
        if (qFile.IsValidQua == true)
        {
            i = 0;
            //Creates note + rotates it
            foreach (HitObject ho in qFile.HitObjects)
            {
                if (i < 200) //Only 500 for testing
                {
                    GameObject hoo = Instantiate(hitObjectTest, transform.Find("HitContainer"));
                    hoo.transform.Find("HitImage").transform.localScale = Vector3.one * (128f / noteSize * (columnSize/ 128f));
                    hoo.transform.localPosition = new Vector3(ho.KeyLane*(columnSize/ 128f) -(columnSize/ 128f * 2.5f), uScrollFloat * ho.StartTime/ 1000f * scrollSpeed, 0);
                    hoo.transform.Find("HitImage").transform.eulerAngles = new Vector3(0, 0, noteRot[ho.KeyLane-1]); //Rotation
                    if (ho.EndTime > 0 && ho.EndTime > ho.StartTime)
                    {
                        float lnSize = ho.EndTime - ho.StartTime;
                        
                        hoo.transform.Find("SliderMiddle").transform.localScale = Vector3.one * (128f / noteSize * (columnSize / 128f));
                        hoo.transform.Find("SliderEnd").transform.localScale = Vector3.one * (128f / noteSize * (columnSize / 128f));

                        hoo.transform.Find("SliderMiddle").GetComponent<SpriteRenderer>().size = new Vector2(1f, -uScrollFloat * lnSize / 1000f * scrollSpeed * (128f / columnSize));
                        if (uScrollFloat >= 1f) hoo.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().flipY = true;
                        hoo.transform.Find("SliderEnd").transform.localPosition = new Vector3(0f, uScrollFloat * lnSize / 1000f * scrollSpeed, 0.1f);
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

        //HitBar offset is 6 (+0.5) units. Osu parses notes 0.07seconds late so 0.07seconds is subtracted to counteract.
        transform.Find("HitContainer").transform.localPosition = new Vector3(0, -uScrollFloat * (curSongTime-0.1f) * (float)scrollSpeed  - uScrollFloat* receptorOffset / 100f + uScrollFloat * (columnSize / 256f), 0);

        
    }
	

}
