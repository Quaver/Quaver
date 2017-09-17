
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
        private float _timeElapsed = 0;
        // Update is called once per frame
        private void Update()
        {
            _timeElapsed += Time.deltaTime;
            transform.localScale = Vector3.one * (startSize + Mathf.Sqrt(_timeElapsed / burstLength) * burstSize);
            transform.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f - Mathf.Pow(_timeElapsed / burstLength, 1.5f));
            if (_timeElapsed >= burstLength) Destroy(this.gameObject);
        }
    }
}

