// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [AddComponentMenu("Image Effects/Color Adjustments/Color Correction (Ramp)")]
    public class ColorCorrectionRamp : ImageEffectBase
    {
        public Texture textureRamp;

        // Called by camera to apply image effect
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            material.SetTexture("_RampTex", textureRamp);
            Graphics.Blit(source, destination, material);
        }
    }
}
