using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Gameplay
{
    public partial class PlayScreen
    {
        //Config Values
        private int _config_scrollSpeed;
        private bool _config_timingBars;
        private bool _config_upScroll;
        private KeyCode[] _config_KeyBindings;
        private int _config_offset;

        //Game Constant Values
        private const int config_playStartDelayed = 1000; //delays 1 second before song starts
        private const int maxNoteCount = 64; //dont know if this should be config or not
        private const float config_PixelUnitSize = 128; //pixels to units. 128 pixels = 1 unit.
        private int[] _judgeTimes = new int[6] { 16, 37, 70, 100, 124, 80 }; //OD9 judge times in ms (0,1,2,3,4), LN offset 5

        //Referencing Values
        private const int missTime = 500; //after 500ms, the note will be removed
        private List<NoteObject>[] _hitQueue;
        private List<NoteObject> _noteQueue, _lnQueue, _offLNQueue;
        private GameObject[] _activeNotes;
        private float _scrollNegativeFactor = 1f;
        private bool[] _keyDown;

        //Initialize Notes
        private void np_init()
        {
            //Declare Reference Variables
            int i = 0;
            _activeNotes = new GameObject[maxNoteCount];
            _keyDown = new bool[4];
            _hitQueue = new List<NoteObject>[4];
            _lnQueue = new List<NoteObject>();
            _offLNQueue = new List<NoteObject>();
            for (i = 0; i < 4; i++) _hitQueue[i] = new List<NoteObject>();

            //Copy + Convert to NoteObjects
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

            //Create starting notes
            for (i = 0; i < maxNoteCount; i++)
            {
                if (_noteQueue.Count > 0) _activeNotes[i] = np_InstantiateNote(null);
                else break;
            }
        }

        //Move Notes
        private void np_MovePlayNotes()
        {
            for (int j = 0; j < 4; j++)
            {
                for (int k = 0; k < _hitQueue[j].Count; k++)
                {
                    if (_curSongTime > _hitQueue[j][k].StartTime + _judgeTimes[4])
                    {
                        //Note missed
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
        }

        //Resize LNs in lnQueue
        private void np_ResizeLongNotes()
        {
            for (int k = 0; k < _lnQueue.Count; k++)
            {
                if (_curSongTime > Mathf.Max(_lnQueue[k].StartTime, _lnQueue[k].EndTime) + _judgeTimes[4])
                {
                    //Late LN Release
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
        }

        //Moves notes in ghostQueue
        private void np_MoveGhostNotes()
        {
            if (_offLNQueue.Count > 0)
            {
                for (int k = 0; k < _offLNQueue.Count; k++)
                {
                    if (_curSongTime > Mathf.Max(_offLNQueue[k].StartTime, _offLNQueue[k].EndTime) + missTime)
                    {
                        np_RemoveNote(_offLNQueue[k].HitSet);
                        _offLNQueue.RemoveAt(k);
                        k--;
                    }
                    else
                    {
                        SetNotePos(_offLNQueue[k], 3);
                    }
                }
            }
        }

        //Creates/receycles new note
        private GameObject np_InstantiateNote(GameObject hoo)
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
                ho.SliderMiddleObject.gameObject.SetActive(false);
                ho.SliderEndObject.gameObject.SetActive(false);
            }

            ho.HitNote.transform.eulerAngles = new Vector3(0, 0, _skin_receptorRotations[ho.KeyLane - 1]); //Rotation
            SetNotePos(ho, 0);
            _hitQueue[ho.KeyLane - 1].Add(ho);
            _noteQueue.RemoveAt(0);
            return hoo;
        }

        //Remove note and either destroy it or recycle
        private void np_RemoveNote(GameObject curNote)
        {
            if (_noteQueue.Count > 0) np_InstantiateNote(curNote);
            else Destroy(curNote);
        }
    }
}
