using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Quaver.Main
{

    public class SongSelectObject : MonoBehaviour {

        //Song Variables
        //public Beatmap SelectBeatmap; BEATMAP REFERENCE
        public int diffCount; // temp

        //UI Variables
        public GameObject SelectObject;
        public Transform ParentTransform;
        public SongSelectUI SelectParent;
        public RawImage bgImage;
        public RawImage rankingImage;

        public Text TitleText;
        public Text ArtistText;
        public int selectPos;
        public int posY;
        public int sizeY;

        public SongSelectObject(bool SubSelection, GameObject newSObject, Transform newParent, int newPosY, int newSelectPos, SongSelectUI SongList)
        {
            diffCount = (int)Random.Range(1, 12); //TEMP

            SelectObject = Instantiate(newSObject, newParent);
            ParentTransform = SelectObject.transform.GetComponent<Transform>();
            sizeY = (int)ParentTransform.GetComponent<RectTransform>().rect.size.y;
            posY = newPosY;
            TitleText = SelectObject.transform.Find("SongTitle").GetComponent<Text>();
            rankingImage = SelectObject.transform.Find("Ranking").GetComponent<RawImage>();
            if (!SubSelection)
            {
                ArtistText = SelectObject.transform.Find("SongArtist").GetComponent<Text>();
                bgImage = SelectObject.transform.Find("bgThumbnail").GetComponent<RawImage>();
            }
            SelectParent = SongList;
            selectPos = newSelectPos;

            ParentTransform.localPosition = new Vector2(5, posY);
        }
    }
}
