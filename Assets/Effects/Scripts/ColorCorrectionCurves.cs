
using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [AddComponentMenu("Image Effects/Color Adjustments/Color Correction (Curves, Saturation)")]
    public class ColorCorrectionCurves : PostEffectsBase
    {
        public enum ColorCorrectionMode
        {
            Simple = 0,
            Advanced = 1
        }

        public AnimationCurve redChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        public AnimationCurve greenChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        public AnimationCurve blueChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        public bool useDepthCorrection = false;

        public AnimationCurve zCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        public AnimationCurve depthRedChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        public AnimationCurve depthGreenChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        public AnimationCurve depthBlueChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        private Material _ccMaterial;
        private Material _ccDepthMaterial;
        private Material _selectiveCcMaterial;

        private Texture2D _rgbChannelTex;
        private Texture2D _rgbDepthChannelTex;
        private Texture2D _zCurveTex;

        public float saturation = 1.0f;

        public bool selectiveCc = false;

        public Color selectiveFromColor = Color.white;
        public Color selectiveToColor = Color.white;

        public ColorCorrectionMode mode;

        public bool updateTextures = true;

        public Shader colorCorrectionCurvesShader = null;
        public Shader simpleColorCorrectionCurvesShader = null;
        public Shader colorCorrectionSelectiveShader = null;

        private bool _updateTexturesOnStartup = true;


        private new void Start()
        {
            base.Start();
            _updateTexturesOnStartup = true;
        }

        private void Awake() { }


        public override bool CheckResources()
        {
            CheckSupport(mode == ColorCorrectionMode.Advanced);

            _ccMaterial = CheckShaderAndCreateMaterial(simpleColorCorrectionCurvesShader, _ccMaterial);
            _ccDepthMaterial = CheckShaderAndCreateMaterial(colorCorrectionCurvesShader, _ccDepthMaterial);
            _selectiveCcMaterial = CheckShaderAndCreateMaterial(colorCorrectionSelectiveShader, _selectiveCcMaterial);

            if (!_rgbChannelTex)
                _rgbChannelTex = new Texture2D(256, 4, TextureFormat.ARGB32, false, true);
            if (!_rgbDepthChannelTex)
                _rgbDepthChannelTex = new Texture2D(256, 4, TextureFormat.ARGB32, false, true);
            if (!_zCurveTex)
                _zCurveTex = new Texture2D(256, 1, TextureFormat.ARGB32, false, true);

            _rgbChannelTex.hideFlags = HideFlags.DontSave;
            _rgbDepthChannelTex.hideFlags = HideFlags.DontSave;
            _zCurveTex.hideFlags = HideFlags.DontSave;

            _rgbChannelTex.wrapMode = TextureWrapMode.Clamp;
            _rgbDepthChannelTex.wrapMode = TextureWrapMode.Clamp;
            _zCurveTex.wrapMode = TextureWrapMode.Clamp;

            if (!isSupported)
                ReportAutoDisable();
            return isSupported;
        }

        public void UpdateParameters()
        {
            CheckResources(); // textures might not be created if we're tweaking UI while disabled

            if (redChannel != null && greenChannel != null && blueChannel != null)
            {
                for (float i = 0.0f; i <= 1.0f; i += 1.0f / 255.0f)
                {
                    float rCh = Mathf.Clamp(redChannel.Evaluate(i), 0.0f, 1.0f);
                    float gCh = Mathf.Clamp(greenChannel.Evaluate(i), 0.0f, 1.0f);
                    float bCh = Mathf.Clamp(blueChannel.Evaluate(i), 0.0f, 1.0f);

                    _rgbChannelTex.SetPixel((int)Mathf.Floor(i * 255.0f), 0, new Color(rCh, rCh, rCh));
                    _rgbChannelTex.SetPixel((int)Mathf.Floor(i * 255.0f), 1, new Color(gCh, gCh, gCh));
                    _rgbChannelTex.SetPixel((int)Mathf.Floor(i * 255.0f), 2, new Color(bCh, bCh, bCh));

                    float zC = Mathf.Clamp(zCurve.Evaluate(i), 0.0f, 1.0f);

                    _zCurveTex.SetPixel((int)Mathf.Floor(i * 255.0f), 0, new Color(zC, zC, zC));

                    rCh = Mathf.Clamp(depthRedChannel.Evaluate(i), 0.0f, 1.0f);
                    gCh = Mathf.Clamp(depthGreenChannel.Evaluate(i), 0.0f, 1.0f);
                    bCh = Mathf.Clamp(depthBlueChannel.Evaluate(i), 0.0f, 1.0f);

                    _rgbDepthChannelTex.SetPixel((int)Mathf.Floor(i * 255.0f), 0, new Color(rCh, rCh, rCh));
                    _rgbDepthChannelTex.SetPixel((int)Mathf.Floor(i * 255.0f), 1, new Color(gCh, gCh, gCh));
                    _rgbDepthChannelTex.SetPixel((int)Mathf.Floor(i * 255.0f), 2, new Color(bCh, bCh, bCh));
                }

                _rgbChannelTex.Apply();
                _rgbDepthChannelTex.Apply();
                _zCurveTex.Apply();
            }
        }

        private void UpdateTextures()
        {
            UpdateParameters();
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (CheckResources() == false)
            {
                Graphics.Blit(source, destination);
                return;
            }

            if (_updateTexturesOnStartup)
            {
                UpdateParameters();
                _updateTexturesOnStartup = false;
            }

            if (useDepthCorrection)
                GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;

            RenderTexture renderTarget2Use = destination;

            if (selectiveCc)
            {
                renderTarget2Use = RenderTexture.GetTemporary(source.width, source.height);
            }

            if (useDepthCorrection)
            {
                _ccDepthMaterial.SetTexture("_RgbTex", _rgbChannelTex);
                _ccDepthMaterial.SetTexture("_ZCurve", _zCurveTex);
                _ccDepthMaterial.SetTexture("_RgbDepthTex", _rgbDepthChannelTex);
                _ccDepthMaterial.SetFloat("_Saturation", saturation);

                Graphics.Blit(source, renderTarget2Use, _ccDepthMaterial);
            }
            else
            {
                _ccMaterial.SetTexture("_RgbTex", _rgbChannelTex);
                _ccMaterial.SetFloat("_Saturation", saturation);

                Graphics.Blit(source, renderTarget2Use, _ccMaterial);
            }

            if (selectiveCc)
            {
                _selectiveCcMaterial.SetColor("selColor", selectiveFromColor);
                _selectiveCcMaterial.SetColor("targetColor", selectiveToColor);
                Graphics.Blit(renderTarget2Use, destination, _selectiveCcMaterial);

                RenderTexture.ReleaseTemporary(renderTarget2Use);
            }
        }
    }
}
