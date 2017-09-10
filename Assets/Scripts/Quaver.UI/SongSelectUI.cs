using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Main;
using UnityEngine.UI;

namespace Quaver.Main
{
    public class SongSelectUI : MonoBehaviour
    {
        //Reference Values
        public GameObject SongSelect;
        public GameObject SelectionSet;
        public GameObject DiffSelect;
        private SongSelectObject[] SongList;
        private SongSelectObject[] DifficultyList;
        private int totalBeatmaps;

        //Diff/Song Selection
        private int SongSelection = 0;
        private int DiffSelection = 0;

        //Beatmap display Variables
        private bool mouseRightDown = false;
        private int ObjectYSize = 0;
        private int offsetFromSelection = 0;
        private float SelectYPos = 0;
        private int SelectionDiffCount;

        //Animatioon
        private float posTween = 0;
        private float offsetTween = 0;

        //TEST
        private int testSelectionSize = 800; //use this to stress test

        public void Start()
        {
            SongList = new SongSelectObject[testSelectionSize];
            DifficultyList = new SongSelectObject[0];
            for (int i = 0; i < testSelectionSize; i++)
            {
                SongList[i] = new SongSelectObject(false, SongSelect, SelectionSet.transform, ObjectYSize, i, this.GetComponent<SongSelectUI>());
                int curPos = i;
                //DONT FORGET TO REMOVE THE EVENT LISTENER AFTER DESTROYING OBJECTS
                SongList[i].SelectObject.GetComponent<Button>().onClick.AddListener(() => { Clicked(curPos, false); });
                SongList[i].TitleText.text = "Song Name " + (i+1); // temp
                ObjectYSize += SongList[i].sizeY+5;
            }
            SelectYPos = ObjectYSize;
            totalBeatmaps = SongList.Length;

        }

        public void Clicked(int pos, bool subSelection)
        {
            if (!subSelection)
            {
                SongSelection = pos;
                for (int i = 0; i < DifficultyList.Length; i++)
                {
                    DifficultyList[i].SelectObject.GetComponent<Button>().onClick.RemoveAllListeners();
                    Destroy(DifficultyList[i].SelectObject);
                }
                DifficultyList = new SongSelectObject[SongList[pos].diffCount];
                int newSongPos = 0;
                for (int i = 0; i < SongList[pos].diffCount; i++)
                {
                    DifficultyList[i] = new SongSelectObject(true, DiffSelect, SelectionSet.transform, SongList[pos].posY - 75 - newSongPos, i, this.GetComponent<SongSelectUI>());
                    DifficultyList[i].TitleText.text = "Random Difficulty " + (i+1);
                    int curPos = i;
                    DifficultyList[i].SelectObject.GetComponent<Button>().onClick.AddListener(() => { Clicked(curPos, true); });
                    newSongPos += DifficultyList[i].sizeY+ 5;
                }
                offsetFromSelection = newSongPos;
                offsetTween = 0;
                SelectYPos = SongList[pos].posY - (int)(offsetFromSelection/2f);
            }
            else
            {
                SelectYPos = DifficultyList[pos].posY;
                print(pos + " difficulty selected!");
                DiffSelection = pos;
            }
        }


        private void Update()
        {
            //Set SelectYPos
            SelectYPos += Mathf.Min(Input.GetAxis("Mouse ScrollWheel") * 1500f, 2000f);
            if (Input.GetMouseButtonDown(1)) mouseRightDown = true;
            else if (Input.GetMouseButtonUp(1)) mouseRightDown = false;
            if (mouseRightDown) SelectYPos = (int)((Input.mousePosition.y/Screen.height) * (float)(ObjectYSize));
            SelectYPos = Mathf.Min(Mathf.Max(440 - offsetFromSelection, SelectYPos), ObjectYSize - 520); //540 - 100 (ui Size)
            //Set Selection Position
            posTween += (SelectYPos - posTween) * Mathf.Min(Time.deltaTime * 5f, 1);
            SelectionSet.transform.localPosition = new Vector2(-550, -posTween + 540);

            //Set offsetPos (when song is selected)
            if (offsetTween != offsetFromSelection)
            {
                offsetTween += (offsetFromSelection - offsetTween) * Mathf.Min(Time.deltaTime * 10f, 1);
                for (int i = 0; i <totalBeatmaps; i++)
                {
                    float tweener = SongList[i].ParentTransform.localPosition.y;
                    if (i< SongSelection) tweener += (SongList[i].posY - offsetTween - tweener) * Mathf.Min(Time.deltaTime * 15f, 1); 
                    else  tweener +=  (SongList[i].posY - tweener) * Mathf.Min(Time.deltaTime * 15f, 1);
                    SongList[i].ParentTransform.localPosition = new Vector2(5, tweener);
                }
            }
        }
    }
}
