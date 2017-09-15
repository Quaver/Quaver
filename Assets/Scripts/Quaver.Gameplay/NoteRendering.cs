// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Qua;
using Quaver.Graphics;
using Quaver.Main;
using Quaver.Audio;

namespace Quaver.Gameplay
{
    public class NoteRendering : GameState
    {
        /*Classes/Gameobjects (public variables will be changed later)*/
        public GameObject hitObjectTest;
        public GameObject receptorBar;
        private GameObject[] _receptors;
        public GameObject lightingBar;
        private GameObject[] _hitLighting;
        public GameObject particleContainer;
        public GameObject hitContainer;
        public GameObject bgMask;

        /*GSM REFERENCES (Anything that references the game state manager)*/
        private QuaFile _qFile;
        private AudioSource _songAudio;

        /*CONFIG VALUES*/
        private int _config_scrollSpeed;
        private bool _config_timingBars;
        private bool _config_upScroll;
        private KeyCode[] _config_KeyBindings;
        private int _config_offset;

        private const int config_playStartDelayed = 1000; //delays 1 second before song starts
        private const int maxNoteCount = 64; //dont know if this should be config or not
        private const float config_PixelUnitSize = 128; //pixels to units. 128 pixels = 1 unit.

        private int[] _judgeTimes = new int[6] { 16, 37, 70, 100, 124, 80 }; //OD9 judge times in ms (0,1,2,3,4), LN offset 5

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
        private const int skin_columnSize = 250;
        private const int skin_receptorYOffset = 50; //Sets the receptor's y position offset, relative to the edge of the screen.
        //private float[] skin_receptorRotations = new float[4] { -90f, 0f, 180f, 90f }; //Rotation of arrows if arrow skin is used
        private float[] _skin_receptorRotations = new float[4] { 0, 0, 0, 0 }; //Rotation of arrows if arrow skin is used

        /*Referencing Values*/
        private const int missTime = 500; //after 500ms, the note will be removed
        private List<TimingObject> _svQueue,_timingQueue,_barQueue;
        private List<NoteObject>[] _hitQueue;
        private List<NoteObject> _noteQueue,_lnQueue,_offLNQueue;
        private GameObject[] _activeNotes;
        private ulong[] _svCalc; //Stores SV position data for efficiency
        private float _actualSongTime;
        private float _curSongTime;
        private float _scrollNegativeFactor = 1f;
        private bool[] _keyDown;
        private float _receptorYPos;
        private float[] _receptorXPos,_receptorXOffset,_receptorSize;
        private float _modInterval;
        private ulong _curSVPos;
        private int _curSVint;
        private bool _songDone;
        private float _songEndOffset;

        public void Start()
        {
            //Check if beatmap is valid
            _qFile = QuaParser.Parse(Manager.currentMap.Path);
            if (!_qFile.IsValidQua)
            {
                print("IS NOT VALID QUA FILE");
            }
            else if (_qFile.IsValidQua)
            {
                //Copy + Convert to NoteObjects
                int i = 0;
                int j = 0;
                NoteObject newNote;
                _noteQueue = new List<NoteObject>();
                for (i = 0; i < _qFile.HitObjects.Count; i++)
                {
                    newNote = new NoteObject();
                    newNote.StartTime = _qFile.HitObjects[i].StartTime;
                    newNote.EndTime = _qFile.HitObjects[i].EndTime;
                    newNote.KeyLane = _qFile.HitObjects[i].KeyLane;
                    _noteQueue.Add(newNote);
                }

                TimingObject newTime;
                _svQueue = new List<TimingObject>();
                for (i = 0; i < _qFile.SliderVelocities.Count; i++)
                {
                    newTime = new TimingObject();
                    newTime.StartTime = _qFile.SliderVelocities[i].StartTime;
                    newTime.Multiplier = _qFile.SliderVelocities[i].Multiplier;
                    _svQueue.Add(newTime);
                }


                _timingQueue = new List<TimingObject>();
                for (i = 0; i < _qFile.TimingPoints.Count; i++)
                {
                    newTime = new TimingObject();
                    newTime.StartTime = _qFile.TimingPoints[i].StartTime;
                    newTime.BPM = _qFile.TimingPoints[i].BPM;
                    _timingQueue.Add(newTime);
                }

                //Declare Reference Values
                _hitQueue = new List<NoteObject>[4];
                _lnQueue = new List<NoteObject>();
                _offLNQueue = new List<NoteObject>();
                _barQueue = new List<TimingObject>();
                _activeNotes = new GameObject[maxNoteCount];

                _songAudio = transform.GetComponent<AudioSource>();
                _receptorXPos = new float[4];
                _receptorXOffset = new float[4];
                _receptorSize = new float[4];
                _keyDown = new bool[4];

                _songAudio = Manager.SongAudioSource;
                //songAudio.clip = qFile.audi; get audio from qfile.

                //Declare Config Variables
                _config_scrollSpeed = Manager.GameConfig.ScrollSpeed;
                _config_timingBars = true;//should be config
                _config_upScroll = !Manager.GameConfig.DownScroll;
                _config_KeyBindings = new KeyCode[] { Manager.GameConfig.KeyLaneMania1, Manager.GameConfig.KeyLaneMania2, Manager.GameConfig.KeyLaneMania3, Manager.GameConfig.KeyLaneMania4 };
                _config_offset = Manager.GameConfig.GlobalOffset;


                //TempValues
                float longestBpmTime = 0;
                int avgBpmPos = 0;
                float averageBpm = 100; //Change later


                //Declare Other Values
                _curSongTime = -config_playStartDelayed;
                _actualSongTime = -(float)config_playStartDelayed / 1000f;
                _curSVPos = (ulong)(-config_playStartDelayed + 10000f); //10000ms added since curSVPos is a uLong
                _curSVint = 0;

                //Declare Receptor Values
                if (_config_upScroll) _scrollNegativeFactor = -1f;
                _receptorYPos = _scrollNegativeFactor * (skin_columnSize / 256f + (float)skin_receptorYOffset / 100f - 10f);
                _receptors = new GameObject[4];
                _hitLighting = new GameObject[4];
                for (i = 0; i < 4; i++)
                {
                    _receptors[i] = receptorBar.transform.Find("R" + (i + 1)).gameObject;
                    _receptorXPos[i] = (i - 1.5f) * ((skin_columnSize + (float)skin_noteBufferSpacing) / config_PixelUnitSize);
                    if (i >= 2 && mod_split) _receptors[i].transform.localPosition = new Vector3(_receptorXPos[i], -_receptorYPos, 1);
                    else _receptors[i].transform.localPosition = new Vector3(_receptorXPos[i], _receptorYPos, 1);
                    _receptors[i].transform.localScale = Vector3.one * (skin_columnSize / (float)_receptors[i].transform.GetComponent<SpriteRenderer>().sprite.rect.size.x);
                    _receptors[i].transform.transform.eulerAngles = new Vector3(0, 0, _skin_receptorRotations[i]);

                    _hitQueue[i] = new List<NoteObject>();

                    _hitLighting[i] = lightingBar.transform.Find("L" + (i + 1)).gameObject;
                    _hitLighting[i].transform.localScale = new Vector3(_receptors[i].transform.localScale.x,
                        _scrollNegativeFactor * skin_hitLightingScale //*(config_PixelUnitSize / (float)hitLighting[i].transform.GetComponent<SpriteRenderer>().sprite.rect.size.y)
                        , 1f);
                    _hitLighting[i].transform.localPosition = _receptors[i].transform.localPosition + new Vector3(0, _hitLighting[i].transform.localScale.y * 2f, 1f);
                }

                //Set Skin Values
                bgMask.transform.localScale = new Vector3(
                    ((float)(skin_columnSize + skin_bgMaskBufferSize + skin_noteBufferSpacing) / config_PixelUnitSize) * 4f * (config_PixelUnitSize / (float)bgMask.transform.GetComponent<SpriteRenderer>().sprite.rect.size.x),
                    20f * (config_PixelUnitSize / (float)bgMask.transform.GetComponent<SpriteRenderer>().sprite.rect.size.y)
                    , 1f);

                //Calculate Average BPM of map
                if (!mod_noSV && _svQueue.Count > 1)
                {
                    if (_timingQueue.Count > 1)
                    {
                        foreach (TimingObject tp in _timingQueue)
                        {
                            if (i + 1 < _timingQueue.Count)
                            {
                                if (_timingQueue[i + 1].StartTime - _timingQueue[i].StartTime > longestBpmTime)
                                {
                                    avgBpmPos = i;
                                    longestBpmTime = _timingQueue[i + 1].StartTime - _timingQueue[i].StartTime;
                                }
                            }
                        }
                        averageBpm = _timingQueue[avgBpmPos].BPM;
                    }
                    else averageBpm = _timingQueue[0].BPM;

                    //Create and converts timing points to SV's
                    int hij = 0;
                    for (j = 0; j < _timingQueue.Count; j++)
                    {
                        if (_timingQueue[j].StartTime < _svQueue[0].StartTime)
                        {
                            TimingObject newTp = new TimingObject();
                            newTp.StartTime = _timingQueue[j].StartTime;
                            newTp.Multiplier = 1f;
                            _svQueue.Insert(0, newTp);
                        }
                        else if (_timingQueue[j].StartTime >= _svQueue[_svQueue.Count - 1].StartTime)
                        {
                            if (_timingQueue[j].StartTime - _svQueue[_svQueue.Count - 1].StartTime > 0.001f)
                            {
                                TimingObject newTp = new TimingObject();
                                newTp.StartTime = _timingQueue[j].StartTime;
                                newTp.Multiplier = 1f;
                                _svQueue.Add(newTp);
                            }
                        }
                        else
                        {
                            for (i = hij; i < _svQueue.Count; i++)
                            {
                                if (i + 1 < _svQueue.Count && _timingQueue[j].StartTime < _svQueue[i + 1].StartTime)
                                {
                                    if (Mathf.Abs(_timingQueue[j].StartTime - _svQueue[i + 1].StartTime) > 0.001f)
                                    {
                                        TimingObject newTp = new TimingObject();
                                        newTp.StartTime = _timingQueue[j].StartTime;
                                        newTp.Multiplier = 1f;
                                        _svQueue.Add(newTp);
                                        i++;
                                        hij = i;
                                    }
                                }
                                else break;
                            }
                        }
                    }

                    //Normalizes SV's in between each BPM change interval
                    hij = 0;
                    _svQueue.Sort(delegate (TimingObject p1, TimingObject p2) { return p1.StartTime.CompareTo(p2.StartTime); });
                    if (_timingQueue.Count >= 1)
                    {
                        for (i = 0; i < _timingQueue.Count; i++)
                        {
                            //print("c" + i);
                            for (j = hij; j < _svQueue.Count; j++)
                            {
                                if (_svQueue[j].StartTime >= _timingQueue[i].StartTime - 0.2f)
                                {
                                    hij = j;
                                    TimingObject newSV = new TimingObject();
                                    newSV.StartTime = _svQueue[j].StartTime;
                                    newSV.Multiplier = Mathf.Min(_svQueue[j].Multiplier * _timingQueue[i].BPM / averageBpm, 512f);
                                    _svQueue.RemoveAt(j);
                                    _svQueue.Insert(j, newSV);
                                    break;
                                }
                            }
                        }
                    }
                }
                //IF NO_SV is disabled or if there's no SV
                else
                {
                    TimingObject newTp = new TimingObject();
                    newTp.StartTime = 0;
                    newTp.Multiplier = 1f;
                    _svQueue.Add(newTp);
                }

                //Calculates SV for efficiency
                _svCalc = new ulong[_svQueue.Count];
                ulong svPosTime = 0;
                _svCalc[0] = 0;
                for (i = 0; i < _svQueue.Count; i++)
                {
                    if (i + 1 < _svQueue.Count)
                    {
                        ulong templ = (ulong)((_svQueue[i + 1].StartTime - _svQueue[i].StartTime) * _svQueue[i].Multiplier);
                        if (templ > 10000000) templ = 10000000;
                        svPosTime += templ;
                        _svCalc[i + 1] = svPosTime;
                    }
                    else break;
                }

                //Create Timing bars
                if (!mod_split && _config_timingBars)
                {
                    float curBarTime = 0;
                    for (i = 0; i < _timingQueue.Count; i++)
                    {
                        curBarTime = _timingQueue[i].StartTime;

                        if (_barQueue.Count > 0 && _barQueue[0].StartTime + 2 > curBarTime) _barQueue.RemoveAt(0);
                        curBarTime += 1000f * 4f * 60f / (_timingQueue[i].BPM);
                        TimingObject curTiming;

                        if (i + 1 < _timingQueue.Count)
                        {
                            while (curBarTime < _timingQueue[i + 1].StartTime)
                            {
                                curTiming = new TimingObject();
                                curTiming.StartTime = (int)(curBarTime);
                                _barQueue.Add(curTiming);
                                curBarTime += 1000f * 4f * 60f / (_timingQueue[i].BPM);
                            }
                        }
                        else
                        {
                            while (curBarTime < _songAudio.clip.length * 1000f)
                            {
                                curTiming = new TimingObject();
                                curTiming.StartTime = (int)(curBarTime);
                                _barQueue.Add(curTiming);
                                curBarTime += 1000f * 4f * 60f / (_timingQueue[i].BPM);
                            }
                        }
                    }

                    //Create starting bars
                    List<TimingObject> tempBars = new List<TimingObject>();
                    for (i = 0; i < _barQueue.Count; i++)
                    {
                        TimingObject hoo = new TimingObject();
                        GameObject curBar = Instantiate(timingBar, hitContainer.transform);
                        curBar.transform.localScale = new Vector3(((float)(skin_columnSize + skin_bgMaskBufferSize + skin_noteBufferSpacing) / config_PixelUnitSize) * 4f * (config_PixelUnitSize / (float)curBar.transform.GetComponent<SpriteRenderer>().sprite.rect.size.x),
                            (config_PixelUnitSize / (float)curBar.transform.GetComponent<SpriteRenderer>().sprite.rect.size.y) * ((float)skin_timingBarPixelSize / config_PixelUnitSize)
                            , 1f);
                        curBar.transform.localPosition = new Vector3(0f, PosFromSV(_barQueue[i].StartTime), 2f);

                        hoo.StartTime = _barQueue[i].StartTime;
                        hoo.TimingBar = curBar;
                        tempBars.Add(hoo);
                    }

                    _barQueue = new List<TimingObject>(tempBars);
                }

                //Create starting notes
                for (i = 0; i < maxNoteCount; i++)
                {
                    if (_noteQueue.Count > 0) _activeNotes[i] = InstantiateNote(null);
                    else break;
                }

                //Plays the song, but delayed
                AudioPlayer.LoadSong(Manager.currentMap, Manager.SongAudioSource, false, (float)config_playStartDelayed / 1000f);
                loaded = true;
            }
        }

        public void Update()
        {
            if (_qFile.IsValidQua && isActive)
            {
                //Check what to do after unpaused
                if (_songAudio.time >= _songAudio.clip.length && !_songDone) _songDone = true;

                if (!_songDone && !_songAudio.isPlaying && _songAudio.time < _songAudio.clip.length) _songAudio.UnPause();
                else if (_songDone) _songEndOffset += Time.deltaTime;


                //Song Time Calculation (ms)
                if (_songDone)
                {
                    _actualSongTime = _songAudio.clip.length + _songEndOffset;
                }
                else
                {
                    if (_actualSongTime < 0) _actualSongTime += Time.deltaTime;
                    else _actualSongTime = ((_songAudio.time) + (_actualSongTime + Time.deltaTime)) / 2f;
                }
                _curSongTime = _actualSongTime * 1000f - _config_offset;

                //Calculates curSV Position
                int k = 0; int j = 0;

                if (_curSongTime >= _svQueue[_svQueue.Count - 1].StartTime)
                {
                    _curSVint = _svQueue.Count - 1;
                }
                else if (_curSVint < _svQueue.Count - 2)
                {
                    for (j = _curSVint; j < _svQueue.Count - 1; j++)
                    {
                        if (_curSongTime > _svQueue[_curSVint + 1].StartTime) _curSVint++;
                        else break;
                    }
                }
                _curSVPos = _svCalc[_curSVint] + (ulong)((float)((_curSongTime) - (_svQueue[_curSVint].StartTime)) * _svQueue[_curSVint].Multiplier + 10000);

                //Receptor Modifiers
                if (mod_spin || mod_shuffle)
                {
                    for (k = 0; k < 4; k++)
                    {
                        ModifyReceptors(k);
                    }
                }

                //Move bars
                if (_config_timingBars && !mod_split)
                {
                    if (_barQueue.Count > 0 && _curSongTime > _barQueue[0].StartTime + missTime)
                    {
                        Destroy(_barQueue[0].TimingBar);
                        _barQueue.RemoveAt(0);
                    }
                    else
                    {
                        for (k = 0; k < Mathf.Min(_barQueue.Count, maxNoteCount); k++)
                        {
                            _barQueue[k].TimingBar.transform.localPosition = new Vector3(0f, PosFromSV(_barQueue[k].StartTime), 2f);
                        }
                    }
                }

                //Move notes and check if miss
                for (j = 0; j < 4; j++)
                {
                    for (k = 0; k < _hitQueue[j].Count; k++)
                    {
                        if (_curSongTime > _hitQueue[j][k].StartTime + _judgeTimes[4])
                        {
                            print("[Note Render] MISS");
                            _hitQueue[j][k].HitSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                            _hitQueue[j][k].SliderEndSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                            _hitQueue[j][k].SliderMiddleSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);

                            _offLNQueue.Add(_hitQueue[j][k]);
                            _hitQueue[j].RemoveAt(k);
                            k--;
                        }
                        else
                        {
                            SetNotePos(_hitQueue[j][k], 1);
                        }
                    }
                }
                //LN Check
                for (k = 0; k < _lnQueue.Count; k++)
                {
                    if (_curSongTime > Mathf.Max(_lnQueue[k].StartTime, _lnQueue[k].EndTime) + _judgeTimes[4])
                    {
                        print("[Note Render] LATE LN RELEASE");

                        NoteObject newNote = new NoteObject();

                        newNote.StartTime = (int)_curSongTime;
                        newNote.EndTime = _lnQueue[k].EndTime;
                        newNote.KeyLane = _lnQueue[k].KeyLane;

                        newNote.HitSet = _lnQueue[k].HitSet;
                        newNote.HitSprite = _lnQueue[k].HitSprite;
                        newNote.SliderEndSprite = _lnQueue[k].SliderEndSprite;
                        newNote.SliderMiddleSprite = _lnQueue[k].SliderMiddleSprite;
                        newNote.SliderEndObject = _lnQueue[k].SliderEndObject;
                        newNote.SliderMiddleObject = _lnQueue[k].SliderMiddleObject;

                        newNote.HitSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                        newNote.SliderEndSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                        newNote.SliderMiddleSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                        newNote.SliderEndObject.SetActive(false);
                        newNote.SliderMiddleObject.SetActive(false);

                        _offLNQueue.Add(newNote);
                        _lnQueue.RemoveAt(k);
                        k--;
                    }
                    else
                    {
                        SetNotePos(_lnQueue[k], 2);
                    }
                }

                //Ghost LN keys
                if (_offLNQueue.Count > 0)
                {
                    for (k = 0; k < _offLNQueue.Count; k++)
                    {
                        if (_curSongTime > Mathf.Max(_offLNQueue[k].StartTime, _offLNQueue[k].EndTime) + missTime)
                        {
                            RemoveNote(_offLNQueue[k].HitSet);
                            _offLNQueue.RemoveAt(k);
                            k--;
                        }
                        else
                        {
                            SetNotePos(_offLNQueue[k], 3);
                        }
                    }
                }
                //Key Press Check
                for (k = 0; k < 4; k++)
                {
                    if (!_keyDown[k])
                    {
                        if (Input.GetKeyDown(_config_KeyBindings[k]))
                        {
                            _keyDown[k] = true;
                            _receptors[k].transform.localScale = Vector3.one * (skin_columnSize / config_PixelUnitSize) * 1.1f;
                            _receptors[k].transform.GetComponent<SpriteRenderer>().sprite = receptorSprite[1];
                            _hitLighting[k].SetActive(true); //temp
                            JudgeNote(k + 1, _curSongTime);
                        }
                    }
                    else
                    {
                        if (Input.GetKeyUp(_config_KeyBindings[k]))
                        {
                            _keyDown[k] = false;
                            _receptors[k].transform.GetComponent<SpriteRenderer>().sprite = receptorSprite[0];
                            _hitLighting[k].SetActive(false); //temp
                            JudgeLN(k + 1, _curSongTime);
                        }
                    }
                }
            }
            else
            {
                if (_songAudio.isPlaying) _songAudio.Pause();
            }
        }

        //Creates/receycles new note
        private GameObject InstantiateNote(GameObject hoo)
        {
            NoteObject ho = _noteQueue[0];
            if (hoo == null)
            {
                hoo = Instantiate(hitObjectTest, hitContainer.transform);
                hoo.transform.Find("HitImage").transform.localScale = Vector3.one * (skin_columnSize / (float)hoo.transform.Find("HitImage").transform.GetComponent<SpriteRenderer>().sprite.rect.size.x); //skin_columnSize
                hoo.transform.Find("SliderMiddle").transform.localScale = Vector3.one * (skin_columnSize / (float)hoo.transform.Find("SliderMiddle").transform.GetComponent<SpriteRenderer>().sprite.rect.size.x);
                hoo.transform.Find("SliderEnd").transform.localScale = Vector3.one * (skin_columnSize / (float)hoo.transform.Find("SliderEnd").transform.GetComponent<SpriteRenderer>().sprite.rect.size.x);
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

            ho.HitNote.transform.eulerAngles = new Vector3(0, 0, _skin_receptorRotations[ho.KeyLane - 1]); //Rotation
            SetNotePos(ho, 0);
            _hitQueue[ho.KeyLane - 1].Add(ho);
            _noteQueue.RemoveAt(0);
            return hoo;
        }

        // Check if LN is released on time or early
        private void JudgeLN(int kkey, float timePos)
        {
            int curNote = -1; //Cannot create null struct :(
            float closestTime = 1000f;
            for (int i = 0; i < _lnQueue.Count; i++)
            {
                if (_lnQueue[i].KeyLane == kkey)
                {
                    closestTime = timePos - _lnQueue[i].EndTime;
                    curNote = i;
                }
            }
            if (curNote >= 0 && curNote < _lnQueue.Count)
            {
                if (closestTime < -_judgeTimes[5])
                {
                    //Darkens early/mis-released LNs. Use skin images instead later

                    NoteObject newNote = new NoteObject();

                    newNote.StartTime = (int)_curSongTime;
                    newNote.EndTime = _lnQueue[curNote].EndTime;
                    newNote.KeyLane = _lnQueue[curNote].KeyLane;

                    newNote.HitSet = _lnQueue[curNote].HitSet;
                    newNote.HitSprite = _lnQueue[curNote].HitSprite;
                    newNote.SliderEndSprite = _lnQueue[curNote].SliderEndSprite;
                    newNote.SliderMiddleSprite = _lnQueue[curNote].SliderMiddleSprite;
                    newNote.SliderEndObject = _lnQueue[curNote].SliderEndObject;
                    newNote.SliderMiddleObject = _lnQueue[curNote].SliderMiddleObject;

                    newNote.HitSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    newNote.SliderMiddleSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    newNote.SliderEndSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);

                    _offLNQueue.Add(newNote);
                    _lnQueue.RemoveAt(curNote);
                    print("[Note Render] EARLY LN RELEASE");
                }
                else if (closestTime > -_judgeTimes[5] && closestTime < _judgeTimes[5])
                {
                    RemoveNote(_lnQueue[curNote].HitSet); ;
                    _lnQueue.RemoveAt(curNote);
                    print("[Note Render] PERFECT LN RELEASE");
                }
            }
        }

        //Check if note is hit on time/late/early
        private void JudgeNote(int kkey, float timePos)
        {
            if (_hitQueue[kkey - 1].Count > 0 && _hitQueue[kkey - 1][0].StartTime - _curSongTime < _judgeTimes[4])
            {
                float closestTime = _hitQueue[kkey - 1][0].StartTime - _curSongTime;
                float absTime = Mathf.Abs(closestTime);
                if (absTime < _judgeTimes[4])
                {
                    if (absTime < _judgeTimes[0])
                    {
                        print("[Note Render] MARV");
                    }
                    else if (absTime < _judgeTimes[1])
                    {
                        print("[Note Render] PERF");
                    }
                    else if (absTime < _judgeTimes[2])
                    {
                        print("[Note Render] GREAT");
                    }
                    else if (absTime < _judgeTimes[3])
                    {
                        print("[Note Render] GOOD");
                    }
                    else
                    {
                        print("[Note Render] BAD");
                    }
                    //Check if LN
                    if (_hitQueue[kkey - 1][0].EndTime > 0)
                    {
                        _lnQueue.Add(_hitQueue[kkey - 1][0]);
                        _hitQueue[kkey - 1].RemoveAt(0);
                    }
                    else
                    {
                        RemoveNote(_hitQueue[kkey - 1][0].HitSet);
                        _hitQueue[kkey - 1].RemoveAt(0);
                    }
                    //Create particles
                    GameObject cp = Instantiate(circleParticleSystem, particleContainer.transform);
                    cp.transform.localPosition = _receptors[kkey - 1].transform.localPosition + new Vector3(0, 0, 4f);

                    GameObject hb = Instantiate(NoteHitParticle, particleContainer.transform);
                    hb.transform.localPosition = _receptors[kkey - 1].transform.localPosition + new Vector3(0, 0, -2f);
                    hb.transform.eulerAngles = _receptors[kkey - 1].transform.eulerAngles;
                    hb.transform.localScale = _receptors[kkey - 1].transform.localScale;
                    hb.GetComponent<NoteBurst>().startSize = hb.transform.localScale.x - 0.05f;
                    hb.GetComponent<NoteBurst>().burstSize = 0.1f;
                    hb.GetComponent<NoteBurst>().burstLength = 0.35f;
                }
            }
        }

        //Remove note and either destroy it or recycle
        private void RemoveNote(GameObject curNote)
        {
            if (_noteQueue.Count > 0) InstantiateNote(curNote);
            else Destroy(curNote);
        }


        // FOR SWAN MVOE THIS OUT
        // FOR SWAN MVOE THIS OUT
        // FOR SWAN MVOE THIS OUT
        // FOR SWAN MVOE THIS OUT





        // Modifies the receptors if visual mods are on.
        private void ModifyReceptors(int k)
        {
            //Spin Receptors
            if (mod_spin) _skin_receptorRotations[k] += 0.5f;
            if (_skin_receptorRotations[k] >= 360) _skin_receptorRotations[k] -= 360;

            //XOff Receptors
            if (mod_shuffle)
            {
                if (k == 0 || k == 2) _receptorXOffset[k] = (Mathf.Pow(Mathf.Sin(_curSongTime * (Mathf.PI / 180f) / 10f), 3) + 1f) * skin_columnSize / config_PixelUnitSize / 2f;
                else _receptorXOffset[k] = -(Mathf.Pow(Mathf.Sin(_curSongTime * (Mathf.PI / 180f) / 10f), 3) + 1f) * skin_columnSize / config_PixelUnitSize / 2f;
            }

            //Set Receptor Transform
            if (mod_spin) _receptors[k].transform.localEulerAngles = new Vector3(0, 0, _skin_receptorRotations[k]);
            if (mod_shuffle) _receptors[k].transform.localPosition = new Vector3(_receptorXPos[k] + _receptorXOffset[k], _receptorYPos, 1);
            _receptors[k].transform.localScale = Vector3.one * (_receptors[k].transform.localScale.x + ((skin_columnSize / config_PixelUnitSize) - _receptors[k].transform.localScale.x) / 3f);
        }

        //Sets the GameObject position of the note
        private void SetNotePos(NoteObject curNote, int lnMode)
        {
            float StartTime = curNote.StartTime;
            if (lnMode == 2) StartTime = _curSongTime;
            float splitFactor = 1f;
            if (mod_split && curNote.KeyLane >= 3) splitFactor = -1f;
            if (lnMode != 2 || mod_shuffle)
                curNote.HitSet.transform.localPosition = new Vector3(_receptorXPos[curNote.KeyLane - 1] + _receptorXOffset[curNote.KeyLane - 1], splitFactor * PosFromSV(StartTime), 0);
            else curNote.HitSet.transform.localPosition = new Vector3(_receptorXPos[curNote.KeyLane - 1] + _receptorXOffset[curNote.KeyLane - 1], _receptorYPos * splitFactor, 0);
            if (mod_spin) curNote.HitSprite.transform.eulerAngles = new Vector3(0, 0, _skin_receptorRotations[curNote.KeyLane - 1]);
            if ((lnMode != 1 || mod_pull) && lnMode != 3 && curNote.EndTime > 0 && curNote.EndTime > StartTime)
            {
                float lnSize = splitFactor * Mathf.Min(Mathf.Abs(PosFromSV(curNote.EndTime) - PosFromSV(StartTime)), 50f);

                curNote.SliderMiddleSprite.size = new Vector2(1f, -_scrollNegativeFactor * lnSize * (config_PixelUnitSize / skin_columnSize));
                curNote.SliderEndObject.transform.localPosition = new Vector3(0f, _scrollNegativeFactor * lnSize, 0.1f);

                if (lnMode == 0)
                {
                    curNote.SliderMiddleObject.transform.localScale = Vector3.one * (skin_columnSize / config_PixelUnitSize);
                    curNote.SliderMiddleSprite.color = new Color(1f, 1f, 1f, 1f);
                    curNote.SliderMiddleObject.SetActive(true);

                    curNote.SliderEndSprite.color = new Color(1f, 1f, 1f, 1f);
                    curNote.SliderEndObject.transform.localScale = Vector3.one * (skin_columnSize / config_PixelUnitSize);
                    curNote.SliderEndObject.SetActive(true);
                    if (splitFactor >= 1) curNote.SliderEndSprite.flipY = !_config_upScroll;
                    else curNote.SliderEndSprite.flipY = _config_upScroll;
                }
            }
        }

        //Calculates the position from SV
        private float PosFromSV(float timePos)
        {
            float returnVal;
            if (Mathf.Abs(timePos - _curSongTime) >= 0.01)
            {
                if (!mod_noSV)
                {
                    ulong svPosTime = 0;
                    int curPos = 0;
                    if (timePos >= _svQueue[_svQueue.Count - 1].StartTime)
                    {
                        curPos = _svQueue.Count - 1;
                    }
                    else
                    {
                        for (int i = 0; i < _svQueue.Count - 1; i++)
                        {
                            if (timePos < _svQueue[i + 1].StartTime)
                            {
                                curPos = i;
                                break;
                            }
                        }
                    }
                    svPosTime = _svCalc[curPos] + (ulong)(15000 + (timePos - _svQueue[curPos].StartTime) * _svQueue[curPos].Multiplier);
                    //10000ms added for negative, since svPos is a ulong

                    returnVal = (float)(svPosTime - _curSVPos - 5000f) / 1000f * (float)_config_scrollSpeed * (1 / _songAudio.pitch);
                }
                else returnVal = (timePos - _curSongTime) / 1000f * (float)_config_scrollSpeed * (1 / _songAudio.pitch);
            }
            else returnVal = 0;

            if (mod_pull) returnVal = 2f * Mathf.Max(Mathf.Pow(returnVal, 0.6f), 0) + Mathf.Min(timePos - _curSongTime, 0f) / 1000f * (float)_config_scrollSpeed * (1 / _songAudio.pitch);

            return returnVal * _scrollNegativeFactor + _receptorYPos;
        }
    }

    //Dont rmeove below this
}