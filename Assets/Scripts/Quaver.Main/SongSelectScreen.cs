// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Quaver.Main;
using Quaver.UI;
using Quaver.Cache;
using Quaver.SongSelect;
using Quaver.Audio;
using Quaver.Graphics;

namespace Quaver.Main
{
    public class SongSelectScreen : GameState
    {
        //UI Object Variables
        public GameObject SongSelectUI;
        public GameObject SongSelect;
        public GameObject DiffSelect;

        //Reference Values
        private List<MapDirectory> _sortedMapSets; //Change this list when sorting;
        private List<CachedBeatmap> _mapSetDifficulties;
        private GameObject _selectionUI;
        private GameObject _selectionSet;
        private SongSelectObject[] _songList;
        private SongSelectObject[] _difficultyList;
        private int _totalBeatmaps;

        //Diff/Song Selection
        private int _songSelection = -1;

        //Beatmap display Variables
        private bool _mouseRightDown = false;
        private int _objectYSize = 0;
        private int _offsetFromSelection = 0;
        private float _selectYPos = 0;

        //Animatioon
        private float _posTween = 0;
        private float _offsetTween = 0;

        private void SortBeatmaps()
        {
            //Sorts Beatmaps. SortedBeatmaps = new sorted beatmap list;
        }

        private void Start()
        {
            _sortedMapSets = Manager.MapDirectories;
            _totalBeatmaps = _sortedMapSets.Count;

            _selectionUI = Instantiate(SongSelectUI, this.transform.Find("SongSelect Canvas").transform);
            _selectionSet = _selectionUI.transform.Find("SelectionWindow").transform.transform.Find("SelectionCapture").gameObject;
            _songList = new SongSelectObject[_totalBeatmaps];
            _difficultyList = new SongSelectObject[0];
            for (int i = 0; i < _totalBeatmaps; i++)
            {
                //Song Object UI Initialization
                _songList[i] = ScriptableObject.CreateInstance<SongSelectObject>(); // SongSelectObject(0, SongSelect, SelectionSet.transform, ObjectYSize,SortedMapSets[i]);
                _songList[i].init(0, SongSelect, _selectionSet.transform, _objectYSize, _sortedMapSets[i]);

                //Song Object Set Text to mapset
                _songList[i].ParentTransform.localPosition = new Vector2(5, _songList[i].posY);
                _songList[i].TitleText.text = _sortedMapSets[i].Beatmaps[0].Title;
                _songList[i].SubText.text = _sortedMapSets[i].Beatmaps[0].Artist; //+ " | " + SortedBeatmaps[i].;
                if (_sortedMapSets[i].Beatmaps[0].Creator.Length > 0)
                    _songList[i].SubText.text += " | " + _sortedMapSets[i].Beatmaps[0].Creator;

                //Set the Y Position of the next UI
                _objectYSize += _songList[i].sizeY + 5;

                //Song Object UI Image Load
                BackgroundLoader.LoadTexture(_sortedMapSets[i].Beatmaps[0], _songList[i].bgImage);

                //Song Object UI Add Event Listener
                //DONT FORGET TO REMOVE THE EVENT LISTENER AFTER DESTROYING OBJECTS
                int curPos = i;
                _songList[i].SelectObject.GetComponent<Button>().onClick.AddListener(() => { Clicked(curPos, false); });
            }
            _selectYPos = _objectYSize;
        }



        private void Update()
        {
            //If Scroll
            _selectYPos += Mathf.Min(Input.GetAxis("Mouse ScrollWheel") * 1500f, 2000f);

            //If RightClick
            if (Input.GetMouseButtonDown(1)) _mouseRightDown = true;
            else if (Input.GetMouseButtonUp(1)) _mouseRightDown = false;

            //Check if RightClick is active
            if (_mouseRightDown) _selectYPos = (int)((Input.mousePosition.y / Screen.height) * (float)(_objectYSize));

            //Set position boundary (top,bottom)
            _selectYPos = Mathf.Min(Mathf.Max(390 - _offsetFromSelection, _selectYPos), _objectYSize - 485);

            //Set Selection Y Position
            _posTween += (_selectYPos - _posTween) * Mathf.Min(Time.deltaTime * 5f, 1);
            _selectionSet.transform.localPosition = new Vector2(-430, -_posTween + 540 ); //1080/2

            //Set offsetPos (when song is selected)
            if (_offsetTween != _offsetFromSelection)
            {
                _offsetTween += (_offsetFromSelection - _offsetTween) * Mathf.Min(Time.deltaTime * 10f, 1);
                for (int i = 0; i < _totalBeatmaps; i++)
                {
                    float tweener = _songList[i].ParentTransform.localPosition.y;
                    if (i < _songSelection) tweener += (_songList[i].posY - _offsetTween - tweener) * Mathf.Min(Time.deltaTime * 15f, 1);
                    else tweener += (_songList[i].posY - tweener) * Mathf.Min(Time.deltaTime * 15f, 1);
                    _songList[i].ParentTransform.localPosition = new Vector2(5, tweener);
                }
            }

            //Animate Difficulty Selection UI Position
            if (_difficultyList.Length > 0)
            {
                if (_offsetTween >= _offsetFromSelection - 20 && _difficultyList[_difficultyList.Length - 1].ParentTransform.localPosition.x > 4.99f)
                {
                    for (int i = 0; i < _difficultyList.Length; i++)
                    {
                        float tweener = _difficultyList[i].ParentTransform.localPosition.x + (5 - _difficultyList[i].ParentTransform.localPosition.x) * Mathf.Min(Time.deltaTime * 15f, 1);
                        _difficultyList[i].ParentTransform.localPosition = new Vector2(tweener, _difficultyList[i].posY);
                    }
                }
            }
        }


        public void Clicked(int pos, bool subSelection)
        {
            //If a mapset is selected
            if (!subSelection)
            {
                for (int i = 0; i < _difficultyList.Length; i++)
                {
                    _difficultyList[i].SelectObject.GetComponent<Button>().onClick.RemoveAllListeners();
                    Destroy(_difficultyList[i].SelectObject);
                    Destroy(_difficultyList[i]);
                }
                if (pos != _songSelection)
                {
                    //Creates new diff list
                    _difficultyList = new SongSelectObject[_sortedMapSets[pos].Beatmaps.Count];
                    int newSongPos = -5;
                    for (int i = 0; i < _sortedMapSets[pos].Beatmaps.Count; i++)
                    {
                        //Song Object UI Initialization
                        _difficultyList[i] = ScriptableObject.CreateInstance<SongSelectObject>(); // SongSelectObject(0, SongSelect, SelectionSet.transform, ObjectYSize,SortedMapSets[i]);
                        _difficultyList[i].init(i + 1, DiffSelect, _selectionSet.transform, _songList[pos].posY - 75 - newSongPos, _sortedMapSets[pos], _sortedMapSets[pos].Beatmaps[i]);

                        //Song Object Set Text to map diff
                        _difficultyList[i].TitleText.text = _sortedMapSets[pos].Beatmaps[i].Difficulty;
                        _difficultyList[i].SubText.text = "\u2605" + string.Format("{0:f2}", _sortedMapSets[pos].Beatmaps[i].Stars / 100f);
                        _difficultyList[i].ParentTransform.localPosition = new Vector2(450 + _difficultyList[i].diffPos * 60f, _difficultyList[i].posY);

                        //Set Y Pos for next object
                        newSongPos += _difficultyList[i].sizeY + 5;

                        //Load BG
                        BackgroundLoader.LoadTexture(_difficultyList[i].Beatmap, _difficultyList[i].bgImage);

                        //Add Event Listener
                        int curPos = i;
                        _difficultyList[i].SelectObject.GetComponent<Button>().onClick.AddListener(() => { Clicked(curPos, true); });
                    }

                    //Sets Pos + Loads Audio/BG for main audio player/bg image
                    _songSelection = pos;
                    if (Manager.currentMap != null && _difficultyList[0].Beatmap.AudioPath != Manager.currentMap.AudioPath)
                    {
                        AudioPlayer.LoadSong(_sortedMapSets[pos].Beatmaps[0], Manager.SongAudioSource, true);
                        BackgroundLoader.LoadSprite(_sortedMapSets[pos].Beatmaps[0], Manager.bgImage);
                    }

                    //Change Diff UI Values
                    newSongPos += 5;
                    _offsetFromSelection = newSongPos;
                    _offsetTween = 0;
                    _selectYPos = _songList[pos].posY - (int)(_offsetFromSelection / 2f);

                    //Sets the current map to the first difficulty of the map
                    Manager.currentMap = _difficultyList[0].Beatmap;
                }
                else
                {
                    //If mapset is clicked twice, no maps will be selected.
                    _offsetFromSelection = 0;
                    _songSelection = -1;
                    _selectYPos = _songList[pos].posY;
                    _difficultyList = new SongSelectObject[0];
                }
            }
            //If a difficulty is selected
            else
            {
                //Set position of object and play/set audio/bg if it's not the same as the prev map.
                _selectYPos = _difficultyList[pos].posY;
                if (_difficultyList[pos].Beatmap.AudioPath != Manager.currentMap.AudioPath)
                {
                    BackgroundLoader.LoadSprite(_difficultyList[pos].Beatmap, Manager.bgImage);
                    AudioPlayer.LoadSong(_difficultyList[pos].Beatmap, Manager.SongAudioSource, true);
                }
                //Sets the current map to the selected diff
                Manager.currentMap = _difficultyList[pos].Beatmap;
            }
        }
    }
}
