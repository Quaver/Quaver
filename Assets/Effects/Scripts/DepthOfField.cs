using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Camera/Depth of Field (Lens Blur, Scatter, DX11)")]
    public class DepthOfField : PostEffectsBase
    {
        public bool visualizeFocus = false;
        public float focalLength = 10.0f;
        public float focalSize = 0.05f;
        public float aperture = 11.5f;
        public float maxBlurSize = 2.0f;
        public Transform focalTransform = null;

        public enum BlurSampleCount
        {
            Low = 0,
            Medium = 1,
            High = 2,
        }

        public BlurSampleCount blurSampleCount = BlurSampleCount.High;

        public bool nearBlur = false;
        public float foregroundOverlap = 1.0f;

        public Shader dofHdrShader;
        private Material _dofHdrMaterial = null;

        private float _focalDistance01 = 10.0f;
        private ComputeBuffer _cbDrawArgs;
        private ComputeBuffer _cbPoints;
        private float _internalBlurWidth = 1.0f;

        public override bool CheckResources()
        {
            CheckSupport(true); // only requires depth, not HDR

            _dofHdrMaterial = CheckShaderAndCreateMaterial(dofHdrShader, _dofHdrMaterial);

            if (!isSupported)
                ReportAutoDisable();

            return isSupported;
        }

        private void OnEnable()
        {
            GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
        }

        private void OnDisable()
        {
            ReleaseComputeResources();

            if (_dofHdrMaterial) DestroyImmediate(_dofHdrMaterial);
            _dofHdrMaterial = null;
        }

        private void ReleaseComputeResources()
        {
            if (_cbDrawArgs != null) _cbDrawArgs.Release();
            _cbDrawArgs = null;
            if (_cbPoints != null) _cbPoints.Release();
            _cbPoints = null;
        }

        private void CreateComputeResources()
        {
            if (_cbDrawArgs == null)
            {
                _cbDrawArgs = new ComputeBuffer(1, 16, ComputeBufferType.IndirectArguments);
                var args = new int[4];
                args[0] = 0;
                args[1] = 1;
                args[2] = 0;
                args[3] = 0;
                _cbDrawArgs.SetData(args);
            }
            if (_cbPoints == null)
            {
                _cbPoints = new ComputeBuffer(90000, 12 + 16, ComputeBufferType.Append);
            }
        }

        private float FocalDistance01(float worldDist)
        {
            return GetComponent<Camera>().WorldToViewportPoint(((worldDist - GetComponent<Camera>().nearClipPlane) * GetComponent<Camera>().transform.forward) + GetComponent<Camera>().transform.position).z / (GetComponent<Camera>().farClipPlane - GetComponent<Camera>().nearClipPlane);
        }

        private void WriteCoc(RenderTexture fromTo, bool fgDilate)
        {
            _dofHdrMaterial.SetTexture("_FgOverlap", null);

            if (nearBlur && fgDilate)
            {
                int rtW = fromTo.width / 2;
                int rtH = fromTo.height / 2;

                // capture fg coc
                RenderTexture temp2 = RenderTexture.GetTemporary(rtW, rtH, 0, fromTo.format);
                Graphics.Blit(fromTo, temp2, _dofHdrMaterial, 4);

                // special blur
                float fgAdjustment = _internalBlurWidth * foregroundOverlap;

                _dofHdrMaterial.SetVector("_Offsets", new Vector4(0.0f, fgAdjustment, 0.0f, fgAdjustment));
                RenderTexture temp1 = RenderTexture.GetTemporary(rtW, rtH, 0, fromTo.format);
                Graphics.Blit(temp2, temp1, _dofHdrMaterial, 2);
                RenderTexture.ReleaseTemporary(temp2);

                _dofHdrMaterial.SetVector("_Offsets", new Vector4(fgAdjustment, 0.0f, 0.0f, fgAdjustment));
                temp2 = RenderTexture.GetTemporary(rtW, rtH, 0, fromTo.format);
                Graphics.Blit(temp1, temp2, _dofHdrMaterial, 2);
                RenderTexture.ReleaseTemporary(temp1);

                // "merge up" with background COC
                _dofHdrMaterial.SetTexture("_FgOverlap", temp2);
                fromTo.MarkRestoreExpected(); // only touching alpha channel, RT restore expected
                Graphics.Blit(fromTo, fromTo, _dofHdrMaterial, 13);
                RenderTexture.ReleaseTemporary(temp2);
            }
            else
            {
                // capture full coc in alpha channel (fromTo is not read, but bound to detect screen flip)
                fromTo.MarkRestoreExpected(); // only touching alpha channel, RT restore expected
                Graphics.Blit(fromTo, fromTo, _dofHdrMaterial, 0);
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!CheckResources())
            {
                Graphics.Blit(source, destination);
                return;
            }

            // clamp & prepare values so they make sense

            if (aperture < 0.0f) aperture = 0.0f;
            if (maxBlurSize < 0.1f) maxBlurSize = 0.1f;
            focalSize = Mathf.Clamp(focalSize, 0.0f, 2.0f);
            _internalBlurWidth = Mathf.Max(maxBlurSize, 0.0f);

            // focal & coc calculations

            _focalDistance01 = (focalTransform) ? (GetComponent<Camera>().WorldToViewportPoint(focalTransform.position)).z / (GetComponent<Camera>().farClipPlane) : FocalDistance01(focalLength);
            _dofHdrMaterial.SetVector("_CurveParams", new Vector4(1.0f, focalSize, aperture / 10.0f, _focalDistance01));

            // possible render texture helpers

            RenderTexture rtLow = null;
            RenderTexture rtLow2 = null;
            float fgBlurDist = _internalBlurWidth * foregroundOverlap;

            if (visualizeFocus)
            {
                WriteCoc(source, true);
                Graphics.Blit(source, destination, _dofHdrMaterial, 16);
            }
            else
            {
                source.filterMode = FilterMode.Bilinear;

                WriteCoc(source, true);

                rtLow = RenderTexture.GetTemporary(source.width >> 1, source.height >> 1, 0, source.format);
                rtLow2 = RenderTexture.GetTemporary(source.width >> 1, source.height >> 1, 0, source.format);

                int blurPass = (blurSampleCount == BlurSampleCount.High || blurSampleCount == BlurSampleCount.Medium) ? 17 : 11;

                _dofHdrMaterial.SetVector("_Offsets", new Vector4(0.0f, _internalBlurWidth, 0.1f, _internalBlurWidth));

                // blur
                Graphics.Blit(source, rtLow, _dofHdrMaterial, 6);
                Graphics.Blit(rtLow, rtLow2, _dofHdrMaterial, blurPass);

                // cheaper blur in high resolution, upsample and combine
                _dofHdrMaterial.SetTexture("_LowRez", rtLow2);
                _dofHdrMaterial.SetTexture("_FgOverlap", null);
                _dofHdrMaterial.SetVector("_Offsets", Vector4.one * ((1.0f * source.width) / (1.0f * rtLow2.width)) * _internalBlurWidth);
                Graphics.Blit(source, destination, _dofHdrMaterial, blurSampleCount == BlurSampleCount.High ? 18 : 12);
            }

            if (rtLow) RenderTexture.ReleaseTemporary(rtLow);
            if (rtLow2) RenderTexture.ReleaseTemporary(rtLow2);
        }
    }
}
