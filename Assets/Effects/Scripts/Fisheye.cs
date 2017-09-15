// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Displacement/Fisheye")]
    public class Fisheye : PostEffectsBase
    {
        public float strengthX = 0.05f;
        public float strengthY = 0.05f;

        public Shader fishEyeShader = null;
        private Material _fisheyeMaterial = null;


        public override bool CheckResources()
        {
            CheckSupport(false);
            _fisheyeMaterial = CheckShaderAndCreateMaterial(fishEyeShader, _fisheyeMaterial);

            if (!isSupported)
                ReportAutoDisable();
            return isSupported;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (CheckResources() == false)
            {
                Graphics.Blit(source, destination);
                return;
            }

            float oneOverBaseSize = 80.0f / 512.0f; // to keep values more like in the old version of fisheye

            float ar = (source.width * 1.0f) / (source.height * 1.0f);

            _fisheyeMaterial.SetVector("intensity", new Vector4(strengthX * ar * oneOverBaseSize, strengthY * oneOverBaseSize, strengthX * ar * oneOverBaseSize, strengthY * oneOverBaseSize));
            Graphics.Blit(source, destination, _fisheyeMaterial);
        }
    }
}
