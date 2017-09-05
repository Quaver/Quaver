using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Qua.Scripts;

public class InstantiateNoteTest : MonoBehaviour {
    /*Classes/Gameobjects*/
    private QuaFile qFile;
    public GameObject hitObjectTest;
    public GameObject receptorBar;
    private GameObject[] receptors;
    public GameObject arrowParticles;
    public GameObject hitContainer;

    /*CONFIG VALUES*/
    private const int noteSize = 128; //temp, size of noteskin in pixels
    private const int columnSize = 220; //temp
    private const int scrollSpeed = 24; //temp
    private const int receptorOffset = 575; //temp
    private const bool upScroll = true; //true = upscroll, false = downscroll
    private KeyCode[] maniaKeyBindings = new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.K, KeyCode.L };
    private const int maxNoteCount = 200; //temp
    private const int playerOffset = 0;
    private const int osuOffset = 170;

    /*SKINNING VALUES*/
    public Sprite[] receptorSprite;
    public GameObject circleParticleSystem;
    public GameObject hitBurst;

    /*Referencing Values*/
    private List<HitObject> noteQueue;
    private List<SliderVelocity> SvQueue;
    private List<TimingPoint> timingQueue;
    private List<HitObject> hitQueue;
    //private List<HitObject> barQueue;
    private float curSongTime;
    private const float waitTilPlay = 0.5f; //waits 2 seconds until song starts
    private float[] noteRot = new float[4] { -90f, 0f, 180f, 90f }; //Rotation of arrows if arrow skin is used
    private float uScrollFloat = 1f;
    private bool[] keyDown = new bool[4];
    private float averageBpm;
    private int[] judgeTimes = new int[5] { 16, 37, 70, 100, 124}; //OD9 judge times in ms
    private const int missTime = 200; //after 200ms, if the player doesn't press it will count as a miss.

    void Start () {
        //Changes the transparency mode of the camera so images dont clip
        GameObject.Find("Main Camera").GetComponent<Camera>().transparencySortMode = TransparencySortMode.Orthographic;

        qFile = QuaParser.Parse("E:\\GitHub\\Quaver\\TestFiles\\Qua\\planet_shaper.qua");
        if (qFile.IsValidQua == true)
        {
            //Declare Reference Values
            noteQueue = qFile.HitObjects;
            SvQueue = qFile.SliderVelocities;
            timingQueue = qFile.TimingPoints;
            hitQueue = new List<HitObject>();
            //barQueue = new List<HitObject>();

            averageBpm = 147f; //Change later

            int longestBpmTime = 0;
            int avgBpmPos = 0;
            int i = 0;

            //Calculate Average BPM of map
            if (timingQueue.Count > 1)
            {
                foreach (TimingPoint tp in timingQueue)
                {
                    if (i+1 < timingQueue.Count)
                    {
                        if (timingQueue[i+1].StartTime - timingQueue[i].StartTime > longestBpmTime)
                        {
                            avgBpmPos = i;
                            longestBpmTime = timingQueue[i + 1].StartTime - timingQueue[i].StartTime;
                        }
                    }
                }
                averageBpm = timingQueue[avgBpmPos].BPM;
            }
            else
            {
                averageBpm = timingQueue[0].BPM;
            }

            //Create and converts timing points to SV's
            foreach (TimingPoint tp in timingQueue)
            {
                for (i = 0; i < SvQueue.Count; i++)
                {
                    if (i == 0 && tp.StartTime < SvQueue[i].StartTime)
                    {
                        SliderVelocity newTp = new SliderVelocity();
                        newTp.StartTime = tp.StartTime;
                        newTp.Multiplier = averageBpm / tp.BPM;
                        SvQueue.Insert(i, newTp);
                    }
                    else if (tp.StartTime > SvQueue[i].StartTime)
                    {
                        SliderVelocity newTp = new SliderVelocity();
                        newTp.StartTime = tp.StartTime;
                        newTp.Multiplier = averageBpm / tp.BPM;
                        SvQueue.Insert(i, newTp);
                        if (SvQueue[i].StartTime == SvQueue[i + 1].StartTime)
                        {
                            SvQueue.RemoveAt(i + 1);
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
            }
            //Normalizes SV's in between each BPM change interval
            if (timingQueue.Count > 1)
            {
                i = 0;
                for (i= 0;i < timingQueue.Count;i++)
                {
                    if (i+1 < timingQueue.Count)
                    {
                        for (int j = 0; j < SvQueue.Count; j++)
                        {
                            //Check if SV point is between timing point to normalize bpm
                            if (SvQueue[j].StartTime >= timingQueue[i].StartTime && SvQueue[j].StartTime < timingQueue[i + 1].StartTime)
                            {
                                SliderVelocity newTp = new SliderVelocity();
                                newTp.StartTime = SvQueue[j].StartTime;
                                newTp.Multiplier = SvQueue[j].Multiplier * (averageBpm / timingQueue[i].BPM);
                                SvQueue.Insert(i, newTp);
                                if (SvQueue[i].StartTime == SvQueue[i + 1].StartTime)
                                {
                                    SvQueue.RemoveAt(i + 1);
                                }
                                else
                                {
                                    i++;
                                }
                            }
                        }
                    }
                    i++;
                }
            }

            //Declare Receptor Values
            if (upScroll) uScrollFloat = -1f;
            receptorBar.transform.localPosition = new Vector3(0, -uScrollFloat * receptorOffset / 100f + uScrollFloat * (columnSize / 256f), 1f);
            arrowParticles.transform.localPosition = new Vector3(0, -uScrollFloat * receptorOffset / 100f + uScrollFloat * (columnSize / 256f), 0f);
            receptors = new GameObject[4];
            receptors[0] = receptorBar.transform.Find("R1").gameObject;
            receptors[1] = receptorBar.transform.Find("R2").gameObject;
            receptors[2] = receptorBar.transform.Find("R3").gameObject;
            receptors[3] = receptorBar.transform.Find("R4").gameObject;

            i = 0;
            foreach (GameObject r0 in receptors)
            {
                r0.transform.localScale = Vector3.one * (128f / noteSize * (columnSize / 128f));
                r0.transform.localPosition = new Vector3((i + 1) * (columnSize / 128f) - (columnSize / 128f * 2.5f), 0, 0);
                r0.transform.transform.eulerAngles = new Vector3(0, 0, noteRot[i]); //Rotation
                i++;
            }

            //Create starting notes
            i = 0;
            for (i=0;i< maxNoteCount; i++)
            {
                if (noteQueue.Count > 0)
                {
                    InstantiateNote();
                }
                else
                {
                    break;
                }
            }

            //Plays the song, but delayed
            curSongTime = -waitTilPlay;
            transform.GetComponent<AudioSource>().PlayDelayed(waitTilPlay);
        }
    }

    void Update()
    {
        if (qFile.IsValidQua == true)//Check if map is done or if qFile is valid
        {
            //Song Time Calculation
            //HitBar offset is 6 (+0.5) units. Osu parses notes 0.07seconds late so 0.07seconds is subtracted to counteract.
            
            if (curSongTime<0)
            {
                curSongTime += Time.deltaTime;
            }
            else
            {
                //Averages between frame + music time!
                curSongTime = ((transform.GetComponent<AudioSource>().time)+(curSongTime+Time.deltaTime))/2f;
            }
            hitContainer.transform.localPosition = new Vector3(0, -uScrollFloat * (PosFromSV(curSongTime - osuOffset / 1000f)) * (float)scrollSpeed - uScrollFloat * receptorOffset / 100f + uScrollFloat * (columnSize / 256f), 0);

            //NotePos/NoteMiss Check
            int k = 0;
            for (k = 0; k < hitQueue.Count; k++)
            {
                if (curSongTime * 1000f - osuOffset - missTime > hitQueue[k].StartTime) //Todo: Update miss offset later
                {
                    print("MISS");
                    Destroy(hitQueue[k].note);
                    hitQueue.RemoveAt(k);
                    k--;
                }
            }

            //Key Press Check
            for (k = 0; k < 4; k++)
            {
                receptors[k].transform.localScale = Vector3.one * (receptors[k].transform.localScale.x + ((128f / noteSize * (columnSize / 128f)) - receptors[k].transform.localScale.x) / 3f);
            }
            for (k = 0; k < 4; k++)
            {
                if (!keyDown[k])
                {
                    if (Input.GetKeyDown(maniaKeyBindings[k]))
                    {
                        keyDown[k] = true;
                        receptors[k].transform.localScale = Vector3.one * (128f / noteSize * (columnSize / 128f)) * 1.1f;
                        receptors[k].transform.GetComponent<SpriteRenderer>().sprite = receptorSprite[1];
                        JudgeNote(k+1, curSongTime - osuOffset/1000f);
                    }
                }
                else
                {
                    if (Input.GetKeyUp(maniaKeyBindings[k]))
                    {
                        keyDown[k] = false;
                        receptors[k].transform.GetComponent<SpriteRenderer>().sprite = receptorSprite[0];

                    }
                }
            }

            //Note Add Check
            if (noteQueue.Count > 0 && hitQueue.Count < maxNoteCount)
            {
                InstantiateNote();
            }
        }
    }


    void InstantiateNote()
    {
        HitObject ho = noteQueue[0];
        GameObject hoo = Instantiate(hitObjectTest, hitContainer.transform);
        hoo.transform.Find("HitImage").transform.localScale = Vector3.one * (128f / noteSize * (columnSize / 128f));
        hoo.transform.localPosition = new Vector3(ho.KeyLane * (columnSize / 128f) - (columnSize / 128f * 2.5f), uScrollFloat * PosFromSV(ho.StartTime/ 1000f) * scrollSpeed, 0);
        hoo.transform.Find("HitImage").transform.eulerAngles = new Vector3(0, 0, noteRot[ho.KeyLane - 1]); //Rotation
        if (false ||(ho.EndTime > 0 && ho.EndTime > ho.StartTime))
        {
            float lnSize = PosFromSV(ho.EndTime) - PosFromSV(ho.StartTime);

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
        ho.note = hoo;
        hitQueue.Add(ho);
        noteQueue.RemoveAt(0);
    }

    void JudgeNote(int kkey,float timePos)
    {
        int curNote = 0; //Cannot create null struct :(
        float closestTime = 1f;
        int i = 0;
        for (i = 0; i < hitQueue.Count; i++)
        {
            float curNoteTime = hitQueue[i].StartTime / 1000f - timePos;
            if (hitQueue[i].KeyLane == kkey && curNoteTime < closestTime && Mathf.Abs(curNoteTime) < judgeTimes[4]/1000f)
            {
                closestTime = curNoteTime;
                curNote = i;
            }
        }
        closestTime = Mathf.Abs(closestTime);
        if (closestTime < judgeTimes[4] / 1000f)
        {
            if (closestTime < judgeTimes[0] / 1000f)
            {
                print("MARV");
            }
            else if (closestTime < judgeTimes[1] / 1000f)
            {
                print("PERF");
            }
            else if (closestTime < judgeTimes[2] / 1000f)
            {
                print("GREAT");
            }
            else if (closestTime < judgeTimes[3] / 1000f)
            {
                print("GOOD");
            }
            else
            {
                print("BAD");
            }
            Destroy(hitQueue[curNote].note);
            hitQueue.RemoveAt(curNote);
            GameObject cp = Instantiate(circleParticleSystem, arrowParticles.transform);
            cp.transform.localPosition = receptors[kkey - 1].transform.localPosition + new Vector3(0,0,2f);
            GameObject hb = Instantiate(hitBurst, arrowParticles.transform);
            hb.GetComponent<NoteBurst>().startSize = hb.transform.localScale.x;
            hb.transform.localPosition = receptors[kkey - 1].transform.localPosition + new Vector3(0, 0, -2f);
            hb.transform.eulerAngles = receptors[kkey - 1].transform.eulerAngles;
            hb.transform.localScale = receptors[kkey - 1].transform.localScale;
        }
    }
	
    float PosFromSV(float timePos)
    {
        timePos = timePos * 1000f;
        float svPos = 0;
        int i = 0;
        bool endLoop = false;
        while (!endLoop && i < SvQueue.Count)
        {
            if (i + 1 < SvQueue.Count)
            {
                if(timePos > SvQueue[i+1].StartTime)
                {
                    svPos += (SvQueue[i + 1].StartTime - SvQueue[i].StartTime)/1000f * SvQueue[i].Multiplier;
                }
                else
                {
                    svPos += (timePos - SvQueue[i].StartTime)/1000f * SvQueue[i].Multiplier;
                    endLoop = true;
                }
            }
            else
            {
                svPos += (timePos - SvQueue[i].StartTime)/1000f * SvQueue[i].Multiplier;
                endLoop = true;
            }
            i++;
        }
        //print(i+ "/ "+svPos);
        return svPos;
    }

}
