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
        /*Classes/Gameobjects (public variables will be changed later)*/
        private QuaFile qFile;
        public GameObject hitObjectTest;
        public GameObject receptorBar;
        private GameObject[] receptors;
        public GameObject lightingBar;
        private GameObject[] hitLighting;
        public GameObject particleContainer; 
        public GameObject hitContainer; 
        public GameObject bgImage; 
        public GameObject bgMask;

        /*CONFIG VALUES*/
        private const float config_columnSize = 320; //temp
        private const int config_scrollSpeed = 25; //temp
        private const int config_receptorOffset = 100; //temp
        private const bool config_timingBars = true;
        private const bool config_upScroll = false; //true = upscroll, false = downscroll
        private KeyCode[] config_KeyBindings = new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.K, KeyCode.L };
        private const int config_offset = 0;
        private const int config_playStartDelayed = 3000; //waits 2 seconds until song starts

        private float screenHeight;
        private float screenWidth;

        private const int maxNoteCount = 64; //dont know if this should be config or not
        private const float config_PixelUnitSize = 128; //pixels to units. 128 pixels = 1 unit.

        private int[] judgeTimes = new int[6] { 16, 37, 70, 100, 124, 80 }; //OD9 judge times in ms (0,1,2,3,4), LN offset 5

        /*GAME MODS*/
        private const bool mod_noSV = false;
        private const bool mod_pull = false;
        private const bool mod_split = false;
        private const bool mod_spin = false;
        private const bool mod_shuffle = false;

        /*SKINNING VALUES*/
        public Sprite[] receptorSprite;
        public GameObject NoteHitParticle;
        public GameObject circleParticleSystem;
        public GameObject timingBar;

        /*SKIN.INI VALUES*/
        private const int skin_bgMaskBufferSize = 12; //Adds a buffer space to the bg mask (+8 pixels wide)
        private const int skin_noteBufferSpacing = 0; //Spaces notes 0 pixels a part
        private const int skin_timingBarPixelSize = 2;
        private const float skin_hitLightingScale = 4.0f; //Sets the scale of the hit lighting (relative to units)
        //private float[] skin_receptorRotations = new float[4] { -90f, 0f, 180f, 90f }; //Rotation of arrows if arrow skin is used
        private float[] skin_receptorRotations = new float[4] { 0,0,0,0}; //Rotation of arrows if arrow skin is used

        /*Referencing Values*/
        private const int missTime = 500; //after 500ms, the note will be removed
        private AudioSource songAudio;
        private List<TimingObject> SvQueue, timingQueue, barQueue;
        private List<NoteObject>[] hitQueue;
        private List<NoteObject> noteQueue, lnQueue, offLNQueue;
        private GameObject[] activeNotes;
        private ulong[] svCalc; //Stores SV position data for efficiency
        private float actualSongTime;
        private float curSongTime;
        private float scrollNegativeFactor = 1f;
        private bool[] keyDown;
        private float receptorYPos;
        private float[] receptorXPos, receptorXOffset, receptorSize;
        private float modInterval;
        private ulong curSVPos;
        private int curSVint;

        public void Start()
        {
            qFile = QuaParser.Parse("E:\\GitHub\\Quaver\\TestFiles\\Qua\\345.qua");
            if (!qFile.IsValidQua)
            {
                print("IS NOT VALID QUA FILE");
            }
            else if (qFile.IsValidQua)
            {
                //Copy + Convert to NoteObjects
                int i = 0;
                int j = 0;
                NoteObject newNote;
                noteQueue = new List<NoteObject>();
                for (i = 0; i < qFile.HitObjects.Count; i++)
                {
                    newNote = new NoteObject();
                    newNote.StartTime = qFile.HitObjects[i].StartTime;
                    newNote.EndTime = qFile.HitObjects[i].EndTime;
                    newNote.KeyLane = qFile.HitObjects[i].KeyLane;
                    noteQueue.Add(newNote);
                }

                TimingObject newTime;
                SvQueue = new List<TimingObject>();
                for (i = 0; i < qFile.SliderVelocities.Count; i++)
                {
                    newTime = new TimingObject();
                    newTime.StartTime = qFile.SliderVelocities[i].StartTime;
                    newTime.Multiplier = qFile.SliderVelocities[i].Multiplier;
                    SvQueue.Add(newTime);
                }


                timingQueue = new List<TimingObject>();
                for (i = 0; i < qFile.TimingPoints.Count; i++)
                {
                    newTime = new TimingObject();
                    newTime.StartTime = qFile.TimingPoints[i].StartTime;
                    newTime.BPM = qFile.TimingPoints[i].BPM;
                    timingQueue.Add(newTime);
                }

                //Declare Reference Values
                hitQueue = new List<NoteObject>[4];
                lnQueue = new List<NoteObject>();
                offLNQueue = new List<NoteObject>();
                barQueue = new List<TimingObject>();
                activeNotes = new GameObject[maxNoteCount];

                songAudio = transform.GetComponent<AudioSource>();
                screenHeight = Screen.height;
                screenWidth = Screen.width;
                receptorXPos = new float[4];
                receptorXOffset = new float[4];
                receptorSize = new float[4];
                keyDown = new bool[4];

                //TempValues
                float longestBpmTime = 0;
                int avgBpmPos = 0;
                float averageBpm = 100; //Change later


                //Declare Other Values
                curSongTime = -config_playStartDelayed;
                actualSongTime = -(float)config_playStartDelayed/1000f;
                curSVPos = (ulong)(-config_playStartDelayed + 10000f); //10000ms added since curSVPos is a uLong
                curSVint = 0;

                //Declare Receptor Values
                if (config_upScroll) scrollNegativeFactor = -1f;
                receptorYPos = scrollNegativeFactor * (config_columnSize / 256f + (float)config_receptorOffset / 100f - 10f);
                receptors = new GameObject[4];
                hitLighting = new GameObject[4];
                for (i = 0; i < 4; i++)
                {
                    receptors[i] = receptorBar.transform.Find("R"+(i+1)).gameObject;
                    receptorXPos[i] = (i - 1.5f) * ((config_columnSize + (float)skin_noteBufferSpacing) / config_PixelUnitSize);
                    if (i >= 2 && mod_split) receptors[i].transform.localPosition = new Vector3(receptorXPos[i], -receptorYPos, 1);
                    else receptors[i].transform.localPosition = new Vector3(receptorXPos[i], receptorYPos, 1);
                    receptors[i].transform.localScale = Vector3.one * (config_columnSize / (float)receptors[i].transform.GetComponent<SpriteRenderer>().sprite.rect.size.x);
                    receptors[i].transform.transform.eulerAngles = new Vector3(0, 0, skin_receptorRotations[i]);

                    hitQueue[i] = new List<NoteObject>();

                    hitLighting[i] = lightingBar.transform.Find("L" + (i + 1)).gameObject;
                    hitLighting[i].transform.localScale = new Vector3(receptors[i].transform.localScale.x,
                        scrollNegativeFactor*skin_hitLightingScale //*(config_PixelUnitSize / (float)hitLighting[i].transform.GetComponent<SpriteRenderer>().sprite.rect.size.y)
                        , 1f);
                    hitLighting[i].transform.localPosition = receptors[i].transform.localPosition + new Vector3(0, hitLighting[i].transform.localScale.y*2f, 1f);
                }

                //Set Skin Values
                bgImage.transform.localScale = Vector3.one * (20f*(config_PixelUnitSize/(float)bgImage.transform.GetComponent<SpriteRenderer>().sprite.rect.size.y)); //Scales the bg to y axis
                bgMask.transform.localScale = new Vector3(
                    ((float)(config_columnSize + skin_bgMaskBufferSize + skin_noteBufferSpacing) / config_PixelUnitSize) * 4f * (config_PixelUnitSize / (float)bgMask.transform.GetComponent<SpriteRenderer>().sprite.rect.size.x),
                    20f * (config_PixelUnitSize / (float)bgMask.transform.GetComponent<SpriteRenderer>().sprite.rect.size.y)
                    ,1f);
                //SNAP TO X AXIS bg.transform.localScale = Vector3.one * ((float)screenWidth/(float)screenHeight)*20f * (config_PixelUnitSize / (float)bg.transform.GetComponent<SpriteRenderer>().sprite.rect.size.x);

                //Calculate Average BPM of map
                if (!mod_noSV)
                {

                    if (timingQueue.Count > 1)
                    {
                        foreach (TimingObject tp in timingQueue)
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
                    else averageBpm = timingQueue[0].BPM;
                    print("AVERAGE BPM: " + averageBpm);

                    //Create and converts timing points to SV's
                    int hij = 0;
                    if (SvQueue.Count > 0)
                    {
                        for (j = 0; j < timingQueue.Count; j++)
                        {

                            if (timingQueue[j].StartTime < SvQueue[0].StartTime)
                            {
                                TimingObject newTp = new TimingObject();
                                newTp.StartTime = timingQueue[j].StartTime;
                                newTp.Multiplier = 1f;
                                SvQueue.Insert(0, newTp);
                            }
                            else if (timingQueue[j].StartTime >= SvQueue[SvQueue.Count - 1].StartTime)
                            {
                                if (timingQueue[j].StartTime - SvQueue[SvQueue.Count - 1].StartTime > 0.001f)
                                {
                                    TimingObject newTp = new TimingObject();
                                    newTp.StartTime = timingQueue[j].StartTime;
                                    newTp.Multiplier = 1f;
                                    SvQueue.Add(newTp);
                                }
                            }
                            else
                            {
                                for (i = hij; i < SvQueue.Count; i++)
                                {
                                    if (i + 1 < SvQueue.Count && timingQueue[j].StartTime < SvQueue[i + 1].StartTime)
                                    {
                                        if (Mathf.Abs(timingQueue[j].StartTime - SvQueue[i + 1].StartTime) > 0.001f)
                                        {
                                            TimingObject newTp = new TimingObject();
                                            newTp.StartTime = timingQueue[j].StartTime;
                                            newTp.Multiplier = 1f;
                                            SvQueue.Add(newTp);
                                            i++;
                                            hij = i;
                                        }
                                    }
                                    else break;
                                }
                            }
                        }
                    }

                    //Normalizes SV's in between each BPM change interval
                    hij = 0;
                    SvQueue.Sort(delegate (TimingObject p1, TimingObject p2) { return p1.StartTime.CompareTo(p2.StartTime); });
                    if (timingQueue.Count > 1)
                    {
                        for (i = 0; i < timingQueue.Count; i++)
                        {
                            //print("c" + i);
                            for (j = hij; j < SvQueue.Count; j++)
                            {
                                if (SvQueue[j].StartTime >= timingQueue[i].StartTime - 0.2f)
                                {
                                    hij = j;
                                    TimingObject newSV = new TimingObject();
                                    newSV.StartTime = SvQueue[j].StartTime;
                                    newSV.Multiplier = Mathf.Min(SvQueue[j].Multiplier * timingQueue[i].BPM / averageBpm, 512f);
                                    SvQueue.RemoveAt(j);
                                    SvQueue.Insert(j, newSV);
                                    break;
                                }
                            }
                        }
                    }
                }
                //IF NO_SV is disabled
                else
                {
                    TimingObject newTp = new TimingObject();
                    newTp.StartTime = 0;
                    newTp.Multiplier = 1f;
                    SvQueue.Add(newTp);
                }

                //Calculates SV for efficiency
                svCalc = new ulong[SvQueue.Count];
                ulong svPosTime = 0;
                svCalc[0] = 0;
                for (i = 0; i < SvQueue.Count; i++)
                {
                    if (i + 1 < SvQueue.Count)
                    {
                        ulong templ = (ulong)((SvQueue[i + 1].StartTime - SvQueue[i].StartTime) * SvQueue[i].Multiplier);
                        if (templ > 10000000) templ = 10000000;
                        svPosTime += templ;
                        svCalc[i + 1] = svPosTime;
                    }
                    else break;
                }

                //Create Timing bars
                if (!mod_split && config_timingBars) { 
                    float curBarTime = 0;
                    for (i = 0; i < timingQueue.Count; i++)
                    {
                        curBarTime = timingQueue[i].StartTime;

                        if (barQueue.Count > 0 && barQueue[0].StartTime + 2 > curBarTime) barQueue.RemoveAt(0);
                        curBarTime += 1000f * 4f * 60f / (timingQueue[i].BPM);
                        TimingObject curTiming;

                        if (i + 1 < timingQueue.Count)
                        {
                            while (curBarTime < timingQueue[i + 1].StartTime)
                            {
                                curTiming = new TimingObject();
                                curTiming.StartTime = (int)(curBarTime);
                                barQueue.Add(curTiming);
                                curBarTime += 1000f * 4f * 60f / (timingQueue[i].BPM);
                            }
                        }
                        else
                        {
                            while (curBarTime < songAudio.clip.length * 1000f)
                            {
                                curTiming = new TimingObject();
                                curTiming.StartTime = (int)(curBarTime);
                                barQueue.Add(curTiming);
                                curBarTime += 1000f * 4f * 60f / (timingQueue[i].BPM);
                            }
                        }
                    }

                    //Create starting bars
                    List<TimingObject> tempBars = new List<TimingObject>();
                    for (i = 0; i < barQueue.Count; i++)
                    {
                        TimingObject hoo = new TimingObject();
                        GameObject curBar = Instantiate(timingBar, hitContainer.transform);
                        curBar.transform.localScale = new Vector3(((float)(config_columnSize + skin_bgMaskBufferSize + skin_noteBufferSpacing) / config_PixelUnitSize) * 4f * (config_PixelUnitSize / (float)curBar.transform.GetComponent<SpriteRenderer>().sprite.rect.size.x),
                            (config_PixelUnitSize / (float)curBar.transform.GetComponent<SpriteRenderer>().sprite.rect.size.y) * ((float)skin_timingBarPixelSize/config_PixelUnitSize)
                            , 1f);
                        curBar.transform.localPosition = new Vector3(0f, PosFromSV(barQueue[i].StartTime), 2f);

                        hoo.StartTime = barQueue[i].StartTime;
                        hoo.TimingBar = curBar;
                        tempBars.Add(hoo);
                    }

                    barQueue = new List<TimingObject>(tempBars);
                }

                //Create starting notes
                for (i = 0; i < maxNoteCount; i++)
                {
                    if (noteQueue.Count > 0) activeNotes[i] = InstantiateNote(null);
                    else break;
                }

                //Plays the song, but delayed
                songAudio.PlayDelayed((float)config_playStartDelayed/1000f);
                print("TOTAL SV CHANGES: " + SvQueue.Count);

                loaded = true;

            }
        }

        public void Update()
        {
            if (qFile.IsValidQua && isActive)
            {
                //Check what to do after unpaused
                if (!songAudio.isPlaying) songAudio.UnPause();


                //Song Time Calculation (ms)
                if (actualSongTime < 0) actualSongTime += Time.deltaTime;
                else actualSongTime = ((songAudio.time)+(actualSongTime + Time.deltaTime))/2f;
                curSongTime = actualSongTime * 1000f - config_offset;

                //Calculates curSV Position
                int k = 0; int j = 0;

                if (curSongTime >= SvQueue[SvQueue.Count - 1].StartTime)
                {
                    curSVint = SvQueue.Count - 1;
                }
                else if (curSVint < SvQueue.Count-2)
                {
                    for (j= curSVint; j < SvQueue.Count - 1; j++)
                    {
                        if (curSongTime > SvQueue[curSVint + 1].StartTime) curSVint++;
                        else break;
                    }

                }
                curSVPos = svCalc[curSVint] + (ulong)((float)((curSongTime) - (SvQueue[curSVint].StartTime)) * SvQueue[curSVint].Multiplier + 10000);

                //Receptor Modifiers
                if (mod_spin || mod_shuffle)
                {
                    for (k=0;k<4;k++)
                    {
                        ModifyReceptors(k);
                    }

                }

                //Move bars
                if (config_timingBars && !mod_split)
                {
                    if (barQueue.Count > 0 && curSongTime > barQueue[0].StartTime + missTime)
                    {
                        Destroy(barQueue[0].TimingBar);
                        barQueue.RemoveAt(0);
                    }
                    else
                    {
                        for (k = 0; k < Mathf.Min(barQueue.Count, maxNoteCount); k++)
                        {
                            barQueue[k].TimingBar.transform.localPosition = new Vector3(0f, PosFromSV(barQueue[k].StartTime), 2f);
                        }
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
                            hitQueue[j][k].HitSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                            hitQueue[j][k].SliderEndSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                            hitQueue[j][k].SliderMiddleSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);

                            offLNQueue.Add(hitQueue[j][k]);
                            hitQueue[j].RemoveAt(k);
                            k--;
                        }
                        else
                        {
                            SetNotePos(hitQueue[j][k], 1);
                        }
                    }
                }
                //LN Check
                for (k=0;k<lnQueue.Count;k++)
                {
                    if (curSongTime > Mathf.Max(lnQueue[k].StartTime, lnQueue[k].EndTime) + judgeTimes[4])
                    {
                        print("LATE LN RELEASE");

                        NoteObject newNote = new NoteObject();

                        newNote.StartTime = (int)curSongTime;
                        newNote.EndTime = lnQueue[k].EndTime;
                        newNote.KeyLane = lnQueue[k].KeyLane;

                        newNote.HitSet = lnQueue[k].HitSet;
                        newNote.HitSprite = lnQueue[k].HitSprite;
                        newNote.SliderEndSprite = lnQueue[k].SliderEndSprite;
                        newNote.SliderMiddleSprite = lnQueue[k].SliderMiddleSprite;
                        newNote.SliderEndObject = lnQueue[k].SliderEndObject;
                        newNote.SliderMiddleObject = lnQueue[k].SliderMiddleObject;

                        newNote.HitSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                        newNote.SliderEndSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                        newNote.SliderMiddleSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                        newNote.SliderEndObject.SetActive(false);
                        newNote.SliderMiddleObject.SetActive(false);

                        offLNQueue.Add(newNote);
                        lnQueue.RemoveAt(k);
                        k--;
                    }
                    else
                    {
                        SetNotePos(lnQueue[k], 2);
                    }
                }

                //Ghost LN keys
                if (offLNQueue.Count > 0) {
                    for (k = 0; k < offLNQueue.Count; k++)
                    {
                        if (curSongTime > Mathf.Max(offLNQueue[k].StartTime, offLNQueue[k].EndTime) + missTime)
                        {
                            RemoveNote(offLNQueue[k].HitSet);
                            offLNQueue.RemoveAt(k);
                            k--;
                        }
                        else
                        {
                            SetNotePos(offLNQueue[k], 3);
                        }
                    }
                }
                //Key Press Check
                for (k = 0; k < 4; k++)
                {
                    if (!keyDown[k])
                    {
                        if (Input.GetKeyDown(config_KeyBindings[k]))
                        {
                            keyDown[k] = true;
                            receptors[k].transform.localScale = Vector3.one * (config_columnSize / config_PixelUnitSize) * 1.1f;
                            receptors[k].transform.GetComponent<SpriteRenderer>().sprite = receptorSprite[1];
                            hitLighting[k].SetActive(true); //temp
                            JudgeNote(k+1, curSongTime);
                        }
                    }
                    else
                    {
                        if (Input.GetKeyUp(config_KeyBindings[k]))
                        {
                            keyDown[k] = false;
                            receptors[k].transform.GetComponent<SpriteRenderer>().sprite = receptorSprite[0];
                            hitLighting[k].SetActive(false); //temp
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
            NoteObject ho = noteQueue[0];
            if (hoo == null)
            {
                hoo = Instantiate(hitObjectTest, hitContainer.transform);
                hoo.transform.Find("HitImage").transform.localScale = Vector3.one * (config_columnSize/(float)hoo.transform.Find("HitImage").transform.GetComponent<SpriteRenderer>().sprite.rect.size.x); //config_columnSize
                hoo.transform.Find("SliderMiddle").transform.localScale = Vector3.one * (config_columnSize / (float)hoo.transform.Find("SliderMiddle").transform.GetComponent<SpriteRenderer>().sprite.rect.size.x);
                hoo.transform.Find("SliderEnd").transform.localScale = Vector3.one * (config_columnSize / (float)hoo.transform.Find("SliderEnd").transform.GetComponent<SpriteRenderer>().sprite.rect.size.x);
            }

            ho.HitSet = hoo;
            ho.HitNote = hoo.transform.Find("HitImage").gameObject;
            ho.SliderMiddleObject = hoo.transform.Find("SliderMiddle").gameObject;
            ho.SliderEndObject = hoo.transform.Find("SliderEnd").gameObject;

            ho.HitSprite = hoo.transform.Find("HitImage").GetComponent<SpriteRenderer>();
            ho.SliderMiddleSprite = hoo.transform.Find("SliderMiddle").GetComponent<SpriteRenderer>();
            ho.SliderEndSprite = hoo.transform.Find("SliderEnd").GetComponent<SpriteRenderer>();

            ho.HitNote.SetActive(true);
            ho.HitSprite.color = new Color(1f, 1f, 1f, 1f);


            if (ho.EndTime == 0)
            {
                ho.SliderMiddleObject.gameObject.SetActive(false); ;
                ho.SliderEndObject.gameObject.SetActive(false); ;
            }

            ho.HitNote.transform.eulerAngles = new Vector3(0, 0, skin_receptorRotations[ho.KeyLane - 1]); //Rotation
            SetNotePos(ho, 0);
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

                    NoteObject newNote = new NoteObject();

                    newNote.StartTime = (int)curSongTime;
                    newNote.EndTime = lnQueue[curNote].EndTime;
                    newNote.KeyLane = lnQueue[curNote].KeyLane;

                    newNote.HitSet = lnQueue[curNote].HitSet;
                    newNote.HitSprite = lnQueue[curNote].HitSprite;
                    newNote.SliderEndSprite = lnQueue[curNote].SliderEndSprite;
                    newNote.SliderMiddleSprite = lnQueue[curNote].SliderMiddleSprite;
                    newNote.SliderEndObject = lnQueue[curNote].SliderEndObject;
                    newNote.SliderMiddleObject = lnQueue[curNote].SliderMiddleObject;

                    newNote.HitSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    newNote.SliderMiddleSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    newNote.SliderEndSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);

                    offLNQueue.Add(newNote);
                    lnQueue.RemoveAt(curNote);
                    print("EARLY LN RELEASE");
                }
                else if (closestTime > -judgeTimes[5] && closestTime < judgeTimes[5])
                {
                    RemoveNote(lnQueue[curNote].HitSet); ;
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
                        RemoveNote(hitQueue[kkey - 1][0].HitSet);
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
            if (noteQueue.Count > 0) InstantiateNote(curNote);
            else Destroy(curNote);
        }


        // FOR SWAN MVOE THIS OUT
        // FOR SWAN MVOE THIS OUT
        // FOR SWAN MVOE THIS OUT
        // FOR SWAN MVOE THIS OUT





        // Modifies the receptors if visual mods are on.
        void ModifyReceptors(int k)
        {
            //Spin Receptors
            if (mod_spin) skin_receptorRotations[k] += 0.5f;
            if (skin_receptorRotations[k] >= 360) skin_receptorRotations[k] -= 360;

            //XOff Receptors
            if (mod_shuffle)
            {
                if (k == 0 || k == 2) receptorXOffset[k] = (Mathf.Pow(Mathf.Sin(curSongTime * (Mathf.PI / 180f) / 10f), 3) + 1f) * config_columnSize / config_PixelUnitSize / 2f;
                else receptorXOffset[k] = -(Mathf.Pow(Mathf.Sin(curSongTime * (Mathf.PI / 180f) / 10f), 3) + 1f) * config_columnSize / config_PixelUnitSize / 2f;
            }

            //Set Receptor Transform
            if (mod_spin) receptors[k].transform.localEulerAngles = new Vector3(0, 0, skin_receptorRotations[k]);
            if (mod_shuffle) receptors[k].transform.localPosition = new Vector3(receptorXPos[k] + receptorXOffset[k], receptorYPos, 1);
            receptors[k].transform.localScale = Vector3.one * (receptors[k].transform.localScale.x + ((config_columnSize / config_PixelUnitSize) - receptors[k].transform.localScale.x) / 3f);
        }

        //Sets the GameObject position of the note
        void SetNotePos(NoteObject curNote, int lnMode)
        {
            float StartTime = curNote.StartTime;
            if (lnMode == 2) StartTime = curSongTime;
            float splitFactor = 1f;
            if (mod_split && curNote.KeyLane >= 3) splitFactor = -1f;
            if (lnMode != 2 || mod_shuffle)
                curNote.HitSet.transform.localPosition =new Vector3(receptorXPos[curNote.KeyLane - 1] + receptorXOffset[curNote.KeyLane - 1], splitFactor * PosFromSV(StartTime), 0);
            else curNote.HitSet.transform.localPosition = new Vector3(receptorXPos[curNote.KeyLane - 1] + receptorXOffset[curNote.KeyLane - 1], receptorYPos * splitFactor, 0);
            if (mod_spin) curNote.HitSprite.transform.eulerAngles = new Vector3(0, 0, skin_receptorRotations[curNote.KeyLane - 1]);
            if ((lnMode != 1 || mod_pull) && lnMode != 3 && curNote.EndTime > 0 && curNote.EndTime > StartTime)
            {

                float lnSize = splitFactor * Mathf.Min(Mathf.Abs(PosFromSV(curNote.EndTime) - PosFromSV(StartTime)), 50f);

                curNote.SliderMiddleSprite.size = new Vector2(1f, -scrollNegativeFactor * lnSize * (config_PixelUnitSize / config_columnSize));
                curNote.SliderEndObject.transform.localPosition = new Vector3(0f, scrollNegativeFactor * lnSize, 0.1f);

                if (lnMode == 0)
                {
                    curNote.SliderMiddleObject.transform.localScale = Vector3.one * (config_columnSize / config_PixelUnitSize);
                    curNote.SliderMiddleSprite.color = new Color(1f, 1f, 1f, 1f);
                    curNote.SliderMiddleObject.SetActive(true);

                    curNote.SliderEndSprite.color = new Color(1f, 1f, 1f, 1f);
                    curNote.SliderEndObject.transform.localScale = Vector3.one * (config_columnSize / config_PixelUnitSize);
                    curNote.SliderEndObject.SetActive(true);
                    if (splitFactor >= 1) curNote.SliderEndSprite.flipY = !config_upScroll;
                    else curNote.SliderEndSprite.flipY = config_upScroll;
                }
            }
        }

        //Calculates the position from SV
        float PosFromSV(float timePos)
        {
            float returnVal;
            if (Mathf.Abs(timePos- curSongTime) >= 0.01)
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
                    svPosTime = svCalc[curPos]+ (ulong)(15000 + (timePos - SvQueue[curPos].StartTime) * SvQueue[curPos].Multiplier);
                    //10000ms added for negative, since svPos is a ulong

                    returnVal = (float)(svPosTime - curSVPos - 5000f) / 1000f * (float)config_scrollSpeed * (1 / songAudio.pitch);
                }
                else returnVal = (timePos - curSongTime) / 1000f * (float)config_scrollSpeed * (1 / songAudio.pitch);
            }
            else returnVal = 0;

            if (mod_pull) returnVal = 2f * Mathf.Max(Mathf.Pow(returnVal, 0.6f),0) + Mathf.Min(timePos- curSongTime, 0f) / 1000f * (float)config_scrollSpeed * (1 / songAudio.pitch);

            return returnVal * scrollNegativeFactor + receptorYPos;
        }

    }




    //Dont rmeove below this
}
