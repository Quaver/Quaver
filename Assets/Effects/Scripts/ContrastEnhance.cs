// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Color Adjustments/Contrast Enhance (Unsharp Mask)")]
    internal class ContrastEnhance : PostEffectsBase
    {
        public float intensity = 0.5f;
        public float threshold = 0.0f;

        private Material _separableBlurMaterial;
        private Material _contrastCompositeMaterial;

        public float blurSpread = 1.0f;

        public Shader separableBlurShader = null;
        public Shader contrastCompositeShader = null;


        public override bool CheckResources()
        {
            CheckSupport(false);

            _contrastCompositeMaterial = CheckShaderAndCreateMaterial(contrastCompositeShader, _contrastCompositeMaterial);
            _separableBlurMaterial = CheckShaderAndCreateMaterial(separableBlurShader, _separableBlurMaterial);

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

            int rtW = source.width;
            int rtH = source.height;

            RenderTexture color2 = RenderTexture.GetTemporary(rtW / 2, rtH / 2, 0);

            // downsample

            Graphics.Blit(source, color2);
            RenderTexture color4a = RenderTexture.GetTemporary(rtW / 4, rtH / 4, 0);
            Graphics.Blit(color2, color4a);
            RenderTexture.ReleaseTemporary(color2);

            // blur

            _separableBlurMaterial.SetVector("offsets", new Vector4(0.0f, (blurSpread * 1.0f) / color4a.height, 0.0f, 0.0f));
            RenderTexture color4b = RenderTexture.GetTemporary(rtW / 4, rtH / 4, 0);
            Graphics.Blit(color4a, color4b, _separableBlurMaterial);
            RenderTexture.ReleaseTemporary(color4a);

            _separableBlurMaterial.SetVector("offsets", new Vector4((blurSpread * 1.0f) / color4a.width, 0.0f, 0.0f, 0.0f));
            color4a = RenderTexture.GetTemporary(rtW / 4, rtH / 4, 0);
            Graphics.Blit(color4b, color4a, _separableBlurMaterial);
            RenderTexture.ReleaseTemporary(color4b);

            // composite

            _contrastCompositeMaterial.SetTexture("_MainTexBlurred", color4a);
            _contrastCompositeMaterial.SetFloat("intensity", intensity);
            _contrastCompositeMaterial.SetFloat("threshhold", threshold);
            Graphics.Blit(source, destination, _contrastCompositeMaterial);

            RenderTexture.ReleaseTemporary(color4a);
        }
    }
}
