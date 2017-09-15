// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Edge Detection/Crease Shading")]
    internal class CreaseShading : PostEffectsBase
    {
        public float intensity = 0.5f;
        public int softness = 1;
        public float spread = 1.0f;

        public Shader blurShader = null;
        private Material _blurMaterial = null;

        public Shader depthFetchShader = null;
        private Material _depthFetchMaterial = null;

        public Shader creaseApplyShader = null;
        private Material _creaseApplyMaterial = null;


        public override bool CheckResources()
        {
            CheckSupport(true);

            _blurMaterial = CheckShaderAndCreateMaterial(blurShader, _blurMaterial);
            _depthFetchMaterial = CheckShaderAndCreateMaterial(depthFetchShader, _depthFetchMaterial);
            _creaseApplyMaterial = CheckShaderAndCreateMaterial(creaseApplyShader, _creaseApplyMaterial);

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

            float widthOverHeight = (1.0f * rtW) / (1.0f * rtH);
            float oneOverBaseSize = 1.0f / 512.0f;

            RenderTexture hrTex = RenderTexture.GetTemporary(rtW, rtH, 0);
            RenderTexture lrTex1 = RenderTexture.GetTemporary(rtW / 2, rtH / 2, 0);

            Graphics.Blit(source, hrTex, _depthFetchMaterial);
            Graphics.Blit(hrTex, lrTex1);

            for (int i = 0; i < softness; i++)
            {
                RenderTexture lrTex2 = RenderTexture.GetTemporary(rtW / 2, rtH / 2, 0);
                _blurMaterial.SetVector("offsets", new Vector4(0.0f, spread * oneOverBaseSize, 0.0f, 0.0f));
                Graphics.Blit(lrTex1, lrTex2, _blurMaterial);
                RenderTexture.ReleaseTemporary(lrTex1);
                lrTex1 = lrTex2;

                lrTex2 = RenderTexture.GetTemporary(rtW / 2, rtH / 2, 0);
                _blurMaterial.SetVector("offsets", new Vector4(spread * oneOverBaseSize / widthOverHeight, 0.0f, 0.0f, 0.0f));
                Graphics.Blit(lrTex1, lrTex2, _blurMaterial);
                RenderTexture.ReleaseTemporary(lrTex1);
                lrTex1 = lrTex2;
            }

            _creaseApplyMaterial.SetTexture("_HrDepthTex", hrTex);
            _creaseApplyMaterial.SetTexture("_LrDepthTex", lrTex1);
            _creaseApplyMaterial.SetFloat("intensity", intensity);
            Graphics.Blit(source, destination, _creaseApplyMaterial);

            RenderTexture.ReleaseTemporary(hrTex);
            RenderTexture.ReleaseTemporary(lrTex1);
        }
    }
}
