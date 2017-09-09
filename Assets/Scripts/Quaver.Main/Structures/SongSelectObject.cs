using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Quaver.Main
{

    public class SongSelectObject : MonoBehaviour {

        //Song Variables
        public int diffCount = 1;

        //UI Variables
        public GameObject SelectObject;
        public RectTransform ParentTransform;
        public SongSelectUI SelectParent;

        public Text TitleText;
        public Text ArtistText;
        public int selectPos;
        public int posY;
        public int sizeY;

        public SongSelectObject(GameObject newSObject, Transform newParent, int newPosY, int newSelectPos, SongSelectUI SongList)
        {
            SelectObject = Instantiate(newSObject, newParent);
            ParentTransform = SelectObject.transform.GetComponent<RectTransform>();
            sizeY = (int)ParentTransform.rect.size.y;
            posY = newPosY;
            ArtistText = SelectObject.transform.Find("SongArtist").GetComponent<Text>();
            TitleText = SelectObject.transform.Find("SongTitle").GetComponent<Text>();
            SelectParent = SongList;
            selectPos = newSelectPos;

            ParentTransform.localPosition = new Vector2(5, posY);
        }
        void Destroy()
        {
            ParentTransform.GetComponent<Button>().onClick.RemoveAllListeners();
        }

    }
}
