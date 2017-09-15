
using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    public enum LensflareStyle34
    {
        Ghosting = 0,
        Anamorphic = 1,
        Combined = 2,
    }

    public enum TweakMode34
    {
        Basic = 0,
        Complex = 1,
    }

    public enum HDRBloomMode
    {
        Auto = 0,
        On = 1,
        Off = 2,
    }

    public enum BloomScreenBlendMode
    {
        Screen = 0,
        Add = 1,
    }

    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Bloom and Glow/BloomAndFlares (3.5, Deprecated)")]
    public class BloomAndFlares : PostEffectsBase
    {
        public TweakMode34 tweakMode = 0;
        public BloomScreenBlendMode screenBlendMode = BloomScreenBlendMode.Add;

        public HDRBloomMode hdr = HDRBloomMode.Auto;
        private bool _doHdr = false;
        public float sepBlurSpread = 1.5f;
        public float useSrcAlphaAsMask = 0.5f;

        public float bloomIntensity = 1.0f;
        public float bloomThreshold = 0.5f;
        public int bloomBlurIterations = 2;

        public bool lensflares = false;
        public int hollywoodFlareBlurIterations = 2;
        public LensflareStyle34 lensflareMode = (LensflareStyle34)1;
        public float hollyStretchWidth = 3.5f;
        public float lensflareIntensity = 1.0f;
        public float lensflareThreshold = 0.3f;
        public Color flareColorA = new Color(0.4f, 0.4f, 0.8f, 0.75f);
        public Color flareColorB = new Color(0.4f, 0.8f, 0.8f, 0.75f);
        public Color flareColorC = new Color(0.8f, 0.4f, 0.8f, 0.75f);
        public Color flareColorD = new Color(0.8f, 0.4f, 0.0f, 0.75f);
        public Texture2D lensFlareVignetteMask;

        public Shader lensFlareShader;
        private Material _lensFlareMaterial;

        public Shader vignetteShader;
        private Material _vignetteMaterial;

        public Shader separableBlurShader;
        private Material _separableBlurMaterial;

        public Shader addBrightStuffOneOneShader;
        private Material _addBrightStuffBlendOneOneMaterial;

        public Shader screenBlendShader;
        private Material _screenBlend;

        public Shader hollywoodFlaresShader;
        private Material _hollywoodFlaresMaterial;

        public Shader brightPassFilterShader;
        private Material _brightPassFilterMaterial;


        public override bool CheckResources()
        {
            CheckSupport(false);

            _screenBlend = CheckShaderAndCreateMaterial(screenBlendShader, _screenBlend);
            _lensFlareMaterial = CheckShaderAndCreateMaterial(lensFlareShader, _lensFlareMaterial);
            _vignetteMaterial = CheckShaderAndCreateMaterial(vignetteShader, _vignetteMaterial);
            _separableBlurMaterial = CheckShaderAndCreateMaterial(separableBlurShader, _separableBlurMaterial);
            _addBrightStuffBlendOneOneMaterial = CheckShaderAndCreateMaterial(addBrightStuffOneOneShader, _addBrightStuffBlendOneOneMaterial);
            _hollywoodFlaresMaterial = CheckShaderAndCreateMaterial(hollywoodFlaresShader, _hollywoodFlaresMaterial);
            _brightPassFilterMaterial = CheckShaderAndCreateMaterial(brightPassFilterShader, _brightPassFilterMaterial);

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

            // screen blend is not supported when HDR is enabled (will cap values)

            _doHdr = false;
            if (hdr == HDRBloomMode.Auto)
                _doHdr = source.format == RenderTextureFormat.ARGBHalf && GetComponent<Camera>().hdr;
            else
            {
                _doHdr = hdr == HDRBloomMode.On;
            }

            _doHdr = _doHdr && supportHDRTextures;

            BloomScreenBlendMode realBlendMode = screenBlendMode;
            if (_doHdr)
                realBlendMode = BloomScreenBlendMode.Add;

            var rtFormat = (_doHdr) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.Default;
            RenderTexture halfRezColor = RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0, rtFormat);
            RenderTexture quarterRezColor = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0, rtFormat);
            RenderTexture secondQuarterRezColor = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0, rtFormat);
            RenderTexture thirdQuarterRezColor = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0, rtFormat);

            float widthOverHeight = (1.0f * source.width) / (1.0f * source.height);
            float oneOverBaseSize = 1.0f / 512.0f;

            // downsample

            Graphics.Blit(source, halfRezColor, _screenBlend, 2); // <- 2 is stable downsample
            Graphics.Blit(halfRezColor, quarterRezColor, _screenBlend, 2); // <- 2 is stable downsample

            RenderTexture.ReleaseTemporary(halfRezColor);

            // cut colors (thresholding)

            BrightFilter(bloomThreshold, useSrcAlphaAsMask, quarterRezColor, secondQuarterRezColor);
            quarterRezColor.DiscardContents();

            // blurring

            if (bloomBlurIterations < 1) bloomBlurIterations = 1;

            for (int iter = 0; iter < bloomBlurIterations; iter++)
            {
                float spreadForPass = (1.0f + (iter * 0.5f)) * sepBlurSpread;
                _separableBlurMaterial.SetVector("offsets", new Vector4(0.0f, spreadForPass * oneOverBaseSize, 0.0f, 0.0f));

                RenderTexture src = iter == 0 ? secondQuarterRezColor : quarterRezColor;
                Graphics.Blit(src, thirdQuarterRezColor, _separableBlurMaterial);
                src.DiscardContents();

                _separableBlurMaterial.SetVector("offsets", new Vector4((spreadForPass / widthOverHeight) * oneOverBaseSize, 0.0f, 0.0f, 0.0f));
                Graphics.Blit(thirdQuarterRezColor, quarterRezColor, _separableBlurMaterial);
                thirdQuarterRezColor.DiscardContents();
            }

            // lens flares: ghosting, anamorphic or a combination

            if (lensflares)
            {
                if (lensflareMode == 0)
                {
                    BrightFilter(lensflareThreshold, 0.0f, quarterRezColor, thirdQuarterRezColor);
                    quarterRezColor.DiscardContents();

                    // smooth a little, this needs to be resolution dependent
                    /*
                    separableBlurMaterial.SetVector ("offsets", Vector4 (0.0ff, (2.0ff) / (1.0ff * quarterRezColor.height), 0.0ff, 0.0ff));
                    Graphics.Blit (thirdQuarterRezColor, secondQuarterRezColor, separableBlurMaterial);
                    separableBlurMaterial.SetVector ("offsets", Vector4 ((2.0ff) / (1.0ff * quarterRezColor.width), 0.0ff, 0.0ff, 0.0ff));
                    Graphics.Blit (secondQuarterRezColor, thirdQuarterRezColor, separableBlurMaterial);
                    */
                    // no ugly edges!

                    Vignette(0.975f, thirdQuarterRezColor, secondQuarterRezColor);
                    thirdQuarterRezColor.DiscardContents();

                    BlendFlares(secondQuarterRezColor, quarterRezColor);
                    secondQuarterRezColor.DiscardContents();
                }

                // (b) hollywood/anamorphic flares?

                else
                {
                    // thirdQuarter has the brightcut unblurred colors
                    // quarterRezColor is the blurred, brightcut buffer that will end up as bloom

                    _hollywoodFlaresMaterial.SetVector("_threshold", new Vector4(lensflareThreshold, 1.0f / (1.0f - lensflareThreshold), 0.0f, 0.0f));
                    _hollywoodFlaresMaterial.SetVector("tintColor", new Vector4(flareColorA.r, flareColorA.g, flareColorA.b, flareColorA.a) * flareColorA.a * lensflareIntensity);
                    Graphics.Blit(thirdQuarterRezColor, secondQuarterRezColor, _hollywoodFlaresMaterial, 2);
                    thirdQuarterRezColor.DiscardContents();

                    Graphics.Blit(secondQuarterRezColor, thirdQuarterRezColor, _hollywoodFlaresMaterial, 3);
                    secondQuarterRezColor.DiscardContents();

                    _hollywoodFlaresMaterial.SetVector("offsets", new Vector4((sepBlurSpread * 1.0f / widthOverHeight) * oneOverBaseSize, 0.0f, 0.0f, 0.0f));
                    _hollywoodFlaresMaterial.SetFloat("stretchWidth", hollyStretchWidth);
                    Graphics.Blit(thirdQuarterRezColor, secondQuarterRezColor, _hollywoodFlaresMaterial, 1);
                    thirdQuarterRezColor.DiscardContents();

                    _hollywoodFlaresMaterial.SetFloat("stretchWidth", hollyStretchWidth * 2.0f);
                    Graphics.Blit(secondQuarterRezColor, thirdQuarterRezColor, _hollywoodFlaresMaterial, 1);
                    secondQuarterRezColor.DiscardContents();

                    _hollywoodFlaresMaterial.SetFloat("stretchWidth", hollyStretchWidth * 4.0f);
                    Graphics.Blit(thirdQuarterRezColor, secondQuarterRezColor, _hollywoodFlaresMaterial, 1);
                    thirdQuarterRezColor.DiscardContents();

                    if (lensflareMode == (LensflareStyle34)1)
                    {
                        for (int itera = 0; itera < hollywoodFlareBlurIterations; itera++)
                        {
                            _separableBlurMaterial.SetVector("offsets", new Vector4((hollyStretchWidth * 2.0f / widthOverHeight) * oneOverBaseSize, 0.0f, 0.0f, 0.0f));
                            Graphics.Blit(secondQuarterRezColor, thirdQuarterRezColor, _separableBlurMaterial);
                            secondQuarterRezColor.DiscardContents();

                            _separableBlurMaterial.SetVector("offsets", new Vector4((hollyStretchWidth * 2.0f / widthOverHeight) * oneOverBaseSize, 0.0f, 0.0f, 0.0f));
                            Graphics.Blit(thirdQuarterRezColor, secondQuarterRezColor, _separableBlurMaterial);
                            thirdQuarterRezColor.DiscardContents();
                        }

                        AddTo(1.0f, secondQuarterRezColor, quarterRezColor);
                        secondQuarterRezColor.DiscardContents();
                    }
                    else
                    {
                        // (c) combined

                        for (int ix = 0; ix < hollywoodFlareBlurIterations; ix++)
                        {
                            _separableBlurMaterial.SetVector("offsets", new Vector4((hollyStretchWidth * 2.0f / widthOverHeight) * oneOverBaseSize, 0.0f, 0.0f, 0.0f));
                            Graphics.Blit(secondQuarterRezColor, thirdQuarterRezColor, _separableBlurMaterial);
                            secondQuarterRezColor.DiscardContents();

                            _separableBlurMaterial.SetVector("offsets", new Vector4((hollyStretchWidth * 2.0f / widthOverHeight) * oneOverBaseSize, 0.0f, 0.0f, 0.0f));
                            Graphics.Blit(thirdQuarterRezColor, secondQuarterRezColor, _separableBlurMaterial);
                            thirdQuarterRezColor.DiscardContents();
                        }

                        Vignette(1.0f, secondQuarterRezColor, thirdQuarterRezColor);
                        secondQuarterRezColor.DiscardContents();

                        BlendFlares(thirdQuarterRezColor, secondQuarterRezColor);
                        thirdQuarterRezColor.DiscardContents();

                        AddTo(1.0f, secondQuarterRezColor, quarterRezColor);
                        secondQuarterRezColor.DiscardContents();
                    }
                }
            }

            // screen blend bloom results to color buffer

            _screenBlend.SetFloat("_Intensity", bloomIntensity);
            _screenBlend.SetTexture("_ColorBuffer", source);
            Graphics.Blit(quarterRezColor, destination, _screenBlend, (int)realBlendMode);

            RenderTexture.ReleaseTemporary(quarterRezColor);
            RenderTexture.ReleaseTemporary(secondQuarterRezColor);
            RenderTexture.ReleaseTemporary(thirdQuarterRezColor);
        }

        private void AddTo(float intensity_, RenderTexture from, RenderTexture to)
        {
            _addBrightStuffBlendOneOneMaterial.SetFloat("_Intensity", intensity_);
            Graphics.Blit(from, to, _addBrightStuffBlendOneOneMaterial);
        }

        private void BlendFlares(RenderTexture from, RenderTexture to)
        {
            _lensFlareMaterial.SetVector("colorA", new Vector4(flareColorA.r, flareColorA.g, flareColorA.b, flareColorA.a) * lensflareIntensity);
            _lensFlareMaterial.SetVector("colorB", new Vector4(flareColorB.r, flareColorB.g, flareColorB.b, flareColorB.a) * lensflareIntensity);
            _lensFlareMaterial.SetVector("colorC", new Vector4(flareColorC.r, flareColorC.g, flareColorC.b, flareColorC.a) * lensflareIntensity);
            _lensFlareMaterial.SetVector("colorD", new Vector4(flareColorD.r, flareColorD.g, flareColorD.b, flareColorD.a) * lensflareIntensity);
            Graphics.Blit(from, to, _lensFlareMaterial);
        }

        private void BrightFilter(float thresh, float useAlphaAsMask, RenderTexture from, RenderTexture to)
        {
            if (_doHdr)
                _brightPassFilterMaterial.SetVector("threshold", new Vector4(thresh, 1.0f, 0.0f, 0.0f));
            else
                _brightPassFilterMaterial.SetVector("threshold", new Vector4(thresh, 1.0f / (1.0f - thresh), 0.0f, 0.0f));
            _brightPassFilterMaterial.SetFloat("useSrcAlphaAsMask", useAlphaAsMask);
            Graphics.Blit(from, to, _brightPassFilterMaterial);
        }

        private void Vignette(float amount, RenderTexture from, RenderTexture to)
        {
            if (lensFlareVignetteMask)
            {
                _screenBlend.SetTexture("_ColorBuffer", lensFlareVignetteMask);
                Graphics.Blit(from, to, _screenBlend, 3);
            }
            else
            {
                _vignetteMaterial.SetFloat("vignetteIntensity", amount);
                Graphics.Blit(from, to, _vignetteMaterial);
            }
        }
    }
}
