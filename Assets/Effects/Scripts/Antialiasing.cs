// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    public enum AAMode
    {
        FXAA2 = 0,
        FXAA3Console = 1,
        FXAA1PresetA = 2,
        FXAA1PresetB = 3,
        NFAA = 4,
        SSAA = 5,
        DLAA = 6,
    }

    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Other/Antialiasing")]
    public class Antialiasing : PostEffectsBase
    {
        public AAMode mode = AAMode.FXAA3Console;

        public bool showGeneratedNormals = false;
        public float offsetScale = 0.2f;
        public float blurRadius = 18.0f;

        public float edgeThresholdMin = 0.05f;
        public float edgeThreshold = 0.2f;
        public float edgeSharpness = 4.0f;

        public bool dlaaSharp = false;

        public Shader ssaaShader;
        private Material _ssaa;
        public Shader dlaaShader;
        private Material _dlaa;
        public Shader nfaaShader;
        private Material _nfaa;
        public Shader shaderFXAAPreset2;
        private Material _materialFXAAPreset2;
        public Shader shaderFXAAPreset3;
        private Material _materialFXAAPreset3;
        public Shader shaderFXAAII;
        private Material _materialFXAAII;
        public Shader shaderFXAAIII;
        private Material _materialFXAAIII;


        public Material CurrentAAMaterial()
        {
            Material returnValue = null;

            switch (mode)
            {
                case AAMode.FXAA3Console:
                    returnValue = _materialFXAAIII;
                    break;
                case AAMode.FXAA2:
                    returnValue = _materialFXAAII;
                    break;
                case AAMode.FXAA1PresetA:
                    returnValue = _materialFXAAPreset2;
                    break;
                case AAMode.FXAA1PresetB:
                    returnValue = _materialFXAAPreset3;
                    break;
                case AAMode.NFAA:
                    returnValue = _nfaa;
                    break;
                case AAMode.SSAA:
                    returnValue = _ssaa;
                    break;
                case AAMode.DLAA:
                    returnValue = _dlaa;
                    break;
                default:
                    returnValue = null;
                    break;
            }

            return returnValue;
        }


        public override bool CheckResources()
        {
            CheckSupport(false);

            _materialFXAAPreset2 = CreateMaterial(shaderFXAAPreset2, _materialFXAAPreset2);
            _materialFXAAPreset3 = CreateMaterial(shaderFXAAPreset3, _materialFXAAPreset3);
            _materialFXAAII = CreateMaterial(shaderFXAAII, _materialFXAAII);
            _materialFXAAIII = CreateMaterial(shaderFXAAIII, _materialFXAAIII);
            _nfaa = CreateMaterial(nfaaShader, _nfaa);
            _ssaa = CreateMaterial(ssaaShader, _ssaa);
            _dlaa = CreateMaterial(dlaaShader, _dlaa);

            if (!ssaaShader.isSupported)
            {
                NotSupported();
                ReportAutoDisable();
            }

            return isSupported;
        }


        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (CheckResources() == false)
            {
                Graphics.Blit(source, destination);
                return;
            }

            // ----------------------------------------------------------------
            // FXAA antialiasing modes

            if (mode == AAMode.FXAA3Console && (_materialFXAAIII != null))
            {
                _materialFXAAIII.SetFloat("_EdgeThresholdMin", edgeThresholdMin);
                _materialFXAAIII.SetFloat("_EdgeThreshold", edgeThreshold);
                _materialFXAAIII.SetFloat("_EdgeSharpness", edgeSharpness);

                Graphics.Blit(source, destination, _materialFXAAIII);
            }
            else if (mode == AAMode.FXAA1PresetB && (_materialFXAAPreset3 != null))
            {
                Graphics.Blit(source, destination, _materialFXAAPreset3);
            }
            else if (mode == AAMode.FXAA1PresetA && _materialFXAAPreset2 != null)
            {
                source.anisoLevel = 4;
                Graphics.Blit(source, destination, _materialFXAAPreset2);
                source.anisoLevel = 0;
            }
            else if (mode == AAMode.FXAA2 && _materialFXAAII != null)
            {
                Graphics.Blit(source, destination, _materialFXAAII);
            }
            else if (mode == AAMode.SSAA && _ssaa != null)
            {
                // ----------------------------------------------------------------
                // SSAA antialiasing
                Graphics.Blit(source, destination, _ssaa);
            }
            else if (mode == AAMode.DLAA && _dlaa != null)
            {
                // ----------------------------------------------------------------
                // DLAA antialiasing

                source.anisoLevel = 0;
                RenderTexture interim = RenderTexture.GetTemporary(source.width, source.height);
                Graphics.Blit(source, interim, _dlaa, 0);
                Graphics.Blit(interim, destination, _dlaa, dlaaSharp ? 2 : 1);
                RenderTexture.ReleaseTemporary(interim);
            }
            else if (mode == AAMode.NFAA && _nfaa != null)
            {
                // ----------------------------------------------------------------
                // nfaa antialiasing

                source.anisoLevel = 0;

                _nfaa.SetFloat("_OffsetScale", offsetScale);
                _nfaa.SetFloat("_BlurRadius", blurRadius);

                Graphics.Blit(source, destination, _nfaa, showGeneratedNormals ? 1 : 0);
            }
            else
            {
                // none of the AA is supported, fallback to a simple blit
                Graphics.Blit(source, destination);
            }
        }
    }
}
