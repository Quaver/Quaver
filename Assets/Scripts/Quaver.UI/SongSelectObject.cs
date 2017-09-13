using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Quaver.SongSelect;
using Quaver.Cache;

namespace Quaver.UI
{

    public class SongSelectObject : MonoBehaviour {

        //Song Variables
        public MapDirectory MapSet;
        public CachedBeatmap Beatmap;

        //UI Variables
        public GameObject SelectObject;
        public Transform ParentTransform;
        public RawImage bgImage;
        public RawImage rankingImage;

        public Text TitleText;
        public Text SubText;
        public int posY;
        public int sizeY;

        public SongSelectObject(int SubSelection, GameObject newSObject, Transform newParent, int newPosY, MapDirectory newMapSet, CachedBeatmap newBeatmap = null)
        {
            MapSet = newMapSet;
            Beatmap = newBeatmap;
            SelectObject = Instantiate(newSObject, newParent);
            ParentTransform = SelectObject.transform.GetComponent<Transform>();
            sizeY = (int)ParentTransform.GetComponent<RectTransform>().rect.size.y;
            posY = newPosY;
            TitleText = SelectObject.transform.Find("SongTitle").GetComponent<Text>();
            rankingImage = SelectObject.transform.Find("Ranking").GetComponent<RawImage>();
            bgImage = SelectObject.transform.Find("bgImage").GetComponent<RawImage>();
            if (SubSelection == 0)
            {
                SubText = SelectObject.transform.Find("SongArtist").GetComponent<Text>();
                ParentTransform.localPosition = new Vector2(5, posY);
            }
            else
            {
                ParentTransform.localPosition = new Vector2(450 + SubSelection * 60f, posY);
                SubText = SelectObject.transform.Find("SongDiff").GetComponent<Text>();
            }

        }
    }
}
