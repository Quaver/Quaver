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
    private const int receptorOffset = 605; //temp
    private const bool upScroll = true; //true = upscroll, false = downscroll
    private KeyCode[] maniaKeyBindings = new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.K, KeyCode.L };
    private const int maxNoteCount = 100; //temp
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
    private List<HitObject> lnQueue;
    private List<HitObject> offLNQueue;
    //private List<HitObject> barQueue;
    private float actualSongTime;
    private float curSongTime;
    private const float waitTilPlay = 0.5f; //waits 2 seconds until song starts
    private float[] noteRot = new float[4] { -90f, 0f, 180f, 90f }; //Rotation of arrows if arrow skin is used
    private float uScrollFloat = 1f;
    private bool[] keyDown = new bool[4];
    private float averageBpm;
    private int[] judgeTimes = new int[6] { 16, 37, 70, 100, 124, 80}; //OD9 judge times in ms (0,1,2,3,4), LN offset 5
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
            lnQueue = new List<HitObject>();
            offLNQueue = new List<HitObject>();
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
            actualSongTime = -waitTilPlay;
            transform.GetComponent<AudioSource>().PlayDelayed(waitTilPlay);
        }
    }

    void Update()
    {
        if (qFile.IsValidQua == true)//Check if map is done or if qFile is valid
        {
            //Song Time Calculation
            //HitBar offset is 6 (+0.5) units. Osu parses notes 0.07seconds late so 0.07seconds is subtracted to counteract.
            
            if (actualSongTime < 0)
            {
                actualSongTime += Time.deltaTime;
            }
            else
            {
                //Averages between frame + music time!
                actualSongTime = ((transform.GetComponent<AudioSource>().time)+(actualSongTime + Time.deltaTime))/2f;
            }
            //curSongTime in ms
            curSongTime = actualSongTime*1000f - osuOffset;
            hitContainer.transform.localPosition = new Vector3(0, -uScrollFloat * (PosFromSV(curSongTime)) * (float)scrollSpeed - uScrollFloat * receptorOffset / 100f + uScrollFloat * (columnSize / 256f), 0);

            //NotePos/NoteMiss Check
            int k = 0;
            for (k = 0; k < hitQueue.Count; k++)
            {
                if (curSongTime- missTime > Mathf.Max(hitQueue[k].StartTime,hitQueue[k].EndTime)) //Todo: Update miss offset later
                {
                    print("MISS");
                    Destroy(hitQueue[k].note);
                    hitQueue.RemoveAt(k);
                    k--;
                }
            }
            //Autoplay
            /*
            for (k = 0; k < hitQueue.Count; k++)
            {
                if (curSongTime * 1000f - osuOffset > hitQueue[k].StartTime) //Todo: Update miss offset later
                {
                    JudgeNote(hitQueue[k].KeyLane, curSongTime - osuOffset/1000f);
                }
            }
            */

            //LN Check
            for(k=0;k<lnQueue.Count;k++)
            {
                HitObject ln = lnQueue[k];
                //Places LNs on top of the receptors
                ln.note.transform.localPosition = new Vector3(receptors[ln.KeyLane-1].transform.localPosition.x, uScrollFloat * (PosFromSV(curSongTime)) * (float)scrollSpeed, 0);

                float lnSize = Mathf.Max(Mathf.Min(PosFromSV(ln.EndTime) - PosFromSV(curSongTime), PosFromSV(ln.EndTime) - PosFromSV(ln.StartTime)),0);
                //print((PosFromSV(ln.EndTime) - PosFromSV(ln.StartTime))-lnSize);
                //float lnSize = Mathf.Max(realSize,0);
                ln.note.transform.Find("SliderMiddle").transform.localScale = Vector3.one * (128f / noteSize * (columnSize / 128f));
                ln.note.transform.Find("SliderEnd").transform.localScale = Vector3.one * (128f / noteSize * (columnSize / 128f));
                ln.note.transform.Find("SliderMiddle").GetComponent<SpriteRenderer>().size = new Vector2(1f, -uScrollFloat * lnSize * (float)scrollSpeed * (128f / columnSize));
                if (uScrollFloat >= 1f) ln.note.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().flipY = true;
                ln.note.transform.Find("SliderEnd").transform.localPosition = new Vector3(0f, uScrollFloat * lnSize * (float)scrollSpeed, 0.1f);
                if (lnSize == 0)
                {
                    ln.note.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().size = new Vector2(1f, Mathf.Max((ln.EndTime-curSongTime)/judgeTimes[5]*0.5f,0));
                }
                if (curSongTime - judgeTimes[5] > ln.EndTime)
                {
                    ln.note.transform.Find("HitImage").GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    Destroy(ln.note.transform.Find("SliderMiddle").gameObject);
                    Destroy(ln.note.transform.Find("SliderEnd").gameObject);

                    offLNQueue.Add(lnQueue[k]);
                    lnQueue.RemoveAt(k);
                    k--;
                    print("LATE LN RELEASE");
                }

            }
            for (k=0;k<offLNQueue.Count;k++)
            {
                if (curSongTime - missTime > offLNQueue[k].EndTime)
                {
                    Destroy(offLNQueue[k].note);
                    offLNQueue.RemoveAt(k);
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
                        JudgeNote(k+1, curSongTime);
                    }
                }
                else
                {
                    if (Input.GetKeyUp(maniaKeyBindings[k]))
                    {
                        keyDown[k] = false;
                        receptors[k].transform.GetComponent<SpriteRenderer>().sprite = receptorSprite[0];
                        JudgeLN(k + 1, curSongTime);

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
        hoo.transform.localPosition = new Vector3(ho.KeyLane * (columnSize / 128f) - (columnSize / 128f * 2.5f), uScrollFloat * PosFromSV(ho.StartTime) * (float)scrollSpeed, 0);
        hoo.transform.Find("HitImage").transform.eulerAngles = new Vector3(0, 0, noteRot[ho.KeyLane - 1]); //Rotation
        if (false ||(ho.EndTime > 0 && ho.EndTime > ho.StartTime))
        {
            float lnSize = PosFromSV(ho.EndTime) - PosFromSV(ho.StartTime);

            hoo.transform.Find("SliderMiddle").transform.localScale = Vector3.one * (128f / noteSize * (columnSize / 128f));
            hoo.transform.Find("SliderEnd").transform.localScale = Vector3.one * (128f / noteSize * (columnSize / 128f));

            hoo.transform.Find("SliderMiddle").GetComponent<SpriteRenderer>().size = new Vector2(1f, -uScrollFloat * lnSize * (float)scrollSpeed * (128f / columnSize));
            if (uScrollFloat >= 1f) hoo.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().flipY = true;
            hoo.transform.Find("SliderEnd").transform.localPosition = new Vector3(0f, uScrollFloat * lnSize * (float)scrollSpeed, 0.1f);
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

    void JudgeLN(int kkey, float timePos)
    {
        int curNote = -1; //Cannot create null struct :(
        float closestTime = 1000f;
        for (int i = 0; i < lnQueue.Count; i++)
        {
            if (lnQueue[i].KeyLane == kkey)
            {
                closestTime = timePos - lnQueue[i].EndTime;
                curNote = i;
            }
        }
        if (curNote >= 0 && curNote < lnQueue.Count)
        {
            if (closestTime < -judgeTimes[5])
            {
                //Darkens early/mis-released LNs. Use skin images instead later
                lnQueue[curNote].note.transform.Find("HitImage").GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
                lnQueue[curNote].note.transform.Find("SliderMiddle").GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
                lnQueue[curNote].note.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
                offLNQueue.Add(lnQueue[curNote]);
                lnQueue.RemoveAt(curNote);
                print("EARLY LN RELEASE");
            }
            else if (closestTime > -judgeTimes[5] && closestTime < judgeTimes[5])
            {
                Destroy(lnQueue[curNote].note);
                lnQueue.RemoveAt(curNote);
                print("PERFECT LN RELEASE");
            }
        }
    }

    void JudgeNote(int kkey,float timePos)
    {
        int curNote = 0; //Cannot create null struct :(
        float closestTime = 1000f;
        int i = 0;
        for (i = 0; i < hitQueue.Count; i++)
        {
            float curNoteTime = hitQueue[i].StartTime - timePos;
            if (hitQueue[i].KeyLane == kkey && curNoteTime < closestTime && Mathf.Abs(curNoteTime) < judgeTimes[4])
            {
                closestTime = curNoteTime;
                curNote = i;
            }
        }
        closestTime = Mathf.Abs(closestTime);
        if (closestTime < judgeTimes[4])
        {
            if (closestTime < judgeTimes[0])
            {
                print("MARV");
            }
            else if (closestTime < judgeTimes[1])
            {
                print("PERF");
            }
            else if (closestTime < judgeTimes[2])
            {
                print("GREAT");
            }
            else if (closestTime < judgeTimes[3])
            {
                print("GOOD");
            }
            else
            {
                print("BAD");
            }
            //Check if LN
            if (hitQueue[curNote].EndTime > 0)
            {
                lnQueue.Add(hitQueue[curNote]);
                hitQueue.RemoveAt(curNote);
            } 
            else
            {
                Destroy(hitQueue[curNote].note);
                hitQueue.RemoveAt(curNote);
            }
            //Create particles
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
