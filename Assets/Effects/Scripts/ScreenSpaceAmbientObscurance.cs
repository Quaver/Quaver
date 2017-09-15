// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Rendering/Screen Space Ambient Obscurance")]
    internal class ScreenSpaceAmbientObscurance : PostEffectsBase
    {
        [Range(0, 3)]
        public float intensity = 0.5f;
        [Range(0.1f, 3)]
        public float radius = 0.2f;
        [Range(0, 3)]
        public int blurIterations = 1;
        [Range(0, 5)]
        public float blurFilterDistance = 1.25f;
        [Range(0, 1)]
        public int downsample = 0;

        public Texture2D rand = null;
        public Shader aoShader = null;

        private Material _aoMaterial = null;

        public override bool CheckResources()
        {
            CheckSupport(true);

            _aoMaterial = CheckShaderAndCreateMaterial(aoShader, _aoMaterial);

            if (!isSupported)
                ReportAutoDisable();
            return isSupported;
        }

        private void OnDisable()
        {
            if (_aoMaterial)
                DestroyImmediate(_aoMaterial);
            _aoMaterial = null;
        }

        [ImageEffectOpaque]
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (CheckResources() == false)
            {
                Graphics.Blit(source, destination);
                return;
            }

            Matrix4x4 P = GetComponent<Camera>().projectionMatrix;
            var invP = P.inverse;
            Vector4 projInfo = new Vector4
                ((-2.0f / (Screen.width * P[0])),
                 (-2.0f / (Screen.height * P[5])),
                 ((1.0f - P[2]) / P[0]),
                 ((1.0f + P[6]) / P[5]));

            _aoMaterial.SetVector("_ProjInfo", projInfo); // used for unprojection
            _aoMaterial.SetMatrix("_ProjectionInv", invP); // only used for reference
            _aoMaterial.SetTexture("_Rand", rand); // not needed for DX11 :)
            _aoMaterial.SetFloat("_Radius", radius);
            _aoMaterial.SetFloat("_Radius2", radius * radius);
            _aoMaterial.SetFloat("_Intensity", intensity);
            _aoMaterial.SetFloat("_BlurFilterDistance", blurFilterDistance);

            int rtW = source.width;
            int rtH = source.height;

            RenderTexture tmpRt = RenderTexture.GetTemporary(rtW >> downsample, rtH >> downsample);
            RenderTexture tmpRt2;

            Graphics.Blit(source, tmpRt, _aoMaterial, 0);

            if (downsample > 0)
            {
                tmpRt2 = RenderTexture.GetTemporary(rtW, rtH);
                Graphics.Blit(tmpRt, tmpRt2, _aoMaterial, 4);
                RenderTexture.ReleaseTemporary(tmpRt);
                tmpRt = tmpRt2;

                // @NOTE: it's probably worth a shot to blur in low resolution
                //  instead with a bilat-upsample afterwards ...
            }

            for (int i = 0; i < blurIterations; i++)
            {
                _aoMaterial.SetVector("_Axis", new Vector2(1.0f, 0.0f));
                tmpRt2 = RenderTexture.GetTemporary(rtW, rtH);
                Graphics.Blit(tmpRt, tmpRt2, _aoMaterial, 1);
                RenderTexture.ReleaseTemporary(tmpRt);

                _aoMaterial.SetVector("_Axis", new Vector2(0.0f, 1.0f));
                tmpRt = RenderTexture.GetTemporary(rtW, rtH);
                Graphics.Blit(tmpRt2, tmpRt, _aoMaterial, 1);
                RenderTexture.ReleaseTemporary(tmpRt2);
            }

            _aoMaterial.SetTexture("_AOTex", tmpRt);
            Graphics.Blit(source, destination, _aoMaterial, 2);

            RenderTexture.ReleaseTemporary(tmpRt);
        }
    }
}
