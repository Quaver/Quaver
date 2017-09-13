using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Main;
using Quaver.UI;
using UnityEngine.UI;
using Quaver.Cache;
using Quaver.SongSelect;
using Quaver.Audio;

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
                SongList[i] = new SongSelectObject(0, SongSelect, SelectionSet.transform, ObjectYSize,SortedMapSets[i]);
                int curPos = i;
                //DONT FORGET TO REMOVE THE EVENT LISTENER AFTER DESTROYING OBJECTS
                SongList[i].SelectObject.GetComponent<Button>().onClick.AddListener(() => { Clicked(curPos, false); });
                SongList[i].TitleText.text = SortedMapSets[i].Beatmaps[0].Title;
                SongList[i].SubText.text = SortedMapSets[i].Beatmaps[0].Artist; //+ " | " + SortedBeatmaps[i].;
                BackgroundLoader.LoadTexture(SortedMapSets[i].Beatmaps[0], SongList[i].bgImage);
                //SongList[i].bgImage.texture = SortedBeatmaps[i].
                ObjectYSize += SongList[i].sizeY + 5;
            }
            SelectYPos = ObjectYSize;
        }



        private void Update()
        {
            //Set SelectYPos
            SelectYPos += Mathf.Min(Input.GetAxis("Mouse ScrollWheel") * 1500f, 2000f);
            if (Input.GetMouseButtonDown(1)) mouseRightDown = true;
            else if (Input.GetMouseButtonUp(1)) mouseRightDown = false;
            if (mouseRightDown) SelectYPos = (int)((Input.mousePosition.y / Screen.height) * (float)(ObjectYSize));
            SelectYPos = Mathf.Min(Mathf.Max(390 - offsetFromSelection, SelectYPos), ObjectYSize - 485); //Set position boundary. (485-95,485)

            //Set Selection Position
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
                if (offsetTween >= offsetFromSelection - 20 && DifficultyList[DifficultyList.Length - 1].SelectObject.transform.localPosition.x > 4.99f)
                {
                    for (int i = 0; i < DifficultyList.Length; i++)
                    {
                        float tweener = DifficultyList[i].SelectObject.transform.localPosition.x + (5 - DifficultyList[i].SelectObject.transform.localPosition.x) * Mathf.Min(Time.deltaTime * 15f, 1);
                        DifficultyList[i].SelectObject.transform.localPosition = new Vector2(tweener, DifficultyList[i].posY);
                    }
                }
            }
        }


        public void Clicked(int pos, bool subSelection)
        {
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
                        DifficultyList[i] = new SongSelectObject(i + 1, DiffSelect, SelectionSet.transform, SongList[pos].posY - 75 - newSongPos, SortedMapSets[pos], SortedMapSets[pos].Beatmaps[i]);
                        DifficultyList[i].TitleText.text = SortedMapSets[pos].Beatmaps[i].Difficulty;
                        DifficultyList[i].SubText.text = "★" + string.Format("{0:f2}", SortedMapSets[pos].Beatmaps[i].Stars / 100f);
                        BackgroundLoader.LoadTexture(DifficultyList[i].Beatmap, DifficultyList[i].bgImage);
                        int curPos = i;
                        DifficultyList[i].SelectObject.GetComponent<Button>().onClick.AddListener(() => { Clicked(curPos, true); });
                        newSongPos += DifficultyList[i].sizeY + 5;
                    }

                    //Sets Pos + Loads Audio/BG
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
                    Manager.currentMap = DifficultyList[0].Beatmap;
                }
                else
                {
                    offsetFromSelection = 0;
                    SongSelection = -1;
                    SelectYPos = SongList[pos].posY;
                    DifficultyList = new SongSelectObject[0];
                }
            }
            else
            {
                SelectYPos = DifficultyList[pos].posY;
                if (DifficultyList[pos].Beatmap.AudioPath != Manager.currentMap.AudioPath)
                {
                    BackgroundLoader.LoadSprite(Manager.currentMap, Manager.bgImage);
                    AudioPlayer.LoadSong(DifficultyList[pos].Beatmap, Manager.SongAudioSource, true);
                }
                Manager.currentMap = DifficultyList[pos].Beatmap;
            }
        }

    }
}
