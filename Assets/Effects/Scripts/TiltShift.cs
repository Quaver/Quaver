// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Camera/Tilt Shift (Lens Blur)")]
    internal class TiltShift : PostEffectsBase
    {
        public enum TiltShiftMode
        {
            TiltShiftMode,
            IrisMode,
        }
        public enum TiltShiftQuality
        {
            Preview,
            Normal,
            High,
        }

        public TiltShiftMode mode = TiltShiftMode.TiltShiftMode;
        public TiltShiftQuality quality = TiltShiftQuality.Normal;

        [Range(0.0f, 15.0f)]
        public float blurArea = 1.0f;

        [Range(0.0f, 25.0f)]
        public float maxBlurSize = 5.0f;

        [Range(0, 1)]
        public int downsample = 0;

        public Shader tiltShiftShader = null;
        private Material _tiltShiftMaterial = null;


        public override bool CheckResources()
        {
            CheckSupport(true);

            _tiltShiftMaterial = CheckShaderAndCreateMaterial(tiltShiftShader, _tiltShiftMaterial);

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

            _tiltShiftMaterial.SetFloat("_BlurSize", maxBlurSize < 0.0f ? 0.0f : maxBlurSize);
            _tiltShiftMaterial.SetFloat("_BlurArea", blurArea);
            source.filterMode = FilterMode.Bilinear;

            RenderTexture rt = destination;
            if (downsample > 0f)
            {
                rt = RenderTexture.GetTemporary(source.width >> downsample, source.height >> downsample, 0, source.format);
                rt.filterMode = FilterMode.Bilinear;
            }

            int basePassNr = (int)quality; basePassNr *= 2;
            Graphics.Blit(source, rt, _tiltShiftMaterial, mode == TiltShiftMode.TiltShiftMode ? basePassNr : basePassNr + 1);

            if (downsample > 0)
            {
                _tiltShiftMaterial.SetTexture("_Blurred", rt);
                Graphics.Blit(source, destination, _tiltShiftMaterial, 6);
            }

            if (rt != destination)
                RenderTexture.ReleaseTemporary(rt);
        }
    }
}
