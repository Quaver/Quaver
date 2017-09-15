// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

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

