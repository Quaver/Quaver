using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Qua;
using Quaver.Graphics;
using Quaver.Main;

namespace Quaver.Gameplay
{
    public class InstantiateNoteTest : GameState
    {
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
        private const int scrollSpeed = 22; //temp
        private const int receptorOffset = 605; //temp
        private const bool upScroll = false; //true = upscroll, false = downscroll
        private KeyCode[] maniaKeyBindings = new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.K, KeyCode.L };
        private const int maxNoteCount = 60; //temp
        private const int playerOffset = 0;
        private const int osuOffset = 0;

        /*GAME MODS*/
        private const bool noSV = false;
        private const bool pull = true;
        private const bool split = true;

        /*SKINNING VALUES*/
        public Sprite[] receptorSprite;
        public GameObject circleParticleSystem;
        public GameObject hitBurst;
        public GameObject timingBar;

        /*Referencing Values*/
        private AudioSource songAudio;
        private List<HitObject> noteQueue;
        private List<SliderVelocity> SvQueue;
        private List<TimingPoint> timingQueue;
        private List<HitObject> barQueue;
        private List<HitObject> hitQueue;
        private List<HitObject> lnQueue;
        private List<HitObject> offLNQueue;
        private GameObject[] hitBursts;
        private GameObject[] activeNotes;
        private GameObject[] activeBars;
        private ulong[] svCalc; //Stores SV position data for efficiency
        private float actualSongTime;
        private float curSongTime;
        private const float waitTilPlay = 0.5f; //waits 2 seconds until song starts
        private float[] noteRot = new float[4] { -90f, 0f, 180f, 90f }; //Rotation of arrows if arrow skin is used
        private float uScrollFloat = 1f;
        private bool[] keyDown = new bool[4];
        private float averageBpm;
        private int[] judgeTimes = new int[6] { 16, 37, 70, 100, 124, 80}; //OD9 judge times in ms (0,1,2,3,4), LN offset 5
        private const int missTime = 200; //after 200ms, if the player doesn't press it will count as a miss.
        private float hitYPos;
        private ulong curSVPos;

        public void Start()
        {
            qFile = QuaParser.Parse("E:\\GitHub\\Quaver\\TestFiles\\Qua\\backbeat_maniac.qua");
            if (!qFile.IsValidQua)
            {
                print("IS NOT VALID QUA FILE");
            }
            else if (qFile.IsValidQua)
            {
                //Declare Reference Values
                noteQueue = qFile.HitObjects;
                SvQueue = qFile.SliderVelocities;
                timingQueue = qFile.TimingPoints;
                hitQueue = new List<HitObject>();
                lnQueue = new List<HitObject>();
                offLNQueue = new List<HitObject>();
                barQueue = new List<HitObject>();
                activeBars = new GameObject[maxNoteCount];
                activeNotes = new GameObject[maxNoteCount];

                averageBpm = 120f; //Change later
                songAudio = transform.GetComponent<AudioSource>();

                float longestBpmTime = 0;
                int avgBpmPos = 0;
                int i = 0;

                //Declare Other Values
                curSongTime = -waitTilPlay;
                actualSongTime = -waitTilPlay;
                curSVPos = (ulong)(-waitTilPlay * 1000f + 10000f); //10000ms added since curSVPos is a uLong
                hitYPos = -uScrollFloat * receptorOffset / 100f + uScrollFloat * (columnSize / 256f);

                //Declare Receptor Values
                if (upScroll) uScrollFloat = -1f;
                arrowParticles.transform.localPosition = new Vector3(0, -uScrollFloat * receptorOffset / 100f + uScrollFloat * (columnSize / 256f), 0f);
                receptors = new GameObject[4];
                receptors[0] = receptorBar.transform.Find("R1").gameObject;
                receptors[1] = receptorBar.transform.Find("R2").gameObject;
                receptors[2] = receptorBar.transform.Find("R3").gameObject;
                receptors[3] = receptorBar.transform.Find("R4").gameObject;
                hitBursts = new GameObject[4];


                i = 0;
                foreach (GameObject r0 in receptors)
                {
                    if (i>=2 && split) r0.transform.localPosition = new Vector3((i + 1) * (columnSize / 128f) - (columnSize / 128f * 2.5f), -hitYPos, 0);
                    else r0.transform.localPosition = new Vector3((i + 1) * (columnSize / 128f) - (columnSize / 128f * 2.5f), hitYPos, 0);
                    r0.transform.localScale = Vector3.one * (128f / noteSize * (columnSize / 128f));
                    r0.transform.transform.eulerAngles = new Vector3(0, 0, noteRot[i]); //Rotation
                    i++;
                }

                for (i = 0; i < 4; i++)
                {
                    GameObject hb = Instantiate(hitBurst, arrowParticles.transform);
                    hb.GetComponent<NoteBurst>().startSize = hb.transform.localScale.x;
                    hb.transform.localPosition = receptors[i].transform.localPosition + new Vector3(0, 0, -2f);
                    hb.transform.eulerAngles = receptors[i].transform.eulerAngles;
                    hb.transform.localScale = receptors[i].transform.localScale;
                    hitBursts[i] = hb;
                }

                //Calculate Average BPM of map
                if (timingQueue.Count > 1)
                {
                    foreach (TimingPoint tp in timingQueue)
                    {
                        if (i + 1 < timingQueue.Count)
                        {
                            if (timingQueue[i + 1].StartTime - timingQueue[i].StartTime > longestBpmTime)
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
                print("AVERAGE BPM: " + averageBpm);

                //Create and converts timing points to SV's

                int j = 0;
                for (j = 0; j < timingQueue.Count; j++)
                {
                    for (i = 0; i < SvQueue.Count; i++)
                    {

                        if (i == 0 && timingQueue[j].StartTime < SvQueue[i].StartTime)
                        {
                            SliderVelocity newTp = new SliderVelocity();
                            newTp.StartTime = timingQueue[j].StartTime;
                            newTp.Multiplier = 1f;
                            SvQueue.Insert(i, newTp);
                            break;
                        }
                        else if (i + 1 < SvQueue.Count)
                        {

                            if (timingQueue[j].StartTime >= SvQueue[i].StartTime && timingQueue[j].StartTime < SvQueue[i + 1].StartTime)
                            {
                                if (Mathf.Abs(timingQueue[j].StartTime - SvQueue[i].StartTime) < 1f)
                                {
                                    SliderVelocity newTp = new SliderVelocity();
                                    newTp.StartTime = timingQueue[j].StartTime;
                                    newTp.Multiplier = SvQueue[i].Multiplier;
                                    SvQueue.RemoveAt(i);
                                    SvQueue.Insert(i, newTp);
                                }
                                else
                                {
                                    SliderVelocity newTp = new SliderVelocity();
                                    newTp.StartTime = timingQueue[j].StartTime;
                                    newTp.Multiplier = 1f;
                                    SvQueue.Insert(i, newTp);
                                    i++;
                                }
                                break;
                            }
                        }
                        else if (i == SvQueue.Count)
                        {
                            if (Mathf.Abs(timingQueue[j].StartTime - SvQueue[i].StartTime) < 1f)
                            {
                                SliderVelocity newTp = new SliderVelocity();
                                newTp.StartTime = timingQueue[j].StartTime;
                                newTp.Multiplier = SvQueue[i].Multiplier;
                                SvQueue.RemoveAt(i);
                                SvQueue.Insert(i, newTp);
                                //print("A "+newTp.StartTime);
                            }
                            else
                            {
                                SliderVelocity newTp = new SliderVelocity();
                                newTp.StartTime = timingQueue[j].StartTime;
                                newTp.Multiplier = 1f;
                                SvQueue.Insert(i, newTp);
                                i++;
                            }
                            break;
                        }
                    }
                }

                //Normalizes SV's in between each BPM change interval
                //I can't find where the notes aren't getting sorted properly, so I added this :/
                SvQueue.Sort(delegate (SliderVelocity p1, SliderVelocity p2) { return p1.StartTime.CompareTo(p2.StartTime); });
                if (timingQueue.Count > 1)
                {
                    int hij = 0;
                    for (i = 0; i < timingQueue.Count; i++)
                    {
                        for (j = hij; j < SvQueue.Count; j++)
                        {
                            if (SvQueue[j].StartTime >= timingQueue[i].StartTime)
                            {
                                SliderVelocity newTp = new SliderVelocity();
                                newTp.StartTime = SvQueue[j].StartTime;
                                newTp.Multiplier = Mathf.Min(SvQueue[j].Multiplier * timingQueue[i].BPM / averageBpm, 1000f); //SvQueue[j].Multiplier * (timingQueue[i].BPM /averageBpm);
                                SvQueue.Insert(j, newTp);
                                SvQueue.RemoveAt(j + 1);
                                hij++;
                                break;
                            }
                        }
                    }
                }


                //Calculates SV for efficiency
                svCalc = new ulong[SvQueue.Count];
                ulong svPosTime = 0;
                svCalc[0] = 0;
                for (i = 0; i < SvQueue.Count; i++)
                {
                    if (i + 1 < SvQueue.Count)
                    {
                        svPosTime += (ulong)((SvQueue[i + 1].StartTime - SvQueue[i].StartTime) * SvQueue[i].Multiplier);
                        svCalc[i + 1] = svPosTime;
                    }
                    else break;
                }
                //Create Timing bars
                if (!split) { 
                    float curBarTime = 0;
                    for (i = 0; i < timingQueue.Count; i++)
                    {
                        curBarTime = timingQueue[i].StartTime;

                        if (barQueue.Count > 0 && barQueue[0].StartTime + 2 > curBarTime) barQueue.RemoveAt(0);
                        curBarTime += 1000f * 4f * 60f / (timingQueue[i].BPM);
                        HitObject curTiming;

                        if (i + 1 < timingQueue.Count)
                        {
                            while (curBarTime < timingQueue[i + 1].StartTime)
                            {
                                curTiming = new HitObject();
                                curTiming.StartTime = (int)Mathf.Floor(curBarTime);
                                barQueue.Add(curTiming);
                                curBarTime += 1000f * 4f * 60f / (timingQueue[i].BPM);
                            }
                        }
                        else
                        {
                            while (curBarTime < songAudio.clip.length * 1000f)
                            {
                                curTiming = new HitObject();
                                curTiming.StartTime = (int)Mathf.Floor(curBarTime);
                                barQueue.Add(curTiming);
                                curBarTime += 1000f * 4f * 60f / (timingQueue[i].BPM);
                            }
                        }
                    }

                    //Create starting bars
                    activeBars = new GameObject[noteQueue.Count];
                    List<HitObject> tempBars = new List<HitObject>();
                    for (i = 0; i < barQueue.Count; i++)
                    {
                        HitObject hoo = new HitObject();
                        GameObject curBar = Instantiate(timingBar, hitContainer.transform);
                        curBar.transform.localPosition = new Vector3(0f, PosFromSV(barQueue[i].StartTime), 2f);

                        hoo.StartTime = barQueue[i].StartTime;
                        activeBars[i] = curBar;
                        hoo.note = curBar;
                        tempBars.Add(hoo);
                    }

                    barQueue = new List<HitObject>(tempBars);
                }

                //Create starting notes
                for (i = 0; i < maxNoteCount; i++)
                {
                    if (noteQueue.Count > 0) activeNotes[i] = InstantiateNote(null);
                    else break;
                }

                //Plays the song, but delayed
                songAudio.PlayDelayed(waitTilPlay);
                print("TOTAL SV CHANGES: " + SvQueue.Count);

                loaded = true;

            }
        }

        public void Update()
        {
            if (qFile.IsValidQua && isActive)
            {
                if (!songAudio.isPlaying) songAudio.UnPause();
                //Song Time Calculation (ms)
                if (actualSongTime < 0)
                {
                    actualSongTime += Time.deltaTime;
                }
                else
                {
                    actualSongTime = ((songAudio.time)+(actualSongTime + Time.deltaTime))/2f;
                }

                //Calculates curSV Position
                curSongTime = actualSongTime*1000f - osuOffset;
                curSVPos = 10000;
                int k = 0;
                while (k < SvQueue.Count)
                {
                    if (k + 1 < SvQueue.Count)
                    {
                        if (curSongTime > SvQueue[k + 1].StartTime)
                        {
                            curSVPos += (ulong)((SvQueue[k + 1].StartTime - SvQueue[k].StartTime) * SvQueue[k].Multiplier);
                        }
                        else
                        {
                            curSVPos += (ulong)((curSongTime - SvQueue[k].StartTime) * SvQueue[k].Multiplier);
                            break;
                        }
                    }
                    else
                    {
                        if (k < SvQueue.Count)
                        {
                            curSVPos += (ulong)((curSongTime - SvQueue[k].StartTime) * SvQueue[k].Multiplier);
                        }
                        break;
                    }
                    k++;
                }

                //Move notes and check if miss
                for (k = 0; k < hitQueue.Count; k++)
                {
                    float splitFactor = 1f;
                    if (split && hitQueue[k].KeyLane >= 3) splitFactor = -1f;
                    hitQueue[k].note.transform.localPosition = new Vector3(receptors[hitQueue[k].KeyLane - 1].transform.localPosition.x, Mathf.Max(Mathf.Min(splitFactor*PosFromSV(hitQueue[k].StartTime),100f),-100f), 0);
                    if (pull && hitQueue[k].EndTime >= hitQueue[k].StartTime)
                    {
                        float lnSize = Mathf.Max(Mathf.Min(Mathf.Abs(PosFromSV(hitQueue[k].EndTime) - PosFromSV(hitQueue[k].StartTime)), 25f),0);

                        if (splitFactor >= 1f) hitQueue[k].note.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().flipY = !upScroll;
                        else hitQueue[k].note.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().flipY = upScroll;
                        hitQueue[k].note.transform.Find("SliderMiddle").GetComponent<SpriteRenderer>().size = new Vector2(1f, splitFactor * - uScrollFloat * lnSize * (128f / columnSize));
                        hitQueue[k].note.transform.Find("SliderEnd").transform.localPosition = new Vector3(0f, splitFactor * uScrollFloat * lnSize, 0.1f);
                        if (lnSize == 0)
                        {
                            hitQueue[k].note.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().size = new Vector2(1f, 0.5f * Mathf.Max(splitFactor*(hitQueue[k].EndTime - curSongTime) / judgeTimes[5], 0));
                        }
                    }
                    if (curSongTime > Mathf.Max(hitQueue[k].StartTime,hitQueue[k].EndTime) + missTime)
                    {
                        //print("MISS");
                        RemoveNote(hitQueue[k].note);
                        hitQueue.RemoveAt(k);
                        k--;
                    }
                }
                //Move bars
                for (k = 0; k < Mathf.Min(barQueue.Count,maxNoteCount); k++)
                {
                    barQueue[k].note.transform.localPosition = new Vector3(0f, Mathf.Max(Mathf.Min(PosFromSV(barQueue[k].StartTime), 100f), -100f), 2f);
                    if (barQueue[k].note.transform.localPosition.y* -uScrollFloat > 10f) //Todo: Update miss offset later
                    {
                        Destroy(barQueue[k].note);
                        barQueue.RemoveAt(k);
                        k--;
                    }
                }

                
                //LN Check
                for (k=0;k<lnQueue.Count;k++)
                {
                    HitObject ln = lnQueue[k];
                    //Places LNs on top of the receptors

                    float splitFactor = 1f;
                    if (split && ln.KeyLane >= 3) splitFactor = -1f;
                    ln.note.transform.localPosition = receptors[ln.KeyLane - 1].transform.localPosition;
                    float lnSize = 0;
                    if (lnQueue[k].EndTime > curSongTime) lnSize = Mathf.Min((PosFromSV(lnQueue[k].EndTime) - hitYPos) * uScrollFloat, 25f, (PosFromSV(lnQueue[k].EndTime) - PosFromSV(lnQueue[k].StartTime)) * uScrollFloat);
                    if (splitFactor >= 1f) ln.note.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().flipY = !upScroll;
                    else ln.note.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().flipY = upScroll;
                    ln.note.transform.Find("SliderMiddle").GetComponent<SpriteRenderer>().size = new Vector2(1f, -uScrollFloat * splitFactor * lnSize * (128f / columnSize));
                    ln.note.transform.Find("SliderEnd").transform.localPosition = new Vector3(0f, uScrollFloat * splitFactor * lnSize, 0.1f);
                    if (lnSize == 0)
                    {
                        ln.note.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().size = new Vector2(1f, 0.5f * Mathf.Max((ln.EndTime-curSongTime)/judgeTimes[5],0));
                    }
                    
                    if (curSongTime - judgeTimes[5] > Mathf.Max(ln.EndTime,ln.StartTime))
                    {
                        HitObject removedLN = new HitObject();
                        removedLN.StartTime = (int)Mathf.Floor(curSongTime);
                        removedLN.EndTime = ln.EndTime;
                        removedLN.KeyLane = ln.KeyLane;

                        //GameObject newLn = Instantiate(ln.note, hitContainer.transform);
                        GameObject newLn = ln.note;
                        newLn.transform.Find("HitImage").GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
                        (newLn.transform.Find("SliderMiddle").gameObject).SetActive(false);
                        (newLn.transform.Find("SliderEnd").gameObject).SetActive(false);
                        removedLN.note = newLn;
                        offLNQueue.Add(removedLN);
                        lnQueue.Remove(ln);
                        k--;
                        print("LATE LN RELEASE");
                    }

                }
                
                
                //Ghost LN keys
                for (k = 0; k < offLNQueue.Count; k++)
                {
                    float splitFactor = 1f;
                    if (split && offLNQueue[k].KeyLane >= 3) splitFactor = -1f;
                    offLNQueue[k].note.transform.localPosition =  new Vector3( receptors[offLNQueue[k].KeyLane - 1].transform.localPosition.x, splitFactor*PosFromSV(offLNQueue[k].StartTime), 0);
                    if (curSongTime - missTime > Mathf.Max(offLNQueue[k].StartTime, offLNQueue[k].EndTime))
                    {
                        RemoveNote(offLNQueue[k].note);
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
            }
            else
            {
                if (songAudio.isPlaying) songAudio.Pause();
            }
        }

        GameObject InstantiateNote(GameObject hoo)
        {
            HitObject ho = noteQueue[0];
            if (hoo == null)
            {
                hoo = Instantiate(hitObjectTest, hitContainer.transform);
                hoo.transform.Find("HitImage").transform.localScale = Vector3.one * (128f / noteSize * (columnSize / 128f));
            }
            else
            {
                hoo.transform.Find("HitImage").gameObject.SetActive(true);
                hoo.transform.Find("HitImage").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
            }
            hoo.transform.Find("HitImage").transform.eulerAngles = new Vector3(0, 0, noteRot[ho.KeyLane - 1]); //Rotation

            float splitFactor = 1f;
            if (split && noteQueue[0].KeyLane >= 3) splitFactor = -1f;


            hoo.transform.localPosition = new Vector3(ho.KeyLane * (columnSize / 128f) - (columnSize / 128f * 2.5f), splitFactor*PosFromSV(ho.StartTime), 0);
            if (false ||(ho.EndTime > 0 && ho.EndTime > ho.StartTime))
            {
                
                float lnSize = Mathf.Min(Mathf.Abs(PosFromSV(ho.EndTime) - PosFromSV(ho.StartTime)),25f);

                hoo.transform.Find("SliderMiddle").transform.localScale = Vector3.one * (128f / noteSize * (columnSize / 128f));
                hoo.transform.Find("SliderEnd").transform.localScale = Vector3.one * (128f / noteSize * (columnSize / 128f));
                hoo.transform.Find("SliderMiddle").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
                hoo.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
                hoo.transform.Find("SliderMiddle").GetComponent<SpriteRenderer>().size = new Vector2(1f, splitFactor * - uScrollFloat * lnSize * (128f / columnSize));
                hoo.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().size = new Vector2(1f, 0.5f);
                hoo.transform.Find("SliderEnd").transform.localPosition = new Vector3(0f, splitFactor*uScrollFloat * lnSize, 0.1f);
                (hoo.transform.Find("SliderMiddle").gameObject).SetActive(true);
                (hoo.transform.Find("SliderEnd").gameObject).SetActive(true);
                if (splitFactor >= 1) hoo.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().flipY = !upScroll;
                else hoo.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().flipY = upScroll;
            }
            else
            {
                (hoo.transform.Find("SliderMiddle").gameObject).SetActive(false); ;
                (hoo.transform.Find("SliderEnd").gameObject).SetActive(false); ;
            }
            ho.note = hoo;
            hitQueue.Add(ho);
            noteQueue.RemoveAt(0);
            return hoo;
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
                    HitObject removedLN = new HitObject();
                    removedLN.note = lnQueue[curNote].note;
                    removedLN.KeyLane = lnQueue[curNote].KeyLane;
                    removedLN.EndTime = lnQueue[curNote].EndTime;
                    removedLN.StartTime = (int)Mathf.Floor(curSongTime);
                    removedLN.note.transform.Find("HitImage").GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    removedLN.note.transform.Find("SliderMiddle").GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    removedLN.note.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

                    offLNQueue.Add(removedLN);
                    lnQueue.RemoveAt(curNote);
                    print("EARLY LN RELEASE");
                }
                else if (closestTime > -judgeTimes[5] && closestTime < judgeTimes[5])
                {
                    RemoveNote(lnQueue[curNote].note); ;
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
                    RemoveNote(hitQueue[curNote].note);
                    hitQueue.RemoveAt(curNote);
                }
                //Create particles
                GameObject cp = Instantiate(circleParticleSystem, arrowParticles.transform);
                cp.transform.localPosition = receptors[kkey - 1].transform.localPosition + new Vector3(0,0,4f);
                hitBursts[kkey - 1].transform.GetComponent<NoteBurst>().started = true;
            }
        }

        void RemoveNote(GameObject curNote)
        {
            if (noteQueue.Count > 0)
            {
                InstantiateNote(curNote);
            }
            else
            {
                Destroy(curNote);
            }
        }
        
        float PosFromSV(float timePos)
        {
            float returnVal;
            if (!noSV)
            {
                ulong svPosTime = 0;
                int curPos = 0;
                if (timePos >= SvQueue[SvQueue.Count - 1].StartTime)
                {
                    curPos = SvQueue.Count - 1;
                }
                else
                {
                    for (int i = 0; i < SvQueue.Count - 1; i++)
                    {
                        if (timePos < SvQueue[i + 1].StartTime)
                        {
                            curPos = i;
                            break;
                        }
                    }
                }
                svPosTime = svCalc[curPos] + 15000 + (ulong)((timePos - SvQueue[curPos].StartTime) * SvQueue[curPos].Multiplier);
                //5000ms added for negative, since svPos is a ulong

                returnVal = ((float)(svPosTime - curSVPos) - 5000f) / 1000f * (float)scrollSpeed * (1 / songAudio.pitch);
            }
            else
            {
                returnVal = (timePos - curSongTime) / 1000f * (float)scrollSpeed * (1 / songAudio.pitch);
            }

            if (pull) returnVal = 2f * Mathf.Max(Mathf.Pow(returnVal, 0.6f),0) + Mathf.Min(timePos- curSongTime, 0f) / 1000f * (float)scrollSpeed * (1 / songAudio.pitch);
            return returnVal * uScrollFloat + hitYPos;
        }

    }

}
