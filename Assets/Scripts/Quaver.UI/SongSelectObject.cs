using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Quaver.UI
{

    public class SongSelectObject : MonoBehaviour {

        //Song Variables
        //public Beatmap SelectBeatmap; BEATMAP REFERENCE
        public int diffCount; // temp

        //UI Variables
        public GameObject SelectObject;
        public Transform ParentTransform;
        public GameObject SelectParent;
        public RawImage bgImage;
        public RawImage rankingImage;

        public Text TitleText;
        public Text ArtistText;
        public int selectPos;
        public int posY;
        public int sizeY;

        public SongSelectObject(int SubSelection, GameObject newSObject, Transform newParent, int newPosY, int newSelectPos)
        {
            diffCount = (int)Random.Range(1, 12); //TEMP

            SelectObject = Instantiate(newSObject, newParent);
            ParentTransform = SelectObject.transform.GetComponent<Transform>();
            sizeY = (int)ParentTransform.GetComponent<RectTransform>().rect.size.y;
            posY = newPosY;
            TitleText = SelectObject.transform.Find("SongTitle").GetComponent<Text>();
            rankingImage = SelectObject.transform.Find("Ranking").GetComponent<RawImage>();
            bgImage = SelectObject.GetComponent<RawImage>();
            if (SubSelection == 0)
            {
                ArtistText = SelectObject.transform.Find("SongArtist").GetComponent<Text>();
                ParentTransform.localPosition = new Vector2(5, posY);
            }
            else ParentTransform.localPosition = new Vector2(450 + SubSelection*60f, posY);
            selectPos = newSelectPos;

        }
    }
}
