using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Quaver.SongSelect;
using Quaver.Cache;

namespace Quaver.UI
{

    public class SongSelectObject : ScriptableObject {

        //Song Variables
        public MapDirectory MapSet;
        public CachedBeatmap Beatmap;

        //UI Object Variables
        public GameObject SelectObject;
        public Transform ParentTransform;
        public RawImage bgImage;
        public RawImage rankingImage;

        //Hiarchy Variables
        public Text TitleText;
        public Text SubText;
        public int posY;
        public int sizeY;
        public int diffPos;

        public void init(int SubSelection, GameObject newSObject, Transform newParent, int newPosY, MapDirectory newMapSet, CachedBeatmap newBeatmap = null)
        {

            //Set UI Object Variables
            SelectObject = Instantiate(newSObject, newParent);
            ParentTransform = SelectObject.transform.GetComponent<Transform>();
            TitleText = SelectObject.transform.Find("SongTitle").GetComponent<Text>();
            rankingImage = SelectObject.transform.Find("Ranking").GetComponent<RawImage>();
            bgImage = SelectObject.transform.Find("bgImage").GetComponent<RawImage>();

            //Set Hiarchy Variables
            MapSet = newMapSet;
            Beatmap = newBeatmap;
            diffPos = SubSelection;
            sizeY = (int)ParentTransform.GetComponent<RectTransform>().rect.size.y;
            posY = newPosY;

            //Set SubText (Depending if it's a mapset or diff)
            if (SubSelection == 0)
                SubText = SelectObject.transform.Find("SongArtist").GetComponent<Text>();
            else
                SubText = SelectObject.transform.Find("SongDiff").GetComponent<Text>();
        }
    }
}
