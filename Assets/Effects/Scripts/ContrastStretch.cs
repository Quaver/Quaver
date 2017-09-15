// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [AddComponentMenu("Image Effects/Color Adjustments/Contrast Stretch")]
    public class ContrastStretch : MonoBehaviour
    {
        /// Adaptation speed - percents per frame, if playing at 30FPS.
        /// Default is 0.02 (2% each 1/30s).
        public float adaptationSpeed = 0.02f;

        /// If our scene is really dark (or really bright), we might not want to
        /// stretch its contrast to the full range.
        /// limitMinimum=0, limitMaximum=1 is the same as not applying the effect at all.
        /// limitMinimum=1, limitMaximum=0 is always stretching colors to full range.

        /// The limit on the minimum luminance (0...1) - we won't go above this.
        public float limitMinimum = 0.2f;

        /// The limit on the maximum luminance (0...1) - we won't go below this.
        public float limitMaximum = 0.6f;


        // To maintain adaptation levels over time, we need two 1x1 render textures
        // and ping-pong between them.
        private RenderTexture[] _adaptRenderTex = new RenderTexture[2];
        private int _curAdaptIndex = 0;


        // Computes scene luminance (grayscale) image
        public Shader shaderLum;
        private Material _materialLum;
        protected Material materialLum
        {
            get
            {
                if (_materialLum == null)
                {
                    _materialLum = new Material(shaderLum);
                    _materialLum.hideFlags = HideFlags.HideAndDontSave;
                }
                return _materialLum;
            }
        }

        // Reduces size of the image by 2x2, while computing maximum/minimum values.
        // By repeatedly applying this shader, we reduce the initial luminance image
        // to 1x1 image with minimum/maximum luminances found.
        public Shader shaderReduce;
        private Material _materialReduce;
        protected Material materialReduce
        {
            get
            {
                if (_materialReduce == null)
                {
                    _materialReduce = new Material(shaderReduce);
                    _materialReduce.hideFlags = HideFlags.HideAndDontSave;
                }
                return _materialReduce;
            }
        }

        // Adaptation shader - gradually "adapts" minimum/maximum luminances,
        // based on currently adapted 1x1 image and the actual 1x1 image of the current scene.
        public Shader shaderAdapt;
        private Material _materialAdapt;
        protected Material materialAdapt
        {
            get
            {
                if (_materialAdapt == null)
                {
                    _materialAdapt = new Material(shaderAdapt);
                    _materialAdapt.hideFlags = HideFlags.HideAndDontSave;
                }
                return _materialAdapt;
            }
        }

        // Final pass - stretches the color values of the original scene, based on currently
        // adpated minimum/maximum values.
        public Shader shaderApply;
        private Material _materialApply;
        protected Material materialApply
        {
            get
            {
                if (_materialApply == null)
                {
                    _materialApply = new Material(shaderApply);
                    _materialApply.hideFlags = HideFlags.HideAndDontSave;
                }
                return _materialApply;
            }
        }

        private void Start()
        {
            // Disable if we don't support image effects
            if (!SystemInfo.supportsImageEffects)
            {
                enabled = false;
                return;
            }

            if (!shaderAdapt.isSupported || !shaderApply.isSupported || !shaderLum.isSupported || !shaderReduce.isSupported)
            {
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            for (int i = 0; i < 2; ++i)
            {
                if (!_adaptRenderTex[i])
                {
                    _adaptRenderTex[i] = new RenderTexture(1, 1, 0);
                    _adaptRenderTex[i].hideFlags = HideFlags.HideAndDontSave;
                }
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < 2; ++i)
            {
                DestroyImmediate(_adaptRenderTex[i]);
                _adaptRenderTex[i] = null;
            }
            if (_materialLum)
                DestroyImmediate(_materialLum);
            if (_materialReduce)
                DestroyImmediate(_materialReduce);
            if (_materialAdapt)
                DestroyImmediate(_materialAdapt);
            if (_materialApply)
                DestroyImmediate(_materialApply);
        }


        /// Apply the filter
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // Blit to smaller RT and convert to luminance on the way
            const int TEMP_RATIO = 1; // 4x4 smaller
            RenderTexture rtTempSrc = RenderTexture.GetTemporary(source.width / TEMP_RATIO, source.height / TEMP_RATIO);
            Graphics.Blit(source, rtTempSrc, materialLum);

            // Repeatedly reduce this image in size, computing min/max luminance values
            // In the end we'll have 1x1 image with min/max luminances found.
            const int FINAL_SIZE = 1;
            //const int FINAL_SIZE = 1;
            while (rtTempSrc.width > FINAL_SIZE || rtTempSrc.height > FINAL_SIZE)
            {
                const int REDUCE_RATIO = 2; // our shader does 2x2 reduction
                int destW = rtTempSrc.width / REDUCE_RATIO;
                if (destW < FINAL_SIZE) destW = FINAL_SIZE;
                int destH = rtTempSrc.height / REDUCE_RATIO;
                if (destH < FINAL_SIZE) destH = FINAL_SIZE;
                RenderTexture rtTempDst = RenderTexture.GetTemporary(destW, destH);
                Graphics.Blit(rtTempSrc, rtTempDst, materialReduce);

                // Release old src temporary, and make new temporary the source
                RenderTexture.ReleaseTemporary(rtTempSrc);
                rtTempSrc = rtTempDst;
            }

            // Update viewer's adaptation level
            CalculateAdaptation(rtTempSrc);

            // Apply contrast strech to the original scene, using currently adapted parameters
            materialApply.SetTexture("_AdaptTex", _adaptRenderTex[_curAdaptIndex]);
            Graphics.Blit(source, destination, materialApply);

            RenderTexture.ReleaseTemporary(rtTempSrc);
        }


        /// Helper function to do gradual adaptation to min/max luminances
        private void CalculateAdaptation(Texture curTexture)
        {
            int prevAdaptIndex = _curAdaptIndex;
            _curAdaptIndex = (_curAdaptIndex + 1) % 2;

            // Adaptation speed is expressed in percents/frame, based on 30FPS.
            // Calculate the adaptation lerp, based on current FPS.
            float adaptLerp = 1.0f - Mathf.Pow(1.0f - adaptationSpeed, 30.0f * Time.deltaTime);
            const float kMinAdaptLerp = 0.01f;
            adaptLerp = Mathf.Clamp(adaptLerp, kMinAdaptLerp, 1);

            materialAdapt.SetTexture("_CurTex", curTexture);
            materialAdapt.SetVector("_AdaptParams", new Vector4(
                                                        adaptLerp,
                                                        limitMinimum,
                                                        limitMaximum,
                                                        0.0f
                                                        ));
            // clear destination RT so its contents don't need to be restored
            Graphics.SetRenderTarget(_adaptRenderTex[_curAdaptIndex]);
            GL.Clear(false, true, Color.black);
            Graphics.Blit(
                _adaptRenderTex[prevAdaptIndex],
                _adaptRenderTex[_curAdaptIndex],
                materialAdapt);
        }
    }
}
