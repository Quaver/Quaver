// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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

