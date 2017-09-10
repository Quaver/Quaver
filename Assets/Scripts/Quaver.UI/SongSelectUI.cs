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

        private int ObjectYSize = 0;
        private int Selection = 0;
        private int offsetFromSelection = 0;
        private float SelectYPos = 0;
        private int SelectionDiffCount;

        private int totalBeatmaps;

        //Animatioon
        private float posTween = 0;
        private float offsetTween = 0;

        //TEST
        private int testSelectionSize = 20;

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
                ObjectYSize += SongList[i].sizeY+5;
            }
            SelectYPos = ObjectYSize;
            totalBeatmaps = SongList.Length;

        }

        public void Clicked(int pos, bool subSelection)
        {
            SelectYPos = SongList[pos].posY;
            print("1");
            if (!subSelection)
            {
                print("2");
                Selection = pos;
                for (int i = 0; i < DifficultyList.Length; i++)
                {
                    Destroy(DifficultyList[i].SelectObject);
                }
                DifficultyList = new SongSelectObject[SongList[pos].diffCount];
                int newSongPos = 0;
                for (int i = 0; i < SongList[pos].diffCount; i++)
                {
                    DifficultyList[i] = new SongSelectObject(true, DiffSelect, SelectionSet.transform, SongList[pos].posY - 75 - newSongPos, i, this.GetComponent<SongSelectUI>());
                    newSongPos += DifficultyList[i].sizeY+ 5;
                }
                offsetFromSelection = newSongPos;
                offsetTween = 0;
            }
        }


        private void Update()
        {
            SelectYPos += Mathf.Min(Input.GetAxis("Mouse ScrollWheel") * 1500f, 2000f);
            SelectYPos = Mathf.Min(Mathf.Max(440 - offsetFromSelection, SelectYPos), ObjectYSize - 520); //540 - 100 (ui Size)
            posTween += (SelectYPos - posTween) * Mathf.Min(Time.deltaTime*5f,1);
            SelectionSet.transform.localPosition = new Vector2(-550, -posTween + 540);
            if (offsetTween != offsetFromSelection)
            {
                offsetTween += (offsetFromSelection - offsetTween) * Mathf.Min(Time.deltaTime * 10f, 1);
                for (int i = 0; i <totalBeatmaps; i++)
                {
                    float tweener = SongList[i].ParentTransform.localPosition.y;
                    if (i<Selection) tweener += (SongList[i].posY - offsetTween - tweener) * Mathf.Min(Time.deltaTime * 15f, 1); 
                    else  tweener +=  (SongList[i].posY - tweener) * Mathf.Min(Time.deltaTime * 15f, 1);
                    SongList[i].ParentTransform.localPosition = new Vector2(5, tweener);
                }
            }
        }
    }
}
