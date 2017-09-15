// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [AddComponentMenu("Image Effects/Color Adjustments/Sepia Tone")]
    public class SepiaTone : ImageEffectBase
    {
        // Called by camera to apply image effect
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, material);
        }
    }
}
