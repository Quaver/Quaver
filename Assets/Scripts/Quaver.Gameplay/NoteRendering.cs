using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Qua;
using Quaver.Graphics;
using Quaver.Main;

namespace Quaver.Gameplay
{
    public class NoteRendering : GameState
    {
        /*Classes/Gameobjects*/
        private QuaFile qFile;
        public GameObject hitObjectTest;
        public GameObject receptorBar;
        private GameObject[] receptors;
        public GameObject particleContainer;
        public GameObject hitContainer;

        /*CONFIG VALUES*/
        private const float noteSize = 128; //temp, size of noteskin in pixels
        private const float columnSize = 230; //temp
        private const int scrollSpeed = 22; //temp
        private const int receptorOffset = 605; //temp
        private const bool upScroll = true; //true = upscroll, false = downscroll
        private KeyCode[] maniaKeyBindings = new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.K, KeyCode.L };
        private const int maxNoteCount = 60; //temp
        private const int playerOffset = 0;
        private const int osuOffset = 0;

        /*GAME MODS*/
        private const bool mod_noSV = false;
        private const bool mod_pull = false;
        private const bool mod_split = false;
        private const bool mod_spin = false;
        private const bool mod_shuffle = true;

        /*SKINNING VALUES*/
        public Sprite[] receptorSprite;
        public GameObject NoteHitParticle;
        public GameObject circleParticleSystem;
        public GameObject timingBar;

        /*Referencing Values*/
        private const int missTime = 500; //after 500ms, the note will be removed
        private AudioSource songAudio;
        private List<SliderVelocity> SvQueue;
        private List<TimingPoint> timingQueue;
        private List<HitObject>[] hitQueue;
        private List<HitObject> noteQueue,barQueue, lnQueue, offLNQueue;
        private GameObject[] activeNotes, activeBars;
        private ulong[] svCalc; //Stores SV position data for efficiency
        private float actualSongTime;
        private float curSongTime;
        private const float waitTilPlay = 0.5f; //waits 2 seconds until song starts
        private float uScrollFloat = 1f;
        private bool[] keyDown = new bool[4];
        private float averageBpm;
        private int[] judgeTimes = new int[6] { 16, 37, 70, 100, 124, 80}; //OD9 judge times in ms (0,1,2,3,4), LN offset 5
        private float receptorYPos;
        private float[] receptorXPos = new float[4];
        private float[] receptorXOffset = new float[4];
        private float[] receptorRot = new float[4] { -90f, 0f, 180f, 90f }; //Rotation of arrows if arrow skin is used
        private float modInterval;
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
                hitQueue = new List<HitObject>[4];
                lnQueue = new List<HitObject>();
                offLNQueue = new List<HitObject>();
                barQueue = new List<HitObject>();
                activeBars = new GameObject[maxNoteCount];
                activeNotes = new GameObject[maxNoteCount];

                averageBpm = 120f; //Change later
                songAudio = transform.GetComponent<AudioSource>();

                //TempValues
                float longestBpmTime = 0;
                int avgBpmPos = 0;


                //Declare Other Values
                curSongTime = -waitTilPlay;
                actualSongTime = -waitTilPlay;
                curSVPos = (ulong)(-waitTilPlay * 1000f + 10000f); //10000ms added since curSVPos is a uLong

                //Declare Receptor Values
                if (upScroll) uScrollFloat = -1f;
                receptorYPos = -uScrollFloat * receptorOffset / 100f + uScrollFloat * (columnSize / 256f);
                receptors = new GameObject[4];
                receptors[0] = receptorBar.transform.Find("R1").gameObject;
                receptors[1] = receptorBar.transform.Find("R2").gameObject;
                receptors[2] = receptorBar.transform.Find("R3").gameObject;
                receptors[3] = receptorBar.transform.Find("R4").gameObject;

                int i = 0;
                int j = 0;

                for (i = 0; i < 4; i++)
                {
                    receptorXPos[i] = (i + 1) * (columnSize / noteSize) - (columnSize / noteSize * 2.5f);
                    if (i >= 2 && mod_split) receptors[i].transform.localPosition = new Vector3(receptorXPos[i], -receptorYPos, 0);
                    else receptors[i].transform.localPosition = new Vector3((i - 1.5f) * (columnSize / noteSize), receptorYPos, 0);
                    receptors[i].transform.localScale = Vector3.one * (columnSize / noteSize);
                    receptors[i].transform.transform.eulerAngles = new Vector3(0, 0, receptorRot[i]);

                    hitQueue[i] = new List<HitObject>();
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

                //Create and converts timing points to SV's
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
                if (!mod_split) { 
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
                                curTiming.StartTime = (int)(curBarTime);
                                barQueue.Add(curTiming);
                                curBarTime += 1000f * 4f * 60f / (timingQueue[i].BPM);
                            }
                        }
                        else
                        {
                            while (curBarTime < songAudio.clip.length * 1000f)
                            {
                                curTiming = new HitObject();
                                curTiming.StartTime = (int)(curBarTime);
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
                int j = 0;
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

                //Receptor Modifiers
                if (mod_spin || mod_shuffle)
                {
                    for (k=0;k<4;k++)
                    {
                        ModifyReceptors(k);
                    }

                }

                //Move bars
                for (k = 0; k < Mathf.Min(barQueue.Count, maxNoteCount); k++)
                {
                    if (barQueue[k].note.transform.localPosition.y * -uScrollFloat > 10f) //Todo: Update miss offset later
                    {
                        Destroy(barQueue[k].note);
                        barQueue.RemoveAt(k);
                        k--;
                    }
                    else
                    {
                        barQueue[k].note.transform.localPosition = new Vector3(0f, Mathf.Max(Mathf.Min(PosFromSV(barQueue[k].StartTime), 100f), -100f), 2f);
                    }
                }

                //Move notes and check if miss
                for (j = 0; j < 4; j++)
                {
                    for (k = 0; k < hitQueue[j].Count; k++)
                    {
                        if (curSongTime > hitQueue[j][k].StartTime + judgeTimes[4])
                        {
                            print("MISS");
                            hitQueue[j][k].note.transform.Find("HitImage").GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
                            hitQueue[j][k].note.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
                            hitQueue[j][k].note.transform.Find("SliderMiddle").GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
                            hitQueue[j][k].note.transform.Find("SliderMiddle").gameObject.SetActive(false); ;
                            hitQueue[j][k].note.transform.Find("SliderEnd").gameObject.SetActive(false); ;
                            offLNQueue.Add(hitQueue[j][k]);
                            hitQueue[j].RemoveAt(k);
                            k--;
                        }
                        else
                        {
                            SetNotePos(hitQueue[j][k].note, hitQueue[j][k].KeyLane, hitQueue[j][k].StartTime, hitQueue[j][k].EndTime, 1);
                        }
                    }
                }
                //LN Check
                for (k=0;k<lnQueue.Count;k++)
                {
                    if (curSongTime > Mathf.Max(lnQueue[k].StartTime, lnQueue[k].EndTime) + judgeTimes[4])
                    {
                        print("LATE LN RELEASE");
                        lnQueue[k].note.transform.Find("HitImage").GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
                        lnQueue[k].note.transform.Find("SliderMiddle").gameObject.SetActive(false); ;
                        lnQueue[k].note.transform.Find("SliderEnd").gameObject.SetActive(false); ;

                        HitObject removedLN = new HitObject();
                        removedLN.EndTime = lnQueue[k].EndTime;
                        removedLN.StartTime = (int)(curSongTime);
                        removedLN.KeyLane = lnQueue[k].KeyLane;
                        removedLN.note = lnQueue[k].note;


                        offLNQueue.Add(removedLN);
                        lnQueue.RemoveAt(k);
                        k--;
                    }
                    else
                    {
                        SetNotePos(lnQueue[k].note, lnQueue[k].KeyLane, curSongTime, lnQueue[k].EndTime, 2);
                    }
                }
                
                
                //Ghost LN keys
                if (offLNQueue.Count > 0) {
                    for (k = 0; k < offLNQueue.Count; k++)
                    {
                        if (curSongTime > Mathf.Max(offLNQueue[k].StartTime, offLNQueue[k].EndTime) + missTime)
                        {
                            RemoveNote(offLNQueue[k].note);
                            offLNQueue.RemoveAt(k);
                            k--;
                        }
                        else
                        {
                            SetNotePos(offLNQueue[k].note, offLNQueue[k].KeyLane, offLNQueue[k].StartTime, offLNQueue[k].EndTime, 3);
                        }
                    }
                }
                //Key Press Check
                for (k = 0; k < 4; k++)
                {
                    if (!keyDown[k])
                    {
                        if (Input.GetKeyDown(maniaKeyBindings[k]))
                        {
                            keyDown[k] = true;
                            receptors[k].transform.localScale = Vector3.one * (columnSize / noteSize) * 1.1f;
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
                hoo.transform.Find("HitImage").transform.localScale = Vector3.one * (columnSize / noteSize);
            }
            else
            {
                hoo.transform.Find("HitImage").gameObject.SetActive(true);
                hoo.transform.Find("HitImage").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
            }
            if (ho.EndTime == 0)
            {
                (hoo.transform.Find("SliderMiddle").gameObject).SetActive(false); ;
                (hoo.transform.Find("SliderEnd").gameObject).SetActive(false); ;
            }

            hoo.transform.Find("HitImage").transform.eulerAngles = new Vector3(0, 0, receptorRot[ho.KeyLane - 1]); //Rotation
            SetNotePos(hoo, ho.KeyLane,ho.StartTime,ho.EndTime, 0);
            ho.note = hoo;
            hitQueue[ho.KeyLane-1].Add(ho);
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
                    removedLN.StartTime = (int)(curSongTime);
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
            if (hitQueue[kkey - 1].Count > 0 && hitQueue[kkey - 1][0].StartTime - curSongTime < judgeTimes[4])
            {
                float closestTime = hitQueue[kkey - 1][0].StartTime - curSongTime;
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
                    if (hitQueue[kkey - 1][0].EndTime > 0)
                    {
                        lnQueue.Add(hitQueue[kkey - 1][0]);
                        hitQueue[kkey-1].RemoveAt(0);
                    }
                    else
                    {
                        RemoveNote(hitQueue[kkey - 1][0].note);
                        hitQueue[kkey-1].RemoveAt(0);
                    }
                    //Create particles
                    GameObject cp = Instantiate(circleParticleSystem, particleContainer.transform);
                    cp.transform.localPosition = receptors[kkey - 1].transform.localPosition + new Vector3(0, 0, 4f);

                    GameObject hb = Instantiate(NoteHitParticle, particleContainer.transform);
                    hb.transform.localPosition = receptors[kkey - 1].transform.localPosition + new Vector3(0, 0, -2f);
                    hb.transform.eulerAngles = receptors[kkey - 1].transform.eulerAngles;
                    hb.transform.localScale = receptors[kkey - 1].transform.localScale;
                    hb.GetComponent<NoteBurst>().startSize = hb.transform.localScale.x - 0.05f;
                    hb.GetComponent<NoteBurst>().burstSize = 0.1f;
                    hb.GetComponent<NoteBurst>().burstLength = 0.35f;
                }
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



        // FOR SWAN MVOE THIS OUT
        // FOR SWAN MVOE THIS OUT
        // FOR SWAN MVOE THIS OUT
        // FOR SWAN MVOE THIS OUT





        // Modifies the receptors if visual mods are on.
        void ModifyReceptors(int k)
        {
            //Spin Receptors
            if (mod_spin) receptorRot[k] += 0.5f;
            if (receptorRot[k] >= 360) receptorRot[k] -= 360;

            //XOff Receptors
            if (mod_shuffle)
            {
                if (k == 0 || k == 2) receptorXOffset[k] = (Mathf.Pow(Mathf.Sin(curSongTime * (Mathf.PI / 180f) / 10f), 3) + 1f) * columnSize / noteSize / 2f;
                else receptorXOffset[k] = -(Mathf.Pow(Mathf.Sin(curSongTime * (Mathf.PI / 180f) / 10f), 3) + 1f) * columnSize / noteSize / 2f;
            }

            //Set Receptor Transform
            if (mod_spin) receptors[k].transform.localEulerAngles = new Vector3(0, 0, receptorRot[k]);
            if (mod_shuffle) receptors[k].transform.localPosition = new Vector3(receptorXPos[k] + receptorXOffset[k], receptorYPos, 0f);
            receptors[k].transform.localScale = Vector3.one * (receptors[k].transform.localScale.x + ((columnSize / noteSize) - receptors[k].transform.localScale.x) / 3f);
        }

        //Sets the GameObject position of the note
        void SetNotePos(GameObject curNote, int KeyLane, float StartTime, float EndTime, int lnMode)
        {
            float splitFactor = 1f;
            if (mod_split && noteQueue[0].KeyLane >= 3) splitFactor = -1f;
            if (lnMode != 2 || mod_shuffle) curNote.transform.localPosition = new Vector3(receptorXPos[KeyLane - 1] + receptorXOffset[KeyLane - 1], splitFactor * PosFromSV(StartTime), 0);
            else curNote.transform.localPosition = new Vector3((KeyLane - 2.5f) * (columnSize / noteSize), receptorYPos * splitFactor, 0);
            if (mod_spin) curNote.transform.Find("HitImage").transform.eulerAngles = new Vector3(0, 0, receptorRot[KeyLane - 1]);
            if ((lnMode != 1 || mod_pull) && lnMode != 3 && EndTime > 0 && EndTime > StartTime)
            {

                float lnSize = splitFactor * Mathf.Min(Mathf.Abs(PosFromSV(EndTime) - PosFromSV(StartTime)), 25f);

                curNote.transform.Find("SliderMiddle").transform.localScale = Vector3.one * (columnSize / noteSize);
                curNote.transform.Find("SliderMiddle").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
                curNote.transform.Find("SliderMiddle").GetComponent<SpriteRenderer>().size = new Vector2(1f, -uScrollFloat * lnSize * (noteSize / columnSize));
                curNote.transform.Find("SliderMiddle").gameObject.SetActive(true);

                curNote.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
                curNote.transform.Find("SliderEnd").transform.localScale = Vector3.one * (columnSize / noteSize);
                curNote.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().size = new Vector2(1f, 0.5f);
                curNote.transform.Find("SliderEnd").transform.localPosition = new Vector3(0f, uScrollFloat * lnSize, 0.1f);
                curNote.transform.Find("SliderEnd").gameObject.SetActive(true);
                if (splitFactor >= 1) curNote.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().flipY = !upScroll;
                else curNote.transform.Find("SliderEnd").GetComponent<SpriteRenderer>().flipY = upScroll;
            }
        }

        //Calculates the position from SV
        float PosFromSV(float timePos)
        {
            float returnVal;
            if (Mathf.Abs(timePos- curSongTime) >= 0.0001)
            {
                if (!mod_noSV)
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
            }
            else
            {
                returnVal = 0;
            }

            if (mod_pull) returnVal = 2f * Mathf.Max(Mathf.Pow(returnVal, 0.6f),0) + Mathf.Min(timePos- curSongTime, 0f) / 1000f * (float)scrollSpeed * (1 / songAudio.pitch);
            return returnVal * uScrollFloat + receptorYPos;
        }

    }




    //Dont rmeove below this
}
