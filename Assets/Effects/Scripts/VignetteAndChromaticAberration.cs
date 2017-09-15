// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Camera/Vignette and Chromatic Aberration")]
    public class VignetteAndChromaticAberration : PostEffectsBase
    {
        public enum AberrationMode
        {
            Simple = 0,
            Advanced = 1,
        }


        public AberrationMode mode = AberrationMode.Simple;
        public float intensity = 0.375f;                    // intensity == 0 disables pre pass (optimization)
        public float chromaticAberration = 0.2f;
        public float axialAberration = 0.5f;
        public float blur = 0.0f;                           // blur == 0 disables blur pass (optimization)
        public float blurSpread = 0.75f;
        public float luminanceDependency = 0.25f;
        public float blurDistance = 2.5f;
        public Shader vignetteShader;
        public Shader separableBlurShader;
        public Shader chromAberrationShader;


        private Material _vignetteMaterial;
        private Material _separableBlurMaterial;
        private Material _chromAberrationMaterial;


        public override bool CheckResources()
        {
            CheckSupport(false);

            _vignetteMaterial = CheckShaderAndCreateMaterial(vignetteShader, _vignetteMaterial);
            _separableBlurMaterial = CheckShaderAndCreateMaterial(separableBlurShader, _separableBlurMaterial);
            _chromAberrationMaterial = CheckShaderAndCreateMaterial(chromAberrationShader, _chromAberrationMaterial);

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

            bool doPrepass = (Mathf.Abs(blur) > 0.0f || Mathf.Abs(intensity) > 0.0f);

            float widthOverHeight = (1.0f * rtW) / (1.0f * rtH);
            const float oneOverBaseSize = 1.0f / 512.0f;

            RenderTexture color = null;
            RenderTexture color2A = null;

            if (doPrepass)
            {
                color = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);

                // Blur corners
                if (Mathf.Abs(blur) > 0.0f)
                {
                    color2A = RenderTexture.GetTemporary(rtW / 2, rtH / 2, 0, source.format);

                    Graphics.Blit(source, color2A, _chromAberrationMaterial, 0);

                    for (int i = 0; i < 2; i++)
                    {	// maybe make iteration count tweakable
                        _separableBlurMaterial.SetVector("offsets", new Vector4(0.0f, blurSpread * oneOverBaseSize, 0.0f, 0.0f));
                        RenderTexture color2B = RenderTexture.GetTemporary(rtW / 2, rtH / 2, 0, source.format);
                        Graphics.Blit(color2A, color2B, _separableBlurMaterial);
                        RenderTexture.ReleaseTemporary(color2A);

                        _separableBlurMaterial.SetVector("offsets", new Vector4(blurSpread * oneOverBaseSize / widthOverHeight, 0.0f, 0.0f, 0.0f));
                        color2A = RenderTexture.GetTemporary(rtW / 2, rtH / 2, 0, source.format);
                        Graphics.Blit(color2B, color2A, _separableBlurMaterial);
                        RenderTexture.ReleaseTemporary(color2B);
                    }
                }

                _vignetteMaterial.SetFloat("_Intensity", intensity);		// intensity for vignette
                _vignetteMaterial.SetFloat("_Blur", blur);					// blur intensity
                _vignetteMaterial.SetTexture("_VignetteTex", color2A);	// blurred texture

                Graphics.Blit(source, color, _vignetteMaterial, 0);			// prepass blit: darken & blur corners
            }

            _chromAberrationMaterial.SetFloat("_ChromaticAberration", chromaticAberration);
            _chromAberrationMaterial.SetFloat("_AxialAberration", axialAberration);
            _chromAberrationMaterial.SetVector("_BlurDistance", new Vector2(-blurDistance, blurDistance));
            _chromAberrationMaterial.SetFloat("_Luminance", 1.0f / Mathf.Max(Mathf.Epsilon, luminanceDependency));

            if (doPrepass) color.wrapMode = TextureWrapMode.Clamp;
            else source.wrapMode = TextureWrapMode.Clamp;
            Graphics.Blit(doPrepass ? color : source, destination, _chromAberrationMaterial, mode == AberrationMode.Advanced ? 2 : 1);

            RenderTexture.ReleaseTemporary(color);
            RenderTexture.ReleaseTemporary(color2A);
        }
    }
}
