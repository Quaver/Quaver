// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [AddComponentMenu("Image Effects/Color Adjustments/Grayscale")]
    public class Grayscale : ImageEffectBase
    {
        public Texture textureRamp;
        public float rampOffset;

        // Called by camera to apply image effect
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            material.SetTexture("_RampTex", textureRamp);
            material.SetFloat("_RampOffset", rampOffset);
            Graphics.Blit(source, destination, material);
        }
    }
}
