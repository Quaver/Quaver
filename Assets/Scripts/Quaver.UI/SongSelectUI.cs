using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaver.Main;
using UnityEngine.UI;

namespace Quaver.Main
{
    public class SongSelectUI : MonoBehaviour
    {
        public GameObject SongSelect;
        public GameObject SelectionSet;
        private SongSelectObject[] SongList;

        private int ObjectYSize = 0;
        private int Selection = 0;
        private float SelectYPos = 0;
        private int SelectionDiffCount;

        //ANIMATION
        private float posTween = 0;

        public void Start()
        {
            SongList = new SongSelectObject[10];
            for (int i = 0; i < 10; i++)
            {
                SongList[i] = new SongSelectObject(SongSelect, SelectionSet.transform, ObjectYSize, i, this.GetComponent<SongSelectUI>());
                int curPos = i;
                //DONT FORGET TO REMOVE THE EVENT LISTENER AFTER DESTROYING OBJECTS
                SongList[i].SelectObject.GetComponent<Button>().onClick.AddListener(() => { print("A"); Clicked(curPos); });
                ObjectYSize += SongList[i].sizeY+5;
            }

        }

        public void Clicked(int pos)
        {
            SelectYPos = SongList[pos].posY;
        }


        private void Update()
        {
            posTween += (SelectYPos - posTween) / 15f;
            SelectionSet.transform.localPosition = new Vector2(-550, -posTween + 540);
            SelectYPos += Mathf.Min(Input.GetAxis("Mouse ScrollWheel")*1500f,2000f);
            SelectYPos = Mathf.Min(Mathf.Max(0, SelectYPos),ObjectYSize);
        }
    }
}
