// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [AddComponentMenu("Image Effects/Displacement/Vortex")]
    public class Vortex : ImageEffectBase
    {
        public Vector2 radius = new Vector2(0.4F, 0.4F);
        public float angle = 50;
        public Vector2 center = new Vector2(0.5F, 0.5F);

        // Called by camera to apply image effect
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            ImageEffects.RenderDistortion(material, source, destination, angle, center, radius);
        }
    }
}
