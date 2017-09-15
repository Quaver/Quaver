
using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Edge Detection/Edge Detection")]
    public class EdgeDetection : PostEffectsBase
    {
        public enum EdgeDetectMode
        {
            TriangleDepthNormals = 0,
            RobertsCrossDepthNormals = 1,
            SobelDepth = 2,
            SobelDepthThin = 3,
            TriangleLuminance = 4,
        }


        public EdgeDetectMode mode = EdgeDetectMode.SobelDepthThin;
        public float sensitivityDepth = 1.0f;
        public float sensitivityNormals = 1.0f;
        public float lumThreshold = 0.2f;
        public float edgeExp = 1.0f;
        public float sampleDist = 1.0f;
        public float edgesOnly = 0.0f;
        public Color edgesOnlyBgColor = Color.white;

        public Shader edgeDetectShader;
        private Material _edgeDetectMaterial = null;
        private EdgeDetectMode _oldMode = EdgeDetectMode.SobelDepthThin;


        public override bool CheckResources()
        {
            CheckSupport(true);

            _edgeDetectMaterial = CheckShaderAndCreateMaterial(edgeDetectShader, _edgeDetectMaterial);
            if (mode != _oldMode)
                SetCameraFlag();

            _oldMode = mode;

            if (!isSupported)
                ReportAutoDisable();
            return isSupported;
        }


        private new void Start()
        {
            _oldMode = mode;
        }

        private void SetCameraFlag()
        {
            if (mode == EdgeDetectMode.SobelDepth || mode == EdgeDetectMode.SobelDepthThin)
                GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
            else if (mode == EdgeDetectMode.TriangleDepthNormals || mode == EdgeDetectMode.RobertsCrossDepthNormals)
                GetComponent<Camera>().depthTextureMode |= DepthTextureMode.DepthNormals;
        }

        private void OnEnable()
        {
            SetCameraFlag();
        }

        [ImageEffectOpaque]
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (CheckResources() == false)
            {
                Graphics.Blit(source, destination);
                return;
            }

            Vector2 sensitivity = new Vector2(sensitivityDepth, sensitivityNormals);
            _edgeDetectMaterial.SetVector("_Sensitivity", new Vector4(sensitivity.x, sensitivity.y, 1.0f, sensitivity.y));
            _edgeDetectMaterial.SetFloat("_BgFade", edgesOnly);
            _edgeDetectMaterial.SetFloat("_SampleDistance", sampleDist);
            _edgeDetectMaterial.SetVector("_BgColor", edgesOnlyBgColor);
            _edgeDetectMaterial.SetFloat("_Exponent", edgeExp);
            _edgeDetectMaterial.SetFloat("_Threshold", lumThreshold);

            Graphics.Blit(source, destination, _edgeDetectMaterial, (int)mode);
        }
    }
}
