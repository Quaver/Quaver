
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
using Quaver.Difficulty;
using Quaver.Qua;
using Quaver.Utils;

namespace Quaver.Main
{
    public class SongSelectScreen : GameStateObject
    {
        //UI Object Variables
        private GameObject _SongInfoWindow;
        private GameObject _ScrollBar;
        private RectTransform _ScrollRect;
        private RectTransform _ScreenUI;
        public GameObject SongSelectUI;
        public GameObject SongSelect;
        public GameObject DiffSelect;
        public GameObject Game_UI;

        //Reference Values
        private List<MapDirectory> _sortedMapSets; //Change this list when sorting;
        private List<CachedBeatmap> _mapSetDifficulties;
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
            UpdateSongList(Manager.currentMap);
        }

        public void UpdateSongList(CachedBeatmap InitSelection = null)
        {
            //Initialize
            _sortedMapSets = GameStateManager.MapDirectories;
            _totalBeatmaps = _sortedMapSets.Count;

            //Set GameObject Variabls
            StateUI = Instantiate(SongSelectUI, Manager.ActiveStateUISet.transform);
            _SongInfoWindow = StateUI.transform.Find("InformationWindow").transform.Find("SongInfo").gameObject;
            _ScrollBar = StateUI.transform.Find("SelectionWindow").transform.Find("SongScroll").gameObject;
            _selectionSet = StateUI.transform.Find("SelectionWindow").transform.transform.Find("SelectionCapture").gameObject;
            _ScreenUI = Manager.ActiveStateUISet.GetComponent<RectTransform>();
            _ScrollRect = _selectionSet.GetComponent<RectTransform>();

            //Add EventListener to ScrollBar
            _ScrollBar.GetComponent<Scrollbar>().onValueChanged.AddListener((float pos) => { SetScrollPos(pos,false); });

            //Create Song Select UI
            _songList = new SongSelectObject[_totalBeatmaps];
            _difficultyList = new SongSelectObject[0];
            for (int i = 0; i < _totalBeatmaps; i++)
            {
                //Song Object UI Initialization
                _songList[i] = ScriptableObject.CreateInstance<SongSelectObject>();
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
                int curPos = i;
                _songList[i].SelectObject.GetComponent<Button>().onClick.AddListener(() => { SelectMapObject(curPos, false); });
            }
            //Set these variables to the size of the song scroll
            _ScrollBar.GetComponent<Scrollbar>().size = Mathf.Max(Mathf.Min(1080f / ((float)_objectYSize+1080f),1),0.1f);    
            
            if (InitSelection != null)
            {
                //int curIndex = GameStateManager.MapDirectories.IndexOf(InitSelection.Directory);
                //SelectMapObject(curIndex, true);
            }
            //else _selectYPos = _objectYSize;
            _selectYPos = _objectYSize;

        }

        //Remove All Event Listeners generated from this class
        private void RemoveListeners()
        {
            int i = 0;
            //ScrollBar
            _ScrollBar.GetComponent<Scrollbar>().onValueChanged.RemoveAllListeners();

            //Diff List
            for (i = 0; i < _difficultyList.Length; i++)
            {
                _difficultyList[i].SelectObject.GetComponent<Button>().onClick.RemoveAllListeners();
                Destroy(_difficultyList[i].SelectObject);
                Destroy(_difficultyList[i]);
            }

            //MapSet List
            for (i = 0; i < _songList.Length; i++)
            {
                _songList[i].SelectObject.GetComponent<Button>().onClick.RemoveAllListeners();
                Destroy(_songList[i].SelectObject);
                Destroy(_songList[i]);
            }
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

            //Set ScrollBar Position
            SetScrollPos(1f - (_selectYPos / _objectYSize), true);
            _selectYPos = Mathf.Min(Mathf.Max(0, _selectYPos), _objectYSize);

            //Set Selection Y Position
            _posTween += (_selectYPos - _posTween) * Mathf.Min(Time.deltaTime * 5f, 1);
            _ScrollRect.anchoredPosition = new Vector2(0, -_posTween - ((float)_ScreenUI.rect.height / 2f));

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
                        float tweener = _difficultyList[i].ParentTransform.localPosition.x + ((5 - _difficultyList[i].ParentTransform.localPosition.x) * Mathf.Min(Time.deltaTime * 15f, 1));
                        _difficultyList[i].ParentTransform.localPosition = new Vector2(tweener, _difficultyList[i].posY);
                    }
                }
            }
        }

        private void SelectMapObject(int pos, bool subSelection)
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
                        _difficultyList[i] = ScriptableObject.CreateInstance<SongSelectObject>();
                        _difficultyList[i].init(i + 1, DiffSelect, _selectionSet.transform, _songList[pos].posY - 75 - newSongPos, _sortedMapSets[pos], _sortedMapSets[pos].Beatmaps[i]);

                        //Song Object Set Text to map diff
                        _difficultyList[i].TitleText.text = _sortedMapSets[pos].Beatmaps[i].Difficulty;
                        _difficultyList[i].SubText.text = "\u2605" + string.Format("{0:f2}", _sortedMapSets[pos].Beatmaps[i].Stars / 100f);
                        _difficultyList[i].ParentTransform.localPosition = new Vector2(450 + (_difficultyList[i].diffPos * 60f), _difficultyList[i].posY);

                        //Set Y Pos for next object
                        newSongPos += _difficultyList[i].sizeY + 5;

                        //Load BG
                        BackgroundLoader.LoadTexture(_difficultyList[i].Beatmap, _difficultyList[i].bgImage);

                        //Add Event Listener
                        int curPos = i;
                        _difficultyList[i].SelectObject.GetComponent<Button>().onClick.AddListener(() => { SelectMapObject(curPos, true); });
                    }

                    //Sets Pos + Loads Audio/BG for main audio player/bg image
                    _songSelection = pos;
                    if (Manager.currentMap != null && _difficultyList[0].Beatmap.AudioPath != Manager.currentMap.AudioPath)
                    {
                        SetSongStats(_difficultyList[0].Beatmap);
                    }

                    //Change Diff UI Values
                    newSongPos += 5;
                    _offsetFromSelection = newSongPos;
                    _offsetTween = 0;
                    _selectYPos = _songList[pos].posY - (int)(_offsetFromSelection / 2f);

                    //Sets the current map to the first difficulty of the map set
                    Manager.currentMap = _difficultyList[0].Beatmap;

                    // Change the window title to the clicked map.
                    MainUtilities.ChangeWindowTitle("Quaver - " + Manager.currentMap.Artist + " - " + Manager.currentMap.Title + " [" + Manager.currentMap.Difficulty + "]");
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
                    SetSongStats(_difficultyList[pos].Beatmap);
                }
                //Sets the current map to the selected diff
                Manager.currentMap = _difficultyList[pos].Beatmap;

                // Change the window title to the clicked difficulty
                MainUtilities.ChangeWindowTitle("Quaver - " + Manager.currentMap.Artist + " - " + Manager.currentMap.Title + " [" + Manager.currentMap.Difficulty + "]");                
            }
        }

        private void SetSongStats(CachedBeatmap _map)
        {
            //Play Audio and Set Images
            AudioPlayer.LoadSong(_map, Manager.SongAudioSource, true);
            BackgroundLoader.LoadSprite(_map, Manager.bgImage);
            BackgroundLoader.LoadTexture(_map, _SongInfoWindow.transform.Find("SongBG").GetComponent<RawImage>());

            //Set Song Info Text
            _SongInfoWindow.transform.Find("Title").GetComponent<Text>().text = _map.Title;
            _SongInfoWindow.transform.Find("DiffName").GetComponent<Text>().text = _map.Difficulty;
            _SongInfoWindow.transform.Find("Star Rating").GetComponent<Text>().text = "\u2605" + string.Format("{0:f2}", _map.Stars / 100f);
            _SongInfoWindow.transform.Find("Description").GetComponent<Text>().text = _map.Description;
            _SongInfoWindow.transform.Find("Artist").GetComponent<Text>().text = "Artist: " + _map.Artist;
            _SongInfoWindow.transform.Find("Mapper").GetComponent<Text>().text = "Mapper: " + _map.Creator;
            _SongInfoWindow.transform.Find("Source").GetComponent<Text>().text = "Source: " + _map.Source;
            _SongInfoWindow.transform.Find("Tags").GetComponent<Text>().text = "Tags: " + _map.Tags;
            //BPM
            //LENGTH

            //Get Difficulty Stats
            Difficulty.Difficulty DiffParsed = DifficultyCalculator.CalculateDifficulty(ThreadWrapper.ParseQuaInNewThread(_map.Path).HitObjects);
            int[] npsList = DiffParsed.npsInterval;

            //Set NPS Graph
            _SongInfoWindow.transform.Find("NPSGraph").transform.Find("avgNps").GetComponent<Text>().text = "Average NPS: " + string.Format("{0:f2}", DiffParsed.AverageNPS);
            //TEMP
            _SongInfoWindow.transform.Find("NPSGraph").transform.Find("PlaceHolder").GetComponent<Text>().text = "";
            for (int i=0; i < npsList.Length && i < 24; i++)
            {
                if (i % 12 == 0 && i != 0) _SongInfoWindow.transform.Find("NPSGraph").transform.Find("PlaceHolder").GetComponent<Text>().text = _SongInfoWindow.transform.Find("NPSGraph").transform.Find("PlaceHolder").GetComponent<Text>().text + "\n";
                _SongInfoWindow.transform.Find("NPSGraph").transform.Find("PlaceHolder").GetComponent<Text>().text = _SongInfoWindow.transform.Find("NPSGraph").transform.Find("PlaceHolder").GetComponent<Text>().text + npsList[i] + ", ";
            }
        }

        public void SetScrollPos(float pos, bool ManualChange)
        {
            if (!ManualChange)
            {
                _selectYPos = (1f - pos) * _objectYSize;
            }
            else
            {
                _ScrollBar.GetComponent<Scrollbar>().value = pos;
            }
        }

    }
}
