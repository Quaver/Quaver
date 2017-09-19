using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Graphics
{
    public class ParticleRemover : MonoBehaviour
    {
        public float removeTime;
        private float _timeElapsed;
        private void Update()
        {
            _timeElapsed += Time.deltaTime;
            if (_timeElapsed > removeTime)
            {
                Destroy(this.gameObject);
            }
        }
    }
}

