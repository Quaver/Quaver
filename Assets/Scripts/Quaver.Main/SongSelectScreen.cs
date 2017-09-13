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
        private List<MapDirectory> SortedMapSets; //Change this list when sorting;
        private List<CachedBeatmap> MapSetDifficulties;
        private GameObject SelectionUI;
        private GameObject SelectionSet;
        private SongSelectObject[] SongList;
        private SongSelectObject[] DifficultyList;
        private int totalBeatmaps;

        //Diff/Song Selection
        private int SongSelection = -1;

        //Beatmap display Variables
        private bool mouseRightDown = false;
        private int ObjectYSize = 0;
        private int offsetFromSelection = 0;
        private float SelectYPos = 0;

        //Animatioon
        private float posTween = 0;
        private float offsetTween = 0;

        void SortBeatmaps()
        {
            //Sorts Beatmaps. SortedBeatmaps = new sorted beatmap list;
        }

        void Start()
        {
            SortedMapSets = Manager.MapDirectories;
            totalBeatmaps = SortedMapSets.Count;

            SelectionUI = Instantiate(SongSelectUI, this.transform.Find("SongSelect Canvas").transform);
            SelectionSet = SelectionUI.transform.Find("SelectionWindow").transform.transform.Find("SelectionCapture").gameObject;
            SongList = new SongSelectObject[totalBeatmaps];
            DifficultyList = new SongSelectObject[0];
            for (int i = 0; i < totalBeatmaps; i++)
            {
                //Song Object UI Initialization
                SongList[i] = ScriptableObject.CreateInstance<SongSelectObject>(); // SongSelectObject(0, SongSelect, SelectionSet.transform, ObjectYSize,SortedMapSets[i]);
                SongList[i].init(0, SongSelect, SelectionSet.transform, ObjectYSize, SortedMapSets[i]);

                //Song Object Set Text to mapset
                SongList[i].ParentTransform.localPosition = new Vector2(5, SongList[i].posY);
                SongList[i].TitleText.text = SortedMapSets[i].Beatmaps[0].Title;
                SongList[i].SubText.text = SortedMapSets[i].Beatmaps[0].Artist; //+ " | " + SortedBeatmaps[i].;
                if (SortedMapSets[i].Beatmaps[0].Creator.Length > 0)
                    SongList[i].SubText.text += " | " + SortedMapSets[i].Beatmaps[0].Creator;

                //Set the Y Position of the next UI
                ObjectYSize += SongList[i].sizeY + 5;

                //Song Object UI Image Load
                BackgroundLoader.LoadTexture(SortedMapSets[i].Beatmaps[0], SongList[i].bgImage);

                //Song Object UI Add Event Listener
                //DONT FORGET TO REMOVE THE EVENT LISTENER AFTER DESTROYING OBJECTS
                int curPos = i;
                SongList[i].SelectObject.GetComponent<Button>().onClick.AddListener(() => { Clicked(curPos, false); });
            }
            SelectYPos = ObjectYSize;
        }



        private void Update()
        {
            //If Scroll
            SelectYPos += Mathf.Min(Input.GetAxis("Mouse ScrollWheel") * 1500f, 2000f);

            //If RightClick
            if (Input.GetMouseButtonDown(1)) mouseRightDown = true;
            else if (Input.GetMouseButtonUp(1)) mouseRightDown = false;

            //Check if RightClick is active
            if (mouseRightDown) SelectYPos = (int)((Input.mousePosition.y / Screen.height) * (float)(ObjectYSize));

            //Set position boundary. (485-95,485)
            SelectYPos = Mathf.Min(Mathf.Max(390 - offsetFromSelection, SelectYPos), ObjectYSize - 485); 

            //Set Selection Y Position
            posTween += (SelectYPos - posTween) * Mathf.Min(Time.deltaTime * 5f, 1);
            SelectionSet.transform.localPosition = new Vector2(-430, -posTween + 540);

            //Set offsetPos (when song is selected)
            if (offsetTween != offsetFromSelection)
            {
                offsetTween += (offsetFromSelection - offsetTween) * Mathf.Min(Time.deltaTime * 10f, 1);
                for (int i = 0; i < totalBeatmaps; i++)
                {
                    float tweener = SongList[i].ParentTransform.localPosition.y;
                    if (i < SongSelection) tweener += (SongList[i].posY - offsetTween - tweener) * Mathf.Min(Time.deltaTime * 15f, 1);
                    else tweener += (SongList[i].posY - tweener) * Mathf.Min(Time.deltaTime * 15f, 1);
                    SongList[i].ParentTransform.localPosition = new Vector2(5, tweener);
                }
            }

            //Animate Difficulty Selection UI Position
            if (DifficultyList.Length > 0)
            {
                if (offsetTween >= offsetFromSelection - 20 && DifficultyList[DifficultyList.Length - 1].ParentTransform.localPosition.x > 4.99f)
                {
                    for (int i = 0; i < DifficultyList.Length; i++)
                    {
                        float tweener = DifficultyList[i].ParentTransform.localPosition.x + (5 - DifficultyList[i].ParentTransform.localPosition.x) * Mathf.Min(Time.deltaTime * 15f, 1);
                        DifficultyList[i].ParentTransform.localPosition = new Vector2(tweener, DifficultyList[i].posY);
                    }
                }
            }
        }


        public void Clicked(int pos, bool subSelection)
        {
            //If a mapset is selected
            if (!subSelection)
            {
                for (int i = 0; i < DifficultyList.Length; i++)
                {
                    DifficultyList[i].SelectObject.GetComponent<Button>().onClick.RemoveAllListeners();
                    Destroy(DifficultyList[i].SelectObject);
                    Destroy(DifficultyList[i]);
                }
                if (pos != SongSelection)
                {

                    //Creates new diff list
                    DifficultyList = new SongSelectObject[SortedMapSets[pos].Beatmaps.Count];
                    int newSongPos = -5;
                    for (int i = 0; i < SortedMapSets[pos].Beatmaps.Count; i++)
                    {
                        //Song Object UI Initialization
                        DifficultyList[i] = ScriptableObject.CreateInstance<SongSelectObject>(); // SongSelectObject(0, SongSelect, SelectionSet.transform, ObjectYSize,SortedMapSets[i]);
                        DifficultyList[i].init(i + 1, DiffSelect, SelectionSet.transform, SongList[pos].posY - 75 - newSongPos, SortedMapSets[pos], SortedMapSets[pos].Beatmaps[i]);

                        //Song Object Set Text to map diff
                        DifficultyList[i].TitleText.text = SortedMapSets[pos].Beatmaps[i].Difficulty;
                        DifficultyList[i].SubText.text = "★" + string.Format("{0:f2}", SortedMapSets[pos].Beatmaps[i].Stars / 100f);
                        DifficultyList[i].ParentTransform.localPosition = new Vector2(450 + DifficultyList[i].diffPos * 60f, DifficultyList[i].posY);

                        //Set Y Pos for next object
                        newSongPos += DifficultyList[i].sizeY + 5;

                        //Load BG
                        BackgroundLoader.LoadTexture(DifficultyList[i].Beatmap, DifficultyList[i].bgImage);

                        //Add Event Listener
                        int curPos = i;
                        DifficultyList[i].SelectObject.GetComponent<Button>().onClick.AddListener(() => { Clicked(curPos, true); });
                    }

                    //Sets Pos + Loads Audio/BG for main audio player/bg image
                    SongSelection = pos;
                    if (Manager.currentMap != null && DifficultyList[0].Beatmap.AudioPath != Manager.currentMap.AudioPath)
                    {
                        AudioPlayer.LoadSong(SortedMapSets[pos].Beatmaps[0], Manager.SongAudioSource, true);
                        BackgroundLoader.LoadSprite(SortedMapSets[pos].Beatmaps[0], Manager.bgImage);
                    }

                    //Change Diff UI Values
                    newSongPos += 5;
                    offsetFromSelection = newSongPos;
                    offsetTween = 0;
                    SelectYPos = SongList[pos].posY - (int)(offsetFromSelection / 2f);

                    //Sets the current map to the first difficulty of the map
                    Manager.currentMap = DifficultyList[0].Beatmap;
                }
                else
                {
                    //If mapset is clicked twice, no maps will be selected.
                    offsetFromSelection = 0;
                    SongSelection = -1;
                    SelectYPos = SongList[pos].posY;
                    DifficultyList = new SongSelectObject[0];
                }
            }
            //If a difficulty is selected
            else
            {
                //Set position of object and play/set audio/bg if it's not the same as the prev map.
                SelectYPos = DifficultyList[pos].posY;
                if (DifficultyList[pos].Beatmap.AudioPath != Manager.currentMap.AudioPath)
                {
                    BackgroundLoader.LoadSprite(DifficultyList[pos].Beatmap, Manager.bgImage);
                    AudioPlayer.LoadSong(DifficultyList[pos].Beatmap, Manager.SongAudioSource, true);
                }
                //Sets the current map to the selected diff
                Manager.currentMap = DifficultyList[pos].Beatmap;
            }
        }

    }
}
