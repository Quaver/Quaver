using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Quaver.UI
{

    public class SongSelectObject : MonoBehaviour {

        //Song Variables

        //UI Variables
        public GameObject SelectObject;
        public Transform ParentTransform;
        public RawImage bgImage;
        public RawImage rankingImage;

        public Text TitleText;
        public Text SubText;
        public int posY;
        public int sizeY;

        public SongSelectObject(int SubSelection, GameObject newSObject, Transform newParent, int newPosY)
        {

            SelectObject = Instantiate(newSObject, newParent);
            ParentTransform = SelectObject.transform.GetComponent<Transform>();
            sizeY = (int)ParentTransform.GetComponent<RectTransform>().rect.size.y;
            posY = newPosY;
            TitleText = SelectObject.transform.Find("SongTitle").GetComponent<Text>();
            rankingImage = SelectObject.transform.Find("Ranking").GetComponent<RawImage>();
            bgImage = SelectObject.GetComponent<RawImage>();
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
