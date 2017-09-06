using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Graphics 
{
    public class NoteBurst : MonoBehaviour
    {
        public float burstLength;
        public float burstSize;
        public float startSize;
        private float timeElapsed = 0;
        // Update is called once per frame
        void Update()
        {
            timeElapsed += Time.deltaTime;
            transform.localScale = (Vector3.one  + Vector3.one * startSize * Mathf.Sqrt(timeElapsed / burstLength)) * burstSize;
            transform.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f- Mathf.Pow(timeElapsed / burstLength,0.2f));
            if (timeElapsed >= burstLength)
            {
                Destroy(this.gameObject);
            }
        }
    }
}

